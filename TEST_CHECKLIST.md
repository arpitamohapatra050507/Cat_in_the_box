# Prototype Test Checklist

Run this once in the Unity editor and once in the final Windows build.

## Fast editor pass

1. Press Play and confirm the road, cabin, dashboard, body, fog, and mirror appear.
2. Before accelerating, confirm the engine is silent while the radio static is
   quiet. Hold `W`; the engine pitch and volume should rise smoothly over the
   still-audible static, without exceeding 50% source volume. Release `W` or
   brake to a complete halt and confirm the engine returns to silence.
3. Tap and hold steering in both directions; it should ramp smoothly rather than
   snapping sideways. Confirm the road perspective yaws as the car turns and
   that the steering wheel rotates with it. Steer into each shoulder; the car
   must remain constrained and play an impact.
4. Hold the right mouse button and look left, right, up, and down. The view must
   remain within the cabin and return smoothly to the road when released.
5. Press `F9`; verify the fork prompt appears and select the left lane.
6. Before `F10`, hold `R` and confirm the generated empty backseat is visible
   behind the covered body with no white figure present. Press `F10`, hold `R`
   again, and verify the body's position changes and the grainy white passenger
   appears and flickers. The mirror must remain dark instead of clearing to
   daylight blue.
7. Press `F11`; verify the temporary success screen appears.
8. Restart, use `F9`, select the right lane, then use `F10` and `F11`; verify failure.
9. Press Enter from each ending and confirm the scene restarts cleanly.
10. Toggle `M`; confirm the radio audio and animated display turn off and on
    while the independently mixed engine continues to follow vehicle speed.
11. Confirm the generated dashboard is readable and the radio display aligns
    with its blank screen at 1280x720 and 1920x1080. The dashboard should stay
    in the lower portion of the view, slope away from the camera, fill both
    lower side edges, and never cover the road horizon. Confirm the wheel has
    clean transparent edges.
12. Look right and confirm the front passenger seat has visible cushion,
    backrest, headrest, and parallax without blocking the forward road view.

## Full pacing pass

1. Start without debug shortcuts and complete the left route.
2. Record total completion time; target 3–5 minutes for a new player.
3. Confirm the clue "the dead always keep to the left" appears before the fork.
4. Confirm the junction has no road gaps and the repeating chunks never expose empty space.
5. Confirm the mirror remains readable at the intended build resolution and
   the apparition's transparent edges do not show a rectangular background.
6. Confirm all messages fit at 1280×720 and 1920×1080.

## Submission pass

- Build Windows x86-64 with `Prototype.unity` enabled.
- Test the executable on a second machine.
- Confirm there are no missing shader, material, audio, or scene warnings.
- Preserve `ASSET_PROVENANCE.md` with the submission evidence.
- Add a content warning only when later narrative material requires one.
