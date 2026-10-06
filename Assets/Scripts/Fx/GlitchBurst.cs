using UnityEngine;
using PussInHell.Npc;

namespace PussInHell.Fx
{
    public class GlitchBurst : MonoBehaviour
    {
        [Range(0f, 1f)]
        [SerializeField] private float peakIntensity = 1f;
        [SerializeField] private float duration = 0.8f;
        [SerializeField] private AnimationCurve falloff = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        private float elapsed;
        private bool isActive;

        public void Play()
        {
            elapsed = 0f;
            isActive = true;
        }

        public void Stop()
        {
            isActive = false;
        }

        private void Update()
        {
            if (!isActive) return;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            if (GlitchIntensityHub.Instance != null)
                GlitchIntensityHub.Instance.Report(peakIntensity * falloff.Evaluate(t));

            if (t >= 1f) isActive = false;
        }
    }
}
