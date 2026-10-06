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
        [Tooltip("0 = instant teleport, otherwise seconds of a visible rush to the destination")]
        [SerializeField] private float dashDuration = 0.15f;
        [SerializeField] private AnimationCurve dashCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private bool once = true;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [Tooltip("Animator trigger fired at the jump. Empty: none.")]
        [SerializeField] private string animatorTrigger = "";

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip teleportSound;

        [Header("Events")]
        public UnityEvent onDashStarted;
        public UnityEvent onTeleported;

        private Coroutine routine;
        private bool isDashing;
        private float dashElapsed;
        private Vector3 dashFromPosition;
        private Quaternion dashFromRotation;

        public bool HasFired { get; private set; }

        public void Play()
        {
            if (once && HasFired) return;
            if (routine != null || isDashing || target == null || destination == null) return;
            routine = StartCoroutine(Routine());
        }

        public void Cancel()
        {
            isDashing = false;
            if (routine == null) return;
            StopCoroutine(routine);
            routine = null;
        }

        public void TeleportNow()
        {
            Cancel();
            Arrive();
        }

        private IEnumerator Routine()
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);
            routine = null;
            StartDash();
        }

        private void StartDash()
        {
            if (target == null || destination == null) return;

            onDashStarted?.Invoke();

            if (dashDuration <= 0f)
            {
                Arrive();
                return;
            }

            dashFromPosition = target.position;
            dashFromRotation = target.rotation;
            dashElapsed = 0f;
            isDashing = true;
        }

        private void LateUpdate()
        {
            if (!isDashing) return;

            dashElapsed += Time.deltaTime;
            float t = dashCurve.Evaluate(Mathf.Clamp01(dashElapsed / dashDuration));
            target.position = Vector3.LerpUnclamped(dashFromPosition, destination.position, t);
            if (applyDestinationRotation)
                target.rotation = Quaternion.SlerpUnclamped(dashFromRotation, destination.rotation, t);

            if (dashElapsed >= dashDuration)
            {
                isDashing = false;
                Arrive();
            }
        }

        private void Arrive()
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
