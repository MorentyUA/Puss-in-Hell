using UnityEngine;
using UnityEngine.UI;

namespace PussInHell.Settings
{
    public class VolumeSlidersUI : MonoBehaviour
    {
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider fxSlider;
        [SerializeField] private Slider musicSlider;

        [Header("UI Sounds")]
        [SerializeField] private AudioSource uiAudioSource;
        [UnityEngine.Serialization.FormerlySerializedAs("sound_click")]
        [SerializeField] private AudioClip clickSound;
        [UnityEngine.Serialization.FormerlySerializedAs("sound_hover")]
        [SerializeField] private AudioClip hoverSound;

        private void Awake()
        {
            if (uiAudioSource == null) uiAudioSource = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            Refresh();
            if (masterSlider != null) masterSlider.onValueChanged.AddListener(SetMasterVolume);
            if (fxSlider != null) fxSlider.onValueChanged.AddListener(SetFXVolume);
            if (musicSlider != null) musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        private void OnDisable()
        {
            if (masterSlider != null) masterSlider.onValueChanged.RemoveListener(SetMasterVolume);
            if (fxSlider != null) fxSlider.onValueChanged.RemoveListener(SetFXVolume);
            if (musicSlider != null) musicSlider.onValueChanged.RemoveListener(SetMusicVolume);
        }

        public void Refresh()
        {
            var settings = GameSettings.Instance;
            if (settings == null) return;
            if (masterSlider != null) masterSlider.SetValueWithoutNotify(settings.MasterVolume);
            if (fxSlider != null) fxSlider.SetValueWithoutNotify(settings.FXVolume);
            if (musicSlider != null) musicSlider.SetValueWithoutNotify(settings.MusicVolume);
        }

        public void SetMasterVolume(float volume)
        {
            if (GameSettings.Instance != null) GameSettings.Instance.MasterVolume = volume;
        }

        public void SetFXVolume(float volume)
        {
            if (GameSettings.Instance != null) GameSettings.Instance.FXVolume = volume;
        }

        public void SetMusicVolume(float volume)
        {
            if (GameSettings.Instance != null) GameSettings.Instance.MusicVolume = volume;
        }

        public void PlayClickSound() => Play(clickSound);

        public void PlayHoverSound() => Play(hoverSound);

        private void Play(AudioClip clip)
        {
            if (uiAudioSource != null && clip != null) uiAudioSource.PlayOneShot(clip);
        }
    }
}
