using UnityEngine;

namespace LastPassenger
{
    public sealed class MirrorSystem : MonoBehaviour
    {
        private Camera rearCamera;
        private RenderTexture mirrorTexture;
        private Transform coveredBody;
        private Material bodyMaterial;
        private Material anomalyMaterial;
        private Transform anomalySprite;
        private Material anomalySpriteMaterial;
        private Vector3 anomalyBasePosition;
        private bool anomalyTriggered;
        private bool mirrorObservedAfterAnomaly;
        private float pulse;

        public bool IsExpanded { get; private set; }
        public bool WasObservedAfterAnomaly => mirrorObservedAfterAnomaly;
        public Texture MirrorTexture => mirrorTexture;

        public void Build(Transform vehicle, Transform body)
        {
            coveredBody = body;
            bodyMaterial = RuntimeGeometry.Material("Covered body cloth", new Color(0.18f, 0.19f, 0.17f), 0f, 0.08f);
            anomalyMaterial = RuntimeGeometry.Material("Anomalous cloth", new Color(0.22f, 0.025f, 0.02f), 0f, 0.15f, true);

            SetBodyMaterial(bodyMaterial);
            BuildGeneratedRearCabin(vehicle);

            GameObject cameraObject = new GameObject("Rear-view mirror camera");
            cameraObject.transform.SetParent(vehicle, false);
            cameraObject.transform.localPosition = new Vector3(0f, 1.52f, -0.15f);
            cameraObject.transform.localRotation = Quaternion.Euler(8f, 180f, 0f);
            rearCamera = cameraObject.AddComponent<Camera>();
            rearCamera.fieldOfView = 74f;
            rearCamera.nearClipPlane = 0.05f;
            rearCamera.farClipPlane = 180f;
            rearCamera.depth = -2f;
            rearCamera.allowHDR = false;
            rearCamera.clearFlags = CameraClearFlags.SolidColor;
            rearCamera.backgroundColor = new Color(0.004f, 0.007f, 0.009f, 1f);

            mirrorTexture = new RenderTexture(640, 256, 16, RenderTextureFormat.ARGB32)
            {
                name = "Generated rear-view mirror"
            };
            rearCamera.targetTexture = mirrorTexture;
        }

        public void TriggerAnomaly(bool severe)
        {
            if (anomalyTriggered || coveredBody == null) return;
            anomalyTriggered = true;

            if (anomalySprite != null)
            {
                anomalyBasePosition = severe
                    ? new Vector3(0f, 1.28f, -2.14f)
                    : new Vector3(0.76f, 1.22f, -2.14f);
                anomalySprite.localPosition = anomalyBasePosition;
                anomalySprite.localScale = severe
                    ? new Vector3(0.92f, 1.38f, 1f)
                    : new Vector3(0.72f, 1.08f, 1f);
                anomalySprite.gameObject.SetActive(true);
            }

            coveredBody.localPosition += severe ? new Vector3(0f, 0.45f, 0.48f) : new Vector3(0.18f, 0.08f, 0.12f);
            coveredBody.localRotation = severe ? Quaternion.Euler(12f, 0f, 0f) : Quaternion.Euler(86f, 0f, 7f);
            coveredBody.localScale = severe ? new Vector3(1.1f, 1.18f, 1.08f) : Vector3.one;
            SetBodyMaterial(severe ? anomalyMaterial : bodyMaterial);
        }

        private void Update()
        {
            IsExpanded = PrototypeInput.MirrorHeld;

            if (anomalyTriggered && IsExpanded)
            {
                mirrorObservedAfterAnomaly = true;
            }

            if (anomalyTriggered && coveredBody != null)
            {
                pulse += Time.deltaTime;
                coveredBody.localPosition += Vector3.up * (Mathf.Sin(pulse * 2.7f) * 0.0015f);

                if (anomalySprite != null)
                {
                    float xJitter = (Mathf.PerlinNoise(pulse * 17f, 0.2f) - 0.5f) * 0.025f;
                    float yJitter = (Mathf.PerlinNoise(0.7f, pulse * 21f) - 0.5f) * 0.018f;
                    anomalySprite.localPosition = anomalyBasePosition + new Vector3(xJitter, yJitter, 0f);

                    float alpha = 0.54f + Mathf.PerlinNoise(pulse * 12f, 4.1f) * 0.42f;
                    Color flicker = new Color(1f, 1f, 1f, alpha);
                    anomalySpriteMaterial.color = flicker;
                    if (anomalySpriteMaterial.HasProperty("_BaseColor"))
                    {
                        anomalySpriteMaterial.SetColor("_BaseColor", flicker);
                    }
                }
            }
        }

        private void BuildGeneratedRearCabin(Transform vehicle)
        {
            Texture2D cabinTexture = Resources.Load<Texture2D>("Mirror/RearCabinBackseat");
            if (cabinTexture != null)
            {
                Material cabinMaterial = RuntimeGeometry.TexturedMaterial(
                    "Generated rear-cabin plate",
                    cabinTexture);
                RuntimeGeometry.TexturedQuad(
                    "Generated rear cabin and backseat",
                    vehicle,
                    new Vector3(0f, 1.18f, -2.48f),
                    new Vector2(8.9f, 3.56f),
                    cabinMaterial,
                    new Vector3(0f, 180f, 0f));
            }

            Texture2D apparitionTexture = Resources.Load<Texture2D>("Mirror/WhiteGrainAnomaly");
            if (apparitionTexture == null) return;

            anomalySpriteMaterial = RuntimeGeometry.TexturedMaterial(
                "Generated white-grain apparition",
                apparitionTexture,
                transparent: true);
            GameObject apparition = RuntimeGeometry.TexturedQuad(
                "White grainy passenger anomaly",
                vehicle,
                new Vector3(0.76f, 1.22f, -2.14f),
                new Vector2(0.72f, 1.08f),
                anomalySpriteMaterial,
                new Vector3(0f, 180f, 0f));
            anomalySprite = apparition.transform;
            anomalyBasePosition = anomalySprite.localPosition;
            apparition.SetActive(false);
        }

        private void SetBodyMaterial(Material material)
        {
            if (coveredBody == null) return;
            Renderer[] renderers = coveredBody.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++) renderers[i].sharedMaterial = material;
        }

        private void OnGUI()
        {
            if (mirrorTexture == null) return;
            if (PrototypeGameManager.Instance != null &&
                (PrototypeGameManager.Instance.State == PrototypeGameManager.RunState.Menu ||
                 PrototypeGameManager.Instance.State == PrototypeGameManager.RunState.Success ||
                 PrototypeGameManager.Instance.State == PrototypeGameManager.RunState.Failure)) return;

            float width = IsExpanded ? Mathf.Min(Screen.width * 0.72f, 900f) : Mathf.Min(Screen.width * 0.34f, 440f);
            float height = width * 0.31f;
            Rect mirrorRect = new Rect((Screen.width - width) * 0.5f, IsExpanded ? 54f : 18f, width, height);

            Color previous = GUI.color;
            GUI.color = new Color(0.02f, 0.025f, 0.025f, 0.96f);
            GUI.DrawTexture(new Rect(mirrorRect.x - 8f, mirrorRect.y - 8f, mirrorRect.width + 16f, mirrorRect.height + 16f), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.DrawTexture(mirrorRect, mirrorTexture, ScaleMode.StretchToFill, false);

            GUIStyle label = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = IsExpanded ? 18 : 12,
                normal = { textColor = new Color(0.72f, 0.78f, 0.76f) }
            };
            GUI.Label(new Rect(mirrorRect.x, mirrorRect.yMax + 6f, mirrorRect.width, 24f), IsExpanded ? "REAR-VIEW MIRROR — release R to lower" : "Hold R to inspect", label);
            GUI.color = previous;
        }

        private void OnDestroy()
        {
            if (mirrorTexture != null)
            {
                mirrorTexture.Release();
                Destroy(mirrorTexture);
            }
        }
    }
}
