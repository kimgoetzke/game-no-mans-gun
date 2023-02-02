using UnityEngine;
using Cinemachine;

namespace CaptainHindsight
{
    public class MCameraShake : MonoBehaviour
    {
        public static MCameraShake Instance { get; private set; }
        private CinemachineVirtualCamera cinemachineCamera;
        private float shakeTimer;
        private float shakeTimerTotal;
        private float startingIntensity;

        private void Awake()
        {
            Instance = this;
            cinemachineCamera = GetComponent<CinemachineVirtualCamera>();
        }

        public void ShakeCamera(float intensity, float time)
        {
            // Grab the noise profile we configured on the cinemachine camera
            CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = cinemachineCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

            // Set variables for time and intensity
            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = intensity;
            startingIntensity = intensity;
            shakeTimerTotal = time;
            shakeTimer = time;

        }

        private void Update()
        {
            if (shakeTimer > 0)
            {
                shakeTimer -= Time.deltaTime;

                CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = cinemachineCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

                // Stop camera shake slowly over time
                cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = Mathf.Lerp(startingIntensity, 0f, 1 - (shakeTimer / shakeTimerTotal));
            }
        }
    }
}