using UnityEngine;

namespace LastPassenger
{
    public sealed class LastPassengerBootstrap : MonoBehaviour
    {
        private bool built;

        private void Awake()
        {
            if (built) return;
            built = true;

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
            BuildMainCamera(vehicle.transform);

            AudioClip customEngine = Resources.Load<AudioClip>("Audio/CarEngine");
            controller.ConfigureAudio(
                customEngine != null ? customEngine : ProceduralAudio.EngineLoop(),
                ProceduralAudio.Impact());

            BuildCabin(vehicle.transform, controller, cabin, trim, instrument, glass);
            Transform body = BuildCoveredBody(vehicle.transform, cloth);
            BuildHeadlights(vehicle.transform);

            GameObject mirrorObject = new GameObject("Mirror system");
            MirrorSystem mirror = mirrorObject.AddComponent<MirrorSystem>();
            mirror.Build(vehicle.transform, body);

            RoadGenerator road = GetComponent<RoadGenerator>();
            if (road == null)
            {
                GameObject roadObject = new GameObject("Generated repeating road");
                road = roadObject.AddComponent<RoadGenerator>();
            }
            road.Build(vehicle.transform);

            GameObject managerObject = new GameObject("Prototype game state");
            PrototypeGameManager manager = managerObject.AddComponent<PrototypeGameManager>();
            manager.Configure(controller, mirror);

        }

        private static void BuildEnvironment()
        {
            Light[] sceneLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            for (int i = 0; i < sceneLights.Length; i++) sceneLights[i].enabled = false;

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
            Camera[] sceneCameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            GameObject cameraObject = new GameObject("Driver camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(vehicle, false);
            cameraObject.transform.localPosition = new Vector3(-0.56f, 1.34f, 0.08f);
            cameraObject.transform.localRotation = Quaternion.Euler(7f, 0f, 0f);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 68f;
            camera.nearClipPlane = 0.04f;
            camera.farClipPlane = 360f;
            camera.depth = 10f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.004f, 0.007f, 0.009f);
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<DriverCameraLook>();

            for (int i = 0; i < sceneCameras.Length; i++)
            {
                Camera sceneCamera = sceneCameras[i];
                sceneCamera.enabled = false;
                if (sceneCamera.CompareTag("MainCamera")) sceneCamera.tag = "Untagged";

                AudioListener sceneListener = sceneCamera.GetComponent<AudioListener>();
                if (sceneListener != null) sceneListener.enabled = false;
            }

            camera.enabled = true;
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

            BuildDashboardFascia(cabinRoot.transform, instrument, cabin, trim);
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

            BuildPassengerSeat(cabinRoot.transform);
            BuildSteeringWheelSprite(cabinRoot.transform, controller);
        }

        private static void BuildDashboardFascia(
            Transform parent,
            Material instrument,
            Material cabin,
            Material trim)
        {
            GameObject dashboardRoot = RuntimeGeometry.Empty(
                "Lowered slanted dashboard",
                parent,
                new Vector3(0f, 0.66f, 1.24f));
            dashboardRoot.transform.localRotation = Quaternion.Euler(21f, 0f, 0f);

            RuntimeGeometry.Primitive(
                "Dashboard left edge fill",
                PrimitiveType.Cube,
                dashboardRoot.transform,
                new Vector3(-1.77f, -0.04f, 0.08f),
                new Vector3(0.38f, 1.08f, 0.34f),
                cabin);
            RuntimeGeometry.Primitive(
                "Dashboard right edge fill",
                PrimitiveType.Cube,
                dashboardRoot.transform,
                new Vector3(1.77f, -0.04f, 0.08f),
                new Vector3(0.38f, 1.08f, 0.34f),
                cabin);
            RuntimeGeometry.Primitive(
                "Dashboard upper edge trim",
                PrimitiveType.Cube,
                dashboardRoot.transform,
                new Vector3(0f, 0.53f, 0.07f),
                new Vector3(3.75f, 0.08f, 0.22f),
                trim);

            Texture2D dashboardTexture = Resources.Load<Texture2D>("Dashboard/DarkDashboardFascia");
            if (dashboardTexture != null)
            {
                Material dashboardMaterial = RuntimeGeometry.TexturedMaterial("Generated dashboard fascia", dashboardTexture);
                RuntimeGeometry.TexturedQuad(
                    "Generated dashboard fascia",
                    dashboardRoot.transform,
                    Vector3.zero,
                    new Vector2(3.62f, 1.26f),
                    dashboardMaterial);
            }

            GameObject radioDisplay = RuntimeGeometry.Primitive(
                "Animated radio display",
                PrimitiveType.Cube,
                dashboardRoot.transform,
                new Vector3(0.03f, -0.005f, -0.035f),
                new Vector3(0.41f, 0.11f, 0.025f),
                instrument);
            GameObject tuningNeedle = RuntimeGeometry.Primitive(
                "Radio tuning needle",
                PrimitiveType.Cube,
                dashboardRoot.transform,
                new Vector3(0.03f, -0.005f, -0.055f),
                new Vector3(0.012f, 0.074f, 0.012f),
                instrument);

            RadioAnimator radioAnimator = radioDisplay.AddComponent<RadioAnimator>();
            radioAnimator.Configure(radioDisplay.GetComponent<Renderer>().sharedMaterial, tuningNeedle.transform);
        }

        private static void BuildSteeringWheelSprite(Transform parent, VehicleController controller)
        {
            Texture2D wheelTexture = Resources.Load<Texture2D>("Dashboard/DarkSteeringWheel");
            if (wheelTexture == null) return;

            Material wheelMaterial = RuntimeGeometry.TexturedMaterial(
                "Generated steering wheel sprite",
                wheelTexture,
                transparent: true);
            GameObject wheel = RuntimeGeometry.TexturedQuad(
                "Animated 2D steering wheel",
                parent,
                new Vector3(-0.56f, 0.68f, 0.88f),
                new Vector2(0.74f, 0.74f),
                wheelMaterial);

            SteeringWheelAnimator animator = wheel.AddComponent<SteeringWheelAnimator>();
            animator.Configure(controller);
        }

        private static void BuildPassengerSeat(Transform parent)
        {
            Material seat = RuntimeGeometry.Material(
                "Worn passenger-seat vinyl",
                new Color(0.035f, 0.042f, 0.038f),
                0f,
                0.11f);
            Material seam = RuntimeGeometry.Material(
                "Passenger-seat seams",
                new Color(0.075f, 0.082f, 0.075f),
                0f,
                0.08f);

            GameObject seatRoot = RuntimeGeometry.Empty(
                "Front passenger seat",
                parent,
                new Vector3(0.93f, 0.03f, -0.23f));

            RuntimeGeometry.Primitive(
                "Passenger seat cushion",
                PrimitiveType.Cube,
                seatRoot.transform,
                new Vector3(0f, 0.36f, 0.14f),
                new Vector3(0.72f, 0.18f, 0.78f),
                seat,
                new Vector3(-4f, 0f, 0f));
            RuntimeGeometry.Primitive(
                "Passenger seat backrest",
                PrimitiveType.Cube,
                seatRoot.transform,
                new Vector3(0f, 0.91f, -0.19f),
                new Vector3(0.72f, 0.94f, 0.19f),
                seat,
                new Vector3(-8f, 0f, 0f));
            RuntimeGeometry.Primitive(
                "Passenger headrest",
                PrimitiveType.Cube,
                seatRoot.transform,
                new Vector3(0f, 1.49f, -0.30f),
                new Vector3(0.40f, 0.30f, 0.18f),
                seat,
                new Vector3(-8f, 0f, 0f));
            RuntimeGeometry.Primitive(
                "Passenger backrest seam",
                PrimitiveType.Cube,
                seatRoot.transform,
                new Vector3(0f, 0.92f, -0.295f),
                new Vector3(0.025f, 0.72f, 0.012f),
                seam,
                new Vector3(-8f, 0f, 0f));
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

    }
}
