# Asset Provenance

This prototype contains no downloaded stock art, models, or animations. Three
team-supplied audio files are tracked below and still require creator/license
confirmation before submission.

The procedural geometry, materials, fog, interface elements, and fallback audio
waveforms are generated at runtime by C# source written by OpenAI Codex from the
team's creative brief. The source code is preserved in `Assets/Scripts/` so
judges can inspect how each temporary asset was created.

## Generated content inventory

- Road modules, lane markings, junction signs, trees, poles, and barriers
- Hearse dashboard, windshield frame, steering wheel, cabin, and covered body
- Rear-view mirror render and anomaly states
- Fallback engine and radio loops, wind ambience, impact sound, and horror sting
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
  used by the live rear-view camera. Its exact prompt and checksum are stored
  beside it in `RearCabinBackseat_SOURCE.md`.
- `Assets/Resources/Mirror/WhiteGrainAnomaly.png` — generated with OpenAI Codex
  built-in image generation on 2026-08-18 as a transparent apparition layer.
  Its exact prompt and checksum are stored beside it in
  `WhiteGrainAnomaly_SOURCE.md`.

## Team-supplied audio

- `Assets/Resources/Audio/RadioStatic.mp3` — 4.86-second radio-noise loop,
  imported as normalized mono. Details are in `RadioStatic_SOURCE.md`.
- `Assets/Resources/Audio/CarEngine.mp3` — 17.06-second engine loop, imported as
  mono and driven by vehicle speed. Details are in `CarEngine_SOURCE.md`.
- `Assets/Resources/Audio/MenuTheme.mp3` — 192.84-second stereo title theme,
  streamed to limit memory use. Details are in `MenuTheme_SOURCE.md`.

Confirm the original creator, AI-generation method where applicable, license,
and event-rule eligibility for all three files before the final submission.

## Team-created assets pending import

- The `demo` branch contains `Assets/Models/RoadTemplate.fbx`, credited by the
  team as an older custom racing-game asset. Its referenced `Color_Grid.png`
  texture is not present in Git yet, so it has not been imported into the
  prototype. Record the original author and license/permission when the missing
  texture is supplied.
