# Frost traffic-car visual extraction record

- Source: team-supplied old racing-game scene export
- Source commit: `5504d70` on the repository's `demo` branch
- Source asset: `Assets/Models/FrostCar.fbx`
- Source SHA-256: `eff6b872b4ef101b2f1872f2703a31fcdeb393cd6c55083317bc2815460b8045`
- Clean runtime asset: `Assets/Resources/Models/Traffic/FrostCarVisual.fbx`
- Clean SHA-256: `f1a0e0bf61d8071df6a56bad0a4161b34d92ca144a3a573b804816ad59bd26fb`
- Processed: 2026-08-19 with Blender 5.2

## Processing

The source FBX was a whole racing-game scene rather than a model-only prefab.
It contained cameras, a light, UI hierarchy, effects, scene helpers, and the
car. Only these nine mesh objects were retained:

- `Body_1`
- `DoorL` and `DoorR`
- `RearWindow` and `Windshield`
- `WheelMesh`, `WheelMesh_1`, `WheelMesh_2`, and `WheelMesh_3`

Their evaluated world transforms were baked into independent meshes. The
combined model was centered, grounded, uniformly scaled to approximately
1.78 units wide by 4.16 units long, and exported as a static FBX facing the
traffic system's forward axis. No geometry was generated or redrawn.

At runtime, imported Standard materials are replaced with the prototype's
URP-safe body, glass, tyre, headlight, and tail-light materials. The traffic
manager remains responsible for motion and logical collision.
