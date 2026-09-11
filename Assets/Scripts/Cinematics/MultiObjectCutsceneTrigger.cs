using UnityEngine;
using UnityEngine.Playables;
using Cinemachine;
using System.Collections.Generic;

/// <summary>
/// Запускает катсцену когда все указанные HaloHighlighter объекты активированы
/// </summary>
public class HaloHighlighterCutsceneManager : MonoBehaviour
{
    [Header("Объекты для отслеживания")]
    [Tooltip("Список HaloHighlighter объектов. Когда ВСЕ активированы — запускается катсцена")]
    [SerializeField] private List<HaloHighlighter> requiredHighlighters = new List<HaloHighlighter>();

    [Header("Cutscene Settings")]
    [SerializeField] private PlayableDirector timeline;
    [SerializeField] private CinemachineVirtualCamera cutsceneCamera;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource cutsceneAudioSource;
    [SerializeField] private bool stopAudioOnEnd = true;

    [Header("Skip Settings")]
    [SerializeField] private bool allowSkip = true;
    [SerializeField] private KeyCode skipKey = KeyCode.Escape;

    [Header("Trigger Settings")]
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private string playerTag = "Player";

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    private HashSet<HaloHighlighter> activatedHighlighters = new HashSet<HaloHighlighter>();
    private bool hasTriggered = false;
    private bool cutscenePlaying = false;

    // Для сохранения предыдущей камеры
    private CinemachineVirtualCameraBase previousCamera;
    private CinemachineBrain cinemachineBrain;
    private CinemachineBlendDefinition originalBlend;

    private void OnEnable()
    {
        HaloHighlighter.OnAnyActivated += OnHighlighterActivated;
    }

    private void OnDisable()
    {
        HaloHighlighter.OnAnyActivated -= OnHighlighterActivated;
    }

    private void Start()
    {
        if (timeline != null)
        {
            timeline.stopped += OnTimelineStopped;
        }

        if (cutsceneCamera != null)
        {
            cutsceneCamera.Priority = 0;
        }

        // Находим CinemachineBrain для управления блендингом
        cinemachineBrain = FindObjectOfType<CinemachineBrain>();
        if (cinemachineBrain != null)
        {
            // Сохраняем оригинальный блендинг
            originalBlend = cinemachineBrain.m_DefaultBlend;
        }

        // Проверяем уже активированные объекты
        foreach (var highlighter in requiredHighlighters)
        {
            if (highlighter != null && highlighter.IsActivated)
            {
                activatedHighlighters.Add(highlighter);
            }
        }

        if (showDebugLogs)
        {
            Debug.Log($"[HaloHighlighterCutsceneManager] Требуется активировать {requiredHighlighters.Count} объектов");
        }
    }

    private void Update()
    {
        if (cutscenePlaying && allowSkip && Input.GetKeyDown(skipKey))
        {
            SkipCutscene();
        }
    }

    private void OnHighlighterActivated(HaloHighlighter highlighter)
    {
        if (!requiredHighlighters.Contains(highlighter))
            return;

        activatedHighlighters.Add(highlighter);

        if (showDebugLogs)
        {
            Debug.Log($"[HaloHighlighterCutsceneManager] Активирован: {highlighter.name} ({activatedHighlighters.Count}/{requiredHighlighters.Count})");
        }

        CheckAllActivated();
    }

    private void CheckAllActivated()
    {
        if (triggerOnce && hasTriggered)
            return;

        if (cutscenePlaying)
            return;

        foreach (var highlighter in requiredHighlighters)
        {
            if (highlighter == null) continue;

            if (!activatedHighlighters.Contains(highlighter))
                return;
        }

        if (showDebugLogs)
        {
            Debug.Log("[HaloHighlighterCutsceneManager] ВСЕ объекты активированы! Запуск катсцены...");
        }

        StartCutscene();
    }

    private void StartCutscene()
    {
        hasTriggered = true;
        cutscenePlaying = true;

        GabrielBissonnette.SAD.PauseMenuManager.IsCutscenePlaying = true;

        DisablePlayerControl();

        // === МГНОВЕННОЕ ПЕРЕКЛЮЧЕНИЕ КАМЕРЫ ===
        if (cutsceneCamera != null)
        {
            // Сохраняем текущую активную камеру
            if (cinemachineBrain != null)
            {
                previousCamera = cinemachineBrain.ActiveVirtualCamera as CinemachineVirtualCameraBase;

                // Устанавливаем мгновенный переход (Cut)
                cinemachineBrain.m_DefaultBlend = new CinemachineBlendDefinition(
                    CinemachineBlendDefinition.Style.Cut, 0f);
            }

            // Понижаем приоритет предыдущей камеры
            if (previousCamera != null)
            {
                previousCamera.Priority = 0;
            }

            // Включаем камеру катсцены с высоким приоритетом
            cutsceneCamera.Priority = 100;

            if (showDebugLogs)
            {
                Debug.Log($"[HaloHighlighterCutsceneManager] Переключение на камеру катсцены. Предыдущая: {(previousCamera != null ? previousCamera.name : "null")}");
            }
        }

        if (cutsceneAudioSource != null)
        {
            cutsceneAudioSource.Play();
        }

        if (timeline != null)
        {
            timeline.Play();
        }
    }

    private void SkipCutscene()
    {
        GabrielBissonnette.SAD.PauseMenuManager.BlockMenuForOneFrame();

        if (timeline != null)
        {
            timeline.Stop();
        }
        else
        {
            EndCutscene();
        }
    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        if (director == timeline)
        {
            EndCutscene();
        }
    }

    private void EndCutscene()
    {
        cutscenePlaying = false;

        GabrielBissonnette.SAD.PauseMenuManager.IsCutscenePlaying = false;

        // === МГНОВЕННОЕ ВОЗВРАЩЕНИЕ НА ПРЕДЫДУЩУЮ КАМЕРУ ===
        if (cutsceneCamera != null)
        {
            // Отключаем камеру катсцены
            cutsceneCamera.Priority = 0;

            // Возвращаем предыдущую камеру
            if (previousCamera != null)
            {
                previousCamera.Priority = 10;

                if (showDebugLogs)
                {
                    Debug.Log($"[HaloHighlighterCutsceneManager] Возврат на камеру: {previousCamera.name}");
                }
            }

            // Восстанавливаем оригинальный блендинг после переключения
            if (cinemachineBrain != null)
            {
                StartCoroutine(RestoreBlendAfterDelay());
            }
        }

        if (stopAudioOnEnd && cutsceneAudioSource != null)
        {
            cutsceneAudioSource.Stop();
        }

        EnablePlayerControl();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private System.Collections.IEnumerator RestoreBlendAfterDelay()
    {
        // Ждём 2 кадра чтобы мгновенный переход успел произойти
        yield return null;
        yield return null;

        if (cinemachineBrain != null)
        {
            cinemachineBrain.m_DefaultBlend = originalBlend;

            if (showDebugLogs)
            {
                Debug.Log("[HaloHighlighterCutsceneManager] Блендинг камер восстановлен");
            }
        }
    }

    private void DisablePlayerControl()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (player == null)
        {
            player = GameObject.Find("Player");
        }

        if (player == null)
        {
            return;
        }

        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            var animator = playerController.GetComponent<Animator>();
            if (animator == null)
                animator = playerController.GetComponentInChildren<Animator>();

            if (animator != null)
            {
                animator.SetBool("walk", false);
                animator.SetBool("run", false);
            }

            playerController.enabled = false;
        }

        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void EnablePlayerControl()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (player == null)
        {
            player = GameObject.Find("Player");
        }

        if (player == null)
        {
            return;
        }

        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = true;
        }
    }

    private void OnDestroy()
    {
        if (timeline != null)
        {
            timeline.stopped -= OnTimelineStopped;
        }

        // Восстанавливаем блендинг на случай если объект уничтожен во время катсцены
        if (cinemachineBrain != null && cutscenePlaying)
        {
            cinemachineBrain.m_DefaultBlend = originalBlend;
        }
    }

    public void ResetProgress()
    {
        activatedHighlighters.Clear();
        hasTriggered = false;

        foreach (var highlighter in requiredHighlighters)
        {
            if (highlighter != null)
            {
                highlighter.ResetActivation();
            }
        }
    }

    public (int current, int total) GetProgress()
    {
        return (activatedHighlighters.Count, requiredHighlighters.Count);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = hasTriggered ? Color.green : Color.cyan;

        foreach (var highlighter in requiredHighlighters)
        {
            if (highlighter != null)
            {
                bool isActive = highlighter.IsActivated;
                Gizmos.color = isActive ? Color.green : Color.yellow;
                Gizmos.DrawLine(transform.position, highlighter.transform.position);
                Gizmos.DrawWireSphere(highlighter.transform.position, 0.3f);
            }
        }
    }
#endif
}
