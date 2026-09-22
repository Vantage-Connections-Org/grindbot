import Link from "next/link";
import { Robot } from "@/components/Robot";
import { SiteFooter, SiteHeader } from "@/components/SiteChrome";
import { REPO_URL } from "@/lib/site";

export default function NotFound() {
  return (
    <>
      <SiteHeader />
      <main className="mx-auto flex max-w-xl flex-col items-center px-4 py-24 text-center sm:px-6">
        <span className="grid size-28 place-items-center rounded-2xl bg-[#1b1c1c]">
          <Robot size={72} face="cyclops" />
        </span>
        <h1 className="mt-6 text-3xl font-semibold tracking-tight md:text-4xl">Nothing here.</h1>
        <p className="mt-3 text-muted">The link is old or mistyped. The robot has no opinion about it.</p>
        <div className="mt-8 flex flex-wrap justify-center gap-3">
          <Link
            href="/"
            className="rounded-xl bg-accent px-5 py-3 font-medium text-accent-ink transition hover:bg-accent-hover active:scale-[0.98]"
          >
            Go to the home page
          </Link>
          <a
            href={REPO_URL}
            className="rounded-xl border border-border bg-surface px-5 py-3 font-medium transition hover:border-muted active:scale-[0.98]"
          >
            View the source
          </a>
        </div>
      </main>
      <SiteFooter />
    </>
  );
}
