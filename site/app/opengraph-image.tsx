import { ImageResponse } from "next/og";
import { NAME, TAGLINE } from "@/lib/site";

export const size = { width: 1200, height: 630 };
export const contentType = "image/png";
export const alt = `${NAME} — ${TAGLINE}`;

// Deliberately no message lines here: the packs are funny in a screen corner and grim in
// a social card. Tagline only.
export default function OpengraphImage() {
  return new ImageResponse(
    (
      <div
        style={{
          width: "100%",
          height: "100%",
          display: "flex",
          flexDirection: "column",
          justifyContent: "space-between",
          background: "#141414",
          color: "#ECEEED",
          padding: 80,
        }}
      >
        <div style={{ display: "flex", alignItems: "center", gap: 20, fontSize: 34, color: "#9BA3A0" }}>
          <div
            style={{
              display: "flex",
              width: 56,
              height: 56,
              borderRadius: 16,
              background: "#1D1E1E",
              border: "2px solid #2C2E2E",
              alignItems: "center",
              justifyContent: "center",
              gap: 8,
            }}
          >
            <div style={{ width: 10, height: 18, borderRadius: 5, background: "#33D6A8" }} />
            <div style={{ width: 10, height: 18, borderRadius: 5, background: "#33D6A8" }} />
          </div>
          {NAME}
        </div>
        <div style={{ display: "flex", flexDirection: "column", gap: 24 }}>
          <div style={{ fontSize: 96, letterSpacing: -3, lineHeight: 1.05 }}>{TAGLINE}</div>
          <div style={{ fontSize: 34, color: "#9BA3A0" }}>
            Click-through desktop toy for macOS and Windows. Open source, MIT.
          </div>
        </div>
      </div>
    ),
    size,
  );
}
