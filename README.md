# The Last Passenger — Unity Prototype

A self-contained first-person driving-horror prototype for Unity 6. The road,
car interior, covered body, mirror feed, lighting, fog, signs, and scenery are
generated at runtime. Team-supplied audio is stored directly in the project;
no external Unity packages are needed.

## Open and run

1. Add this folder to Unity Hub and open it with Unity `6000.5.8f1` (the
   repository's current editor version), or let Unity Hub install that version.
2. Wait for scripts and assets to import.
3. Open `Assets/Scenes/MainMenu.unity` and press Play.
4. Click **Play** to load `Assets/Scenes/Prototype.unity` through
   `SceneManager.LoadScene`. The menu is build scene 0 and gameplay is scene 1.

The **Tools > The Last Passenger** menu contains shortcuts for opening either
scene and repairing their Build Settings order.

## Controls

- `W` / Up Arrow — accelerate
- `S` / Down Arrow — brake
- `A` / `D` — steer within the road
- Hold right mouse button and move the mouse — look around the cabin
- Hold `R` — enlarge and inspect the rear-view mirror
- `M` — toggle the radio
- `Enter` — start from the menu or restart gameplay after an ending
- `Escape` — return from gameplay to the main menu; quit from the menu

Debug shortcuts are available only in the editor or development builds:

- `F9` — jump near the junction
- `F10` — jump near the mirror anomaly
- `F11` — jump near the ending

## Prototype flow

The radio warns that "the dead keep left." At the fork, position the car in
the left or right lane before crossing the decision line. A mirror anomaly
occurs later on either route. The right route ends in failure; the left route
reaches the temporary delivery ending.

Road messages are editable without changing C# in
`Assets/Resources/road_events.json`. Each entry has an ID, trigger distance,
message, color, and display duration; this is the extension point for later
radio reports, memories, accusations, and ambiguous story fragments.

Steering now rotates the vehicle and moves it along its heading instead of
sliding it sideways. A transparent 2D wheel follows steering input in front of
a lowered, widened, slanted dashboard with physical side trim. A procedural
front passenger seat supplies real parallax when looking right. The rear-view
camera renders a generated dark backseat plate, then reveals an independently
flickering transparent white-grain apparition during the anomaly. The radio
display flickers and changes during the same event. The team-supplied static is installed at
`Assets/Resources/Audio/RadioStatic.mp3`. The uploaded engine loop replaces the
procedural engine when available, and the uploaded menu theme is streamed only
while the title screen is open. Procedural radio and engine clips remain as
safe fallbacks.

The gameplay bootstrap exists only in `Prototype.unity`; loading the menu can
no longer create the road or car accidentally. It disables the placeholder
scene camera before creating the driver camera. The driver viewpoint is aligned
with the wheel and keeps its slight downward neutral pitch while free-look is
used.

Engine and radio are separate 2D audio sources, so they remain layered instead
of replacing one another. Engine volume rises smoothly from 4% at rest to a
hard 50% cap at maximum speed; the normalized static sits at 3% during normal
driving.

## Replacing road and tree assets

Select **Gameplay Bootstrap** in `Prototype.unity`. Its `RoadGenerator`
component exposes three serialized prefab fields:

- **Road Chunk Prefab** — an 80-metre segment whose origin is at the beginning,
  centred on X=0, extending forward along local +Z. Include road, shoulders and
  markings, but leave roadside trees to the generator.
- **Living Tree Prefab** — origin at the trunk base.
- **Dead Tree Prefab** — origin at the trunk base.

Each field is independent. An empty road slot uses the existing procedural
asphalt, while either empty tree slot uses its corresponding procedural tree.
Prefab-authored scale is preserved with mild per-instance scaling and random Y
rotation. The car, camera, dashboard and mirror do not depend on these slots.

The procedural road material can still load
`Assets/Resources/Road/RoadAlbedo.png` when provided.

The production asset workflow and first three generation batches are described
in `ASSET_PLAN.md`.

## Building

Use **File > Build Profiles**, select Windows x86-64, and include
`Assets/Scenes/MainMenu.unity` first and `Assets/Scenes/Prototype.unity` second.
The editor scene utility repairs this order automatically. The generated
materials use this project's Universal Render Pipeline, and the controls
support its Input System configuration.

## Scope boundaries

This prototype does not decide who killed whom, why it happened, or whether
the final interpretation is supernatural. Those details can be layered onto
the configurable road-event structure after the driving loop is accepted.
