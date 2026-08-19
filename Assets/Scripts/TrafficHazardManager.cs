using System;
using System.Collections.Generic;
using UnityEngine;

namespace LastPassenger
{
    /// <summary>
    /// Spawns lightweight road hazards and resolves them without relying on
    /// Unity physics. The player vehicle is transform-driven, so each hazard
    /// tests its previous and current relative X/Z positions against a logical
    /// collision box. This also prevents fast oncoming cars tunnelling through
    /// the player between frames.
    /// </summary>
    public sealed class TrafficHazardManager : MonoBehaviour
    {
        private enum HazardKind
        {
            OncomingCar,
            SlowerCar,
            ChaseBarricade,
            RoadFigure
        }

        private sealed class Hazard
        {
            public GameObject root;
            public HazardKind kind;
            public float forwardSpeed;
            public float collisionHalfWidth;
            public float collisionHalfLength;
            public Vector2 previousRelativePosition;
        }

        private const int MaximumOrdinaryTraffic = 3;
        private const float LeftLane = -1.55f;
        private const float RightLane = 1.55f;
        private const float SpawnIntervalMinimum = 7f;
        private const float SpawnIntervalMaximum = 12f;
        private const float MinimumBarricadeSpacing = 27f;

        private readonly List<Hazard> hazards = new List<Hazard>();

        private VehicleController vehicle;
        private PrototypeGameManager manager;
        private PrototypeAssetConfiguration assetConfiguration;
        private GameObject defaultTrafficModel;
        private float nextTrafficSpawnTime;
        private float nextRoadFigureTime;
        private bool chaseActive;
        private bool finalStateCleared;
        private bool junctionQuietActive;
        private int lastBarricadeLaneIndex = -1;

        private Material carBodyMaterial;
        private Material carGlassMaterial;
        private Material tyreMaterial;
        private Material wheelHubMaterial;
        private Material headlightMaterial;
        private Material tailLightMaterial;
        private Material oncomingImageMaterial;
        private Material rearImageMaterial;
        private Material barricadeImageMaterial;
        private Material barricadeFrameMaterial;
        private Material barricadeDarkMaterial;
        private Material sideImageMaterial;
        private Material topImageMaterial;
        private Material roadFigureMaterial;

        public event Action BarricadeHit;

        public void Configure(
            VehicleController vehicleController,
            PrototypeGameManager gameManager,
            PrototypeAssetConfiguration config)
        {
            ClearAllHazards();
            vehicle = vehicleController;
            manager = gameManager;
            assetConfiguration = config;
            defaultTrafficModel = Resources.Load<GameObject>("Models/Traffic/FrostCarVisual");
            chaseActive = false;
            finalStateCleared = false;
            junctionQuietActive = false;
            lastBarricadeLaneIndex = -1;
            ScheduleNextTrafficSpawn();
            ScheduleNextRoadFigure();
            BuildFallbackMaterials();
        }

        public void SetChaseActive(bool active)
        {
            if (chaseActive == active) return;

            chaseActive = active;
            if (active)
            {
                ClearHazards(HazardKind.OncomingCar, HazardKind.SlowerCar, HazardKind.RoadFigure);
            }
            else
            {
                ClearHazards(HazardKind.ChaseBarricade);
                ScheduleNextTrafficSpawn();
            }
        }

        public void SpawnChaseBarricade()
        {
            if (!chaseActive || vehicle == null) return;

            float z = vehicle.transform.position.z + UnityEngine.Random.Range(52f, 62f);
            if (HasBarricadeNear(z, MinimumBarricadeSpacing)) return;

            float[] lanes = { -1.8f, 0f, 1.8f };
            int laneIndex = UnityEngine.Random.Range(0, lanes.Length);
            if (laneIndex == lastBarricadeLaneIndex)
            {
                laneIndex = (laneIndex + UnityEngine.Random.Range(1, lanes.Length)) % lanes.Length;
            }
            lastBarricadeLaneIndex = laneIndex;

            GameObject root;
            float collisionHalfWidth = 1.38f;
            float collisionHalfLength = 0.72f;

            GameObject barricadePrefab = assetConfiguration != null
                ? assetConfiguration.BarricadePrefab
                : null;
            if (barricadePrefab != null)
            {
                root = InstantiateLogicalPrefab(
                    barricadePrefab,
                    "Chase barricade",
                    new Vector3(lanes[laneIndex], 0f, z),
                    Quaternion.identity);
                MeasureLogicalCollision(root, 0.48f, 0.38f, out collisionHalfWidth, out collisionHalfLength);
                collisionHalfWidth = Mathf.Min(collisionHalfWidth, 1.45f);
                collisionHalfLength = Mathf.Min(collisionHalfLength, 1.1f);
            }
            else
            {
                root = BuildFallbackBarricade(new Vector3(lanes[laneIndex], 0f, z));
            }

            AddHazard(root, HazardKind.ChaseBarricade, 0f, collisionHalfWidth, collisionHalfLength);
        }

        private bool HasBarricadeNear(float worldZ, float minimumSpacing)
        {
            for (int i = 0; i < hazards.Count; i++)
            {
                Hazard hazard = hazards[i];
                if (hazard.kind == HazardKind.ChaseBarricade && hazard.root != null &&
                    Mathf.Abs(hazard.root.transform.position.z - worldZ) < minimumSpacing)
                {
                    return true;
                }
            }
            return false;
        }

        private void Update()
        {
            if (vehicle == null || manager == null) return;

            bool runFinished = !manager.IsGameplayActive;
            if (runFinished)
            {
                if (!finalStateCleared)
                {
                    ClearAllHazards();
                    finalStateCleared = true;
                }
                return;
            }

            finalStateCleared = false;
            bool junctionQuiet = vehicle.Distance >= 500f && vehicle.Distance <= 720f;
            if (junctionQuiet && !junctionQuietActive)
            {
                ClearHazards(HazardKind.OncomingCar, HazardKind.SlowerCar, HazardKind.RoadFigure);
            }
            else if (!junctionQuiet && junctionQuietActive)
            {
                ScheduleNextTrafficSpawn();
            }
            junctionQuietActive = junctionQuiet;

            if (!chaseActive && !junctionQuiet && Time.time >= nextTrafficSpawnTime)
            {
                if (CountOrdinaryTraffic() < MaximumOrdinaryTraffic)
                {
                    SpawnOrdinaryTraffic();
                }
                ScheduleNextTrafficSpawn();
            }

            if (!chaseActive && !junctionQuiet && Time.time >= nextRoadFigureTime)
            {
                SpawnRoadFigure();
                ScheduleNextRoadFigure();
            }

            UpdateHazards();
        }

        private void SpawnOrdinaryTraffic()
        {
            bool oncoming = UnityEngine.Random.value < 0.58f;
            float lane = UnityEngine.Random.value < 0.5f ? LeftLane : RightLane;
            float distanceAhead = oncoming
                ? UnityEngine.Random.Range(88f, 125f)
                : UnityEngine.Random.Range(68f, 102f);
            float z = vehicle.transform.position.z + distanceAhead;
            float forwardSpeed = oncoming
                ? -UnityEngine.Random.Range(6.5f, 9.5f)
                : UnityEngine.Random.Range(3.2f, 5.4f);

            HazardKind kind = oncoming ? HazardKind.OncomingCar : HazardKind.SlowerCar;
            GameObject root;
            // Includes the player's logical half-width as well as the image-backed car.
            // A player straddling the centre line must therefore choose a clear lane.
            float collisionHalfWidth = 1.72f;
            float collisionHalfLength = 2.2f;
            GameObject configuredTrafficPrefab = assetConfiguration != null
                ? assetConfiguration.TrafficCarPrefab
                : null;
            bool oldWholeSceneFrostAsset = configuredTrafficPrefab != null &&
                configuredTrafficPrefab.name.Equals("FrostCar", StringComparison.OrdinalIgnoreCase);
            bool useBuiltInTrafficModel = configuredTrafficPrefab == null || oldWholeSceneFrostAsset;
            GameObject trafficPrefab = useBuiltInTrafficModel && defaultTrafficModel != null
                ? defaultTrafficModel
                : configuredTrafficPrefab;

            if (trafficPrefab != null)
            {
                Quaternion rotation = oncoming
                    ? Quaternion.Euler(0f, 180f, 0f)
                    : Quaternion.identity;
                root = InstantiateLogicalPrefab(
                    trafficPrefab,
                    oncoming ? "Oncoming traffic" : "Slower traffic",
                    new Vector3(lane, 0f, z),
                    rotation);
                if (trafficPrefab == defaultTrafficModel)
                {
                    ApplyBuiltInTrafficMaterials(root, oncoming);
                }
                MeasureLogicalCollision(root, 0.65f, 0.7f, out collisionHalfWidth, out collisionHalfLength);
            }
            else
            {
                root = BuildFallbackCar(new Vector3(lane, 0f, z), oncoming);
            }

            AddHazard(root, kind, forwardSpeed, collisionHalfWidth, collisionHalfLength);
        }

        private void SpawnRoadFigure()
        {
            if (roadFigureMaterial == null || vehicle == null) return;

            float lane = UnityEngine.Random.Range(-2.15f, 2.15f);
            float z = vehicle.transform.position.z + UnityEngine.Random.Range(58f, 76f);
            GameObject root = RuntimeGeometry.Empty(
                "Figure standing in the headlights",
                transform,
                new Vector3(lane, 0f, z));
            RuntimeGeometry.TexturedQuad(
                "Black roadside figure",
                root.transform,
                new Vector3(0f, 1.72f, 0f),
                new Vector2(1.5f, 3.45f),
                roadFigureMaterial);
            AddHazard(root, HazardKind.RoadFigure, 0f, 0.82f, 0.65f);
        }

        private void AddHazard(
            GameObject root,
            HazardKind kind,
            float forwardSpeed,
            float collisionHalfWidth,
            float collisionHalfLength)
        {
            Vector3 relative = root.transform.position - vehicle.transform.position;
            hazards.Add(new Hazard
            {
                root = root,
                kind = kind,
                forwardSpeed = forwardSpeed,
                collisionHalfWidth = collisionHalfWidth,
                collisionHalfLength = collisionHalfLength,
                previousRelativePosition = new Vector2(relative.x, relative.z)
            });
        }

        private void UpdateHazards()
        {
            for (int i = hazards.Count - 1; i >= 0; i--)
            {
                Hazard hazard = hazards[i];
                if (hazard.root == null)
                {
                    hazards.RemoveAt(i);
                    continue;
                }

                Vector3 position = hazard.root.transform.position;
                position.z += hazard.forwardSpeed * Time.deltaTime;
                hazard.root.transform.position = position;

                Vector3 relative3D = position - vehicle.transform.position;
                Vector2 currentRelative = new Vector2(relative3D.x, relative3D.z);
                bool collided = SegmentIntersectsBox(
                    hazard.previousRelativePosition,
                    currentRelative,
                    hazard.collisionHalfWidth,
                    hazard.collisionHalfLength);

                if (collided)
                {
                    ResolveCollision(hazard);
                    Destroy(hazard.root);
                    hazards.RemoveAt(i);
                    continue;
                }

                hazard.previousRelativePosition = currentRelative;
                if (currentRelative.y < -28f || currentRelative.y > 175f)
                {
                    Destroy(hazard.root);
                    hazards.RemoveAt(i);
                }
            }
        }

        private void ResolveCollision(Hazard hazard)
        {
            if (hazard.kind == HazardKind.ChaseBarricade)
            {
                vehicle.ApplyImpact(0.62f);
                manager.NotifyBarricadeCollision();
                BarricadeHit?.Invoke();
                return;
            }

            if (hazard.kind == HazardKind.RoadFigure)
            {
                manager.NotifyRoadFigurePassed();
                return;
            }

            vehicle.ApplyImpact(0f);
            manager.NotifyTrafficCollision();
        }

        private GameObject BuildFallbackCar(Vector3 position, bool oncoming)
        {
            GameObject root = RuntimeGeometry.Empty(
                oncoming ? "Generated oncoming traffic" : "Generated slower traffic",
                transform,
                position);

            RuntimeGeometry.TaperedBox(
                "Tapered lower sedan hull",
                root.transform,
                new Vector3(0f, 0.57f, 0f),
                new Vector2(1.72f, 3.28f),
                new Vector2(1.54f, 3.02f),
                0.56f,
                carBodyMaterial);
            RuntimeGeometry.TaperedBox(
                "Sloped sedan cabin",
                root.transform,
                new Vector3(0f, 1.07f, 0.08f),
                new Vector2(1.43f, 1.92f),
                new Vector2(1.08f, 1.22f),
                0.62f,
                carGlassMaterial);
            RuntimeGeometry.TaperedBox(
                "Thin sedan roof",
                root.transform,
                new Vector3(0f, 1.395f, 0.08f),
                new Vector2(1.09f, 1.24f),
                new Vector2(1.02f, 1.14f),
                0.08f,
                carBodyMaterial);

            RuntimeGeometry.Primitive("Front bumper", PrimitiveType.Cube, root.transform,
                new Vector3(0f, 0.44f, -1.66f), new Vector3(1.58f, 0.12f, 0.09f), carBodyMaterial);
            RuntimeGeometry.Primitive("Rear bumper", PrimitiveType.Cube, root.transform,
                new Vector3(0f, 0.44f, 1.66f), new Vector3(1.58f, 0.12f, 0.09f), carBodyMaterial);

            for (int side = -1; side <= 1; side += 2)
            {
                for (int end = -1; end <= 1; end += 2)
                {
                    RuntimeGeometry.Primitive("Tyre", PrimitiveType.Cylinder, root.transform,
                        new Vector3(side * 0.88f, 0.36f, end * 1.08f),
                        new Vector3(0.66f, 0.13f, 0.66f), tyreMaterial,
                        new Vector3(0f, 0f, 90f));
                    RuntimeGeometry.Primitive("Wheel hub", PrimitiveType.Cylinder, root.transform,
                        new Vector3(side * 0.895f, 0.36f, end * 1.08f),
                        new Vector3(0.34f, 0.14f, 0.34f), wheelHubMaterial,
                        new Vector3(0f, 0f, 90f));
                }
            }

            Material lampMaterial = oncoming ? headlightMaterial : tailLightMaterial;
            RuntimeGeometry.Primitive(oncoming ? "Left headlamp" : "Left tail lamp", PrimitiveType.Cube, root.transform,
                new Vector3(-0.53f, 0.62f, -1.65f), new Vector3(0.38f, 0.2f, 0.07f), lampMaterial);
            RuntimeGeometry.Primitive(oncoming ? "Right headlamp" : "Right tail lamp", PrimitiveType.Cube, root.transform,
                new Vector3(0.53f, 0.62f, -1.65f), new Vector3(0.38f, 0.2f, 0.07f), lampMaterial);

            Material imageMaterial = oncoming ? oncomingImageMaterial : rearImageMaterial;
            if (imageMaterial != null)
            {
                RuntimeGeometry.TexturedQuad(
                    oncoming ? "Oncoming car image" : "Traffic car rear image",
                    root.transform,
                    new Vector3(0f, 0.83f, -1.712f),
                    new Vector2(1.92f, 1.22f),
                    imageMaterial);
            }


            if (sideImageMaterial != null)
            {
                RuntimeGeometry.TexturedQuad(
                    "Traffic car right side image",
                    root.transform,
                    new Vector3(0.902f, 0.78f, 0f),
                    new Vector2(3.32f, 1.31f),
                    sideImageMaterial,
                    new Vector3(0f, -90f, 0f));
                RuntimeGeometry.TexturedQuad(
                    "Traffic car left side image",
                    root.transform,
                    new Vector3(-0.902f, 0.78f, 0f),
                    new Vector2(3.32f, 1.31f),
                    sideImageMaterial,
                    new Vector3(0f, 90f, 0f),
                    flipHorizontal: true);
            }

            if (topImageMaterial != null)
            {
                RuntimeGeometry.TexturedQuad(
                    "Traffic car top hull image",
                    root.transform,
                    new Vector3(0f, 1.443f, 0f),
                    new Vector2(1.72f, 3.3f),
                    topImageMaterial,
                    new Vector3(90f, 0f, 0f),
                    flipVertical: true);
            }

            return root;
        }

        private GameObject BuildFallbackBarricade(Vector3 position)
        {
            GameObject root = RuntimeGeometry.Empty("Generated chase barricade", transform, position);

            RuntimeGeometry.Primitive("Barricade backing", PrimitiveType.Cube, root.transform,
                new Vector3(0f, 0.94f, 0f), new Vector3(2.16f, 1.08f, 0.12f), barricadeDarkMaterial);
            RuntimeGeometry.Primitive("Left frame post", PrimitiveType.Cube, root.transform,
                new Vector3(-1.04f, 0.72f, 0.02f), new Vector3(0.09f, 1.44f, 0.1f), barricadeFrameMaterial);
            RuntimeGeometry.Primitive("Right frame post", PrimitiveType.Cube, root.transform,
                new Vector3(1.04f, 0.72f, 0.02f), new Vector3(0.09f, 1.44f, 0.1f), barricadeFrameMaterial);
            RuntimeGeometry.Primitive("Top frame rail", PrimitiveType.Cube, root.transform,
                new Vector3(0f, 1.48f, 0.02f), new Vector3(2.18f, 0.09f, 0.1f), barricadeFrameMaterial);
            RuntimeGeometry.Primitive("Bottom frame rail", PrimitiveType.Cube, root.transform,
                new Vector3(0f, 0.42f, 0.02f), new Vector3(2.18f, 0.09f, 0.1f), barricadeFrameMaterial);

            RuntimeGeometry.Primitive("Left support foot", PrimitiveType.Cube, root.transform,
                new Vector3(-0.9f, 0.09f, 0f), new Vector3(0.72f, 0.11f, 0.5f), barricadeFrameMaterial);
            RuntimeGeometry.Primitive("Right support foot", PrimitiveType.Cube, root.transform,
                new Vector3(0.9f, 0.09f, 0f), new Vector3(0.72f, 0.11f, 0.5f), barricadeFrameMaterial);

            if (barricadeImageMaterial != null)
            {
                RuntimeGeometry.TexturedQuad("Reflective barricade image", root.transform,
                    new Vector3(0f, 0.95f, -0.071f), new Vector2(2.05f, 1.0f), barricadeImageMaterial);
            }

            return root;
        }

        private GameObject InstantiateLogicalPrefab(
            GameObject prefab,
            string instanceName,
            Vector3 position,
            Quaternion rotation)
        {
            GameObject quarantine = new GameObject("Inactive prefab quarantine");
            quarantine.transform.SetParent(transform, false);
            quarantine.SetActive(false);
            GameObject instance = Instantiate(prefab, position, rotation, quarantine.transform);
            instance.name = instanceName;

            // Traffic prefabs are visual ingredients only. Old racing prefabs may
            // contain player controllers, cameras, audio listeners, lights, or UI
            // behaviours that must never take ownership of this prototype.
            Behaviour[] behaviours = instance.GetComponentsInChildren<Behaviour>(true);
            for (int i = 0; i < behaviours.Length; i++) behaviours[i].enabled = false;

            ParticleSystem[] particleSystems = instance.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < particleSystems.Length; i++)
            {
                particleSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            Collider[] colliders = instance.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++) colliders[i].enabled = false;

            Rigidbody[] rigidbodies = instance.GetComponentsInChildren<Rigidbody>(true);
            for (int i = 0; i < rigidbodies.Length; i++)
            {
                rigidbodies[i].isKinematic = true;
                rigidbodies[i].useGravity = false;
            }

            instance.transform.SetParent(transform, true);
            instance.SetActive(true);
            Destroy(quarantine);
            GroundPrefab(instance);
            return instance;
        }

        private void ApplyBuiltInTrafficMaterials(GameObject instance, bool oncoming)
        {
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                Renderer renderer = renderers[rendererIndex];
                Material[] imported = renderer.sharedMaterials;
                Material[] replacements = new Material[imported.Length];
                bool isWheel = renderer.name.ToLowerInvariant().Contains("wheel");

                for (int materialIndex = 0; materialIndex < imported.Length; materialIndex++)
                {
                    string importedName = imported[materialIndex] != null
                        ? imported[materialIndex].name.ToLowerInvariant()
                        : string.Empty;

                    if (isWheel)
                    {
                        replacements[materialIndex] = tyreMaterial;
                    }
                    else if (importedName.Contains("window"))
                    {
                        replacements[materialIndex] = carGlassMaterial;
                    }
                    else if (importedName.Contains("light"))
                    {
                        replacements[materialIndex] = oncoming ? headlightMaterial : tailLightMaterial;
                    }
                    else
                    {
                        replacements[materialIndex] = carBodyMaterial;
                    }
                }

                renderer.sharedMaterials = replacements;
            }
        }

        private static void GroundPrefab(GameObject instance)
        {
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return;

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);

            Vector3 position = instance.transform.position;
            position.y -= bounds.min.y;
            instance.transform.position = position;
        }

        private static void MeasureLogicalCollision(
            GameObject root,
            float playerHalfWidth,
            float playerHalfLength,
            out float collisionHalfWidth,
            out float collisionHalfLength)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                collisionHalfWidth = 1.35f;
                collisionHalfLength = 2.2f;
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);

            collisionHalfWidth = Mathf.Clamp(bounds.extents.x + playerHalfWidth, 0.95f, 2.65f);
            collisionHalfLength = Mathf.Clamp(bounds.extents.z + playerHalfLength, 0.65f, 4.8f);
        }

        private static bool SegmentIntersectsBox(
            Vector2 start,
            Vector2 end,
            float halfWidth,
            float halfLength)
        {
            float minimumTime = 0f;
            float maximumTime = 1f;
            Vector2 movement = end - start;

            return ClipSegmentAxis(start.x, movement.x, -halfWidth, halfWidth, ref minimumTime, ref maximumTime) &&
                   ClipSegmentAxis(start.y, movement.y, -halfLength, halfLength, ref minimumTime, ref maximumTime);
        }

        private static bool ClipSegmentAxis(
            float origin,
            float movement,
            float minimum,
            float maximum,
            ref float minimumTime,
            ref float maximumTime)
        {
            if (Mathf.Abs(movement) < 0.00001f)
            {
                return origin >= minimum && origin <= maximum;
            }

            float first = (minimum - origin) / movement;
            float second = (maximum - origin) / movement;
            if (first > second)
            {
                float swap = first;
                first = second;
                second = swap;
            }

            minimumTime = Mathf.Max(minimumTime, first);
            maximumTime = Mathf.Min(maximumTime, second);
            return minimumTime <= maximumTime;
        }

        private int CountOrdinaryTraffic()
        {
            int count = 0;
            for (int i = 0; i < hazards.Count; i++)
            {
                if (hazards[i].kind == HazardKind.OncomingCar ||
                    hazards[i].kind == HazardKind.SlowerCar) count++;
            }
            return count;
        }

        private void ScheduleNextTrafficSpawn()
        {
            nextTrafficSpawnTime = Time.time + UnityEngine.Random.Range(
                SpawnIntervalMinimum,
                SpawnIntervalMaximum);
        }

        private void ScheduleNextRoadFigure()
        {
            nextRoadFigureTime = Time.time + UnityEngine.Random.Range(105f, 135f);
        }

        private void ClearHazards(params HazardKind[] kinds)
        {
            for (int i = hazards.Count - 1; i >= 0; i--)
            {
                bool shouldClear = false;
                for (int kindIndex = 0; kindIndex < kinds.Length; kindIndex++)
                {
                    if (hazards[i].kind == kinds[kindIndex])
                    {
                        shouldClear = true;
                        break;
                    }
                }

                if (!shouldClear) continue;
                if (hazards[i].root != null) Destroy(hazards[i].root);
                hazards.RemoveAt(i);
            }
        }

        private void ClearAllHazards()
        {
            for (int i = 0; i < hazards.Count; i++)
            {
                if (hazards[i].root != null) Destroy(hazards[i].root);
            }
            hazards.Clear();
        }

        private void BuildFallbackMaterials()
        {
            if (carBodyMaterial != null) return;

            carBodyMaterial = RuntimeGeometry.Material("Traffic paint", new Color(0.055f, 0.07f, 0.075f), 0.32f, 0.45f);
            carGlassMaterial = RuntimeGeometry.Material("Traffic dark glass", new Color(0.018f, 0.028f, 0.036f), 0.08f, 0.72f);
            tyreMaterial = RuntimeGeometry.Material("Traffic tyre", new Color(0.008f, 0.008f, 0.009f), 0f, 0.08f);
            wheelHubMaterial = RuntimeGeometry.Material("Traffic wheel hubs", new Color(0.16f, 0.17f, 0.16f), 0.62f, 0.4f);
            headlightMaterial = RuntimeGeometry.Material("Oncoming headlights", new Color(0.78f, 0.84f, 0.68f), 0f, 0.5f, true);
            tailLightMaterial = RuntimeGeometry.Material("Traffic tail lights", new Color(0.55f, 0.015f, 0.008f), 0f, 0.4f, true);
            barricadeFrameMaterial = RuntimeGeometry.Material("Barricade reflective frame", new Color(0.8f, 0.24f, 0.025f), 0.08f, 0.28f, true);
            barricadeDarkMaterial = RuntimeGeometry.Material("Barricade backing", new Color(0.035f, 0.027f, 0.022f), 0f, 0.12f);

            Texture2D oncomingTexture = Resources.Load<Texture2D>("Traffic/OncomingSedanFront");
            Texture2D rearTexture = Resources.Load<Texture2D>("Traffic/TrafficSedanRear");
            Texture2D barricadeTexture = Resources.Load<Texture2D>("Traffic/BarricadeReflective");
            Texture2D sideTexture = Resources.Load<Texture2D>("Traffic/TrafficSedanSide");
            Texture2D topTexture = Resources.Load<Texture2D>("Traffic/TrafficSedanTop");
            Texture2D figureTexture = Resources.Load<Texture2D>("Anomalies/RoadFigure");
            if (oncomingTexture != null)
            {
                oncomingImageMaterial = RuntimeGeometry.TexturedMaterial("Oncoming sedan image", oncomingTexture, true);
            }
            if (rearTexture != null)
            {
                rearImageMaterial = RuntimeGeometry.TexturedMaterial("Traffic sedan rear image", rearTexture, true);
            }
            if (barricadeTexture != null)
            {
                barricadeImageMaterial = RuntimeGeometry.TexturedMaterial("Reflective barricade image", barricadeTexture, true);
            }
            if (sideTexture != null)
            {
                sideImageMaterial = RuntimeGeometry.TexturedMaterial("Traffic sedan side image", sideTexture, true);
            }
            if (topTexture != null)
            {
                topImageMaterial = RuntimeGeometry.TexturedMaterial("Traffic sedan top image", topTexture, true);
            }
            if (figureTexture != null)
            {
                roadFigureMaterial = RuntimeGeometry.TexturedMaterial("Road figure silhouette", figureTexture, true);
            }
        }
    }
}
