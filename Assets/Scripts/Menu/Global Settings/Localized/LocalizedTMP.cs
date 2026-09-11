using UnityEngine;
using TMPro;

public class LocalizedTMP : MonoBehaviour
{
    [Header("Тексты на разных языках")]

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

    private TMP_Text textMesh;

    private void Awake()
    {
        textMesh = GetComponent<TMP_Text>();
        UpdateText();
    }

    private void OnEnable()
    {
        UpdateText();
    }

    public void UpdateText()
    {
        if (!textMesh) return;

        switch (LanguageManager.CurrentLanguage)
        {
            case GameLanguage.Russian: textMesh.text = Russian; break;
            case GameLanguage.English: textMesh.text = English; break;
            case GameLanguage.Japanese: textMesh.text = Japanese; break;
            case GameLanguage.Ukrainian: textMesh.text = Ukrainian; break;
            case GameLanguage.German: textMesh.text = German; break;
            case GameLanguage.French: textMesh.text = French; break;
            case GameLanguage.Spanish: textMesh.text = Spanish; break;
            case GameLanguage.Turkish: textMesh.text = Turkish; break;
            case GameLanguage.Italian: textMesh.text = Italian; break;
            case GameLanguage.Polish: textMesh.text = Polish; break;
            default: textMesh.text = English; break;
        }
    }
}
