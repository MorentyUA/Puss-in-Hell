using UnityEngine;
using UnityEngine.Events;
using PussInHell.Core;

namespace PussInHell.Interaction
{
    public class ObjectInteraction : MonoBehaviour
    {
        [Header("Interaction")]
        [SerializeField] private float interactionRadius = 20f;
        [SerializeField] private KeyCode interactionKey = KeyCode.E;
        [Tooltip("Distance is measured from here. Empty: child named 'Press Point' or this transform.")]
        [SerializeField] private Transform pressPoint;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [UnityEngine.Serialization.FormerlySerializedAs("animationBoolName")]
        [SerializeField] private string animationParameter = "falling";
        [Tooltip("On: set a bool. Off: fire a trigger.")]
        [SerializeField] private bool useBool = true;

        public UnityEvent onInteracted;

        public bool HasInteracted { get; private set; }
        public bool PlayerInRange { get; private set; }

        private Transform Point => pressPoint != null ? pressPoint : transform;

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            if (pressPoint == null) pressPoint = transform.Find("Press Point");
        }

        private void Update()
        {
            if (HasInteracted) return;
            if (!PlayerLocator.TryGetDistance(Point.position, out float distance)) return;

            PlayerInRange = distance <= interactionRadius;
            if (PlayerInRange && Input.GetKeyDown(interactionKey))
                Interact();
        }

        public void Interact()
        {
            if (HasInteracted) return;
            HasInteracted = true;

            if (animator != null)
            {
                if (useBool) animator.SetBool(animationParameter, true);
                else animator.SetTrigger(animationParameter);
            }

            onInteracted?.Invoke();
        }

        private void OnDrawGizmosSelected()
        {
            var point = pressPoint != null ? pressPoint : (transform.Find("Press Point") ?? transform);
            Gizmos.color = PlayerInRange ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(point.position, interactionRadius);
        }
    }
}
