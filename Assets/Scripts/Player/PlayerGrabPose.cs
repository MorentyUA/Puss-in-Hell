using UnityEngine;

namespace PussInHell.Player
{
    public class PlayerGrabPose : MonoBehaviour
    {
        [SerializeField] private PlayerGrab grab;

        [Header("Hands")]
        [SerializeField] private Transform leftHand;
        [SerializeField] private Transform rightHand;
        [Tooltip("Rotate the hands to face the grabbed surface")]
        [SerializeField] private bool orientHands = true;
        [Tooltip("Extra rotation applied to the hands in the grab pose")]
        [SerializeField] private Vector3 handRotationOffset = Vector3.zero;

        [Header("Lean")]
        [Tooltip("Body parts rotated around the hips to lean into the object")]
        [SerializeField] private Transform[] leanParts;
        [Tooltip("Hip pivot in local space of this object")]
        [SerializeField] private Vector3 leanPivotLocal = new Vector3(0f, -0.7f, 0f);
        [SerializeField] private float leanAngle = 18f;

        [Header("Blend")]
        [SerializeField] private float blendSpeed = 6f;

        private float weight;

        public float Weight => weight;

        private void Awake()
        {
            if (grab == null) grab = GetComponentInParent<PlayerGrab>();
        }

        private void LateUpdate()
        {
            float target = grab != null && grab.IsGrabbing ? 1f : 0f;
            weight = Mathf.MoveTowards(weight, target, Time.deltaTime * blendSpeed);
            if (weight <= 0f) return;

            ApplyLean();
            ApplyHand(leftHand, grab.LeftHandPoint);
            ApplyHand(rightHand, grab.RightHandPoint);
        }

        private void ApplyLean()
        {
            if (leanParts == null) return;

            Vector3 pivot = transform.TransformPoint(leanPivotLocal);
            Vector3 axis = transform.right;
            float angle = leanAngle * weight;
            foreach (var part in leanParts)
                if (part != null) part.RotateAround(pivot, axis, angle);
        }

        private void ApplyHand(Transform hand, Vector3 point)
        {
            if (hand == null) return;

            hand.position = Vector3.Lerp(hand.position, point, weight);
            if (!orientHands) return;

            Vector3 toSurface = -grab.FaceNormal;
            if (toSurface.sqrMagnitude < 0.0001f) return;

            Quaternion target = Quaternion.LookRotation(toSurface, Vector3.up) * Quaternion.Euler(handRotationOffset);
            hand.rotation = Quaternion.Slerp(hand.rotation, target, weight);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.TransformPoint(leanPivotLocal), 0.08f);
        }
    }
}
