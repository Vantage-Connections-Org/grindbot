import AppKit
import Combine
import ServiceManagement
import SwiftUI

final class AppDelegate: NSObject, NSApplicationDelegate {
    private let settings = Settings(Config.load())
    private let deck = MessageDeck()
    private lazy var model = PopupModel(settings: settings)

    private var window: NSWindow!
    private var statusItem: NSStatusItem!
    private var loginItem: NSMenuItem?
    private var pauseItem: NSMenuItem!
    private var faceItems: [NSMenuItem] = []
    private var timer: Timer?
    private var paused = false
    private var settingsWindow: NSWindow?
    private var cancellables = Set<AnyCancellable>()
    private var applied: Config?

    /// Window size at scale 1; everything inside scales with it.
    private func windowSize(_ cfg: Config) -> NSSize {
        NSSize(width: PopupView.baseSize.width * cfg.scale,
               height: PopupView.baseSize.height * cfg.scale)
    }

    func applicationDidFinishLaunching(_ notification: Notification) {
        deck.reload(settings.cfg)

        let host = NSHostingView(rootView: PopupView(model: model, settings: settings))
        host.frame = NSRect(origin: .zero, size: windowSize(settings.cfg))
        window = NSWindow(contentRect: NSRect(origin: .zero, size: windowSize(settings.cfg)),
                          styleMask: .borderless, backing: .buffered, defer: false)
        window.contentView = host
        window.setContentSize(windowSize(settings.cfg))
        window.isOpaque = false
        window.backgroundColor = .clear
        window.hasShadow = false
        window.level = .statusBar
        window.ignoresMouseEvents = true          // never steals clicks or focus
        window.collectionBehavior = [.canJoinAllSpaces, .stationary, .fullScreenAuxiliary]
        reposition()
        window.orderFrontRegardless()

        NotificationCenter.default.addObserver(
            forName: NSApplication.didChangeScreenParametersNotification,
            object: nil, queue: .main) { [weak self] _ in self?.reposition() }

        buildMenu()
        applied = settings.cfg
        observeSettings()
        startTimer()
        DispatchQueue.main.asyncAfter(deadline: .now() + 1.2) { [weak self] in
            guard let self, !self.paused else { return }
            self.next()
        }
    }

    // MARK: - Live settings

    private func observeSettings() {
        // @Published fires before the property is written, so act on the new
        // value handed to the sink rather than reading settings.cfg back.
        settings.$cfg
            .receive(on: DispatchQueue.main)
            .sink { [weak self] cfg in self?.apply(cfg) }
            .store(in: &cancellables)

        settings.$cfg
            .debounce(for: .milliseconds(400), scheduler: DispatchQueue.main)
            .sink { [weak self] cfg in self?.settings.saveError = cfg.save() }
            .store(in: &cancellables)
    }

    private func apply(_ cfg: Config) {
        defer { applied = cfg }
        let old = applied

        if old?.scale != cfg.scale {
            window.setContentSize(windowSize(cfg))
        }
        if old?.scale != cfg.scale || old?.position != cfg.position || old?.screenIndex != cfg.screenIndex {
            reposition(cfg)
        }
        if old?.messagesFile != cfg.messagesFile || old?.shuffle != cfg.shuffle {
            deck.reload(cfg)
        }
        // Only on a real change — otherwise every slider tick restarts the countdown.
        if old?.intervalSeconds != cfg.intervalSeconds, !paused {
            startTimer(cfg)
        }
        for item in faceItems {
            item.state = (item.representedObject as? String) == cfg.face.rawValue ? .on : .off
        }
    }

    // MARK: - Menu

    private func buildMenu() {
        statusItem = NSStatusBar.system.statusItem(withLength: NSStatusItem.variableLength)
        statusItem.button?.title = "🤖"

        let menu = NSMenu()
        let pause = NSMenuItem(title: "Turn off", action: #selector(togglePause), keyEquivalent: "t")
        pauseItem = pause

        // Explicit targets throughout: a nil target silently disables the item.
        for item in [
            NSMenuItem(title: "Settings…", action: #selector(showSettings), keyEquivalent: ","),
            NSMenuItem(title: "Say something now", action: #selector(sayNow), keyEquivalent: "s"),
            pause,
        ] {
            item.target = self
            menu.addItem(item)
        }

        let faceMenu = NSMenu()
        for face in Face.allCases {
            let item = NSMenuItem(title: face.rawValue.capitalized,
                                  action: #selector(pickFace(_:)), keyEquivalent: "")
            item.target = self
            item.representedObject = face.rawValue
            item.state = face == settings.cfg.face ? .on : .off
            faceMenu.addItem(item)
            faceItems.append(item)
        }
        let faceRoot = NSMenuItem(title: "Face", action: nil, keyEquivalent: "")
        faceRoot.submenu = faceMenu
        menu.addItem(faceRoot)

        menu.addItem(.separator())
        let login = NSMenuItem(title: "Start at login", action: #selector(toggleLoginItem), keyEquivalent: "")
        login.target = self
        login.state = LoginItem.enabled ? .on : .off
        loginItem = login
        menu.addItem(login)

        menu.addItem(.separator())
        for item in [
            NSMenuItem(title: "Reload config & messages", action: #selector(reload), keyEquivalent: "r"),
            NSMenuItem(title: "Edit messages…", action: #selector(editMessages), keyEquivalent: ""),
            NSMenuItem(title: "Edit config.json…", action: #selector(editConfig), keyEquivalent: ""),
        ] {
            item.target = self
            menu.addItem(item)
        }

        menu.addItem(.separator())
        let quit = NSMenuItem(title: "Quit", action: #selector(quitApp), keyEquivalent: "q")
        quit.target = self
        menu.addItem(quit)
        statusItem.menu = menu
    }

    // MARK: - Placement

    private func reposition(_ cfg: Config? = nil) {
        let cfg = cfg ?? settings.cfg
        let screens = NSScreen.screens
        guard !screens.isEmpty else { return }
        // Pinned by default: NSScreen.main follows keyboard focus, so the popup
        // would otherwise land on a different display run to run.
        let idx = cfg.screenIndex
        let screen = screens.indices.contains(idx) ? screens[idx] : (NSScreen.main ?? screens[0])
        let v = screen.visibleFrame
        let f = window.frame.size

        let x = cfg.position.isLeft ? v.minX : v.maxX - f.width
        let y = cfg.position.isTop ? v.maxY - f.height : v.minY
        window.setFrameOrigin(CGPoint(x: x, y: y))
    }

    // MARK: - Speaking

    private func startTimer(_ cfg: Config? = nil) {
        let cfg = cfg ?? settings.cfg
        timer?.invalidate()
        let t = Timer(timeInterval: cfg.intervalSeconds, repeats: true) { [weak self] _ in
            self?.next()
        }
        RunLoop.main.add(t, forMode: .common)
        timer = t
    }

    private func next() {
        guard let message = deck.next() else {
            model.say("\(settings.cfg.messagesFile) is empty — add one line per message.")
            return
        }
        model.say(message)
    }

    // MARK: - Actions

    @objc private func sayNow() { next() }

    /// Off = no timer, nothing on screen. On = fire one now, then resume.
    @objc private func togglePause() {
        paused.toggle()
        pauseItem.title = paused ? "Turn on" : "Turn off"
        statusItem.button?.title = paused ? "💤" : "🤖"
        if paused {
            timer?.invalidate()
            timer = nil
            model.hide()
        } else {
            startTimer()
            next()
        }
    }

    /// Session-only switch; set `face` in config.json to make it stick.
    @objc private func pickFace(_ sender: NSMenuItem) {
        guard let raw = sender.representedObject as? String, let face = Face(rawValue: raw) else { return }
        settings.cfg.face = face
        for item in faceItems { item.state = (item.representedObject as? String) == raw ? .on : .off }
    }

    @objc private func reload() {
        settings.cfg = Config.load()
        deck.reload(settings.cfg)
        for item in faceItems {
            item.state = (item.representedObject as? String) == settings.cfg.face.rawValue ? .on : .off
        }
        window.setContentSize(windowSize(settings.cfg))
        reposition()
        startTimer()
        next()
    }

    @objc private func editMessages() { open(settings.cfg.messagesFile) }
    @objc private func editConfig() { open("config.json") }

    private func open(_ name: String) {
        guard let url = fileNextToApp(name) else { return }
        NSWorkspace.shared.open(url)
    }

    @objc private func showSettings() {
        if settingsWindow == nil {
            let view = SettingsView(settings: settings, onPreview: { [weak self] in self?.next() })
            let w = NSWindow(contentRect: NSRect(x: 0, y: 0, width: 520, height: 720),
                             styleMask: [.titled, .closable, .miniaturizable, .resizable],
                             backing: .buffered, defer: false)
            w.title = "GrindBot Settings"
            w.contentView = NSHostingView(rootView: view)
            w.isReleasedWhenClosed = false   // accessory apps reuse the window
            w.center()
            settingsWindow = w
        }
        NSApp.activate(ignoringOtherApps: true)   // .accessory apps aren't frontmost
        settingsWindow?.makeKeyAndOrderFront(nil)
    }

    @objc private func toggleLoginItem() {
        let error = LoginItem.set(!LoginItem.enabled)
        loginItem?.state = LoginItem.enabled ? .on : .off
        guard let error else { return }
        let alert = NSAlert()
        alert.messageText = "Couldn't change the login item"
        alert.informativeText = error
        alert.runModal()
    }

    @objc private func quitApp() { NSApp.terminate(nil) }
}

/// Run at login. The Windows build has had this from the start via an HKCU Run
/// value; SMAppService is the macOS 13+ equivalent, and it only works for an app
/// registered from a real bundle — running the binary directly will report an
/// error rather than silently doing nothing.
enum LoginItem {
    static var enabled: Bool { SMAppService.mainApp.status == .enabled }

    /// Returns nil on success, or a message to show the user.
    static func set(_ on: Bool) -> String? {
        do {
            if on {
                try SMAppService.mainApp.register()
            } else {
                try SMAppService.mainApp.unregister()
            }
            return nil
        } catch {
            return error.localizedDescription
        }
    }
}
