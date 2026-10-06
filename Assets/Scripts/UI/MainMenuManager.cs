using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PussInHell.Settings;

namespace GabrielBissonnette.SAD
{
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Sequence Manager")]
        public Intro introState;
        public enum Intro { OneLiner_FadingMenu, FadingMenu, MenuOnly }

        [Header("Buttons")]
        [Space(10)] public bool manualModeButtons;
        public Buttons buttonsAppearance;
        public enum Buttons { Rounded, Rounded_Outlined, Rounded_AlwaysFilled, Squared, Squared_Outlined, Squared_AlwaysFilled }

        [Header("Colors")]
        [Space(10)] public bool manualModeColor;
        public Color32 mainColor;
        public float alpha_godrays = 0.13f;
        public float alpha_particleSlowNormal = 0.5f;
        public float alpha_particleHuge = 0.05f;

        [Header("Intro Sequence")]
        [Space(10)] public bool manualModeIntroText;
        [SerializeField] string introTextContent = "It is never too late to be who you might have been.";

        [Header("Scene")]
        [Space(10)] [SerializeField] string sceneToLoad;
        [SerializeField] float delayBeforeLoading = 3f;

        [Header("Home Panel")]
        [Space(10)] public bool manualModeTexts;
        [SerializeField] string play = "Play";
        [SerializeField] string settings = "Options";
        [SerializeField] string quit = "Quit";

        [Header("Fade Image on Play")]
        [Space(10)] [SerializeField] Image fadeImage;
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

            if (fadeImage != null)
            {
                Color c = fadeImage.color;
                c.a = 0f;
                fadeImage.color = c;
            }
        }

        private void Start()
        {
            if (!manualModeTexts) UpdateTexts();
            if (!manualModeButtons) UpdateButtons();
            LoadVolume();
        }

        #region Levels

        public void LoadLevel()
        {
            if (main_animator != null)
            {
                main_animator.enabled = true;
                main_animator.SetTrigger("LoadScene");
            }

            if (fadeImage != null) StartCoroutine(FadeInImage());
            StartCoroutine(WaitToLoadLevel());
        }

        private IEnumerator FadeInImage()
        {
            Color start = fadeImage.color;
            Color target = start;
            target.a = 1f;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fadeImage.color = Color.Lerp(start, target, elapsed / fadeDuration);
                yield return null;
            }
            fadeImage.color = target;
        }

        private IEnumerator WaitToLoadLevel()
        {
            yield return new WaitForSeconds(delayBeforeLoading);
            SceneManager.LoadScene(sceneToLoad);
        }

        public void Quit()
        {
            Application.Quit();
        }

        #endregion

        #region Audio

        public void SetVolume(float volume)
        {
            if (GameSettings.Instance != null) GameSettings.Instance.MasterVolume = volume;
            else ApplyToMixer(masterParameter, "MasterVolume", volume);
        }

        public void SetFXVolume(float volume)
        {
            if (GameSettings.Instance != null) GameSettings.Instance.FXVolume = volume;
            else ApplyToMixer(fxParameter, "FXVolume", volume);
        }

        public void SetMusicVolume(float volume)
        {
            if (GameSettings.Instance != null) GameSettings.Instance.MusicVolume = volume;
            else ApplyToMixer(musicParameter, "MusicVolume", volume);
        }

        private void ApplyToMixer(string parameter, string prefsKey, float volume)
        {
            if (audioMixer != null) audioMixer.SetFloat(parameter, GameSettings.VolumeToDecibels(volume));
            PlayerPrefs.SetFloat(prefsKey, volume);
        }

        public void LoadVolume()
        {
            float master = GameSettings.Instance != null ? GameSettings.Instance.MasterVolume : PlayerPrefs.GetFloat("MasterVolume", defaultMasterVolume);
            float fx = GameSettings.Instance != null ? GameSettings.Instance.FXVolume : PlayerPrefs.GetFloat("FXVolume", defaultFXVolume);
            float music = GameSettings.Instance != null ? GameSettings.Instance.MusicVolume : PlayerPrefs.GetFloat("MusicVolume", defaultMusicVolume);

            if (volumeSlider != null) { volumeSlider.SetValueWithoutNotify(master); SetVolume(master); }
            if (fxSlider != null) { fxSlider.SetValueWithoutNotify(fx); SetFXVolume(fx); }
            if (musicSlider != null) { musicSlider.SetValueWithoutNotify(music); SetMusicVolume(music); }
        }

        public void UIClick() => PlaySound(sound_click);

        public void UIHover() => PlaySound(sound_hover);

        public void UISpecial() => PlaySound(sound_loadScene);

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
        }

        #endregion

        private void IntroSequence()
        {
            if (main_animator != null)
            {
                switch (introState)
                {
                    case Intro.OneLiner_FadingMenu:
                        if (!manualModeTexts && introText != null) introText.text = introTextContent;
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

            if (audioSourceSountrack == null) return;

            audioSourceSountrack.clip = customSoundtrack
                ? customSoundtrackAudio
                : (introState == Intro.OneLiner_FadingMenu ? demo_soundtrack : demo_soundtrack_shorter);
            audioSourceSountrack.Play();
        }

        private void UpdateButtons()
        {
            if (buttons == null || buttonsAnimators == null) return;

            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] == null || buttonsAnimators.Length <= i || buttonsAnimators[i] == null) continue;

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

        private void UpdateTexts()
        {
            if (homePanel_text_play != null) homePanel_text_play.text = play;
            if (homePanel_text_options != null) homePanel_text_options.text = settings;
            if (homePanel_text_quit != null) homePanel_text_quit.text = quit;
        }

        public void UIEditorUpdate()
        {
            if (!manualModeColor)
            {
                if (background_sprite != null) background_sprite.color = mainColor;

                if (godrays_sprite != null)
                {
                    Color godrays = mainColor;
                    godrays.a = alpha_godrays;
                    foreach (var g in godrays_sprite)
                        if (g != null) g.color = godrays;
                }

                if (particles != null)
                {
                    for (int i = 0; i < particles.Length; i++)
                    {
                        if (particles[i] == null) continue;
                        var main = particles[i].main;
                        Color c = mainColor;
                        c.a = i == 2 ? alpha_particleHuge : alpha_particleSlowNormal;
                        main.startColor = new ParticleSystem.MinMaxGradient(c);
                    }
                }
            }

            if (!manualModeTexts) UpdateTexts();
            if (!manualModeButtons) UpdateButtons();

            if (particles == null) return;
            for (int i = 0; i < particles.Length; i++)
            {
                if (particles[i] == null) continue;
                var main = particles[i].main;
                main.prewarm = introState == Intro.MenuOnly || i == 0;
            }
        }

        public void _fadingAnimationIsDone()
        {
            if (main_animator != null) main_animator.enabled = false;
            if (homePanel != null) homePanel.blocksRaycasts = true;
        }
    }
}
