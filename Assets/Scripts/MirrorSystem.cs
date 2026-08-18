using UnityEngine;

namespace LastPassenger
{
    public sealed class MirrorSystem : MonoBehaviour
    {
        private Texture2D mirrorTexture;
        private Texture2D anomalyTexture;
        private Transform coveredBody;
        private Material bodyMaterial;
        private Material anomalyMaterial;
        private bool anomalyTriggered;
        private bool severeAnomaly;
        private bool mirrorObservedAfterAnomaly;
        private float pulse;
        private Vector2 anomalyJitter;
        private float anomalyAlpha;

        public bool IsExpanded { get; private set; }
        public bool WasObservedAfterAnomaly => mirrorObservedAfterAnomaly;
        public Texture MirrorTexture => mirrorTexture;

        public void Build(Transform vehicle, Transform body)
        {
            transform.SetParent(vehicle, false);
            coveredBody = body;
            bodyMaterial = RuntimeGeometry.Material("Covered body cloth", new Color(0.18f, 0.19f, 0.17f), 0f, 0.08f);
            anomalyMaterial = RuntimeGeometry.Material("Anomalous cloth", new Color(0.22f, 0.025f, 0.02f), 0f, 0.15f, true);

            SetBodyMaterial(bodyMaterial);
            mirrorTexture = Resources.Load<Texture2D>("Mirror/RearCabinBackseat");
            anomalyTexture = Resources.Load<Texture2D>("Mirror/WhiteGrainAnomaly");
        }

        public void TriggerAnomaly(bool severe)
        {
            if (anomalyTriggered || coveredBody == null) return;
            anomalyTriggered = true;
            severeAnomaly = severe;

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

                anomalyJitter.x = (Mathf.PerlinNoise(pulse * 17f, 0.2f) - 0.5f) * 0.018f;
                anomalyJitter.y = (Mathf.PerlinNoise(0.7f, pulse * 21f) - 0.5f) * 0.025f;
                anomalyAlpha = 0.54f + Mathf.PerlinNoise(pulse * 12f, 4.1f) * 0.42f;
            }
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
                (PrototypeGameManager.Instance.State == PrototypeGameManager.RunState.Success ||
                 PrototypeGameManager.Instance.State == PrototypeGameManager.RunState.Failure)) return;

            float width = IsExpanded ? Mathf.Min(Screen.width * 0.72f, 900f) : Mathf.Min(Screen.width * 0.34f, 440f);
            float height = width * 0.31f;
            Rect mirrorRect = new Rect((Screen.width - width) * 0.5f, IsExpanded ? 54f : 18f, width, height);

            Color previous = GUI.color;
            GUI.color = new Color(0.02f, 0.025f, 0.025f, 0.96f);
            GUI.DrawTexture(new Rect(mirrorRect.x - 8f, mirrorRect.y - 8f, mirrorRect.width + 16f, mirrorRect.height + 16f), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.DrawTexture(mirrorRect, mirrorTexture, ScaleMode.StretchToFill, false);

            if (anomalyTriggered && anomalyTexture != null)
            {
                float anomalyWidth = mirrorRect.width * (severeAnomaly ? 0.28f : 0.22f);
                float anomalyHeight = mirrorRect.height * (severeAnomaly ? 1.12f : 0.9f);
                float centerX = mirrorRect.x + mirrorRect.width *
                    ((severeAnomaly ? 0.5f : 0.68f) + anomalyJitter.x);
                float bottom = mirrorRect.y + mirrorRect.height * (1.04f + anomalyJitter.y);
                Rect anomalyRect = new Rect(
                    centerX - anomalyWidth * 0.5f,
                    bottom - anomalyHeight,
                    anomalyWidth,
                    anomalyHeight);

                GUI.BeginGroup(mirrorRect);
                Rect clippedAnomalyRect = new Rect(
                    anomalyRect.x - mirrorRect.x,
                    anomalyRect.y - mirrorRect.y,
                    anomalyRect.width,
                    anomalyRect.height);
                GUI.color = new Color(1f, 1f, 1f, anomalyAlpha);
                GUI.DrawTexture(clippedAnomalyRect, anomalyTexture, ScaleMode.ScaleToFit, true);
                GUI.EndGroup();
            }

            GUI.color = Color.white;

            GUIStyle label = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = IsExpanded ? 18 : 12,
                normal = { textColor = new Color(0.72f, 0.78f, 0.76f) }
            };
            GUI.Label(new Rect(mirrorRect.x, mirrorRect.yMax + 6f, mirrorRect.width, 24f), IsExpanded ? "REAR-VIEW MIRROR — release R to lower" : "Hold R to inspect", label);
            GUI.color = previous;
        }
    }
}
