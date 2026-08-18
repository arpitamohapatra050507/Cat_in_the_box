using RadiantTools.AudioSystem;
using RadiantGames.SpeedRacing.Car;
using UnityEngine;

namespace RadiantGames.SpeedRacing
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CarController))]
    public class CarSound : MonoBehaviour
    {
        Rigidbody rb;
        AudioPlayer enginePlayer;
        AudioPlayer driftPlayer;
        CarController carScript;
        CarData carData;

        [Header("Sound Settings")]
        [Space(20f)]
        public bool enableSounds = false;
        [SerializeField] float minEnginePitch;
        [SerializeField] float maxEnginePitch;
        float minEngineSpeed = 4f;
        float maxEngineSpeed = 26f;
        float pitchFromCar;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            carScript = GetComponent<CarController>();
            carData = carScript.carData;
        }
        void Update()
        {
            if (!enableSounds) { return; }
            PlayEngineSound();
            PlayDriftSound();
        }

        void OnCollisionEnter(Collision collision)
        {
            if(rb.velocity.magnitude < carData.minDriftVelocity) { return; }
            AudioManager.Instance.GetAudioPlayer("SoundSFX").PlayAudioOnce(SoundTypes.CollisionSound);
        }

        public void PlayEngineSound()
        {
            if (enginePlayer == null) { InitializeAudioPlayers(); }

            float currentSpeed = rb.velocity.magnitude;
            pitchFromCar = currentSpeed / 50f;

            if (currentSpeed < minEngineSpeed)
            {
                enginePlayer.SetAudioSettings(pitch: minEnginePitch, loop: true, playOnStart: true);
            }
            else if (currentSpeed > maxEngineSpeed)
            {
                enginePlayer.SetAudioSettings(pitch: maxEnginePitch, loop: true, playOnStart: true);
            }
            else
            {
                enginePlayer.SetAudioSettings(pitch: minEnginePitch + pitchFromCar, loop: true, playOnStart: true);
            }
        }
        public void PlayDriftSound()
        {
            //Play drift sound
            if (carScript.isDrifting && rb.velocity.magnitude > carData.minDriftVelocity)
            {
                driftPlayer.SetAudioSettings(volume: 1, loop: true, playOnStart: true);
            }
            else
            {
                driftPlayer.SetAudioSettings(volume: 0, loop: true, playOnStart: true);
            }
        }
        void InitializeAudioPlayers()
        {
            enginePlayer = AudioManager.Instance.MakeAudioPlayer("EngineSound");
            enginePlayer.SetAudioClip(enginePlayer.GetAudioClip(SoundTypes.EngineSound));
            enginePlayer.SetAudioSettings(playOnStart: true, loop: true, volume: 0f);
            enginePlayer.PlayAudio();

            driftPlayer = AudioManager.Instance.MakeAudioPlayer("DriftPlayer");
            driftPlayer.SetAudioClip(driftPlayer.GetAudioClip(SoundTypes.TireSound));
            driftPlayer.SetAudioSettings(playOnStart: true, loop: true, volume: 0f);
            driftPlayer.PlayAudio();
        }
    }
}
