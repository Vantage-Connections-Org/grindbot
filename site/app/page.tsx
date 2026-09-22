import Image from "next/image";
import { GithubMark, SiteFooter, SiteHeader } from "@/components/SiteChrome";
import { getPacks, quote, totalMessages, type Pack } from "@/lib/packs";
import {
  CONFIG_KEYS,
  CUSTOMIZING_URL,
  FACES,
  FAQ,
  MAC_BUILD,
  MAC_BUILD_URL,
  MAC_REQUIREMENTS,
  NAME,
  PACKS_URL,
  REPO_URL,
  TAGLINE,
  WINDOWS_BUILD,
  WINDOWS_BUILD_URL,
  WINDOWS_REQUIREMENTS,
  githubStars,
} from "@/lib/site";

// Three facts about the mechanism, each checkable in the source. Set as engraved plaques.
const WHAT_IT_DOES = [
  {
    title: "Clicks go through it",
    body: "The window sets ignoresMouseEvents on macOS and WS_EX_TRANSPARENT with WS_EX_NOACTIVATE on Windows. It never receives a click, so it cannot swallow one meant for your editor.",
  },
  {
    title: "No dock icon, no taskbar entry",
    body: "LSUIElement on macOS, ShowInTaskbar false on Windows. It runs from the menu bar or the system tray, and nothing new appears in ⌘-Tab or Alt-Tab.",
  },
  {
    title: "Every message is a text file",
    body: "One line per message, # for comments. Edit the file, choose Reload config & messages, and the next popup is your line. No rebuild.",
  },
];

export default async function Home() {
  const packs = getPacks();
  const stars = await githubStars();
  const total = totalMessages(packs);

  // The three gallery captions are read out of packs/ at build time and the build fails if
  // any of the lines is edited or removed. Never classic, never obituary.
  const gallery = [
    {
      src: "/posters/momentum.webp",
      word: "Momentum",
      alt: "A framed poster: the robot alone at the head of an empty boardroom table under a single downlight, with MOMENTUM engraved beneath",
      file: "packs/habits.txt",
      line: quote(packs, "habits", "Momentum is a currency"),
    },
    {
      src: "/posters/discipline.webp",
      word: "Discipline",
      alt: "A framed poster: the robot standing alone in an empty warehouse at sunrise, casting a long shadow, engraved DISCIPLINE beneath",
      file: "packs/discipline.txt",
      line: quote(packs, "discipline", "Motivation left"),
    },
    {
      src: "/posters/synergy.webp",
      word: "Synergy",
      alt: "A framed poster: the robot pointing a stick at a Q3 Objectives chart beside a dead pot plant, an iced coffee and a notepad reading Together we can achieve greatness, engraved SYNERGY beneath",
      file: "packs/hustle.txt",
      line: quote(packs, "hustle", "Everyone wants the exit"),
    },
  ];

  return (
    <>
      <SiteHeader />

      <main className="paper-grain">
        {/* Hero. The poster is the LCP element and paints on the first render: nothing on
            this page is hidden until a scroll event. */}
        <section className="mx-auto grid max-w-5xl grid-cols-1 px-4 pb-20 pt-12 sm:px-6 md:pt-16">
          <h1 className="text-center font-display text-5xl leading-none tracking-tight sm:text-6xl md:text-7xl">
            {NAME}
          </h1>

          {/* The hanging: a brass nail and two wires, then the framed poster. */}
          <div className="mt-10 flex flex-col items-center">
            <span aria-hidden className="size-1.5 rounded-full bg-brass" />
            <span aria-hidden className="h-6 w-px bg-brass/45" />
          </div>
          <Image
            src="/posters/hero-persistence.webp"
            alt="A framed motivational poster hanging on a cream wall: a small white robot standing on a dark desk under a single spotlight, pointing at a framed PRODUCTIVITY bar chart, engraved PERSISTENCE, and beneath that the line a tiny robot yells at you"
            width={1600}
            height={905}
            priority
            quality={58}
            sizes="(min-width: 1024px) 64rem, 100vw"
            className="h-auto w-full"
          />

          <p className="plaque mt-8 text-center text-[13px] text-accent sm:text-sm">{TAGLINE}</p>
          <div className="rule-brass mx-auto mt-3 w-full max-w-md" />

          <p className="mx-auto mt-8 max-w-[56ch] text-center text-lg leading-relaxed text-muted">
            A robot appears in the corner of your screen, types one line, and disappears — every 20 seconds out of the
            box, on a slider that runs from 5 seconds to 5 hours.
          </p>

          <div className="mt-8 flex flex-wrap justify-center gap-3">
            <a
              href="#mac"
              className="inline-flex items-center rounded-sm bg-accent px-6 py-3 font-medium text-accent-ink transition hover:bg-accent-hover"
            >
              Build it on macOS
            </a>
            <a
              href="#windows"
              className="inline-flex items-center rounded-sm border border-brass/60 px-6 py-3 font-medium transition hover:border-brass hover:bg-accent-soft"
            >
              Build it on Windows
            </a>
          </div>

          <p className="mx-auto mt-6 max-w-[56ch] text-center text-sm text-muted">
            There is no release to download, so you build it from source. One command on either platform. MIT, macOS 13
            or newer, Windows 10 or 11.
          </p>

          <a
            href={REPO_URL}
            className="mx-auto mt-4 inline-flex items-center gap-2 text-sm text-muted transition hover:text-text"
          >
            <GithubMark size={16} />
            <span className="underline underline-offset-4">Source on GitHub</span>
            {stars !== null && <span className="tabular-nums">· {stars}★</span>}
          </a>
        </section>

        {/* What it does — three engraved plaques, no cards. */}
        <section className="mx-auto max-w-5xl px-4 pb-20 sm:px-6">
          <div className="rule-brass" />
          <div className="mt-10 grid grid-cols-1 gap-10 md:grid-cols-3 md:gap-8">
            {WHAT_IT_DOES.map((item) => (
              <div key={item.title} className="min-w-0">
                <h2 className="plaque text-[11px] leading-relaxed text-accent sm:text-xs">{item.title}</h2>
                <p className="mt-3 text-sm leading-relaxed text-muted">{item.body}</p>
              </div>
            ))}
          </div>
        </section>

        {/* The gallery: three posters hung in a row, each captioned with its own pack. */}
        <section id="gallery" className="mx-auto max-w-5xl scroll-mt-20 px-4 pb-20 sm:px-6">
          <h2 className="font-display text-3xl tracking-tight sm:text-4xl">Five packs hang in this room.</h2>
          <p className="mt-3 max-w-[62ch] text-muted">
            Each poster below is captioned with a line that ships in the matching file. The words are read out of{" "}
            <a href={PACKS_URL} className="underline underline-offset-4 hover:text-text">
              packs/
            </a>{" "}
            when this page is built, so the page cannot show you a message the app would not.
          </p>

          <div className="mt-12 grid grid-cols-1 gap-12 sm:grid-cols-2 lg:grid-cols-3">
            {gallery.map((poster) => (
              <figure key={poster.word} className="min-w-0">
                <div className="flex flex-col items-center">
                  <span aria-hidden className="size-1.5 rounded-full bg-brass" />
                  <span aria-hidden className="h-5 w-px bg-brass/45" />
                </div>
                <Image
                  src={poster.src}
                  alt={poster.alt}
                  width={900}
                  height={1206}
                  sizes="(min-width: 1024px) 20rem, (min-width: 640px) 45vw, 92vw"
                  className="h-auto w-full"
                />
                <figcaption className="mt-5">
                  <p className="font-display text-lg leading-snug">{poster.line}</p>
                  <p className="mt-2 font-mono text-xs text-muted">{poster.file}</p>
                </figcaption>
              </figure>
            ))}
          </div>
        </section>

        {/* Packs */}
        <section id="packs" className="mx-auto max-w-5xl scroll-mt-20 px-4 pb-20 sm:px-6">
          <div className="rule-brass" />
          <h2 className="mt-10 font-display text-3xl tracking-tight sm:text-4xl">
            {packs.length} packs, {total} lines.
          </h2>
          <p className="mt-3 max-w-[62ch] text-muted">
            Point <code className="font-mono text-sm">messagesFile</code> at one, or write your own and keep as many as
            you like. Two of them are folded shut below: classic is the original joke and its best line does not survive
            a list, and obituary is about dying.
          </p>

          <div className="mt-10 grid grid-cols-1 gap-8 md:grid-cols-2">
            {packs
              .filter((pack) => !pack.guarded)
              .map((pack) => (
                <PackEntry key={pack.slug} pack={pack} />
              ))}
          </div>

          <div className="mt-10 grid grid-cols-1 gap-8">
            {packs
              .filter((pack) => pack.guarded)
              .map((pack) => (
                <details key={pack.slug} className="group min-w-0 border-t border-border pt-5">
                  <summary className="flex cursor-pointer list-none flex-wrap items-baseline gap-x-3 gap-y-1 [&::-webkit-details-marker]:hidden">
                    <span className="plaque text-[11px] text-accent">packs/{pack.file}</span>
                    <span className="text-sm text-muted tabular-nums">{pack.lines.length} lines</span>
                    <span className="text-sm text-muted">— {pack.tone}</span>
                    <span aria-hidden className="ml-auto text-muted transition group-open:rotate-45">
                      +
                    </span>
                  </summary>
                  <ul className="mt-5 grid grid-cols-1 gap-2 sm:grid-cols-2">
                    {pack.lines.map((line) => (
                      <li key={line} className="text-sm leading-snug text-muted">
                        {line}
                      </li>
                    ))}
                  </ul>
                </details>
              ))}
          </div>
        </section>

        {/* Config */}
        <section id="config" className="mx-auto max-w-5xl scroll-mt-20 px-4 pb-20 sm:px-6">
          <div className="rule-brass" />
          <h2 className="mt-10 font-display text-3xl tracking-tight sm:text-4xl">
            One <span className="font-mono text-[0.8em]">config.json</span>, every knob.
          </h2>
          <p className="mt-3 max-w-[62ch] text-muted">
            Every key is optional. Anything missing or malformed falls back to its default, so a broken file degrades
            rather than crashes. The settings window writes the same file and applies changes as you drag. The defaults
            below are the ones in <code className="font-mono text-sm">config.default.json</code>.
          </p>

          <div className="mt-8 grid grid-cols-1 gap-8 lg:grid-cols-[minmax(0,2fr)_minmax(0,1fr)]">
            <div className="min-w-0">
              {/* Scrolls sideways on its own rather than pushing the page wider. */}
              <div className="min-w-0 overflow-x-auto border-y border-border">
                <table className="w-full min-w-[34rem] text-left text-sm">
                  <thead className="border-b border-border">
                    <tr>
                      <th scope="col" className="plaque px-3 py-3 text-[10px] font-normal text-accent">
                        Key
                      </th>
                      <th scope="col" className="plaque px-3 py-3 text-[10px] font-normal text-accent">
                        Default
                      </th>
                      <th scope="col" className="plaque px-3 py-3 text-[10px] font-normal text-accent">
                        What it does
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {CONFIG_KEYS.map((row) => (
                      <tr key={row.key} className="border-b border-border/70 align-top last:border-0">
                        <td className="px-3 py-3 font-mono text-[13px] whitespace-nowrap text-accent">{row.key}</td>
                        <td className="px-3 py-3 font-mono text-[13px] whitespace-nowrap text-muted">{row.value}</td>
                        <td className="px-3 py-3 text-muted">{row.note}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              <p className="mt-6 max-w-[62ch] text-sm text-muted">
                <code className="font-mono">face</code> takes one of {FACES.map((f) => f.name).join(", ")}. There are
                a few more keys than these: the dwell timing values,{" "}
                <code className="font-mono">margin</code> and <code className="font-mono">maxBubbleWidth</code>. Every
                key works on both platforms. The full table is in the{" "}
                <a href={`${REPO_URL}#configuration`} className="underline underline-offset-4 hover:text-text">
                  README
                </a>
                ; changing the drawing itself is in{" "}
                <a href={CUSTOMIZING_URL} className="underline underline-offset-4 hover:text-text">
                  CUSTOMIZING.md
                </a>
                .
              </p>
            </div>

            <figure className="min-w-0">
              <Image
                src="/settings.png"
                alt="The GrindBot settings window on macOS: timing, size and position, colours, face and message file, with a live robot preview at the top"
                width={594}
                height={1100}
                sizes="(min-width: 1024px) 18rem, 92vw"
                className="h-auto w-full border border-border"
              />
              <figcaption className="plaque mt-4 text-[10px] leading-relaxed text-accent">
                The same keys, as a window
              </figcaption>
              <p className="mt-2 text-sm text-muted">
                There is no OK and no Cancel. What you see is running, and it saves itself.
              </p>
            </figure>
          </div>
        </section>

        {/* Build */}
        <section id="build" className="mx-auto max-w-5xl scroll-mt-20 px-4 pb-20 sm:px-6">
          <div className="rule-brass" />
          <h2 className="mt-10 font-display text-3xl tracking-tight sm:text-4xl">Build it.</h2>
          <p className="mt-3 max-w-[62ch] text-muted">
            Both platforms build from the same repo. Neither build is signed by its vendor, and each one asks you to say
            so once.
          </p>
          <div className="mt-8 grid grid-cols-1 gap-8 md:grid-cols-2">
            <BuildBlock
              id="mac"
              title="macOS"
              requirements={MAC_REQUIREMENTS}
              code={MAC_BUILD}
              href={MAC_BUILD_URL}
              note="build.sh compiles Sources/*.swift into GrindBot.app and ad-hoc signs it. Apple has not signed it: move the app somewhere macOS treats as quarantined and Gatekeeper blocks it until you right-click and choose Open once."
            />
            <BuildBlock
              id="windows"
              title="Windows"
              requirements={WINDOWS_REQUIREMENTS}
              code={WINDOWS_BUILD}
              href={WINDOWS_BUILD_URL}
              note="A rewrite in WPF: same robot, same faces, same config.json, run from the system tray. Settings live in %APPDATA%\GrindBot. PowerShell may refuse the script until you unblock it once with Unblock-File .\build.ps1."
            />
          </div>
        </section>

        {/* FAQ */}
        <section id="faq" className="mx-auto max-w-3xl scroll-mt-20 px-4 pb-24 sm:px-6">
          <div className="rule-brass" />
          <h2 className="mt-10 font-display text-3xl tracking-tight sm:text-4xl">Questions</h2>
          <div className="mt-8 divide-y divide-border border-y border-border">
            {FAQ.map((item) => (
              <details key={item.q} className="group py-4">
                <summary className="flex cursor-pointer list-none items-center justify-between gap-4 font-medium [&::-webkit-details-marker]:hidden">
                  {item.q}
                  <span aria-hidden className="shrink-0 text-accent transition group-open:rotate-45">
                    +
                  </span>
                </summary>
                <p className="mt-3 text-muted">{item.a}</p>
              </details>
            ))}
          </div>
        </section>
      </main>

      <SiteFooter />
    </>
  );
}

/** One unguarded pack: the file name as a plaque, the real line count, a short sample. */
function PackEntry({ pack }: { pack: Pack }) {
  const sample = pack.lines.slice(0, 4);
  return (
    <article className="min-w-0 border-t border-border pt-5">
      <div className="flex flex-wrap items-baseline justify-between gap-x-3 gap-y-1">
        <h3 className="plaque text-[11px] text-accent">packs/{pack.file}</h3>
        <span className="text-sm text-muted tabular-nums">{pack.lines.length} lines</span>
      </div>
      <p className="mt-2 text-sm text-muted">{pack.tone}</p>
      <ul className="mt-4 space-y-2">
        {sample.map((line) => (
          <li key={line} className="text-sm leading-snug text-muted">
            {line}
          </li>
        ))}
        {pack.lines.length > sample.length && (
          <li className="text-sm text-muted/70">+ {pack.lines.length - sample.length} more in the file</li>
        )}
      </ul>
    </article>
  );
}

function BuildBlock({
  id,
  title,
  requirements,
  code,
  href,
  note,
}: {
  id: string;
  title: string;
  requirements: string;
  code: string;
  href: string;
  note: string;
}) {
  return (
    <div id={id} className="flex min-w-0 scroll-mt-20 flex-col border-t border-border pt-5">
      <h3 className="plaque text-[11px] text-accent">{title}</h3>
      <p className="mt-3 text-sm text-muted">{requirements}</p>
      <pre className="mt-4 min-w-0 overflow-x-auto bg-frame p-4 font-mono text-[13px] leading-relaxed text-[#e9e5d3]">
        <code>{code}</code>
      </pre>
      <p className="mt-3 text-sm text-muted">{note}</p>
      <a href={href} className="mt-4 text-sm underline underline-offset-4 hover:text-text">
        Full build instructions
      </a>
    </div>
  );
}
