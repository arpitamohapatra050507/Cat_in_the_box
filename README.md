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
- **Traffic Car Prefab** — replaces both generated oncoming and slower cars.
- **Barricade Prefab** — replaces the generated chase barricades.
- **Road Chunk Length** — distance between repeated road-prefab instances;
  leave it at `80` unless the custom chunk has a different length.

Each field falls back independently. For example, assigning only the Meshy fir
prefab replaces the pine trees while the generated road and leafless trees stay
in use. Custom trees are automatically scaled to the randomized target height,
centered, and placed on the ground, so arbitrary Meshy export scale is fine.

A road prefab's root must represent the beginning of the chunk at local
`(0, 0, 0)`, face forward along local `+Z`, and extend for **Road Chunk Length**
units. Stop Play Mode before changing a slot, then start it again to rebuild the
runtime road. A traffic prefab should also face local `+Z`; it is rotated for
oncoming traffic as needed. Traffic and barricade prefabs are grounded from
their visible renderer bounds, and their physical colliders are disabled in
favor of the prototype's transform-safe swept collision test. Rebuilding the
Prototype scene preserves all assigned overrides.

Every slot has an independent fallback. Empty traffic and barricade fields use
generated low-poly models with the AI-generated images in
`Assets/Resources/Traffic`, just as empty road and tree fields retain their
procedural versions.

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
immediately fatal. The fallback remains cheap to render, but its basic 3D hull
now carries generated front/rear, mirrored side-profile, and overhead skins
rather than looking like a single flat card.

The rear-seat apparition checks once every 30 seconds with a 50% chance. When
it appears, the player has three seconds to hold `R` and face it in the
rear-view mirror. Holding the view for about half a second dispels it. Ignoring
it makes pale hands creep in from the screen edges while its audio rises; once
the timer expires, the apparition kills the driver. Missed probability rolls
do not accumulate or overlap later checks.

Once the player has passed the junction and travelled far enough, a horn warns
of a pursuing truck. The chase lasts about 30 seconds. Image-based side mirrors
show the truck gaining or losing ground, while the team-supplied
`Assets/Resources/Audio/Anomalies/TruckChase.mp3` plays over the existing audio
mix. The loud source is capped at 5% playback volume and fades in for three
seconds at pursuit start and out over the last three seconds. Stay above roughly
78% of maximum speed to open the gap. Slowing lets the truck close in. Ordinary
traffic is cleared and a single-lane barricade appears every 1.6–2.5 seconds,
always leaving another path open. Three compact bars above the dashboard show
the engine's remaining chase health; each barricade removes one bar and cuts
speed. At zero bars, or if the truck closes the gap, the engine dies, the truck
slams into the screen, and broken glass frames the death menu. Surviving the
timer fades the truck away and resumes ordinary traffic.

At approximately two-minute randomized intervals, a featureless black figure
can appear in the headlight beam. It has no collision penalty and vanishes when
the player drives through it. It is an atmospheric fake-out, not another health
system.

The dark-red screen border is a danger indicator, not an unexplained creature
or separate anomaly. A short flash means the car struck the shoulder, traffic,
or a barricade. During the truck chase it becomes a pulsing proximity warning:
the stronger and faster it pulses, the closer the truck is. The dashboard also
shows `DANGER BEHIND — KEEP SPEED` while that threat is active.

The forest is darker under a generated soft edge vignette, with lower moon and
ambient illumination. Two fixed long-range headlights plus a wide fill beam
keep the lane, traffic, barricades, and figure readable. They are attached to
the car and intentionally do not follow the mouse yet.

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

When no road prefab is assigned, the generated road material loads
`Assets/Resources/Road/RoadAlbedo.png` when provided. The `demo` branch's FBX
currently references a texture that was not committed, so the procedural
asphalt remains the safe fallback.

The production asset workflow and first three generation batches are described
in `ASSET_PLAN.md`.

## Building

Use **File > Build Profiles**, select Windows x86-64, and include
`Assets/Scenes/Prototype.unity`. The editor bootstrap normally configures this
for you. The generated materials use this project's Universal Render Pipeline,
and the controls support its Input System configuration.

## Scope boundaries

This prototype does not decide who killed whom, why it happened, or whether
the final interpretation is supernatural. Those details can be layered onto
the configurable road-event structure after the driving loop is accepted.
