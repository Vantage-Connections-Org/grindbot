// Every fact and URL the site states, in one place. Everything here was checked against
// the GrindBot source in the repo above this folder. If the app changes, change this first.

export const SITE_URL = process.env.NEXT_PUBLIC_SITE_URL ?? "https://grindbot.melbora.com";
export const REPO = "Vantage-Connections-Org/grindbot";
export const REPO_URL = `https://github.com/${REPO}`;
export const ISSUES_URL = `${REPO_URL}/issues`;
export const LICENSE_URL = `${REPO_URL}/blob/main/LICENSE`;

// There is no release asset yet: both buttons point at build instructions, not a download.
export const MAC_BUILD_URL = `${REPO_URL}#build-and-run`;
export const WINDOWS_BUILD_URL = `${REPO_URL}/blob/main/windows/README.md#build-and-run`;
export const CUSTOMIZING_URL = `${REPO_URL}/blob/main/CUSTOMIZING.md`;
export const PACKS_URL = `${REPO_URL}/tree/main/packs`;

export const NAME = "GrindBot";
export const TAGLINE = "A tiny robot yells at you. Free.";
export const TITLE = "GrindBot: a tiny robot yells at you. Free.";
export const DESCRIPTION =
  "A robot pops up in a corner of your screen, types one line, and disappears. Click-through, so it never takes focus. No dock icon, no taskbar entry. Every message is a text file you can swap mid-session. macOS and Windows, MIT, open source.";

/** The hero demo loop: a ~6s capture of the robot typing one line over a real editor.
 *  Null until the files are in public/. The hero renders the illustrated corner instead —
 *  there is no placeholder video and no poster frame standing in for footage that does
 *  not exist. Set this to the two paths and the <video> takes over. */
export const HERO_VIDEO: { webm: string; mp4: string; alt: string } | null = null;

/** Verbatim shell commands from the repo's two READMEs. */
export const MAC_BUILD = `git clone https://github.com/${REPO}.git GrindBot
cd GrindBot
./build.sh
open GrindBot.app`;

export const WINDOWS_BUILD = `git clone https://github.com/${REPO}.git grindbot
cd grindbot\\windows
.\\build.ps1 -Run`;

export const MAC_REQUIREMENTS = "macOS 13 or newer, plus the Xcode command line tools (xcode-select --install).";
export const WINDOWS_REQUIREMENTS =
  "Windows 10 or 11, plus the .NET 8 SDK to build it. Build once with -SelfContained and the dist folder runs on a machine with no .NET at all.";

/** The five robot faces, from the Face enum in Sources/Config.swift. */
export const FACES = [
  { name: "visor", note: "Two glowing eyes. The default." },
  { name: "cyclops", note: "One big lens." },
  { name: "pixel", note: "8-bit." },
  { name: "angry", note: "Angled brows." },
  { name: "dot", note: "Eyes and a smile." },
] as const;

/** config.json keys, with defaults from config.default.json and notes from the README table. */
export const CONFIG_KEYS = [
  { key: "intervalSeconds", value: "20", note: "Seconds between messages. Anything from 1 second up; the settings slider covers 5s to 5h." },
  { key: "dwellMode", value: '"length"', note: '"length" scales time on screen with message length, "fixed" gives every message the same time.' },
  { key: "scale", value: "1.5", note: "Overall size. 1.0 is the original, clamped to 0.5–4.0." },
  { key: "position", value: '"bottomRight"', note: "Which corner: bottomRight, bottomLeft, topRight, topLeft. The layout mirrors itself on the left." },
  { key: "screenIndex", value: "0", note: "Which display: 0 is primary, 1 the next, -1 follows keyboard focus." },
  { key: "face", value: '"visor"', note: "visor, cyclops, pixel, angry or dot." },
  { key: "accent", value: '"#33D6A8"', note: "Eyes, antenna and chest light." },
  { key: "shell", value: '"#FCFCFC"', note: "Robot body plastic. Ears, arms and neck are derived shades of it." },
  { key: "visor", value: '"#212121"', note: "The dark face panel behind the eyes." },
  { key: "typeSpeed", value: "0.022", note: "Seconds per character for the typewriter effect. 0 shows the whole message at once." },
  { key: "shuffle", value: "false", note: "Random order instead of in-order. Plays the whole list before reshuffling." },
  { key: "messagesFile", value: '"messages.txt"', note: "Path to the message file, relative to the app." },
] as const;

/** Live star count from the GitHub API (cached for an hour). Null if the API is unreachable. */
export async function githubStars(): Promise<number | null> {
  try {
    const res = await fetch(`https://api.github.com/repos/${REPO}`, {
      headers: { Accept: "application/vnd.github+json" },
      next: { revalidate: 3600 },
    });
    if (!res.ok) return null;
    const data = (await res.json()) as { stargazers_count?: number };
    return typeof data.stargazers_count === "number" ? data.stargazers_count : null;
  } catch {
    return null;
  }
}

export const FAQ: { q: string; a: string }[] = [
  {
    q: "Is GrindBot signed?",
    a: "No. On macOS build.sh ad-hoc signs the app, but it is not signed by Apple, so if you move it somewhere macOS treats as quarantined you right-click and choose Open once to get past Gatekeeper. The Windows build is not code-signed either; you build it yourself from source with the .NET 8 SDK.",
  },
  {
    q: "Does it steal focus or interrupt a recording?",
    a: "No. The overlay window is click-through on both platforms (ignoresMouseEvents on macOS, WS_EX_TRANSPARENT plus WS_EX_NOACTIVATE on Windows), so clicks pass straight to whatever is underneath. macOS runs it as an accessory app with LSUIElement set, so there is no dock icon and no app switcher entry; on Windows the overlay sets ShowInTaskbar to false. It lives in the menu bar or the system tray instead.",
  },
  {
    q: "Does it work with more than one monitor?",
    a: "Yes. The screenIndex key picks the display: 0 is your primary, 1 the next, and -1 follows keyboard focus. On Windows the monitor list is reordered so the primary comes first, matching what the settings picker shows.",
  },
  {
    q: "How do I add my own messages?",
    a: "Messages are one per line in a plain text file. Blank lines and lines starting with # are ignored. Edit messages.txt, or drop your own file in packs/ and point messagesFile at it, then hit Reload config & messages in the menu. No rebuild. On macOS the files sit next to the app; on Windows they live in %APPDATA%\\GrindBot.",
  },
  {
    q: "Does it phone home?",
    a: "No. There is no backend, no account and no network code at all: neither the Swift sources nor the Windows sources make a single HTTP request. It reads your config and message files and draws a robot.",
  },
  {
    q: "Where do I download it?",
    a: "There is no packaged release yet, so you build it from source. On macOS that is ./build.sh; on Windows it is .\\build.ps1 -Run. Both take one command and are on this page.",
  },
];
