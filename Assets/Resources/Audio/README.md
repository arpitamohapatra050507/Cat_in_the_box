# Runtime audio

- `RadioStatic.mp3` loops through the in-car radio. Unity normalizes and imports
  it as mono because the uploaded source is unusually quiet.
- `CarEngine.mp3` loops through the vehicle and follows its speed through pitch
  and volume changes.
- `MenuTheme.mp3` is streamed in stereo and plays only on the title screen.

The radio and engine retain procedural fallbacks. If a loop clicks at its seam,
make a short equal-power crossfade in an audio editor and re-export it under the
same resource name.
