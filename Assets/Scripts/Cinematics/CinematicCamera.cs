using UnityEngine;

public class CinematicCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Camera Position")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -6f);
    [SerializeField] private float distance = 6f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 8f;
    [SerializeField] private float cameraHeight = 2f;

    [Header("Rotation Settings")]
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float rotationSmoothness = 10f;
    [SerializeField] private float minVerticalAngle = -35f;
    [SerializeField] private float maxVerticalAngle = 60f;

    [Header("Collision Settings")]
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float collisionRadius = 0.3f;
    [SerializeField] private float collisionPadding = 0.2f;
    [SerializeField] private int collisionCheckCount = 5; // Количество проверок по лучу
    [SerializeField] private float emergencyDistance = 0.5f; // Минимальное расстояние при экстренной ситуации

    [Header("Look Ahead")]
    [SerializeField] private float lookAheadDistance = 2f;
    [SerializeField] private float lookAheadSpeed = 2f;

    private float yaw;
    private float pitch;
    private Vector3 currentLookAhead;
    private Vector3 currentVelocity;
    private float currentDistance;
    private float targetDistance;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("CinematicCamera: Target не назначен!");
            enabled = false;
            return;
        }

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
        currentDistance = distance;
        targetDistance = distance;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        HandleRotationInput();
        HandleZoomInput();
        UpdateCameraPosition();
        LookAtTarget();
    }

    void HandleRotationInput()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        yaw += mouseX * mouseSensitivity;
        pitch -= mouseY * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
    }

    void HandleZoomInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            distance = Mathf.Clamp(distance - scroll * 5f, minDistance, maxDistance);
        }
    }

    void UpdateCameraPosition()
    {
        Vector3 pivotPoint = target.position + Vector3.up * cameraHeight;
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        // Желаемое направление камеры
        Vector3 desiredDirection = rotation * Vector3.back;

        // Находим безопасное расстояние с улучшенной проверкой коллизий
        float safeDistance = FindSafeDistance(pivotPoint, desiredDirection, distance);

        // Плавно обновляем целевое расстояние
        targetDistance = safeDistance;
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * rotationSmoothness);

        // Финальная позиция камеры
        Vector3 finalPosition = pivotPoint + desiredDirection * currentDistance;

        // Дополнительная проверка: если камера всё равно внутри коллайдера
        if (IsPositionInsideCollider(finalPosition))
        {
            // Экстренный режим: ставим камеру прямо над игроком
            finalPosition = target.position + Vector3.up * (cameraHeight + emergencyDistance);
            currentDistance = emergencyDistance;
        }

        transform.position = Vector3.Lerp(transform.position, finalPosition, Time.deltaTime * rotationSmoothness);
        transform.rotation = rotation;
    }

    float FindSafeDistance(Vector3 origin, Vector3 direction, float maxDist)
    {
        float safeDistance = maxDist;
        float stepSize = maxDist / collisionCheckCount;

        // Метод 1: SphereCast для основной проверки
        if (Physics.SphereCast(origin, collisionRadius, direction, out RaycastHit sphereHit, maxDist, collisionMask))
        {
            safeDistance = Mathf.Max(sphereHit.distance - collisionPadding, minDistance);
        }

        // Метод 2: Множественные raycast'ы для точности
        for (int i = 1; i <= collisionCheckCount; i++)
        {
            float checkDistance = stepSize * i;
            Vector3 checkPoint = origin + direction * checkDistance;

            if (Physics.CheckSphere(checkPoint, collisionRadius, collisionMask))
            {
                safeDistance = Mathf.Min(safeDistance, checkDistance - collisionPadding);
                break;
            }
        }

        // Метод 3: Raycast от желаемой позиции обратно к игроку
        Vector3 desiredPos = origin + direction * maxDist;
        if (Physics.Raycast(desiredPos, -direction, out RaycastHit backHit, maxDist, collisionMask))
        {
            float distanceFromOrigin = maxDist - backHit.distance;
            safeDistance = Mathf.Min(safeDistance, distanceFromOrigin - collisionPadding);
        }

        // Метод 4: Проверка от игрока к желаемой позиции обычным Raycast
        if (Physics.Raycast(origin, direction, out RaycastHit forwardHit, maxDist, collisionMask))
        {
            safeDistance = Mathf.Min(safeDistance, forwardHit.distance - collisionPadding);
        }

        return Mathf.Max(safeDistance, minDistance);
    }

    bool IsPositionInsideCollider(Vector3 position)
    {
        // Проверяем, находится ли позиция внутри коллайдера
        Collider[] colliders = Physics.OverlapSphere(position, collisionRadius * 0.5f, collisionMask);
        return colliders.Length > 0;
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

        Vector3 lookAtPoint = target.position + currentLookAhead + Vector3.up * cameraHeight * 0.5f;
        transform.LookAt(lookAtPoint);
    }

    void OnDrawGizmosSelected()
    {
        if (target == null) return;

        Vector3 pivotPoint = target.position + Vector3.up * cameraHeight;

        // Показываем pivot point
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(pivotPoint, 0.2f);

        // Показываем линию до камеры
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, pivotPoint);

        // Показываем радиус коллизии камеры
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, collisionRadius);

        // Показываем желаемую позицию
        if (Application.isPlaying)
        {
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 desiredPos = pivotPoint + rotation * Vector3.back * distance;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(desiredPos, 0.15f);
        }
    }
}
