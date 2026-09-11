using UnityEngine;

public class EnableAudioAfterDelay : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float delay = 2f;
    [SerializeField] private float fadeTime = 0.5f;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
            audioSource.volume = 0f;   // ← Изначально громкость = 0
    }

    private void Start()
    {
        StartCoroutine(FadeInAudio());
    }

    private System.Collections.IEnumerator FadeInAudio()
    {
        if (audioSource == null)
            yield break;

        yield return new WaitForSeconds(delay);

        float t = 0;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, 1f, t / fadeTime);
            yield return null;
        }

        audioSource.volume = 1f; // Гарантируем финальное значение
    }
}
