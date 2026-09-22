# Contributing

The message packs are the part of this project most worth adding to. A pack is
a text file; you do not need to build the app to write one, and a pull request
that only touches `packs/` is a completely normal contribution.

## Writing a pack

One message per line. Blank lines are ignored, and a line starting with `#` is
a comment — the first line of every shipped pack is a one-line description of
its tone, which is a good habit to copy.

```
# Hours, momentum, decay. Aimed at the habit, not the soul.
Your ambition has an expiration date. Check the label.
```

What makes a line work in this app specifically:

- **It has to land in about two seconds.** The bubble is on screen for
  `3.2 + 0.055 × characters` seconds, capped at 9. A line that needs a run-up
  is gone before it pays off.
- **Under ~90 characters.** Longer wraps to three lines and starts covering
  what the viewer is actually doing. `habits.txt` and `obituary.txt` sit at the
  top of that range deliberately.
- **One idea per line.** There is no second sentence to save the first.
- **It is read cold, mid-task.** No setup, no callbacks to other lines, no
  in-jokes that need the rest of the pack.
- **Pick a register and hold it.** The packs are separate because each one is
  consistent: `discipline.txt` never winks, `hustle.txt` never drops the
  parody. A mixed pack reads like an accident.

What will get a pack turned down: slurs or punching at a group, anything
sexual, anything that reads as a real instruction to hurt yourself, and brand
or personality impersonation. `obituary.txt` is bleak on purpose and stays; the
line between that and something genuinely harmful is whether it is fatalistic
about *time* or about *you*.

New packs go in `packs/<name>.txt`. Say in the pull request what the pack is
for in one sentence — if that sentence is hard to write, the pack is probably
two packs.

## Code

Two implementations, deliberately: `Sources/` is Swift for macOS, `windows/src`
is C# for Windows. They share `config.json`, the packs, and the robot's
geometry, and neither is generated from the other.

If you add a config key, add it to both, or say plainly in the pull request
that you have only done one — a key that silently does nothing on the other
platform is the bug class this project has had most often.

- macOS: `./build.sh`, then `open GrindBot.app`. Needs the Xcode command line
  tools.
- Windows: `cd windows; .\build.ps1 -Run`. Needs the .NET 8 SDK.

CI builds both on every push and checks the things that break silently: both
architecture slices in the macOS binary, the packs reaching the artifact, the
shipped config being the defaults rather than yours, and the app staying out of
the Dock.

## Reporting something

Say which OS and which build, what you expected, and what happened. For the
macOS build especially: it is verified by CI but has had little real-world use,
so "it did nothing at all" is a useful report, not a non-report.
