using UnityEngine;
using Cinemachine;

namespace PussInHell.Fx
{
    public class CinemachineNoiseShake : MonoBehaviour
    {
        [Tooltip("Virtual camera with a Basic Multi Channel Perlin noise component")]
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [SerializeField] private float amplitude = 2f;
        [SerializeField] private float frequency = 3f;
        [SerializeField] private float duration = 0.6f;
        [SerializeField] private AnimationCurve falloff = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        private CinemachineBasicMultiChannelPerlin noise;
        private float elapsed;
        private bool isActive;

        private void Awake()
        {
            if (virtualCamera != null)
                noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            Apply(0f);
        }

        public void Shake()
        {
            elapsed = 0f;
            isActive = true;
        }

        public void Stop()
        {
            isActive = false;
            Apply(0f);
        }

        private void Update()
        {
            if (!isActive) return;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Apply(falloff.Evaluate(t));

            if (t >= 1f)
            {
                isActive = false;
                Apply(0f);
            }
        }

        private void Apply(float strength)
        {
            if (noise == null) return;
            noise.m_AmplitudeGain = amplitude * strength;
            noise.m_FrequencyGain = frequency;
        }
    }
}
