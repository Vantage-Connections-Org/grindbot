import AppKit
import SwiftUI

/// The whole config surface as a window. Edits apply live and save themselves
/// to config.json — there is no OK/Cancel, what you see is what's running.
struct SettingsView: View {
    @ObservedObject var settings: Settings
    var onPreview: () -> Void

    private var cfg: Config { settings.cfg }

    var body: some View {
        ScrollView {
            VStack(alignment: .leading, spacing: 14) {
                preview
                timing
                sizeAndPosition
                colors
                faces
                messages
                footer
            }
            .padding(18)
        }
        .frame(minWidth: 460)
    }

    // MARK: - Live preview

    private var preview: some View {
        HStack(spacing: 14) {
            RobotView(style: RobotStyle(accent: cfg.accentColor, shell: Color(hex: cfg.shell),
                                        visor: Color(hex: cfg.visor), scale: 1.1, face: cfg.face),
                      blink: false)
                .frame(width: 70)
            VStack(alignment: .leading, spacing: 4) {
                Text("Live preview").font(.headline)
                Text("Changes apply immediately and save to config.json.")
                    .font(.caption).foregroundStyle(.secondary)
                Button("Show a message now", action: onPreview)
                    .padding(.top, 2)
            }
            Spacer()
        }
        .padding(14)
        .frame(maxWidth: .infinity)
        .background(RoundedRectangle(cornerRadius: 10).fill(Color(white: 0.12)))
    }

    // MARK: - Sections

    private var timing: some View {
        Section("Timing") {
            HStack {
                Text("Every").frame(width: 110, alignment: .leading)
                // Snapped to sensible steps: a linear 5s–5h slider would make
                // everything under a minute impossible to hit.
                Slider(value: intervalIndex, in: 0...Double(SettingsView.intervals.count - 1), step: 1)
                Text(SettingsView.humanDuration(cfg.intervalSeconds))
                    .font(.callout.monospacedDigit())
                    .foregroundStyle(.secondary)
                    .frame(width: 90, alignment: .trailing)
            }

            Picker("Time on screen", selection: $settings.cfg.dwellMode) {
                Text("Scale with message length").tag("length")
                Text("Same for every message").tag("fixed")
            }
            .pickerStyle(.radioGroup)

            if cfg.dwellMode == "fixed" {
                slider("Duration", value: $settings.cfg.dwellSeconds, in: 1...20, step: 0.5,
                       format: { String(format: "%.1fs", $0) })
            } else {
                slider("Minimum", value: $settings.cfg.dwellBase, in: 1...10, step: 0.1,
                       format: { String(format: "%.1fs", $0) })
                slider("Per character", value: $settings.cfg.dwellPerCharacter, in: 0...0.2, step: 0.005,
                       format: { String(format: "%.3fs", $0) })
                slider("Maximum", value: $settings.cfg.dwellMax, in: 2...30, step: 0.5,
                       format: { String(format: "%.1fs", $0) })
            }

            slider("Typing speed", value: $settings.cfg.typeSpeed, in: 0...0.1, step: 0.002,
                   format: { $0 == 0 ? "instant" : String(format: "%.3fs/char", $0) })
        }
    }

    private var sizeAndPosition: some View {
        Section("Size & position") {
            slider("Size", value: $settings.cfg.scale, in: 0.5...4, step: 0.05,
                   format: { String(format: "%.2f×", $0) })

            Picker("Corner", selection: $settings.cfg.position) {
                Text("Bottom right").tag(Corner.bottomRight)
                Text("Bottom left").tag(Corner.bottomLeft)
                Text("Top right").tag(Corner.topRight)
                Text("Top left").tag(Corner.topLeft)
            }

            Picker("Display", selection: $settings.cfg.screenIndex) {
                Text("Follow keyboard focus").tag(-1)
                ForEach(Array(NSScreen.screens.enumerated()), id: \.offset) { i, screen in
                    Text("\(i + 1) — \(Int(screen.frame.width))×\(Int(screen.frame.height))").tag(i)
                }
            }

            slider("Edge margin", value: $settings.cfg.margin, in: 0...120, step: 1,
                   format: { "\(Int($0))pt" })
            slider("Bubble width", value: $settings.cfg.maxBubbleWidth, in: 140...600, step: 10,
                   format: { "\(Int($0))pt" })
        }
    }

    private var colors: some View {
        Section("Colors") {
            ColorPicker("Accent — eyes, antenna, chest",
                        selection: colorBinding(\.accent), supportsOpacity: false)
            ColorPicker("Shell — body plastic",
                        selection: colorBinding(\.shell), supportsOpacity: false)
            ColorPicker("Visor — face panel",
                        selection: colorBinding(\.visor), supportsOpacity: false)
            HStack {
                Spacer()
                Button("Reset colors") {
                    let d = Config()
                    settings.cfg.accent = d.accent
                    settings.cfg.shell = d.shell
                    settings.cfg.visor = d.visor
                }
            }
        }
    }

    private var faces: some View {
        Section("Face") {
            HStack(spacing: 10) {
                ForEach(Face.allCases, id: \.self) { face in
                    Button {
                        settings.cfg.face = face
                    } label: {
                        VStack(spacing: 6) {
                            FaceView(style: RobotStyle(accent: cfg.accentColor,
                                                       shell: Color(hex: cfg.shell),
                                                       visor: Color(hex: cfg.visor),
                                                       scale: 1.0, face: face),
                                     blink: false)
                                .frame(width: 44, height: 30)
                                .background(RoundedRectangle(cornerRadius: 7).fill(Color(hex: cfg.visor)))
                            Text(face.rawValue.capitalized).font(.caption2)
                        }
                        .padding(6)
                        .background(
                            RoundedRectangle(cornerRadius: 8)
                                .fill(cfg.face == face ? cfg.accentColor.opacity(0.22) : .clear)
                        )
                        .overlay(
                            RoundedRectangle(cornerRadius: 8)
                                .stroke(cfg.face == face ? cfg.accentColor : .clear, lineWidth: 1.5)
                        )
                    }
                    .buttonStyle(.plain)
                }
                Spacer()
            }
        }
    }

    private var messages: some View {
        Section("Messages") {
            Picker("Message file", selection: $settings.cfg.messagesFile) {
                ForEach(SettingsView.discoverMessageFiles(), id: \.self) { path in
                    Text(path).tag(path)
                }
            }
            Toggle("Shuffle instead of playing in order", isOn: $settings.cfg.shuffle)
            HStack {
                Button("Edit this file…") {
                    if let url = fileNextToApp(cfg.messagesFile) { NSWorkspace.shared.open(url) }
                }
                Button("Show in Finder") {
                    if let url = fileNextToApp(cfg.messagesFile) {
                        NSWorkspace.shared.activateFileViewerSelecting([url])
                    }
                }
                Spacer()
            }
        }
    }

    private var footer: some View {
        VStack(alignment: .leading, spacing: 8) {
            if let error = settings.saveError {
                Text(error).font(.caption).foregroundStyle(.red)
            }
            HStack {
                Button("Reset everything to defaults") { settings.cfg = Config() }
                Spacer()
            }
        }
    }

    // MARK: - Helpers

    private func slider(_ title: String, value: Binding<Double>,
                        in range: ClosedRange<Double>, step: Double,
                        format: @escaping (Double) -> String) -> some View {
        HStack {
            Text(title).frame(width: 110, alignment: .leading)
            Slider(value: value, in: range, step: step)
            Text(format(value.wrappedValue))
                .font(.callout.monospacedDigit())
                .foregroundStyle(.secondary)
                .frame(width: 90, alignment: .trailing)
        }
    }

    /// Steps the "Every" slider snaps to, 5 seconds up to 5 hours.
    static let intervals: [Double] = [
        5, 10, 15, 20, 30, 45,
        60, 90, 120, 180, 300, 600, 900, 1200, 1800, 2700,
        3600, 5400, 7200, 10800, 14400, 18000,
    ]

    private var intervalIndex: Binding<Double> {
        Binding(
            get: {
                let target = settings.cfg.intervalSeconds
                let nearest = SettingsView.intervals
                    .enumerated()
                    .min { abs($0.element - target) < abs($1.element - target) }
                return Double(nearest?.offset ?? 0)
            },
            set: { settings.cfg.intervalSeconds = SettingsView.intervals[Int($0.rounded())] }
        )
    }

    static func humanDuration(_ v: Double) -> String {
        if v < 60 { return "\(Int(v))s" }
        if v < 3600 {
            let m = Int(v) / 60, s = Int(v) % 60
            return s == 0 ? "\(m)m" : "\(m)m \(s)s"
        }
        let h = Int(v) / 3600, m = (Int(v) % 3600) / 60
        return m == 0 ? "\(h)h" : "\(h)h \(m)m"
    }

    private func colorBinding(_ key: WritableKeyPath<Config, String>) -> Binding<Color> {
        Binding(get: { Color(hex: settings.cfg[keyPath: key]) },
                set: { settings.cfg[keyPath: key] = $0.hexString })
    }

    /// messages.txt plus anything in packs/, so the picker stays in sync with disk.
    static func discoverMessageFiles() -> [String] {
        // Every root the loader searches, so a pack you drop in Application
        // Support shows up next to the shipped ones.
        var packs: [String] = []
        for root in searchRoots() {
            let dir = root.appendingPathComponent("packs")
            guard let names = try? FileManager.default.contentsOfDirectory(atPath: dir.path) else { continue }
            packs += names.filter { $0.hasSuffix(".txt") }.map { "packs/\($0)" }
        }
        var seen = Set<String>()
        return ["messages.txt"] + packs.sorted().filter { seen.insert($0).inserted }
    }
}

/// Labelled group box — keeps each section visually separated.
private struct Section<Content: View>: View {
    let title: String
    @ViewBuilder let content: Content

    init(_ title: String, @ViewBuilder content: () -> Content) {
        self.title = title
        self.content = content()
    }

    var body: some View {
        GroupBox {
            VStack(alignment: .leading, spacing: 10) { content }
                .frame(maxWidth: .infinity, alignment: .leading)
                .padding(6)
        } label: {
            Text(title).font(.headline)
        }
    }
}
