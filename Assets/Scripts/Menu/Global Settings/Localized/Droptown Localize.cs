using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using GabrielBissonnette.SAD; // для GlobalSettingsManager

public class LocalizedDropdown : MonoBehaviour
{
    [Header("Ссылки (можно не задавать вручную)")]
    public TMP_Dropdown tmpDropdown;
    public Dropdown uGuiDropdown;

    [Header("Опции на языках (ровно 3: HIGH / NORMAL / LOW)")]
    public List<string> Russian = new()   { "Высокий", "Нормальный", "Низкий" };
    public List<string> English = new()   { "High", "Normal", "Low" };
    public List<string> Japanese = new()  { "ハイ", "普通", "低い" };
    public List<string> Ukrainian = new() { "Високий", "Нормальний", "Низький" };
    public List<string> German = new()    { "Hoch", "Normal", "Niedrig" };
    public List<string> French = new()    { "Haut", "Normal", "Faible" };
    public List<string> Spanish = new()   { "Alto", "Normal", "Bajo" };
    public List<string> Turkish = new()   { "Yüksek", "Normal", "Düşük" };
    public List<string> Italian = new()   { "Alto", "Normale", "Basso" };
    public List<string> Polish = new()    { "Wysoki", "Normalny", "Niski" };

    void Awake()
    {
        if (!tmpDropdown) tmpDropdown = GetComponent<TMP_Dropdown>();
        if (!uGuiDropdown) uGuiDropdown = GetComponent<Dropdown>();
    }

    void OnEnable()
    {
        StartCoroutine(EnsureSubscribedAndRefresh());
    }

    void OnDisable()
    {
        if (GlobalSettingsManager.Instance != null)
            GlobalSettingsManager.Instance.OnLanguageChanged.RemoveListener(UpdateDropdown);
    }

    IEnumerator EnsureSubscribedAndRefresh()
    {
        // Ждём, пока синглтон появится и загрузит настройки
        while (GlobalSettingsManager.Instance == null)
            yield return null;

        // Подписка на изменения языка
        GlobalSettingsManager.Instance.OnLanguageChanged.RemoveListener(UpdateDropdown);
        GlobalSettingsManager.Instance.OnLanguageChanged.AddListener(UpdateDropdown);

        UpdateDropdown();
    }

    [ContextMenu("UpdateDropdown")]
    public void UpdateDropdown()
    {
        // Если GSM нет, возьмём язык через LanguageManager (подстраховка)
        var lang = (GlobalSettingsManager.Instance != null)
            ? GlobalSettingsManager.Instance.CurrentLanguage
            : LanguageManager.CurrentLanguage; // :contentReference[oaicite:4]{index=4}

        var list = GetList(lang);

        if (tmpDropdown)
        {
            int prevIndex = tmpDropdown.value;
            tmpDropdown.ClearOptions();
            tmpDropdown.AddOptions(list);
            tmpDropdown.value = Mathf.Clamp(prevIndex, 0, list.Count - 1);
            tmpDropdown.RefreshShownValue();
        }
        else if (uGuiDropdown)
        {
            int prevIndex = uGuiDropdown.value;
            uGuiDropdown.ClearOptions();
            uGuiDropdown.AddOptions(list);
            uGuiDropdown.value = Mathf.Clamp(prevIndex, 0, list.Count - 1);
            uGuiDropdown.RefreshShownValue();
        }
        else
        {
            Debug.LogWarning("[LocalizedDropdown] Ни TMP_Dropdown, ни Dropdown не найдены на объекте.");
        }
    }

    List<string> GetList(GameLanguage lang)
    {
        switch (lang)
        {
            case GameLanguage.Russian:   return Russian;
            case GameLanguage.English:   return English;
            case GameLanguage.Japanese:  return Japanese;
            case GameLanguage.Ukrainian: return Ukrainian;
            case GameLanguage.German:    return German;
            case GameLanguage.French:    return French;
            case GameLanguage.Spanish:   return Spanish;
            case GameLanguage.Turkish:   return Turkish;
            case GameLanguage.Italian:   return Italian;
            case GameLanguage.Polish:    return Polish;
            default:                      return English;
        }
    }
}
