using UnityEngine;
using Cinemachine;

public class CameraSwitchTrigger : MonoBehaviour
{
    [Header("Camera Settings")]
    [Tooltip("Виртуальная камера игрока (обычный режим)")]
    [SerializeField] private CinemachineVirtualCamera playerCamera;

    [Tooltip("Виртуальная камера триггерной зоны")]
    [SerializeField] private CinemachineVirtualCamera triggerCamera;

    [Header("Priority Settings")]
    [SerializeField] private int playerCameraPriority = 10;
    [SerializeField] private int triggerCameraPriority = 20;

    [Header("Trigger Settings")]
    [SerializeField] private string playerTag = "Player";

    [Header("Transition Settings")]
    [SerializeField] private bool instantSwitch = true;

    [Header("Control Settings")]
    [SerializeField] private bool disablePlayerControl = false;
    [SerializeField] private bool lockCursorOnExit = true;

    [Header("FPS Camera Settings")]
    [Tooltip("Объект с FPS-камерой (обычно ребёнок игрока)")]
    [SerializeField] private GameObject fpsCameraObject;
    [SerializeField] private bool manageFPSCamera = true;

    [Header("Culling Mask Settings")]
    [Tooltip("Убирать слой Player из Culling Mask при переключении на FPS")]
    [SerializeField] private bool managePlayerLayer = true;
    [SerializeField] private string playerLayerName = "Player";

    [Header("Debug")]
    [SerializeField] private bool showDebugMessages = false;

    private bool isPlayerInTrigger = false;
    private CinemachineBrain cinemachineBrain;
    private FirstPersonCamera fpsCameraScript;
    private Camera mainCamera;
    private int originalCullingMask;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("CameraSwitchTrigger: Main Camera не найдена!");
            enabled = false;
            return;
        }

        cinemachineBrain = mainCamera.GetComponent<CinemachineBrain>();
        if (cinemachineBrain == null)
        {
            Debug.LogError("CameraSwitchTrigger: CinemachineBrain не найден!");
            enabled = false;
            return;
        }

        // Сохраняем оригинальную маску
        originalCullingMask = mainCamera.cullingMask;

        if (playerCamera == null || triggerCamera == null)
        {
            Debug.LogError("CameraSwitchTrigger: Назначьте обе виртуальные камеры!");
            enabled = false;
            return;
        }

        playerCamera.Priority = playerCameraPriority;
        triggerCamera.Priority = 0;

        if (fpsCameraObject != null)
        {
            fpsCameraScript = fpsCameraObject.GetComponentInChildren<FirstPersonCamera>(true);
        }

        if (manageFPSCamera && fpsCameraObject != null)
        {
            fpsCameraObject.SetActive(false);
        }

        if (showDebugMessages)
            Debug.Log($"[CameraSwitchTrigger] Инициализирован на {gameObject.name}");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag) || isPlayerInTrigger) return;

        isPlayerInTrigger = true;
        SwitchToTriggerCamera();

        if (showDebugMessages)
            Debug.Log($"[Trigger Enter] → {triggerCamera.name}");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag) || !isPlayerInTrigger) return;

        isPlayerInTrigger = false;
        SwitchToPlayerCamera();

        if (showDebugMessages)
            Debug.Log($"[Trigger Exit] → {playerCamera.name}");
    }

    private void SwitchToTriggerCamera()
    {
        // 1. Включаем FPS камеру
        if (manageFPSCamera && fpsCameraObject != null)
            fpsCameraObject.SetActive(true);

        // 2. Синхронизируем её с текущим видом (чтобы не было скачка)
        SyncFPSCameraWithCurrentView();

        // 3. Передаём камеру в PlayerController
        UpdatePlayerControllerCamera(false);

        // 4. Убираем слой Player из Culling Mask
        if (managePlayerLayer && mainCamera != null)
        {
            int playerLayer = LayerMask.NameToLayer(playerLayerName);
            if (playerLayer != -1)
            {
                mainCamera.cullingMask &= ~(1 << playerLayer);

                if (showDebugMessages)
                    Debug.Log($"[Culling Mask] Убран слой '{playerLayerName}' из Main Camera");
            }
            else
            {
                Debug.LogWarning($"CameraSwitchTrigger: Слой '{playerLayerName}' не найден!");
            }
        }

        // 5. Переключаем Cinemachine
        if (instantSwitch)
        {
            triggerCamera.Priority = 100;
            playerCamera.Priority = 0;
        }
        else
        {
            triggerCamera.Priority = triggerCameraPriority;
            playerCamera.Priority = playerCameraPriority - 10;
        }

        if (disablePlayerControl)
            DisablePlayerControl();
    }

    private void SwitchToPlayerCamera()
    {
        // 1. Сначала переключаем Cinemachine (важно!)
        if (instantSwitch)
        {
            playerCamera.Priority = 100;
            triggerCamera.Priority = 0;
        }
        else
        {
            playerCamera.Priority = playerCameraPriority;
            triggerCamera.Priority = 0;
        }

        // 2. Восстанавливаем оригинальный Culling Mask
        if (managePlayerLayer && mainCamera != null)
        {
            mainCamera.cullingMask = originalCullingMask;

            if (showDebugMessages)
                Debug.Log("[Culling Mask] Восстановлена оригинальная маска Main Camera");
        }

        // 3. Возвращаем PlayerController на Main Camera
        UpdatePlayerControllerCamera(true);

        // 4. КРИТИЧЕСКИ: Синхронизируем FPS-камеру с текущим направлением ПЕРЕД выключением
        SyncFPSCameraWithCurrentView();

        // 5. Теперь можно безопасно выключить FPS объект
        if (manageFPSCamera && fpsCameraObject != null)
        {
            fpsCameraObject.SetActive(false);
        }

        if (disablePlayerControl)
            EnablePlayerControl();

        if (lockCursorOnExit)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Главная магия — синхронизация направления взгляда
    private void SyncFPSCameraWithCurrentView()
    {
        if (fpsCameraScript == null || cinemachineBrain == null) return;

        Camera activeCam = cinemachineBrain.OutputCamera;
        if (activeCam == null) activeCam = Camera.main;
        if (activeCam == null) return;

        Vector3 forward = activeCam.transform.forward;

        float yaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        float pitch = Mathf.Asin(forward.y) * -Mathf.Rad2Deg; // инвертируем, т.к. в FPS pitch вниз = положительный

        fpsCameraScript.SetRotationDirectly(pitch, yaw);

        if (showDebugMessages)
            Debug.Log($"FPS камера синхронизирована: Yaw={yaw:F1}°, Pitch={pitch:F1}°");
    }

    private void UpdatePlayerControllerCamera(bool toMainCamera)
    {
        var player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) return;

        var controller = player.GetComponent<PlayerController>();
        if (controller == null) return;

        if (toMainCamera)
        {
            controller.SetCameraTransform(Camera.main.transform);
        }
        else if (fpsCameraObject != null)
        {
            var cam = fpsCameraObject.GetComponentInChildren<Camera>(true);
            if (cam != null)
                controller.SetCameraTransform(cam.transform);
        }
    }

    private void DisablePlayerControl()
    {
        var player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) return;

        var pc = player.GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;

        var fps = player.GetComponentInChildren<FirstPersonCamera>(true);
        if (fps != null) fps.enabled = false;

        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void EnablePlayerControl()
    {
        var player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) return;

        var pc = player.GetComponent<PlayerController>();
        if (pc != null) pc.enabled = true;

        var fps = player.GetComponentInChildren<FirstPersonCamera>(true);
        if (fps != null) fps.enabled = true;
    }

    // Публичные методы
    public void ForceSwitchToTriggerCamera() { isPlayerInTrigger = true; SwitchToTriggerCamera(); }
    public void ForceSwitchToPlayerCamera() { isPlayerInTrigger = false; SwitchToPlayerCamera(); }
    public void SetInstantSwitch(bool instant) => instantSwitch = instant;
    public bool IsPlayerInTrigger() => isPlayerInTrigger;

    void OnDrawGizmos()
    {
        Gizmos.color = isPlayerInTrigger ? Color.green : Color.cyan;
        Gizmos.DrawWireCube(transform.position, transform.localScale);

        if (triggerCamera != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, triggerCamera.transform.position);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (triggerCamera != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(triggerCamera.transform.position, triggerCamera.transform.forward * 3f);
        }
    }
}
