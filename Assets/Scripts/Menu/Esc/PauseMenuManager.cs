using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace GabrielBissonnette.SAD
{
    public class PauseMenuManager : MonoBehaviour
    {
        [Header("Pause Menu Canvas")]
        [SerializeField] private GameObject pauseMenuCanvas;

        [Header("Buttons (Optional)")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Audio (Optional)")]
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;
        private AudioSource uiAudioSource;

        private bool isPaused = false;

        // Флаг для блокировки меню во время катсцены
        public static bool IsCutscenePlaying { get; set; } = false;

        // Флаг для блокировки меню на 1 кадр (для скипа катсцены)
        private static bool blockMenuThisFrame = false;

        private void Awake()
        {
            // Автоматически скрываем меню при старте
            if (pauseMenuCanvas != null)
                pauseMenuCanvas.SetActive(false);

            // Создаём AudioSource для звуков
            uiAudioSource = gameObject.AddComponent<AudioSource>();
        }

        private void Start()
        {
            // Подписываемся на кнопки
            if (resumeButton != null)
                resumeButton.onClick.AddListener(Resume);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OpenSettings);

            if (quitButton != null)
                quitButton.onClick.AddListener(QuitGame);
        }

        private void Update()
        {
            // Сбрасываем блокировку если она была установлена
            if (blockMenuThisFrame)
            {
                blockMenuThisFrame = false;
                return; // Пропускаем этот кадр
            }

            // Открытие/закрытие по ESC (только если НЕ идёт катсцена)
            if (Input.GetKeyDown(KeyCode.Escape) && !IsCutscenePlaying)
            {
                if (isPaused)
                    Resume();
                else
                    Pause();
            }
        }

        // Метод для блокировки меню на 1 кадр (вызывается из CutsceneTrigger при скипе)
        public static void BlockMenuForOneFrame()
        {
            blockMenuThisFrame = true;
        }

        public void Pause()
        {
            if (isPaused) return;

            isPaused = true;
            pauseMenuCanvas.SetActive(true);
            Time.timeScale = 0f; // Пауза игры
            PlaySound(openSound);

            // Опционально: разблокировать курсор
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Resume()
        {
            if (!isPaused) return;

            isPaused = false;
            pauseMenuCanvas.SetActive(false);
            Time.timeScale = 1f; // Возобновить игру
            PlaySound(closeSound);

            // Опционально: заблокировать курсор
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void OpenSettings()
        {
            PlaySound(clickSound);
            // Здесь можно открыть панель настроек
            // Например: settingsPanel.SetActive(true);
            Debug.Log("Settings opened (добавь свою панель)");
        }

        public void QuitGame()
        {
            PlaySound(clickSound);
            Time.timeScale = 1f; // На всякий случай
            SceneManager.LoadScene("menu"); // Или Application.Quit() в билде
            // Application.Quit(); // Для билда
        }

        private void PlaySound(AudioClip clip)
        {
            if (uiAudioSource != null && clip != null)
                uiAudioSource.PlayOneShot(clip);
        }

        // Для вызова из UI кнопок (если нужно)
        public void UIButtonClick() => PlaySound(clickSound);
    }
}
