# Customizing GrindBot

Most changes don't need code — see the config table in the
[README](README.md). This covers the rest.

## Code layout

| File | What's in it |
|---|---|
| `Sources/main.swift` | Entry point. Sets the app to accessory mode (no dock icon) |
| `Sources/Config.swift` | The `Config` struct, `config.json` parsing, `Corner` and `Face` enums, hex colors |
| `Sources/PopupModel.swift` | Timing for one message: typing, dwell, hide. Also `MessageDeck`, which reads the message file |
| `Sources/RobotView.swift` | The robot drawing. `RobotView` is the body, `FaceView` is what's inside the visor |
| `Sources/PopupView.swift` | `BubbleShape` (the speech bubble) and the layout that mirrors for left corners |
| `Sources/SettingsView.swift` | The settings window UI |
| `Sources/AppDelegate.swift` | Window setup, screen placement, menu bar, the interval timer, and the live-apply pipeline |

Everything is drawn with SwiftUI shapes — no image assets, so any change is a
code change and scales cleanly at any size.

After editing, rebuild and relaunch:

```
./build.sh && killall GrindBot; open GrindBot.app
```

## Config files

`config.default.json` is tracked in git and holds the shipped defaults.
`build.sh` copies it to `config.json` on first build, and `config.json` is
gitignored — so personal settings stay out of commits. If you change a
default for everyone, edit `config.default.json`.

## Add a message pack

Create `packs/yours.txt`, one message per line, then point `config.json` at
it:

```json
{ "messagesFile": "packs/yours.txt" }
```

Reload from the menu. Keep as many packs as you like and switch between them.

## Add a robot face

Three steps, all in code:

1. Add a case to `Face` in `Sources/Config.swift`:

   ```swift
   enum Face: String, CaseIterable {
       case visor, cyclops, pixel, angry, dot, yours
   }
   ```

2. Add a matching case and view to `FaceView` in `Sources/RobotView.swift`:

   ```swift
   case .yours: yours

   private var yours: some View {
       HStack(spacing: 9 * s) {
           eye(Circle(), 9, 9)
           eye(Circle(), 9, 9)
       }
   }
   ```

3. Rebuild. It shows up in the Face menu automatically, because the menu is
   built from `Face.allCases`.

Notes:

- Multiply every dimension by `s` (the scale factor) so your face scales with
  the `scale` config key.
- The `eye(_:_:_:)` helper fills a shape with the accent color, adds the glow,
  and handles blinking. Use it and blinking works for free.
- Anything you draw is clipped to the visor: roughly `44 × 30` points before
  scaling.

## Change the robot's body

`RobotView` builds the body from a `VStack`: `antenna`, `head`, a neck
rectangle, then `torso`. Each is a small computed property — edit them
directly. The `shell` gradient is the white plastic look, shared by the head
and torso.

The bob animation is one line at the bottom of `body`. The blink timer lives
in `PopupModel.init`.

## Change the speech bubble

`BubbleShape` in `Sources/PopupView.swift` draws a rounded rectangle with a
tail, as one continuous path — the tail is part of the outline rather than a
separate triangle, so the stroke has no seam where they meet.

The `flipped` flag mirrors the whole path for left-hand screen corners.

Bubble text style (font, size, weight) is in `PopupView.bubble`. The
background uses `.regularMaterial`, which adapts to light and dark mode. Swap
it for a solid color if you want a fixed look:

```swift
BubbleShape(scale: cfg.scale, flipped: onLeft)
    .fill(Color.black.opacity(0.85))
```

## Change the window

`PopupView.baseSize` is the window size at `scale: 1`. `AppDelegate`
sizes the window from the same numbers, so change it in one place.

If you make the bubble much wider or taller, raise `baseSize` too — content
larger than the window gets clipped, and a window taller than the screen gets
pushed off the edge.

## Add a new setting

Four steps:

1. Add the property to `Config` in `Sources/Config.swift`, with its default.
2. Parse it in `Config.load()` — use `number(...)` for numbers, or the
   `obj["key"] as? String` pattern for everything else.
3. Add it to `Config.dictionary` so the settings window can save it.
4. Add a control to the matching section in `Sources/SettingsView.swift`.
   Bind straight to the config: `$settings.cfg.yourKey`.

If the change needs to do something beyond redrawing — resize the window,
restart the timer, reload the message file — handle it in `AppDelegate.apply(_:)`,
which receives every config change.

## How live apply works

`Settings` holds the config as `@Published`. `AppDelegate.observeSettings()`
subscribes twice:

- immediately, to apply the change (resize, reposition, reload messages,
  restart the timer)
- debounced by 400ms, to write `config.json` — so dragging a slider doesn't
  hammer the disk

`apply(_:)` compares against the previously applied config and only acts on
what actually changed. That matters for the interval timer: restarting it on
every tick of a slider would reset the countdown each time.

## Gotchas

- **Give the root view a definite frame.** `NSHostingView` grows the window to
  the view's intrinsic size, which pushes the robot off screen. That's why
  `PopupView` ends with an explicit `.frame(width:height:)`.
- **Menu items need an explicit `target`.** A `nil` target leaves the item
  silently disabled.
- **`NSScreen.main` follows keyboard focus**, so it isn't stable across
  launches on a multi-display setup. `screenIndex` indexes `NSScreen.screens`
  instead.
- **`@Published` fires *before* the value is written.** The sink receives the
  new config, but reading `settings.cfg` back inside the sink can give you the
  old one. Use the value handed to you.
- **An `.accessory` app is never frontmost.** Call
  `NSApp.activate(ignoringOtherApps: true)` before showing the settings
  window, or it opens behind whatever you're looking at.
- **The window is click-through** (`ignoresMouseEvents = true`). If you want
  the robot to respond to clicks, turn that off — but then it can intercept
  clicks meant for whatever is underneath.
