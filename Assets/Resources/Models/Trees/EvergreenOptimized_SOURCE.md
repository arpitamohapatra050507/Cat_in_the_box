# Evergreen runtime optimization record

- Source: team-supplied low-poly-pine-tree folder on the `demo` branch
- Source mesh commit: `aedabbc`
- Source texture commit: `4fc1dfd`
- Source mesh: `Assets/Models/low-poly-pine-tree/source/Evergreen_Geometry_0801193826_texture/Evergreen_Geometry_0801193826_texture.fbx`
- Source texture: `Assets/Models/low-poly-pine-tree/textures/texture_0.png`
- Source FBX SHA-256: `1878ee0c39f2dd3fd6a3682e021a30dc8cd644a660b9d4b87f4a6d34a2363234`
- Source texture SHA-256: `35c3ea1ac2b49b6772daf5fee5a5891fb98721595c7a74c049b53490fa608a3b`
- Runtime mesh: `EvergreenOptimized.fbx`
- Runtime mesh SHA-256: `86e9f94bd531da756581d42c3bfb9d780278a95b2cf3775c1d8f7d817e6852c1`
- Runtime texture: `EvergreenTexture.png`
- Processed: 2026-08-19 with Blender 5.2

The evaluated source orientation was baked, the mesh was centered and grounded,
and its height was normalized to one unit so the road generator can scale it to
each randomized tree height. The 144,662-polygon source was decimated to 2,892
polygons (2 percent) for repeated roadside use while preserving its UV layout.
Runtime code applies the supplied texture through a dark, headlight-reactive
URP Lit material.
