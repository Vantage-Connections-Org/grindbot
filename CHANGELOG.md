# Changelog

## v1.0.0 (unreleased)

First release. The app existed before this, but only as something you built
yourself from a clone; nothing was ever published, and the macOS build in
particular had defects that only appear once it is packaged and installed.

**macOS fixes that only bite an installed build**

- The download shipped **none of the five message packs**. Message files are
  resolved next to the bundle, so choosing `packs/hustle.txt` left the robot
  saying the file was empty, forever. Packs are now inside the bundle and beside
  it in every artifact.
- Settings were written next to the app. Installed to `/Applications` that is
  unwritable, so every change to a slider raised a save error. They now live in
  `~/Library/Application Support/GrindBot`, matching what the Windows build has
  always done with `%APPDATA%\GrindBot`, and a `config.json` next to the app
  still wins so a portable folder keeps working.
- Every build was **Apple Silicon only** and would not launch on an Intel Mac.
  The app is now a universal binary.
- `build.sh` copied the developer's own `config.json` into the bundle. It ships
  `config.default.json`.
- Every shipped pack is CRLF, and the parser trimmed only spaces, so every
  message carried a trailing carriage return and blank lines became empty
  bubbles.
- `dwell` had no floor, so a hand-edited `dwellMax` of `0` hid the bubble before
  its animation finished.
- The bundle had no icon.

**Windows fixes**

- Dragging a colour slider leaked a forever-running animation into WPF's timing
  tree on every tick, and re-read every message file from disk on every tick.
- A recurring fault produced an unescapable series of modal stack traces. It now
  reports once, in words, and keeps running.
- **Reload config & messages** silently rewrote the file it had just read,
  deleting the comments out of a hand-edited `config.json`.
- `messagesFile` could point outside its folder and be launched from the menu.
  It is now restricted to a `.txt` under a known root.
- Deleting the folder left a permanent startup entry pointing at a missing exe.
  There is an uninstaller, and the exe has real version metadata and an icon.

**macOS caught up with Windows**

`theme`, `idleOnly`, `idleSeconds`, `allMessageFiles` and array `messagesFile`
all work on macOS now, along with **Start at login** and **Only when I'm idle**
in the menu. Every key `config.default.json` documents works on both platforms.

**Everything else**

- CI builds both platforms on every push and checks the things that silently
  break the app: both architecture slices, the packs reaching the artifact, the
  shipped config being the defaults, the Dock-icon flag, and the icon.
- A tagged release publishes the Windows zip, a macOS zip and a
  drag-to-Applications disk image.
- A website at [grindbot.melbora.com](https://grindbot.melbora.com).

**Known limitations**

- Neither build is signed. macOS blocks it until you right-click → Open once;
  PowerShell may refuse `build.ps1` until `Unblock-File`.
- The macOS build is verified by CI but has not been run on a real Mac.
