using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastPassenger
{
    public sealed class PrototypeGameManager : MonoBehaviour
    {
        public enum RunState
        {
            Driving,
            JunctionChosen,
            Success,
            Failure
        }

        public static PrototypeGameManager Instance { get; private set; }

        private readonly List<RoadEventDefinition> roadEvents = new List<RoadEventDefinition>();
        private readonly HashSet<string> triggeredEvents = new HashSet<string>();

        private VehicleController vehicle;
        private MirrorSystem mirror;
        private AnomalyDirector anomalyDirector;
        private AudioSource windSource;
        private AudioSource radioSource;
        private AudioSource stingSource;
        private RunState state = RunState.Driving;
        private bool junctionResolved;
        private bool choseLeft;
        private bool radioEnabled = true;
        private string message = string.Empty;
        private Color messageColor = Color.white;
        private float messageUntil;
        private float vignetteAlpha;
        private float threatLevel;
        private float impactFlash;
        private float lastTrafficCollisionTime = -100f;
        private string failureTitle = "THE PASSENGER ARRIVED";
        private string failureDescription = "You chose the quarry. Something else finished the journey.";

        private const float JunctionWarningDistance = 540f;
        private const float JunctionDistance = 650f;
        private const float FailureDistance = 1480f;
        private const float FinishDistance = 2000f;

        public RunState State => state;
        public bool RadioEnabled => radioEnabled;
        public bool IsGameplayActive => state != RunState.Success && state != RunState.Failure;
        public float ThreatLevel => threatLevel;
        public bool AmbientAnomalyActive => mirror != null && mirror.ApparitionVisible;
        public bool TruckChaseAllowed =>
            IsGameplayActive && junctionResolved && vehicle != null &&
            vehicle.Distance >= 780f && Time.timeSinceLevelLoad >= 45f;

        public void Configure(VehicleController controller, MirrorSystem mirrorSystem)
        {
            vehicle = controller;
            mirror = mirrorSystem;

            LoadRoadEvents();

            BuildAudio();
            ShowMessage("W/S drive  •  A/D steer  •  Hold RMB to look  •  Hold R for mirror", Color.white, 8f);
        }

        public void AttachAnomalyDirector(AnomalyDirector director)
        {
            anomalyDirector = director;
        }

        private void LoadRoadEvents()
        {
            TextAsset configuration = Resources.Load<TextAsset>("road_events");
            if (configuration != null)
            {
                RoadEventCollection parsed = JsonUtility.FromJson<RoadEventCollection>(configuration.text);
                if (parsed != null && parsed.events != null) roadEvents.AddRange(parsed.events);
            }

            if (roadEvents.Count > 0) return;

            roadEvents.Add(new RoadEventDefinition("intro", 10f,
                "The quarry closes before dawn. Keep the cargo out of sight.", new Color(0.75f, 0.78f, 0.72f), 7f));
            roadEvents.Add(new RoadEventDefinition("radio-clue", 430f,
                "RADIO: ...the dead always keep to the left...", new Color(0.62f, 0.78f, 0.68f), 8f));
            roadEvents.Add(new RoadEventDefinition("junction", JunctionWarningDistance,
                "FORK AHEAD — choose a lane before the reflective posts.", new Color(0.9f, 0.75f, 0.42f), 8f));
        }

        private void Awake()
        {
            Instance = this;
        }

        private void BuildAudio()
        {
            windSource = gameObject.AddComponent<AudioSource>();
            windSource.clip = ProceduralAudio.WindLoop();
            windSource.loop = true;
            windSource.volume = 0.16f;
            windSource.Play();

            radioSource = gameObject.AddComponent<AudioSource>();
            AudioClip customRadioStatic = Resources.Load<AudioClip>("Audio/RadioStatic");
            radioSource.clip = customRadioStatic != null ? customRadioStatic : ProceduralAudio.RadioStatic();
            radioSource.loop = true;
            radioSource.spatialBlend = 0f;
            radioSource.volume = customRadioStatic != null ? 0.03f : 0.025f;
            radioSource.Play();

            stingSource = gameObject.AddComponent<AudioSource>();
            stingSource.clip = ProceduralAudio.HorrorSting();
            stingSource.volume = 0.55f;
        }

        private void Update()
        {
            if (vehicle == null || mirror == null) return;

            if (PrototypeInput.RadioPressed)
            {
                radioEnabled = !radioEnabled;
                radioSource.mute = !radioEnabled;
                ShowMessage(radioEnabled ? "Radio switched on." : "Radio switched off.", new Color(0.7f, 0.78f, 0.7f), 2f);
            }

            if (state == RunState.Success || state == RunState.Failure)
            {
                if (PrototypeInput.ConfirmPressed) Restart();
                if (PrototypeInput.CancelPressed) Application.Quit();
                return;
            }

            ProcessRoadEvents();

            if (!junctionResolved && vehicle.Distance >= JunctionDistance)
            {
                ResolveJunction();
            }

            if (!choseLeft && junctionResolved && vehicle.Distance >= FailureDistance)
            {
                FailRun();
            }
            else if (choseLeft && junctionResolved && vehicle.Distance >= FinishDistance)
            {
                CompleteRun();
            }

            impactFlash = Mathf.MoveTowards(impactFlash, 0f, Time.deltaTime * 0.9f);
            float dangerPulse = threatLevel > 0f
                ? threatLevel * (0.2f + (Mathf.Sin(Time.time * Mathf.Lerp(4f, 11f, threatLevel)) * 0.5f + 0.5f) * 0.12f)
                : 0f;
            vignetteAlpha = Mathf.MoveTowards(
                vignetteAlpha,
                Mathf.Max(impactFlash, dangerPulse),
                Time.deltaTime * 1.7f);
            DevelopmentDebugShortcuts();
        }

        private void ProcessRoadEvents()
        {
            for (int i = 0; i < roadEvents.Count; i++)
            {
                RoadEventDefinition roadEvent = roadEvents[i];
                if (vehicle.Distance >= roadEvent.triggerDistance && triggeredEvents.Add(roadEvent.id))
                {
                    ShowMessage(roadEvent.message, roadEvent.messageColor, roadEvent.displaySeconds);
                }
            }
        }

        private void ResolveJunction()
        {
            junctionResolved = true;
            choseLeft = vehicle.LanePosition < 0f;
            state = RunState.JunctionChosen;

            if (choseLeft)
            {
                ShowMessage("LEFT ROUTE SELECTED — the radio goes silent.", new Color(0.66f, 0.78f, 0.67f), 6f);
                radioSource.volume = 0.01f;
            }
            else
            {
                ShowMessage("RIGHT ROUTE SELECTED — something knocks behind you.", new Color(1f, 0.48f, 0.35f), 6f);
                radioSource.pitch = 0.58f;
                radioSource.volume = 0.04f;
            }
        }

        private void CompleteRun()
        {
            state = RunState.Success;
            vehicle.SetControlsEnabled(false);
            windSource.volume = 0.04f;
            radioSource.Stop();
            ShowMessage(string.Empty, Color.white, 0f);
        }

        private void FailRun()
        {
            if (state == RunState.Failure || state == RunState.Success) return;
            state = RunState.Failure;
            vehicle.SetControlsEnabled(false);
            stingSource.pitch = 0.55f;
            stingSource.Play();
            ShowMessage(string.Empty, Color.white, 0f);
        }

        public void NotifyRoadImpact()
        {
            if (IsGameplayActive)
            {
                ShowMessage("The shoulder drags at the tyres.", new Color(0.86f, 0.66f, 0.5f), 1.4f);
                impactFlash = Mathf.Max(impactFlash, 0.12f);
            }
        }

        public void NotifyTrafficCollision()
        {
            if (!IsGameplayActive) return;

            impactFlash = Mathf.Max(impactFlash, 0.32f);
            bool repeatedCollision = Time.time - lastTrafficCollisionTime <= 8f;
            lastTrafficCollisionTime = Time.time;
            if (repeatedCollision)
            {
                failureTitle = "PILE-UP";
                failureDescription = "The second impact folds the road around you.";
                FailRun();
                return;
            }

            ShowMessage(
                "METAL ON METAL — another hit will finish the car.",
                new Color(1f, 0.42f, 0.26f),
                3f);
        }

        public void NotifyBarricadeCollision()
        {
            if (!IsGameplayActive) return;
            impactFlash = Mathf.Max(impactFlash, 0.26f);
            ShowMessage(
                "The barricade tears speed from the car.",
                new Color(1f, 0.38f, 0.22f),
                2.2f);
        }

        public void SetThreatLevel(float level)
        {
            threatLevel = Mathf.Clamp01(level);
        }

        public void ShowGameplayMessage(string text, Color color, float seconds)
        {
            if (!IsGameplayActive) return;
            ShowMessage(text, color, seconds);
        }

        public void FailFromHazard(string description)
        {
            if (!IsGameplayActive) return;
            failureTitle = "RUN DOWN";
            failureDescription = description;
            threatLevel = 1f;
            impactFlash = 0.42f;
            FailRun();
        }

        private void ShowMessage(string text, Color color, float seconds)
        {
            message = text;
            messageColor = color;
            messageUntil = Time.time + seconds;
        }

        private void Restart()
        {
            Scene active = SceneManager.GetActiveScene();
            if (active.buildIndex >= 0) SceneManager.LoadScene(active.buildIndex);
            else SceneManager.LoadScene(active.name);
        }

        private void DevelopmentDebugShortcuts()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (PrototypeInput.SkipToTruckPressed) anomalyDirector?.ForceTruckChase();
            if (PrototypeInput.SkipToJunctionPressed) vehicle.TeleportForward(JunctionWarningDistance);
            if (PrototypeInput.SkipToAnomalyPressed) anomalyDirector?.ForceApparition();
            if (PrototypeInput.SkipToEndingPressed)
            {
                if (!junctionResolved)
                {
                    vehicle.TeleportForward(JunctionDistance);
                    ResolveJunction();
                }
                vehicle.TeleportForward(choseLeft ? FinishDistance : FailureDistance);
            }
#endif
        }

        private void OnGUI()
        {
            DrawVignette();
            DrawDashboard();

            if (!string.IsNullOrEmpty(message) && Time.time <= messageUntil)
            {
                GUIStyle messageStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = Mathf.Clamp(Screen.height / 42, 17, 28),
                    fontStyle = FontStyle.Bold,
                    wordWrap = true,
                    normal = { textColor = messageColor }
                };
                Rect box = new Rect(Screen.width * 0.18f, Screen.height * 0.73f, Screen.width * 0.64f, 90f);
                DrawDarkPanel(box);
                GUI.Label(box, message, messageStyle);
            }

            if (state == RunState.Success) DrawEnding(true);
            if (state == RunState.Failure) DrawEnding(false);
        }

        private void DrawDashboard()
        {
            if (vehicle == null || state == RunState.Success || state == RunState.Failure) return;

            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.56f, 0.72f, 0.62f) }
            };
            GUI.Label(new Rect(22f, Screen.height - 74f, 300f, 26f), $"SPEED  {vehicle.Speed * 7.2f:000} km/h", style);
            GUI.Label(new Rect(22f, Screen.height - 48f, 300f, 26f), radioEnabled ? "RADIO  87.3 — SIGNAL WEAK" : "RADIO  OFF", style);

            if (threatLevel > 0.08f)
            {
                GUIStyle dangerStyle = new GUIStyle(style)
                {
                    fontSize = 18,
                    normal = { textColor = new Color(1f, 0.28f, 0.2f) }
                };
                GUI.Label(
                    new Rect(Screen.width - 330f, Screen.height - 62f, 310f, 30f),
                    "DANGER BEHIND — KEEP SPEED",
                    dangerStyle);
            }

            if (!junctionResolved && vehicle.Distance >= JunctionWarningDistance)
            {
                GUIStyle forkStyle = new GUIStyle(style)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 19,
                    normal = { textColor = new Color(0.95f, 0.76f, 0.35f) }
                };
                GUI.Label(new Rect(Screen.width * 0.3f, Screen.height * 0.64f, Screen.width * 0.4f, 30f),
                    vehicle.LanePosition < 0f ? "SELECTING LEFT" : "SELECTING RIGHT", forkStyle);
            }
        }

        private void DrawVignette()
        {
            Color previous = GUI.color;
            GUI.color = new Color(0.12f, 0f, 0f, vignetteAlpha);
            float border = Mathf.Max(36f, Screen.width * 0.045f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, border), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0f, Screen.height - border, Screen.width, border), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0f, 0f, border, Screen.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(Screen.width - border, 0f, border, Screen.height), Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private void DrawEnding(bool success)
        {
            Color previous = GUI.color;
            GUI.color = success ? new Color(0.01f, 0.025f, 0.018f, 0.96f) : new Color(0.12f, 0f, 0f, 0.97f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUIStyle title = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Clamp(Screen.height / 14, 34, 72),
                fontStyle = FontStyle.Bold,
                normal = { textColor = success ? new Color(0.72f, 0.78f, 0.7f) : new Color(0.95f, 0.28f, 0.23f) }
            };
            GUIStyle body = new GUIStyle(title)
            {
                fontSize = Mathf.Clamp(Screen.height / 34, 18, 30),
                fontStyle = FontStyle.Normal,
                wordWrap = true,
                normal = { textColor = new Color(0.78f, 0.78f, 0.74f) }
            };

            GUI.Label(new Rect(0f, Screen.height * 0.27f, Screen.width, 90f), success ? "TEMPORARY DELIVERY" : failureTitle, title);
            GUI.Label(new Rect(Screen.width * 0.18f, Screen.height * 0.45f, Screen.width * 0.64f, 100f),
                success ? "The road releases you. The final story and ending remain deliberately unresolved."
                        : failureDescription, body);
            GUI.Label(new Rect(0f, Screen.height * 0.7f, Screen.width, 40f), "Press Enter to restart", body);
            GUI.color = previous;
        }

        private static void DrawDarkPanel(Rect rect)
        {
            Color previous = GUI.color;
            GUI.color = new Color(0.015f, 0.02f, 0.018f, 0.82f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
