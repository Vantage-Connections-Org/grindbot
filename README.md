# GrindBot

A tiny robot pops up in the corner of your Mac screen, says something, and
disappears. Built for screen recordings and social posts.

No dock icon, no window to manage, no backend. It sits in the menu bar and
draws itself over whatever you're doing — click-through, so it never steals
focus or interrupts what you're recording.

## Requirements

macOS 13+ and the Xcode command line tools:

```
xcode-select --install
```

## Build and run

```
git clone <your-repo-url> GrindBot
cd GrindBot
./build.sh
open GrindBot.app
```

`build.sh` compiles `Sources/*.swift` into `GrindBot.app` and ad-hoc signs
it. The app is unsigned by Apple, so if you move it somewhere macOS treats as
quarantined, right-click → Open once to get past Gatekeeper.

## Menu bar

Everything is under the 🤖 icon:

| Item | What it does |
|---|---|
| Settings… (⌘,) | Open the settings window — every option below, with live preview |
| Say something now | Fire a message immediately — useful for hitting a cue while recording |
| Turn off / Turn on | Stop the popups without quitting. Icon becomes 💤. Turning it back on fires one right away |
| Face | Switch robot face for this session |
| Reload config & messages | Re-read both files without rebuilding |
| Edit messages… / Edit config.json… | Open the files in your default editor |
| Quit | Shut it down |

## Settings window

**Settings…** in the menu opens a window with every option in one place:
timing, size, screen corner and display, robot colors, face, and message file.

Changes apply immediately — the robot resizes, moves, or recolors as you drag
a slider — and save themselves to `config.json`. There's no OK or Cancel; what
you see is what's running. **Show a message now** fires one so you can check
the result without waiting for the timer.

Editing `config.json` by hand still works and is the better path for version
control or sharing a setup with someone else. Hit **Reload config & messages**
after an external edit.

## Messages

One per line in `messages.txt`. Blank lines and lines starting with `#` are
ignored.

```
Lock in bro or your family dies 💀
WHO IS GONNA CARRY THE BOATS?
```

Edit the file, then hit **Reload config & messages**. No rebuild needed.

Three packs ship in `packs/`:

| Pack | Lines | Tone |
|---|---|---|
| `classic.txt` | 5 | The originals |
| `hustle.txt` | 24 | Aggressive entrepreneur |
| `discipline.txt` | 15 | No excuses, drill sergeant |

Point `messagesFile` at one, or write your own:

```json
{ "messagesFile": "packs/hustle.txt" }
```

Messages cycle in order. Set `"shuffle": true` for random order (it shuffles
the whole list, plays through it, then reshuffles — so nothing repeats until
everything has been shown).

## Configuration

Edit `config.json` next to the app, then **Reload config & messages**. Every
key is optional; anything missing or malformed falls back to the default, so
a broken file degrades rather than crashes.

| Key | Default | What it does |
|---|---|---|
| `intervalSeconds` | `20` | Seconds between messages |
| `dwellMode` | `"length"` | `"length"` scales time on screen with message length, `"fixed"` uses the same time for every message |
| `dwellBase` | `3.2` | Length mode: minimum seconds on screen |
| `dwellPerCharacter` | `0.055` | Length mode: seconds added per character |
| `dwellMax` | `9` | Length mode: ceiling, no matter how long the text |
| `dwellSeconds` | `5` | Fixed mode: seconds on screen |
| `scale` | `1.5` | Overall size. `1.0` is the original, clamped to `0.5`–`4.0` |
| `position` | `"bottomRight"` | `bottomRight`, `bottomLeft`, `topRight`, `topLeft`. The layout mirrors itself on the left |
| `screenIndex` | `0` | Which display: `0` is primary, `1` the next, `-1` follows keyboard focus |
| `face` | `"visor"` | `visor`, `cyclops`, `pixel`, `angry`, `dot` |
| `accent` | `"#33D6A8"` | Eyes, antenna, chest light |
| `shell` | `"#FCFCFC"` | Robot body plastic. Ears, arms and neck are derived shades of it |
| `visor` | `"#212121"` | The dark face panel behind the eyes |
| `typeSpeed` | `0.022` | Seconds per character for the typewriter effect. `0` shows the whole message at once |
| `shuffle` | `false` | Random order instead of in-order |
| `messagesFile` | `"messages.txt"` | Path to the message file, relative to the app |
| `margin` | `22` | Gap from the screen edge, in points |
| `maxBubbleWidth` | `280` | Bubble width before wrapping, before `scale` is applied |

### Timing

With `dwellMode: "length"`, time on screen is:

```
min(dwellMax, dwellBase + characters × dwellPerCharacter)
```

So a 20-character line sits for ~4.3s and a 60-character line for ~6.5s, both
capped at `dwellMax`. Typing happens inside that window, so longer messages
still get read time after they finish typing.

## Faces

`visor` (two glowing eyes), `cyclops` (one big lens), `pixel` (8-bit),
`angry` (angled brows), `dot` (eyes and a smile).

Pick one in the settings window, from the **Face** menu, or set `face` in
`config.json`.

Colors are independent of the face: `accent` drives the eyes, antenna and
chest light, `shell` the body, `visor` the panel behind the eyes. A dark
`shell` with a bright `accent` gives a very different robot from the default.

## Customizing

See [CUSTOMIZING.md](CUSTOMIZING.md) for the code layout, how to add your own
face, and how to change the drawing.

## License

MIT — see [LICENSE](LICENSE).
