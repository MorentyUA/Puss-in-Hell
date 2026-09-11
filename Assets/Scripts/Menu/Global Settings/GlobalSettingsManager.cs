using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

namespace GabrielBissonnette.SAD
{
    /// <summary>
    /// Глобальный менеджер настроек игры.
    /// Хранит все настройки в PlayerPrefs и применяет их при запуске.
    /// Добавьте этот скрипт на GameObject в ПЕРВОЙ сцене игры и сделайте его DontDestroyOnLoad.
    /// </summary>
    public class GlobalSettingsManager : MonoBehaviour
    {
        public static GlobalSettingsManager Instance { get; private set; }

        [Header("Audio Mixer")]
        [SerializeField] AudioMixer audioMixer;
        [SerializeField] string masterParameter = "MasterVolume";
        [SerializeField] string fxParameter = "FXVolume";
        [SerializeField] string musicParameter = "MusicVolume";

        [Header("Default Values")]
        [SerializeField] float defaultMasterVolume = 0.7f;
        [SerializeField] float defaultFXVolume = 0.7f;
        [SerializeField] float defaultMusicVolume = 0.7f;
        [SerializeField] int defaultQualityLevel = 1; // 0 = Low, 1 = Medium, 2 = High
        [SerializeField] bool defaultVSyncEnabled = true;
        [SerializeField] GameLanguage defaultLanguage = GameLanguage.English;

        // Ключи для PlayerPrefs
        private const string KEY_MASTER_VOLUME = "MasterVolume";
        private const string KEY_FX_VOLUME = "FXVolume";
        private const string KEY_MUSIC_VOLUME = "MusicVolume";
        private const string KEY_QUALITY_LEVEL = "QualityLevel";
        private const string KEY_VSYNC_ENABLED = "VSyncEnabled";
        private const string KEY_LANGUAGE = "Language";

        // Текущие значения (кэшируем для быстрого доступа)
        private float _masterVolume;
        private float _fxVolume;
        private float _musicVolume;
        private int _qualityLevel;
        private bool _vSyncEnabled;
        private GameLanguage _currentLanguage;

        // Event для уведомления об изменении языка
        public UnityEvent OnLanguageChanged = new UnityEvent();

        #region Properties (публичный доступ к настройкам)

        public float MasterVolume
        {
            get => _masterVolume;
            set
            {
                _masterVolume = Mathf.Clamp01(value);
                ApplyMasterVolume();
                SaveMasterVolume();
            }
        }

        public float FXVolume
        {
            get => _fxVolume;
            set
            {
                _fxVolume = Mathf.Clamp01(value);
                ApplyFXVolume();
                SaveFXVolume();
            }
        }

        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = Mathf.Clamp01(value);
                ApplyMusicVolume();
                SaveMusicVolume();
            }
        }

        public int QualityLevel
        {
            get => _qualityLevel;
            set
            {
                _qualityLevel = Mathf.Clamp(value, 0, QualitySettings.names.Length - 1);
                ApplyQualityLevel();
                SaveQualityLevel();
            }
        }

        public bool VSyncEnabled
        {
            get => _vSyncEnabled;
            set
            {
                _vSyncEnabled = value;
                ApplyVSync();
                SaveVSync();
            }
        }

        public GameLanguage CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                _currentLanguage = value;
                SaveLanguage();
                ApplyLanguage();
            }
        }

        /// <summary>
        /// Получить название текущего уровня качества
        /// </summary>
        public string QualityLevelName => QualitySettings.names[_qualityLevel];

        #endregion

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Загружаем настройки при старте
            LoadAllSettings();
        }

        private void Start()
        {
            // Применяем все настройки
            ApplyAllSettings();
        }

        #region Load Settings

        /// <summary>
        /// Загружает все настройки из PlayerPrefs
        /// </summary>
        public void LoadAllSettings()
        {
            LoadMasterVolume();
            LoadFXVolume();
            LoadMusicVolume();
            LoadQualityLevel();
            LoadVSync();
            LoadLanguage();
        }

        private void LoadMasterVolume()
        {
            if (!PlayerPrefs.HasKey(KEY_MASTER_VOLUME))
            {
                _masterVolume = defaultMasterVolume;
                SaveMasterVolume();
            }
            else
            {
                _masterVolume = PlayerPrefs.GetFloat(KEY_MASTER_VOLUME);
            }
        }

        private void LoadFXVolume()
        {
            if (!PlayerPrefs.HasKey(KEY_FX_VOLUME))
            {
                _fxVolume = defaultFXVolume;
                SaveFXVolume();
            }
            else
            {
                _fxVolume = PlayerPrefs.GetFloat(KEY_FX_VOLUME);
            }
        }

        private void LoadMusicVolume()
        {
            if (!PlayerPrefs.HasKey(KEY_MUSIC_VOLUME))
            {
                _musicVolume = defaultMusicVolume;
                SaveMusicVolume();
            }
            else
            {
                _musicVolume = PlayerPrefs.GetFloat(KEY_MUSIC_VOLUME);
            }
        }

        private void LoadQualityLevel()
        {
            if (!PlayerPrefs.HasKey(KEY_QUALITY_LEVEL))
            {
                _qualityLevel = defaultQualityLevel;
                SaveQualityLevel();
            }
            else
            {
                _qualityLevel = PlayerPrefs.GetInt(KEY_QUALITY_LEVEL);
                _qualityLevel = Mathf.Clamp(_qualityLevel, 0, QualitySettings.names.Length - 1);
            }
        }

        private void LoadVSync()
        {
            if (!PlayerPrefs.HasKey(KEY_VSYNC_ENABLED))
            {
                _vSyncEnabled = defaultVSyncEnabled;
                SaveVSync();
            }
            else
            {
                _vSyncEnabled = PlayerPrefs.GetInt(KEY_VSYNC_ENABLED) == 1;
            }
        }

        private void LoadLanguage()
        {
            if (!PlayerPrefs.HasKey(KEY_LANGUAGE))
            {
                _currentLanguage = defaultLanguage;
                SaveLanguage();
            }
            else
            {
                _currentLanguage = (GameLanguage)PlayerPrefs.GetInt(KEY_LANGUAGE);
            }
        }

        #endregion

        #region Save Settings

        private void SaveMasterVolume()
        {
            PlayerPrefs.SetFloat(KEY_MASTER_VOLUME, _masterVolume);
            PlayerPrefs.Save();
        }

        private void SaveFXVolume()
        {
            PlayerPrefs.SetFloat(KEY_FX_VOLUME, _fxVolume);
            PlayerPrefs.Save();
        }

        private void SaveMusicVolume()
        {
            PlayerPrefs.SetFloat(KEY_MUSIC_VOLUME, _musicVolume);
            PlayerPrefs.Save();
        }

        private void SaveQualityLevel()
        {
            PlayerPrefs.SetInt(KEY_QUALITY_LEVEL, _qualityLevel);
            PlayerPrefs.Save();
        }

        private void SaveVSync()
        {
            PlayerPrefs.SetInt(KEY_VSYNC_ENABLED, _vSyncEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void SaveLanguage()
        {
            PlayerPrefs.SetInt(KEY_LANGUAGE, (int)_currentLanguage);
            PlayerPrefs.Save();
        }

        #endregion

        #region Apply Settings

        /// <summary>
        /// Применяет все настройки к игре
        /// </summary>
        public void ApplyAllSettings()
        {
            ApplyMasterVolume();
            ApplyFXVolume();
            ApplyMusicVolume();
            ApplyQualityLevel();
            ApplyVSync();
            ApplyLanguage();
        }

        private void ApplyMasterVolume()
        {
            if (audioMixer != null)
            {
                float db = VolumeToDecibels(_masterVolume);
                audioMixer.SetFloat(masterParameter, db);
            }
        }

        private void ApplyFXVolume()
        {
            if (audioMixer != null)
            {
                float db = VolumeToDecibels(_fxVolume);
                audioMixer.SetFloat(fxParameter, db);
            }
        }

        private void ApplyMusicVolume()
        {
            if (audioMixer != null)
            {
                float db = VolumeToDecibels(_musicVolume);
                audioMixer.SetFloat(musicParameter, db);
            }
        }

        private void ApplyQualityLevel()
        {
            QualitySettings.SetQualityLevel(_qualityLevel, true);
            Debug.Log($"Уровень качества установлен: {QualitySettings.names[_qualityLevel]} (индекс: {_qualityLevel})");
        }

        private void ApplyVSync()
        {
            QualitySettings.vSyncCount = _vSyncEnabled ? 1 : 0;
            Debug.Log($"VSync: {(_vSyncEnabled ? "ВКЛЮЧЁН" : "ВЫКЛЮЧЕН")}");
        }

        private void ApplyLanguage()
        {
            Debug.Log($"Язык установлен: {_currentLanguage}");
            OnLanguageChanged?.Invoke();

            // Обновляем все LocalizedTMP объекты на сцене
            LocalizedTMP[] localizedTexts = FindObjectsByType<LocalizedTMP>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var text in localizedTexts)
            {
                text.UpdateText();
            }
        }

        #endregion

        #region Quality Level Methods

        /// <summary>
        /// Переключить на следующий уровень качества (циклично)
        /// </summary>
        public void CycleQualityLevel()
        {
            QualityLevel = (_qualityLevel + 1) % QualitySettings.names.Length;
        }

        /// <summary>
        /// Переключить на предыдущий уровень качества (циклично)
        /// </summary>
        public void CycleQualityLevelBack()
        {
            _qualityLevel--;
            if (_qualityLevel < 0)
                _qualityLevel = QualitySettings.names.Length - 1;

            QualityLevel = _qualityLevel;
        }

        /// <summary>
        /// Установить качество по названию
        /// </summary>
        public void SetQualityByName(string qualityName)
        {
            string[] names = QualitySettings.names;
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i].Equals(qualityName, System.StringComparison.OrdinalIgnoreCase))
                {
                    QualityLevel = i;
                    return;
                }
            }
            Debug.LogWarning($"Уровень качества '{qualityName}' не найден!");
        }

        /// <summary>
        /// Получить все доступные уровни качества
        /// </summary>
        public string[] GetAllQualityLevels()
        {
            return QualitySettings.names;
        }

        #endregion

        #region VSync Methods

        /// <summary>
        /// Переключить VSync (вкл/выкл)
        /// </summary>
        public void ToggleVSync()
        {
            VSyncEnabled = !VSyncEnabled;
        }

        #endregion

        #region Language Methods

        /// <summary>
        /// Установить язык по индексу
        /// </summary>
        public void SetLanguageByIndex(int index)
        {
            if (System.Enum.IsDefined(typeof(GameLanguage), index))
            {
                CurrentLanguage = (GameLanguage)index;
            }
            else
            {
                Debug.LogWarning($"Неверный индекс языка: {index}");
            }
        }

        /// <summary>
        /// Получить индекс текущего языка
        /// </summary>
        public int GetCurrentLanguageIndex()
        {
            return (int)_currentLanguage;
        }

        /// <summary>
        /// Получить название текущего языка
        /// </summary>
        public string GetCurrentLanguageName()
        {
            return _currentLanguage.ToString();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Конвертация из линейного значения (0-1) в децибелы (-80 до 0)
        /// </summary>
        private float VolumeToDecibels(float volume)
        {
            if (volume <= 0f)
                return -80f;

            return Mathf.Log10(volume) * 20f;
        }

        /// <summary>
        /// Сброс всех настроек к значениям по умолчанию
        /// </summary>
        public void ResetToDefaults()
        {
            MasterVolume = defaultMasterVolume;
            FXVolume = defaultFXVolume;
            MusicVolume = defaultMusicVolume;
            QualityLevel = defaultQualityLevel;
            VSyncEnabled = defaultVSyncEnabled;
            CurrentLanguage = defaultLanguage;

            Debug.Log("Настройки сброшены к значениям по умолчанию");
        }

        /// <summary>
        /// Очистка всех сохраненных настроек
        /// </summary>
        public void ClearAllSettings()
        {
            PlayerPrefs.DeleteKey(KEY_MASTER_VOLUME);
            PlayerPrefs.DeleteKey(KEY_FX_VOLUME);
            PlayerPrefs.DeleteKey(KEY_MUSIC_VOLUME);
            PlayerPrefs.DeleteKey(KEY_QUALITY_LEVEL);
            PlayerPrefs.DeleteKey(KEY_VSYNC_ENABLED);
            PlayerPrefs.DeleteKey(KEY_LANGUAGE);
            PlayerPrefs.Save();

            Debug.Log("Все настройки удалены из PlayerPrefs");
        }

        #endregion

        #region Debug Methods

        [ContextMenu("Print Current Settings")]
        private void PrintCurrentSettings()
        {
            Debug.Log($"=== ТЕКУЩИЕ НАСТРОЙКИ ===\n" +
                      $"Master Volume: {_masterVolume:F2} ({VolumeToDecibels(_masterVolume):F1} dB)\n" +
                      $"FX Volume: {_fxVolume:F2} ({VolumeToDecibels(_fxVolume):F1} dB)\n" +
                      $"Music Volume: {_musicVolume:F2} ({VolumeToDecibels(_musicVolume):F1} dB)\n" +
                      $"Quality Level: {QualitySettings.names[_qualityLevel]} (индекс: {_qualityLevel})\n" +
                      $"VSync: {(_vSyncEnabled ? "ВКЛЮЧЁН" : "ВЫКЛЮЧЕН")}\n" +
                      $"Language: {_currentLanguage}");
        }

        [ContextMenu("Reset To Defaults")]
        private void DebugResetToDefaults()
        {
            ResetToDefaults();
        }

        [ContextMenu("Clear All Settings")]
        private void DebugClearAllSettings()
        {
            ClearAllSettings();
        }

        [ContextMenu("Cycle Quality Level")]
        private void DebugCycleQuality()
        {
            CycleQualityLevel();
            PrintCurrentSettings();
        }

        [ContextMenu("Toggle VSync")]
        private void DebugToggleVSync()
        {
            ToggleVSync();
            PrintCurrentSettings();
        }

        #endregion
    }
}
