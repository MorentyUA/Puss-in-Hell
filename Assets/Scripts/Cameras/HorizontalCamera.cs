using UnityEngine;

public class HorizontalCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Camera Position")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 8f, -6f);
    [SerializeField] private float angle = 35f;

    [Header("Smooth Settings")]
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float rotationSpeed = 3f;

    [Header("Look Ahead")]
    [SerializeField] private float lookAheadDistance = 2f;
    [SerializeField] private float lookAheadSpeed = 2f;

    [Header("Collision Settings")]
    [Tooltip("Слои, с которыми камера должна сталкиваться")]
    [SerializeField] private LayerMask collisionMask;
    [Tooltip("Минимальная дистанция от игрока, чтобы камера не залезала внутрь")]
    [SerializeField] private float minDistance = 0.5f;
    [Tooltip("Радиус проверки столкновений камеры")]
    [SerializeField] private float collisionRadius = 0.3f;

    private Vector3 currentLookAhead;

    void Start()
    {
        if (target == null)
            Debug.LogError("CinematicCamera: Target не назначен! Назначь игрока в инспекторе.");
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = CalculateTargetPosition();
        Vector3 adjustedPosition = HandleCameraCollision(target.position, desiredPosition);

        transform.position = Vector3.Lerp(transform.position, adjustedPosition, followSpeed * Time.deltaTime);
        LookAtTarget();
    }

    Vector3 CalculateTargetPosition()
    {
        return target.position + offset;
    }

    Vector3 HandleCameraCollision(Vector3 targetPosition, Vector3 desiredPosition)
    {
        Vector3 direction = desiredPosition - targetPosition;
        float distance = direction.magnitude;

        // Проверяем, есть ли что-то между игроком и камерой
        if (Physics.SphereCast(targetPosition, collisionRadius, direction.normalized, out RaycastHit hit, distance, collisionMask))
        {
            // Смещаем камеру к точке столкновения с отступом
            float adjustedDistance = Mathf.Max(hit.distance - 0.1f, minDistance);
            return targetPosition + direction.normalized * adjustedDistance;
        }

        return desiredPosition;
    }

    void LookAtTarget()
    {
        Vector3 targetVelocity = Vector3.zero;

        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            targetVelocity = targetRb.linearVelocity;
        }

        Vector3 lookAheadTarget = targetVelocity.normalized * lookAheadDistance;
        currentLookAhead = Vector3.Lerp(currentLookAhead, lookAheadTarget, lookAheadSpeed * Time.deltaTime);

        Vector3 lookAtPoint = target.position + currentLookAhead;

        Quaternion targetRotation = Quaternion.LookRotation(lookAtPoint - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        Vector3 eulerAngles = transform.eulerAngles;
        eulerAngles.x = angle;
        transform.eulerAngles = eulerAngles;
    }

    void OnDrawGizmosSelected()
    {
        if (target == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, target.position);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(target.position + offset, 0.5f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(target.position + currentLookAhead, 0.3f);
    }
}
