using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GabrielBissonnette.SAD
{
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Sequence Manager")]
        [Tooltip("Choose a type of intro.")] public Intro introState;
        public enum Intro { OneLiner_FadingMenu, FadingMenu, MenuOnly };

        [Header("Buttons")]
        [Space(10)] [Tooltip("Enable to change the texts manually.")] public bool manualModeButtons;
        [Tooltip("Choose a type of intro.")] public Buttons buttonsAppearance;
        public enum Buttons { Rounded, Rounded_Outlined, Rounded_AlwaysFilled, Squared, Squared_Outlined, Squared_AlwaysFilled };

        [Header("Colors")]
        [Space(10)] [Tooltip("Enable to change the colors manually.")] public bool manualModeColor;
        public Color32 mainColor;
        public float alpha_godrays = 0.13f;
        public float alpha_particleSlowNormal = 0.5f;
        public float alpha_particleHuge = 0.05f;

        [Header("Intro Sequence")]
        [Space(10)] [Tooltip("Enable to change the text manually.")] public bool manualModeIntroText;
        [SerializeField] string introTextContent = "It is never too late to be who you might have been.";

        [Header("Scene")]
        [Space(10)] [SerializeField] string sceneToLoad;
        [SerializeField] float delayBeforeLoading = 3f;

        [Header("Home Panel")]
        [Space(10)] [Tooltip("Enable to change the texts manually.")] public bool manualModeTexts;
        [SerializeField] string play = "Play";
        [SerializeField] string settings = "Options";
        [SerializeField] string quit = "Quit";

        [Header("Fade Image on Play")]
        [Space(10)] [Tooltip("Image to fade in when Play is clicked")]
        [SerializeField] Image fadeImage;
        [SerializeField] float fadeDuration = 3f;

        [Header("Audio Mixer")]
        [Space(10)]
        [SerializeField] AudioMixer audioMixer;
        [SerializeField] string masterParameter = "MasterVolume";
        [SerializeField] string fxParameter = "FXVolume";
        [SerializeField] string musicParameter = "MusicVolume";
        [SerializeField] float defaultMasterVolume = 0.7f;
        [SerializeField] float defaultFXVolume = 0.7f;
        [SerializeField] float defaultMusicVolume = 0.7f;

        [Header("Audio")]
        [SerializeField] bool customSoundtrack;
        [SerializeField] AudioClip customSoundtrackAudio;
        [SerializeField] AudioClip sound_click;
        [SerializeField] AudioClip sound_hover;
        [SerializeField] AudioClip sound_loadScene;

        // Refs
        [Header("---- References")]
        [Space(50)] public Animator main_animator;
        [SerializeField] Image background_sprite;
        [Space(10)] [SerializeField] TextMeshProUGUI introText;
        public CanvasGroup homePanel;
        [SerializeField] TextMeshProUGUI homePanel_text_play;
        [SerializeField] TextMeshProUGUI homePanel_text_options;
        [SerializeField] TextMeshProUGUI homePanel_text_quit;

        [Space(10)] [SerializeField] Sprite buttonRounded;
        [SerializeField] Sprite buttonRoundedOutlined;
        [SerializeField] Sprite buttonSquared;
        [SerializeField] Sprite buttonSquaredOutlined;

        [Space(10)] [SerializeField] ParticleSystem[] particles;
        [SerializeField] Image[] godrays_sprite;
        [SerializeField] Image[] buttons;
        [SerializeField] Animator[] buttonsAnimators;
        [SerializeField] RuntimeAnimatorController buttonsAnimator_darkText;
        [SerializeField] RuntimeAnimatorController buttonsAnimator_lightText;
        [SerializeField] RuntimeAnimatorController buttonsAnimator_alwaysFilled;

        [Space(10)] [SerializeField] AudioSource audioSource;
        [SerializeField] AudioSource audioSourceSountrack;
        [SerializeField] AudioClip demo_soundtrack;
        [SerializeField] AudioClip demo_soundtrack_shorter;
        [SerializeField] Slider volumeSlider;
        [SerializeField] Slider fxSlider;
        [SerializeField] Slider musicSlider;

        private void Awake()
        {
            IntroSequence();

            // Устанавливаем начальную прозрачность изображения
            if (fadeImage != null)
            {
                Color c = fadeImage.color;
                c.a = 0f;
                fadeImage.color = c;
            }
        }

        void Start()
        {
            if (!manualModeTexts)
                UpdateTexts();

            if (!manualModeButtons)
                UpdateButtons();

            SetStartVolume();
        }

        #region Levels
        public void LoadLevel()
        {
            // Fade Animation
            if (main_animator != null)
            {
                main_animator.enabled = true;
                main_animator.SetTrigger("LoadScene");
            }

            // Запускаем плавное появление изображения
            if (fadeImage != null)
            {
                StartCoroutine(FadeInImage());
            }

            StartCoroutine(WaitToLoadLevel());
        }

        IEnumerator FadeInImage()
        {
            float elapsedTime = 0f;
            Color startColor = fadeImage.color;
            Color targetColor = startColor;
            targetColor.a = 1f; // 255 в формате 0-1

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeDuration;

                fadeImage.color = Color.Lerp(startColor, targetColor, t);

                yield return null;
            }

            // Гарантируем, что финальное значение точное
            fadeImage.color = targetColor;
        }

        IEnumerator WaitToLoadLevel()
        {
            yield return new WaitForSeconds(delayBeforeLoading);

            // Scene Load
            SceneManager.LoadScene(sceneToLoad);
        }

        public void Quit()
        {
            Application.Quit();
        }
        #endregion

        #region Audio

        // Master Volume
        public void SetVolume(float _volume)
        {
            if (audioMixer != null)
            {
                float db = VolumeToDecibels(_volume);
                audioMixer.SetFloat(masterParameter, db);
            }
            PlayerPrefs.SetFloat("MasterVolume", _volume);
        }

        // FX Volume
        public void SetFXVolume(float _volume)
        {
            if (audioMixer != null)
            {
                float db = VolumeToDecibels(_volume);
                audioMixer.SetFloat(fxParameter, db);
            }
            PlayerPrefs.SetFloat("FXVolume", _volume);
        }

        // Music Volume
        public void SetMusicVolume(float _volume)
        {
            if (audioMixer != null)
            {
                float db = VolumeToDecibels(_volume);
                audioMixer.SetFloat(musicParameter, db);
            }
            PlayerPrefs.SetFloat("MusicVolume", _volume);
        }

        // Конвертация из линейного значения (0-1) в децибелы (-80 до 0)
        float VolumeToDecibels(float volume)
        {
            if (volume <= 0f)
                return -80f;

            return Mathf.Log10(volume) * 20f;
        }

        void SetStartVolume()
        {
            // Master Volume
            if (!PlayerPrefs.HasKey("MasterVolume"))
            {
                PlayerPrefs.SetFloat("MasterVolume", defaultMasterVolume);
            }

            // FX Volume
            if (!PlayerPrefs.HasKey("FXVolume"))
            {
                PlayerPrefs.SetFloat("FXVolume", defaultFXVolume);
            }

            // Music Volume
            if (!PlayerPrefs.HasKey("MusicVolume"))
            {
                PlayerPrefs.SetFloat("MusicVolume", defaultMusicVolume);
            }

            LoadVolume();
        }

        public void LoadVolume()
        {
            // Master Volume
            if (volumeSlider != null)
            {
                float masterVol = PlayerPrefs.GetFloat("MasterVolume");
                volumeSlider.value = masterVol;
                SetVolume(masterVol);
            }

            // FX Volume
            if (fxSlider != null)
            {
                float fxVol = PlayerPrefs.GetFloat("FXVolume");
                fxSlider.value = fxVol;
                SetFXVolume(fxVol);
            }

            // Music Volume
            if (musicSlider != null)
            {
                float musicVol = PlayerPrefs.GetFloat("MusicVolume");
                musicSlider.value = musicVol;
                SetMusicVolume(musicVol);
            }
        }

        public void UIClick()
        {
            if (audioSource != null)
                audioSource.PlayOneShot(sound_click);
        }

        public void UIHover()
        {
            if (audioSource != null)
                audioSource.PlayOneShot(sound_hover);
        }

        public void UISpecial()
        {
            if (audioSource != null)
                audioSource.PlayOneShot(sound_loadScene);
        }

        #endregion

        void IntroSequence()
        {
            if (main_animator != null)
            {
                switch (introState)
                {
                    case Intro.OneLiner_FadingMenu:

                        if (!manualModeTexts && introText != null)
                            introText.text = introTextContent;

                        main_animator.SetTrigger("OneLiner_FadingMenu");
                        break;

                    case Intro.FadingMenu:
                        main_animator.SetTrigger("FadingMenu");
                        break;

                    case Intro.MenuOnly:
                        main_animator.SetTrigger("MenuOnly");
                        break;
                }
            }

            // Soundtrack
            if (audioSourceSountrack != null)
            {
                if (customSoundtrack)
                    audioSourceSountrack.clip = customSoundtrackAudio;
                else
                    audioSourceSountrack.clip = (introState == Intro.OneLiner_FadingMenu) ? demo_soundtrack : demo_soundtrack_shorter;

                audioSourceSountrack.Play();
            }
        }

        void UpdateButtons()
        {
            if (buttons == null || buttonsAnimators == null) return;

            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] == null || buttonsAnimators.Length <= i || buttonsAnimators[i] == null)
                    continue;

                switch (buttonsAppearance)
                {
                    case Buttons.Rounded:
                        buttons[i].sprite = buttonRounded;
                        buttonsAnimators[i].runtimeAnimatorController = buttonsAnimator_darkText;
                        break;
                    case Buttons.Squared:
                        buttons[i].sprite = buttonSquared;
                        buttonsAnimators[i].runtimeAnimatorController = buttonsAnimator_darkText;
                        break;
                    case Buttons.Squared_Outlined:
                        buttons[i].sprite = buttonSquaredOutlined;
                        buttonsAnimators[i].runtimeAnimatorController = buttonsAnimator_lightText;
                        break;
                    case Buttons.Rounded_Outlined:
                        buttons[i].sprite = buttonRoundedOutlined;
                        buttonsAnimators[i].runtimeAnimatorController = buttonsAnimator_lightText;
                        break;
                    case Buttons.Rounded_AlwaysFilled:
                        buttons[i].sprite = buttonRounded;
                        buttonsAnimators[i].runtimeAnimatorController = buttonsAnimator_alwaysFilled;
                        break;
                    case Buttons.Squared_AlwaysFilled:
                        buttons[i].sprite = buttonSquared;
                        buttonsAnimators[i].runtimeAnimatorController = buttonsAnimator_alwaysFilled;
                        break;
                }
            }
        }

        void UpdateTexts()
        {
            if (homePanel_text_play != null)
                homePanel_text_play.text = play;

            if (homePanel_text_options != null)
                homePanel_text_options.text = settings;

            if (homePanel_text_quit != null)
                homePanel_text_quit.text = quit;
        }

        // SAFE VERSION – без NullReferenceException в инспекторе
        public void UIEditorUpdate()
        {
            #region Colors
            if (!manualModeColor)
            {
                if (background_sprite != null)
                    background_sprite.color = mainColor;

                if (godrays_sprite != null)
                {
                    Color newColor_godrays = mainColor;
                    newColor_godrays.a = alpha_godrays;

                    foreach (var g in godrays_sprite)
                        if (g != null)
                            g.color = newColor_godrays;
                }

                if (particles != null)
                {
                    for (int i = 0; i < particles.Length; i++)
                    {
                        if (particles[i] == null) continue;

                        var main1 = particles[i].main;

                        if (i == 2)
                        {
                            Color c = mainColor;
                            c.a = alpha_particleHuge;
                            main1.startColor = new ParticleSystem.MinMaxGradient(c);
                        }
                        else
                        {
                            Color c = mainColor;
                            c.a = alpha_particleSlowNormal;
                            main1.startColor = new ParticleSystem.MinMaxGradient(c);
                        }
                    }
                }
            }
            #endregion

            if (!manualModeTexts)
                UpdateTexts();

            if (!manualModeButtons)
                UpdateButtons();

            #region Particles Prewarm
            if (particles != null)
            {
                for (int i = 0; i < particles.Length; i++)
                {
                    if (particles[i] == null) continue;

                    var main1 = particles[i].main;

                    if (introState == Intro.MenuOnly)
                        main1.prewarm = true;
                    else
                        main1.prewarm = (i == 0);
                }
            }
            #endregion
        }

        public void _fadingAnimationIsDone()
        {
            if (main_animator != null)
                main_animator.enabled = false;

            if (homePanel != null)
                homePanel.blocksRaycasts = true;
        }
    }
}
