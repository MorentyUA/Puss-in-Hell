using System;
using UnityEngine;
using UnityEngine.Events;
using PussInHell.Core;

namespace PussInHell.Interaction
{
    public class Interactable : MonoBehaviour
    {
        [Min(0f)]
        [SerializeField] private float activationRadius = 3f;
        [Tooltip("Off: activates as soon as the player enters the radius")]
        [SerializeField] private bool requireInteractKey = true;
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        [Header("Events")]
        public UnityEvent onRangeEntered;
        public UnityEvent onRangeExited;
        public UnityEvent onActivated;

        public static event Action<Interactable> AnyActivated;

        public bool IsActivated { get; private set; }
        public bool IsInRange { get; private set; }
        public float ActivationRadius => activationRadius;

        private void Update()
        {
            if (IsActivated) return;
            if (!PlayerLocator.TryGetDistance(transform.position, out float distance)) return;

            bool inRange = distance < activationRadius;
            if (inRange != IsInRange)
            {
                IsInRange = inRange;
                if (inRange) onRangeEntered?.Invoke();
                else onRangeExited?.Invoke();
            }

            if (!IsInRange) return;
            if (!requireInteractKey || Input.GetKeyDown(interactKey))
                Activate();
        }

        public void Activate()
        {
            if (IsActivated) return;

            if (IsInRange)
            {
                IsInRange = false;
                onRangeExited?.Invoke();
            }

            IsActivated = true;
            onActivated?.Invoke();
            AnyActivated?.Invoke(this);
        }

        public void ResetActivation()
        {
            IsActivated = false;
            IsInRange = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = IsActivated ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, activationRadius);
        }
    }
}
