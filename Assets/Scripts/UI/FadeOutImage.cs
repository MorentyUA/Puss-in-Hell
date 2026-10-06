using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PussInHell.UI
{
    public class FadeOutImage : MonoBehaviour
    {
        [SerializeField] private Image targetImage;
        [SerializeField] private float duration = 5f;

        private void Start()
        {
            if (targetImage == null) targetImage = GetComponent<Image>();
            if (targetImage != null) StartCoroutine(FadeOut());
        }

        private IEnumerator FadeOut()
        {
            Color color = targetImage.color;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                color.a = Mathf.Lerp(1f, 0f, elapsed / duration);
                targetImage.color = color;
                yield return null;
            }
            color.a = 0f;
            targetImage.color = color;
        }
    }
}
