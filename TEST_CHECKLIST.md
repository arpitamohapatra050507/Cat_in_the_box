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
6. Before `F10`, hold `R` and confirm the fixed generated empty-backseat image
   fills the mirror with no live car geometry or white figure visible. Press
   `F10`, hold `R` again, and verify the grainy white passenger appears and
   flickers as a separate image layer. The mirror must remain dark and must not
   change with the live camera view.
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

## Prefab override pass

1. Use **Tools > The Last Passenger > Select Prefab Overrides** and confirm the
   scene configuration object exposes Road Chunk, Pine Tree, Leafless Tree, and
   Road Chunk Length fields.
2. Leave all three prefab fields empty, enter Play Mode, and confirm the current
   generated road and both generated tree types still appear.
3. Assign only a pine prefab, restart Play Mode, and confirm it replaces the
   common trees while the generated road and leafless trees remain.
4. Assign a road prefab with the correct length, restart Play Mode, and confirm
   all ordinary chunks use it, remain gap-free, and continue recycling as the
   car advances.
5. Remove the prefab assignments and confirm the procedural fallbacks return.
6. Use **Rebuild Prototype Scene** and confirm assigned overrides are preserved.

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
