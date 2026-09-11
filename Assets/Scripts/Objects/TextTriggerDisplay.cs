using System.Collections;
using UnityEngine;
using TMPro;

public class TextTriggerDisplay : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private TextMeshProUGUI tmpText; // Ссылка на TextMeshPro UI

    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float displayDuration = 5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private string playerTag = "Player";

    private Coroutine fadeCoroutine;

    private void Start()
    {
        // Убеждаемся что текст скрыт в начале
        if (tmpText != null)
        {
            SetTextAlpha(0f);
        }
        else
        {
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        // Проверяем что это игрок
        if (other.CompareTag(playerTag))
        {

            if (tmpText == null)
            {
                return;
            }

            // Останавливаем предыдущую анимацию если есть
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            // Запускаем новую анимацию
            fadeCoroutine = StartCoroutine(ShowTextSequence());
        }
    }

    private IEnumerator ShowTextSequence()
    {

        // Плавное появление
        yield return StartCoroutine(FadeText(0f, 0.1f, fadeInDuration));

        // Показываем текст
        yield return new WaitForSeconds(displayDuration);

        // Плавное исчезновение
        yield return StartCoroutine(FadeText(0.1f, 0f, fadeOutDuration));

    }

    private IEnumerator FadeText(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Используем плавную интерполяцию
            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, t);

            SetTextAlpha(currentAlpha);

            yield return null;
        }

        // Устанавливаем финальное значение
        SetTextAlpha(endAlpha);
    }

    private void SetTextAlpha(float alpha)
    {
        if (tmpText != null)
        {
            Color color = tmpText.color;
            color.a = alpha;
            tmpText.color = color;
        }
    }
}
