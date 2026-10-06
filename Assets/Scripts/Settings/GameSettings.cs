using System;
using UnityEngine;
using UnityEngine.Audio;
using PussInHell.Localization;

namespace PussInHell.Settings
{
    public class GameSettings : MonoBehaviour
    {
        public static GameSettings Instance { get; private set; }

        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private string masterParameter = "MasterVolume";
        [SerializeField] private string fxParameter = "FXVolume";
        [SerializeField] private string musicParameter = "MusicVolume";

        [Header("Defaults")]
        [Range(0f, 1f)] [SerializeField] private float defaultMasterVolume = 0.7f;
        [Range(0f, 1f)] [SerializeField] private float defaultFXVolume = 0.7f;
        [Range(0f, 1f)] [SerializeField] private float defaultMusicVolume = 0.7f;
        [SerializeField] private int defaultQualityLevel = 1;
        [SerializeField] private bool defaultVSyncEnabled = true;
        [SerializeField] private GameLanguage defaultLanguage = GameLanguage.English;

        private const string KeyMaster = "MasterVolume";
        private const string KeyFx = "FXVolume";
        private const string KeyMusic = "MusicVolume";
        private const string KeyQuality = "QualityLevel";
        private const string KeyVSync = "VSyncEnabled";
        private const string KeyLanguage = "Language";

        private float masterVolume;
        private float fxVolume;
        private float musicVolume;
        private int qualityLevel;
        private bool vSyncEnabled;

        public event Action Changed;

        public float MasterVolume
        {
            get => masterVolume;
            set { masterVolume = Mathf.Clamp01(value); ApplyVolume(masterParameter, masterVolume); Save(KeyMaster, masterVolume); }
        }

        public float FXVolume
        {
            get => fxVolume;
            set { fxVolume = Mathf.Clamp01(value); ApplyVolume(fxParameter, fxVolume); Save(KeyFx, fxVolume); }
        }

        public float MusicVolume
        {
            get => musicVolume;
            set { musicVolume = Mathf.Clamp01(value); ApplyVolume(musicParameter, musicVolume); Save(KeyMusic, musicVolume); }
        }

        public int QualityLevel
        {
            get => qualityLevel;
            set { qualityLevel = Mathf.Clamp(value, 0, QualitySettings.names.Length - 1); ApplyQuality(); Save(KeyQuality, qualityLevel); }
        }

        public bool VSyncEnabled
        {
            get => vSyncEnabled;
            set { vSyncEnabled = value; ApplyVSync(); Save(KeyVSync, vSyncEnabled ? 1 : 0); }
        }

        public GameLanguage Language
        {
            get => LanguageProvider.Current;
            set { Save(KeyLanguage, (int)value); LanguageProvider.Current = value; Changed?.Invoke(); }
        }

        public string QualityLevelName => QualitySettings.names[qualityLevel];
        public string[] QualityLevelNames => QualitySettings.names;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        private void Start()
        {
            ApplyAll();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Load()
        {
            masterVolume = PlayerPrefs.GetFloat(KeyMaster, defaultMasterVolume);
            fxVolume = PlayerPrefs.GetFloat(KeyFx, defaultFXVolume);
            musicVolume = PlayerPrefs.GetFloat(KeyMusic, defaultMusicVolume);
            qualityLevel = Mathf.Clamp(PlayerPrefs.GetInt(KeyQuality, defaultQualityLevel), 0, QualitySettings.names.Length - 1);
            vSyncEnabled = PlayerPrefs.GetInt(KeyVSync, defaultVSyncEnabled ? 1 : 0) == 1;

            int language = PlayerPrefs.GetInt(KeyLanguage, (int)defaultLanguage);
            LanguageProvider.Current = Enum.IsDefined(typeof(GameLanguage), language) ? (GameLanguage)language : defaultLanguage;
        }

        public void ApplyAll()
        {
            ApplyVolume(masterParameter, masterVolume);
            ApplyVolume(fxParameter, fxVolume);
            ApplyVolume(musicParameter, musicVolume);
            ApplyQuality();
            ApplyVSync();
            LanguageProvider.Refresh();
            Changed?.Invoke();
        }

        private void ApplyVolume(string parameter, float volume)
        {
            if (audioMixer != null) audioMixer.SetFloat(parameter, VolumeToDecibels(volume));
        }

        private void ApplyQuality()
        {
            QualitySettings.SetQualityLevel(qualityLevel, true);
            Changed?.Invoke();
        }

        private void ApplyVSync()
        {
            QualitySettings.vSyncCount = vSyncEnabled ? 1 : 0;
            Changed?.Invoke();
        }

        public void CycleQualityLevel() => QualityLevel = (qualityLevel + 1) % QualitySettings.names.Length;

        public void CycleQualityLevelBack() => QualityLevel = (qualityLevel - 1 + QualitySettings.names.Length) % QualitySettings.names.Length;

        public void SetQualityByName(string qualityName)
        {
            int index = Array.FindIndex(QualitySettings.names, n => n.Equals(qualityName, StringComparison.OrdinalIgnoreCase));
            if (index >= 0) QualityLevel = index;
        }

        public void ToggleVSync() => VSyncEnabled = !VSyncEnabled;

        public void SetLanguageByIndex(int index)
        {
            if (Enum.IsDefined(typeof(GameLanguage), index)) Language = (GameLanguage)index;
        }

        public void ResetToDefaults()
        {
            MasterVolume = defaultMasterVolume;
            FXVolume = defaultFXVolume;
            MusicVolume = defaultMusicVolume;
            QualityLevel = defaultQualityLevel;
            VSyncEnabled = defaultVSyncEnabled;
            Language = defaultLanguage;
        }

        public static float VolumeToDecibels(float volume)
        {
            return volume <= 0f ? -80f : Mathf.Log10(volume) * 20f;
        }

        private static void Save(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
        }

        private static void Save(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
        }
    }
}
