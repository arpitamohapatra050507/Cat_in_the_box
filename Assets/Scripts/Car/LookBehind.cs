using Cinemachine;
using RadiantGames.SpeedRacing.Car;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RadiantGames.SpeedRacing
{
    public class LookBehind : MonoBehaviour
    {
        CarController carScript;
        [SerializeField] CinemachineVirtualCamera vcam;
        void Start()
        {
            carScript = GetComponent<CarController>();
        }
        void Update()
        {
            var transposer = vcam.GetCinemachineComponent<CinemachineTransposer>();
            if (carScript.isLookingBack) 
            {
                transposer.m_FollowOffset.z = 20;
            }
            else
            {
                transposer.m_FollowOffset.z = -10;
            }
        }
    }
}
