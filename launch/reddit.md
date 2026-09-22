# Reddit drafts

One per subreddit, rewritten each time. Cross-posting the same paragraph to
three subs is the fastest way to get all three removed.

Read each sub's self-promotion rule first. r/macapps wants the developer flair
and a clear "I made this". Several Windows subs require a post age or karma
floor. None of them want a landing page as the primary link.

---

## r/macapps

**Title:** I made a menu bar app that puts a tiny robot in your screen corner to yell at you

**Body:**

```
It sits in the menu bar with no dock icon (LSUIElement), and every so often a
small robot fades into a corner of the screen, types one line, and disappears.
The overlay is click-through, so it never steals focus — you can keep typing
while it's on screen.

Everything is a text file. Five message packs ship (hustle, discipline, habits,
classic, obituary) and messagesFile can point at your own. Timing, size, screen
corner, display, robot colours and face are all in config.json, and the
settings window writes the same file live as you drag a slider.

macOS 13+, universal binary, MIT, free, no account, no telemetry. It is not
signed by Apple, so the first launch needs right-click → Open.

Source and download: https://github.com/Vantage-Connections-Org/grindbot
```

---

## r/windowsapps

**Title:** GrindBot — a tray app that drops a small robot into your screen corner with a one-line message

**Body:**

```
Windows port of a little overlay toy. It lives in the system tray, draws a
small robot over whatever you're doing, types one message, and vanishes. The
window is WS_EX_TRANSPARENT | WS_EX_NOACTIVATE, so clicks go straight through
to whatever is underneath and it never takes focus.

The Windows build has a few things the Mac one doesn't: light/dark/auto theme,
"only speak when I've been idle for N seconds", drawing messages from every
pack at once, and start-with-Windows. Settings live in %APPDATA%\GrindBot, so
rebuilding or deleting the folder never touches them.

Self-contained, so no .NET install needed. Not code-signed, so SmartScreen may
ask once. MIT, free, no telemetry.

https://github.com/Vantage-Connections-Org/grindbot
```

---

## r/commandline (only if the tone fits that week)

**Title:** A desktop overlay whose entire config surface is one JSON file and some .txt files

**Body:**

```
Wrote a small overlay toy and kept the config deliberately boring: one
config.json, and message packs that are literally one line of text per message,
# for comments, blank lines ignored. Reload re-reads both without a rebuild.

The interesting constraint was making it click-through on both platforms so it
can draw over a terminal without ever stealing focus or a keystroke. macOS:
ignoresMouseEvents on a borderless NSWindow. Windows: WS_EX_TRANSPARENT |
WS_EX_NOACTIVATE on a WPF window with AllowsTransparency.

Two implementations, same config format, ~1200 lines of Swift and ~2600 of C#.
MIT. https://github.com/Vantage-Connections-Org/grindbot
```
