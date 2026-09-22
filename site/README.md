# grindbot.melbora.com

The GrindBot landing site. Next.js App Router, Tailwind v4, no other runtime dependencies.

Every claim on the page comes from `lib/site.ts`, and the pack names, line counts and quoted
lines come from `lib/packs.ts`, which reads `../packs/*.txt` at build time. If the app changes,
change those two files; nothing else states a fact on its own.

```
npm install
npm run dev
npm run build
```

`lib/packs.ts` resolves `../packs` from the build's working directory, so the site must be built
from inside this folder with the rest of the repo checked out next to it.
