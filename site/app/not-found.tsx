import Link from "next/link";
import { GrindBotMark } from "@/components/Mark";
import { SiteFooter, SiteHeader } from "@/components/SiteChrome";
import { REPO_URL } from "@/lib/site";

export default function NotFound() {
  return (
    <>
      <SiteHeader />
      <main className="paper-grain">
        <div className="mx-auto flex max-w-xl flex-col items-center px-4 py-24 text-center sm:px-6">
          <GrindBotMark size={88} />
          <h1 className="mt-8 font-display text-4xl tracking-tight sm:text-5xl">Nothing here.</h1>
          <p className="plaque mt-4 text-[11px] text-accent">The link is old, or mistyped</p>
          <div className="rule-brass mt-4 w-full max-w-xs" />
          <div className="mt-8 flex flex-wrap justify-center gap-3">
            <Link
              href="/"
              className="rounded-sm bg-accent px-6 py-3 font-medium text-accent-ink transition hover:bg-accent-hover"
            >
              Go to the home page
            </Link>
            <a
              href={REPO_URL}
              className="rounded-sm border border-brass/60 px-6 py-3 font-medium transition hover:border-brass hover:bg-accent-soft"
            >
              View the source
            </a>
          </div>
        </div>
      </main>
      <SiteFooter />
    </>
  );
}
