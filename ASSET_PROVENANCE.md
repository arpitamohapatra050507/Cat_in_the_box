# Asset Provenance

This prototype contains no downloaded stock-asset package. Temporary visuals
are either assembled at runtime by C# source written by OpenAI Codex from the
team's creative brief or generated with Codex's built-in image generator.
Audio is either synthesized by the runtime C# or supplied by the team and
listed with its source status below. The runtime source remains in
`Assets/Scripts/`, and every generated bitmap has a neighboring generation
record so judges can inspect how the temporary content was made.

## Generated content inventory

- Road modules, lane markings, junction signs, trees, poles, and barriers
- Hearse dashboard, windshield frame, steering wheel, cabin, and covered body
- Image-composited rear-view mirror and anomaly states
- Image-composited traffic cars, chase truck, side mirrors, and barricades
- Engine tone, wind ambience, radio static, impact sound, and horror sting
- Text interface, route prompt, temporary success screen, and failure screen

## Before submission

- Confirm in writing that AI-written procedural Unity geometry and synthesized
  audio satisfy the event's "AI-generated assets only" rule.
- Record the Unity version used for the final build.
- Add every later Meshy, Tripo, Scenario, or image-generation export with its
  prompt, generation date, model/version, license, and required attribution.
- Do not introduce stock Unity Asset Store content without organizer approval.

## Generated reference images

- `Assets/GeneratedAssets/Trees/References/DarkFir_MeshyReference.png` — generated
  with OpenAI Codex built-in image generation on 2026-08-18 as an image-to-3D
  reference for Meshy. The full prompt is stored beside it in
  `DarkFir_MeshyReference_SOURCE.md`.

## Generated runtime images

- `Assets/Resources/Road/RoadAlbedo.png` — generated with OpenAI Codex built-in
  image generation on 2026-08-19 as a seamless, top-down, dark rural-asphalt
  albedo. Runtime code tiles it over the procedural road with a non-metallic,
  low-smoothness material. Its full prompt, integration notes, and checksum are
  stored in `RoadAlbedo_SOURCE.md`.
- `Assets/Resources/Forest/DarkFirBillboard.png` — generated with OpenAI Codex
  built-in image generation on 2026-08-19 by editing the existing generated fir
  reference into a transparent, near-black background tree. It is retained for
  provenance but no longer loaded at runtime because its cyan rim and background
  read too brightly in standalone builds. The distant forest now uses batched,
  texture-free procedural silhouettes. Its exact prompt and checksum remain
  stored beside it in `DarkFirBillboard_SOURCE.md`.
- `Assets/Resources/Dashboard/DarkDashboardFascia.png` — generated with OpenAI
  Codex built-in image generation on 2026-08-18 and used as the in-world
  dashboard fascia. Its exact prompt is stored beside it in
  `DarkDashboardFascia_SOURCE.md`.
- `Assets/Resources/Dashboard/DarkSteeringWheel.png` — generated with OpenAI
  Codex built-in image generation on 2026-08-18 as a transparent, separately
  animated steering-wheel sprite. Its exact prompt is stored beside it in
  `DarkSteeringWheel_SOURCE.md`.
- `Assets/Resources/Mirror/RearCabinBackseat.png` — generated with OpenAI Codex
  built-in image generation on 2026-08-18 as the dark, empty rear-cabin plate
  used by the fully image-based rear-view mirror. Its exact prompt and checksum
  are stored beside it in `RearCabinBackseat_SOURCE.md`.
- `Assets/Resources/Mirror/WhiteGrainAnomaly.png` — generated with OpenAI Codex
  built-in image generation on 2026-08-18 as a transparent apparition layer.
  Its exact prompt and checksum are stored beside it in
  `WhiteGrainAnomaly_SOURCE.md`.
- `Assets/Resources/Anomalies/PursuerTruckFront.png` — generated with OpenAI
  Codex built-in image generation on 2026-08-18 as a transparent, front-facing
  truck layer for the side-mirror chase. Its prompt summary and checksum are
  stored beside it in `PursuerTruckFront_SOURCE.md`.
- `Assets/Resources/Anomalies/SideMirrorNightRoad.png` — generated with OpenAI
  Codex built-in image generation on 2026-08-18 as the opaque road plate behind
  the side-mirror chase. Its prompt summary and checksum are stored beside it
  in `SideMirrorNightRoad_SOURCE.md`.
- `Assets/Resources/Traffic/BarricadeReflective.png` — generated with OpenAI
  Codex built-in image generation on 2026-08-18 as a transparent visual layer
  for chase barricades. Its prompt summary and checksum are stored beside it in
  `BarricadeReflective_SOURCE.md`.
- `Assets/Resources/Traffic/OncomingSedanFront.png` — generated with OpenAI
  Codex built-in image generation on 2026-08-18 as the transparent front view
  of oncoming traffic. Its prompt summary and checksum are stored beside it in
  `OncomingSedanFront_SOURCE.md`.
- `Assets/Resources/Traffic/TrafficSedanRear.png` — generated with OpenAI Codex
  built-in image generation on 2026-08-18 as the transparent rear view of
  same-direction traffic. Its prompt summary and checksum are stored beside it
  in `TrafficSedanRear_SOURCE.md`.
- `Assets/Resources/Traffic/TrafficSedanSide.png` — generated with OpenAI Codex
  built-in image generation on 2026-08-18 as the side skin shared by both sides
  of the procedural traffic-car hull. The image generator twice baked its
  checkerboard into the pixels, so a connected-background alpha cleanup was
  applied without redrawing the car. Prompt and checksum are in
  `TrafficSedanSide_SOURCE.md`.
- `Assets/Resources/Traffic/TrafficSedanTop.png` — generated with OpenAI Codex
  built-in image generation on 2026-08-18 as the overhead skin on the traffic
  car hull. Prompt and checksum are in `TrafficSedanTop_SOURCE.md`.
- `Assets/Resources/Anomalies/ApparitionHandsEdges.png` — generated with OpenAI
  Codex built-in image generation on 2026-08-18 as the transparent escalating
  hands overlay for the rear-seat anomaly. Prompt and checksum are in
  `ApparitionHandsEdges_SOURCE.md`.
- `Assets/Resources/Anomalies/BrokenGlassOverlay.png` — generated with OpenAI
  Codex built-in image generation on 2026-08-18 as the transparent collision
  and truck-catch failure layer. Prompt and checksum are in
  `BrokenGlassOverlay_SOURCE.md`.
- `Assets/Resources/Anomalies/RoadFigure.png` — generated with OpenAI Codex
  built-in image generation on 2026-08-18 as a transparent road-silhouette
  billboard. Prompt and checksum are in `RoadFigure_SOURCE.md`.
- `Assets/Resources/Anomalies/CliffRoadEnding.png` — generated with OpenAI Codex
  built-in image generation on 2026-08-18 as the ten-minute cliff finale plate.
  Prompt and checksum are in `CliffRoadEnding_SOURCE.md`.

## Team-supplied audio

- `Assets/Resources/Audio/RadioStatic.wav` — converted from a team-supplied
  WhatsApp MPEG on 2026-08-18. Source and conversion details are stored in
  `RadioStatic_SOURCE.md`; confirm the original creator/license before final
  submission.
- `Assets/Resources/Audio/CarEngine.mp3` — supplied by a team member on
  2026-08-18 and used as the speed-driven engine loop. Format and source details
  are stored in `CarEngine_SOURCE.md`; confirm the original creator/license
  before final submission.
- `Assets/Resources/Audio/Anomalies/GhostAppearance.mp3` — copied from the
  team-supplied `sfx/ghost_appearance.mp3` in commit `ead9c19` on 2026-08-18 and
  used for the rear-seat apparition. Format, loudness, checksum, and source
  details are stored in `GhostAppearance_SOURCE.md`; confirm the original
  creator/license before final submission.
- `Assets/Resources/Audio/Anomalies/TruckChase.mp3` — copied from the
  team-supplied `music/creature_chase.mp3` in commit `ead9c19` on 2026-08-18 and
  used during the pursuer-truck sequence. Format, loudness, checksum, and source
  details are stored in `TruckChase_SOURCE.md`; confirm the original
  creator/license before final submission. This source is unusually loud, so
  runtime playback must remain strongly attenuated.
- `Assets/Resources/Anomalies/CarDeathScreen.png` and
  `TruckDeathScreen.png` — team-supplied death-screen artwork copied from the
  local project handoff on 2026-08-20. The car-collision and truck-catch paths
  use them directly; the former broken-glass overlay is no longer drawn.
- `Assets/Resources/Audio/Anomalies/Scary3.mp3`, `Scary4.wav`, `Scary5.wav`,
  `Thunder.wav`, and `TruckJumpscare.mp3` — copied from the team-supplied
  `sfx/` folder in the latest audio update. The first four are scattered as
  low-volume ambient stings; `TruckJumpscare.mp3` replaces the procedural truck
  impact sound.
- `Assets/Resources/Audio/Anomalies/Scary1.mp3` and `Scary2.mp3` — copied from
  the team-supplied `sfx/scary_noises/` folder in commit `62bcb42` on
  2026-08-19 and used as restrained checkpoint stings. Their format, measured
  levels, checksums, and source status are stored in `Scary1_SOURCE.md` and
  `Scary2_SOURCE.md`; confirm the original creator/license before submission.

## Team-created or supplied assets

- `Assets/Models/FrostCar.fbx` — team-supplied export from an older racing
  project, originally uploaded to the `demo` branch in commit `5504d70`.
  It contained the whole old scene. A Blender-cleaned model-only derivative is
  used at `Assets/Resources/Models/Traffic/FrostCarVisual.fbx`; the extraction
  details and both checksums are stored in
  `FrostCarVisual_SOURCE.md`. Confirm its original authorship/license and that
  this older, apparently non-AI model is eligible under the jam's AI-only asset
  rule before submission.

- `Assets/Models/RoadTemplate.fbx` — team-supplied road scene from the older
  racing project, originally uploaded to the `demo` branch in commit `b02d5a5`.
  A Blender-cleaned road-only derivative is retained at
  `Assets/Resources/Models/Road/RoadTemplateTestVisual.fbx`; it is normalized
  to 8 by 80 world units and has runtime-remapped materials, so the missing
  legacy `Color_Grid.png` is not required. Its first Unity import had an
  incompatible axis, so runtime code currently rejects that exact asset and
  uses the procedural road fallback. Extraction details and checksums are stored
  in `RoadTemplateTestVisual_SOURCE.md`. Confirm the original
  authorship/license and AI-only-rule eligibility before submission.

- `Assets/Resources/Models/Trees/EvergreenOptimized.fbx` and
  `EvergreenTexture.png` — optimized runtime derivative and texture from the
  team's `demo`-branch evergreen source (mesh commit `aedabbc`, texture commit
  `4fc1dfd`). The derivative reduces the source from
  about 145,000 polygons to 2,892 for repeated roadside use. Source path,
  transformation, and checksums are stored in `EvergreenOptimized_SOURCE.md`.
  Its first Unity import also had an incompatible axis and is not loaded by
  default; procedural pines remain the empty-slot fallback. Explicit serialized
  tree-prefab assignments are still honored without a name-based block.
  Confirm the original authorship/license and AI-only-rule eligibility before
  submission.

- `Assets/Resources/Models/Trees/TeamPineRuntime.fbx` — build-packaged copy of
  the team's reduced `Assets/Models/tree.fbx` from commit `13a0cf3`. Runtime
  code loads it only when no Pine Tree Prefab override is assigned, then applies
  the packaged `EvergreenTexture.png` using a dark headlight-reactive material.
  This explicit Resources dependency prevents standalone builds that start from
  `SampleScene` from silently dropping back to procedural cone trees. Source and
  runtime checksums are recorded in `TeamPineRuntime_SOURCE.md`. Confirm the
  original authorship/license and AI-only-rule eligibility before submission.
