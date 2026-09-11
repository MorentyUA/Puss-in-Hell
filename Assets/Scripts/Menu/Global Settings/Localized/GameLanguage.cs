using UnityEngine;

public enum GameLanguage
{
    Russian,
    English,
    Japanese,
    Ukrainian,
    German,
    French,
    Spanish,
    Turkish,
    Italian,
    Polish
}

// Этот класс теперь не нужен, логика перенесена в GlobalSettingsManager
// Оставлен для совместимости, но можно удалить
public class LanguageManager : MonoBehaviour
{
    public static GameLanguage CurrentLanguage
    {
        get
        {
            if (GabrielBissonnette.SAD.GlobalSettingsManager.Instance != null)
                return GabrielBissonnette.SAD.GlobalSettingsManager.Instance.CurrentLanguage;
            return GameLanguage.English;
        }
    }

    public static void SetLanguage(GameLanguage lang)
    {
        if (GabrielBissonnette.SAD.GlobalSettingsManager.Instance != null)
            GabrielBissonnette.SAD.GlobalSettingsManager.Instance.CurrentLanguage = lang;
    }
}
