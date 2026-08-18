using UnityEngine;

namespace LastPassenger
{
    public sealed class SteeringWheelAnimator : MonoBehaviour
    {
        [SerializeField] private float maximumWheelAngle = 115f;
        [SerializeField] private float animationSpeed = 12f;

        private VehicleController vehicle;
        private Quaternion neutralRotation;

        public void Configure(VehicleController controller)
        {
            vehicle = controller;
            neutralRotation = transform.localRotation;
        }

        private void Update()
        {
            if (vehicle == null) return;

            Quaternion target = neutralRotation * Quaternion.AngleAxis(
                -vehicle.SteeringInput * maximumWheelAngle,
                Vector3.forward);
            float blend = 1f - Mathf.Exp(-animationSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, target, blend);
        }
    }
}
