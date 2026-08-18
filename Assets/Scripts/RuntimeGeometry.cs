using UnityEngine;

namespace LastPassenger
{
    public static class RuntimeGeometry
    {
        public static Material Material(string name, Color color, float metallic = 0f, float smoothness = 0.15f, bool emissive = false)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
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

        public static Material TexturedMaterial(string name, Texture texture)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Texture");
            Material material = new Material(shader) { name = name, color = Color.white, mainTexture = texture };

            if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", Color.white);
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
            Vector3 localEuler = default)
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
            mesh.uv = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 0f)
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

        public static GameObject Empty(string name, Transform parent, Vector3 localPosition)
        {
            GameObject created = new GameObject(name);
            created.transform.SetParent(parent, false);
            created.transform.localPosition = localPosition;
            return created;
        }
    }
}
