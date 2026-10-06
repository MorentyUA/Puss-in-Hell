using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace PussInHell.Cinematics
{
    public class DelayedTeleport : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Transform destination;
        [SerializeField] private bool applyDestinationRotation = true;

        [Header("Timing")]
        [Tooltip("Seconds between Play() and the jump")]
        [SerializeField] private float delay = 4f;
        [SerializeField] private bool once = true;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [Tooltip("Animator trigger fired at the jump. Empty: none.")]
        [SerializeField] private string animatorTrigger = "";

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip teleportSound;

        public UnityEvent onTeleported;

        private Coroutine routine;

        public bool HasFired { get; private set; }

        public void Play()
        {
            if (once && HasFired) return;
            if (routine != null || target == null || destination == null) return;
            routine = StartCoroutine(Routine());
        }

        public void Cancel()
        {
            if (routine == null) return;
            StopCoroutine(routine);
            routine = null;
        }

        public void TeleportNow()
        {
            Cancel();
            Teleport();
        }

        private IEnumerator Routine()
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);
            routine = null;
            Teleport();
        }

        private void Teleport()
        {
            if (target == null || destination == null) return;

            if (applyDestinationRotation) target.SetPositionAndRotation(destination.position, destination.rotation);
            else target.position = destination.position;

            var body = target.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            if (animator != null && !string.IsNullOrEmpty(animatorTrigger)) animator.SetTrigger(animatorTrigger);
            if (audioSource != null && teleportSound != null) audioSource.PlayOneShot(teleportSound);

            HasFired = true;
            onTeleported?.Invoke();
        }

        private void OnDrawGizmosSelected()
        {
            if (target == null || destination == null) return;
            Gizmos.color = HasFired ? Color.green : Color.magenta;
            Gizmos.DrawLine(target.position, destination.position);
            Gizmos.DrawWireSphere(destination.position, 0.3f);
            Gizmos.DrawRay(destination.position, destination.forward * 1.5f);
        }
    }
}
