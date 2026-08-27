using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Media;

namespace GrindBot;

/// Where the popup sits.
public enum Corner { bottomRight, bottomLeft, topRight, topLeft }

/// Which robot face.
public enum Face { visor, cyclops, pixel, angry, dot }

public static class CornerExt
{
    public static bool IsLeft(this Corner c) => c == Corner.bottomLeft || c == Corner.topLeft;
    public static bool IsTop(this Corner c) => c == Corner.topLeft || c == Corner.topRight;
}

/// Colors + size + face, everything the robot needs to draw itself.
public readonly record struct RobotStyle(Color Accent, Color Shell, Color Visor, double Scale, Face Face);

/// Everything a user can change without touching C#. Read from config.json;
/// every key is optional and falls back to the value set here.
public sealed class Config
{
    public double intervalSeconds = 20;
    public string dwellMode = "length";        // "length" (scales with text) or "fixed"
    public double dwellSeconds = 5;            // used when dwellMode == "fixed"
    public double dwellBase = 3.2;             // length mode: floor
    public double dwellPerCharacter = 0.055;   // length mode: added per character
    public double dwellMax = 9;                // length mode: ceiling
    public double scale = 1.5;
    public Corner position = Corner.bottomRight;
    public int screenIndex = 0;                // -1 follows keyboard focus
    public Face face = Face.visor;
    public string accent = "#33D6A8";          // eyes, antenna, chest light
    public string shell = "#FCFCFC";           // robot plastic
    public string visor = "#212121";           // the dark face panel
    public double typeSpeed = 0.022;           // 0 disables the typewriter effect
    public bool shuffle = false;
    public string messagesFile = "messages.txt";   // single path; what macOS reads
    public List<string>? messageFileList;          // Windows: "messagesFile" as an array
    public bool allMessageFiles = false;           // Windows: draw from every file found
    public double margin = 22;                 // gap from the screen edge, in DIPs
    public double maxBubbleWidth = 280;        // before scale is applied
    public string theme = "auto";              // Windows-only: auto | dark | light
    public bool idleOnly = false;              // Windows-only: hold messages while you're working
    public double idleSeconds = 120;           // Windows-only: how long you must be idle first

    public Color AccentColor => ColorX.FromHex(accent);
    public RobotStyle RobotStyle =>
        new(ColorX.FromHex(accent), ColorX.FromHex(shell), ColorX.FromHex(visor), scale, face);

    /// Seconds the bubble stays up for a given message.
    public double Dwell(string text)
    {
        if (dwellMode == "fixed") return Math.Max(0.5, dwellSeconds);
        return Math.Min(dwellMax, dwellBase + TextX.GraphemeCount(text) * dwellPerCharacter);
    }

    /// Every file the deck should draw from, in order.
    public List<string> MessageFiles()
    {
        if (allMessageFiles) return Paths.DiscoverMessageFiles();
        if (messageFileList is { Count: > 0 }) return new List<string>(messageFileList);
        return new List<string> { messagesFile };
    }

    /// Cheap comparison so a reload only happens when the selection really moved.
    public string MessageFilesKey() =>
        allMessageFiles ? "*" : string.Join("|", MessageFiles());

    public Config Clone()
    {
        var c = (Config)MemberwiseClone();
        // MemberwiseClone is shallow: without this, editing a clone's list
        // would reach back and mutate the config it was copied from.
        c.messageFileList = messageFileList is null ? null : new List<string>(messageFileList);
        return c;
    }

    /// Missing file, malformed JSON, or unknown keys all degrade to defaults
    /// rather than failing — this file is meant to be hand-edited.
    public static Config Load()
    {
        var c = new Config();
        var path = Paths.NextToApp("config.json");
        if (path is null) return c;

        JsonObject? obj;
        try
        {
            var opts = new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip,
            };
            obj = JsonNode.Parse(File.ReadAllText(path), null, opts) as JsonObject;
        }
        catch { return c; }
        if (obj is null) return c;

        double? Num(string key)
        {
            if (!obj.TryGetPropertyValue(key, out var n) || n is null) return null;
            try { return n.GetValue<double>(); } catch { return null; }
        }
        string? Str(string key)
        {
            if (!obj.TryGetPropertyValue(key, out var n) || n is null) return null;
            try { return n.GetValue<string>(); } catch { return null; }
        }
        bool? Bool(string key)
        {
            if (!obj.TryGetPropertyValue(key, out var n) || n is null) return null;
            try { return n.GetValue<bool>(); } catch { return null; }
        }

        c.intervalSeconds   = Num("intervalSeconds") is double i ? Math.Max(1, i) : c.intervalSeconds;
        c.dwellSeconds      = Num("dwellSeconds") ?? c.dwellSeconds;
        c.dwellBase         = Num("dwellBase") ?? c.dwellBase;
        c.dwellPerCharacter = Num("dwellPerCharacter") ?? c.dwellPerCharacter;
        c.dwellMax          = Num("dwellMax") ?? c.dwellMax;
        c.scale             = Num("scale") is double s ? Math.Min(4, Math.Max(0.5, s)) : c.scale;
        c.typeSpeed         = Num("typeSpeed") is double t ? Math.Max(0, t) : c.typeSpeed;
        c.margin            = Num("margin") ?? c.margin;
        c.maxBubbleWidth    = Num("maxBubbleWidth") ?? c.maxBubbleWidth;
        c.screenIndex       = Num("screenIndex") is double si ? (int)si : c.screenIndex;
        c.shuffle           = Bool("shuffle") ?? c.shuffle;
        c.idleOnly          = Bool("idleOnly") ?? c.idleOnly;
        c.idleSeconds       = Num("idleSeconds") is double idle ? Math.Max(5, idle) : c.idleSeconds;

        if (Str("dwellMode") is string dm) c.dwellMode = dm;
        if (Str("accent") is string a) c.accent = a;
        if (Str("shell") is string sh) c.shell = sh;
        if (Str("visor") is string v) c.visor = v;
        if (Str("theme") is string th) c.theme = th;
        c.allMessageFiles = Bool("allMessageFiles") ?? c.allMessageFiles;
        if (Str("messagesFile") is string mf && mf.Length > 0) c.messagesFile = mf;
        else if (obj["messagesFile"] is JsonArray arr)
        {
            // Array form is a Windows extension. macOS reads this key as a
            // string, sees nothing usable, and falls back to its default —
            // which is why messagesFile below is also kept as a valid path.
            var list = arr.Select(n => n?.GetValue<string>())
                          .Where(v => !string.IsNullOrWhiteSpace(v))
                          .Select(v => v!)
                          .ToList();
            if (list.Count > 0)
            {
                c.messageFileList = list;
                c.messagesFile = list[0];
            }
        }
        if (Str("position") is string p && Enum.TryParse<Corner>(p, true, out var corner)) c.position = corner;
        if (Str("face") is string f && Enum.TryParse<Face>(f, true, out var fc)) c.face = fc;
        return c;
    }

    private JsonObject ToJson()
    {
        // Sorted keys, to match the macOS build's output and keep diffs quiet.
        // Slider drift turns 1.2 into 1.2000000000000002; nothing here needs
        // more than four places, and the file is meant to be hand-edited.
        static double R(double v) => Math.Round(v, 4);

        var o = new JsonObject();
        o["accent"] = accent;
        o["dwellBase"] = R(dwellBase);
        o["dwellMax"] = R(dwellMax);
        o["dwellMode"] = dwellMode;
        o["dwellPerCharacter"] = R(dwellPerCharacter);
        o["dwellSeconds"] = R(dwellSeconds);
        o["face"] = face.ToString();
        o["idleOnly"] = idleOnly;
        o["idleSeconds"] = R(idleSeconds);
        o["intervalSeconds"] = R(intervalSeconds);
        o["margin"] = R(margin);
        o["maxBubbleWidth"] = R(maxBubbleWidth);
        o["allMessageFiles"] = allMessageFiles;
        if (messageFileList is { Count: > 1 })
        {
            var arr = new JsonArray();
            foreach (var f in messageFileList) arr.Add(f);
            o["messagesFile"] = arr;
        }
        else o["messagesFile"] = messagesFile;
        o["position"] = position.ToString();
        o["scale"] = R(scale);
        o["screenIndex"] = screenIndex;
        o["shell"] = shell;
        o["shuffle"] = shuffle;
        o["theme"] = theme;
        o["typeSpeed"] = R(typeSpeed);
        o["visor"] = visor;
        return o;
    }

    /// Writes config.json next to the exe. Returns an error string on failure —
    /// the app can sit somewhere the user can't write to.
    public string? Save()
    {
        var path = Paths.ResolveWrite("config.json");
        try
        {
            var json = ToJson().ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            var tmp = path + ".tmp";
            File.WriteAllText(tmp, json);
            File.Move(tmp, path, true);
            return null;
        }
        catch (Exception e)
        {
            return "Couldn't save to " + path + ": " + e.Message;
        }
    }
}

/// Observable box so a config change re-renders the views in place.
public sealed class Settings
{
    private Config _cfg;
    public Config Cfg
    {
        get => _cfg;
        set { _cfg = value; Changed?.Invoke(value); }
    }
    public string? SaveError;

    /// Fired for every mutation, including in-place ones via Mutate.
    public event Action<Config>? Changed;

    public Settings(Config cfg) { _cfg = cfg; }

    /// Edit a copy, then publish it — mirrors SwiftUI's value-type @Published.
    public void Mutate(Action<Config> edit)
    {
        var next = _cfg.Clone();
        edit(next);
        Cfg = next;
    }
}

public static class ColorX
{
    /// "#33D6A8" / "33D6A8". Anything unparseable falls back to the default green.
    public static Color FromHex(string hex)
    {
        var raw = (hex ?? "").Trim().TrimStart('#').Trim();
        if (raw.Length == 6 && uint.TryParse(raw, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var v))
            return Color.FromRgb((byte)((v >> 16) & 0xFF), (byte)((v >> 8) & 0xFF), (byte)(v & 0xFF));
        return Color.FromRgb(51, 214, 168);
    }

    public static string ToHex(this Color c) => "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");

    /// Shifts brightness, so one shell color can drive the whole plastic
    /// gradient plus the darker ears, arms and neck.
    public static Color Adjust(this Color c, double delta)
    {
        static byte Clamp(double v) => (byte)Math.Round(Math.Min(255, Math.Max(0, v)));
        return Color.FromRgb(Clamp(c.R + delta * 255), Clamp(c.G + delta * 255), Clamp(c.B + delta * 255));
    }

    public static Color WithAlpha(this Color c, double a) =>
        Color.FromArgb((byte)Math.Round(Math.Min(1, Math.Max(0, a)) * 255), c.R, c.G, c.B);

    public static SolidColorBrush Brush(this Color c)
    {
        var b = new SolidColorBrush(c);
        b.Freeze();
        return b;
    }
}

public static class TextX
{
    /// Grapheme clusters, so 💀 and friends never get split mid-message.
    public static List<string> Graphemes(string s)
    {
        var result = new List<string>();
        var e = StringInfo.GetTextElementEnumerator(s ?? "");
        while (e.MoveNext()) result.Add((string)e.Current);
        return result;
    }

    public static int GraphemeCount(string s) => Graphemes(s).Count;
}

/// Set GRINDBOT_DEBUG=1 to get a running log next to the exe. Off by default,
/// and never fatal — a locked log file must not take the robot down.
public static class Log
{
    private static readonly bool On =
        Environment.GetEnvironmentVariable("GRINDBOT_DEBUG") is "1" or "true";

    public static void Write(string message)
    {
        if (!On) return;
        try
        {
            File.AppendAllText(Path.Combine(Paths.AppDir, "grindbot.log"),
                DateTime.Now.ToString("HH:mm:ss.fff") + "  " + message + Environment.NewLine);
        }
        catch { }
    }
}

public static class Paths
{
    /// Where GrindBot.exe lives. Build output — treat it as disposable.
    public static string AppDir { get; } =
        Path.GetDirectoryName(Environment.ProcessPath ?? AppContext.BaseDirectory)!;

    /// Where your settings live: %APPDATA%\GrindBot. Survives rebuilds, deleting
    /// dist\, and moving the exe — which the old "next to the app" spot did not.
    public static string DataDir { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GrindBot");

    /// First match wins:
    ///   1. next to the exe   — portable: drop a config.json in and it takes over
    ///   2. %APPDATA%\GrindBot — the normal home
    ///   3. the repo root      — only when running out of the source tree
    public static IEnumerable<string> Roots()
    {
        yield return AppDir;
        yield return DataDir;

        var dir = new DirectoryInfo(AppDir);
        for (int i = 0; i < 6 && dir?.Parent is not null; i++)
        {
            dir = dir.Parent;
            if (File.Exists(Path.Combine(dir.FullName, "config.default.json")))
            {
                yield return dir.FullName;
                yield break;
            }
        }
    }

    public static string? NextToApp(string name)
    {
        foreach (var root in Roots())
        {
            var p = Path.Combine(root, Relative(name));
            if (File.Exists(p)) return p;
        }
        return null;
    }

    /// Where a save should land: back where the file was found, or DataDir if
    /// it doesn't exist yet. Never silently into the build folder.
    public static string ResolveWrite(string name)
    {
        if (NextToApp(name) is string found) return found;
        var target = Path.Combine(DataDir, Relative(name));
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        return target;
    }

    /// First run anywhere — a fresh machine, or a copied dist\ folder — lays the
    /// shipped defaults down in DataDir so there is something to edit and a
    /// durable place to save it. Existing files are never touched.
    public static void SeedUserFiles()
    {
        Seed("config.json", "config.default.json");
        Seed("messages.txt", "messages.txt");
    }

    private static void Seed(string name, string resource)
    {
        if (NextToApp(name) is not null) return;
        if (Embedded(resource) is not string text) return;
        try
        {
            Directory.CreateDirectory(DataDir);
            File.WriteAllText(Path.Combine(DataDir, name), text);
        }
        catch { /* read-only profile: the app still runs on built-in defaults */ }
    }

    private static string? Embedded(string logicalName)
    {
        using var stream = typeof(Paths).Assembly.GetManifestResourceStream(logicalName);
        if (stream is null) return null;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static string Relative(string name) =>
        name.Replace('/', Path.DirectorySeparatorChar);

    /// messages.txt plus every packs/ folder we can see, so your own packs in
    /// %APPDATA%\GrindBot\packs show up next to the shipped ones.
    public static List<string> DiscoverMessageFiles()
    {
        var found = new List<string>();
        foreach (var root in Roots())
        {
            var packs = Path.Combine(root, "packs");
            if (!Directory.Exists(packs)) continue;
            found.AddRange(Directory.GetFiles(packs, "*.txt").Select(f => "packs/" + Path.GetFileName(f)));
        }
        return found
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .Prepend("messages.txt")
            .ToList();
    }
}
