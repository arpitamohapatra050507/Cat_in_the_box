using System.Collections.Generic;
using UnityEngine;

namespace LastPassenger
{
    public sealed class RoadGenerator : MonoBehaviour
    {
        private const int ChunkCount = 16;
        private const float DefaultChunkLength = 80f;
        private const float RoadWidth = 8f;
        private const int NearTreesPerChunk = 16;
        private const int FarTreesPerChunk = 36;

        private readonly List<Transform> chunks = new List<Transform>();
        private Transform vehicle;
        private GameObject roadChunkPrefab;
        private GameObject pineTreePrefab;
        private GameObject leaflessTreePrefab;
        private float chunkLength = DefaultChunkLength;
        private Material roadMaterial;
        private Material lineMaterial;
        private Material dirtMaterial;
        private Material barkMaterial;
        private Material branchMaterial;
        private Material farForestMaterial;
        private Material packagedPineMaterial;
        private bool usingPackagedPine;

        public void Build(
            Transform vehicleTransform,
            PrototypeAssetConfiguration assetConfiguration = null)
        {
            vehicle = vehicleTransform;
            if (assetConfiguration != null)
            {
                roadChunkPrefab = assetConfiguration.RoadChunkPrefab;
                pineTreePrefab = assetConfiguration.PineTreePrefab;
                leaflessTreePrefab = assetConfiguration.LeaflessTreePrefab;
                chunkLength = assetConfiguration.RoadChunkLength;
            }

            // The experimental cleaned team road had an incompatible axis in
            // Unity. Keep its exact old assignments on the stable procedural
            // fallback, but accept every explicitly assigned tree prefab.
            if (HasKnownBrokenRoadAxis(roadChunkPrefab)) roadChunkPrefab = null;

            // The generated Prototype scene is intentionally not required by
            // standalone builds. If it does not provide an Inspector override,
            // load the reduced team tree from Resources so Unity includes the
            // model and its texture as explicit player-build dependencies.
            if (pineTreePrefab == null)
            {
                pineTreePrefab = Resources.Load<GameObject>("Models/Trees/TeamPineRuntime");
                usingPackagedPine = pineTreePrefab != null;
            }

            roadMaterial = RuntimeGeometry.Material(
                "Rough generated asphalt",
                new Color(0.105f, 0.102f, 0.1f),
                metallic: 0f,
                smoothness: 0.055f);
            if (roadMaterial != null)
            {
                if (roadMaterial.HasProperty("_SpecularHighlights"))
                {
                    roadMaterial.SetFloat("_SpecularHighlights", 0f);
                }
                roadMaterial.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
            }
            Texture2D roadTexture = Resources.Load<Texture2D>("Road/RoadAlbedo");
            RuntimeGeometry.ApplyTexture(roadMaterial, roadTexture, new Vector2(1f, 10f));
            lineMaterial = RuntimeGeometry.Material("White lane paint", new Color(0.68f, 0.68f, 0.64f), 0f, 0.08f);
            dirtMaterial = RuntimeGeometry.Material("Night soil", new Color(0.00025f, 0.00032f, 0.00027f));
            barkMaterial = RuntimeGeometry.Material("Dead bark", new Color(0.003f, 0.002f, 0.0015f));
            branchMaterial = RuntimeGeometry.Material("Dead needles", new Color(0.0005f, 0.001f, 0.0007f));
            farForestMaterial = RuntimeGeometry.Material(
                "Batched distant pine silhouettes",
                new Color(0.0015f, 0.0018f, 0.0016f),
                metallic: 0f,
                smoothness: 0f);
            if (farForestMaterial != null && farForestMaterial.HasProperty("_Cull"))
            {
                farForestMaterial.SetFloat("_Cull", 0f);
            }
            Texture2D packagedPineTexture = Resources.Load<Texture2D>("Models/Trees/EvergreenTexture");
            if (usingPackagedPine && packagedPineTexture != null)
            {
                packagedPineMaterial = RuntimeGeometry.Material(
                    "Packaged team pine night material",
                    new Color(0.055f, 0.064f, 0.052f, 1f),
                    metallic: 0f,
                    smoothness: 0.04f);
                RuntimeGeometry.ApplyTexture(packagedPineMaterial, packagedPineTexture, Vector2.one);
            }

            for (int i = 0; i < ChunkCount; i++)
            {
                chunks.Add(CreateChunk(i, i * chunkLength));
            }
        }

        private Transform CreateChunk(int index, float z)
        {
            GameObject chunk = RuntimeGeometry.Empty($"Repeating road {index:00}", transform, new Vector3(0f, 0f, z));

            if (roadChunkPrefab != null)
            {
                GameObject customRoad = Instantiate(roadChunkPrefab, chunk.transform);
                customRoad.name = $"Custom road chunk {index:00}";
                customRoad.transform.localPosition = Vector3.zero;
                customRoad.transform.localRotation = Quaternion.identity;
            }
            else
            {
                BuildProceduralRoadChunk(chunk.transform);
            }

            System.Random random = new System.Random(7331 + index * 109);
            for (int i = 0; i < NearTreesPerChunk; i++)
            {
                float localZ = 4f + (float)random.NextDouble() * (chunkLength - 8f);
                float side = i % 2 == 0 ? -1f : 1f;
                float x = side * (5.4f + (float)random.NextDouble() * 6.4f);
                float height = 3.4f + (float)random.NextDouble() * 3.8f;
                bool isPine = random.NextDouble() < 0.84;
                BuildTree(chunk.transform, new Vector3(x, 0f, localZ), height, isPine);
            }

            if (farForestMaterial != null) BuildFarForest(chunk.transform, random);

            return chunk.transform;
        }

        private void BuildFarForest(Transform chunk, System.Random random)
        {
            List<Vector3> vertices = new List<Vector3>(FarTreesPerChunk * 32);
            List<int> triangles = new List<int>(FarTreesPerChunk * 48);

            for (int i = 0; i < FarTreesPerChunk; i++)
            {
                float side = i % 2 == 0 ? -1f : 1f;
                float x = side * (10.5f + (float)random.NextDouble() * 15.5f);
                float z = 1f + (float)random.NextDouble() * (chunkLength - 2f);
                float height = 5.2f + (float)random.NextDouble() * 4.8f;
                float width = height * (0.38f + (float)random.NextDouble() * 0.1f);
                Vector3 basePosition = new Vector3(x, 0f, z);
                AddPineSilhouette(vertices, triangles, basePosition, Vector3.right, width, height);
                AddPineSilhouette(vertices, triangles, basePosition,
                    new Vector3(0.58f, 0f, 0.82f).normalized, width, height);
            }

            Mesh mesh = new Mesh { name = "Combined texture-free distant forest mesh" };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GameObject forest = new GameObject("Batched texture-free distant pine silhouettes");
            forest.transform.SetParent(chunk, false);
            forest.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = forest.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = farForestMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private static void AddPineSilhouette(
            List<Vector3> vertices,
            List<int> triangles,
            Vector3 basePosition,
            Vector3 horizontal,
            float width,
            float height)
        {
            float trunkHalfWidth = width * 0.055f;
            int trunkStart = vertices.Count;
            vertices.Add(basePosition - horizontal * trunkHalfWidth);
            vertices.Add(basePosition + Vector3.up * (height * 0.35f) - horizontal * trunkHalfWidth);
            vertices.Add(basePosition + Vector3.up * (height * 0.35f) + horizontal * trunkHalfWidth);
            vertices.Add(basePosition + horizontal * trunkHalfWidth);
            triangles.Add(trunkStart);
            triangles.Add(trunkStart + 1);
            triangles.Add(trunkStart + 2);
            triangles.Add(trunkStart);
            triangles.Add(trunkStart + 2);
            triangles.Add(trunkStart + 3);

            float[] baseHeights = { 0.16f, 0.34f, 0.51f, 0.66f };
            float[] apexHeights = { 0.68f, 0.81f, 0.92f, 1f };
            float[] halfWidths = { 0.5f, 0.41f, 0.31f, 0.2f };
            for (int tier = 0; tier < baseHeights.Length; tier++)
            {
                int start = vertices.Count;
                Vector3 tierBase = basePosition + Vector3.up * (height * baseHeights[tier]);
                vertices.Add(tierBase - horizontal * (width * halfWidths[tier]));
                vertices.Add(basePosition + Vector3.up * (height * apexHeights[tier]));
                vertices.Add(tierBase + horizontal * (width * halfWidths[tier]));
                triangles.Add(start);
                triangles.Add(start + 1);
                triangles.Add(start + 2);
            }
        }

        private void BuildProceduralRoadChunk(Transform chunk)
        {
            RuntimeGeometry.Primitive("Road", PrimitiveType.Cube, chunk,
                new Vector3(0f, -0.12f, chunkLength * 0.5f), new Vector3(RoadWidth, 0.22f, chunkLength), roadMaterial);
            RuntimeGeometry.Primitive("Left shoulder", PrimitiveType.Cube, chunk,
                new Vector3(-9f, -0.3f, chunkLength * 0.5f), new Vector3(10f, 0.35f, chunkLength), dirtMaterial);
            RuntimeGeometry.Primitive("Right shoulder", PrimitiveType.Cube, chunk,
                new Vector3(9f, -0.3f, chunkLength * 0.5f), new Vector3(10f, 0.35f, chunkLength), dirtMaterial);

            for (float markZ = 8f; markZ < chunkLength; markZ += 12f)
            {
                RuntimeGeometry.Primitive("Broken centre line", PrimitiveType.Cube, chunk,
                    new Vector3(0f, 0.015f, markZ), new Vector3(0.09f, 0.018f, 5.5f), lineMaterial);
            }
        }

        private void BuildTree(Transform parent, Vector3 position, float height, bool isPine)
        {
            GameObject tree = RuntimeGeometry.Empty(
                isPine ? "Roadside pine tree" : "Roadside leafless tree",
                parent,
                position);
            tree.transform.localEulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
            GameObject treePrefab = isPine ? pineTreePrefab : leaflessTreePrefab;
            if (treePrefab != null)
            {
                GameObject instance = BuildPrefabTree(tree.transform, treePrefab, height);
                if (isPine && usingPackagedPine && instance != null && packagedPineMaterial != null)
                {
                    ApplyMaterial(instance, packagedPineMaterial);
                }
                return;
            }

            RuntimeGeometry.Primitive("Trunk", PrimitiveType.Cylinder, tree.transform,
                new Vector3(0f, height * 0.5f, 0f), new Vector3(0.18f, height * 0.5f, 0.18f), barkMaterial,
                new Vector3(0f, 0f, Random.Range(-5f, 5f)));

            if (isPine)
            {
                BuildPineCrown(tree.transform, height);
                return;
            }

            for (int i = 0; i < 3; i++)
            {
                float y = height * (0.48f + i * 0.17f);
                float direction = i % 2 == 0 ? -1f : 1f;
                RuntimeGeometry.Primitive("Branch", PrimitiveType.Cylinder, tree.transform,
                    new Vector3(direction * 0.45f, y, 0f), new Vector3(0.07f, 0.62f, 0.07f), branchMaterial,
                    new Vector3(0f, 0f, direction * 52f));
            }
        }

        private static GameObject BuildPrefabTree(Transform parent, GameObject prefab, float targetHeight)
        {
            GameObject instance = Instantiate(prefab, parent);
            instance.name = $"Custom {prefab.name}";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return instance;

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            if (bounds.size.y <= 0.001f) return instance;

            float uniformScale = targetHeight / bounds.size.y;
            instance.transform.localScale = Vector3.one * uniformScale;

            renderers = instance.GetComponentsInChildren<Renderer>(true);
            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);

            Vector3 desiredBase = parent.position;
            instance.transform.position += new Vector3(
                desiredBase.x - bounds.center.x,
                desiredBase.y - bounds.min.y,
                desiredBase.z - bounds.center.z);
            return instance;
        }

        private static void ApplyMaterial(GameObject root, Material material)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].sharedMaterial = material;
            }
        }

        private static bool HasKnownBrokenRoadAxis(GameObject prefab)
        {
            if (prefab == null) return false;
            return prefab.name.Equals("RoadTemplate", System.StringComparison.OrdinalIgnoreCase) ||
                   prefab.name.Equals("RoadTemplateTestVisual", System.StringComparison.OrdinalIgnoreCase);
        }

        private void BuildPineCrown(Transform tree, float height)
        {
            for (int i = 0; i < 4; i++)
            {
                float t = i / 3f;
                float width = Mathf.Lerp(height * 0.25f, height * 0.09f, t);
                float crownHeight = Mathf.Lerp(height * 0.24f, height * 0.16f, t);
                float y = height * Mathf.Lerp(0.43f, 0.82f, t);

                RuntimeGeometry.Primitive(
                    "Pine foliage tier",
                    PrimitiveType.Sphere,
                    tree,
                    new Vector3(0f, y, 0f),
                    new Vector3(width, crownHeight, width),
                    branchMaterial);
            }
        }

        private void Update()
        {
            if (vehicle == null || chunks.Count == 0) return;

            float furthestZ = float.MinValue;
            for (int i = 0; i < chunks.Count; i++) furthestZ = Mathf.Max(furthestZ, chunks[i].position.z);

            for (int i = 0; i < chunks.Count; i++)
            {
                Transform chunk = chunks[i];
                if (vehicle.position.z - chunk.position.z > chunkLength)
                {
                    furthestZ += chunkLength;
                    Vector3 position = chunk.position;
                    position.z = furthestZ;
                    chunk.position = position;
                }
            }
        }
    }
}
