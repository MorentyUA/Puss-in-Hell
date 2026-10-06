using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PussInHell.Settings;

namespace PussInHell.Localization
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public class LanguageDropdownUI : MonoBehaviour
    {
        [Tooltip("Show each language in its own script instead of English names")]
        [SerializeField] private bool useNativeNames = true;

        private static readonly Dictionary<GameLanguage, string> NativeNames = new Dictionary<GameLanguage, string>
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

        private TMP_Dropdown dropdown;

        private void Awake()
        {
            dropdown = GetComponent<TMP_Dropdown>();
        }

        private void Start()
        {
            FillOptions();
            dropdown.SetValueWithoutNotify((int)LanguageProvider.Current);
            dropdown.RefreshShownValue();
            dropdown.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDestroy()
        {
            if (dropdown != null) dropdown.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void FillOptions()
        {
            var options = new List<string>();
            foreach (GameLanguage language in Enum.GetValues(typeof(GameLanguage)))
                options.Add(useNativeNames && NativeNames.TryGetValue(language, out var native) ? native : language.ToString());

            dropdown.ClearOptions();
            dropdown.AddOptions(options);
        }

        private void OnValueChanged(int index)
        {
            if (!Enum.IsDefined(typeof(GameLanguage), index)) return;

            if (GameSettings.Instance != null) GameSettings.Instance.Language = (GameLanguage)index;
            else LanguageProvider.Current = (GameLanguage)index;
        }

        public void Refresh()
        {
            dropdown.SetValueWithoutNotify((int)LanguageProvider.Current);
            dropdown.RefreshShownValue();
        }

        public void ToggleNativeNames()
        {
            useNativeNames = !useNativeNames;
            int index = dropdown.value;
            FillOptions();
            dropdown.SetValueWithoutNotify(index);
            dropdown.RefreshShownValue();
        }
    }
}
