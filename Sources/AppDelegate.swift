import AppKit
import SwiftUI

final class AppDelegate: NSObject, NSApplicationDelegate {
    private let settings = Settings(Config.load())
    private let deck = MessageDeck()
    private lazy var model = PopupModel(settings: settings)

    private var window: NSWindow!
    private var statusItem: NSStatusItem!
    private var pauseItem: NSMenuItem!
    private var faceItems: [NSMenuItem] = []
    private var timer: Timer?
    private var paused = false

    /// Window size at scale 1; everything inside scales with it.
    private var windowSize: NSSize {
        NSSize(width: PopupView.baseSize.width * settings.cfg.scale,
               height: PopupView.baseSize.height * settings.cfg.scale)
    }

    func applicationDidFinishLaunching(_ notification: Notification) {
        deck.reload(settings.cfg)

        let host = NSHostingView(rootView: PopupView(model: model, settings: settings))
        host.frame = NSRect(origin: .zero, size: windowSize)
        window = NSWindow(contentRect: NSRect(origin: .zero, size: windowSize),
                          styleMask: .borderless, backing: .buffered, defer: false)
        window.contentView = host
        window.setContentSize(windowSize)
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
        startTimer()
        DispatchQueue.main.asyncAfter(deadline: .now() + 1.2) { [weak self] in
            guard let self, !self.paused else { return }
            self.next()
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

    private func reposition() {
        let screens = NSScreen.screens
        guard !screens.isEmpty else { return }
        // Pinned by default: NSScreen.main follows keyboard focus, so the popup
        // would otherwise land on a different display run to run.
        let idx = settings.cfg.screenIndex
        let screen = screens.indices.contains(idx) ? screens[idx] : (NSScreen.main ?? screens[0])
        let v = screen.visibleFrame
        let f = window.frame.size

        let x = settings.cfg.position.isLeft ? v.minX : v.maxX - f.width
        let y = settings.cfg.position.isTop ? v.maxY - f.height : v.minY
        window.setFrameOrigin(CGPoint(x: x, y: y))
    }

    // MARK: - Speaking

    private func startTimer() {
        timer?.invalidate()
        let t = Timer(timeInterval: settings.cfg.intervalSeconds, repeats: true) { [weak self] _ in
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
        window.setContentSize(windowSize)
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

    @objc private func quitApp() { NSApp.terminate(nil) }
}
