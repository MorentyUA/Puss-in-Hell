using UnityEngine;

namespace PussInHell.Cameras
{
    public class CinematicCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Distance")]
        [SerializeField] private float distance = 6f;
        [SerializeField] private float minDistance = 1f;
        [SerializeField] private float maxDistance = 8f;
        [SerializeField] private float cameraHeight = 2f;
        [SerializeField] private float zoomSpeed = 5f;

        [Header("Rotation")]
        [SerializeField] private float mouseSensitivity = 3f;
        [SerializeField] private float rotationSmoothness = 10f;
        [SerializeField] private float minVerticalAngle = -35f;
        [SerializeField] private float maxVerticalAngle = 60f;

        [Header("Collision")]
        [SerializeField] private LayerMask collisionMask;
        [SerializeField] private float collisionRadius = 0.3f;
        [SerializeField] private float collisionPadding = 0.2f;
        [SerializeField] private int collisionCheckCount = 5;
        [SerializeField] private float emergencyDistance = 0.5f;

        [Header("Look Ahead")]
        [SerializeField] private float lookAheadDistance = 2f;
        [SerializeField] private float lookAheadSpeed = 2f;

        [Header("Cursor")]
        [SerializeField] private bool lockCursorOnStart = true;

        private float yaw;
        private float pitch;
        private float currentDistance;
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
            Vector3 angles = transform.eulerAngles;
            yaw = angles.y;
            pitch = angles.x;
            currentDistance = distance;

            if (lockCursorOnStart)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void LateUpdate()
        {
            ReadInput();
            UpdatePosition();
            LookAtTarget();
        }

        private void ReadInput()
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * mouseSensitivity, minVerticalAngle, maxVerticalAngle);

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f) distance = Mathf.Clamp(distance - scroll * zoomSpeed, minDistance, maxDistance);
        }

        private void UpdatePosition()
        {
            Vector3 pivot = target.position + Vector3.up * cameraHeight;
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 direction = rotation * Vector3.back;

            float safeDistance = FindSafeDistance(pivot, direction, distance);
            currentDistance = Mathf.Lerp(currentDistance, safeDistance, Time.deltaTime * rotationSmoothness);

            Vector3 finalPosition = pivot + direction * currentDistance;
            if (Physics.CheckSphere(finalPosition, collisionRadius * 0.5f, collisionMask))
            {
                finalPosition = target.position + Vector3.up * (cameraHeight + emergencyDistance);
                currentDistance = emergencyDistance;
            }

            transform.position = Vector3.Lerp(transform.position, finalPosition, Time.deltaTime * rotationSmoothness);
            transform.rotation = rotation;
        }

        private float FindSafeDistance(Vector3 origin, Vector3 direction, float maxDist)
        {
            float safe = maxDist;

            if (Physics.SphereCast(origin, collisionRadius, direction, out RaycastHit sphereHit, maxDist, collisionMask))
                safe = Mathf.Max(sphereHit.distance - collisionPadding, minDistance);

            float step = maxDist / Mathf.Max(1, collisionCheckCount);
            for (int i = 1; i <= collisionCheckCount; i++)
            {
                float checkDistance = step * i;
                if (!Physics.CheckSphere(origin + direction * checkDistance, collisionRadius, collisionMask)) continue;
                safe = Mathf.Min(safe, checkDistance - collisionPadding);
                break;
            }

            Vector3 desired = origin + direction * maxDist;
            if (Physics.Raycast(desired, -direction, out RaycastHit backHit, maxDist, collisionMask))
                safe = Mathf.Min(safe, maxDist - backHit.distance - collisionPadding);

            if (Physics.Raycast(origin, direction, out RaycastHit forwardHit, maxDist, collisionMask))
                safe = Mathf.Min(safe, forwardHit.distance - collisionPadding);

            return Mathf.Max(safe, minDistance);
        }

        private void LookAtTarget()
        {
            Vector3 velocity = targetBody != null ? targetBody.linearVelocity : Vector3.zero;
            currentLookAhead = Vector3.Lerp(currentLookAhead, velocity.normalized * lookAheadDistance, lookAheadSpeed * Time.deltaTime);
            transform.LookAt(target.position + currentLookAhead + Vector3.up * cameraHeight * 0.5f);
        }

        private void OnDrawGizmosSelected()
        {
            if (target == null) return;
            Vector3 pivot = target.position + Vector3.up * cameraHeight;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(pivot, 0.2f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, pivot);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, collisionRadius);
        }
    }
}
