/** The GrindBot mark. Geometry is lifted from windows/src/Robot.cs with face = visor, and
 *  is used verbatim — antenna, ears, head, visor, eyes. Fills are the theme tokens, so it
 *  inverts with the page. Decorative: the wordmark next to it carries the name. */
export function GrindBotMark({ size = 28, className }: { size?: number; className?: string }) {
  return (
    <svg
      viewBox="0 -9 64 70"
      width={size}
      height={(size * 70) / 64}
      className={className}
      aria-hidden
      focusable="false"
    >
      <rect x="30.75" y="-3" width="2.5" height="12" rx="1.25" fill="var(--muted)" />
      <circle cx="32" cy="-4" r="4" fill="var(--accent)" />
      <rect x="3" y="31" width="5" height="14" rx="2.5" fill="var(--muted)" />
      <rect x="56" y="31" width="5" height="14" rx="2.5" fill="var(--muted)" />
      <rect
        x="5"
        y="16"
        width="54"
        height="44"
        rx="13"
        fill="var(--surface)"
        stroke="rgba(0,0,0,.10)"
        strokeWidth="1"
      />
      <rect x="10" y="23" width="44" height="30" rx="9" fill="#212121" />
      <rect x="19.5" y="32" width="8" height="12" rx="4" fill="var(--accent)" />
      <rect x="36.5" y="32" width="8" height="12" rx="4" fill="var(--accent)" />
    </svg>
  );
}
