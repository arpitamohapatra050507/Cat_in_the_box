using UnityEngine;

namespace LastPassenger
{
    public sealed class MirrorSystem : MonoBehaviour
    {
        private Texture2D mirrorTexture;
        private Texture2D anomalyTexture;
        private Texture2D sideMirrorTexture;
        private Texture2D truckTexture;
        private Texture2D handsTexture;

        private bool apparitionVisible;
        private bool severeApparition;
        private bool apparitionDispelled;
        private float apparitionStartedAt;
        private float apparitionSafetyExpiry;
        private float observationTime;
        private float pulse;
        private Vector2 anomalyJitter;
        private float anomalyAlpha;
        private float apparitionDanger;

        private bool truckRequested;
        private float truckFade;
        private float truckProximity;
        private float requestedTruckProximity;
        private float truckJumpscareStartedAt = -100f;
        private float truckJumpscareDuration;

        public bool IsExpanded { get; private set; }
        public bool WasObservedAfterAnomaly => apparitionDispelled;
        public bool WasDispelledByObservation => apparitionDispelled;
        public bool ApparitionVisible => apparitionVisible;
        public Texture MirrorTexture => mirrorTexture;

        public void Build(Transform vehicle, Transform body)
        {
            transform.SetParent(vehicle, false);
            _ = body;
            mirrorTexture = Resources.Load<Texture2D>("Mirror/RearCabinBackseat");
            anomalyTexture = Resources.Load<Texture2D>("Mirror/WhiteGrainAnomaly");
            sideMirrorTexture = Resources.Load<Texture2D>("Anomalies/SideMirrorNightRoad");
            truckTexture = Resources.Load<Texture2D>("Anomalies/PursuerTruckFront");
            handsTexture = Resources.Load<Texture2D>("Anomalies/ApparitionHandsEdges");
        }

        public bool ShowSeatApparition(bool severe, float seconds)
        {
            if (apparitionVisible || anomalyTexture == null) return false;

            apparitionVisible = true;
            severeApparition = severe;
            apparitionDispelled = false;
            observationTime = 0f;
            pulse = Random.Range(0f, 20f);
            apparitionStartedAt = Time.time;
            apparitionSafetyExpiry = apparitionStartedAt + Mathf.Max(8f, seconds + 5f);
            anomalyAlpha = 0f;
            apparitionDanger = 0f;
            return true;
        }

        public void TriggerAnomaly(bool severe)
        {
            ShowSeatApparition(severe, 3f);
        }

        public void HideSeatApparition()
        {
            apparitionVisible = false;
            observationTime = 0f;
            apparitionDanger = 0f;
        }

        public void SetApparitionDanger(float danger)
        {
            apparitionDanger = Mathf.Clamp01(danger);
        }

        public void SetTruckPursuit(bool visible, float proximity)
        {
            truckRequested = visible;
            requestedTruckProximity = Mathf.Clamp01(proximity);
        }

        public void StartTruckJumpscare(float duration)
        {
            truckJumpscareDuration = Mathf.Max(0.2f, duration);
            truckJumpscareStartedAt = Time.unscaledTime;
        }

        private void Update()
        {
            IsExpanded = PrototypeInput.MirrorHeld;

            if (apparitionVisible)
            {
                pulse += Time.deltaTime;
                float elapsed = Time.time - apparitionStartedAt;
                float fadeIn = Mathf.Clamp01(elapsed / 0.25f);
                float flicker = 0.54f + Mathf.PerlinNoise(pulse * 12f, 4.1f) * 0.42f;
                anomalyAlpha = fadeIn * flicker;
                anomalyJitter.x = (Mathf.PerlinNoise(pulse * 17f, 0.2f) - 0.5f) * 0.018f;
                anomalyJitter.y = (Mathf.PerlinNoise(0.7f, pulse * 21f) - 0.5f) * 0.025f;

                if (IsExpanded)
                {
                    observationTime += Time.deltaTime;
                    if (observationTime >= 0.55f)
                    {
                        apparitionDispelled = true;
                        apparitionVisible = false;
                        apparitionDanger = 0f;
                    }
                }
                else
                {
                    observationTime = Mathf.MoveTowards(observationTime, 0f, Time.deltaTime * 2f);
                }

                if (Time.time >= apparitionSafetyExpiry)
                {
                    HideSeatApparition();
                }
            }
            else
            {
                anomalyAlpha = Mathf.MoveTowards(anomalyAlpha, 0f, Time.deltaTime * 4f);
            }

            truckFade = Mathf.MoveTowards(
                truckFade,
                truckRequested ? 1f : 0f,
                Time.deltaTime * (truckRequested ? 1.8f : 3.2f));
            truckProximity = Mathf.MoveTowards(
                truckProximity,
                requestedTruckProximity,
                Time.deltaTime * 0.72f);
        }

        private void OnGUI()
        {
            PrototypeGameManager manager = PrototypeGameManager.Instance;
            bool gameplay = manager != null && manager.IsGameplayActive;

            if (gameplay)
            {
                DrawCabinMirror();
                if (truckFade > 0.01f && sideMirrorTexture != null)
                {
                    DrawTruckMirrors();
                }

                DrawApparitionHands();
            }

            DrawTruckJumpscare();
        }

        private void DrawCabinMirror()
        {
            if (mirrorTexture == null) return;

            float width = IsExpanded
                ? Mathf.Min(Screen.width * 0.72f, 900f)
                : Mathf.Min(Screen.width * 0.34f, 440f);
            float height = width * 0.31f;
            Rect mirrorRect = new Rect(
                (Screen.width - width) * 0.5f,
                IsExpanded ? 54f : 18f,
                width,
                height);

            Color previous = GUI.color;
            GUI.color = new Color(0.02f, 0.025f, 0.025f, 0.96f);
            GUI.DrawTexture(
                new Rect(mirrorRect.x - 8f, mirrorRect.y - 8f, mirrorRect.width + 16f, mirrorRect.height + 16f),
                Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.DrawTexture(mirrorRect, mirrorTexture, ScaleMode.StretchToFill, false);

            if ((apparitionVisible || anomalyAlpha > 0.01f) && anomalyTexture != null)
            {
                float anomalyWidth = mirrorRect.width * (severeApparition ? 0.28f : 0.22f);
                float anomalyHeight = mirrorRect.height * (severeApparition ? 1.12f : 0.9f);
                float centerX = mirrorRect.x + mirrorRect.width *
                    ((severeApparition ? 0.5f : 0.68f) + anomalyJitter.x);
                float bottom = mirrorRect.y + mirrorRect.height * (1.04f + anomalyJitter.y);
                Rect anomalyRect = new Rect(
                    centerX - anomalyWidth * 0.5f,
                    bottom - anomalyHeight,
                    anomalyWidth,
                    anomalyHeight);

                GUI.BeginGroup(mirrorRect);
                GUI.color = new Color(1f, 1f, 1f, anomalyAlpha);
                GUI.DrawTexture(
                    new Rect(
                        anomalyRect.x - mirrorRect.x,
                        anomalyRect.y - mirrorRect.y,
                        anomalyRect.width,
                        anomalyRect.height),
                    anomalyTexture,
                    ScaleMode.ScaleToFit,
                    true);
                GUI.EndGroup();
            }

            GUI.color = Color.white;
            GUIStyle label = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = IsExpanded ? 18 : 12,
                normal = { textColor = apparitionVisible
                    ? new Color(1f, 0.76f, 0.67f)
                    : new Color(0.72f, 0.78f, 0.76f) }
            };
            string prompt = apparitionVisible
                ? (IsExpanded ? "KEEP LOOKING — hold R" : "HOLD R — LOOK BEHIND YOU")
                : (IsExpanded ? "REAR-VIEW MIRROR — release R to lower" : "Hold R to inspect");
            GUI.Label(
                new Rect(mirrorRect.x, mirrorRect.yMax + 6f, mirrorRect.width, 24f),
                prompt,
                label);
            GUI.color = previous;
        }

        private void DrawApparitionHands()
        {
            if (handsTexture == null || apparitionDanger <= 0.01f) return;

            float eased = apparitionDanger * apparitionDanger;
            float scale = Mathf.Lerp(1.22f, 1f, eased);
            float width = Screen.width * scale;
            float height = Screen.height * scale;
            Rect rect = new Rect(
                (Screen.width - width) * 0.5f,
                (Screen.height - height) * 0.5f,
                width,
                height);

            Color previous = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.08f, 0.96f, eased));
            GUI.DrawTexture(rect, handsTexture, ScaleMode.StretchToFill, true);
            GUI.color = previous;
        }

        private void DrawTruckMirrors()
        {
            float width = Mathf.Min(Screen.width * 0.19f, 320f);
            float height = width * 0.48f;
            float y = Screen.height * 0.39f;
            Rect left = new Rect(Screen.width * 0.025f, y, width, height);
            Rect right = new Rect(Screen.width - Screen.width * 0.025f - width, y, width, height);

            DrawTruckMirror(left, true);
            DrawTruckMirror(right, false);
        }

        private void DrawTruckMirror(Rect rect, bool leftSide)
        {
            Color previous = GUI.color;
            GUI.color = new Color(0.01f, 0.012f, 0.012f, truckFade * 0.98f);
            GUI.DrawTexture(
                new Rect(rect.x - 7f, rect.y - 7f, rect.width + 14f, rect.height + 14f),
                Texture2D.whiteTexture);

            GUI.color = new Color(1f, 1f, 1f, truckFade);
            if (leftSide)
            {
                GUI.DrawTextureWithTexCoords(rect, sideMirrorTexture, new Rect(1f, 0f, -1f, 1f));
            }
            else
            {
                GUI.DrawTexture(rect, sideMirrorTexture, ScaleMode.StretchToFill, false);
            }

            if (truckTexture != null)
            {
                float easedProximity = truckProximity * truckProximity * (3f - 2f * truckProximity);
                float truckHeight = rect.height * Mathf.Lerp(0.34f, 1.55f, easedProximity);
                float truckWidth = truckHeight * (truckTexture.width / (float)truckTexture.height);
                float centerOffset = rect.width * (leftSide ? 0.055f : -0.055f);
                Rect truckRect = new Rect(
                    rect.width * 0.5f + centerOffset - truckWidth * 0.5f,
                    rect.height * 0.98f - truckHeight,
                    truckWidth,
                    truckHeight);

                GUI.BeginGroup(rect);
                GUI.color = new Color(1f, 1f, 1f, truckFade);
                GUI.DrawTexture(truckRect, truckTexture, ScaleMode.ScaleToFit, true);
                GUI.EndGroup();
            }

            GUI.color = previous;
        }

        private void DrawTruckJumpscare()
        {
            if (truckTexture == null || truckJumpscareDuration <= 0f) return;

            float progress = (Time.unscaledTime - truckJumpscareStartedAt) / truckJumpscareDuration;
            if (progress < 0f || progress >= 1f) return;

            float eased = 1f - Mathf.Pow(1f - Mathf.Clamp01(progress), 4f);
            float targetHeight = Screen.height * Mathf.Lerp(0.28f, 2.35f, eased);
            float targetWidth = targetHeight * (truckTexture.width / (float)truckTexture.height);
            float shake = eased * 18f;
            Rect rect = new Rect(
                (Screen.width - targetWidth) * 0.5f + Random.Range(-shake, shake),
                (Screen.height - targetHeight) * 0.5f + Random.Range(-shake, shake),
                targetWidth,
                targetHeight);

            Color previous = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTexture(rect, truckTexture, ScaleMode.ScaleToFit, true);
            if (progress > 0.7f)
            {
                GUI.color = new Color(1f, 1f, 1f, Mathf.InverseLerp(0.7f, 1f, progress) * 0.68f);
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            }
            GUI.color = previous;
        }
    }
}
