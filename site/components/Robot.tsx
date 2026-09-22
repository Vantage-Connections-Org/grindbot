/** The robot, redrawn as SVG from Sources/RobotView.swift — same parts, same numbers:
 *  antenna, head, visor, ears, neck, torso, chest light, arms. Default colours are the
 *  shipped ones (shell #FCFCFC, visor #212121, accent #33D6A8), so it is the real robot.
 *  Decorative by default: pass a label when it carries meaning. */
export function Robot({
  size = 120,
  face = "visor",
  accent = "#33D6A8",
  shell = "#FCFCFC",
  visor = "#212121",
  bob = false,
  label,
}: {
  size?: number;
  face?: "visor" | "cyclops" | "pixel" | "angry" | "dot";
  accent?: string;
  shell?: string;
  visor?: string;
  bob?: boolean;
  label?: string;
}) {
  const id = `r-${face}-${accent.replace("#", "")}`;
  const trim = "#cfcfcf";
  const trimLight = "#d6d6d6";

  return (
    <svg
      viewBox="-34 -2 68 94"
      width={size}
      height={(size * 94) / 68}
      className={bob ? "bob" : undefined}
      role={label ? "img" : undefined}
      aria-label={label}
      aria-hidden={label ? undefined : true}
      focusable="false"
    >
      <defs>
        <linearGradient id={`${id}-shell`} x1="0" y1="0" x2="0" y2="1">
          <stop offset="0%" stopColor={shell} />
          <stop offset="100%" stopColor="#dcdcdc" />
        </linearGradient>
        <filter id={`${id}-glow`} x="-120%" y="-120%" width="340%" height="340%">
          <feGaussianBlur stdDeviation="2.4" result="b" />
          <feMerge>
            <feMergeNode in="b" />
            <feMergeNode in="SourceGraphic" />
          </feMerge>
        </filter>
      </defs>

      {/* antenna */}
      <rect x="-1.25" y="4" width="2.5" height="11" rx="1.25" fill={trim} />
      <circle cx="0" cy="2.5" r="4" fill={accent} filter={`url(#${id}-glow)`} />

      {/* ears */}
      <rect x="-29" y="31" width="5" height="14" rx="2.5" fill={trimLight} />
      <rect x="24" y="31" width="5" height="14" rx="2.5" fill={trimLight} />

      {/* head and visor */}
      <rect x="-27" y="16" width="54" height="44" rx="13" fill={`url(#${id}-shell)`} />
      <rect x="-22" y="23" width="44" height="30" rx="9" fill={visor} />
      <g filter={`url(#${id}-glow)`}>
        <Face face={face} accent={accent} />
      </g>

      {/* neck */}
      <rect x="-4" y="60" width="8" height="4" fill={trim} />

      {/* arms */}
      <rect x="-25" y="68.5" width="5" height="15" rx="2.5" fill={trim} />
      <rect x="20" y="68.5" width="5" height="15" rx="2.5" fill={trim} />

      {/* torso and chest light */}
      <rect x="-21" y="64" width="42" height="26" rx="10" fill={`url(#${id}-shell)`} />
      <circle cx="0" cy="77" r="3.5" fill={accent} opacity="0.9" />
    </svg>
  );
}

/** What sits inside the visor. One branch per case in FaceView. */
function Face({ face, accent }: { face: string; accent: string }) {
  switch (face) {
    case "cyclops":
      return (
        <>
          <circle cx="0" cy="38" r="8.5" fill={accent} />
          <circle cx="0" cy="38" r="8.5" fill="none" stroke="rgba(0,0,0,0.45)" strokeWidth="2.5" />
          <circle cx="0" cy="38" r="2.5" fill="rgba(0,0,0,0.55)" />
        </>
      );
    case "pixel":
      return (
        <>
          <rect x="-10.5" y="30" width="7" height="7" fill={accent} />
          <rect x="3.5" y="30" width="7" height="7" fill={accent} />
          <rect x="-5" y="42" width="4" height="3" fill={accent} opacity="0.75" />
          <rect x="-2" y="42" width="4" height="3" fill={accent} opacity="0.75" />
          <rect x="1" y="42" width="4" height="3" fill={accent} opacity="0.75" />
        </>
      );
    case "angry":
      return (
        <>
          <rect x="-12.5" y="36" width="8" height="9" rx="4" fill={accent} />
          <rect x="4.5" y="36" width="8" height="9" rx="4" fill={accent} />
          <rect x="-13" y="30" width="10" height="3" rx="1.5" fill={accent} transform="rotate(18 -8 31.5)" />
          <rect x="3" y="30" width="10" height="3" rx="1.5" fill={accent} transform="rotate(-18 8 31.5)" />
        </>
      );
    case "dot":
      return (
        <>
          <circle cx="-7" cy="35" r="3.5" fill={accent} />
          <circle cx="7" cy="35" r="3.5" fill={accent} />
          <path d="M -7 43 Q 0 48 7 43" fill="none" stroke={accent} strokeWidth="2.5" strokeLinecap="round" />
        </>
      );
    default:
      return (
        <>
          <rect x="-12.5" y="32" width="8" height="12" rx="4" fill={accent} />
          <rect x="4.5" y="32" width="8" height="12" rx="4" fill={accent} />
        </>
      );
  }
}
