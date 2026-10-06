using UnityEngine;

namespace PussInHell.Cameras
{
    public class HorizontalCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Position")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 8f, -6f);
        [SerializeField] private float angle = 35f;

        [Header("Smoothing")]
        [SerializeField] private float followSpeed = 5f;
        [SerializeField] private float rotationSpeed = 3f;

        [Header("Look Ahead")]
        [SerializeField] private float lookAheadDistance = 2f;
        [SerializeField] private float lookAheadSpeed = 2f;

        [Header("Collision")]
        [SerializeField] private LayerMask collisionMask;
        [SerializeField] private float minDistance = 0.5f;
        [SerializeField] private float collisionRadius = 0.3f;

        private Vector3 currentLookAhead;
        private Rigidbody targetBody;

        private void Start()
        {
            if (target == null)
            {
                enabled = false;
                return;
            }
            targetBody = target.GetComponent<Rigidbody>();
        }

        private void LateUpdate()
        {
            Vector3 desired = ResolveCollision(target.position, target.position + offset);
            transform.position = Vector3.Lerp(transform.position, desired, followSpeed * Time.deltaTime);
            LookAtTarget();
        }

        private Vector3 ResolveCollision(Vector3 from, Vector3 desired)
        {
            Vector3 direction = desired - from;
            float distance = direction.magnitude;
            if (!Physics.SphereCast(from, collisionRadius, direction.normalized, out RaycastHit hit, distance, collisionMask))
                return desired;

            return from + direction.normalized * Mathf.Max(hit.distance - 0.1f, minDistance);
        }

        private void LookAtTarget()
        {
            Vector3 velocity = targetBody != null ? targetBody.linearVelocity : Vector3.zero;
            currentLookAhead = Vector3.Lerp(currentLookAhead, velocity.normalized * lookAheadDistance, lookAheadSpeed * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(target.position + currentLookAhead - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            Vector3 euler = transform.eulerAngles;
            euler.x = angle;
            transform.eulerAngles = euler;
        }

        private void OnDrawGizmosSelected()
        {
            if (target == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, target.position);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(target.position + offset, 0.5f);
        }
    }
}
