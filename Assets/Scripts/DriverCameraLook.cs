using UnityEngine;

namespace LastPassenger
{
    public sealed class DriverCameraLook : MonoBehaviour
    {
        [Header("Hold right mouse button to look")]
        [SerializeField] private float sensitivity = 0.08f;
        [SerializeField] private float yawLimit = 42f;
        [SerializeField] private float pitchLimit = 14f;
        [SerializeField] private float smoothing = 14f;
        [SerializeField] private float returnSpeed = 36f;

        private Vector2 targetAngles;
        private Vector2 currentAngles;

        private void Update()
        {
            float deltaTime = Time.unscaledDeltaTime;

            if (PrototypeInput.CameraLookHeld)
            {
                Vector2 mouseDelta = PrototypeInput.CameraLookDelta;
                targetAngles.x = Mathf.Clamp(
                    targetAngles.x - mouseDelta.y * sensitivity,
                    -pitchLimit,
                    pitchLimit);
                targetAngles.y = Mathf.Clamp(
                    targetAngles.y + mouseDelta.x * sensitivity,
                    -yawLimit,
                    yawLimit);
            }
            else
            {
                targetAngles = Vector2.MoveTowards(targetAngles, Vector2.zero, returnSpeed * deltaTime);
            }

            float blend = 1f - Mathf.Exp(-smoothing * deltaTime);
            currentAngles = Vector2.Lerp(currentAngles, targetAngles, blend);
            transform.localRotation = Quaternion.Euler(currentAngles.x, currentAngles.y, 0f);
        }
    }
}
