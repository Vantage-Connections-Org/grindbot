# Show HN

**Title** (80 char limit — this is 71):

```
Show HN: GrindBot – a tiny robot pops up in your screen corner and yells at you
```

**URL:** https://github.com/Vantage-Connections-Org/grindbot

**First comment:**

```
I built this for screen recordings. A small robot fades into a corner of the
screen, types one line, and disappears. The overlay is click-through, so it
never takes focus or eats a click meant for your editor — on macOS that's
ignoresMouseEvents, on Windows WS_EX_TRANSPARENT | WS_EX_NOACTIVATE. No dock
icon, no taskbar entry, no window to manage.

The messages are plain text files, one line each, and that's the whole product.
Five packs ship: hustle (LinkedIn parody), discipline (drill sergeant), habits
(the longest and driest of them), classic, and obituary, which is exactly what
it sounds like. Point messagesFile at your own file and it reads that instead.
104 lines total, which is not many, and the packs are the part I'd most like
other people to add to.

Two implementations, one config format: macOS is ~1200 lines of Swift/SwiftUI,
Windows is ~2600 lines of C#/WPF. Same config.json, same packs, same robot
drawn from the same geometry. There is no backend, no account and no network
code in either source tree — grep for URLSession or HttpClient and you get
nothing.

Things I'd rather you hear from me than find out:
- Neither build is signed. macOS Gatekeeper blocks it until you right-click
  Open once; Windows SmartScreen may ask.
- The Windows port has features macOS doesn't: theme, "only when I'm idle",
  drawing from every pack at once, and run-at-login.
- It's a toy. It does not track anything, does not know whether you actually
  got back to work, and will not make you productive. It will make your screen
  recordings funnier.

MIT. https://grindbot.melbora.com
```

**Notes for posting**

- Post the repo, not the landing page. HN clicks through to source.
- Do not post before a release exists; "build it yourself" kills a Show HN.
- If it gets traction the top comment will be about the obituary pack. That's
  fine — it's a real part of the product. Don't be defensive, and don't quote
  its darkest line in a reply.
- Answer the "why not just a cron job and notify-send" question honestly: you
  could, this one draws itself over your work without stealing focus and looks
  like something rather than a notification.
