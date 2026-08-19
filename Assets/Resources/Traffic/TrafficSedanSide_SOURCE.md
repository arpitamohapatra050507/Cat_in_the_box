# Traffic sedan side generation record

- Generator: OpenAI Codex built-in image generation
- Generated: 2026-08-18
- Runtime asset: `TrafficSedanSide.png`
- SHA-256: `4dc8f618112ef17ef2bcc108887d58b9a237933ff67f2bfa6056a212a5102883`
- Intended use: right-side skin for the existing procedural traffic-car hull; horizontally flipped for the left side

## Final prompt set

Using the existing front and rear traffic-sedan cutouts as identity references,
create a precise orthographic side profile of the exact same anonymous dirty
late-1990s dark sedan. Preserve the nearly black rain-streaked paint, proportions,
windows, wheels, grime, and survival-horror lighting. Show the complete passenger
side perfectly flat and centered, with no perspective, ground, shadow, scenery,
badge, text, logo, people, or watermark, isolated on transparent alpha.

The generator returned a baked checkerboard after two transparency requests.
ImageMagick flood-fill removed only the connected bright background to produce
the final transparent runtime cutout; the generated car pixels were not redrawn.
