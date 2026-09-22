import AppKit
import SwiftUI

// MARK: - Where the popup sits

enum Corner: String, CaseIterable {
    case bottomRight, bottomLeft, topRight, topLeft

    var isLeft: Bool { self == .bottomLeft || self == .topLeft }
    var isTop: Bool { self == .topLeft || self == .topRight }

    var alignment: Alignment {
        switch self {
        case .bottomRight: return .bottomTrailing
        case .bottomLeft:  return .bottomLeading
        case .topRight:    return .topTrailing
        case .topLeft:     return .topLeading
        }
    }

    /// Anchor for the pop-in animation, so it grows out of its own corner.
    var anchor: UnitPoint {
        switch self {
        case .bottomRight: return .bottomTrailing
        case .bottomLeft:  return .bottomLeading
        case .topRight:    return .topTrailing
        case .topLeft:     return .topLeading
        }
    }
}

// MARK: - Which robot face

enum Face: String, CaseIterable {
    case visor, cyclops, pixel, angry, dot
}

// MARK: - Config

/// Everything a user can change without touching Swift. Read from config.json;
/// every key is optional and falls back to the value set here.
struct Config {
    var intervalSeconds: Double = 20
    var dwellMode: String = "length"        // "length" (scales with text) or "fixed"
    var dwellSeconds: Double = 5            // used when dwellMode == "fixed"
    var dwellBase: Double = 3.2             // length mode: floor
    var dwellPerCharacter: Double = 0.055   // length mode: added per character
    var dwellMax: Double = 9                // length mode: ceiling
    var scale: Double = 1.5
    var position: Corner = .bottomRight
    var screenIndex: Int = 0                // -1 follows keyboard focus
    var face: Face = .visor
    var accent: String = "#33D6A8"        // eyes, antenna, chest light
    var shell: String = "#FCFCFC"         // robot plastic
    var visor: String = "#212121"         // the dark face panel
    var typeSpeed: Double = 0.022           // 0 disables the typewriter effect
    var shuffle: Bool = false
    var messagesFile: String = "messages.txt"
    /// "messagesFile" given as an array. Windows has always honoured this; macOS
    /// used to read the key as a string, see nothing usable, and fall back.
    var messageFileList: [String]? = nil
    /// Draw from every message file we can find rather than a fixed selection.
    var allMessageFiles = false
    /// Hold messages until you have stopped typing for idleSeconds.
    var idleOnly = false
    var idleSeconds: Double = 120
    var margin: Double = 22                 // gap from the screen edge, in points
    var maxBubbleWidth: Double = 280        // before scale is applied

    var accentColor: Color { Color(hex: accent) }

    var robotStyle: RobotStyle {
        RobotStyle(accent: Color(hex: accent), shell: Color(hex: shell),
                   visor: Color(hex: visor), scale: scale, face: face)
    }

    /// Every file the deck should draw from, in order.
    func messageFiles() -> [String] {
        if allMessageFiles { return SettingsView.discoverMessageFiles() }
        if let list = messageFileList, !list.isEmpty { return list }
        return [messagesFile]
    }

    /// Cheap comparison so a reload only happens when the selection really moved.
    func messageFilesKey() -> String {
        allMessageFiles ? "*" : messageFiles().joined(separator: "|")
    }

    /// Seconds the bubble stays up for a given message.
    func dwell(for text: String) -> Double {
        if dwellMode == "fixed" { return max(0.5, dwellSeconds) }
        // Floor it: a hand-edited "dwellMax": 0 would otherwise hide the bubble
        // before the spring animation even lands, which reads as a broken app.
        return max(0.5, min(dwellMax, dwellBase + Double(text.count) * dwellPerCharacter))
    }
}

extension Config {
    /// Missing file, malformed JSON, or unknown keys all degrade to defaults
    /// rather than failing — this file is meant to be hand-edited.
    static func load() -> Config {
        var c = Config()
        guard let url = fileNextToApp("config.json"),
              let data = try? Data(contentsOf: url),
              let obj = (try? JSONSerialization.jsonObject(with: data)) as? [String: Any]
        else { return c }

        func number(_ key: String) -> Double? { (obj[key] as? NSNumber)?.doubleValue }

        c.intervalSeconds     = number("intervalSeconds").map { max(1, $0) } ?? c.intervalSeconds
        c.dwellSeconds        = number("dwellSeconds") ?? c.dwellSeconds
        c.dwellBase           = number("dwellBase") ?? c.dwellBase
        c.dwellPerCharacter   = number("dwellPerCharacter") ?? c.dwellPerCharacter
        c.dwellMax            = number("dwellMax") ?? c.dwellMax
        c.scale               = number("scale").map { min(4, max(0.5, $0)) } ?? c.scale
        c.typeSpeed           = number("typeSpeed").map { max(0, $0) } ?? c.typeSpeed
        c.margin              = number("margin") ?? c.margin
        c.maxBubbleWidth      = number("maxBubbleWidth") ?? c.maxBubbleWidth
        c.screenIndex         = (obj["screenIndex"] as? NSNumber)?.intValue ?? c.screenIndex
        c.shuffle             = (obj["shuffle"] as? Bool) ?? c.shuffle

        if let s = obj["dwellMode"] as? String { c.dwellMode = s }
        if let s = obj["accent"] as? String { c.accent = s }
        if let s = obj["shell"] as? String { c.shell = s }
        if let s = obj["visor"] as? String { c.visor = s }
        c.allMessageFiles = (obj["allMessageFiles"] as? Bool) ?? c.allMessageFiles
        c.idleOnly        = (obj["idleOnly"] as? Bool) ?? c.idleOnly
        c.idleSeconds     = number("idleSeconds").map { max(5, $0) } ?? c.idleSeconds
        if let s = obj["messagesFile"] as? String, !s.isEmpty {
            c.messagesFile = s
        } else if let list = obj["messagesFile"] as? [String] {
            let cleaned = list.filter { !$0.trimmingCharacters(in: .whitespaces).isEmpty }
            if !cleaned.isEmpty {
                c.messageFileList = cleaned
                // Keep the single key valid too, so a config written by either
                // platform still means something to the other.
                c.messagesFile = cleaned[0]
            }
        }
        if let s = obj["position"] as? String, let p = Corner(rawValue: s) { c.position = p }
        if let s = obj["face"] as? String, let f = Face(rawValue: s) { c.face = f }
        return c
    }
}

extension Config {
    var dictionary: [String: Any] {
        [
            "intervalSeconds": intervalSeconds, "dwellMode": dwellMode,
            "dwellSeconds": dwellSeconds, "dwellBase": dwellBase,
            "dwellPerCharacter": dwellPerCharacter, "dwellMax": dwellMax,
            "scale": scale, "position": position.rawValue, "screenIndex": screenIndex,
            "face": face.rawValue, "accent": accent, "shell": shell, "visor": visor,
            "typeSpeed": typeSpeed, "shuffle": shuffle,
            "margin": margin, "maxBubbleWidth": maxBubbleWidth,
            "allMessageFiles": allMessageFiles,
            "idleOnly": idleOnly, "idleSeconds": idleSeconds,
            // An array selection has to survive the save, or the 400ms autosave
            // would quietly collapse a multi-file config back to one file.
            "messagesFile": messageFileList ?? messagesFile,
        ]
    }

    /// Writes config.json back where it was found, or to Application Support.
    /// Returns an error string on failure.
    @discardableResult
    func save() -> String? {
        let url = writeDestination("config.json")
        do {
            let data = try JSONSerialization.data(withJSONObject: dictionary,
                                                  options: [.prettyPrinted, .sortedKeys])
            try data.write(to: url, options: .atomic)
            return nil
        } catch {
            return "Couldn't save to \(url.path): \(error.localizedDescription)"
        }
    }
}

/// Colors + size + face, everything the robot needs to draw itself.
struct RobotStyle {
    let accent: Color
    let shell: Color
    let visor: Color
    let scale: Double
    let face: Face
}

extension Color {
    /// Shifts brightness, so one shell color can drive the whole plastic
    /// gradient plus the darker ears, arms and neck.
    func adjust(_ delta: Double) -> Color {
        guard let c = NSColor(self).usingColorSpace(.sRGB) else { return self }
        return Color(red: min(1, max(0, Double(c.redComponent) + delta)),
                     green: min(1, max(0, Double(c.greenComponent) + delta)),
                     blue: min(1, max(0, Double(c.blueComponent) + delta)))
    }

    var hexString: String {
        guard let c = NSColor(self).usingColorSpace(.sRGB) else { return "#000000" }
        return String(format: "#%02X%02X%02X",
                      Int(round(c.redComponent * 255)),
                      Int(round(c.greenComponent * 255)),
                      Int(round(c.blueComponent * 255)))
    }
}

/// Everything GrindBot writes lives here. The app itself may sit in /Applications,
/// which the user can't write to — the Windows build has used %APPDATA%\GrindBot
/// for the same reason.
let dataDir: URL = {
    let base = FileManager.default.urls(for: .applicationSupportDirectory, in: .userDomainMask).first
        ?? URL(fileURLWithPath: NSHomeDirectory()).appendingPathComponent("Library/Application Support")
    let dir = base.appendingPathComponent("GrindBot", isDirectory: true)
    try? FileManager.default.createDirectory(at: dir, withIntermediateDirectories: true)
    return dir
}()

/// First match wins:
///   1. next to GrindBot.app — portable: drop a config.json in and it takes over
///   2. ~/Library/Application Support/GrindBot — the normal home
///   3. the repo root — only when running out of the source tree
func searchRoots() -> [URL] {
    let appDir = Bundle.main.bundleURL.deletingLastPathComponent()
    var roots = [appDir, dataDir]
    var dir = appDir
    for _ in 0..<6 {
        let up = dir.deletingLastPathComponent()
        if up.path == dir.path { break }
        dir = up
        if FileManager.default.fileExists(atPath: dir.appendingPathComponent("config.default.json").path) {
            roots.append(dir)
            break
        }
    }
    return roots
}

/// Prefers a file on disk so users can edit config and messages without opening
/// the bundle; falls back to the bundled copy.
func fileNextToApp(_ name: String) -> URL? {
    for root in searchRoots() {
        let candidate = root.appendingPathComponent(name)
        if FileManager.default.fileExists(atPath: candidate.path) { return candidate }
    }
    // Then the copy inside the bundle. Done by path, not by resource name, so a
    // nested "packs/hustle.txt" resolves — forResource: would not find it.
    if let resources = Bundle.main.resourceURL {
        let bundled = resources.appendingPathComponent(name)
        if FileManager.default.fileExists(atPath: bundled.path) { return bundled }
    }
    let stem = (name as NSString).deletingPathExtension
    let ext = (name as NSString).pathExtension
    return Bundle.main.url(forResource: stem, withExtension: ext)
}

/// Where a save should land: back where the file was found, or dataDir if it
/// doesn't exist yet. Never inside the app bundle.
func writeDestination(_ name: String) -> URL {
    for root in searchRoots() {
        let candidate = root.appendingPathComponent(name)
        if FileManager.default.fileExists(atPath: candidate.path),
           FileManager.default.isWritableFile(atPath: candidate.path) { return candidate }
    }
    return dataDir.appendingPathComponent(name)
}

extension Color {
    /// "#33D6A8" / "33D6A8". Anything unparseable falls back to the default green.
    init(hex: String) {
        let raw = hex.trimmingCharacters(in: CharacterSet(charactersIn: "# "))
        guard raw.count == 6, let v = UInt32(raw, radix: 16) else {
            self = Color(red: 0.20, green: 0.84, blue: 0.66)
            return
        }
        self = Color(red: Double((v >> 16) & 0xFF) / 255,
                     green: Double((v >> 8) & 0xFF) / 255,
                     blue: Double(v & 0xFF) / 255)
    }
}

/// Observable box so a config reload re-renders the SwiftUI views in place.
final class Settings: ObservableObject {
    @Published var cfg: Config
    @Published var saveError: String?
    init(_ cfg: Config) { self.cfg = cfg }
}
