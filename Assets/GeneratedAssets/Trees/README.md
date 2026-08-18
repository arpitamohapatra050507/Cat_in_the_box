# Generated tree drop-off

Create one subfolder per Meshy asset, for example:

```text
Trees/
  Pine_01/
    Pine_01.fbx
    Pine_01_BaseColor.png
    Pine_01_Normal.png
    SOURCE.md
```

Use FBX plus PNG textures. Do not add the downloaded ZIP, Blender backup files,
or a GLB unless a GLB importer is added to the project first. Keep each export
comfortably below GitHub's 100 MB per-file limit; a 1024px texture set and a
low-poly/remeshed model should be enough for roadside trees.

Suggested first Meshy prompt:

> Isolated stylized low-poly conifer pine tree for a late-1990s horror driving
> game, tall strong triangular silhouette, dark desaturated needles, rough
> crooked brown trunk, sparse uneven lower branches, slightly unhealthy,
> game-ready topology, no ground, no rocks, no snow, no environment, no text.

Export the pine first. The second asset should use the same style but be a dead,
leafless pine with broken asymmetric branches. Put the exact prompt, generation
date, Meshy model/version, and license or plan information in `SOURCE.md`.
