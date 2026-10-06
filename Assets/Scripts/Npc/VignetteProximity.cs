using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using PussInHell.Core;

namespace PussInHell.Npc
{
    public class VignetteProximity : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private float maxDetectionRadius = 10f;
        [SerializeField] private float minDetectionRadius = 2f;

        [Header("Vignette Intensity")]
        [SerializeField] private float vignetteInside = 0.5f;
        [SerializeField] private float vignetteOutside = 0.3f;
        [Range(0.1f, 10f)]
        [SerializeField] private float smoothSpeed = 3f;

        [Tooltip("Volume with a Vignette override. Empty: first Volume in the scene.")]
        [SerializeField] private Volume volume;

        private Vignette vignette;
        private float currentIntensity;

        private void Start()
        {
            if (volume == null) volume = FindFirstObjectByType<Volume>();
            if (volume == null || volume.profile == null || !volume.profile.TryGet(out vignette))
            {
                enabled = false;
                return;
            }

            currentIntensity = vignetteOutside;
            vignette.intensity.Override(currentIntensity);
        }

        private void Update()
        {
            if (!PlayerLocator.TryGetDistance(transform.position, out float distance)) return;

            float target = distance <= minDetectionRadius
                ? vignetteInside
                : Mathf.Lerp(vignetteInside, vignetteOutside, Mathf.InverseLerp(minDetectionRadius, maxDetectionRadius, distance));

            currentIntensity = Mathf.Lerp(currentIntensity, target, Time.deltaTime * smoothSpeed);
            vignette.intensity.Override(currentIntensity);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, minDetectionRadius);
            Gizmos.color = new Color(1f, 0.6f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, maxDetectionRadius);
        }
    }
}
