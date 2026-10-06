using UnityEngine;
using FronkonGames.Glitches.Hacked;

namespace PussInHell.Npc
{
    public class GlitchIntensityHub : MonoBehaviour
    {
        public static GlitchIntensityHub Instance { get; private set; }

        [Tooltip("Hacked renderer feature from the URP renderer asset")]
        [SerializeField] private Hacked hackedFeature;
        [SerializeField] private float smoothSpeed = 5f;

        private float currentIntensity;
        private float targetIntensity;

        public float CurrentIntensity => currentIntensity;
        public bool IsActive { get; private set; }

        private void Awake()
        {
            Instance = this;
            Apply(0f);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * smoothSpeed);
            Apply(currentIntensity);

            if (!IsActive && targetIntensity > 0f) IsActive = true;
            else if (IsActive && targetIntensity <= 0f && currentIntensity <= 0.01f) IsActive = false;

            targetIntensity = 0f;
        }

        public void Report(float intensity)
        {
            if (intensity > targetIntensity) targetIntensity = intensity;
        }

        private void Apply(float value)
        {
            if (hackedFeature == null || hackedFeature.settings == null) return;
            hackedFeature.settings.intensity = value;
        }
    }
}
