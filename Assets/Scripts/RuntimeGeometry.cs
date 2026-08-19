using UnityEngine;

namespace LastPassenger
{
    public static class RuntimeGeometry
    {
        public static Material Material(string name, Color color, float metallic = 0f, float smoothness = 0.15f, bool emissive = false)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null)
            {
                Debug.LogError("The runtime Lit shader is missing. Keep URP/Lit in Graphics Settings > Always Included Shaders.");
                return null;
            }
            Material material = new Material(shader) { name = name, color = color };

            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", smoothness);

            if (emissive && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 2.4f);
            }

            return material;
        }

        public static Material TexturedMaterial(string name, Texture texture, bool transparent = false)
        {
            return TexturedMaterial(name, texture, Color.white, transparent);
        }

        public static Material TexturedMaterial(
            string name,
            Texture texture,
            Color tint,
            bool transparent = false)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Texture");
            if (shader == null)
            {
                Debug.LogError("The runtime Unlit shader is missing. Keep URP/Unlit in Graphics Settings > Always Included Shaders.");
                return null;
            }
            Material material = new Material(shader) { name = name, color = tint, mainTexture = texture };

            if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", tint);

            if (transparent)
            {
                material.SetOverrideTag("RenderType", "Transparent");
                if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
                if (material.HasProperty("_Blend")) material.SetFloat("_Blend", 0f);
                if (material.HasProperty("_SrcBlend"))
                {
                    material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                }
                if (material.HasProperty("_DstBlend"))
                {
                    material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                }
                if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0f);
                if (material.HasProperty("_Cull")) material.SetFloat("_Cull", 0f);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }

            return material;
        }

        public static void ApplyTexture(Material material, Texture texture, Vector2 tiling)
        {
            if (material == null || texture == null) return;

            texture.wrapMode = TextureWrapMode.Repeat;
            material.mainTexture = texture;
            material.mainTextureScale = tiling;
            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", texture);
                material.SetTextureScale("_BaseMap", tiling);
            }
        }

        public static GameObject TexturedQuad(
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector2 localSize,
            Material material,
            Vector3 localEuler = default,
            bool flipHorizontal = false,
            bool flipVertical = false)
        {
            GameObject created = new GameObject(name);
            created.transform.SetParent(parent, false);
            created.transform.localPosition = localPosition;
            created.transform.localEulerAngles = localEuler;
            created.transform.localScale = new Vector3(localSize.x, localSize.y, 1f);

            Mesh mesh = new Mesh { name = $"{name} mesh" };
            mesh.vertices = new[]
            {
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f),
                new Vector3(0.5f, -0.5f, 0f)
            };
            float left = flipHorizontal ? 1f : 0f;
            float right = flipHorizontal ? 0f : 1f;
            float bottom = flipVertical ? 1f : 0f;
            float top = flipVertical ? 0f : 1f;
            mesh.uv = new[]
            {
                new Vector2(left, bottom),
                new Vector2(left, top),
                new Vector2(right, top),
                new Vector2(right, bottom)
            };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            created.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = created.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return created;
        }

        public static GameObject Primitive(
            string name,
            PrimitiveType primitiveType,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Material material,
            Vector3 localEuler = default,
            bool keepCollider = false)
        {
            GameObject created = GameObject.CreatePrimitive(primitiveType);
            created.name = name;
            created.transform.SetParent(parent, false);
            created.transform.localPosition = localPosition;
            created.transform.localEulerAngles = localEuler;
            created.transform.localScale = localScale;

            Renderer renderer = created.GetComponent<Renderer>();
            if (renderer != null && material != null) renderer.sharedMaterial = material;

            Collider collider = created.GetComponent<Collider>();
            if (collider != null && !keepCollider) Object.Destroy(collider);
            return created;
        }

        public static GameObject TaperedBox(
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector2 bottomSize,
            Vector2 topSize,
            float height,
            Material material)
        {
            GameObject created = new GameObject(name);
            created.transform.SetParent(parent, false);
            created.transform.localPosition = localPosition;

            float bottomHalfWidth = bottomSize.x * 0.5f;
            float bottomHalfDepth = bottomSize.y * 0.5f;
            float topHalfWidth = topSize.x * 0.5f;
            float topHalfDepth = topSize.y * 0.5f;
            float halfHeight = height * 0.5f;

            Mesh mesh = new Mesh { name = $"{name} mesh" };
            mesh.vertices = new[]
            {
                new Vector3(-bottomHalfWidth, -halfHeight, -bottomHalfDepth),
                new Vector3(bottomHalfWidth, -halfHeight, -bottomHalfDepth),
                new Vector3(bottomHalfWidth, -halfHeight, bottomHalfDepth),
                new Vector3(-bottomHalfWidth, -halfHeight, bottomHalfDepth),
                new Vector3(-topHalfWidth, halfHeight, -topHalfDepth),
                new Vector3(topHalfWidth, halfHeight, -topHalfDepth),
                new Vector3(topHalfWidth, halfHeight, topHalfDepth),
                new Vector3(-topHalfWidth, halfHeight, topHalfDepth)
            };
            mesh.triangles = new[]
            {
                0, 4, 5, 0, 5, 1,
                1, 5, 6, 1, 6, 2,
                2, 6, 7, 2, 7, 3,
                3, 7, 4, 3, 4, 0,
                4, 7, 6, 4, 6, 5,
                3, 0, 1, 3, 1, 2
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            created.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = created.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            return created;
        }

        public static GameObject Empty(string name, Transform parent, Vector3 localPosition)
        {
            GameObject created = new GameObject(name);
            created.transform.SetParent(parent, false);
            created.transform.localPosition = localPosition;
            return created;
        }
    }
}
