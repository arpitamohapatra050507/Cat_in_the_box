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
5. Let the game run for at least 20–30 seconds. Confirm oncoming and slower
   traffic appear in either road lane, move in the correct direction, and can
   be steered around. No more than three traffic cars should coexist.
6. Intentionally hit one traffic car. Confirm the impact sound and red border
   flash, then an immediate `HEAD-ON` failure with a broken-glass overlay.
   Restart before continuing.
7. Press `F9`; verify the fork prompt appears and select the left lane.
8. Before `F10`, hold `R` and confirm the fixed generated empty-backseat image
   fills the mirror with no live car geometry or white figure visible. Press
   `F10`, hold `R` again, and verify the grainy white passenger appears and
   flickers as a separate image layer while its appearance sound plays. The
   mirror must remain dark and must not change with the live camera view.
   Keep observing it for roughly 1.1 seconds and confirm it fades progressively
   rather than popping out, while the hands and apparition audio also recede.
   Note that
   `F10` forces the apparition in place; it does not teleport the vehicle.
9. Press `F10` again and do not touch `R`. Confirm hands grow inward from the
   screen edges, apparition audio becomes louder, and the driver dies after
   about three seconds. Repeat once and verify holding `R` is the only way to
   interrupt the threat.
10. Without using `F10`, continue driving through several 30-second checks. An
   apparition has a 50% chance on each check, so a missed interval is valid and
   two apparitions must never overlap.
11. Press `F11`; verify the car drives into the cliff sequence and reaches the
   `NO ROAD BELOW` ending after the short fall.
12. Press Enter from each ending and confirm the scene restarts cleanly.
13. Toggle `M`; confirm the radio audio and animated display turn off and on
    while the independently mixed engine continues to follow vehicle speed.
14. Confirm the generated dashboard is readable and the radio display aligns
    with its blank screen at 1280x720 and 1920x1080. The dashboard should stay
    in the lower portion of the view, slope away from the camera, fill both
    lower side edges, and never cover the road horizon. Confirm the wheel has
    clean transparent edges.
15. Look right and confirm the front passenger seat has visible cushion,
    backrest, headrest, and parallax without blocking the forward road view.

## Truck-chase pass

1. Restart, begin accelerating, then press `F8`. Confirm the horn warning plays
   before the active pursuit begins. `F8` should work without moving the car or
   requiring the natural distance trigger.
2. Confirm two image-based side mirrors fade in and show the pursuing truck.
   They must not render live cabin or road geometry.
3. Confirm three small engine-health bars appear above the dashboard. Hold full
   throttle. Confirm the `DANGER BEHIND — KEEP SPEED` indicator and
   red border respond to proximity, and that the truck stops gaining or becomes
   smaller once speed exceeds roughly 78% of maximum.
4. Release the throttle long enough to lose speed. Confirm the border pulses
   more strongly and the truck grows in both side mirrors. If it reaches the
   player, confirm the truck image rapidly fills the screen, a loud impact
   plays, and the broken-glass `ENGINE DEAD` failure screen appears.
5. Restart and press `F8` again. During the active chase, confirm ordinary
   traffic is removed and reflective barricades spawn in only one of the three
   lane positions. Confirm they appear farther ahead, never overlap into an
   impossible wall, and always leave enough room to evade.
6. Avoid the barricades and maintain full throttle for roughly 30 seconds.
   Confirm the truck and side mirrors fade away, the threat border clears, and
   ordinary traffic resumes on its 7–12-second schedule.
7. Run the chase once more and deliberately strike three barricades. Confirm
   each hit removes exactly one health bar and cuts speed. On the third hit,
   confirm the car stops, the truck catches it, and the full jumpscare/death
   sequence plays. Each barricade must trigger only one impact.
8. Confirm the chase track begins at its intended quiet volume without a
   fade-in, repeats continuously without random volume dips, and starts fading
   only after the chase has ended. Listen for clipping or unwanted muting while
   the engine, quiet radio static,
   horn, apparition cue, and truck-chase audio overlap.

## Prefab override pass

1. Use **Tools > The Last Passenger > Select Prefab Overrides** and confirm the
   scene configuration object exposes Road Chunk, Pine Tree, Leafless Tree,
   Traffic Car, Barricade, and Road Chunk Length fields.
2. Leave all five prefab fields empty, enter Play Mode, and confirm the current
   generated road, both generated tree types, image-backed traffic cars, and
   framed reflective barricades still appear.
3. Assign only a pine prefab, restart Play Mode, and confirm it replaces the
   common trees while the generated road and leafless trees remain.
4. Assign a road prefab with the correct length, restart Play Mode, and confirm
   all ordinary chunks use it, remain gap-free, and continue recycling as the
   car advances.
5. Assign a traffic-car prefab whose root faces `+Z`. Confirm slower traffic
   faces forward, oncoming instances are rotated correctly, and collision still
   works even if the prefab contains colliders or rigidbodies.
6. Assign a barricade prefab, press `F8`, and confirm spawned chase obstacles
   use it and are grounded correctly.
7. Remove the prefab assignments and confirm every procedural/image fallback
   returns independently.
8. Use **Rebuild Prototype Scene** and confirm assigned overrides are preserved.

## Full pacing pass

1. Start without debug shortcuts and complete either route.
2. Record total completion time; target roughly ten minutes at sustained full
   speed, with slower runs taking longer.
3. Confirm the clue "the dead always keep to the left" appears before the fork.
4. Confirm the junction has no road gaps and the repeating chunks never expose empty space.
5. Confirm the truck chase triggers naturally only after the junction, after
   reaching roughly 780 distance units, and after at least 45 seconds in the
   level. Complete it at full throttle without debug shortcuts.
6. Confirm the mirror remains readable at the intended build resolution and
   the apparition's transparent edges do not show a rectangular background.
7. Confirm ordinary traffic and chase barricades do not visibly float, overlap
   the roadside, or tunnel through the player at the intended frame rate.
8. Confirm the red border appears only as a brief collision flash or an active
   truck-proximity pulse, then fades fully when the danger ends.
9. Confirm all messages fit at 1280×720 and 1920×1080.
10. Confirm the dark vignette reduces peripheral visibility without obscuring
    the dashboard, and that the fixed headlights reveal traffic and barricades
    before their collision boxes reach the player.
11. Drive naturally for at least 2–3 minutes. Confirm a black human silhouette
    can appear around a randomized two-minute interval and vanishes harmlessly
    when driven through.
12. Complete the run and confirm both junction choices can reach the cliff,
    that the road-to-cliff transition plays once, and that Enter restarts after
    `NO ROAD BELOW`.
13. During a full run, confirm the attainable speed begins increasing after
    about one minute, reaches roughly 112 km/h by the late run, and makes
    steering harder without causing sudden speed jumps. A clean run should
    still reach the cliff at roughly ten minutes.

## Submission pass

- Build Windows x86-64 with `Prototype.unity` enabled.
- Test the executable on a second machine.
- Confirm there are no missing shader, material, audio, or scene warnings.
- Preserve `ASSET_PROVENANCE.md` with the submission evidence.
- Add a content warning only when later narrative material requires one.
