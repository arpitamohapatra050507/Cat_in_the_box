# Road texture handoff

`RoadGenerator` automatically loads `Assets/Resources/Road/RoadAlbedo.png` and
tiles it over the procedural road at a 1-by-10 ratio. The generated texture is
an albedo only; white centre markings remain separate procedural geometry.
Runtime material settings keep it non-metallic and nearly fully rough so the
headlights reveal fine asphalt detail without creating broad grey reflections.

The `demo` branch currently has `Assets/Models/RoadTemplate.fbx`, but its
materials reference this uncommitted Windows-only path:

```text
D:\Applications\Blender\Material\Color_Grid.png
```

The committed generated texture replaces that missing legacy material. Do not
merge the entire FBX merely for its material: it also contains desert roadside
and cactus geometry that does not fit the forest scene.
