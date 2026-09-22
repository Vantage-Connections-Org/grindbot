import { ImageResponse } from "next/og";
import { NAME, TAGLINE } from "@/lib/site";

export const size = { width: 1200, height: 630 };
export const contentType = "image/png";
export const alt = `${NAME} — ${TAGLINE}`;

// The card is the gallery wall, not a poster: cream stock, a brass hairline, the mark and
// the wordmark. Deliberately no message lines — the packs are funny in a screen corner and
// grim in a social card, and the two guarded packs must never leave the page at all.

const CREAM = "#f3efe6";
const INK = "#171513";
const MUTED = "#6b635a";
const BRASS = "#a67c34";
const FRAME = "#15130f";

/** Instrument Serif, the page's display face, fetched as TTF so satori can use it. Returns
 *  null if Google Fonts is unreachable at build time: the card then renders in the default
 *  face rather than failing the build. */
async function displayFont(): Promise<ArrayBuffer | null> {
  try {
    const css = await fetch("https://fonts.googleapis.com/css2?family=Instrument+Serif").then((r) => r.text());
    const url = css.match(/src:\s*url\((https:[^)]+)\)\s*format\('truetype'\)/)?.[1];
    if (!url) return null;
    return await fetch(url).then((r) => r.arrayBuffer());
  } catch {
    return null;
  }
}

/** The mark, in the same rectangles as app/icon.svg. Satori has no <svg> element support
 *  for arbitrary markup, so it is rebuilt out of divs. */
function Mark() {
  const ear = { width: 11, height: 26, borderRadius: 6, background: "#f2e7cf" };
  const eye = { width: 9, height: 19, borderRadius: 5, background: "#f0a91e" };
  return (
    <div
      style={{
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        width: 120,
        height: 120,
        borderRadius: 26,
        background: FRAME,
        border: `7px solid ${BRASS}`,
      }}
    >
      <div style={{ display: "flex", alignItems: "center" }}>
        <div style={ear} />
        <div
          style={{
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            gap: 11,
            width: 71,
            height: 62,
            marginLeft: -3,
            marginRight: -3,
            borderRadius: 19,
            background: "#f7eeda",
          }}
        >
          <div
            style={{
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              gap: 11,
              width: 53,
              height: 34,
              borderRadius: 13,
              background: FRAME,
            }}
          >
            <div style={eye} />
            <div style={eye} />
          </div>
        </div>
        <div style={ear} />
      </div>
    </div>
  );
}

export default async function OpengraphImage() {
  const font = await displayFont();
  return new ImageResponse(
    (
      <div
        style={{
          width: "100%",
          height: "100%",
          display: "flex",
          flexDirection: "column",
          justifyContent: "space-between",
          background: CREAM,
          color: INK,
          padding: 80,
        }}
      >
        <Mark />
        <div style={{ display: "flex", flexDirection: "column" }}>
          <div
            style={{
              display: "flex",
              fontFamily: font ? "Instrument Serif" : undefined,
              fontSize: 132,
              letterSpacing: -2,
              lineHeight: 1,
            }}
          >
            {NAME}
          </div>
          <div style={{ display: "flex", height: 2, background: BRASS, marginTop: 34, marginBottom: 30 }} />
          <div
            style={{
              display: "flex",
              fontFamily: font ? "Instrument Serif" : undefined,
              fontSize: 40,
              letterSpacing: 8,
              textTransform: "uppercase",
              color: BRASS,
            }}
          >
            {TAGLINE}
          </div>
          <div style={{ display: "flex", fontSize: 30, color: MUTED, marginTop: 26 }}>
            Click-through desktop toy for macOS and Windows. Open source, MIT.
          </div>
        </div>
      </div>
    ),
    {
      ...size,
      fonts: font ? [{ name: "Instrument Serif", data: font, style: "normal", weight: 400 }] : undefined,
    },
  );
}
