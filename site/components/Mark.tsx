/** The GrindBot mark: the robot's head, cream on a dark plaque inside a brass border.
 *  Geometry is kept in step with app/icon.svg — same rects, same numbers — so the favicon
 *  and the wordmark lockup are one object. Fixed colours, not theme tokens: this is a dark
 *  object hung on a cream page, in both colour schemes. Decorative; the wordmark beside it
 *  carries the name. */
export function GrindBotMark({ size = 28, className }: { size?: number; className?: string }) {
  return (
    <svg viewBox="0 0 64 64" width={size} height={size} className={className} aria-hidden focusable="false">
      <rect width="64" height="64" rx="14" fill="#15130f" />
      <rect x="2.5" y="2.5" width="59" height="59" rx="12" fill="none" stroke="#c89b4a" strokeWidth="3.5" />
      <rect x="9" y="27" width="6" height="14" rx="3" fill="#f2e7cf" />
      <rect x="49" y="27" width="6" height="14" rx="3" fill="#f2e7cf" />
      <rect x="13" y="16" width="38" height="33" rx="10" fill="#f7eeda" />
      <rect x="18" y="23" width="28" height="18" rx="7" fill="#15130f" />
      <rect x="24" y="27" width="5" height="10" rx="2.5" fill="#f0a91e" />
      <rect x="35" y="27" width="5" height="10" rx="2.5" fill="#f0a91e" />
    </svg>
  );
}
