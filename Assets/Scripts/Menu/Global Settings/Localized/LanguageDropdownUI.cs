using UnityEngine;
using TMPro;
using System.Collections.Generic;
using GabrielBissonnette.SAD;

/// <summary>
/// UI компонент для выбора языка через Dropdown
/// Добавьте этот скрипт на GameObject с компонентом TMP_Dropdown
/// </summary>
[RequireComponent(typeof(TMP_Dropdown))]
public class LanguageDropdownUI : MonoBehaviour
{
    [Header("Настройки отображения")]
    [Tooltip("Показывать названия языков на самих языках")]
    [SerializeField] private bool useNativeNames = true;

    private TMP_Dropdown dropdown;

    // Названия языков на английском
    private readonly Dictionary<GameLanguage, string> englishNames = new Dictionary<GameLanguage, string>
    {
        { GameLanguage.Russian, "Russian" },
        { GameLanguage.English, "English" },
        { GameLanguage.Japanese, "Japanese" },
        { GameLanguage.Ukrainian, "Ukrainian" },
        { GameLanguage.German, "German" },
        { GameLanguage.French, "French" },
        { GameLanguage.Spanish, "Spanish" },
        { GameLanguage.Turkish, "Turkish" },
        { GameLanguage.Italian, "Italian" },
        { GameLanguage.Polish, "Polish" }
    };

    // Названия языков на родных языках
    private readonly Dictionary<GameLanguage, string> nativeNames = new Dictionary<GameLanguage, string>
    {
        { GameLanguage.Russian, "Русский" },
        { GameLanguage.English, "English" },
        { GameLanguage.Japanese, "日本語" },
        { GameLanguage.Ukrainian, "Українська" },
        { GameLanguage.German, "Deutsch" },
        { GameLanguage.French, "Français" },
        { GameLanguage.Spanish, "Español" },
        { GameLanguage.Turkish, "Türkçe" },
        { GameLanguage.Italian, "Italiano" },
        { GameLanguage.Polish, "Polski" }
    };

    private void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
    }

    private void Start()
    {
        InitializeDropdown();
        SetCurrentLanguage();

        // Подписываемся на изменение значения dropdown
        dropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    private void OnDestroy()
    {
        if (dropdown != null)
        {
            dropdown.onValueChanged.RemoveListener(OnLanguageChanged);
        }
    }

    /// <summary>
    /// Инициализирует dropdown списком языков
    /// </summary>
    private void InitializeDropdown()
    {
        dropdown.ClearOptions();

        List<string> options = new List<string>();
        Dictionary<GameLanguage, string> namesToUse = useNativeNames ? nativeNames : englishNames;

        // Добавляем все языки в том порядке, в котором они определены в enum
        foreach (GameLanguage lang in System.Enum.GetValues(typeof(GameLanguage)))
        {
            if (namesToUse.ContainsKey(lang))
            {
                options.Add(namesToUse[lang]);
            }
            else
            {
                // Если название не найдено, используем имя из enum
                options.Add(lang.ToString());
            }
        }

        dropdown.AddOptions(options);
    }

    /// <summary>
    /// Устанавливает текущий язык в dropdown
    /// </summary>
    private void SetCurrentLanguage()
    {
        if (GlobalSettingsManager.Instance != null)
        {
            int currentLanguageIndex = (int)GlobalSettingsManager.Instance.CurrentLanguage;
            dropdown.value = currentLanguageIndex;
            dropdown.RefreshShownValue();
        }
    }

    /// <summary>
    /// Обработчик изменения языка в dropdown
    /// </summary>
    private void OnLanguageChanged(int index)
    {
        if (GlobalSettingsManager.Instance != null)
        {
            GlobalSettingsManager.Instance.SetLanguageByIndex(index);
            Debug.Log($"Язык изменён на: {(GameLanguage)index}");
        }
        else
        {
            Debug.LogWarning("GlobalSettingsManager не найден! Убедитесь, что он есть на сцене.");
        }
    }

    /// <summary>
    /// Обновить dropdown (полезно если язык изменился извне)
    /// </summary>
    public void RefreshDropdown()
    {
        SetCurrentLanguage();
    }

    /// <summary>
    /// Переключить режим отображения названий (родные/английские)
    /// </summary>
    public void ToggleNativeNames()
    {
        useNativeNames = !useNativeNames;
        int currentIndex = dropdown.value;
        InitializeDropdown();
        dropdown.value = currentIndex;
        dropdown.RefreshShownValue();
    }
}
