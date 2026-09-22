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
    q: "Is it code-signed?",
    a: "No. build.sh ad-hoc signs the macOS app; Apple has not signed it. Move it somewhere macOS treats as quarantined and Gatekeeper blocks it until you right-click and choose Open once. The Windows build is not signed either, and PowerShell may refuse to run build.ps1 until you unblock it once with Unblock-File .\\build.ps1.",
  },
  {
    q: "Does it steal focus?",
    a: "No. The window sets ignoresMouseEvents on macOS and WS_EX_TRANSPARENT with WS_EX_NOACTIVATE on Windows, so clicks land on whatever is underneath it. macOS runs it with LSUIElement, so there is no dock icon and no app-switcher entry. On Windows the overlay sets ShowInTaskbar to false. It runs from the menu bar or the system tray.",
  },
  {
    q: "Which monitor does it use?",
    a: "The one screenIndex points at. 0 is your primary, 1 the next, -1 follows keyboard focus. On Windows the monitor list is reordered to put the primary first, so the numbers match the settings picker.",
  },
  {
    q: "How do I add my own messages?",
    a: "One message per line in a text file. Blank lines and lines starting with # are ignored. Edit messages.txt, or drop a file in packs/ and point messagesFile at it, then hit Reload config & messages. No rebuild. The files sit next to the app on macOS and in %APPDATA%\\GrindBot on Windows.",
  },
  {
    q: "Does it phone home?",
    a: "No. There is no backend and no account. Neither source tree contains a single HTTP call: it reads your config and message files, and draws a robot.",
  },
  {
    q: "Where is the download?",
    a: "There is no release yet. You build it from source, which is one command on either platform, both of them on this page.",
  },
];
