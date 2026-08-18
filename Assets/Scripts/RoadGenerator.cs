using System.Collections.Generic;
using UnityEngine;

namespace LastPassenger
{
    public sealed class RoadGenerator : MonoBehaviour
    {
        private const int ChunkCount = 16;
        private const float ChunkLength = 80f;
        private const float RoadWidth = 8f;

        private readonly List<Transform> chunks = new List<Transform>();
        private Transform vehicle;
        private Material roadMaterial;
        private Material lineMaterial;
        private Material dirtMaterial;
        private Material barkMaterial;
        private Material branchMaterial;
        private Material reflectorMaterial;

        public void Build(Transform vehicleTransform)
        {
            vehicle = vehicleTransform;
            roadMaterial = RuntimeGeometry.Material("Wet black asphalt", new Color(0.035f, 0.04f, 0.045f), 0.05f, 0.32f);
            lineMaterial = RuntimeGeometry.Material("Faded lane paint", new Color(0.46f, 0.43f, 0.29f), 0f, 0.08f);
            dirtMaterial = RuntimeGeometry.Material("Night soil", new Color(0.018f, 0.024f, 0.018f));
            barkMaterial = RuntimeGeometry.Material("Dead bark", new Color(0.075f, 0.055f, 0.045f));
            branchMaterial = RuntimeGeometry.Material("Dead needles", new Color(0.025f, 0.05f, 0.035f));
            reflectorMaterial = RuntimeGeometry.Material("Cold reflector", new Color(0.4f, 0.72f, 0.76f), 0f, 0.35f, true);

            for (int i = 0; i < ChunkCount; i++)
            {
                chunks.Add(CreateChunk(i, i * ChunkLength));
            }

            BuildJunction(650f);
        }

        private Transform CreateChunk(int index, float z)
        {
            GameObject chunk = RuntimeGeometry.Empty($"Repeating road {index:00}", transform, new Vector3(0f, 0f, z));

            RuntimeGeometry.Primitive("Road", PrimitiveType.Cube, chunk.transform,
                new Vector3(0f, -0.12f, ChunkLength * 0.5f), new Vector3(RoadWidth, 0.22f, ChunkLength), roadMaterial);
            RuntimeGeometry.Primitive("Left shoulder", PrimitiveType.Cube, chunk.transform,
                new Vector3(-9f, -0.3f, ChunkLength * 0.5f), new Vector3(10f, 0.35f, ChunkLength), dirtMaterial);
            RuntimeGeometry.Primitive("Right shoulder", PrimitiveType.Cube, chunk.transform,
                new Vector3(9f, -0.3f, ChunkLength * 0.5f), new Vector3(10f, 0.35f, ChunkLength), dirtMaterial);

            for (float markZ = 8f; markZ < ChunkLength; markZ += 12f)
            {
                RuntimeGeometry.Primitive("Broken centre line", PrimitiveType.Cube, chunk.transform,
                    new Vector3(0f, 0.015f, markZ), new Vector3(0.09f, 0.018f, 5.5f), lineMaterial);
            }

            System.Random random = new System.Random(7331 + index * 109);
            for (int i = 0; i < 9; i++)
            {
                float localZ = 4f + (float)random.NextDouble() * (ChunkLength - 8f);
                float side = i % 2 == 0 ? -1f : 1f;
                float x = side * (6f + (float)random.NextDouble() * 7f);
                float height = 2.5f + (float)random.NextDouble() * 3.5f;
                bool isPine = random.NextDouble() < 0.8;
                BuildTree(chunk.transform, new Vector3(x, 0f, localZ), height, isPine);
            }

            return chunk.transform;
        }

        private void BuildTree(Transform parent, Vector3 position, float height, bool isPine)
        {
            GameObject tree = RuntimeGeometry.Empty(
                isPine ? "Generated pine tree" : "Generated leafless tree",
                parent,
                position);
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

        private void BuildJunction(float z)
        {
            GameObject junction = RuntimeGeometry.Empty("Junction decision gate", transform, new Vector3(0f, 0f, z));

            RuntimeGeometry.Primitive("Left gate post", PrimitiveType.Cube, junction.transform,
                new Vector3(-2.1f, 0.8f, 0f), new Vector3(0.18f, 1.6f, 0.18f), reflectorMaterial);
            RuntimeGeometry.Primitive("Right gate post", PrimitiveType.Cube, junction.transform,
                new Vector3(2.1f, 0.8f, 0f), new Vector3(0.18f, 1.6f, 0.18f), reflectorMaterial);

            BuildArrowSign(junction.transform, -2.3f, "LEFT — CREMATORIUM", -1f);
            BuildArrowSign(junction.transform, 2.3f, "RIGHT — OLD QUARRY", 1f);

            for (int i = 0; i < 10; i++)
            {
                float t = i / 9f;
                float zOffset = 8f + t * 42f;
                float spread = t * 4.2f;
                RuntimeGeometry.Primitive("Left fork reflector", PrimitiveType.Cube, junction.transform,
                    new Vector3(-spread, 0.04f, zOffset), new Vector3(0.08f, 0.025f, 3f), lineMaterial,
                    new Vector3(0f, -8f, 0f));
                RuntimeGeometry.Primitive("Right fork reflector", PrimitiveType.Cube, junction.transform,
                    new Vector3(spread, 0.04f, zOffset), new Vector3(0.08f, 0.025f, 3f), lineMaterial,
                    new Vector3(0f, 8f, 0f));
            }
        }

        private void BuildArrowSign(Transform parent, float x, string label, float direction)
        {
            GameObject sign = RuntimeGeometry.Empty(label, parent, new Vector3(x, 0f, -8f));
            RuntimeGeometry.Primitive("Post", PrimitiveType.Cube, sign.transform,
                new Vector3(0f, 1.25f, 0f), new Vector3(0.12f, 2.5f, 0.12f), barkMaterial);
            RuntimeGeometry.Primitive(label, PrimitiveType.Cube, sign.transform,
                new Vector3(0f, 2.25f, 0f), new Vector3(1.45f, 0.58f, 0.12f), lineMaterial,
                new Vector3(0f, direction * 7f, 0f));
        }

        private void Update()
        {
            if (vehicle == null || chunks.Count == 0) return;

            float furthestZ = float.MinValue;
            for (int i = 0; i < chunks.Count; i++) furthestZ = Mathf.Max(furthestZ, chunks[i].position.z);

            for (int i = 0; i < chunks.Count; i++)
            {
                Transform chunk = chunks[i];
                if (vehicle.position.z - chunk.position.z > ChunkLength)
                {
                    furthestZ += ChunkLength;
                    Vector3 position = chunk.position;
                    position.z = furthestZ;
                    chunk.position = position;
                }
            }
        }
    }
}
