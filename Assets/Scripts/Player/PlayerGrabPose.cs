using UnityEngine;

namespace PussInHell.Player
{
    public class PlayerGrabPose : MonoBehaviour
    {
        [SerializeField] private PlayerGrab grab;

        [Header("Arms")]
        [Tooltip("Rigid arm meshes: pivot at the hand, mesh extends along local +Y to the shoulder")]
        [SerializeField] private Transform leftHand;
        [SerializeField] private Transform rightHand;
        [Tooltip("Local axis of the arm mesh that points from the hand to the shoulder")]
        [SerializeField] private Vector3 armAxis = Vector3.up;
        [Tooltip("Arms may rotate at most this far from their animated direction")]
        [SerializeField] private float maxAimAngle = 100f;
        [Tooltip("Arms lean together with the body before aiming")]
        [SerializeField] private bool leanArms = true;

        [Header("Lean")]
        [Tooltip("Body parts rotated around the hips to lean into the object")]
        [SerializeField] private Transform[] leanParts;
        [Tooltip("Hip pivot in local space of this object")]
        [SerializeField] private Vector3 leanPivotLocal = new Vector3(0f, -0.6f, 0f);
        [SerializeField] private float leanAngle = 18f;

        [Header("Blend")]
        [SerializeField] private float blendSpeed = 6f;

        private float weight;
        private Vector3 leftShoulderLocal;
        private Vector3 rightShoulderLocal;

        public float Weight => weight;

        private void Awake()
        {
            if (grab == null) grab = GetComponentInParent<PlayerGrab>();
            leftShoulderLocal = MeasureShoulder(leftHand);
            rightShoulderLocal = MeasureShoulder(rightHand);
        }

        private Vector3 MeasureShoulder(Transform hand)
        {
            if (hand == null) return Vector3.zero;
            var filter = hand.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null) return armAxis.normalized * 0.3f;

            Bounds bounds = filter.sharedMesh.bounds;
            Vector3 axis = armAxis.normalized;
            float extent = Mathf.Max(Vector3.Dot(bounds.max, axis), Vector3.Dot(bounds.min, axis));
            return axis * extent;
        }

        private void LateUpdate()
        {
            float target = grab != null && grab.IsGrabbing ? 1f : 0f;
            weight = Mathf.MoveTowards(weight, target, Time.deltaTime * blendSpeed);
            if (weight <= 0f) return;

            ApplyLean();
            AimArm(leftHand, leftShoulderLocal, grab.LeftHandPoint);
            AimArm(rightHand, rightShoulderLocal, grab.RightHandPoint);
        }

        private void ApplyLean()
        {
            Vector3 pivot = transform.TransformPoint(leanPivotLocal);
            Vector3 axis = transform.right;
            float angle = leanAngle * weight;

            if (leanParts != null)
                foreach (var part in leanParts)
                    if (part != null) part.RotateAround(pivot, axis, angle);

            if (!leanArms) return;
            if (leftHand != null) leftHand.RotateAround(pivot, axis, angle);
            if (rightHand != null) rightHand.RotateAround(pivot, axis, angle);
        }

        private void AimArm(Transform hand, Vector3 shoulderLocal, Vector3 point)
        {
            if (hand == null) return;

            Vector3 shoulder = hand.TransformPoint(shoulderLocal);
            Vector3 currentDirection = -hand.TransformDirection(armAxis).normalized;
            Vector3 desiredDirection = point - shoulder;
            if (desiredDirection.sqrMagnitude < 0.0001f) return;
            desiredDirection.Normalize();

            float angle = Vector3.Angle(currentDirection, desiredDirection);
            if (angle > maxAimAngle)
                desiredDirection = Vector3.Slerp(currentDirection, desiredDirection, maxAimAngle / angle);

            Quaternion aimed = Quaternion.FromToRotation(currentDirection, desiredDirection) * hand.rotation;
            hand.rotation = Quaternion.Slerp(hand.rotation, aimed, weight);
            hand.position = shoulder - hand.TransformVector(shoulderLocal);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.TransformPoint(leanPivotLocal), 0.08f);
            if (!Application.isPlaying) return;
            Gizmos.color = Color.cyan;
            if (leftHand != null) Gizmos.DrawWireSphere(leftHand.TransformPoint(leftShoulderLocal), 0.04f);
            if (rightHand != null) Gizmos.DrawWireSphere(rightHand.TransformPoint(rightShoulderLocal), 0.04f);
        }
    }
}
