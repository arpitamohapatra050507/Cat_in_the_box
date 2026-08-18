# The Last Passenger — Unity Prototype

A self-contained first-person driving-horror prototype for Unity 6. The road,
car interior, covered body, mirror feed, lighting, fog, signs, scenery, and
audio are generated at runtime. No external art or audio packages are needed.

## Open and run

1. Add this folder to Unity Hub and open it with Unity `6000.5.8f1` (the
   repository's current editor version), or let Unity Hub install that version.
2. Wait for scripts to compile. The editor creates `Assets/Scenes/Prototype.unity`
   and adds it to Build Settings automatically.
3. Open that scene if Unity did not do so automatically, then press Play.
4. To rebuild the scene manually, use **Tools > The Last Passenger > Rebuild Prototype Scene**.

## Replace the generated road and trees

Open **Tools > The Last Passenger > Select Prefab Overrides**. Unity selects the
`Prototype asset configuration — assign prefabs here` object in the Prototype
scene. Its Inspector contains these optional serialized fields:

- **Road Chunk Prefab** — replaces every ordinary repeating road chunk.
- **Pine Tree Prefab** — replaces the common roadside fir trees.
- **Leafless Tree Prefab** — replaces the occasional bare trees.
- **Road Chunk Length** — distance between repeated road-prefab instances;
  leave it at `80` unless the custom chunk has a different length.

Each field falls back independently. For example, assigning only the Meshy fir
prefab replaces the pine trees while the generated road and leafless trees stay
in use. Custom trees are automatically scaled to the randomized target height,
centered, and placed on the ground, so arbitrary Meshy export scale is fine.

A road prefab's root must represent the beginning of the chunk at local
`(0, 0, 0)`, face forward along local `+Z`, and extend for **Road Chunk Length**
units. Stop Play Mode before changing a slot, then start it again to rebuild the
runtime road. Rebuilding the Prototype scene preserves all assigned overrides.

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
mirror is fully image-based: it displays a fixed generated dark backseat plate
and never renders the vehicle's live geometry. A separately composited,
transparent white-grain apparition flickers over that image during the anomaly.
The radio display flickers and changes during the same event. The team-supplied static is
installed at `Assets/Resources/Audio/RadioStatic.wav` and plays quietly on its
own 2D audio source. The team-supplied engine at
`Assets/Resources/Audio/CarEngine.mp3` is layered over it on a separate source:
its volume rises smoothly from silence while accelerating, reaches at most 50%
at full speed, and returns to silence when the car stops. Procedural audio is
used if either custom clip is missing.

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
