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
| `Sources/AppDelegate.swift` | Window setup, screen placement, menu bar, the interval timer |

Everything is drawn with SwiftUI shapes — no image assets, so any change is a
code change and scales cleanly at any size.

After editing, rebuild and relaunch:

```
./build.sh && killall GrindBot; open GrindBot.app
```

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

## Gotchas

- **Give the root view a definite frame.** `NSHostingView` grows the window to
  the view's intrinsic size, which pushes the robot off screen. That's why
  `PopupView` ends with an explicit `.frame(width:height:)`.
- **Menu items need an explicit `target`.** A `nil` target leaves the item
  silently disabled.
- **`NSScreen.main` follows keyboard focus**, so it isn't stable across
  launches on a multi-display setup. `screenIndex` indexes `NSScreen.screens`
  instead.
- **The window is click-through** (`ignoresMouseEvents = true`). If you want
  the robot to respond to clicks, turn that off — but then it can intercept
  clicks meant for whatever is underneath.
