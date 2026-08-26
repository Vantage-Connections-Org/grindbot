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
    var accent: String = "#33D6A8"
    var typeSpeed: Double = 0.022           // 0 disables the typewriter effect
    var shuffle: Bool = false
    var messagesFile: String = "messages.txt"
    var margin: Double = 22                 // gap from the screen edge, in points
    var maxBubbleWidth: Double = 280        // before scale is applied

    var accentColor: Color { Color(hex: accent) }

    /// Seconds the bubble stays up for a given message.
    func dwell(for text: String) -> Double {
        if dwellMode == "fixed" { return max(0.5, dwellSeconds) }
        return min(dwellMax, dwellBase + Double(text.count) * dwellPerCharacter)
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
        if let s = obj["messagesFile"] as? String, !s.isEmpty { c.messagesFile = s }
        if let s = obj["position"] as? String, let p = Corner(rawValue: s) { c.position = p }
        if let s = obj["face"] as? String, let f = Face(rawValue: s) { c.face = f }
        return c
    }
}

/// Prefers a file sitting next to RobotPopup.app so users can edit config and
/// messages without opening the bundle; falls back to the bundled copy.
func fileNextToApp(_ name: String) -> URL? {
    let neighbour = Bundle.main.bundleURL.deletingLastPathComponent().appendingPathComponent(name)
    if FileManager.default.fileExists(atPath: neighbour.path) { return neighbour }
    let stem = (name as NSString).deletingPathExtension
    let ext = (name as NSString).pathExtension
    return Bundle.main.url(forResource: stem, withExtension: ext)
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
    init(_ cfg: Config) { self.cfg = cfg }
}
