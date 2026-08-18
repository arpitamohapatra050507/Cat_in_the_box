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
