# Radio static handoff

Put the final static loop at `Assets/Resources/Audio/RadioStatic.wav`. The game
loads that exact resource automatically and falls back to the synthesized
static until it exists. A loopable WAV is preferred; keep the peak level below
clipping and leave a short crossfade at the loop boundary if needed.
