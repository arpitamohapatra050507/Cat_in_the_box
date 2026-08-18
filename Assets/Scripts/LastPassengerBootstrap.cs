using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastPassenger
{
    public sealed class LastPassengerBootstrap : MonoBehaviour
    {
        private static bool started;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneBootstrap()
        {
            started = false;
            SceneManager.sceneLoaded -= CreatePrototype;
            SceneManager.sceneLoaded += CreatePrototype;
        }

        private static void CreatePrototype(Scene scene, LoadSceneMode mode)
        {
            if (started || Object.FindFirstObjectByType<PrototypeGameManager>() != null) return;
            started = true;
            new GameObject("The Last Passenger — Generated Prototype").AddComponent<LastPassengerBootstrap>();
        }

        private void Awake()
        {
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = 60;
            BuildEnvironment();

            Material cabin = RuntimeGeometry.Material("Hearse interior", new Color(0.025f, 0.028f, 0.026f), 0.1f, 0.18f);
            Material trim = RuntimeGeometry.Material("Worn metal trim", new Color(0.12f, 0.13f, 0.12f), 0.55f, 0.32f);
            Material instrument = RuntimeGeometry.Material("Green instrument glow", new Color(0.05f, 0.34f, 0.17f), 0f, 0.25f, true);
            Material glass = RuntimeGeometry.Material("Dark glass", new Color(0.035f, 0.055f, 0.06f), 0.05f, 0.72f);
            Material cloth = RuntimeGeometry.Material("Body covering", new Color(0.18f, 0.19f, 0.17f), 0f, 0.08f);

            GameObject vehicle = new GameObject("Player hearse");
            vehicle.transform.position = new Vector3(0f, 0.18f, 0f);
            VehicleController controller = vehicle.AddComponent<VehicleController>();
            controller.ConfigureAudio(ProceduralAudio.EngineLoop(), ProceduralAudio.Impact());

            BuildCabin(vehicle.transform, controller, cabin, trim, instrument, glass);
            Transform body = BuildCoveredBody(vehicle.transform, cloth);
            BuildMainCamera(vehicle.transform);
            BuildHeadlights(vehicle.transform);

            GameObject mirrorObject = new GameObject("Mirror system");
            MirrorSystem mirror = mirrorObject.AddComponent<MirrorSystem>();
            mirror.Build(vehicle.transform, body);

            GameObject roadObject = new GameObject("Generated repeating road");
            RoadGenerator road = roadObject.AddComponent<RoadGenerator>();
            road.Build(vehicle.transform);

            GameObject managerObject = new GameObject("Prototype game state");
            PrototypeGameManager manager = managerObject.AddComponent<PrototypeGameManager>();
            manager.Configure(controller, mirror);

        }

        private static void BuildEnvironment()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.018f;
            RenderSettings.fogColor = new Color(0.012f, 0.018f, 0.022f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.025f, 0.035f, 0.04f);
            RenderSettings.skybox = null;

            GameObject moonObject = new GameObject("Cold moon light");
            Light moon = moonObject.AddComponent<Light>();
            moon.type = LightType.Directional;
            moon.color = new Color(0.28f, 0.38f, 0.5f);
            moon.intensity = 0.32f;
            moon.shadows = LightShadows.Soft;
            moonObject.transform.rotation = Quaternion.Euler(36f, -28f, 0f);
        }

        private static Camera BuildMainCamera(Transform vehicle)
        {
            GameObject cameraObject = new GameObject("Driver camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(vehicle, false);
            cameraObject.transform.localPosition = new Vector3(0f, 1.47f, 0.15f);
            cameraObject.transform.localRotation = Quaternion.identity;

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 72f;
            camera.nearClipPlane = 0.04f;
            camera.farClipPlane = 360f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.004f, 0.007f, 0.009f);
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<DriverCameraLook>();
            return camera;
        }

        private static void BuildCabin(
            Transform vehicle,
            VehicleController controller,
            Material cabin,
            Material trim,
            Material instrument,
            Material glass)
        {
            GameObject cabinRoot = RuntimeGeometry.Empty("Generated hearse cabin", vehicle, Vector3.zero);

            RuntimeGeometry.Primitive("Dashboard support", PrimitiveType.Cube, cabinRoot.transform,
                new Vector3(0f, 0.46f, 1.18f), new Vector3(3.2f, 0.3f, 0.72f), cabin,
                new Vector3(-6f, 0f, 0f));

            BuildDashboardFascia(cabinRoot.transform, instrument);
            RuntimeGeometry.Primitive("Cabin floor", PrimitiveType.Cube, cabinRoot.transform,
                new Vector3(0f, 0.02f, -0.35f), new Vector3(3.5f, 0.18f, 4.7f), cabin);

            RuntimeGeometry.Primitive("Left A pillar", PrimitiveType.Cube, cabinRoot.transform,
                new Vector3(-1.6f, 1.55f, 0.95f), new Vector3(0.16f, 1.8f, 0.18f), trim,
                new Vector3(0f, 0f, -11f));
            RuntimeGeometry.Primitive("Right A pillar", PrimitiveType.Cube, cabinRoot.transform,
                new Vector3(1.6f, 1.55f, 0.95f), new Vector3(0.16f, 1.8f, 0.18f), trim,
                new Vector3(0f, 0f, 11f));
            RuntimeGeometry.Primitive("Roof edge", PrimitiveType.Cube, cabinRoot.transform,
                new Vector3(0f, 2.34f, 0.8f), new Vector3(3.35f, 0.16f, 0.18f), trim);
            RuntimeGeometry.Primitive("Rear partition", PrimitiveType.Cube, cabinRoot.transform,
                new Vector3(0f, 1.2f, -2.7f), new Vector3(3.4f, 2.4f, 0.1f), glass);
            RuntimeGeometry.Primitive("Left door", PrimitiveType.Cube, cabinRoot.transform,
                new Vector3(-1.7f, 0.85f, -0.2f), new Vector3(0.15f, 1.7f, 3.2f), cabin);
            RuntimeGeometry.Primitive("Right door", PrimitiveType.Cube, cabinRoot.transform,
                new Vector3(1.7f, 0.85f, -0.2f), new Vector3(0.15f, 1.7f, 3.2f), cabin);

            BuildSteeringWheel(cabinRoot.transform, trim, controller);
        }

        private static void BuildDashboardFascia(Transform parent, Material instrument)
        {
            Texture2D dashboardTexture = Resources.Load<Texture2D>("Dashboard/DarkDashboardFascia");
            if (dashboardTexture != null)
            {
                Material dashboardMaterial = RuntimeGeometry.TexturedMaterial("Generated dashboard fascia", dashboardTexture);
                RuntimeGeometry.TexturedQuad(
                    "Generated dashboard fascia",
                    parent,
                    new Vector3(0f, 1f, 1.45f),
                    new Vector2(2.72f, 0.95f),
                    dashboardMaterial);
            }

            GameObject radioDisplay = RuntimeGeometry.Primitive(
                "Animated radio display",
                PrimitiveType.Cube,
                parent,
                new Vector3(0.02f, 0.985f, 1.405f),
                new Vector3(0.38f, 0.1f, 0.025f),
                instrument);
            GameObject tuningNeedle = RuntimeGeometry.Primitive(
                "Radio tuning needle",
                PrimitiveType.Cube,
                parent,
                new Vector3(0.02f, 0.985f, 1.385f),
                new Vector3(0.012f, 0.074f, 0.012f),
                instrument);

            RadioAnimator radioAnimator = radioDisplay.AddComponent<RadioAnimator>();
            radioAnimator.Configure(radioDisplay.GetComponent<Renderer>().sharedMaterial, tuningNeedle.transform);
        }

        private static void BuildSteeringWheel(Transform parent, Material material, VehicleController controller)
        {
            GameObject wheel = RuntimeGeometry.Empty("Steering wheel", parent, new Vector3(-0.62f, 0.97f, 0.62f));
            wheel.transform.localEulerAngles = new Vector3(68f, 0f, 0f);

            for (int i = 0; i < 12; i++)
            {
                float angle = i / 12f * Mathf.PI * 2f;
                Vector3 position = new Vector3(Mathf.Cos(angle) * 0.34f, Mathf.Sin(angle) * 0.34f, 0f);
                RuntimeGeometry.Primitive("Wheel rim", PrimitiveType.Cube, wheel.transform, position,
                    new Vector3(0.16f, 0.07f, 0.07f), material, new Vector3(0f, 0f, -angle * Mathf.Rad2Deg));
            }

            RuntimeGeometry.Primitive("Wheel hub", PrimitiveType.Cylinder, wheel.transform,
                Vector3.zero, new Vector3(0.12f, 0.06f, 0.12f), material, new Vector3(90f, 0f, 0f));

            for (int i = 0; i < 3; i++)
            {
                float angle = i * 120f;
                float radians = angle * Mathf.Deg2Rad;
                RuntimeGeometry.Primitive(
                    "Wheel spoke",
                    PrimitiveType.Cube,
                    wheel.transform,
                    new Vector3(Mathf.Cos(radians) * 0.16f, Mathf.Sin(radians) * 0.16f, 0f),
                    new Vector3(0.32f, 0.045f, 0.045f),
                    material,
                    new Vector3(0f, 0f, angle));
            }

            SteeringWheelAnimator animator = wheel.AddComponent<SteeringWheelAnimator>();
            animator.Configure(controller);
        }

        private static Transform BuildCoveredBody(Transform vehicle, Material cloth)
        {
            GameObject body = RuntimeGeometry.Empty("Covered passenger", vehicle, new Vector3(0f, 0.64f, -1.55f));
            body.transform.localEulerAngles = new Vector3(90f, 0f, 0f);

            RuntimeGeometry.Primitive("Wrapped torso", PrimitiveType.Capsule, body.transform,
                Vector3.zero, new Vector3(0.58f, 1.02f, 0.44f), cloth);
            RuntimeGeometry.Primitive("Covered head", PrimitiveType.Sphere, body.transform,
                new Vector3(0f, 1.05f, 0f), new Vector3(0.46f, 0.5f, 0.44f), cloth);
            RuntimeGeometry.Primitive("Sheet fold", PrimitiveType.Cube, body.transform,
                new Vector3(0f, -0.2f, -0.39f), new Vector3(1.25f, 1.65f, 0.08f), cloth,
                new Vector3(0f, 0f, 3f));
            return body.transform;
        }

        private static void BuildHeadlights(Transform vehicle)
        {
            for (int i = 0; i < 2; i++)
            {
                GameObject lightObject = new GameObject(i == 0 ? "Left headlight" : "Right headlight");
                lightObject.transform.SetParent(vehicle, false);
                lightObject.transform.localPosition = new Vector3(i == 0 ? -0.9f : 0.9f, 0.62f, 1.55f);
                lightObject.transform.localRotation = Quaternion.Euler(4f, 0f, 0f);

                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Spot;
                light.range = 72f;
                light.spotAngle = 48f;
                light.innerSpotAngle = 30f;
                light.intensity = 4.6f;
                light.color = new Color(0.78f, 0.82f, 0.72f);
                light.shadows = LightShadows.Soft;
            }
        }

        private void OnDestroy()
        {
            started = false;
        }
    }
}
