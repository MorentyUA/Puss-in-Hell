using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PussInHell.Core;

namespace PussInHell.UI
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject pauseMenuCanvas;
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
        [SerializeField] private string menuSceneName = "menu";

        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private GameObject settingsPanel;

        [Header("Audio")]
        [SerializeField] private AudioSource uiAudioSource;
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;

        public bool IsPaused { get; private set; }

        private void Awake()
        {
            if (pauseMenuCanvas != null) pauseMenuCanvas.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (uiAudioSource == null) uiAudioSource = GetComponent<AudioSource>();
            if (uiAudioSource == null) uiAudioSource = gameObject.AddComponent<AudioSource>();
        }

        private void Start()
        {
            if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
            if (quitButton != null) quitButton.onClick.AddListener(QuitToMenu);
        }

        private void OnDestroy()
        {
            if (IsPaused) Time.timeScale = 1f;
        }

        private void Update()
        {
            if (CutsceneState.IsMenuBlocked || CutsceneState.IsPlaying) return;
            if (!Input.GetKeyDown(pauseKey)) return;

            if (IsPaused) Resume();
            else Pause();
        }

        public void Pause()
        {
            if (IsPaused) return;
            IsPaused = true;

            if (pauseMenuCanvas != null) pauseMenuCanvas.SetActive(true);
            Time.timeScale = 0f;
            Play(openSound);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Resume()
        {
            if (!IsPaused) return;
            IsPaused = false;

            if (pauseMenuCanvas != null) pauseMenuCanvas.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            Time.timeScale = 1f;
            Play(closeSound);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void OpenSettings()
        {
            Play(clickSound);
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        public void QuitToMenu()
        {
            Play(clickSound);
            Time.timeScale = 1f;
            SceneManager.LoadScene(menuSceneName);
        }

        public void QuitGame()
        {
            Play(clickSound);
            Time.timeScale = 1f;
            Application.Quit();
        }

        public void UIButtonClick() => Play(clickSound);

        private void Play(AudioClip clip)
        {
            if (uiAudioSource != null && clip != null) uiAudioSource.PlayOneShot(clip);
        }
    }
}
