using UnityEngine;
using UnityEngine.UI;

public class FadeOutImage : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private float duration = 5f;

    void Start()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        if (targetImage != null)
            StartCoroutine(FadeOut());
    }

    private System.Collections.IEnumerator FadeOut()
    {
        Color startColor = targetImage.color;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, time / duration);

            startColor.a = alpha;
            targetImage.color = startColor;

            yield return null;
        }

        // гарантируем, что alpha стал 0
        startColor.a = 0f;
        targetImage.color = startColor;
    }
}
