using RadiantTools.AudioSystem;
using System;
using UnityEngine;

namespace RadiantGames.SpeedRacing.Car
{
    [RequireComponent(typeof(Rigidbody))]
    public class CarController : MonoBehaviour
    {
        Rigidbody rb;
        //These will be used by Input to manipulate the car
        [Header("Car Statistics")]
        public CarData carData;
        [NonSerialized] public float throttle;
        [NonSerialized] public float steer;
        [NonSerialized] public bool isDrifting;
        [NonSerialized] public bool isLookingBack;

        [Header("Car References")]
        [Space(10f)]
        [SerializeField] Transform centerOfMass;
        [Header("Car Effects")]
        [SerializeField] ParticleSystem[] smokeEffect;
        [SerializeField] TrailRenderer[] tireMarks;
        [Header("Wheels")]
        [SerializeField] WheelCollider[] wheelColliders;
        [SerializeField] WheelCollider[] frontWheels;
        [SerializeField] GameObject[] wheelMeshes;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.centerOfMass = centerOfMass.localPosition;
        }
        void FixedUpdate()
        {
            AccelerateCar();
            SteerCar();
            DriftCar();
        }
        void SteerCar()
        {
            foreach (WheelCollider wheel in frontWheels)
            {
                wheel.steerAngle = carData.maxSteerAngle * steer;
                wheel.gameObject.transform.localEulerAngles = new Vector3(0f, carData.maxSteerAngle * steer, 0f);
            }
        }
        void AccelerateCar()
        {
            foreach (WheelCollider wheel in wheelColliders)
            {
                wheel.motorTorque = carData.torque * Time.deltaTime * throttle;
            }
            //This will give the effect that the wheel itself is turning to push the car forward
            foreach (GameObject mesh in wheelMeshes)
            {
                mesh.transform.Rotate(rb.velocity.magnitude * (transform.InverseTransformDirection(rb.velocity).z >= 0 ? 1 : -1) / (2 * Mathf.PI * wheelColliders[0].radius),0,0);
            }
            LimitMaxVelocity();
        }
        void LimitMaxVelocity()
        {
            //Limiting the speed of the car
            if (rb.velocity.magnitude > carData.maxVelocity)
            {
                rb.velocity = rb.velocity.normalized * carData.maxVelocity;
            }
        }
        void DriftCar()
        {
            if(rb.velocity.magnitude < carData.minDriftVelocity) 
            {
                isDrifting = false;
                return; 
            }
            //For Effects
            foreach(TrailRenderer trail in tireMarks)
            {
                trail.emitting = isDrifting;
            }
            foreach (ParticleSystem particleSystem in smokeEffect)
            {
                if (isDrifting) { particleSystem.Play(); }
                else { particleSystem.Stop(); }
            }
            //Drifting Logic
            foreach (WheelCollider wheelCollider in wheelColliders)
            {
                WheelFrictionCurve sidewaysFriction = wheelCollider.sidewaysFriction;
                WheelFrictionCurve forwardFriction = wheelCollider.forwardFriction;
                if (isDrifting) 
                { 
                    sidewaysFriction.asymptoteValue = 0.6f;
                    forwardFriction.asymptoteValue = 0.9f;
                    //If wheels are forward wheels
                    if(wheelCollider.gameObject.name == "RearLeftWheel" || wheelCollider.gameObject.name == "RearRightWheel")
                    {
                        sidewaysFriction.stiffness = 0.6f;
                        forwardFriction.stiffness = 0.6f;
                    }
                }
                else 
                {
                    sidewaysFriction.asymptoteValue = 0.75f;
                    forwardFriction.asymptoteValue = 0.5f;
                    //If wheels are forward wheels
                    if (wheelCollider.gameObject.name == "RearLeftWheel" || wheelCollider.gameObject.name == "RearRightWheel")
                    {
                        sidewaysFriction.stiffness = 1f;
                        forwardFriction.stiffness = 1f;
                    }
                }
                wheelCollider.sidewaysFriction = sidewaysFriction;
                wheelCollider.forwardFriction = forwardFriction;
            }
        }
    }
}
