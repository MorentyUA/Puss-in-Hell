using System.Collections;
using UnityEngine;

namespace PussInHell.Audio
{
    public class AudioFadeIn : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private float delay = 2f;
        [SerializeField] private float fadeTime = 0.5f;
        [Range(0f, 1f)]
        [SerializeField] private float targetVolume = 1f;

        private void Awake()
        {
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            if (audioSource != null) audioSource.volume = 0f;
        }

        private void Start()
        {
            if (audioSource != null) StartCoroutine(FadeIn());
        }

        private IEnumerator FadeIn()
        {
            yield return new WaitForSeconds(delay);

            float elapsed = 0f;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / fadeTime);
                yield return null;
            }
            audioSource.volume = targetVolume;
        }
    }
}
