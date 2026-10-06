using UnityEngine;
using TMPro;

namespace PussInHell.Localization
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedTMP : MonoBehaviour
    {
        [TextArea] public string Russian;
        [TextArea] public string English;
        [TextArea] public string Japanese;
        [TextArea] public string Ukrainian;
        [TextArea] public string German;
        [TextArea] public string French;
        [TextArea] public string Spanish;
        [TextArea] public string Turkish;
        [TextArea] public string Italian;
        [TextArea] public string Polish;

        private TMP_Text text;

        private void Awake()
        {
            text = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            LanguageProvider.Changed += OnLanguageChanged;
            UpdateText();
        }

        private void OnDisable()
        {
            LanguageProvider.Changed -= OnLanguageChanged;
        }

        private void OnLanguageChanged(GameLanguage language) => UpdateText();

        public void UpdateText()
        {
            if (text == null) text = GetComponent<TMP_Text>();
            if (text != null) text.text = GetText(LanguageProvider.Current);
        }

        public string GetText(GameLanguage language)
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
