using RadiantTools.SaveSystem;
using System;
using UnityEngine;

namespace RadiantGames.SpeedRacing
{
    [CreateAssetMenu(menuName = "Scritable Objects/Car Data",fileName ="CarDataObject")]
    public class CarData : ScriptableObject
    {
        [Header("Prefab")]
        public GameObject carPrefab;
        [Header("Car Details")]
        public string carName;
        public int price;
        public bool isOwned = false;
        public bool isEquipped = false;
        [Header("Car Stats")]
        public float torque = 20000f;
        public float maxSteerAngle = 20f;
        public float maxVelocity = 35f;
        public float minDriftVelocity = 15f; //Min speed for drifting
    }
}
