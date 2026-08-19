# Team pine runtime packaging record

- Source: team-supplied reduced pine model on `main`
- Source commit: `13a0cf3`
- Source path: `Assets/Models/tree.fbx`
- Source SHA-256: `e56b34a00127f7b698a2172af14bb272680ae6911a50e18f467e11bb148fffda`
- Runtime path: `Assets/Resources/Models/Trees/TeamPineRuntime.fbx`
- Runtime SHA-256: `e56b34a00127f7b698a2172af14bb272680ae6911a50e18f467e11bb148fffda`
- Packaged: 2026-08-19

The binary is copied without geometry changes. Moving the runtime copy under
`Resources` makes it an explicit standalone-build dependency. `RoadGenerator`
loads it only when the serialized Pine Tree Prefab slot is empty and applies
the existing packaged `EvergreenTexture.png` at runtime. Serialized tree
prefabs continue to take priority.
