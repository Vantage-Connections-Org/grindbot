# GrindBot for Windows

The same robot, rewritten for Windows. A tiny robot pops up in the corner of
your screen, says something, and disappears — click-through, so it never steals
focus or interrupts what you're recording.

No taskbar entry, no window to manage, no backend. It sits in the system tray
and draws itself over whatever you're doing.

<img src="../docs/faces.png" alt="The five faces" width="420">

## Requirements

Windows 10 or 11, and one of:

- the [.NET 8 SDK](https://dotnet.microsoft.com/download) to build it, or
- nothing at all, if you build once with `-SelfContained` and copy `dist\` around

## Build and run

```powershell
git clone <your-repo-url> grindbot
cd grindbot\windows
.\build.ps1 -Run
```

That publishes `dist\GrindBot.exe` and the shipped `packs\`. The robot appears
in your system tray and says its first line after a second or so.

| Flag | What it does |
|---|---|
| `-Run` | Start GrindBot as soon as it's built |
| `-SelfContained` | Bundle the .NET runtime, so `dist\` runs on a machine with no .NET installed (~150 MB) |
| `-Portable` | Keep your settings inside `dist\` instead of `%APPDATA%`, so the folder travels as one unit |

`packs\` always refreshes on build. Your settings are never touched — see below.

If PowerShell blocks the script, either
`powershell -ExecutionPolicy Bypass -File .\build.ps1` or unblock it once with
`Unblock-File .\build.ps1`.

## Where your settings live

```
%APPDATA%\GrindBot\
  config.json     every setting, written the moment you change one
  messages.txt    your messages
  packs\          your own packs, merged into the picker with the shipped ones
```

**Not** in `dist\`. That folder is build output — a clean rebuild, a `-SelfContained`
switch, or a stray delete would take your settings with it. `%APPDATA%\GrindBot`
survives all three. GrindBot creates it and lays down the defaults the first time
it runs, including on a machine that has never seen the source.

The settings window shows the exact path, with an **Open folder** button next to
**Show a message now**.

Resolution order, first match wins:

1. **Next to `GrindBot.exe`** — drop a `config.json` in `dist\` and it takes over.
   This is what `-Portable` sets up, and how you carry a configured copy on a stick
2. **`%APPDATA%\GrindBot`** — the normal home
3. **The repo root** — only when running straight out of the source tree

The same order resolves `messages.txt` and any `packs/…` path, so a pack in
`%APPDATA%\GrindBot\packs` works exactly like a shipped one.

If you built an earlier version that kept settings in `dist\`, `build.ps1` moves
them to `%APPDATA%\GrindBot` once and tells you it did.

## Tray menu

Everything is under the robot icon in the system tray. Left-click or
right-click opens the menu; double-click fires a message immediately.

| Item | What it does |
|---|---|
| Settings… | Open the settings window — every option below, with live preview |
| Say something now | Fire a message immediately — useful for hitting a cue while recording |
| Turn off / Turn on | Stop the popups without quitting. The icon closes its eyes. Turning it back on fires one right away |
| Face | Switch robot face |
| Start with Windows | Add or remove GrindBot from your login items (one `HKCU\...\Run` value, nothing else) |
| Reload config & messages | Re-read both files without rebuilding |
| Edit messages… / Edit config.json… | Open the files in your default editor |
| Quit | Shut it down |

The tray icon is drawn from your own `accent` and `shell` colors, so recoloring
the robot recolors the icon.

## Settings window

**Settings…** opens a window with every option in one place: timing, size,
screen corner and display, robot colors, face, and message file.

Changes apply immediately — the robot resizes, moves, or recolors as you drag a
slider — and save themselves to `config.json`. There's no OK or Cancel; what you
see is what's running. The **Every** slider snaps to sensible steps from 5
seconds up to 5 hours. **Show a message now** fires one so you can check the
result without waiting for the timer.

Windows has no system color picker control to borrow, so each color is a hex
field plus red/green/blue sliders and a live swatch. Typing a hex value and
pressing Enter works too.

## Messages and configuration

Identical to the macOS build — see the [main README](../README.md) for
`messages.txt`, the packs in `packs/`, and every `config.json` key. On Windows
both files live in `%APPDATA%\GrindBot` (see above), not next to the exe.

Two keys behave slightly differently here:

| Key | Windows behaviour |
|---|---|
| `screenIndex` | `0` is your **primary** display, `1` the next, and so on. `-1` follows the focused window. Windows enumerates monitors in device order, so the list is reordered to put the primary first — matching what the settings picker shows |
| `margin` | In device-independent pixels, not macOS points. Same numbers, same result at 100% scaling; scales automatically on high-DPI displays |

And one key is Windows-only:

| Key | Default | What it does |
|---|---|---|
| `theme` | `"auto"` | Speech-bubble colors: `auto` follows your Windows light/dark setting, `dark` and `light` pin it |

## What's different from the Mac version

Everything user-facing is the same. Under the hood:

- **AppKit + SwiftUI → WPF.** The robot is the same drawing, shape for shape and
  number for number, rebuilt in WPF vector shapes. `Sources\RobotView.swift` and
  `src\Robot.cs` are a line-by-line read of each other.
- **Menu bar → system tray.** `NSStatusItem` becomes a `NotifyIcon`, and the
  emoji icon becomes a drawn one — Windows tray icons are bitmaps, not text.
- **The bubble is a translucent plate, not blurred glass.** macOS gives
  `.regularMaterial` a live backdrop blur for free; there's no equivalent for a
  click-through layered window on Windows, so the bubble is a solid translucent
  panel that follows your system theme instead.
- **Emoji render in monochrome.** WPF draws Segoe UI Emoji as outline glyphs, so
  💀 comes out as a white skull rather than a colored one. Everything still reads.
- **Per-monitor DPI is explicit.** The window is placed in physical pixels via
  `SetWindowPos`, so mixed-DPI multi-monitor setups land where you asked.

## Debugging

Set `GRINDBOT_DEBUG=1` before launching and GrindBot writes a `grindbot.log`
next to the exe: which config it loaded, how many messages it found, and every
line it says.

```powershell
$env:GRINDBOT_DEBUG = "1"; .\dist\GrindBot.exe
```

## Code layout

| File | What's in it |
|---|---|
| `src\Program.cs` | App lifecycle, tray menu, timers — the old `AppDelegate` |
| `src\Config.cs` | Config load/save, colors, paths, message-file discovery |
| `src\Popup.cs` | The overlay window, the speech bubble shape, the message deck |
| `src\Robot.cs` | The robot and the five faces, drawn as WPF shapes |
| `src\SettingsWindow.cs` | The settings surface |
| `src\TrayIcon.cs` | The drawn tray icon |
| `src\Native.cs` | Win32 interop: click-through, placement, theme, run-at-login |

Adding a face is the same two edits as on macOS: a `case` in `FaceVisual` and a
member on the `Face` enum.
