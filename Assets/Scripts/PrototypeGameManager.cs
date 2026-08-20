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
            CliffEnding,
            Success,
            Failure
        }

        public static PrototypeGameManager Instance { get; private set; }

        private readonly List<RoadEventDefinition> roadEvents = new List<RoadEventDefinition>();
        private readonly List<AnomalyCheckpointDefinition> anomalyCheckpoints =
            new List<AnomalyCheckpointDefinition>();
        private readonly HashSet<string> triggeredEvents = new HashSet<string>();

        private VehicleController vehicle;
        private MirrorSystem mirror;
        private AnomalyDirector anomalyDirector;
        private TrafficHazardManager trafficManager;
        private AudioSource windSource;
        private AudioSource radioSource;
        private AudioSource stingSource;
        private AudioSource checkpointScareSource;
        private AudioClip[] ambientScareClips;
        private float nextAmbientScareTime;
        private RunState state = RunState.Driving;
        private int nextCheckpointIndex;
        private int checkpointLevel;
        private bool radioEnabled = true;
        private string message = string.Empty;
        private Color messageColor = Color.white;
        private float messageUntil;
        private float redVignetteAlpha;
        private float threatLevel;
        private float impactFlash;
        private int chaseHealth;
        private bool chaseHudVisible;
        private string failureTitle = "THE PASSENGER ARRIVED";
        private string failureDescription = "The road finished the journey for you.";
        private Texture2D nightVignetteTexture;
        private Texture2D carDeathTexture;
        private Texture2D truckDeathTexture;
        private Texture2D failureTexture;
        private Texture2D cliffEndingTexture;
        private float cliffEndingStartedAt;

        private const float FinishDistance = 6000f;
        private const float CliffEndingDuration = 3.5f;

        public RunState State => state;
        public bool RadioEnabled => radioEnabled;
        public bool IsGameplayActive => state == RunState.Driving;
        public float ThreatLevel => threatLevel;
        public int CheckpointLevel => checkpointLevel;
        public bool AmbientAnomalyActive => mirror != null && mirror.ApparitionVisible;
        public bool TruckChaseAllowed =>
            IsGameplayActive && checkpointLevel >= 2 && vehicle != null;

        public void Configure(VehicleController controller, MirrorSystem mirrorSystem)
        {
            vehicle = controller;
            mirror = mirrorSystem;
            LoadRoadEvents();
            LoadAnomalyCheckpoints();
            BuildAudio();
            BuildScreenAssets();
            ShowMessage("W/S drive  •  A/D steer  •  Hold RMB to look  •  Hold R for mirror", Color.white, 8f);
        }

        public void AttachAnomalyDirector(AnomalyDirector director)
        {
            anomalyDirector = director;
        }

        public void AttachTrafficManager(TrafficHazardManager manager)
        {
            trafficManager = manager;
        }

        private void Awake()
        {
            Instance = this;
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
            roadEvents.Add(new RoadEventDefinition("radio-clue", 700f,
                "RADIO: ...the vehicle has not stopped since midnight...", new Color(0.62f, 0.78f, 0.68f), 7f));
        }

        private void LoadAnomalyCheckpoints()
        {
            TextAsset configuration = Resources.Load<TextAsset>("anomaly_checkpoints");
            if (configuration != null)
            {
                AnomalyCheckpointCollection parsed =
                    JsonUtility.FromJson<AnomalyCheckpointCollection>(configuration.text);
                if (parsed != null && parsed.checkpoints != null)
                {
                    anomalyCheckpoints.AddRange(parsed.checkpoints);
                }
            }

            anomalyCheckpoints.RemoveAll(checkpoint => checkpoint == null);
            anomalyCheckpoints.Sort((left, right) => left.progress.CompareTo(right.progress));
            if (anomalyCheckpoints.Count > 0) return;

            anomalyCheckpoints.Add(CreateFallbackCheckpoint("checkpoint-1", 0.2f, 14f, 0.5f, 95f, 120f, "roadFigure"));
            anomalyCheckpoints.Add(CreateFallbackCheckpoint("checkpoint-2", 0.4f, 13f, 0.52f, 85f, 110f, "truck"));
            anomalyCheckpoints.Add(CreateFallbackCheckpoint("checkpoint-3", 0.6f, 12f, 0.56f, 75f, 100f, "roadFigure"));
            anomalyCheckpoints.Add(CreateFallbackCheckpoint("checkpoint-4", 0.8f, 11f, 0.6f, 65f, 90f, "apparition"));
        }

        private static AnomalyCheckpointDefinition CreateFallbackCheckpoint(
            string id,
            float progress,
            float apparitionSeconds,
            float apparitionChance,
            float figureMinimum,
            float figureMaximum,
            string action)
        {
            return new AnomalyCheckpointDefinition
            {
                id = id,
                progress = progress,
                message = id.ToUpperInvariant().Replace('-', ' '),
                messageColor = new Color(0.78f, 0.62f, 0.55f),
                displaySeconds = 4f,
                apparitionCheckSeconds = apparitionSeconds,
                apparitionChance = apparitionChance,
                roadFigureMinimumSeconds = figureMinimum,
                roadFigureMaximumSeconds = figureMaximum,
                action = action
            };
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

            checkpointScareSource = gameObject.AddComponent<AudioSource>();
            checkpointScareSource.playOnAwake = false;
            checkpointScareSource.loop = false;
            checkpointScareSource.spatialBlend = 0f;
            ambientScareClips = new[] {
                Resources.Load<AudioClip>("Audio/Anomalies/Scary3"),
                Resources.Load<AudioClip>("Audio/Anomalies/Scary4"),
                Resources.Load<AudioClip>("Audio/Anomalies/Scary5"),
                Resources.Load<AudioClip>("Audio/Anomalies/Thunder")
            };
            nextAmbientScareTime = Time.time + Random.Range(55f, 95f);
        }

        private void BuildScreenAssets()
        {
            carDeathTexture = Resources.Load<Texture2D>("Anomalies/CarDeathScreen");
            truckDeathTexture = Resources.Load<Texture2D>("Anomalies/TruckDeathScreen");
            cliffEndingTexture = Resources.Load<Texture2D>("Anomalies/CliffRoadEnding");
            nightVignetteTexture = BuildRadialVignetteTexture(192);
        }

        private void Update()
        {
            if (vehicle == null || mirror == null) return;

            if (state == RunState.CliffEnding)
            {
                if (Time.time - cliffEndingStartedAt >= CliffEndingDuration) CompleteRun();
                impactFlash = Mathf.MoveTowards(impactFlash, 0f, Time.deltaTime * 0.9f);
                return;
            }

            if (state == RunState.Success || state == RunState.Failure)
            {
                if (PrototypeInput.ConfirmPressed) Restart();
                if (PrototypeInput.CancelPressed) Application.Quit();
                return;
            }

            if (PrototypeInput.RadioPressed)
            {
                radioEnabled = !radioEnabled;
                radioSource.mute = !radioEnabled;
                ShowMessage(radioEnabled ? "Radio switched on." : "Radio switched off.",
                    new Color(0.7f, 0.78f, 0.7f), 2f);
            }

            ProcessRoadEvents();
            ProcessAnomalyCheckpoints();
            if (Time.time >= nextAmbientScareTime && checkpointScareSource != null && ambientScareClips != null)
            {
                AudioClip scare = ambientScareClips[Random.Range(0, ambientScareClips.Length)];
                if (scare != null) checkpointScareSource.PlayOneShot(scare, 0.16f);
                nextAmbientScareTime = Time.time + Random.Range(55f, 95f);
            }
            if (vehicle.Distance >= FinishDistance) BeginCliffEnding();

            impactFlash = Mathf.MoveTowards(impactFlash, 0f, Time.deltaTime * 0.9f);
            float dangerPulse = threatLevel > 0f
                ? threatLevel * (0.2f + (Mathf.Sin(Time.time * Mathf.Lerp(4f, 11f, threatLevel)) * 0.5f + 0.5f) * 0.12f)
                : 0f;
            redVignetteAlpha = Mathf.MoveTowards(
                redVignetteAlpha,
                Mathf.Max(impactFlash, dangerPulse),
                Time.deltaTime * 1.7f);
            EventTestShortcuts();
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

        private void ProcessAnomalyCheckpoints()
        {
            while (nextCheckpointIndex < anomalyCheckpoints.Count)
            {
                AnomalyCheckpointDefinition checkpoint = anomalyCheckpoints[nextCheckpointIndex];
                float triggerDistance = FinishDistance * Mathf.Clamp(checkpoint.progress, 0.05f, 0.95f);
                if (vehicle.Distance < triggerDistance) return;

                nextCheckpointIndex++;
                checkpointLevel = nextCheckpointIndex;
                TriggerCheckpoint(checkpoint);
            }
        }

        private void TriggerCheckpoint(AnomalyCheckpointDefinition checkpoint)
        {
            anomalyDirector?.SetAmbientPacing(
                checkpoint.apparitionCheckSeconds,
                checkpoint.apparitionChance);
            trafficManager?.SetRoadFigurePacing(
                checkpoint.roadFigureMinimumSeconds,
                checkpoint.roadFigureMaximumSeconds);

            if (!string.IsNullOrWhiteSpace(checkpoint.message))
            {
                ShowMessage(
                    checkpoint.message,
                    checkpoint.messageColor,
                    Mathf.Max(1f, checkpoint.displaySeconds));
            }

            if (!string.IsNullOrWhiteSpace(checkpoint.audioResource))
            {
                AudioClip clip = Resources.Load<AudioClip>(checkpoint.audioResource);
                if (clip != null && checkpointScareSource != null)
                {
                    checkpointScareSource.PlayOneShot(clip, Mathf.Clamp01(checkpoint.audioVolume));
                }
            }

            string action = string.IsNullOrWhiteSpace(checkpoint.action)
                ? "none"
                : checkpoint.action.Trim().ToLowerInvariant();
            switch (action)
            {
                case "roadfigure":
                    trafficManager?.ForceRoadFigure();
                    break;
                case "apparition":
                    anomalyDirector?.ForceApparition();
                    break;
                case "truck":
                    anomalyDirector?.ForceTruckChase();
                    break;
            }
        }

        private void BeginCliffEnding()
        {
            if (!IsGameplayActive) return;
            state = RunState.CliffEnding;
            cliffEndingStartedAt = Time.time;
            chaseHudVisible = false;
            threatLevel = 0f;
            radioSource.Stop();
            windSource.volume = 0.3f;
            vehicle.BeginCliffFall();
            ShowMessage(string.Empty, Color.white, 0f);
        }

        private void CompleteRun()
        {
            state = RunState.Success;
            vehicle.SetControlsEnabled(false);
            windSource.volume = 0.04f;
        }

        private void FailRun()
        {
            if (!IsGameplayActive) return;
            state = RunState.Failure;
            chaseHudVisible = false;
            vehicle.SetControlsEnabled(false);
            stingSource.pitch = 0.55f;
            stingSource.Play();
            ShowMessage(string.Empty, Color.white, 0f);
        }

        public void NotifyRoadImpact()
        {
            if (!IsGameplayActive) return;
            ShowMessage("The shoulder drags at the tyres.", new Color(0.86f, 0.66f, 0.5f), 1.4f);
            impactFlash = Mathf.Max(impactFlash, 0.12f);
        }

        public void NotifyTrafficCollision()
        {
            if (!IsGameplayActive) return;
            impactFlash = 0.45f;
            failureTexture = carDeathTexture;
            failureTitle = "HEAD-ON";
            failureDescription = "There is no second chance at this speed.";
            FailRun();
        }

        public void NotifyBarricadeCollision()
        {
            if (!IsGameplayActive) return;
            impactFlash = Mathf.Max(impactFlash, 0.3f);
        }

        public void NotifyRoadFigurePassed()
        {
            if (!IsGameplayActive) return;
            ShowMessage("Nothing struck the car. The shape is simply gone.",
                new Color(0.66f, 0.7f, 0.68f), 3f);
        }

        public void BeginTruckChaseHud(int bars)
        {
            chaseHealth = Mathf.Max(1, bars);
            chaseHudVisible = true;
        }

        public int DamageChaseVehicle()
        {
            chaseHealth = Mathf.Max(0, chaseHealth - 1);
            impactFlash = Mathf.Max(impactFlash, 0.34f);
            return chaseHealth;
        }

        public void EndTruckChaseHud()
        {
            chaseHudVisible = false;
        }

        public void SetThreatLevel(float level)
        {
            threatLevel = Mathf.Clamp01(level);
        }

        public void ShowGameplayMessage(string text, Color color, float seconds)
        {
            if (IsGameplayActive) ShowMessage(text, color, seconds);
        }

        public void FailFromApparition()
        {
            if (!IsGameplayActive) return;
            failureTitle = "DON'T LOOK AWAY";
            failureDescription = "The hands reached the driver's seat before you faced what was behind you.";
            failureTexture = carDeathTexture;
            threatLevel = 1f;
            FailRun();
        }

        public void FailFromTruckImpact()
        {
            if (!IsGameplayActive) return;
            failureTitle = "ENGINE DEAD";
            failureDescription = "The truck does not brake. It drives through the rear of the car.";
            failureTexture = truckDeathTexture;
            threatLevel = 1f;
            impactFlash = 0.55f;
            FailRun();
        }

        public void FailFromHazard(string description)
        {
            if (!IsGameplayActive) return;
            failureTitle = "RUN DOWN";
            failureDescription = description;
            failureTexture = carDeathTexture;
            threatLevel = 1f;
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

        private void EventTestShortcuts()
        {
            if (PrototypeInput.SkipToTruckPressed) anomalyDirector?.ForceTruckChase();
            if (PrototypeInput.SkipToCheckpointPressed) TeleportToNextCheckpoint();
            if (PrototypeInput.SkipToAnomalyPressed) anomalyDirector?.ForceApparition();
            if (PrototypeInput.SkipToEndingPressed)
            {
                vehicle.TeleportForward(FinishDistance);
            }
        }

        private void TeleportToNextCheckpoint()
        {
            if (nextCheckpointIndex >= anomalyCheckpoints.Count) return;
            float progress = Mathf.Clamp(anomalyCheckpoints[nextCheckpointIndex].progress, 0.05f, 0.95f);
            vehicle.TeleportForward(FinishDistance * progress);
        }

        private void OnGUI()
        {
            DrawNightVignette();
            DrawRedVignette();
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

            if (state == RunState.CliffEnding) DrawCliffEnding();
            if (state == RunState.Success) DrawEnding(true);
            if (state == RunState.Failure) DrawEnding(false);
        }

        private void DrawDashboard()
        {
            if (vehicle == null || !IsGameplayActive) return;

            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.56f, 0.72f, 0.62f) }
            };
            GUI.Label(new Rect(22f, Screen.height - 74f, 300f, 26f), $"SPEED  {vehicle.Speed * 7.2f:000} km/h", style);
            GUI.Label(new Rect(22f, Screen.height - 48f, 300f, 26f), radioEnabled ? "RADIO  87.3 — SIGNAL WEAK" : "RADIO  OFF", style);

            if (chaseHudVisible) DrawChaseHealth();
            if (threatLevel > 0.08f)
            {
                GUIStyle dangerStyle = new GUIStyle(style)
                {
                    fontSize = 18,
                    normal = { textColor = new Color(1f, 0.28f, 0.2f) }
                };
                GUI.Label(new Rect(Screen.width - 350f, Screen.height - 62f, 330f, 30f),
                    "DANGER BEHIND — KEEP SPEED", dangerStyle);
            }

        }

        private void DrawChaseHealth()
        {
            const float barWidth = 62f;
            const float barHeight = 11f;
            const float gap = 9f;
            float totalWidth = barWidth * 3f + gap * 2f;
            float startX = (Screen.width - totalWidth) * 0.5f;
            float y = Screen.height * 0.73f - 22f;
            Color previous = GUI.color;

            for (int i = 0; i < 3; i++)
            {
                Rect outer = new Rect(startX + i * (barWidth + gap), y, barWidth, barHeight);
                GUI.color = new Color(0.01f, 0.012f, 0.012f, 0.94f);
                GUI.DrawTexture(new Rect(outer.x - 2f, outer.y - 2f, outer.width + 4f, outer.height + 4f), Texture2D.whiteTexture);
                GUI.color = i < chaseHealth
                    ? new Color(0.7f, 0.13f, 0.08f, 0.95f)
                    : new Color(0.12f, 0.04f, 0.035f, 0.75f);
                GUI.DrawTexture(outer, Texture2D.whiteTexture);
            }
            GUI.color = previous;
        }

        private void DrawNightVignette()
        {
            if (nightVignetteTexture == null || state == RunState.Success) return;
            Color previous = GUI.color;

            // This uniform veil uses Unity's built-in white texture, so it is
            // available in standalone players without a shader variant or a
            // scene-local post-processing profile. The radial pass below then
            // closes down peripheral visibility while headlights retain the
            // brightest values in the centre of the road.
            GUI.color = new Color(0f, 0.002f, 0.004f, 0.4f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);

            GUI.color = new Color(1f, 1f, 1f, 0.94f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), nightVignetteTexture, ScaleMode.StretchToFill, true);
            GUI.color = previous;
        }

        private void DrawRedVignette()
        {
            if (redVignetteAlpha <= 0.002f) return;
            Color previous = GUI.color;
            GUI.color = new Color(0.12f, 0f, 0f, redVignetteAlpha);
            float border = Mathf.Max(36f, Screen.width * 0.045f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, border), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0f, Screen.height - border, Screen.width, border), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0f, 0f, border, Screen.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(Screen.width - border, 0f, border, Screen.height), Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private void DrawCliffEnding()
        {
            float progress = Mathf.Clamp01((Time.time - cliffEndingStartedAt) / CliffEndingDuration);
            Color previous = GUI.color;
            if (cliffEndingTexture != null)
            {
                GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(progress * 2.4f));
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), cliffEndingTexture, ScaleMode.ScaleAndCrop, false);
            }
            GUI.color = new Color(0f, 0f, 0f, Mathf.InverseLerp(0.62f, 1f, progress));
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private void DrawEnding(bool success)
        {
            Color previous = GUI.color;
            GUI.color = success ? new Color(0.005f, 0.008f, 0.008f, 0.97f) : new Color(0.08f, 0f, 0f, 0.9f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            if (!success && failureTexture != null)
            {
                GUI.color = Color.white;
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), failureTexture, ScaleMode.ScaleAndCrop, true);
            }
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
            GUI.color = previous;
        }

        private static Texture2D BuildRadialVignetteTexture(int size)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Generated night vignette",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x / (float)(size - 1) - 0.5f) * 2f;
                    float ny = (y / (float)(size - 1) - 0.5f) * 2f;
                    float ellipticalDistance = Mathf.Sqrt(nx * nx * 0.72f + ny * ny);
                    float alpha = Mathf.SmoothStep(0f, 0.92f, Mathf.InverseLerp(0.42f, 1.2f, ellipticalDistance));
                    pixels[y * size + x] = new Color(0f, 0.004f, 0.008f, alpha);
                }
            }
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            return texture;
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
            if (nightVignetteTexture != null) Destroy(nightVignetteTexture);
        }
    }
}
