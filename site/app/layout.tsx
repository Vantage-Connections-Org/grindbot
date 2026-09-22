import type { Metadata, Viewport } from "next";
import { Instrument_Serif, Inter, JetBrains_Mono } from "next/font/google";
import { Analytics } from "@vercel/analytics/next";
import { DESCRIPTION, FAQ, LICENSE_URL, NAME, REPO_URL, SITE_URL, TITLE } from "@/lib/site";
import "./globals.css";

// Display face carries the whole identity: the engraved caps under a poster.
const display = Instrument_Serif({ variable: "--font-display", subsets: ["latin"], weight: "400" });
const body = Inter({ variable: "--font-body", subsets: ["latin"] });
const mono = JetBrains_Mono({ variable: "--font-mono-face", subsets: ["latin"] });

export const metadata: Metadata = {
  metadataBase: new URL(SITE_URL),
  title: TITLE,
  description: DESCRIPTION,
  applicationName: NAME,
  keywords: [
    "GrindBot",
    "desktop toy",
    "screen recording",
    "motivational overlay",
    "click-through overlay",
    "macOS menu bar app",
    "Windows tray app",
    "open source",
  ],
  alternates: { canonical: "/" },
  openGraph: { type: "website", url: "/", siteName: NAME, title: TITLE, description: DESCRIPTION },
  twitter: { card: "summary_large_image", title: TITLE, description: DESCRIPTION },
};

export const viewport: Viewport = {
  themeColor: [
    { media: "(prefers-color-scheme: light)", color: "#f3efe6" },
    { media: "(prefers-color-scheme: dark)", color: "#17150f" },
  ],
};

// Structured data: what the app is, and the FAQ. No ratings, no download count, no price
// beyond free — only things that are true of the repo.
const jsonLd = [
  {
    "@context": "https://schema.org",
    "@type": "SoftwareApplication",
    name: NAME,
    description: DESCRIPTION,
    applicationCategory: "UtilitiesApplication",
    operatingSystem: "macOS 13+, Windows 10, Windows 11",
    url: SITE_URL,
    softwareHelp: `${REPO_URL}#readme`,
    license: LICENSE_URL,
    isAccessibleForFree: true,
    offers: { "@type": "Offer", price: "0", priceCurrency: "USD" },
    codeRepository: REPO_URL,
  },
  {
    "@context": "https://schema.org",
    "@type": "FAQPage",
    mainEntity: FAQ.map(({ q, a }) => ({
      "@type": "Question",
      name: q,
      acceptedAnswer: { "@type": "Answer", text: a },
    })),
  },
];

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html lang="en" className={`${display.variable} ${body.variable} ${mono.variable} antialiased`}>
      <body className="min-h-[100dvh] font-sans">
        {children}
        <Analytics />
        <script type="application/ld+json" dangerouslySetInnerHTML={{ __html: JSON.stringify(jsonLd) }} />
      </body>
    </html>
  );
}
