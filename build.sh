#!/bin/bash
set -euo pipefail
cd "$(dirname "$0")"

APP="GrindBot.app"
rm -rf "$APP"
mkdir -p "$APP/Contents/MacOS" "$APP/Contents/Resources"

# First run: seed a personal config from the shipped defaults.
[ -f config.json ] || cp config.default.json config.json

# Universal: macos-14 runners are arm64, and swiftc with no -target would build
# an Apple-Silicon-only binary that Intel Macs cannot launch at all.
swiftc -O -target arm64-apple-macos13.0  Sources/*.swift -o "$APP/Contents/MacOS/GrindBot-arm64"
swiftc -O -target x86_64-apple-macos13.0 Sources/*.swift -o "$APP/Contents/MacOS/GrindBot-x86_64"
lipo -create -output "$APP/Contents/MacOS/GrindBot" \
     "$APP/Contents/MacOS/GrindBot-arm64" "$APP/Contents/MacOS/GrindBot-x86_64"
rm -f "$APP/Contents/MacOS/GrindBot-arm64" "$APP/Contents/MacOS/GrindBot-x86_64"

# Ship the defaults, never the config.json sitting in this working copy — that
# one is the developer's personal settings.
cp messages.txt "$APP/Contents/Resources/"
cp config.default.json "$APP/Contents/Resources/config.json"
# Bundled so a lone GrindBot.app still has all five packs: someone who drags
# only the app out of the disk image would otherwise have none.
cp -R packs "$APP/Contents/Resources/"

cat > "$APP/Contents/Info.plist" <<'PLIST'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleName</key><string>GrindBot</string>
  <key>CFBundleDisplayName</key><string>GrindBot</string>
  <key>CFBundleExecutable</key><string>GrindBot</string>
  <key>CFBundleIdentifier</key><string>local.grindbot</string>
  <key>CFBundlePackageType</key><string>APPL</string>
  <key>CFBundleShortVersionString</key><string>1.0</string>
  <key>LSMinimumSystemVersion</key><string>13.0</string>
  <key>LSUIElement</key><true/>
  <key>NSPrincipalClass</key><string>NSApplication</string>
  <key>NSHighResolutionCapable</key><true/>
</dict>
</plist>
PLIST

codesign --force --deep --sign - "$APP" 2>/dev/null || true
echo "Built $(pwd)/$APP"
