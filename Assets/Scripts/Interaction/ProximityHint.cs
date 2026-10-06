using System;
using UnityEngine;
using UnityEngine.Events;
using PussInHell.Core;
using PussInHell.Hints;

namespace PussInHell.Interaction
{
    public class ProximityHint : MonoBehaviour
    {
        [Header("Radii")]
        [Tooltip("Hint becomes visible inside this distance")]
        [SerializeField] private float interactionRadius = 3f;
        [Tooltip("Hint pulses and the key works inside this distance")]
        [SerializeField] private float pulseDistance = 2f;
        [Tooltip("Hint hides when the player is closer than this")]
        [SerializeField] private float disappearDistance = 1f;

        [Header("Interaction")]
        [SerializeField] private bool isOneTimeInteract = false;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private string messageKey = "hint_default";
        [SerializeField] private float messageDuration = 5f;

        public UnityEvent onInteracted;

        public bool HasInteracted { get; private set; }
        public bool IsVisible { get; private set; }
        public bool IsNear { get; private set; }
        public float Distance { get; private set; } = float.PositiveInfinity;

        public event Action<bool> VisibilityChanged;
        public event Action<bool> NearChanged;
        public event Action Interacted;

        private void Update()
        {
            if (HasInteracted) return;
            if (!PlayerLocator.TryGetDistance(transform.position, out float distance)) return;
            Distance = distance;

            if (isOneTimeInteract && distance <= pulseDistance && Input.GetKeyDown(interactKey))
            {
                Interact();
                return;
            }

            SetVisible(distance >= disappearDistance && distance <= interactionRadius);
            SetNear(distance <= pulseDistance && distance > disappearDistance);
        }

        public void Interact()
        {
            if (HasInteracted) return;
            HasInteracted = true;
            SetVisible(false);
            SetNear(false);

            if (HintMessageService.Instance != null && !string.IsNullOrEmpty(messageKey))
                HintMessageService.Instance.Show(messageKey, messageDuration);

            onInteracted?.Invoke();
            Interacted?.Invoke();
        }

        private void SetVisible(bool visible)
        {
            if (IsVisible == visible) return;
            IsVisible = visible;
            VisibilityChanged?.Invoke(visible);
        }

        private void SetNear(bool near)
        {
            if (IsNear == near) return;
            IsNear = near;
            NearChanged?.Invoke(near);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, interactionRadius);
            Gizmos.color = Color.green; Gizmos.DrawWireSphere(transform.position, pulseDistance);
            Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, disappearDistance);
        }
    }
}
