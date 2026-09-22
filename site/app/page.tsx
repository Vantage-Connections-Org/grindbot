import { Reveal } from "@/components/Reveal";
import { Robot } from "@/components/Robot";
import { GithubMark, SiteFooter, SiteHeader } from "@/components/SiteChrome";
import { getPacks, totalMessages, type Pack } from "@/lib/packs";
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

// One line the robot is actually shipped with, from packs/hustle.txt. The hero never
// quotes classic or obituary.
const HERO_LINE = "Talk is cheap. Ship something.";

const WHAT_IT_DOES = [
  {
    title: "Clicks go straight through it",
    body: "The overlay window sets ignoresMouseEvents on macOS and WS_EX_TRANSPARENT on Windows, so you can click, drag and type as if it were not there. It never takes focus mid-take.",
  },
  {
    title: "No dock icon, no taskbar entry",
    body: "macOS runs it as an accessory app (LSUIElement), Windows keeps it out of the taskbar. It lives in the menu bar or the system tray, so an uncluttered recording stays uncluttered.",
  },
  {
    title: "Every message is a text file",
    body: "One line per message, # for comments. Edit the file, hit Reload config & messages, and the next popup is your line. No rebuild, no format, no database.",
  },
];

export default async function Home() {
  const packs = getPacks();
  const stars = await githubStars();
  const total = totalMessages(packs);

  return (
    <>
      <SiteHeader />

      <main>
        {/* Hero. No fade-in: this is the LCP element and must paint on the first render. */}
        <section className="mx-auto grid max-w-5xl items-center gap-12 px-4 pb-16 pt-12 sm:px-6 md:pt-20 lg:grid-cols-[1.05fr_1fr]">
          <div>
            <h1 className="text-4xl font-semibold leading-[1.05] tracking-tighter sm:text-5xl lg:text-6xl">{TAGLINE}</h1>
            <p className="mt-5 max-w-[48ch] text-lg leading-relaxed text-muted">
              {NAME} is a tiny robot that pops up in the corner of your screen, types one line at you, and disappears —
              built for the people who spend all day recording that screen.
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
              MIT licensed. macOS 13+ and Windows 10/11. No packaged release yet — you build it from source, which is
              one command on either platform.
            </p>
          </div>

          {/* A screen corner, the way the robot actually sits on one. */}
          <div className="relative overflow-hidden rounded-2xl border border-border bg-[#1b1c1c] p-4 shadow-[0_24px_60px_-28px_rgb(22_24_24/0.5)] sm:p-6">
            <div className="flex gap-1.5">
              <span className="size-2.5 rounded-full bg-white/15" />
              <span className="size-2.5 rounded-full bg-white/15" />
              <span className="size-2.5 rounded-full bg-white/15" />
            </div>
            <div className="mt-4 h-24 rounded-lg bg-white/[0.04] sm:h-32" />
            <div className="mt-3 flex items-end justify-end gap-2 sm:gap-3">
              <p className="max-w-[16ch] rounded-2xl rounded-br-sm bg-white/10 px-3 py-2 text-sm leading-snug text-white sm:max-w-[20ch] sm:text-base">
                {HERO_LINE}
              </p>
              <Robot size={72} bob label={`The ${NAME} robot, saying: ${HERO_LINE}`} />
            </div>
          </div>
        </section>

        {/* What it does */}
        <section className="border-y border-border bg-surface">
          <div className="mx-auto max-w-5xl px-4 py-16 sm:px-6">
            <Reveal>
              <h2 className="max-w-[22ch] text-3xl font-semibold tracking-tight md:text-4xl">
                It sits on top of your screen and stays out of the way.
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

        {/* Packs */}
        <section id="packs" className="mx-auto max-w-5xl scroll-mt-20 px-4 py-20 sm:px-6">
          <Reveal>
            <h2 className="max-w-[24ch] text-3xl font-semibold tracking-tight md:text-4xl">
              Five packs ship with it. {total} lines in total.
            </h2>
            <p className="mt-3 max-w-[62ch] text-muted">
              Each pack is a text file in{" "}
              <a href={PACKS_URL} className="underline underline-offset-4 hover:text-text">
                packs/
              </a>
              . Point <code className="font-mono text-sm">messagesFile</code> at one, or write your own and keep as many
              as you like. Two of them are folded away below — they read badly without the joke around them.
            </p>
          </Reveal>
          <div className="mt-10 grid gap-4 md:grid-cols-2">
            {packs.map((pack, i) => (
              <Reveal key={pack.slug} delay={i * 0.05} className={pack.guarded ? "md:col-span-1" : undefined}>
                <PackCard pack={pack} />
              </Reveal>
            ))}
          </div>
        </section>

        {/* Faces */}
        <section className="border-y border-border bg-surface">
          <div className="mx-auto max-w-5xl px-4 py-16 sm:px-6">
            <Reveal>
              <h2 className="text-3xl font-semibold tracking-tight md:text-4xl">Five faces, and the colours are yours.</h2>
              <p className="mt-3 max-w-[60ch] text-muted">
                Pick one in the settings window, from the Face menu, or with the <code className="font-mono text-sm">face</code>{" "}
                key. Colours are separate: <code className="font-mono text-sm">accent</code> drives the eyes, antenna and
                chest light, <code className="font-mono text-sm">shell</code> the body,{" "}
                <code className="font-mono text-sm">visor</code> the panel behind the eyes.
              </p>
            </Reveal>
            <ul className="mt-10 grid grid-cols-2 gap-4 sm:grid-cols-3 md:grid-cols-5">
              {FACES.map((face, i) => (
                <Reveal key={face.name} delay={i * 0.05}>
                  <li className="flex h-full flex-col items-center rounded-2xl border border-border bg-[#1b1c1c] p-4 text-center">
                    <Robot size={62} face={face.name} />
                    <h3 className="mt-3 font-mono text-sm text-white">{face.name}</h3>
                    <p className="mt-1 text-xs leading-snug text-white/60">{face.note}</p>
                  </li>
                </Reveal>
              ))}
            </ul>
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
              rather than crashes. The settings window writes the same file, and changes apply live. Defaults below are
              the ones in <code className="font-mono text-sm">config.default.json</code>.
            </p>
          </Reveal>
          <Reveal delay={0.06}>
            {/* Scrolls horizontally rather than forcing the page to, on a narrow phone. */}
            <div className="mt-8 overflow-x-auto rounded-2xl border border-border bg-surface">
              <table className="w-full min-w-[34rem] text-left text-sm">
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
                    <tr key={row.key} className="border-b border-border last:border-0 align-top">
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
            <p className="mt-4 text-sm text-muted">
              There are more: dwell timing, <code className="font-mono">margin</code>,{" "}
              <code className="font-mono">maxBubbleWidth</code>, and a few Windows-only keys like{" "}
              <code className="font-mono">theme</code> and <code className="font-mono">idleOnly</code>. The full table is
              in the{" "}
              <a href={`${REPO_URL}#configuration`} className="underline underline-offset-4 hover:text-text">
                README
              </a>
              ; adding a face or changing the drawing is in{" "}
              <a href={CUSTOMIZING_URL} className="underline underline-offset-4 hover:text-text">
                CUSTOMIZING.md
              </a>
              .
            </p>
          </Reveal>
        </section>

        {/* Install */}
        <section className="border-y border-border bg-surface">
          <div className="mx-auto max-w-5xl px-4 py-16 sm:px-6">
            <Reveal>
              <h2 className="text-3xl font-semibold tracking-tight md:text-4xl">Build it.</h2>
              <p className="mt-3 max-w-[60ch] text-muted">
                There is no release to download yet. Both platforms build from the same repo in one command.
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
                  note="build.sh compiles Sources/*.swift into GrindBot.app and ad-hoc signs it. It appears in the menu bar under the robot icon."
                />
              </Reveal>
              <Reveal delay={0.06}>
                <BuildCard
                  id="windows"
                  title="Windows"
                  requirements={WINDOWS_REQUIREMENTS}
                  code={WINDOWS_BUILD}
                  href={WINDOWS_BUILD_URL}
                  note="A full rewrite in WPF: same robot, same faces, same config.json, driven from the system tray. Settings live in %APPDATA%\GrindBot."
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
            Show the lines
          </summary>
          <p className="mt-3 text-xs text-muted">
            {pack.slug === "obituary"
              ? "Deliberately bleak. It is a joke about mortality, and it does not read like one out of context."
              : "The five originals the app shipped with. They are funnier in the corner of a screen than in a list."}
          </p>
          <ul className="mt-3 space-y-2">
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
