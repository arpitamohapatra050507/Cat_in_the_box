using UnityEngine;

namespace LastPassenger
{
    public sealed class VehicleController : MonoBehaviour
    {
        [Header("Lane driving")]
        [SerializeField] private float maximumSpeed = 18f;
        [SerializeField] private float lateRunMaximumSpeed = 23.5f;
        [SerializeField] private float speedRampStartsAfter = 60f;
        [SerializeField] private float speedRampDuration = 240f;
        [SerializeField] private float acceleration = 3.4f;
        [SerializeField] private float braking = 7f;
        [SerializeField] private float naturalDrag = 0.8f;
        [SerializeField] private float steeringResponse = 2.2f;
        [SerializeField] private float steeringReturnSpeed = 3f;
        [SerializeField] private float maximumTurnRate = 26f;
        [SerializeField] private float maximumHeading = 14f;
        [SerializeField] private float roadAlignmentSpeed = 8f;
        [SerializeField] private float laneLimit = 3.25f;

        [Header("Engine mix")]
        [SerializeField, Range(0f, 0.5f)] private float maximumEngineVolume = 0.5f;
        [SerializeField] private float engineVolumeResponse = 0.75f;

        private AudioSource engineSource;
        private AudioSource impactSource;
        private float speed;
        private float steeringInput;
        private float heading;
        private float visualRoll;
        private float edgeCooldown;
        private float currentMaximumSpeed;
        private bool controlsEnabled = true;
        private bool cliffFalling;
        private float cliffFallStartedAt;

        public float Speed => speed;
        public float MaximumSpeed => currentMaximumSpeed > 0f ? currentMaximumSpeed : maximumSpeed;
        public float Distance => transform.position.z;
        public float LanePosition => transform.position.x;
        public float SteeringInput => steeringInput;
        public float Heading => heading;
        public float SpeedRatio => MaximumSpeed > 0f ? Mathf.Clamp01(speed / MaximumSpeed) : 0f;

        public void ConfigureAudio(AudioClip engine, AudioClip impact)
        {
            engineSource = gameObject.AddComponent<AudioSource>();
            engineSource.clip = engine;
            engineSource.loop = true;
            engineSource.spatialBlend = 0f;
            engineSource.volume = 0f;
            engineSource.Play();

            impactSource = gameObject.AddComponent<AudioSource>();
            impactSource.clip = impact;
            impactSource.spatialBlend = 0f;
            impactSource.volume = 0.4f;
        }

        public void SetControlsEnabled(bool enabled)
        {
            controlsEnabled = enabled;
            if (!enabled)
            {
                speed = Mathf.MoveTowards(speed, 0f, braking * Time.deltaTime);
            }
        }

        public void TeleportForward(float distance)
        {
            Vector3 position = transform.position;
            position.z = Mathf.Max(position.z, distance);
            transform.position = position;
        }

        public void ApplyImpact(float retainedSpeed)
        {
            speed *= Mathf.Clamp01(retainedSpeed);
            heading = Mathf.Clamp(
                heading + Random.Range(-4.5f, 4.5f),
                -maximumHeading,
                maximumHeading);
            visualRoll += Random.Range(-2.2f, 2.2f);
            edgeCooldown = Mathf.Max(edgeCooldown, 0.3f);
            impactSource?.Play();
        }

        public void BeginCliffFall()
        {
            controlsEnabled = false;
            cliffFalling = true;
            cliffFallStartedAt = Time.time;
            speed = Mathf.Max(speed, MaximumSpeed * 0.82f);
            heading *= 0.35f;
        }

        private void Update()
        {
            UpdateMaximumSpeed();
            if (cliffFalling)
            {
                UpdateCliffFall();
                return;
            }

            edgeCooldown -= Time.deltaTime;

            float throttle = controlsEnabled && PrototypeInput.AccelerateHeld ? 1f : 0f;
            bool brakingInput = controlsEnabled && PrototypeInput.BrakeHeld;

            if (throttle > 0f)
            {
                speed = Mathf.MoveTowards(speed, MaximumSpeed, acceleration * Time.deltaTime);
            }
            else
            {
                speed = Mathf.MoveTowards(speed, 0f, naturalDrag * Time.deltaTime);
            }

            if (brakingInput)
            {
                speed = Mathf.MoveTowards(speed, 0f, braking * Time.deltaTime);
            }

            float targetSteering = 0f;
            if (controlsEnabled && PrototypeInput.LeftHeld) targetSteering -= 1f;
            if (controlsEnabled && PrototypeInput.RightHeld) targetSteering += 1f;

            float steeringChangeSpeed = Mathf.Approximately(targetSteering, 0f)
                ? steeringReturnSpeed
                : steeringResponse;
            steeringInput = Mathf.MoveTowards(
                steeringInput,
                targetSteering,
                steeringChangeSpeed * Time.deltaTime);

            float speedRatio = SpeedRatio;
            float steeringAuthority = Mathf.Lerp(0.18f, 1f, speedRatio);
            if (Mathf.Abs(steeringInput) > 0.01f)
            {
                heading += steeringInput * maximumTurnRate * steeringAuthority * Time.deltaTime;
            }
            else
            {
                heading = Mathf.MoveTowards(heading, 0f, roadAlignmentSpeed * Time.deltaTime);
            }
            heading = Mathf.Clamp(heading, -maximumHeading, maximumHeading);

            Vector3 position = transform.position;
            Vector3 travelDirection = Quaternion.Euler(0f, heading, 0f) * Vector3.forward;
            position += travelDirection * (speed * Time.deltaTime);

            float unclampedX = position.x;
            position.x = Mathf.Clamp(position.x, -laneLimit, laneLimit);

            bool hitRoadEdge = !Mathf.Approximately(unclampedX, position.x);
            if (hitRoadEdge)
            {
                float inwardHeading = -Mathf.Sign(position.x) * 5f;
                heading = Mathf.MoveTowards(heading, inwardHeading, 24f * Time.deltaTime);

                if (edgeCooldown <= 0f)
                {
                    speed *= 0.65f;
                    edgeCooldown = 0.45f;
                    impactSource?.Play();
                    PrototypeGameManager.Instance?.NotifyRoadImpact();
                }
            }

            transform.position = position;

            float targetRoll = -steeringInput * Mathf.Lerp(0f, 2.2f, speedRatio);
            visualRoll = Mathf.Lerp(visualRoll, targetRoll, Time.deltaTime * 5f);
            transform.rotation = Quaternion.Euler(0f, heading, visualRoll);

            if (engineSource != null)
            {
                engineSource.pitch = Mathf.Lerp(0.72f, 1.45f, speedRatio);
                float targetEngineVolume = speedRatio * maximumEngineVolume;
                engineSource.volume = Mathf.MoveTowards(
                    engineSource.volume,
                    targetEngineVolume,
                    engineVolumeResponse * Time.deltaTime);

                if (speed <= 0.001f)
                {
                    engineSource.volume = 0f;
                }
            }
        }

        private void UpdateCliffFall()
        {
            float elapsed = Time.time - cliffFallStartedAt;
            speed = Mathf.MoveTowards(speed, MaximumSpeed * 1.08f, acceleration * Time.deltaTime);
            Vector3 position = transform.position;
            position += Vector3.forward * speed * Time.deltaTime;
            if (elapsed > 0.65f)
            {
                float fallTime = elapsed - 0.65f;
                position.y -= (2.5f + fallTime * 9f) * Time.deltaTime;
            }
            transform.position = position;
            transform.rotation = Quaternion.Euler(
                Mathf.Lerp(0f, 34f, Mathf.Clamp01(elapsed / 2.2f)),
                heading,
                Mathf.Sin(elapsed * 2.8f) * 5f);

            if (engineSource != null)
            {
                engineSource.pitch = Mathf.Lerp(1.38f, 1.7f, Mathf.Clamp01(elapsed / 2f));
                engineSource.volume = maximumEngineVolume;
            }
        }

        private void UpdateMaximumSpeed()
        {
            float ramp = Mathf.InverseLerp(
                speedRampStartsAfter,
                speedRampStartsAfter + Mathf.Max(1f, speedRampDuration),
                Time.timeSinceLevelLoad);
            ramp = ramp * ramp * (3f - 2f * ramp);
            currentMaximumSpeed = Mathf.Lerp(
                maximumSpeed,
                Mathf.Max(maximumSpeed, lateRunMaximumSpeed),
                ramp);
        }
    }
}
