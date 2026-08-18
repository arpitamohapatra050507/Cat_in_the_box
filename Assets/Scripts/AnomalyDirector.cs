using System.Collections;
using UnityEngine;

namespace LastPassenger
{
    /// <summary>
    /// Coordinates repeatable mirror scares and the single truck pursuit without
    /// coupling either anomaly to the road generator or the vehicle visuals.
    /// </summary>
    public sealed class AnomalyDirector : MonoBehaviour
    {
        [Header("Rear-seat apparition")]
        [SerializeField] private float apparitionCheckInterval = 10f;
        [SerializeField, Range(0f, 1f)] private float apparitionChance = 0.5f;
        [SerializeField] private float apparitionDuration = 4.5f;
        [SerializeField, Range(0f, 1f)] private float ghostAudioVolume = 0.4f;

        [Header("Truck pursuit")]
        [SerializeField] private float truckWarningDuration = 4.5f;
        [SerializeField] private float truckChaseDuration = 12f;
        [SerializeField] private Vector2 barricadeInterval = new Vector2(3.2f, 4.5f);
        [SerializeField, Range(0f, 0.05f)] private float maximumTruckVolume = 0.05f;
        [SerializeField] private float initialTruckGap = 16f;
        [SerializeField] private float barricadeGapPenalty = 7f;
        [SerializeField, Range(0f, 1f)] private float dangerSpeedRatio = 0.83f;

        private VehicleController vehicle;
        private MirrorSystem mirror;
        private TrafficHazardManager traffic;
        private PrototypeGameManager gameManager;
        private AudioSource effectsSource;
        private AudioSource truckSource;
        private AudioClip ghostClip;
        private AudioClip hornClip;
        private float apparitionCheckTimer;
        private float truckGap;
        private bool truckSequenceRequested;
        private bool truckSequenceRunning;
        private bool truckChaseActive;
        private bool truckChaseCompleted;

        public bool MajorAnomalyActive =>
            truckSequenceRunning || (mirror != null && mirror.ApparitionVisible);

        public void Configure(
            VehicleController controller,
            MirrorSystem mirrorSystem,
            TrafficHazardManager trafficManager,
            PrototypeGameManager manager)
        {
            if (traffic != null)
            {
                traffic.BarricadeHit -= OnBarricadeHit;
            }

            vehicle = controller;
            mirror = mirrorSystem;
            traffic = trafficManager;
            gameManager = manager;

            if (traffic != null)
            {
                traffic.BarricadeHit += OnBarricadeHit;
            }

            BuildAudio();
            apparitionCheckTimer = apparitionCheckInterval;
        }

        public void ForceApparition()
        {
            if (!CanRunGameplayAnomaly() || truckSequenceRunning) return;
            ShowApparition();
            apparitionCheckTimer = apparitionCheckInterval;
        }

        public void ForceTruckChase()
        {
            if (!CanRunGameplayAnomaly() || truckSequenceRunning) return;
            truckChaseCompleted = false;
            truckSequenceRequested = true;
        }

        private void BuildAudio()
        {
            if (effectsSource == null)
            {
                effectsSource = gameObject.AddComponent<AudioSource>();
                effectsSource.playOnAwake = false;
                effectsSource.spatialBlend = 0f;
            }

            if (truckSource == null)
            {
                truckSource = gameObject.AddComponent<AudioSource>();
                truckSource.playOnAwake = false;
                truckSource.loop = false;
                truckSource.spatialBlend = 0f;
                truckSource.volume = 0f;
            }

            ghostClip = Resources.Load<AudioClip>("Audio/Anomalies/GhostAppearance");
            hornClip = ProceduralAudio.TruckHorn();
            truckSource.clip = Resources.Load<AudioClip>("Audio/Anomalies/TruckChase");
        }

        private void Update()
        {
            if (!CanRunGameplayAnomaly())
            {
                if (truckSequenceRunning) AbortTruckSequence();
                return;
            }

            if (!truckChaseCompleted && !truckSequenceRunning &&
                !truckSequenceRequested && gameManager.TruckChaseAllowed)
            {
                truckSequenceRequested = true;
            }

            if (truckSequenceRequested && !truckSequenceRunning &&
                !mirror.ApparitionVisible)
            {
                StartCoroutine(TruckSequence());
                return;
            }

            if (truckSequenceRunning) return;

            apparitionCheckTimer -= Time.deltaTime;
            if (apparitionCheckTimer > 0f) return;

            apparitionCheckTimer = Mathf.Max(0.1f, apparitionCheckInterval);
            if (Random.value <= apparitionChance)
            {
                ShowApparition();
            }
        }

        private bool CanRunGameplayAnomaly()
        {
            return vehicle != null && mirror != null && traffic != null &&
                   gameManager != null && gameManager.IsGameplayActive;
        }

        private void ShowApparition()
        {
            bool severe = Random.value < 0.5f;
            float duration = Mathf.Max(0.25f, apparitionDuration);
            if (!mirror.ShowSeatApparition(severe, duration)) return;

            if (ghostClip != null && effectsSource != null)
            {
                effectsSource.PlayOneShot(ghostClip, ghostAudioVolume);
            }

            gameManager.ShowGameplayMessage(
                severe ? "Something is sitting behind you." : "The rear seat shifts in the mirror.",
                severe ? new Color(0.95f, 0.3f, 0.26f) : new Color(0.72f, 0.78f, 0.74f),
                duration);
        }

        private IEnumerator TruckSequence()
        {
            truckSequenceRequested = false;
            truckSequenceRunning = true;
            apparitionCheckTimer = apparitionCheckInterval;

            float warningTime = Mathf.Max(0.5f, truckWarningDuration);
            gameManager.ShowGameplayMessage(
                "A horn sounds behind you. Do not slow down.",
                new Color(1f, 0.5f, 0.28f),
                warningTime);
            // If inspecting the apparition dismissed it early, do not let its
            // remaining one-shot audio muddy the truck's warning cadence.
            if (effectsSource != null) effectsSource.Stop();
            PlayHorn();

            float warningElapsed = 0f;
            bool secondHornPlayed = false;
            while (warningElapsed < warningTime)
            {
                if (!CanRunGameplayAnomaly())
                {
                    AbortTruckSequence();
                    yield break;
                }

                warningElapsed += Time.deltaTime;
                if (!secondHornPlayed && warningElapsed >= warningTime * 0.55f)
                {
                    secondHornPlayed = true;
                    PlayHorn();
                }

                float warningProximity = Mathf.Lerp(0.06f, 0.18f,
                    Mathf.Clamp01(warningElapsed / warningTime));
                mirror.SetTruckPursuit(true, warningProximity);
                gameManager.SetThreatLevel(warningProximity * 0.35f);
                yield return null;
            }

            truckChaseActive = true;
            truckGap = Mathf.Max(initialTruckGap, 2f);
            traffic.SetChaseActive(true);

            if (truckSource != null && truckSource.clip != null)
            {
                truckSource.volume = maximumTruckVolume * 0.55f;
                truckSource.Play();
            }

            gameManager.ShowGameplayMessage(
                "FULL THROTTLE — keep the truck out of reach.",
                new Color(1f, 0.32f, 0.22f),
                5f);

            float elapsed = 0f;
            // Put the first obstacle in play early enough that the player can
            // encounter two fair, well-spaced barricades during a short chase.
            float nextBarricade = Random.Range(1.1f, 1.8f);

            while (elapsed < truckChaseDuration)
            {
                if (!CanRunGameplayAnomaly())
                {
                    AbortTruckSequence();
                    yield break;
                }

                elapsed += Time.deltaTime;
                UpdateTruckGap(Time.deltaTime);

                nextBarricade -= Time.deltaTime;
                if (nextBarricade <= 0f)
                {
                    traffic.SpawnChaseBarricade();
                    nextBarricade = Random.Range(
                        Mathf.Min(barricadeInterval.x, barricadeInterval.y),
                        Mathf.Max(barricadeInterval.x, barricadeInterval.y));
                }

                float proximity = GapToProximity(truckGap);
                mirror.SetTruckPursuit(true, proximity);
                gameManager.SetThreatLevel(proximity);
                if (truckSource != null && truckSource.isPlaying)
                {
                    truckSource.volume = Mathf.Lerp(
                        maximumTruckVolume * 0.45f,
                        maximumTruckVolume,
                        proximity);
                }

                if (truckGap <= 1.5f)
                {
                    gameManager.FailFromHazard("The truck fills every mirror, then drives straight through you.");
                    AbortTruckSequence();
                    yield break;
                }

                yield return null;
            }

            EndTruckSequence();
        }

        private void UpdateTruckGap(float deltaTime)
        {
            float speedRatio = Mathf.Clamp01(vehicle.SpeedRatio);
            if (speedRatio < dangerSpeedRatio)
            {
                float normalizedSpeed = dangerSpeedRatio > 0f
                    ? speedRatio / dangerSpeedRatio
                    : 1f;
                // Even sitting just below the safe band is fatal over a full chase;
                // the player has to commit to the accelerator instead of coasting.
                float closingSpeed = Mathf.Lerp(2.4f, 1.35f, normalizedSpeed);
                truckGap -= closingSpeed * deltaTime;
            }
            else
            {
                float escapeProgress = Mathf.InverseLerp(dangerSpeedRatio, 1f, speedRatio);
                truckGap += escapeProgress * 0.9f * deltaTime;
            }

            truckGap = Mathf.Clamp(truckGap, 0f, 26f);
        }

        private float GapToProximity(float gap)
        {
            return 1f - Mathf.InverseLerp(1.5f, 22f, gap);
        }

        private void OnBarricadeHit()
        {
            if (!truckChaseActive) return;

            truckGap = Mathf.Max(0f, truckGap - barricadeGapPenalty);
            float proximity = GapToProximity(truckGap);
            mirror.SetTruckPursuit(true, proximity);
            gameManager.SetThreatLevel(proximity);
            gameManager.ShowGameplayMessage(
                "The impact lets the truck gain ground.",
                new Color(1f, 0.32f, 0.22f),
                2f);
        }

        private void EndTruckSequence()
        {
            truckChaseActive = false;
            truckChaseCompleted = true;
            truckSequenceRunning = false;
            truckSequenceRequested = false;
            traffic.SetChaseActive(false);
            mirror.SetTruckPursuit(false, 0f);
            gameManager.SetThreatLevel(0f);

            if (truckSource != null) truckSource.Stop();

            gameManager.ShowGameplayMessage(
                "The headlights shrink, then vanish without turning away.",
                new Color(0.68f, 0.74f, 0.72f),
                4.5f);
            apparitionCheckTimer = apparitionCheckInterval;
        }

        private void AbortTruckSequence()
        {
            truckChaseActive = false;
            truckSequenceRunning = false;
            truckSequenceRequested = false;
            if (traffic != null) traffic.SetChaseActive(false);
            if (mirror != null) mirror.SetTruckPursuit(false, 0f);
            if (gameManager != null) gameManager.SetThreatLevel(0f);
            if (truckSource != null) truckSource.Stop();
        }

        private void PlayHorn()
        {
            if (hornClip != null && effectsSource != null)
            {
                effectsSource.PlayOneShot(hornClip, 0.42f);
            }
        }

        private void OnDisable()
        {
            if (traffic != null)
            {
                traffic.BarricadeHit -= OnBarricadeHit;
            }

            AbortTruckSequence();
        }

        private void OnEnable()
        {
            if (traffic == null) return;
            traffic.BarricadeHit -= OnBarricadeHit;
            traffic.BarricadeHit += OnBarricadeHit;
        }
    }
}
