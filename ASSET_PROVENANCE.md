# Asset Provenance

This prototype contains no downloaded stock art, models, music, animations, or
sound effects.

All visible geometry, materials, fog, interface elements, and audio waveforms
are generated at runtime by C# source written by OpenAI Codex from the team's
creative brief. The source code is preserved in `Assets/Scripts/` so judges can
inspect how each temporary asset was created.

## Generated content inventory

- Road modules, lane markings, junction signs, trees, poles, and barriers
- Hearse dashboard, windshield frame, steering wheel, cabin, and covered body
- Rear-view mirror render and anomaly states
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

- `Assets/Resources/Dashboard/DarkDashboardFascia.png` — generated with OpenAI
  Codex built-in image generation on 2026-08-18 and used as the in-world
  dashboard fascia. Its exact prompt is stored beside it in
  `DarkDashboardFascia_SOURCE.md`.
- `Assets/Resources/Dashboard/DarkSteeringWheel.png` — generated with OpenAI
  Codex built-in image generation on 2026-08-18 as a transparent, separately
  animated steering-wheel sprite. Its exact prompt is stored beside it in
  `DarkSteeringWheel_SOURCE.md`.

## Team-supplied audio

- `Assets/Resources/Audio/RadioStatic.wav` — converted from a team-supplied
  WhatsApp MPEG on 2026-08-18. Source and conversion details are stored in
  `RadioStatic_SOURCE.md`; confirm the original creator/license before final
  submission.

## Team-created assets pending import

- The `demo` branch contains `Assets/Models/RoadTemplate.fbx`, credited by the
  team as an older custom racing-game asset. Its referenced `Color_Grid.png`
  texture is not present in Git yet, so it has not been imported into the
  prototype. Record the original author and license/permission when the missing
  texture is supplied.
