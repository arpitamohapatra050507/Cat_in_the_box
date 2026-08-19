using System.Collections;
using UnityEngine;

namespace LastPassenger
{
    public sealed class AnomalyDirector : MonoBehaviour
    {
        [Header("Rear-seat apparition")]
        [SerializeField] private float apparitionCheckInterval = 20f;
        [SerializeField, Range(0f, 1f)] private float apparitionChance = 0.5f;
        [SerializeField] private float apparitionKillDelay = 3f;
        [SerializeField, Range(0f, 1f)] private float ghostStartingVolume = 0.16f;
        [SerializeField, Range(0f, 1f)] private float ghostMaximumVolume = 0.85f;

        [Header("Truck pursuit")]
        [SerializeField] private float truckWarningDuration = 4.5f;
        [SerializeField] private float truckChaseDuration = 30f;
        [SerializeField] private Vector2 barricadeInterval = new Vector2(2.2f, 3.1f);
        [SerializeField, Range(0f, 0.05f)] private float maximumTruckVolume = 0.05f;
        [SerializeField] private float truckAudioFadeSeconds = 3f;
        [SerializeField] private float initialTruckGap = 20f;
        [SerializeField] private float barricadeGapPenalty = 4f;
        [SerializeField, Range(0f, 1f)] private float dangerSpeedRatio = 0.78f;

        private VehicleController vehicle;
        private MirrorSystem mirror;
        private TrafficHazardManager traffic;
        private PrototypeGameManager gameManager;
        private AudioSource effectsSource;
        private AudioSource apparitionSource;
        private AudioSource truckSource;
        private AudioClip ghostClip;
        private AudioClip hornClip;
        private AudioClip truckImpactClip;
        private float apparitionCheckTimer;
        private float apparitionThreatSeconds;
        private float apparitionFadeStartSeconds;
        private bool apparitionBeingObserved;
        private float truckGap;
        private bool apparitionThreatActive;
        private bool truckSequenceRequested;
        private bool truckSequenceRunning;
        private bool truckChaseActive;
        private bool truckChaseCompleted;
        private bool truckCatchRunning;

        public bool MajorAnomalyActive => apparitionThreatActive || truckSequenceRunning;

        public void Configure(
            VehicleController controller,
            MirrorSystem mirrorSystem,
            TrafficHazardManager trafficManager,
            PrototypeGameManager manager)
        {
            if (traffic != null) traffic.BarricadeHit -= OnBarricadeHit;

            vehicle = controller;
            mirror = mirrorSystem;
            traffic = trafficManager;
            gameManager = manager;

            if (traffic != null) traffic.BarricadeHit += OnBarricadeHit;
            BuildAudio();
            apparitionCheckTimer = apparitionCheckInterval;
        }

        public void ForceApparition()
        {
            if (!CanRunGameplayAnomaly() || truckSequenceRunning || apparitionThreatActive) return;
            ShowApparition();
            apparitionCheckTimer = apparitionCheckInterval;
        }

        public void ForceTruckChase()
        {
            if (!CanRunGameplayAnomaly() || truckSequenceRunning || apparitionThreatActive) return;
            truckChaseCompleted = false;
            truckSequenceRequested = true;
        }

        private void BuildAudio()
        {
            effectsSource = EnsureSource(effectsSource, false);
            apparitionSource = EnsureSource(apparitionSource, true);
            truckSource = EnsureSource(truckSource, true);

            ghostClip = Resources.Load<AudioClip>("Audio/Anomalies/GhostAppearance");
            hornClip = ProceduralAudio.TruckHorn();
            truckImpactClip = ProceduralAudio.TruckImpact();
            apparitionSource.clip = ghostClip;
            truckSource.clip = Resources.Load<AudioClip>("Audio/Anomalies/TruckChase");
        }

        private AudioSource EnsureSource(AudioSource source, bool loop)
        {
            if (source == null) source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.loop = loop;
            source.volume = 0f;
            return source;
        }

        private void Update()
        {
            if (!CanRunGameplayAnomaly())
            {
                if (truckSequenceRunning) AbortTruckSequence();
                if (apparitionThreatActive) EndApparition(false);
                return;
            }

            if (apparitionThreatActive)
            {
                UpdateApparitionThreat();
                return;
            }

            if (!truckChaseCompleted && !truckSequenceRunning &&
                !truckSequenceRequested && gameManager.TruckChaseAllowed)
            {
                truckSequenceRequested = true;
            }

            if (truckSequenceRequested && !truckSequenceRunning)
            {
                StartCoroutine(TruckSequence());
                return;
            }

            if (truckSequenceRunning) return;

            apparitionCheckTimer -= Time.deltaTime;
            if (apparitionCheckTimer > 0f) return;

            apparitionCheckTimer = Mathf.Max(0.1f, apparitionCheckInterval);
            if (Random.value <= apparitionChance) ShowApparition();
        }

        private bool CanRunGameplayAnomaly()
        {
            return vehicle != null && mirror != null && traffic != null &&
                   gameManager != null && gameManager.IsGameplayActive;
        }

        private void ShowApparition()
        {
            if (!mirror.ShowSeatApparition(Random.value < 0.5f, apparitionKillDelay)) return;

            apparitionThreatActive = true;
            apparitionThreatSeconds = 0.01f;
            apparitionFadeStartSeconds = 0f;
            apparitionBeingObserved = false;
            mirror.SetApparitionDanger(0f);
            mirror.SetApparitionOpacity(1f);
            if (apparitionSource != null && ghostClip != null)
            {
                apparitionSource.volume = ghostStartingVolume;
                apparitionSource.Play();
            }

            gameManager.ShowGameplayMessage(
                "Something is behind you. HOLD R. LOOK AT IT.",
                new Color(1f, 0.3f, 0.26f),
                apparitionKillDelay * 2f);
        }

        private void UpdateApparitionThreat()
        {
            float killDelay = Mathf.Max(0.1f, apparitionKillDelay);
            bool observing = PrototypeInput.MirrorHeld;
            if (observing)
            {
                if (!apparitionBeingObserved)
                {
                    apparitionBeingObserved = true;
                    apparitionFadeStartSeconds = Mathf.Max(0.01f, apparitionThreatSeconds);
                }
                apparitionThreatSeconds = Mathf.MoveTowards(
                    apparitionThreatSeconds,
                    0f,
                    Time.deltaTime);
            }
            else
            {
                apparitionBeingObserved = false;
                apparitionFadeStartSeconds = 0f;
                apparitionThreatSeconds = Mathf.Min(
                    killDelay,
                    apparitionThreatSeconds + Time.deltaTime);
            }

            float progress = Mathf.Clamp01(apparitionThreatSeconds / killDelay);
            float opacity = observing
                ? Mathf.Clamp01(apparitionThreatSeconds / Mathf.Max(0.01f, apparitionFadeStartSeconds))
                : 1f;
            mirror.SetApparitionDanger(progress);
            mirror.SetApparitionOpacity(opacity);
            gameManager.SetThreatLevel(progress * 0.62f);
            if (apparitionSource != null && apparitionSource.isPlaying)
            {
                float dangerVolume = Mathf.Lerp(ghostStartingVolume, ghostMaximumVolume,
                    progress * progress);
                apparitionSource.volume = dangerVolume * opacity;
            }

            if (observing && apparitionThreatSeconds <= 0f)
            {
                EndApparition(true);
                return;
            }

            if (apparitionThreatSeconds < killDelay) return;

            mirror.HideSeatApparition();
            mirror.SetApparitionDanger(1f);
            if (apparitionSource != null) apparitionSource.Stop();
            apparitionThreatActive = false;
            gameManager.FailFromApparition();
        }

        private void EndApparition(bool dispelled)
        {
            apparitionThreatActive = false;
            mirror?.HideSeatApparition();
            mirror?.SetApparitionDanger(0f);
            mirror?.SetApparitionOpacity(0f);
            if (apparitionSource != null) apparitionSource.Stop();
            if (gameManager != null) gameManager.SetThreatLevel(0f);
            if (dispelled && gameManager != null)
            {
                gameManager.ShowGameplayMessage(
                    "The seat is empty again.",
                    new Color(0.68f, 0.76f, 0.73f),
                    2f);
            }
            apparitionCheckTimer = apparitionCheckInterval;
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

                float proximity = Mathf.Lerp(0.06f, 0.18f,
                    Mathf.Clamp01(warningElapsed / warningTime));
                mirror.SetTruckPursuit(true, proximity);
                gameManager.SetThreatLevel(proximity * 0.35f);
                yield return null;
            }

            truckChaseActive = true;
            truckGap = Mathf.Max(initialTruckGap, 2f);
            traffic.SetChaseActive(true);
            gameManager.BeginTruckChaseHud(3);

            if (truckSource != null && truckSource.clip != null)
            {
                truckSource.time = 0f;
                truckSource.volume = maximumTruckVolume;
                truckSource.Play();
            }

            gameManager.ShowGameplayMessage(
                "FULL THROTTLE — three impacts will kill the engine.",
                new Color(1f, 0.32f, 0.22f),
                5f);

            float elapsed = 0f;
            float nextBarricade = Random.Range(1.2f, 1.8f);
            while (elapsed < truckChaseDuration)
            {
                if (truckCatchRunning) yield break;
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
                MaintainTruckAudioLoop();

                if (truckGap <= 1.5f)
                {
                    yield return TruckCatch();
                    yield break;
                }

                yield return null;
            }

            yield return EndTruckSequence();
        }

        private void MaintainTruckAudioLoop()
        {
            if (truckSource == null || !truckSource.isPlaying) return;

            // The team clip contains a long silent tail. Restart its audible
            // section before that tail so a 30-second chase stays continuous.
            float audibleLoopEnd = Mathf.Min(12f, truckSource.clip.length - 0.1f);
            if (audibleLoopEnd > 0.1f && truckSource.time >= audibleLoopEnd)
            {
                truckSource.time = 0f;
            }
            truckSource.volume = maximumTruckVolume;
        }

        private void UpdateTruckGap(float deltaTime)
        {
            float speedRatio = Mathf.Clamp01(vehicle.SpeedRatio);
            if (speedRatio < dangerSpeedRatio)
            {
                float normalizedSpeed = dangerSpeedRatio > 0f
                    ? speedRatio / dangerSpeedRatio
                    : 1f;
                truckGap -= Mathf.Lerp(2.25f, 1.05f, normalizedSpeed) * deltaTime;
            }
            else
            {
                float escapeProgress = Mathf.InverseLerp(dangerSpeedRatio, 1f, speedRatio);
                truckGap += escapeProgress * 0.64f * deltaTime;
            }

            truckGap = Mathf.Clamp(truckGap, 0f, 30f);
        }

        private float GapToProximity(float gap)
        {
            return 1f - Mathf.InverseLerp(1.5f, 26f, gap);
        }

        private void OnBarricadeHit()
        {
            if (!truckChaseActive || truckCatchRunning) return;

            truckGap = Mathf.Max(0f, truckGap - barricadeGapPenalty);
            int remaining = gameManager.DamageChaseVehicle();
            float proximity = GapToProximity(truckGap);
            mirror.SetTruckPursuit(true, proximity);
            gameManager.SetThreatLevel(proximity);

            if (remaining <= 0)
            {
                StartCoroutine(TruckCatch());
                return;
            }

            gameManager.ShowGameplayMessage(
                remaining == 1 ? "ENGINE FAILING — one bar remains." : "The truck gains ground.",
                new Color(1f, 0.32f, 0.22f),
                2f);
        }

        private IEnumerator TruckCatch()
        {
            if (truckCatchRunning) yield break;
            truckCatchRunning = true;
            truckChaseActive = false;
            vehicle.ApplyImpact(0f);
            vehicle.SetControlsEnabled(false);
            mirror.SetTruckPursuit(true, 1f);
            gameManager.SetThreatLevel(1f);
            traffic.SetChaseActive(false);
            if (truckSource != null) truckSource.volume = maximumTruckVolume;

            yield return new WaitForSeconds(0.38f);
            if (effectsSource != null && truckImpactClip != null)
            {
                effectsSource.PlayOneShot(truckImpactClip, 0.95f);
            }
            mirror.StartTruckJumpscare(0.9f);
            yield return new WaitForSecondsRealtime(0.86f);

            gameManager.FailFromTruckImpact();
            AbortTruckSequence();
            truckCatchRunning = false;
        }

        private IEnumerator EndTruckSequence()
        {
            truckChaseActive = false;
            truckChaseCompleted = true;
            truckSequenceRequested = false;
            traffic.SetChaseActive(false);
            mirror.SetTruckPursuit(false, 0f);
            gameManager.SetThreatLevel(0f);
            gameManager.EndTruckChaseHud();

            gameManager.ShowGameplayMessage(
                "The headlights shrink, then vanish without turning away.",
                new Color(0.68f, 0.74f, 0.72f),
                4.5f);

            if (truckSource != null && truckSource.isPlaying)
            {
                float startingVolume = truckSource.volume;
                float elapsed = 0f;
                float fadeSeconds = Mathf.Max(0.1f, truckAudioFadeSeconds);
                while (elapsed < fadeSeconds && truckSource.isPlaying)
                {
                    elapsed += Time.deltaTime;
                    truckSource.volume = Mathf.Lerp(startingVolume, 0f,
                        Mathf.Clamp01(elapsed / fadeSeconds));
                    yield return null;
                }
                truckSource.Stop();
                truckSource.volume = 0f;
            }

            truckSequenceRunning = false;
            apparitionCheckTimer = apparitionCheckInterval;
        }

        private void AbortTruckSequence()
        {
            truckChaseActive = false;
            truckSequenceRunning = false;
            truckSequenceRequested = false;
            if (traffic != null) traffic.SetChaseActive(false);
            if (mirror != null)
            {
                mirror.SetTruckPursuit(false, 0f);
                mirror.SetApparitionDanger(0f);
            }
            if (gameManager != null)
            {
                gameManager.SetThreatLevel(0f);
                gameManager.EndTruckChaseHud();
            }
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
            if (traffic != null) traffic.BarricadeHit -= OnBarricadeHit;
            AbortTruckSequence();
            EndApparition(false);
        }

        private void OnEnable()
        {
            if (traffic == null) return;
            traffic.BarricadeHit -= OnBarricadeHit;
            traffic.BarricadeHit += OnBarricadeHit;
        }
    }
}
