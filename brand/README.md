# Brand assets

The identity is a parody of the corporate motivational poster, because that is
what the message packs are. Cream paper, black frames, brass, engraved serif
capitals. The page is light in both colour schemes; the posters are the dark
objects on it.

| File | What it is |
|---|---|
| `hero-v1.png` | The landscape `PERSISTENCE` poster used as the site hero |
| `poster-momentum.png` | `MOMENTUM` — robot alone at an empty boardroom table |
| `poster-discipline.png` | `DISCIPLINE` — dawn warehouse, long shadow |
| `poster-synergy.png` | `SYNERGY` — dying plant, Q3 objectives chart |
| `icon-render.png` | App icon render, source for `.ico` / `.icns` |
| `icon-v1.svg` | First vector icon pass |

Shipping copies live in `site/public/posters/*.webp`, resized and compressed
(351 KB for all four, from ~24 MB of source PNG). The favicon at
`site/app/icon.svg` is drawn by hand rather than traced from the render: the
render's detail collapses below about 32px, so the mark keeps one dark visor
block and two fat eye bars and drops the antenna.

Palette: `--bg #f3efe6`, `--ink #171513`, `--brass #a67c34`, `--brass-bright
#c89b4a`, `--frame #15130f`. Display face is Instrument Serif.

Generated with Higgsfield (GPT Image 2 for the posters, Recraft for the first
vector pass). Regenerate with the prompts recorded in the commit history rather
than editing the PNGs.
