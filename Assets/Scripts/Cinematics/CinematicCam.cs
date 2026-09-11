using UnityEngine;
using UnityEngine.Playables;
using Cinemachine;

public class CutsceneTrigger : MonoBehaviour
{
    [Header("Cutscene Settings")]
    [SerializeField] private PlayableDirector timeline;
    [SerializeField] private CinemachineVirtualCamera cutsceneCamera;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource cutsceneAudioSource;
    [SerializeField] private bool stopAudioOnEnd = true;

    [Header("Trigger Settings")]
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private string playerTag = "Player";

    [Header("Skip Settings")]
    [SerializeField] private bool allowSkip = true;
    [SerializeField] private KeyCode skipKey = KeyCode.Escape;

    private bool hasTriggered = false;
    private bool cutscenePlaying = false;

    private void Start()
    {
        if (timeline == null)
        {
        }

        if (cutsceneCamera == null)
        {
        }

        // Подписываемся на событие завершения Timeline
        if (timeline != null)
        {
            timeline.stopped += OnTimelineStopped;
        }

        // Убеждаемся, что катсцена камера изначально неактивна
        if (cutsceneCamera != null)
        {
            cutsceneCamera.Priority = 0;
        }
    }

    private void Update()
    {
        // Скип катсцены на ESC
        if (cutscenePlaying && allowSkip && Input.GetKeyDown(skipKey))
        {
            SkipCutscene();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (triggerOnce && hasTriggered)
                return;

            if (cutscenePlaying)
                return;

            StartCutscene();
        }
    }

    private void StartCutscene()
    {
        hasTriggered = true;
        cutscenePlaying = true;

        // БЛОКИРУЕМ МЕНЮ через статический флаг
        GabrielBissonnette.SAD.PauseMenuManager.IsCutscenePlaying = true;

        // Отключаем управление игрока СРАЗУ
        DisablePlayerControl();

        // Включаем камеру катсцены
        if (cutsceneCamera != null)
        {
            cutsceneCamera.Priority = 10;
        }
        else
        {
        }

        // Воспроизводим звук катсцены
        if (cutsceneAudioSource != null)
        {
            cutsceneAudioSource.Play();
        }

        // Запускаем Timeline
        if (timeline != null)
        {
            timeline.Play();
        }
        else
        {
        }
    }

    private void SkipCutscene()
    {
        // Временно блокируем меню на 1 кадр после скипа
        GabrielBissonnette.SAD.PauseMenuManager.BlockMenuForOneFrame();

        if (timeline != null)
        {
            // Останавливаем Timeline (это вызовет OnTimelineStopped)
            timeline.Stop();
        }
        else
        {
            // Если Timeline нет, сразу заканчиваем
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

        // РАЗБЛОКИРУЕМ МЕНЮ
        GabrielBissonnette.SAD.PauseMenuManager.IsCutscenePlaying = false;

        // Выключаем камеру катсцены
        if (cutsceneCamera != null)
        {
            cutsceneCamera.Priority = 0;
        }

        // Останавливаем звук если нужно
        if (stopAudioOnEnd && cutsceneAudioSource != null)
        {
            cutsceneAudioSource.Stop();
        }

        // Включаем управление игрока
        EnablePlayerControl();

        // ВАЖНО: Блокируем курсор обратно после катсцены
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void DisablePlayerControl()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (player == null)
        {
            // Попробуем найти по имени
            player = GameObject.Find("Player");
            if (player != null)
            {
            }
            else
            {
                return;
            }
        }

        // Отключаем PlayerController
        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // СБРАСЫВАЕМ АНИМАЦИЮ перед отключением
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
        else
        {
        }

        // Останавливаем Rigidbody
        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        else
        {
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

        // Включаем PlayerController
        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        else
        {
        }
    }

    private void OnDestroy()
    {
        if (timeline != null)
        {
            timeline.stopped -= OnTimelineStopped;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = hasTriggered ? Color.green : Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
