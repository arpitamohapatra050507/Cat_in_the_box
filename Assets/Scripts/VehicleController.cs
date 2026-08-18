using UnityEngine;

namespace LastPassenger
{
    public sealed class VehicleController : MonoBehaviour
    {
        [Header("Lane driving")]
        [SerializeField] private float maximumSpeed = 10f;
        [SerializeField] private float acceleration = 3.4f;
        [SerializeField] private float braking = 7f;
        [SerializeField] private float naturalDrag = 0.8f;
        [SerializeField] private float steeringSpeed = 3.8f;
        [SerializeField] private float steeringResponse = 2.2f;
        [SerializeField] private float steeringReturnSpeed = 3f;
        [SerializeField] private float laneLimit = 3.25f;

        private AudioSource engineSource;
        private AudioSource impactSource;
        private float speed;
        private float steeringInput;
        private float edgeCooldown;
        private bool controlsEnabled = true;

        public float Speed => speed;
        public float MaximumSpeed => maximumSpeed;
        public float Distance => transform.position.z;
        public float LanePosition => transform.position.x;

        public void ConfigureAudio(AudioClip engine, AudioClip impact)
        {
            engineSource = gameObject.AddComponent<AudioSource>();
            engineSource.clip = engine;
            engineSource.loop = true;
            engineSource.spatialBlend = 0f;
            engineSource.volume = 0.2f;
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

        private void Update()
        {
            edgeCooldown -= Time.deltaTime;

            float throttle = controlsEnabled && PrototypeInput.AccelerateHeld ? 1f : 0f;
            bool brakingInput = controlsEnabled && PrototypeInput.BrakeHeld;

            if (throttle > 0f)
            {
                speed = Mathf.MoveTowards(speed, maximumSpeed, acceleration * Time.deltaTime);
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

            Vector3 position = transform.position;
            position.z += speed * Time.deltaTime;
            position.x += steeringInput * steeringSpeed * Mathf.Lerp(0.35f, 1f, speed / maximumSpeed) * Time.deltaTime;

            float unclampedX = position.x;
            position.x = Mathf.Clamp(position.x, -laneLimit, laneLimit);

            if (!Mathf.Approximately(unclampedX, position.x) && edgeCooldown <= 0f)
            {
                speed *= 0.65f;
                edgeCooldown = 0.45f;
                impactSource?.Play();
                PrototypeGameManager.Instance?.NotifyRoadImpact();
            }

            transform.position = position;

            float lean = -steeringInput * Mathf.Lerp(0f, 2.2f, speed / maximumSpeed);
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.Euler(0f, steeringInput * 1.2f, lean),
                Time.deltaTime * 5f);

            if (engineSource != null)
            {
                engineSource.pitch = Mathf.Lerp(0.72f, 1.45f, speed / maximumSpeed);
                engineSource.volume = Mathf.Lerp(0.12f, 0.32f, speed / maximumSpeed);
            }
        }
    }
}
