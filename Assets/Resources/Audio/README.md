# Radio static handoff

Put the final static loop at `Assets/Resources/Audio/RadioStatic.wav`. The game
loads that exact resource automatically and falls back to the synthesized
static until it exists. A loopable WAV is preferred; keep the peak level below
clipping and leave a short crossfade at the loop boundary if needed.

Checkpoint and anomaly clips live in `Audio/Anomalies`. The four-run checkpoints
load their optional one-shots by the resource paths configured in
`Assets/Resources/anomaly_checkpoints.json`; see `ANOMALY_AUTHORING.md`.
