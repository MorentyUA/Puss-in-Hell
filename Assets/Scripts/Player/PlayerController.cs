using UnityEngine;

[System.Serializable]
public class FootstepLayer
{
    [Tooltip("Слой, по которому будут определяться звуки шагов (Ground, Wood и т.д.)")]
    public LayerMask layer;

    [Tooltip("Источник звука для этого слоя")]
    public AudioSource source;

    [Tooltip("Пул звуков шагов для этого слоя")]
    public AudioClip[] clips;

    [HideInInspector] public int currentIndex = 0;
}

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;
    [SerializeField] private float runDeceleration = 8f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float airControl = 0.3f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Camera Reference")]
    [SerializeField] private bool useMainCamera = true; // НОВОЕ: использовать Main Camera автоматически
    [SerializeField] private Transform cameraTransform;

    [Header("Footstep Settings")]
    [SerializeField] private FootstepLayer[] footstepLayers;

    [Header("Fear Settings")]
    [SerializeField] private float fearThreshold = 0.1f;

    [Header("Fear Visual Effects")]
    [SerializeField] private GameObject fearEffect1;
    [SerializeField] private GameObject fearEffect2;

    [Header("AFK Settings")]
    [SerializeField] private float afkTriggerTime = 10f;
    [SerializeField] private float afkCooldown = 10f;

    private Rigidbody rb;
    private bool isGrounded;
    private Vector3 currentVelocity;
    private Vector3 targetDirection;
    private bool isWalking;
    private bool isRunning;
    private bool wasRunning;
    private float currentMaxSpeed;
    private bool isFeared = false;

    private float idleTimer = 0f;
    private float afkCooldownTimer = 0f;
    private bool isAFK = false;

    private readonly string WALK_PARAM = "walk";
    private readonly string RUN_PARAM = "run";
    private readonly string JUMP_TRIGGER = "jump";
    private readonly string FEAR_PARAM = "fear";
    private readonly string AFK_TRIGGER = "afk";
    private readonly string AFK2_TRIGGER = "afk2";

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        // ИЗМЕНЕНО: инициализация камеры
        UpdateCameraReference();

        rb.freezeRotation = true;
        rb.linearDamping = 0f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        currentMaxSpeed = walkSpeed;

        if (fearEffect1 != null) fearEffect1.SetActive(false);
        if (fearEffect2 != null) fearEffect2.SetActive(false);
    }

    void Update()
    {
        CheckGround();
        UpdateFearState();
        UpdateCameraReference(); // НОВОЕ: обновляем камеру каждый кадр

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // ИЗМЕНЕНО: проверяем камеру перед использованием
        if (cameraTransform == null)
        {
            Debug.LogWarning("PlayerController: Camera Transform отсутствует!");
            return;
        }

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = (camForward * vertical + camRight * horizontal).normalized;
        targetDirection = moveDirection;

        bool isPressingRun = Input.GetKey(KeyCode.LeftShift);
        isRunning = isPressingRun && moveDirection.magnitude > 0.1f && isGrounded;
        isWalking = moveDirection.magnitude > 0.1f && isGrounded;

        float targetSpeed = isRunning ? runSpeed : walkSpeed;
        currentMaxSpeed = Mathf.Lerp(currentMaxSpeed, targetSpeed, acceleration * Time.deltaTime);

        UpdateAnimation();
        UpdateAFKState();

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
            ResetAFK();
        }
    }

    void FixedUpdate()
    {
        MovePlayer();
        RotatePlayer();
    }

    // НОВОЕ: метод для обновления ссылки на камеру
    void UpdateCameraReference()
    {
        if (useMainCamera)
        {
            // Всегда используем Main Camera (которой управляет CinemachineBrain)
            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }
        // Если useMainCamera = false, используем заданную вручную cameraTransform
    }

    void MovePlayer()
    {
        Vector3 targetVelocity = targetDirection * currentMaxSpeed;

        float speedTransition = targetDirection.magnitude > 0.1f ? acceleration : (wasRunning ? runDeceleration : deceleration);
        if (!isGrounded) speedTransition *= airControl;

        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, speedTransition * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector3(currentVelocity.x, rb.linearVelocity.y, currentVelocity.z);
    }

    void RotatePlayer()
    {
        if (targetDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            float rotSpeed = isRunning ? rotationSpeed * 1.3f : rotationSpeed;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotSpeed * Time.fixedDeltaTime);
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        animator?.SetTrigger(JUMP_TRIGGER);
    }

    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void UpdateFearState()
    {
        if (GameRenderManager.Instance == null) return;

        float currentGlitchIntensity = 0f;
        if (GameRenderManager.Instance.hackedFeature != null &&
            GameRenderManager.Instance.hackedFeature.settings != null)
        {
            currentGlitchIntensity = GameRenderManager.Instance.hackedFeature.settings.intensity;
        }

        bool shouldBeFeared = currentGlitchIntensity >= fearThreshold;

        if (shouldBeFeared != isFeared)
        {
            isFeared = shouldBeFeared;
            animator?.SetBool(FEAR_PARAM, isFeared);

            if (fearEffect1 != null)
                fearEffect1.SetActive(isFeared);

            if (fearEffect2 != null)
                fearEffect2.SetActive(isFeared);
        }
    }

    void UpdateAFKState()
    {
        bool isPlayerActive = isWalking || isRunning || !isGrounded;

        if (isPlayerActive)
        {
            if (isAFK)
            {
                isAFK = false;
            }
            idleTimer = 0f;
        }
        else
        {
            if (afkCooldownTimer > 0f)
            {
                afkCooldownTimer -= Time.deltaTime;
            }
            else
            {
                idleTimer += Time.deltaTime;

                if (idleTimer >= afkTriggerTime)
                {
                    TriggerAFK();
                }
            }
        }
    }

    void TriggerAFK()
    {
        if (animator == null) return;

        bool useFirstAFK = Random.value > 0.5f;

        if (useFirstAFK)
        {
            animator.SetTrigger(AFK_TRIGGER);
        }
        else
        {
            animator.SetTrigger(AFK2_TRIGGER);
        }

        idleTimer = 0f;
        afkCooldownTimer = afkCooldown;
    }

    void ResetAFK()
    {
        idleTimer = 0f;
        isAFK = false;
    }

    void UpdateAnimation()
    {
        if (animator == null) return;
        animator.SetBool(WALK_PARAM, isWalking);
        animator.SetBool(RUN_PARAM, isRunning);
    }

    public void PlayFootstep()
    {
        if (!isGrounded) return;

        Ray ray = new Ray(groundCheck.position + Vector3.up * 0.1f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 1f))
        {
            int hitLayer = hit.collider.gameObject.layer;

            foreach (var layer in footstepLayers)
            {
                if ((layer.layer.value & (1 << hitLayer)) != 0)
                {
                    PlaySoundFromLayer(layer);
                    return;
                }
            }
        }
    }

    void PlaySoundFromLayer(FootstepLayer layer)
    {
        if (layer.source == null || layer.clips == null || layer.clips.Length == 0) return;

        layer.source.clip = layer.clips[layer.currentIndex];
        layer.source.Play();

        layer.currentIndex = (layer.currentIndex + 1) % layer.clips.Length;
    }

    // НОВОЕ: публичный метод для принудительной установки камеры
    public void SetCameraTransform(Transform newCamera)
    {
        cameraTransform = newCamera;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
