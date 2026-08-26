#!/bin/bash
set -euo pipefail
cd "$(dirname "$0")"

APP="GrindBot.app"
rm -rf "$APP"
mkdir -p "$APP/Contents/MacOS" "$APP/Contents/Resources"

# First run: seed a personal config from the shipped defaults.
[ -f config.json ] || cp config.default.json config.json

swiftc -O Sources/*.swift -o "$APP/Contents/MacOS/GrindBot"
cp messages.txt config.json "$APP/Contents/Resources/"

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
