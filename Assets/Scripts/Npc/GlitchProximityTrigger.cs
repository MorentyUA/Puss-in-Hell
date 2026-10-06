using UnityEngine;
using PussInHell.Core;

namespace PussInHell.Npc
{
    public class GlitchProximityTrigger : MonoBehaviour
    {
        [SerializeField] private float maxDetectionRadius = 10f;
        [SerializeField] private float minDetectionRadius = 2f;
        [Range(0f, 1f)]
        [SerializeField] private float hackPower = 1f;

        private void Update()
        {
            if (GlitchIntensityHub.Instance == null) return;
            if (!PlayerLocator.TryGetDistance(transform.position, out float distance)) return;
            if (distance > maxDetectionRadius) return;

            float intensity = distance <= minDetectionRadius
                ? 1f
                : 1f - Mathf.InverseLerp(minDetectionRadius, maxDetectionRadius, distance);

            GlitchIntensityHub.Instance.Report(intensity * hackPower);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, minDetectionRadius);
            Gizmos.color = new Color(1f, 0.3f, 0f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, maxDetectionRadius);
        }
    }
}
