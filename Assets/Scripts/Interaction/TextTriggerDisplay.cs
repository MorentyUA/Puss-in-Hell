using System.Collections;
using UnityEngine;
using TMPro;
using PussInHell.Core;

namespace PussInHell.Interaction
{
    public class TextTriggerDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI tmpText;
        [Range(0f, 1f)]
        [SerializeField] private float visibleAlpha = 0.1f;
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float displayDuration = 5f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private string playerTag = PlayerLocator.Tag;
        [Tooltip("Show only on the first trigger enter")]
        [SerializeField] private bool showOnlyOnce = true;

        private Coroutine fadeRoutine;
        private bool hasShown;

        private void Start()
        {
            SetAlpha(0f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag) || tmpText == null) return;
            if (showOnlyOnce && hasShown) return;

            hasShown = true;
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(ShowSequence());
        }

        private IEnumerator ShowSequence()
        {
            yield return Fade(0f, visibleAlpha, fadeInDuration);
            yield return new WaitForSeconds(displayDuration);
            yield return Fade(visibleAlpha, 0f, fadeOutDuration);
            fadeRoutine = null;
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
            if (tmpText == null) return;
            var color = tmpText.color;
            color.a = alpha;
            tmpText.color = color;
        }
    }
}
