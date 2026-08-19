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

Each field falls back independently. With no assignments, the road and
leafless trees use the stable procedural versions, while common pines load the
build-packaged reduced team tree. Assigning only a Meshy fir replaces those
packaged pines without affecting anything else. Custom trees are automatically
scaled to the randomized target height,
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
low-poly car and AI-generated images in `Assets/Resources/Traffic`. An empty
Pine Tree field loads `Assets/Resources/Models/Trees/TeamPineRuntime.fbx` and
its packaged texture; empty road, leafless-tree, and barricade fields use the
generated versions. The experimental cleaned team road remains blocked because
of its incompatible axis. Tree overrides are not
blocked by asset name: explicitly assigning the team's working tree prefab (or
any later Meshy tree) always uses that prefab.

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

Temporary event-test shortcuts are available in both editor and standalone builds:

- `T` — immediately request the truck-chase sequence
- `G` — immediately show a rear-seat apparition
- `H` — immediately place the tall road figure ahead of the car
- `Y` — jump to the cliff-ending trigger

The legacy function-key shortcuts remain available as aliases:

- `F8` — immediately request the truck-chase sequence
- `F9` — jump to the next anomaly checkpoint
- `F10` — immediately show a rear-seat apparition; it no longer teleports the car
- `F11` — jump near the ending

## Prototype flow

The road is one continuous route: the unused fake fork and choice have been
removed. Four equal-distance checkpoints at 20%, 40%, 60%, and 80% gradually
tighten anomaly pacing. At sustained maximum speed, the cliff ending arrives
at roughly five minutes; slower driving naturally takes longer.

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

The rear-seat apparition begins with a 14-second/50% roll and becomes slightly
more frequent at later checkpoints, capped at 11 seconds/60%. Its
danger meter rises from zero to three seconds while ignored. Holding `R` turns
that same meter backwards at the same rate: looking after 2.75 seconds therefore
takes 2.75 seconds of uninterrupted observation to clear it. The apparition,
edge hands, and audio all fade continuously as the meter drains. Releasing `R`
before zero makes the danger climb again; reaching three seconds while not
draining it kills the driver. Missed probability rolls do not accumulate or
overlap later checks.

At checkpoint II, a horn warns of a pursuing truck. The chase lasts about 30 seconds. Image-based side mirrors
show the truck gaining or losing ground. Its mirror image now starts larger and
grows to 182% of the mirror height at maximum proximity, making the pursuer
feel closer. The team-supplied
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

The harmless black road figure starts at a randomized 95–120 second interval
and can appear more often after checkpoints. It vanishes when the player drives
through it. `Scary1` and `Scary2` are quiet checkpoint one-shots over the mix.

The player's available top speed rises smoothly from 18 to 23.5 world units
per second, beginning after the first minute and reaching its maximum around
the five-minute finale. Faster late-run driving makes steering and obstacle
avoidance more demanding; impacts still remove actual speed. The cliff is at
distance 6000 so a clean high-speed run remains approximately five minutes.

The dark-red screen border is a danger indicator, not an unexplained creature
or separate anomaly. A short flash means the car struck the shoulder, traffic,
or a barricade. During the truck chase it becomes a pulsing proximity warning:
the stronger and faster it pulses, the closer the truck is. The dashboard also
shows `DANGER BEHIND — KEEP SPEED` while that threat is active.

The road is headlight-only: startup disables scene-authored lights, removes the
moon, forces black ambient/reflections/fog, and disables HDR on the driving
camera. Only the car's focused headlight spots plus its short wide near-field
beam remain. A build-safe darkness veil and radial vignette add exposure
control; this is not dependent on a custom darkness shader. Asphalt, soil,
bark, and foliage are almost unreadable outside the beams.

Each recycled road chunk contains 16 near-field 3D trees. Pines use the
build-packaged reduced team model unless a serialized override is supplied;
leafless trees retain their procedural fallback. Behind them are 36 generated
distant fir silhouettes. Their crossed triangular geometry is combined into
one mesh and one renderer per chunk, so the forest looks dense without loading
the old cyan texture or creating hundreds of background renderers.

Road messages are editable without changing C# in
`Assets/Resources/road_events.json`. Each entry has an ID, trigger distance,
message, color, and display duration; this is the extension point for later
radio reports, memories, accusations, and ambiguous story fragments.

Checkpoint pacing, checkpoint audio, and immediate checkpoint actions are
editable in `Assets/Resources/anomaly_checkpoints.json`. See
[`ANOMALY_AUTHORING.md`](ANOMALY_AUTHORING.md) for exact fields, valid actions,
audio paths, code ownership, and test keys.

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

When no road prefab is assigned, the prototype uses its original procedural
8-by-80 road. `Assets/Resources/Road/RoadAlbedo.png` supplies a generated,
top-down dark asphalt surface, tiled once across the width and ten times down
each chunk. The material is non-metallic with very low smoothness, preventing
grey aggregate and repaired patches from producing broad headlight glare. Its
broken centre stripes remain separate neutral-white geometry. The
experimental cleaned road remains under `Assets/Resources/Models/Road` for a
future axis-corrected import, but it is deliberately not loaded by default.

The production asset workflow and first three generation batches are described
in `ASSET_PLAN.md`.

## Building

Use **File > Build Profiles** and select Windows x86-64. A generated local
`Assets/Scenes/Prototype.unity` can be included after running the editor scene
builder; the committed `SampleScene.unity` is also a valid build entry because
the runtime bootstrap creates the prototype systems itself. The generated
materials use this project's Universal Render Pipeline.
URP Lit and Unlit are explicitly retained in Graphics Settings because runtime
geometry resolves them by name. Transparent image layers use the dedicated
`LastPassenger/TransparentTexture` shader, which is both loaded from Resources
and explicitly retained in Graphics Settings so standalone builds cannot strip
its alpha-blending pass. The controls support the Input System configuration.

## Scope boundaries

This prototype does not decide who killed whom, why it happened, or whether
the final interpretation is supernatural. Those details can be layered onto
the configurable road-event structure after the driving loop is accepted.
