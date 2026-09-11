using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using GabrielBissonnette.SAD;

[System.Serializable]
public class LocalizedMessage
{
    public string key; // Уникальный ключ, например "hint_door_locked"
    public string russian;
    public string english;
    public string japanese;
    public string ukrainian;
    public string german;
    public string french;
    public string spanish;
    public string turkish;
    public string italian;
    public string polish;

    // Получить перевод по текущему языку
    public string GetText()
    {
        if (GlobalSettingsManager.Instance == null) return english;

        return GlobalSettingsManager.Instance.CurrentLanguage switch
        {
            GameLanguage.Russian => russian,
            GameLanguage.English => english,
            GameLanguage.Japanese => japanese,
            GameLanguage.Ukrainian => ukrainian,
            GameLanguage.German => german,
            GameLanguage.French => french,
            GameLanguage.Spanish => spanish,
            GameLanguage.Turkish => turkish,
            GameLanguage.Italian => italian,
            GameLanguage.Polish => polish,
            _ => english
        };
    }
}

public class HintMessageManager : MonoBehaviour
{
    public static HintMessageManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private Canvas messageCanvas;
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("Animation")]
    [SerializeField] private float defaultMessageDuration = 5f;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    [Header("Localization")]
    [SerializeField] private List<LocalizedMessage> localizedMessages = new List<LocalizedMessage>();

    // Словарь для быстрого поиска по ключу
    private Dictionary<string, LocalizedMessage> messageDict = new Dictionary<string, LocalizedMessage>();

    private Coroutine currentFadeCoroutine = null;
    private Color originalColor;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Инициализация словаря
        foreach (var msg in localizedMessages)
        {
            if (!string.IsNullOrEmpty(msg.key) && !messageDict.ContainsKey(msg.key))
            {
                messageDict[msg.key] = msg;
            }
        }

        if (messageText != null)
        {
            originalColor = messageText.color;
            messageText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        }

        if (messageCanvas != null)
            messageCanvas.gameObject.SetActive(false);

        // Подписываемся на смену языка
        if (GlobalSettingsManager.Instance != null)
        {
            GlobalSettingsManager.Instance.OnLanguageChanged.AddListener(OnLanguageChanged);
        }
    }

    private void OnDestroy()
    {
        if (GlobalSettingsManager.Instance != null)
        {
            GlobalSettingsManager.Instance.OnLanguageChanged.RemoveListener(OnLanguageChanged);
        }
    }

    private void OnLanguageChanged()
    {
        // Если текущее сообщение видно — обновляем его текст
        if (currentFadeCoroutine != null && messageText != null && messageText.gameObject.activeSelf)
        {
            string currentKey = GetCurrentMessageKey();
            if (!string.IsNullOrEmpty(currentKey))
            {
                messageText.text = GetLocalizedText(currentKey);
            }
        }
    }

    // Временное хранение текущего ключа (для обновления при смене языка)
    private string currentMessageKey = null;
    private string GetCurrentMessageKey() => currentMessageKey;

    /// <summary>
    /// Показать локализованное сообщение по ключу
    /// </summary>
    public void ShowMessage(string key, float duration = -1f)
    {
        if (duration < 0f) duration = defaultMessageDuration;

        string text = GetLocalizedText(key);
        if (string.IsNullOrEmpty(text))
        {
            Debug.LogWarning($"Локализация не найдена для ключа: {key}");
            return;
        }

        currentMessageKey = key;

        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }

        messageText.text = text;
        currentFadeCoroutine = StartCoroutine(FadeInAndOut(duration));
    }

    /// <summary>
    /// Показать сырой текст (без локализации) — только для отладки
    /// </summary>
    public void ShowRawMessage(string text, float duration = -1f)
    {
        if (duration < 0f) duration = defaultMessageDuration;

        currentMessageKey = null;

        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }

        messageText.text = text;
        currentFadeCoroutine = StartCoroutine(FadeInAndOut(duration));
    }

    private string GetLocalizedText(string key)
    {
        if (messageDict.TryGetValue(key, out LocalizedMessage msg))
        {
            return msg.GetText();
        }
        return key; // fallback
    }

    private IEnumerator FadeInAndOut(float duration)
    {
        if (messageCanvas != null)
            messageCanvas.gameObject.SetActive(true);

        if (messageText != null)
            messageText.gameObject.SetActive(true);

        Color color = originalColor;

        // Мгновенно скрыть
        color.a = 0f;
        messageText.color = color;

        // Fade In
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(0f, originalColor.a, elapsed / fadeInDuration);
            messageText.color = color;
            yield return null;
        }

        color.a = originalColor.a;
        messageText.color = color;

        // Hold
        yield return new WaitForSeconds(duration);

        // Fade Out
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(originalColor.a, 0f, elapsed / fadeOutDuration);
            messageText.color = color;
            yield return null;
        }

        color.a = 0f;
        messageText.color = color;

        // Deactivate
        if (messageText != null)
            messageText.gameObject.SetActive(false);
        if (messageCanvas != null)
            messageCanvas.gameObject.SetActive(false);

        currentFadeCoroutine = null;
        currentMessageKey = null;
    }

    // === ОТЛАДКА ===
    [ContextMenu("Test: Door Locked")]
    private void TestDoorLocked()
    {
        ShowMessage("hint_door_locked", 4f);
    }

    [ContextMenu("Test: Key Found")]
    private void TestKeyFound()
    {
        ShowMessage("hint_key_found", 3f);
    }
}
