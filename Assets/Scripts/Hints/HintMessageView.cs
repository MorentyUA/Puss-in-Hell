using System.Collections;
using UnityEngine;
using TMPro;

namespace PussInHell.Hints
{
    public class HintMessageView : MonoBehaviour
    {
        [SerializeField] private HintMessageService service;
        [SerializeField] private Canvas messageCanvas;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;

        private Coroutine routine;
        private Color originalColor;

        private void Awake()
        {
            if (service == null) service = GetComponent<HintMessageService>();
            if (messageText != null)
            {
                originalColor = messageText.color;
                SetAlpha(0f);
            }
            if (messageCanvas != null) messageCanvas.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (service == null) return;
            service.MessageRequested += Show;
            service.MessageTextChanged += UpdateText;
        }

        private void OnDisable()
        {
            if (service == null) return;
            service.MessageRequested -= Show;
            service.MessageTextChanged -= UpdateText;
        }

        private void Show(string text, float duration)
        {
            if (messageText == null) return;
            if (routine != null) StopCoroutine(routine);
            messageText.text = text;
            routine = StartCoroutine(FadeInAndOut(duration));
        }

        private void UpdateText(string text)
        {
            if (routine != null && messageText != null) messageText.text = text;
        }

        private IEnumerator FadeInAndOut(float duration)
        {
            if (messageCanvas != null) messageCanvas.gameObject.SetActive(true);
            messageText.gameObject.SetActive(true);

            yield return Fade(0f, originalColor.a, fadeInDuration);
            yield return new WaitForSeconds(duration);
            yield return Fade(originalColor.a, 0f, fadeOutDuration);

            messageText.gameObject.SetActive(false);
            if (messageCanvas != null) messageCanvas.gameObject.SetActive(false);
            routine = null;
            if (service != null) service.ClearCurrent();
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                SetAlpha(Mathf.Lerp(from, to, elapsed / duration));
                yield return null;
            }
            SetAlpha(to);
        }

        private void SetAlpha(float alpha)
        {
            var color = originalColor;
            color.a = alpha;
            messageText.color = color;
        }
    }
}
