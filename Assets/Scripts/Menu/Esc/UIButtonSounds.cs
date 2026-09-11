using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIButtonSounds : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Audio Source (куда проигрывать звуки)")]
    public AudioSource audioSource;

    [Header("Звук наведения")]
    public AudioClip hoverSound;

    [Header("Звук клика")]
    public AudioClip clickSound;

    // Наведение мыши
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (audioSource != null && hoverSound != null)
            audioSource.PlayOneShot(hoverSound);
    }

    // Клик
    public void OnPointerClick(PointerEventData eventData)
    {
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }
}
