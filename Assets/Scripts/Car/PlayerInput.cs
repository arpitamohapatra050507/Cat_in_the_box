using UnityEngine;
using RadiantGames.SpeedRacing.Multiplayer;
using Unity.VisualScripting;
using UnityEngine.UI;

namespace RadiantGames.SpeedRacing.Car
{
    [RequireComponent(typeof(CarController))]
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] MultiplayerOptions multiplayerOptions;
        CarController carController;

        [Header("Mobile Controls")]
        [SerializeField] bool mobileControls = false;
        bool isAccelerating = false;
        bool isBreaking = false;
        [SerializeField] GameObject mobileControlUI;
        void Start()
        {
            carController = GetComponent<CarController>();
            if(mobileControls) { mobileControlUI.SetActive(true); }
            else { mobileControlUI.SetActive(false); }
        }
        public void onAccPointerEnter() 
        { 
            isAccelerating = true;
            isBreaking = false;
            //print("Acc -> " + isAccelerating + '\n' + "Breaking -> " + isBreaking);
        }
        public void onBreakPointerEnter() 
        { 
            isBreaking = true;
            isAccelerating = false;
            //print("Acc -> " + isAccelerating + '\n' + "Breaking -> " + isBreaking);
        }
        public void onPointerExit() 
        {
            isAccelerating = false;
            isBreaking = false;
            //print("Acc -> " + isAccelerating + '\n' + "Breaking -> " + isBreaking);
        }
        void Update()
        {
            //If the game mode is multiplayer then let NetworkCar handle movement
            if(multiplayerOptions.gameMode == MultiplayerOptions.GameMode.Multiplayer) { return; }
            if (RaceHandler.RaceState != RaceState.Resumed) { return; }
            HandleInput();
        }
        public void HandleInput() 
        {
            if(mobileControls)
            {
                carController.steer = Input.acceleration.x;
                if(isAccelerating) { carController.throttle = 1; }
                else { carController.throttle = 0; }
                if (isBreaking) { carController.throttle = -1; }
                return;
            }
            carController.throttle = Input.GetAxis("Vertical");
            carController.steer = Input.GetAxis("Horizontal");
            if (Input.GetKey(KeyCode.LeftShift))
            {
                carController.isDrifting = true;
            }
            else
            {
                carController.isDrifting = false;
            }
            if (Input.GetKey(KeyCode.Q))
            {
                carController.isLookingBack = true;
            }
            else
            {
                carController.isLookingBack = false;
            }
        }
    }
}
