# Prototype Test Checklist

Run this once in the Unity editor and once in the final Windows build.

## Fast editor pass

1. Press Play and confirm the road, cabin, dashboard, body, fog, and mirror appear.
2. Hold `W`; speed should rise smoothly and the engine pitch should increase.
3. Tap and hold steering in both directions; it should ramp smoothly rather than
   snapping sideways. Steer into each shoulder; the car must remain constrained
   and play an impact.
4. Press `F9`; verify the fork prompt appears and select the left lane.
5. Press `F10`; hold `R` and verify the body's position changes in the mirror.
   The mirror background must remain dark instead of clearing to daylight blue.
6. Press `F11`; verify the temporary success screen appears.
7. Restart, use `F9`, select the right lane, then use `F10` and `F11`; verify failure.
8. Press Enter from each ending and confirm the scene restarts cleanly.

## Full pacing pass

1. Start without debug shortcuts and complete the left route.
2. Record total completion time; target 3–5 minutes for a new player.
3. Confirm the clue "the dead always keep to the left" appears before the fork.
4. Confirm the junction has no road gaps and the repeating chunks never expose empty space.
5. Confirm the mirror remains readable at the intended build resolution.
6. Confirm all messages fit at 1280×720 and 1920×1080.

## Submission pass

- Build Windows x86-64 with `Prototype.unity` enabled.
- Test the executable on a second machine.
- Confirm there are no missing shader, material, audio, or scene warnings.
- Preserve `ASSET_PROVENANCE.md` with the submission evidence.
- Add a content warning only when later narrative material requires one.
