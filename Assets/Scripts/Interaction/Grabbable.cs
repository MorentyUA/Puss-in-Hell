using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using PussInHell.Core;

namespace PussInHell.Interaction
{
    [RequireComponent(typeof(Rigidbody))]
    public class Grabbable : MonoBehaviour
    {
        public static readonly List<Grabbable> All = new List<Grabbable>();

        [Header("Physics")]
        [SerializeField] private Rigidbody body;
        [Tooltip("Collider whose faces the hands attach to. Empty: first collider on this object.")]
        [SerializeField] private Collider grabCollider;

        [Header("Hands")]
        [Tooltip("Distance between the two hands on the face")]
        [SerializeField] private float handSpacing = 0.45f;
        [Tooltip("Hand height as a fraction of the object height")]
        [Range(0f, 1f)]
        [SerializeField] private float handHeightFraction = 0.6f;
        [Tooltip("How far the hands stay off the surface")]
        [SerializeField] private float surfaceOffset = 0.1f;

        [Header("Proximity")]
        [Tooltip("Player distance that counts as near (for highlights)")]
        [SerializeField] private float nearRadius = 3f;

        [Header("Events")]
        public UnityEvent onPlayerNear;
        public UnityEvent onPlayerFar;
        public UnityEvent onGrabbed;
        public UnityEvent onReleased;

        public Rigidbody Body => body;
        public bool IsGrabbed { get; private set; }
        public bool IsPlayerNear { get; private set; }

        private void Awake()
        {
            if (body == null) body = GetComponent<Rigidbody>();
            if (grabCollider == null) grabCollider = GetComponent<Collider>();
        }

        private void OnEnable()
        {
            if (!All.Contains(this)) All.Add(this);
        }

        private void OnDisable()
        {
            All.Remove(this);
            if (IsPlayerNear)
            {
                IsPlayerNear = false;
                onPlayerFar?.Invoke();
            }
        }

        private void Update()
        {
            if (!PlayerLocator.TryGetDistance(transform.position, out float distance)) return;

            bool near = distance <= nearRadius && !IsGrabbed;
            if (near == IsPlayerNear) return;

            IsPlayerNear = near;
            if (near) onPlayerNear?.Invoke();
            else onPlayerFar?.Invoke();
        }

        public void SetGrabbed(bool grabbed)
        {
            if (IsGrabbed == grabbed) return;
            IsGrabbed = grabbed;
            if (grabbed) onGrabbed?.Invoke();
            else onReleased?.Invoke();
        }

        public bool GetGrabPoints(Vector3 playerPosition, out Vector3 leftHand, out Vector3 rightHand, out Vector3 faceNormal)
        {
            leftHand = rightHand = transform.position;
            faceNormal = Vector3.forward;
            if (grabCollider == null) return false;

            Bounds bounds = grabCollider.bounds;
            Vector3 toPlayer = playerPosition - bounds.center;
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude < 0.0001f) return false;

            Vector3 bestAxis = Vector3.forward;
            float bestDot = float.NegativeInfinity;
            foreach (var axis in new[] { transform.forward, -transform.forward, transform.right, -transform.right })
            {
                Vector3 flat = axis;
                flat.y = 0f;
                if (flat.sqrMagnitude < 0.0001f) continue;
                float dot = Vector3.Dot(flat.normalized, toPlayer.normalized);
                if (dot <= bestDot) continue;
                bestDot = dot;
                bestAxis = flat.normalized;
            }

            faceNormal = bestAxis;
            float halfExtent = Mathf.Abs(Vector3.Dot(bounds.extents, new Vector3(Mathf.Abs(bestAxis.x), 0f, Mathf.Abs(bestAxis.z))));
            Vector3 faceCenter = bounds.center + bestAxis * (halfExtent + surfaceOffset);
            faceCenter.y = bounds.min.y + bounds.size.y * handHeightFraction;

            Vector3 playerRight = Vector3.Cross(Vector3.up, -bestAxis).normalized;
            rightHand = faceCenter + playerRight * handSpacing * 0.5f;
            leftHand = faceCenter - playerRight * handSpacing * 0.5f;
            return true;
        }

        public static Grabbable FindNearest(Vector3 position, Vector3 forward, float radius, float maxAngle)
        {
            Grabbable best = null;
            float bestDistance = radius;
            foreach (var grabbable in All)
            {
                if (grabbable == null || grabbable.IsGrabbed) continue;
                float distance = Vector3.Distance(position, grabbable.transform.position);
                if (distance > bestDistance) continue;

                Vector3 to = grabbable.transform.position - position;
                to.y = 0f;
                if (Vector3.Angle(forward, to) > maxAngle) continue;

                best = grabbable;
                bestDistance = distance;
            }
            return best;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = IsGrabbed ? Color.green : Color.cyan;
            Gizmos.DrawWireSphere(transform.position, nearRadius);
        }
    }
}
