import Image from "next/image";
import { HeroMedia } from "@/components/HeroMedia";
import { Reveal } from "@/components/Reveal";
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
  PACKS_URL,
  REPO_URL,
  TAGLINE,
  WINDOWS_BUILD,
  WINDOWS_BUILD_URL,
  WINDOWS_REQUIREMENTS,
  githubStars,
} from "@/lib/site";

const WHAT_IT_DOES = [
  {
    title: "Clicks go through it",
    body: "The window sets ignoresMouseEvents on macOS and WS_EX_TRANSPARENT on Windows. It never receives a click, so it cannot swallow one meant for your editor.",
  },
  {
    title: "No dock icon, no taskbar entry",
    body: "LSUIElement on macOS, ShowInTaskbar false on Windows. It runs from the menu bar or the system tray, and nothing new shows up in ⌘-Tab or Alt-Tab.",
  },
  {
    title: "Every message is a text file",
    body: "One line per message, # for comments. Edit the file, hit Reload config & messages, and the next popup is your line. No rebuild.",
  },
];

export default async function Home() {
  const packs = getPacks();
  const stars = await githubStars();
  const total = totalMessages(packs);

  // Both quotes are pulled from packs/habits.txt at build time and the build fails if the
  // lines change. Never classic, never obituary.
  const heroLine = quote(packs, "habits", "Momentum is a currency");
  const pullQuote = quote(packs, "habits", "Skill decays");

  return (
    <>
      <SiteHeader />

      <main>
        {/* Hero. No fade-in: this is the LCP element and must paint on the first render. */}
        <section className="mx-auto grid max-w-5xl items-center gap-12 px-4 pb-16 pt-12 sm:px-6 md:pt-20 lg:grid-cols-[1.05fr_1fr]">
          <div>
            <h1 className="text-4xl font-semibold leading-[1.05] tracking-tighter sm:text-5xl lg:text-6xl">{TAGLINE}</h1>
            <p className="mt-5 max-w-[48ch] text-lg leading-relaxed text-muted">
              A robot appears in the corner of your screen, types one line, and disappears. Every 20 seconds out of the
              box; the slider runs from 5 seconds to 5 hours. Clicks pass straight through it.
            </p>
            <div className="mt-8 flex flex-wrap gap-3">
              <a
                href="#mac"
                className="inline-flex items-center gap-2 rounded-xl bg-accent px-5 py-3 font-medium text-accent-ink transition hover:bg-accent-hover active:scale-[0.98]"
              >
                Build it on macOS
              </a>
              <a
                href="#windows"
                className="inline-flex items-center gap-2 rounded-xl border border-border bg-surface px-5 py-3 font-medium transition hover:border-muted active:scale-[0.98]"
              >
                Build it on Windows
              </a>
            </div>
            <a
              href={REPO_URL}
              className="mt-5 inline-flex items-center gap-2 text-sm text-muted transition hover:text-text"
            >
              <GithubMark size={16} />
              <span className="underline underline-offset-4">Source on GitHub</span>
              {stars !== null && <span className="tabular-nums">· {stars}★</span>}
            </a>
            <p className="mt-6 text-sm text-muted">
              No release to download yet, so you build it. MIT. macOS 13 or newer, Windows 10 or 11.
            </p>
          </div>

          <HeroMedia line={heroLine} />
        </section>

        {/* What it does */}
        <section className="border-y border-border bg-surface">
          <div className="mx-auto max-w-5xl px-4 py-16 sm:px-6">
            <Reveal>
              <h2 className="max-w-[22ch] text-3xl font-semibold tracking-tight md:text-4xl">
                It draws over your screen and touches nothing else.
              </h2>
            </Reveal>
            <div className="mt-10 grid gap-6 md:grid-cols-3">
              {WHAT_IT_DOES.map((item, i) => (
                <Reveal key={item.title} delay={i * 0.06}>
                  <div className="h-full rounded-2xl border border-border bg-bg p-6">
                    <h3 className="font-semibold">{item.title}</h3>
                    <p className="mt-2 text-sm leading-relaxed text-muted">{item.body}</p>
                  </div>
                </Reveal>
              ))}
            </div>
          </div>
        </section>

        {/* One line, on its own. */}
        <section className="mx-auto max-w-5xl px-4 py-20 sm:px-6">
          <Reveal>
            <figure>
              <blockquote className="max-w-[20ch] text-3xl font-medium leading-tight tracking-tight sm:text-4xl md:text-5xl">
                {pullQuote}
              </blockquote>
              <figcaption className="mt-6 font-mono text-sm text-muted">packs/habits.txt</figcaption>
            </figure>
          </Reveal>
        </section>

        {/* Packs */}
        <section id="packs" className="mx-auto max-w-5xl scroll-mt-20 border-t border-border px-4 py-20 sm:px-6">
          <Reveal>
            <h2 className="max-w-[24ch] text-3xl font-semibold tracking-tight md:text-4xl">
              Five packs, {total} lines.
            </h2>
            <p className="mt-3 max-w-[62ch] text-muted">
              Each one is a text file in{" "}
              <a href={PACKS_URL} className="underline underline-offset-4 hover:text-text">
                packs/
              </a>
              . Point <code className="font-mono text-sm">messagesFile</code> at one, or write your own and keep as many
              as you like. The last two are folded shut. Classic is the original joke and its best line does not survive
              a list; obituary is about dying.
            </p>
          </Reveal>
          <div className="mt-10 grid gap-4 md:grid-cols-2">
            {packs.map((pack, i) => (
              <Reveal key={pack.slug} delay={i * 0.05}>
                <PackCard pack={pack} />
              </Reveal>
            ))}
          </div>
        </section>

        {/* Faces */}
        <section className="border-y border-border bg-surface">
          <div className="mx-auto max-w-5xl px-4 py-16 sm:px-6">
            <Reveal>
              <h2 className="text-3xl font-semibold tracking-tight md:text-4xl">Five faces. Three colours.</h2>
              <p className="mt-3 max-w-[62ch] text-muted">
                Set <code className="font-mono text-sm">face</code> to one of{" "}
                {FACES.map((f) => f.name).join(", ")}. Colour is separate:{" "}
                <code className="font-mono text-sm">accent</code> drives the eyes, antenna and chest light,{" "}
                <code className="font-mono text-sm">shell</code> the body,{" "}
                <code className="font-mono text-sm">visor</code> the panel behind the eyes. A dark shell with a bright
                accent is a different robot.
              </p>
            </Reveal>
            <Reveal delay={0.06}>
              <div className="mt-8 overflow-hidden rounded-2xl border border-border bg-[#1b1c1c]">
                <Image
                  src="/faces.png"
                  alt="The five robot faces side by side: visor, cyclops, pixel, angry and dot"
                  width={1600}
                  height={421}
                  className="h-auto w-full"
                />
              </div>
              <ul className="mt-4 grid grid-cols-2 gap-x-6 gap-y-2 text-sm text-muted sm:grid-cols-5">
                {FACES.map((face) => (
                  <li key={face.name}>
                    <span className="font-mono text-accent">{face.name}</span> — {face.note}
                  </li>
                ))}
              </ul>
            </Reveal>
          </div>
        </section>

        {/* Config */}
        <section id="config" className="mx-auto max-w-5xl scroll-mt-20 px-4 py-20 sm:px-6">
          <Reveal>
            <h2 className="max-w-[24ch] text-3xl font-semibold tracking-tight md:text-4xl">
              One <code className="font-mono">config.json</code>, every knob.
            </h2>
            <p className="mt-3 max-w-[62ch] text-muted">
              Every key is optional. Anything missing or malformed falls back to its default, so a broken file degrades
              rather than crashes. The settings window writes the same file and applies changes as you drag. Defaults
              below are the ones in <code className="font-mono text-sm">config.default.json</code>.
            </p>
          </Reveal>
          <div className="mt-8 grid gap-6 lg:grid-cols-[minmax(0,2fr)_minmax(0,1fr)]">
            <Reveal delay={0.06}>
              {/* Scrolls sideways on its own rather than pushing the page wider. */}
              <div className="overflow-x-auto rounded-2xl border border-border bg-surface">
                <table className="w-full min-w-[32rem] text-left text-sm">
                  <thead className="border-b border-border text-muted">
                    <tr>
                      <th scope="col" className="px-4 py-3 font-medium">
                        Key
                      </th>
                      <th scope="col" className="px-4 py-3 font-medium">
                        Default
                      </th>
                      <th scope="col" className="px-4 py-3 font-medium">
                        What it does
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {CONFIG_KEYS.map((row) => (
                      <tr key={row.key} className="border-b border-border align-top last:border-0">
                        <td className="px-4 py-3 font-mono text-[13px] whitespace-nowrap text-accent">{row.key}</td>
                        <td className="px-4 py-3 font-mono text-[13px] whitespace-nowrap text-muted">{row.value}</td>
                        <td className="px-4 py-3 text-muted">{row.note}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </Reveal>
            <Reveal delay={0.1}>
              <figure className="rounded-2xl border border-border bg-surface p-4">
                <Image
                  src="/settings.png"
                  alt="The GrindBot settings window on macOS: timing, size and position, colours, face and message file, with a live robot preview at the top"
                  width={594}
                  height={1100}
                  className="h-auto w-full rounded-lg"
                />
                <figcaption className="mt-3 text-sm text-muted">
                  The same keys, as a window. There is no OK and no Cancel — what you see is running, and it saves
                  itself.
                </figcaption>
              </figure>
            </Reveal>
          </div>
          <Reveal delay={0.14}>
            <p className="mt-6 max-w-[62ch] text-sm text-muted">
              There are more: dwell timing, <code className="font-mono">margin</code>,{" "}
              <code className="font-mono">maxBubbleWidth</code>, and Windows-only keys like{" "}
              <code className="font-mono">theme</code>, <code className="font-mono">idleOnly</code> and{" "}
              <code className="font-mono">allMessageFiles</code>. The full table is in the{" "}
              <a href={`${REPO_URL}#configuration`} className="underline underline-offset-4 hover:text-text">
                README
              </a>
              . Adding a face or changing the drawing is in{" "}
              <a href={CUSTOMIZING_URL} className="underline underline-offset-4 hover:text-text">
                CUSTOMIZING.md
              </a>
              .
            </p>
          </Reveal>
        </section>

        {/* Build */}
        <section className="border-y border-border bg-surface">
          <div className="mx-auto max-w-5xl px-4 py-16 sm:px-6">
            <Reveal>
              <h2 className="text-3xl font-semibold tracking-tight md:text-4xl">Build it.</h2>
              <p className="mt-3 max-w-[60ch] text-muted">
                There is nothing to download. Both platforms build from the same repo, in one command.
              </p>
            </Reveal>
            <div className="mt-8 grid gap-4 md:grid-cols-2">
              <Reveal>
                <BuildCard
                  id="mac"
                  title="macOS"
                  requirements={MAC_REQUIREMENTS}
                  code={MAC_BUILD}
                  href={MAC_BUILD_URL}
                  note="build.sh compiles Sources/*.swift into GrindBot.app and ad-hoc signs it. Apple has not signed it: move the app somewhere quarantined and Gatekeeper blocks it until you right-click and choose Open once."
                />
              </Reveal>
              <Reveal delay={0.06}>
                <BuildCard
                  id="windows"
                  title="Windows"
                  requirements={WINDOWS_REQUIREMENTS}
                  code={WINDOWS_BUILD}
                  href={WINDOWS_BUILD_URL}
                  note="A rewrite in WPF: same robot, same faces, same config.json, run from the system tray. Settings live in %APPDATA%\GrindBot. PowerShell may refuse the script until you unblock it once with Unblock-File .\build.ps1."
                />
              </Reveal>
            </div>
          </div>
        </section>

        {/* FAQ */}
        <section id="faq" className="mx-auto max-w-3xl scroll-mt-20 px-4 py-20 sm:px-6">
          <Reveal>
            <h2 className="text-3xl font-semibold tracking-tight md:text-4xl">Questions</h2>
          </Reveal>
          <div className="mt-8 divide-y divide-border border-y border-border">
            {FAQ.map((item, i) => (
              <Reveal key={item.q} delay={i * 0.04}>
                <details className="group py-4">
                  <summary className="flex cursor-pointer list-none items-center justify-between gap-4 font-medium [&::-webkit-details-marker]:hidden">
                    {item.q}
                    <span aria-hidden className="text-muted transition group-open:rotate-45">
                      +
                    </span>
                  </summary>
                  <p className="mt-3 text-muted">{item.a}</p>
                </details>
              </Reveal>
            ))}
          </div>
        </section>
      </main>

      <SiteFooter />
    </>
  );
}

/** One pack. Guarded packs keep every line behind a toggle; the rest show a short sample. */
function PackCard({ pack }: { pack: Pack }) {
  const sample = pack.lines.slice(0, 4);
  return (
    <article className="flex h-full flex-col rounded-2xl border border-border bg-surface p-5">
      <div className="flex flex-wrap items-baseline justify-between gap-x-3 gap-y-1">
        <h3 className="font-mono text-sm text-accent">packs/{pack.file}</h3>
        <span className="text-sm text-muted tabular-nums">{pack.lines.length} lines</span>
      </div>
      <p className="mt-1 text-sm text-muted">{pack.tone}</p>
      {pack.guarded ? (
        <details className="group mt-4">
          <summary className="inline-flex cursor-pointer list-none items-center gap-2 rounded-lg border border-border px-3 py-2 text-sm font-medium transition hover:border-muted [&::-webkit-details-marker]:hidden">
            <span aria-hidden className="text-muted transition group-open:rotate-45">
              +
            </span>
            Show all {pack.lines.length} lines
          </summary>
          <ul className="mt-4 space-y-2">
            {pack.lines.map((line) => (
              <li key={line} className="text-sm leading-snug text-muted">
                {line}
              </li>
            ))}
          </ul>
        </details>
      ) : (
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
      )}
    </article>
  );
}

function BuildCard({
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
    <div id={id} className="flex h-full scroll-mt-20 flex-col rounded-2xl border border-border bg-bg p-5">
      <h3 className="text-lg font-semibold">{title}</h3>
      <p className="mt-2 text-sm text-muted">{requirements}</p>
      <pre className="mt-4 overflow-x-auto rounded-xl bg-[#1b1c1c] p-4 font-mono text-[13px] leading-relaxed text-white/90">
        <code>{code}</code>
      </pre>
      <p className="mt-3 text-sm text-muted">{note}</p>
      <a href={href} className="mt-4 text-sm underline underline-offset-4 hover:text-text">
        Full build instructions
      </a>
    </div>
  );
}
