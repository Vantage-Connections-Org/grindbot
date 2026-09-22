import { Robot } from "@/components/Robot";
import { HERO_VIDEO } from "@/lib/site";

/** The hero slot. Once the ~6s demo loop exists, set HERO_VIDEO in lib/site.ts to the two
 *  file names and the <video> renders here. Until then this draws the robot the way it sits
 *  on a screen corner — an illustration, not a screenshot, and not a poster frame standing
 *  in for footage that has not been captured. Nothing here breaks when the video lands. */
export function HeroMedia({ line }: { line: string }) {
  return (
    <div className="relative overflow-hidden rounded-2xl border border-border bg-[#1b1c1c] p-4 shadow-[0_24px_60px_-28px_rgb(22_24_24/0.5)] sm:p-6">
      {HERO_VIDEO ? (
        <video
          autoPlay
          muted
          loop
          playsInline
          aria-label={HERO_VIDEO.alt}
          className="h-auto w-full rounded-lg"
        >
          <source src={HERO_VIDEO.webm} type="video/webm" />
          <source src={HERO_VIDEO.mp4} type="video/mp4" />
        </video>
      ) : (
        <>
          <div className="flex gap-1.5">
            <span className="size-2.5 rounded-full bg-white/15" />
            <span className="size-2.5 rounded-full bg-white/15" />
            <span className="size-2.5 rounded-full bg-white/15" />
          </div>
          <div className="mt-4 h-24 rounded-lg bg-white/[0.04] sm:h-32" />
          <div className="mt-3 flex items-end justify-end gap-2 sm:gap-3">
            <p className="max-w-[18ch] rounded-2xl rounded-br-sm bg-white/10 px-3 py-2 text-sm leading-snug text-white sm:max-w-[22ch] sm:text-base">
              {line}
            </p>
            <Robot size={72} bob label={`The GrindBot robot, saying: ${line}`} />
          </div>
        </>
      )}
    </div>
  );
}
