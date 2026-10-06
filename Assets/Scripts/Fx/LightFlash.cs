using UnityEngine;

namespace PussInHell.Fx
{
    public class LightFlash : MonoBehaviour
    {
        [SerializeField] private Light targetLight;
        [SerializeField] private float peakIntensity = 20f;
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private AnimationCurve falloff = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        [Tooltip("Light is disabled outside the flash")]
        [SerializeField] private bool disableWhenIdle = true;

        private float baseIntensity;
        private float elapsed;
        private bool isActive;

        private void Awake()
        {
            if (targetLight == null) targetLight = GetComponent<Light>();
            if (targetLight == null) return;

            baseIntensity = disableWhenIdle ? 0f : targetLight.intensity;
            if (disableWhenIdle) targetLight.enabled = false;
        }

        public void Flash()
        {
            if (targetLight == null) return;
            elapsed = 0f;
            isActive = true;
            targetLight.enabled = true;
        }

        private void Update()
        {
            if (!isActive || targetLight == null) return;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            targetLight.intensity = Mathf.Lerp(baseIntensity, peakIntensity, falloff.Evaluate(t));

            if (t < 1f) return;

            isActive = false;
            targetLight.intensity = baseIntensity;
            if (disableWhenIdle) targetLight.enabled = false;
        }
    }
}
