# Generated road-albedo record

- Generated: 2026-08-19
- Generator: OpenAI Codex built-in image generation
- Runtime asset: `RoadAlbedo.png`
- Dimensions: 1254 by 1254 pixels, RGB PNG
- SHA-256: `3f9047721e36deb6da4e7805e14f74e12b0fa57c5fcf269db8648ecc58e47eee`

## Prompt

```text
Use case: stylized-concept
Asset type: seamless tileable game texture for a Unity night-road material
Primary request: realistic old rural asphalt, very dark charcoal-black, fine compact aggregate, subtle longitudinal tyre wear, sparse hairline cracks and faint repaired patches integrated naturally into the surface
Style/medium: photorealistic PBR-style diffuse albedo texture, restrained and grounded
Composition/framing: perfectly orthographic straight top-down square texture, uniform scale across the entire image, edge-to-edge seamless tiling in both axes, no focal point
Lighting/mood: flat neutral diffuse reference lighting only; no directional lighting and no baked shadows
Materials/textures: dry-to-slightly-damp rough asphalt with high microscopic roughness; repaired areas remain nearly the same dark value as the surrounding road
Constraints: seamless edges; uniform dark value; no road markings; no centre stripe; no shoulder; no curb; no gravel border; no objects; no perspective; no horizon; no vignette; no text; no logos; no watermark
Avoid: bright gray slabs, pale patches, puddles, glossy reflections, headlight glare, specular hotspots, strong cracks, repeating hero features, cyan or blue color cast
```

Unity imports the source at a maximum of 1024 pixels, repeats it at a 1-by-10
tiling ratio over each 8-by-80 procedural road chunk, and combines it with a
non-metallic, low-smoothness material. White lane markings remain separate
geometry so they are not baked into the repeating asphalt.
