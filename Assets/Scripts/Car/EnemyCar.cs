using UnityEngine;
using RadiantGames.SpeedRacing.Racetrack;
using RadiantTools.AudioSystem;
using System.Collections;
using System;

namespace RadiantGames.SpeedRacing.Car
{
    [RequireComponent(typeof(CarController))]
    public class EnemyCar : MonoBehaviour
    {
        CarController enemyCar;
        LapTracker lapTracker;
        //If the car crashes we need to reverse the car
        bool autoControl = true;

        //Setting EnemyCar throttle and Steering according to the waypoint
        void Start()
        {
            enemyCar = gameObject.GetComponent<CarController>();
            lapTracker = GetComponent<LapTracker>();
        }
        void Update()
        {
            //If race is not resumed or lapTracker does not exist then return
            if( !lapTracker || RaceHandler.RaceState != RaceState.Resumed) { return; }
            Vector3 checkpointDirection = GetCheckpointDirection();
            enemyCar.steer = CalculateSteer(checkpointDirection);
            if (!autoControl) { return; }
            enemyCar.throttle = CalculateThrottle(enemyCar.steer);
            
            //Just For Debugging
            Debug.DrawRay(transform.position, checkpointDirection * 10, Color.cyan);
            Debug.DrawRay(transform.position, transform.forward * 10, Color.magenta);
        }

        Vector3 GetCheckpointDirection()
        {
            Vector3 checkpointPosition = lapTracker.GetClosestPoint(lapTracker.GetNextCheckpoint().GetComponent<BoxCollider>());
            //Direction = Destination - Source
            return (checkpointPosition - transform.position).normalized;
        }

        float CalculateThrottle(float steer)
        {
            if (steer >= 0.6f || steer <= -0.6f)
            {
                return -1f;
            }
            else if (steer >= 0.4f || steer <= -0.4f)
            {
                return -0.8f;
            }
            else
            {
                return 1 - enemyCar.steer;
            }
        }

        float CalculateSteer(Vector3 checkpointDir)
        {
            float angle = Vector3.SignedAngle(transform.forward.normalized, checkpointDir, Vector3.up);
            return angle / 180f;
        }
        IEnumerator OnCollisionEnter(Collision collision)
        {
            if(collision.gameObject.CompareTag("EnemyCar") || collision.gameObject.CompareTag("PlayerCar"))
            {
                yield return new WaitForSeconds(0f);
            }
            else
            {
                autoControl = false;
                enemyCar.throttle = -1f;
                yield return new WaitForSeconds(2f);
                autoControl = true;
            }
        }
    }
}
