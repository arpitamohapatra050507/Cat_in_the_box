# Road-template test extraction record

- Source: team-supplied older racing-game road export
- Source branch: `demo`
- Source commit: `b02d5a5`
- Source asset: `Assets/Models/RoadTemplate.fbx`
- Source SHA-256: `bfbc61f72baa24b6afb04e2d2fdb8f87d003cf98c83ca2c732e654a86f8ca691`
- Runtime test asset: `RoadTemplateTestVisual.fbx`
- Runtime SHA-256: `013e6af44a03718141140fcc60b7598f3686def912fd27cccadcfd0375bd5ae3`
- Processed: 2026-08-19 with Blender 5.2

The source contained a short road, desert roadside geometry, and cactus
duplicates. Only the authored `Road` mesh was retained. Its complete width was
normalized to 8 Unity units, its length was stretched to 80 units to match one
repeating prototype chunk, its base was grounded at zero, and it was exported
without cameras, lights, animation, colliders, or the desert scenery.

The source's missing `Color_Grid.png` is not required: runtime code remaps the
Concrete, Strip, and Railing material slots to the prototype's build-safe dark
URP materials.
