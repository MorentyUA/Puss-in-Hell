using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float smoothTime = 0.05f;

    [Header("Camera Limits")]
    [SerializeField] private float minVerticalAngle = -80f;
    [SerializeField] private float maxVerticalAngle = 80f;

    [Header("References")]
    [SerializeField] private Transform playerBody;

    [Header("Head Bob Settings (Optional)")]
    [SerializeField] private bool enableHeadBob = false;
    [SerializeField] private float bobFrequency = 2f;
    [SerializeField] private float bobHorizontalAmplitude = 0.05f;
    [SerializeField] private float bobVerticalAmplitude = 0.1f;

    // Внутренние переменные
    private float rotationX = 0f;
    private float rotationY = 0f;
    private float currentRotationX = 0f;
    private float currentRotationY = 0f;
    private float rotationXVelocity;
    private float rotationYVelocity;

    // Head bob
    private float bobTimer = 0f;
    private Vector3 originalLocalPosition;

    void Start()
    {
        if (playerBody == null && transform.parent != null)
        {
            playerBody = transform.parent;
        }

        if (playerBody == null)
        {
            Debug.LogError("FirstPersonCamera: Player Body не назначен и нет родительского объекта!");
            enabled = false;
            return;
        }

        originalLocalPosition = transform.localPosition;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseInput();
        HandleCursorToggle();
    }

    void LateUpdate()
    {
        ApplyCameraRotation();
        ApplyHeadBob();
    }

    void HandleMouseInput()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        rotationY += mouseX;
        rotationX -= mouseY;

        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);
    }

    void ApplyCameraRotation()
    {
        currentRotationX = Mathf.SmoothDamp(currentRotationX, rotationX, ref rotationXVelocity, smoothTime);
        currentRotationY = Mathf.SmoothDamp(currentRotationY, rotationY, ref rotationYVelocity, smoothTime);

        transform.localRotation = Quaternion.Euler(currentRotationX, 0f, 0f);

        if (playerBody != null)
        {
            playerBody.rotation = Quaternion.Euler(0f, currentRotationY, 0f);
        }
    }

    void ApplyHeadBob()
    {
        if (!enableHeadBob) return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        bool isMoving = (horizontal != 0 || vertical != 0);

        if (isMoving)
        {
            bobTimer += Time.deltaTime * bobFrequency;

            float horizontalOffset = Mathf.Sin(bobTimer) * bobHorizontalAmplitude;
            float verticalOffset = Mathf.Cos(bobTimer * 2) * bobVerticalAmplitude;

            transform.localPosition = originalLocalPosition + new Vector3(horizontalOffset, verticalOffset, 0f);
        }
        else
        {
            bobTimer = 0f;
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalLocalPosition, Time.deltaTime * 5f);
        }
    }

    void HandleCursorToggle()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // === НОВЫЙ МЕТОД — КРИТИЧЕСКИ ВАЖЕН ДЛЯ БЕСШОВНОГО ПЕРЕКЛЮЧЕНИЯ ===
    /// <summary>
    /// Мгновенно установить поворот камеры без плавности и накопления
    /// </summary>
    public void SetRotationDirectly(float pitch, float yaw)
    {
        rotationX = pitch;
        rotationY = yaw;

        currentRotationX = pitch;
        currentRotationY = yaw;

        rotationXVelocity = 0f;
        rotationYVelocity = 0f;

        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        if (playerBody != null)
        {
            playerBody.rotation = Quaternion.Euler(0f, yaw, 0f);
        }
    }

    // Публичные методы
    public void SetSensitivity(float sensitivity) => mouseSensitivity = sensitivity;

    public void AddRecoil(float recoilX, float recoilY)
    {
        rotationX += recoilX;
        rotationY += recoilY;
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);
    }

    public void SetHeadBob(bool enabled)
    {
        enableHeadBob = enabled;
        if (!enabled)
            transform.localPosition = originalLocalPosition;
    }

    public float GetVerticalAngle() => currentRotationX;
    public float GetHorizontalAngle() => currentRotationY;
}
