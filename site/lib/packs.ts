import { readFileSync } from "node:fs";
import { join } from "node:path";

// The packs are read straight out of the repo one level up at build time, so the line
// counts and the quoted lines on the page are the file, not a copy of it. Server only.

const PACKS_DIR = join(process.cwd(), "..", "packs");

export type Pack = {
  slug: string;
  file: string;
  tone: string;
  /** Held back from the hero and the OG image: bleak, or reads badly out of context. */
  guarded: boolean;
  lines: string[];
};

/** Tone lines come from the pack table in the repo README. Order is display order:
 *  habits and hustle lead, the two guarded packs come last. */
const PACK_META = [
  { slug: "habits", tone: "Hours, momentum, decay — aimed at the habit", guarded: false },
  { slug: "hustle", tone: "Aggressive entrepreneur", guarded: false },
  { slug: "discipline", tone: "No excuses, drill sergeant", guarded: false },
  { slug: "classic", tone: "The originals", guarded: true },
  { slug: "obituary", tone: "Mortality and legacy. The bleak one", guarded: true },
] as const;

/** A message file the app would read: one per line, blanks and # comments dropped. */
function readMessages(file: string): string[] {
  return readFileSync(join(PACKS_DIR, file), "utf8")
    .split(/\r?\n/)
    .map((line) => line.trim())
    .filter((line) => line.length > 0 && !line.startsWith("#"));
}

export function getPacks(): Pack[] {
  return PACK_META.map(({ slug, tone, guarded }) => {
    const file = `${slug}.txt`;
    return { slug, file, tone, guarded, lines: readMessages(file) };
  });
}

export function totalMessages(packs: Pack[]): number {
  return packs.reduce((sum, p) => sum + p.lines.length, 0);
}

/** Pull one line out of a pack to quote on the page, matched on its opening words.
 *  Throws at build time if the line has been edited or removed, so the site can never
 *  show a message the app would not. Guarded packs are refused outright. */
export function quote(packs: Pack[], slug: string, startsWith: string): string {
  const pack = packs.find((p) => p.slug === slug);
  if (!pack) throw new Error(`quote(): no pack named ${slug}`);
  if (pack.guarded) throw new Error(`quote(): ${slug} is guarded and must not be quoted`);
  const line = pack.lines.find((l) => l.startsWith(startsWith));
  if (!line) throw new Error(`quote(): packs/${pack.file} no longer contains a line starting "${startsWith}"`);
  return line;
}
