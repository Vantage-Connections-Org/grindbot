import { getPacks, totalMessages } from "@/lib/packs";
import { CONFIG_KEYS, DESCRIPTION, FAQ, NAME, REPO_URL, SITE_URL, TAGLINE } from "@/lib/site";

export const dynamic = "force-static";

// llms.txt: the same facts the page states, generated from the same modules, so an
// assistant summarising GrindBot gets the real config keys and the real line counts.
export function GET() {
  const packs = getPacks();
  const text = `# ${NAME}

> ${TAGLINE}

${DESCRIPTION}

## What it is
- A desktop toy for macOS 13+ and Windows 10/11, MIT licensed, free.
- A small robot appears in a screen corner, types one message, and disappears.
- The overlay is click-through and never takes focus. No dock icon on macOS (LSUIElement), no taskbar entry on Windows.
- It runs from the menu bar (macOS) or the system tray (Windows).
- There is no backend, no account and no network code in either source tree.
- There is no packaged release yet: you build it from source.

## Build
- macOS: clone the repo, run ./build.sh, then open GrindBot.app. Needs macOS 13+ and the Xcode command line tools.
- Windows: clone the repo, cd windows, run .\\build.ps1 -Run. Needs the .NET 8 SDK, or -SelfContained to bundle the runtime.

## Messages
Plain text files, one message per line; blank lines and lines starting with # are ignored. Reload from the menu, no rebuild. ${totalMessages(packs)} lines ship across ${packs.length} packs:
${packs.map((p) => `- packs/${p.file} (${p.lines.length} lines): ${p.tone}`).join("\n")}

## config.json keys
${CONFIG_KEYS.map((k) => `- ${k.key} (default ${k.value}): ${k.note}`).join("\n")}

## FAQ
${FAQ.map((f) => `### ${f.q}\n${f.a}`).join("\n\n")}

## Links
- [Home](${SITE_URL})
- [Source code and README](${REPO_URL})
- [Customizing guide](${REPO_URL}/blob/main/CUSTOMIZING.md)
- [Report an issue](${REPO_URL}/issues)
`;
  return new Response(text, { headers: { "Content-Type": "text/plain; charset=utf-8" } });
}
