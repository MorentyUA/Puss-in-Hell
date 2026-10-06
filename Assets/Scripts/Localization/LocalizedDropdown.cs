using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PussInHell.Localization
{
    public class LocalizedDropdown : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown tmpDropdown;
        [SerializeField] private Dropdown uGuiDropdown;

        [Header("Options per language (same count in every list)")]
        public List<string> Russian = new List<string> { "Высокий", "Нормальный", "Низкий" };
        public List<string> English = new List<string> { "High", "Normal", "Low" };
        public List<string> Japanese = new List<string> { "ハイ", "普通", "低い" };
        public List<string> Ukrainian = new List<string> { "Високий", "Нормальний", "Низький" };
        public List<string> German = new List<string> { "Hoch", "Normal", "Niedrig" };
        public List<string> French = new List<string> { "Haut", "Normal", "Faible" };
        public List<string> Spanish = new List<string> { "Alto", "Normal", "Bajo" };
        public List<string> Turkish = new List<string> { "Yüksek", "Normal", "Düşük" };
        public List<string> Italian = new List<string> { "Alto", "Normale", "Basso" };
        public List<string> Polish = new List<string> { "Wysoki", "Normalny", "Niski" };

        private void Awake()
        {
            if (tmpDropdown == null) tmpDropdown = GetComponent<TMP_Dropdown>();
            if (uGuiDropdown == null) uGuiDropdown = GetComponent<Dropdown>();
        }

        private void OnEnable()
        {
            LanguageProvider.Changed += OnLanguageChanged;
            UpdateDropdown();
        }

        private void OnDisable()
        {
            LanguageProvider.Changed -= OnLanguageChanged;
        }

        private void OnLanguageChanged(GameLanguage language) => UpdateDropdown();

        public void UpdateDropdown()
        {
            var options = GetOptions(LanguageProvider.Current);

            if (tmpDropdown != null)
            {
                int index = tmpDropdown.value;
                tmpDropdown.ClearOptions();
                tmpDropdown.AddOptions(options);
                tmpDropdown.SetValueWithoutNotify(Mathf.Clamp(index, 0, options.Count - 1));
                tmpDropdown.RefreshShownValue();
            }
            else if (uGuiDropdown != null)
            {
                int index = uGuiDropdown.value;
                uGuiDropdown.ClearOptions();
                uGuiDropdown.AddOptions(options);
                uGuiDropdown.SetValueWithoutNotify(Mathf.Clamp(index, 0, options.Count - 1));
                uGuiDropdown.RefreshShownValue();
            }
        }

        private List<string> GetOptions(GameLanguage language)
        {
            switch (language)
            {
                case GameLanguage.Russian: return Russian;
                case GameLanguage.Japanese: return Japanese;
                case GameLanguage.Ukrainian: return Ukrainian;
                case GameLanguage.German: return German;
                case GameLanguage.French: return French;
                case GameLanguage.Spanish: return Spanish;
                case GameLanguage.Turkish: return Turkish;
                case GameLanguage.Italian: return Italian;
                case GameLanguage.Polish: return Polish;
                default: return English;
            }
        }
    }
}
