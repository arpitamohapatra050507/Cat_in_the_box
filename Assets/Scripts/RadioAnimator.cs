using UnityEngine;

namespace LastPassenger
{
    public sealed class RadioAnimator : MonoBehaviour
    {
        private static readonly Color ActiveColor = new Color(0.06f, 0.58f, 0.24f);
        private static readonly Color AnomalyColor = new Color(0.7f, 0.055f, 0.025f);

        private Material displayMaterial;
        private Transform tuningNeedle;
        private Vector3 needleOrigin;
        private float phase;

        public void Configure(Material material, Transform needle)
        {
            displayMaterial = material;
            tuningNeedle = needle;
            if (tuningNeedle != null) needleOrigin = tuningNeedle.localPosition;
            phase = Random.Range(0f, 100f);
        }

        private void Update()
        {
            PrototypeGameManager manager = PrototypeGameManager.Instance;
            bool radioEnabled = manager == null || manager.RadioEnabled;
            bool anomaly = manager != null &&
                (manager.ThreatLevel > 0.05f || manager.AmbientAnomalyActive);

            float noise = Mathf.PerlinNoise(phase, Time.time * 4.1f);
            float pulse = 0.7f + Mathf.Sin(Time.time * 7.3f + phase) * 0.14f + noise * 0.16f;
            Color active = anomaly ? AnomalyColor : ActiveColor;
            Color displayColor = radioEnabled ? active * pulse : new Color(0.003f, 0.006f, 0.004f);

            if (displayMaterial != null)
            {
                displayMaterial.color = displayColor;
                if (displayMaterial.HasProperty("_BaseColor")) displayMaterial.SetColor("_BaseColor", displayColor);
                if (displayMaterial.HasProperty("_EmissionColor"))
                {
                    displayMaterial.SetColor("_EmissionColor", radioEnabled ? displayColor * 3f : Color.black);
                }
            }

            if (tuningNeedle != null)
            {
                Vector3 position = needleOrigin;
                position.x += radioEnabled ? Mathf.Sin(Time.time * (anomaly ? 5.7f : 0.42f) + phase) * 0.13f : 0f;
                tuningNeedle.localPosition = Vector3.Lerp(tuningNeedle.localPosition, position, Time.deltaTime * 9f);
                tuningNeedle.gameObject.SetActive(radioEnabled);
            }
        }
    }
}
