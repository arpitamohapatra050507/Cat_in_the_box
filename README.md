# The Last Passenger — Unity Prototype

A self-contained first-person driving-horror prototype for Unity 6. The scene
is assembled at runtime from procedural geometry and the included generated
image/audio assets, with fallbacks for replaceable content. No external art or
audio packages are needed.

## Open and run

1. Add this folder to Unity Hub and open it with Unity `6000.5.8f1` (the
   repository's current editor version), or let Unity Hub install that version.
2. Wait for scripts to compile. The editor creates `Assets/Scenes/Prototype.unity`
   and adds it to Build Settings automatically.
3. Open that scene if Unity did not do so automatically, then press Play.
4. To rebuild the scene manually, use **Tools > The Last Passenger > Rebuild Prototype Scene**.

## Replace generated environment and hazard assets

Open **Tools > The Last Passenger > Select Prefab Overrides**. Unity selects the
`Prototype asset configuration — assign prefabs here` object in the Prototype
scene. Its Inspector contains these optional serialized fields:

- **Road Chunk Prefab** — replaces every ordinary repeating road chunk.
- **Pine Tree Prefab** — replaces the common roadside fir trees.
- **Leafless Tree Prefab** — replaces the occasional bare trees.
- **Traffic Car Prefab** — replaces both default Frost-model oncoming and slower cars.
- **Barricade Prefab** — replaces the generated chase barricades.
- **Road Chunk Length** — distance between repeated road-prefab instances;
  leave it at `80` unless the custom chunk has a different length.

Each field falls back independently. With no assignments, the road uses the
cleaned, normalized team road and pine slots use the optimized team evergreen;
leafless trees and barricades retain their procedural fallbacks. Assigning only
a Meshy fir replaces those default evergreens without affecting anything else.
Custom trees are automatically scaled to the randomized target height,
centered, and placed on the ground, so arbitrary Meshy export scale is fine.

A road prefab's root must represent the beginning of the chunk at local
`(0, 0, 0)`, face forward along local `+Z`, and extend for **Road Chunk Length**
units. Stop Play Mode before changing a slot, then start it again to rebuild the
runtime road. A traffic prefab should also face local `+Z`; it is rotated for
oncoming traffic as needed. Traffic and barricade prefabs are grounded from
their visible renderer bounds, and their physical colliders are disabled in
favor of the prototype's transform-safe swept collision test. Rebuilding the
Prototype scene preserves all assigned overrides.

Every slot has an independent fallback. An empty Traffic Car field loads the
clean model-only extraction at
`Assets/Resources/Models/Traffic/FrostCarVisual.fbx`. The source racing-scene
FBX contained its own cameras, lights, UI, effects, and control setup, so only
its body, doors, windows, and wheel meshes are present in the runtime copy.
An existing Inspector assignment pointing at the original asset named
`FrostCar` is redirected to this clean copy automatically, so old local scenes
do not need their Traffic Car field repaired by hand.
If that resource is unavailable, traffic falls back again to the generated
low-poly car and AI-generated images in `Assets/Resources/Traffic`. Empty road,
pine, leafless-tree, and barricade fields use the cleaned team road, optimized
evergreen, generated bare tree, and generated barricade respectively. Existing
Inspector references named `RoadTemplate` or `Evergreen...` are redirected to
the cleaned runtime copies so old scene assignments cannot reintroduce the
oversized roadside scene or 145,000-polygon source tree.

All instantiated traffic overrides are first created beneath an inactive
quarantine object, then treated as visuals before activation: inherited
`Behaviour` components are disabled, particle effects are stopped, colliders
are disabled, and rigidbodies are made kinematic. Consequently, an old racing
prefab cannot run its player controller, camera, audio listener, or physics even
for its first frame.

## Controls

- `W` / Up Arrow — accelerate
- `S` / Down Arrow — brake
- `A` / `D` — steer within the road
- Hold right mouse button and move the mouse — look around the cabin
- Hold `R` — enlarge and inspect the rear-view mirror
- `M` — toggle the radio
- `Enter` — restart after an ending
- `Escape` — release the mouse or quit a standalone build

Debug shortcuts are available only in the editor or development builds:

- `F8` — immediately request the truck-chase sequence
- `F9` — jump near the junction
- `F10` — immediately show a rear-seat apparition; it no longer teleports the car
- `F11` — jump near the ending

## Prototype flow

The radio warns that "the dead keep left." At the fork, position the car in
the left or right lane before crossing the decision line. Both routes continue
into the full prototype run; at sustained maximum speed, the road reaches its
cliff ending at roughly ten minutes. Slower driving naturally takes longer.

Ordinary road traffic appears throughout the active run. At most three
oncoming or slower cars are present, with another spawn attempt every 7–12
seconds. Their movement and collision use swept X/Z checks rather than
Rigidbody physics, so a fast oncoming car cannot pass through the
transform-driven player between frames. Any collision with another car is
immediately fatal. With no Inspector override, ordinary traffic uses the
model-only Frost car extracted from the team's old racing scene. Its imported
materials are remapped to the prototype's night-safe URP paint, windows, tyres,
headlights, and tail lights. The previous tapered low-poly sedan and its
generated image skins remain available as a last-resort fallback if the cleaned
model cannot be loaded.

The rear-seat apparition checks once every 30 seconds with a 50% chance. Its
danger meter rises from zero to three seconds while ignored. Holding `R` turns
that same meter backwards at the same rate: looking after 2.75 seconds therefore
takes 2.75 seconds of uninterrupted observation to clear it. The apparition,
edge hands, and audio all fade continuously as the meter drains. Releasing `R`
before zero makes the danger climb again; reaching three seconds while not
draining it kills the driver. Missed probability rolls do not accumulate or
overlap later checks.

Once the player has passed the junction and travelled far enough, a horn warns
of a pursuing truck. The chase lasts about 30 seconds. Image-based side mirrors
show the truck gaining or losing ground, while the team-supplied
`Assets/Resources/Audio/Anomalies/TruckChase.mp3` plays over the existing audio
mix. The loud source is capped at 5% playback volume, begins immediately,
repeats its audible section at constant volume, and fades only after the chase
has ended. Stay above roughly
78% of maximum speed to open the gap. Slowing lets the truck close in. Ordinary
traffic is cleared and the director attempts a single-lane barricade every
2.2–3.1 seconds. Obstacles appear farther ahead, use a more forgiving hitbox,
and must remain at least 27 world units apart, so another path and enough
reaction distance remain available. Three compact bars above the dashboard show
the engine's remaining chase health; each barricade removes one bar and cuts
speed. At zero bars, or if the truck closes the gap, the engine dies, the truck
slams into the screen, and broken glass frames the death menu. Surviving the
timer fades the truck away and resumes ordinary traffic.

At approximately two-minute randomized intervals, a featureless black figure
can appear in the headlight beam. It has no collision penalty and vanishes when
the player drives through it. It is an atmospheric fake-out, not another health
system.

The player's available top speed rises smoothly from 10 to 15.5 world units
per second, beginning after the first minute and reaching its maximum around
the nine-minute mark. Faster late-run driving makes steering and obstacle
avoidance more demanding; impacts still remove actual speed. The cliff was
moved to distance 7500 so a clean high-speed run remains approximately ten
minutes despite the increasing limit.

The dark-red screen border is a danger indicator, not an unexplained creature
or separate anomaly. A short flash means the car struck the shoulder, traffic,
or a barricade. During the truck chase it becomes a pulsing proximity warning:
the stronger and faster it pulses, the closer the truck is. The dashboard also
shows `DANGER BEHIND — KEEP SPEED` while that threat is active.

The road now uses headlight-dominant night lighting rather than relying on a
screen overlay: ambient and moon illumination are effectively black, while two
strong focused warm spotlights reveal the lane and a short wide beam fills the
area immediately in front of the car. Asphalt, soil, bark, and foliage remain
almost unreadable outside those beams. Distant forest cards use a Lit material,
so they no longer glow cyan from an Unlit shader and respond to the same night
lighting. The headlights are attached to the car and intentionally do not
follow the mouse yet.

Each recycled road chunk contains 16 near-field 3D trees. The default pine is a
2,892-polygon optimized copy of the team's evergreen source; serialized pine or
leafless overrides still take priority, with procedural geometry as the final
fallback. Behind them are 36 generated distant firs. Those far trees are crossed transparent
cards combined into one mesh and one renderer per chunk, so the forest looks
dense without creating hundreds of separate background renderers.

Road messages are editable without changing C# in
`Assets/Resources/road_events.json`. Each entry has an ID, trigger distance,
message, color, and display duration; this is the extension point for later
radio reports, memories, accusations, and ambiguous story fragments.

Steering now rotates the vehicle and moves it along its heading instead of
sliding it sideways. A transparent 2D wheel follows steering input in front of
a lowered, widened, slanted dashboard with physical side trim. A procedural
front passenger seat supplies real parallax when looking right. The rear-view
mirror is fully image-based: it displays a fixed generated dark backseat plate
and never renders the vehicle's live geometry. A separately composited,
transparent white-grain apparition flickers over that image during the anomaly.
The radio display flickers and changes during the same event. The team-supplied static is
installed at `Assets/Resources/Audio/RadioStatic.wav` and plays quietly on its
own 2D audio source. The team-supplied engine at
`Assets/Resources/Audio/CarEngine.mp3` is layered over it on a separate source:
its volume rises smoothly from silence while accelerating, reaches at most 50%
at full speed, and returns to silence when the car stops. Procedural audio is
used if either custom clip is missing. The apparition and truck-chase clips are
loaded separately, allowing the radio, engine, and anomaly sounds to overlap.

When no road prefab is assigned, the prototype loads
`Assets/Resources/Models/Road/RoadTemplateTestVisual.fbx`: a cleaned copy of the
team road normalized to exactly 8 by 80 world units. Its material slots are
remapped at runtime to the prototype's dark asphalt, lane paint, and rail
materials, so the missing legacy `Color_Grid.png` is not required. Procedural
road geometry remains the final fallback if that resource cannot load.

The production asset workflow and first three generation batches are described
in `ASSET_PLAN.md`.

## Building

Use **File > Build Profiles**, select Windows x86-64, and include
`Assets/Scenes/Prototype.unity`. The editor bootstrap normally configures this
for you. The generated materials use this project's Universal Render Pipeline.
URP Lit and Unlit are explicitly retained in Graphics Settings because runtime
geometry resolves them by name; this prevents shader stripping from producing
null materials in a standalone build. The controls support the Input System
configuration.

## Scope boundaries

This prototype does not decide who killed whom, why it happened, or whether
the final interpretation is supernatural. Those details can be layered onto
the configurable road-event structure after the driving loop is accepted.
