# Launch drafts

Drafts only. Nothing here has been posted, and nothing should be posted without
the owner reading it first.

Before any of this goes out, two things have to be true, because every draft
assumes them:

1. **A release exists.** There are no tags and no GitHub Release yet, so the
   site sends people to build instructions. Tag `v1.0.0` and let the release
   workflow publish the Windows zip, the macOS zip and the DMG.
2. **Someone has run the macOS build on a real Mac.** CI compiles it and checks
   the bundle, but no human has watched the robot appear on a Mac.

If you post before both, expect the first comment to be "where's the download"
and the second to be a Gatekeeper screenshot.

| File | Where | Notes |
|---|---|---|
| `show-hn.md` | Hacker News | Title + body. HN hates adjectives; this one states the mechanic and the limitations |
| `reddit.md` | r/macapps, r/windowsapps, r/commandline | One per subreddit, each rewritten — do not cross-post the same text |
| `x-thread.md` | X | Four posts; the demo video carries it, so do not post without the video |
| `product-hunt.md` | Product Hunt | Tagline, description, first comment |

Honest positioning, used in all of them: this is a toy, it is free, it is MIT,
it has no backend and no telemetry, and the message packs are the product.
