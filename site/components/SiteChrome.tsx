import Link from "next/link";
import { LICENSE_URL, ISSUES_URL, NAME, REPO_URL, githubStars } from "@/lib/site";
import { GrindBotMark } from "@/components/Mark";

function GithubMark({ size = 18 }: { size?: number }) {
  return (
    <svg viewBox="0 0 16 16" width={size} height={size} fill="currentColor" aria-hidden focusable="false">
      <path d="M8 0C3.58 0 0 3.58 0 8c0 3.54 2.29 6.53 5.47 7.59.4.07.55-.17.55-.38 0-.19-.01-.82-.01-1.49-2.01.37-2.53-.49-2.69-.94-.09-.23-.48-.94-.82-1.13-.28-.15-.68-.52-.01-.53.63-.01 1.08.58 1.23.82.72 1.21 1.87.87 2.33.66.07-.52.28-.87.51-1.07-1.78-.2-3.64-.89-3.64-3.95 0-.87.31-1.59.82-2.15-.08-.2-.36-1.02.08-2.12 0 0 .67-.21 2.2.82a7.4 7.4 0 0 1 2-.27c.68 0 1.36.09 2 .27 1.53-1.04 2.2-.82 2.2-.82.44 1.1.16 1.92.08 2.12.51.56.82 1.27.82 2.15 0 3.07-1.87 3.75-3.65 3.95.29.25.54.73.54 1.48 0 1.07-.01 1.93-.01 2.2 0 .21.15.46.55.38A8.01 8.01 0 0 0 16 8c0-4.42-3.58-8-8-8Z" />
    </svg>
  );
}

/** The gallery entrance: a plate on the wall, not a floating app bar. Server component —
 *  the star count is fetched at render and cached for an hour. */
export async function SiteHeader() {
  const stars = await githubStars();
  return (
    <header className="sticky top-0 z-20 border-b border-border bg-bg/90 backdrop-blur">
      <nav className="mx-auto flex h-16 max-w-5xl items-center justify-between gap-4 px-4 sm:px-6">
        <Link href="/" className="flex min-w-0 items-center gap-2.5">
          <GrindBotMark size={26} className="shrink-0" />
          <span className="font-display text-xl leading-none">{NAME}</span>
        </Link>
        <div className="flex items-center gap-1 text-sm">
          <Link href="/#gallery" className="hidden px-3 py-2 text-muted transition hover:text-text sm:block">
            Gallery
          </Link>
          <Link href="/#packs" className="hidden px-3 py-2 text-muted transition hover:text-text sm:block">
            Packs
          </Link>
          <Link href="/#config" className="hidden px-3 py-2 text-muted transition hover:text-text md:block">
            Config
          </Link>
          <Link href="/#build" className="hidden px-3 py-2 text-muted transition hover:text-text md:block">
            Build
          </Link>
          <a
            href={REPO_URL}
            className="ml-1 inline-flex items-center gap-2 rounded-sm border border-brass/50 px-3 py-2 transition hover:border-brass hover:bg-accent-soft"
          >
            <GithubMark />
            <span className="hidden sm:inline">GitHub</span>
            {stars !== null && <span className="text-accent tabular-nums">{stars}★</span>}
          </a>
        </div>
      </nav>
    </header>
  );
}

export function SiteFooter() {
  return (
    <footer className="border-t border-border">
      <div className="mx-auto flex max-w-5xl flex-col gap-4 px-4 py-10 text-sm text-muted sm:flex-row sm:items-center sm:justify-between sm:px-6">
        <p className="max-w-[58ch]">
          {NAME} is free and{" "}
          <a href={LICENSE_URL} className="underline underline-offset-4 hover:text-text">
            MIT licensed
          </a>
          . It is a toy. Take its advice at your own risk.
        </p>
        <div className="flex flex-wrap gap-5">
          <a href={REPO_URL} className="hover:text-text">
            GitHub
          </a>
          <a href={ISSUES_URL} className="hover:text-text">
            Report an issue
          </a>
        </div>
      </div>
    </footer>
  );
}

export { GithubMark };
