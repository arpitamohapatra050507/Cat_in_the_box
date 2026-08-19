# Anomaly and checkpoint authoring

Most pacing changes do not require editing C#. The four equal-distance
checkpoints are defined in:

`Assets/Resources/anomaly_checkpoints.json`

Unity packages this file because it is under `Resources`. Stop Play Mode, edit
the JSON, save it, and start the scene again. Each checkpoint supports:

- `id` — unique internal name.
- `progress` — fraction of the 6,000-unit run, from `0.05` to `0.95`.
  The current values `0.2`, `0.4`, `0.6`, and `0.8` are equally spaced.
- `message`, `messageColor`, `displaySeconds` — checkpoint HUD text.
- `audioResource` — path below `Assets/Resources`, without the extension.
  For example, `Audio/Anomalies/Scary1` loads
  `Assets/Resources/Audio/Anomalies/Scary1.mp3`.
- `audioVolume` — one-shot volume from `0` to `1`.
- `apparitionCheckSeconds` and `apparitionChance` — the recurring rear-seat
  roll used after this checkpoint.
- `roadFigureMinimumSeconds` and `roadFigureMaximumSeconds` — randomized delay
  range for the harmless tall figure.
- `action` — an immediate checkpoint event. Supported values are `none`,
  `roadFigure`, `apparition`, and `truck`.

The code clamps apparition checks to at least 8 seconds, chance to at most 70%,
and road-figure delays to at least 45 seconds. Those guardrails keep a typo in
the JSON from turning the road into an uninterrupted scare sequence.

## Add or change checkpoint audio

1. Put the clip under `Assets/Resources/Audio/Anomalies/`.
2. Let Unity create its `.meta` file.
3. Set `audioResource` to `Audio/Anomalies/FileName`, with no `.mp3` or `.wav`.
4. Start conservatively around `audioVolume: 0.2`; test it over the engine,
   radio, and truck track before raising it.
5. Record the source, license, and checksum in `ASSET_PROVENANCE.md`.

## Add a new anomaly type

The relevant runtime files are:

- `Assets/Scripts/PrototypeGameManager.cs` — loads checkpoint JSON, decides
  when each checkpoint fires, and maps the `action` string in
  `TriggerCheckpoint`.
- `Assets/Scripts/AnomalyDirector.cs` — rear-seat apparition and truck chase
  timing, audio, mutual exclusion, and manual triggers.
- `Assets/Scripts/TrafficHazardManager.cs` — ordinary traffic, barricades, and
  the tall road figure.
- `Assets/Scripts/MirrorSystem.cs` — apparition, pursuing-truck, hands, and
  mirror image compositing.
- `Assets/Scripts/PrototypeInput.cs` — temporary test keys.

For a genuinely new action, add one public `Force...` method to the system that
owns the mechanic, then add one `case` to the switch in
`PrototypeGameManager.TriggerCheckpoint`. Keep major events mutually exclusive:
do not start an apparition during the active truck chase, never block every
road lane, and leave a quiet interval after a forced scare.

Simple narrative-only messages remain in
`Assets/Resources/road_events.json`. Their `triggerDistance` is an absolute
world distance rather than a progress fraction.

## Test shortcuts

- `T` / `F8` — truck chase
- `G` / `F10` — rear-seat apparition
- `H` — tall road figure
- `F9` — advance to the next checkpoint
- `Y` / `F11` — cliff ending

These shortcuts work in editor and standalone builds and are deliberately
separate from natural checkpoint pacing.
