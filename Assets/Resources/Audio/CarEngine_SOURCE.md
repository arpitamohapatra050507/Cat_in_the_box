# Car engine source

- Added: 2026-08-18
- Source file: `car_engine.mp3`, uploaded to the repository root by a team member
- Runtime asset: `CarEngine.mp3`
- Format: 17.06 seconds, 44.1 kHz, stereo MP3, 320 kbps
- Measured integrated loudness: approximately -23.0 LUFS
- Source/license status: team must record the creator and confirm permission before final submission

Unity imports the runtime clip as mono, decompresses it on load, and loops it
through `VehicleController`. Playback pitch and volume follow vehicle speed. The
procedurally synthesized engine remains a fallback if this resource cannot load.
