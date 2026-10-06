using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using Cinemachine;

/// <summary>
/// Короткий кинематографический кадр без Timeline: резко переключает камеру на стартовую точку,
/// за duration секунд плавно ведёт её к конечной точке и возвращает управление игроку.
/// Запуск — Play() (например, из SinkBloodFill.onFillCompleted).
/// </summary>
public class CameraDollyShot : MonoBehaviour
{
    [Header("Камера")]
    [Tooltip("Виртуальная камера кадра. Без Follow/LookAt — её transform двигает этот скрипт.")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private int activePriority = 100;

    [Header("Путь")]
    [Tooltip("Откуда начинается кадр (позиция + поворот)")]
    [SerializeField] private Transform startPoint;
    [Tooltip("Куда камера приезжает к концу кадра (позиция + поворот)")]
    [SerializeField] private Transform endPoint;
    [SerializeField] private float duration = 5f;
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Переходы")]
    [Tooltip("Резкий кат на стартовую точку. Иначе — штатный бленд CinemachineBrain.")]
    [SerializeField] private bool cutOnStart = true;
    [Tooltip("Резкий кат обратно на игровую камеру. Иначе — штатный бленд CinemachineBrain.")]
    [SerializeField] private bool cutOnEnd = false;

    [Header("Перед стартом")]
    [Tooltip("Если этот Timeline ещё играет, он будет остановлен (его менеджер корректно завершит свою катсцену), и кадр возьмёт этот скрипт.")]
    [SerializeField] private PlayableDirector directorToStop;

    [Header("Управление")]
    [SerializeField] private bool disablePlayerControl = true;
    [SerializeField] private string playerTag = "Player";

    [Header("Пропуск")]
    [SerializeField] private bool allowSkip = false;
    [SerializeField] private KeyCode skipKey = KeyCode.Escape;

    [Header("События")]
    public UnityEvent onStarted;
    public UnityEvent onFinished;

    private CinemachineBrain brain;
    private CinemachineBlendDefinition originalBlend;
    private bool blendCaptured;
    private Coroutine shotCoroutine;
    private bool isPlaying;

    public bool IsPlaying => isPlaying;

    private void Start()
    {
        if (virtualCamera != null)
            virtualCamera.Priority = 0;

        CaptureBrain();
    }

    private void Update()
    {
        if (isPlaying && allowSkip && Input.GetKeyDown(skipKey))
            Skip();
    }

    // Запоминаем бленд Brain на старте сцены — до того, как катсцены начали его менять.
    private void CaptureBrain()
    {
        if (brain == null && Camera.main != null)
            brain = Camera.main.GetComponent<CinemachineBrain>();

        if (brain != null && !blendCaptured)
        {
            originalBlend = brain.m_DefaultBlend;
            blendCaptured = true;
        }
    }

    public void Play()
    {
        if (isPlaying) return;
        if (virtualCamera == null || startPoint == null || endPoint == null) return;

        if (directorToStop != null && directorToStop.state == PlayState.Playing)
            directorToStop.Stop();

        CaptureBrain();

        isPlaying = true;
        GabrielBissonnette.SAD.PauseMenuManager.IsCutscenePlaying = true;

        if (disablePlayerControl)
            SetPlayerControl(false);

        shotCoroutine = StartCoroutine(ShotRoutine());
    }

    public void Skip()
    {
        if (!isPlaying) return;

        if (shotCoroutine != null)
        {
            StopCoroutine(shotCoroutine);
            shotCoroutine = null;
        }

        GabrielBissonnette.SAD.PauseMenuManager.BlockMenuForOneFrame();
        Finish();
    }

    private IEnumerator ShotRoutine()
    {
        if (cutOnStart && brain != null)
            brain.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.Cut, 0f);

        virtualCamera.transform.SetPositionAndRotation(startPoint.position, startPoint.rotation);
        virtualCamera.Priority = activePriority;

        onStarted?.Invoke();

        // Два кадра, чтобы Brain успел переключиться катом, потом возвращаем бленд
        yield return null;
        yield return null;

        if (cutOnStart && brain != null && blendCaptured)
            brain.m_DefaultBlend = originalBlend;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = moveCurve.Evaluate(Mathf.Clamp01(elapsed / duration));
            virtualCamera.transform.SetPositionAndRotation(
                Vector3.LerpUnclamped(startPoint.position, endPoint.position, t),
                Quaternion.SlerpUnclamped(startPoint.rotation, endPoint.rotation, t));
            yield return null;
        }

        virtualCamera.transform.SetPositionAndRotation(endPoint.position, endPoint.rotation);
        shotCoroutine = null;
        Finish();
    }

    private void Finish()
    {
        if (cutOnEnd && brain != null)
        {
            brain.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.Cut, 0f);
            StartCoroutine(RestoreBlendAfterDelay());
        }

        virtualCamera.Priority = 0;

        isPlaying = false;
        GabrielBissonnette.SAD.PauseMenuManager.IsCutscenePlaying = false;

        if (disablePlayerControl)
            SetPlayerControl(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        onFinished?.Invoke();
    }

    private IEnumerator RestoreBlendAfterDelay()
    {
        yield return null;
        yield return null;

        if (brain != null && blendCaptured)
            brain.m_DefaultBlend = originalBlend;
    }

    private void SetPlayerControl(bool enabled)
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
            player = GameObject.Find("Player");
        if (player == null) return;

        var controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            if (!enabled)
            {
                var animator = controller.GetComponent<Animator>();
                if (animator == null)
                    animator = controller.GetComponentInChildren<Animator>();

                if (animator != null)
                {
                    animator.SetBool("walk", false);
                    animator.SetBool("run", false);
                }
            }

            controller.enabled = enabled;
        }

        if (!enabled)
        {
            var rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    private void OnDestroy()
    {
        if (isPlaying && brain != null && blendCaptured)
            brain.m_DefaultBlend = originalBlend;
    }

    private void OnDrawGizmos()
    {
        if (startPoint == null || endPoint == null) return;

        Gizmos.color = isPlaying ? Color.green : Color.cyan;
        Gizmos.DrawLine(startPoint.position, endPoint.position);
        Gizmos.DrawWireSphere(startPoint.position, 0.25f);
        Gizmos.DrawWireSphere(endPoint.position, 0.25f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(startPoint.position, startPoint.forward * 2f);
        Gizmos.DrawRay(endPoint.position, endPoint.forward * 2f);
    }
}
