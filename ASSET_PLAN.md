# The Last Passenger — Asset Plan

The prototype will use a hybrid asset approach: generated 2D art for the fixed
cockpit framing and distant scenery, with a very small set of generated 3D
models for objects that need parallax and lighting.

## Art direction lock

- Low-poly late-1990s/early-2000s horror rather than photorealism
- Near-black cabin, cold blue-grey road, dirty desaturated greens and browns
- Chunky silhouettes and imperfect surfaces that remain readable through fog
- Restrained red only for warnings, blood, and anomalies
- No clean modern screens, glossy showroom materials, or bright daytime skies

Keep this direction in every generation prompt. Generate a few candidates,
choose one, and iterate from it instead of mixing unrelated generator styles.

## Asset batch 1 — dashboard fascia

The first dashboard fascia is now generated at
`Assets/Resources/Dashboard/DarkDashboardFascia.png`. It is mapped to an
in-world quad so camera-look still has parallax. The steering wheel and radio
display remain separate procedural objects and animate independently.

Keep the procedural cabin as a structural fallback. Validate the panel at 16:9
and 16:10 resolutions before replacing more of the cabin geometry.

## Asset batch 2 — forest kit

Generate only two 3D source models:

1. A dense conifer/pine, 6–9 metres tall, with a strong triangular silhouette.
2. A dead leafless pine, 5–8 metres tall, with broken asymmetric branches.

Use Meshy or Tripo for image-to-3D after approving front/side concept images.
In Blender, put each origin at the trunk base, apply transforms, remove hidden
geometry, reduce material slots, and decimate each tree to roughly 1,500–3,000
triangles. Export FBX with one 1024px PNG texture set per tree. Put each export
in its own subfolder under `Assets/GeneratedAssets/Trees/`; that folder contains
the first prompt and handoff checklist. GLB can wait until the project has a
compatible importer.

Unity can turn two models into dozens of apparent variants by changing scale,
Y rotation, spacing, tint, and the number of dead trees in each road chunk.
Start around 80% living pine and 20% leafless pine, with more dead trees after
the junction or during anomalies.

## Asset batch 3 — distant forest

Generate a seamless, transparent 2048x512 forest silhouette strip. Place it on
large crossed billboard planes behind the 3D roadside trees. It fills the
horizon cheaply while nearby 3D trees preserve motion parallax.

## Order of work

1. Approve one cockpit concept and one forest concept sheet.
2. Validate the generated dashboard fascia, animated wheel, and radio placement.
3. Generate and clean the two tree models.
4. Add the distant billboard only if visible gaps remain through the fog.
5. Record prompts, generator, date, license, and manual cleanup in
   `ASSET_PROVENANCE.md` before committing each asset.
