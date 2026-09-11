using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System;

namespace GabrielBissonnette.SAD
{
    [RequireComponent(typeof(AudioSource))] // Для UI звуков (опционально)
    public class AudioSettingsManager : MonoBehaviour
    {
        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer audioMixer;

        [Header("Volume Parameters")]
        [SerializeField] private string masterParameter = "MasterVolume";
        [SerializeField] private string fxParameter = "FXVolume";
        [SerializeField] private string musicParameter = "MusicVolume";

        [Header("Default Values (0 to 1)")]
        [Range(0f, 1f)] [SerializeField] private float defaultMasterVolume = 0.7f;
        [Range(0f, 1f)] [SerializeField] private float defaultFXVolume = 0.7f;
        [Range(0f, 1f)] [SerializeField] private float defaultMusicVolume = 0.7f;

        [Header("UI Sliders")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider fxSlider;
        [SerializeField] private Slider musicSlider;

        [Header("UI Click Sounds (Optional)")]
        [SerializeField] private AudioClip sound_click;
        [SerializeField] private AudioClip sound_hover;

        private AudioSource uiAudioSource;

        private const string MASTER_KEY = "MasterVolume";
        private const string FX_KEY = "FXVolume";
        private const string MUSIC_KEY = "MusicVolume";

        private void Awake()
        {
            // Создаём или получаем AudioSource для UI звуков
            uiAudioSource = GetComponent<AudioSource>();
            if (uiAudioSource == null)
            {
                uiAudioSource = gameObject.AddComponent<AudioSource>();
            }

            InitializeVolumePreferences();
            AssignSliderEvents();
            LoadAndApplyVolumes();
        }

        private void InitializeVolumePreferences()
        {
            if (!PlayerPrefs.HasKey(MASTER_KEY))
                PlayerPrefs.SetFloat(MASTER_KEY, defaultMasterVolume);

            if (!PlayerPrefs.HasKey(FX_KEY))
                PlayerPrefs.SetFloat(FX_KEY, defaultFXVolume);

            if (!PlayerPrefs.HasKey(MUSIC_KEY))
                PlayerPrefs.SetFloat(MUSIC_KEY, defaultMusicVolume);

            PlayerPrefs.Save();
        }

        private void AssignSliderEvents()
        {
            if (masterSlider != null)
                masterSlider.onValueChanged.AddListener(SetMasterVolume);

            if (fxSlider != null)
                fxSlider.onValueChanged.AddListener(SetFXVolume);

            if (musicSlider != null)
                musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        private void LoadAndApplyVolumes()
        {
            float masterVol = PlayerPrefs.GetFloat(MASTER_KEY);
            float fxVol = PlayerPrefs.GetFloat(FX_KEY);
            float musicVol = PlayerPrefs.GetFloat(MUSIC_KEY);

            if (masterSlider != null) masterSlider.value = masterVol;
            if (fxSlider != null) fxSlider.value = fxVol;
            if (musicSlider != null) musicSlider.value = musicVol;

            SetMasterVolume(masterVol);
            SetFXVolume(fxVol);
            SetMusicVolume(musicVol);
        }

        #region Volume Setters

        public void SetMasterVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(MASTER_KEY, volume);
            ApplyVolumeToMixer(masterParameter, volume);
        }

        public void SetFXVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(FX_KEY, volume);
            ApplyVolumeToMixer(fxParameter, volume);
        }

        public void SetMusicVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(MUSIC_KEY, volume);
            ApplyVolumeToMixer(musicParameter, volume);
        }

        private void ApplyVolumeToMixer(string parameter, float linearVolume)
        {
            if (audioMixer == null) return;

            float db = linearVolume > 0f ? Mathf.Log10(linearVolume) * 20f : -80f;
            audioMixer.SetFloat(parameter, db);
        }

        #endregion

        #region UI Sound Effects

        public void PlayClickSound()
        {
            if (uiAudioSource != null && sound_click != null)
                uiAudioSource.PlayOneShot(sound_click);
        }

        public void PlayHoverSound()
        {
            if (uiAudioSource != null && sound_hover != null)
                uiAudioSource.PlayOneShot(sound_hover);
        }

        #endregion

        #if UNITY_EDITOR
        // Для удобной отладки в инспекторе
        [ContextMenu("Reset All Volumes to Default")]
        private void ResetVolumes()
        {
            PlayerPrefs.DeleteKey(MASTER_KEY);
            PlayerPrefs.DeleteKey(FX_KEY);
            PlayerPrefs.DeleteKey(MUSIC_KEY);
            InitializeVolumePreferences();
            LoadAndApplyVolumes();
            Debug.Log("Audio volumes reset to defaults.");
        }
        #endif
    }
}
