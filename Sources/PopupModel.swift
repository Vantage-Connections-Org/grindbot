import SwiftUI

/// Drives one message: type it out, hold it, take it away.
final class PopupModel: ObservableObject {
    @Published var shownText = ""
    @Published var visible = false
    @Published var blink = false

    private let settings: Settings
    private var fullText = ""
    private var typeTimer: Timer?
    private var hideWork: DispatchWorkItem?

    init(settings: Settings) {
        self.settings = settings
        let t = Timer(timeInterval: 2.4, repeats: true) { [weak self] _ in
            guard let self, self.visible else { return }
            self.blink = true
            DispatchQueue.main.asyncAfter(deadline: .now() + 0.13) { self.blink = false }
        }
        RunLoop.main.add(t, forMode: .common)
    }

    func say(_ text: String) {
        let cfg = settings.cfg
        hideWork?.cancel()
        typeTimer?.invalidate()
        fullText = text
        shownText = cfg.typeSpeed > 0 ? "" : text
        withAnimation(.spring(response: 0.42, dampingFraction: 0.7)) { visible = true }

        if cfg.typeSpeed > 0 {
            var idx = fullText.startIndex
            let typer = Timer(timeInterval: cfg.typeSpeed, repeats: true) { [weak self] t in
                guard let self, idx < self.fullText.endIndex else { t.invalidate(); return }
                self.shownText.append(self.fullText[idx])
                idx = self.fullText.index(after: idx)
            }
            RunLoop.main.add(typer, forMode: .common)
            typeTimer = typer
        }

        let work = DispatchWorkItem { [weak self] in
            withAnimation(.easeInOut(duration: 0.35)) { self?.visible = false }
        }
        hideWork = work
        DispatchQueue.main.asyncAfter(deadline: .now() + cfg.dwell(for: text), execute: work)
    }

    /// Pull the bubble off screen now, cancelling any pending hide.
    func hide() {
        hideWork?.cancel()
        typeTimer?.invalidate()
        withAnimation(.easeInOut(duration: 0.25)) { visible = false }
    }
}

/// Reads the message file and hands out messages in order, or shuffled.
final class MessageDeck {
    private(set) var messages: [String] = []
    private var queue: [String] = []
    private var shuffle = false

    func reload(_ cfg: Config) {
        shuffle = cfg.shuffle
        messages = MessageDeck.read(cfg.messagesFile)
        queue = []
    }

    func next() -> String? {
        guard !messages.isEmpty else { return nil }
        if queue.isEmpty { queue = shuffle ? messages.shuffled() : messages }
        return queue.removeFirst()
    }

    /// One message per line; blank lines and `#` comments ignored.
    private static func read(_ name: String) -> [String] {
        guard let url = fileNextToApp(name),
              let raw = try? String(contentsOf: url, encoding: .utf8) else { return [] }
        // Split on any newline and trim CR too: the shipped packs are CRLF, and
        // trimming only .whitespaces leaves a stray \r on every single message.
        return raw.split(whereSeparator: \.isNewline)
            .map { $0.trimmingCharacters(in: .whitespacesAndNewlines) }
            .filter { !$0.isEmpty && !$0.hasPrefix("#") }
    }
}
