using UnityEngine;
using UnityEngine.EventSystems;

namespace PussInHell.UI
{
    public class UIButtonSounds : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip clickSound;

        public void OnPointerEnter(PointerEventData eventData) => Play(hoverSound);

        public void OnPointerClick(PointerEventData eventData) => Play(clickSound);

        private void Play(AudioClip clip)
        {
            if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
        }
    }
}
