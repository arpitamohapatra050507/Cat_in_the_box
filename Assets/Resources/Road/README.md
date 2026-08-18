# Road texture handoff

`RoadGenerator` automatically loads `Assets/Resources/Road/RoadAlbedo.png` and
tiles it over the procedural road. Until that file exists, the dark material
fallback remains active.

The `demo` branch currently has `Assets/Models/RoadTemplate.fbx`, but its
materials reference this uncommitted Windows-only path:

```text
D:\Applications\Blender\Material\Color_Grid.png
```

Ask the asset author to commit the missing PNG or export a seamless asphalt
albedo. Rename the selected runtime texture to `RoadAlbedo.png`. Do not merge
the entire FBX merely for its material: it also contains desert roadside and
cactus geometry that does not fit the forest scene.
