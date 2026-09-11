using UnityEngine;
using System.Collections;

public class FlashlightToggle : MonoBehaviour
{
    [Header("Availability")]
    [Tooltip("Доступен ли фонарик игроку")]
    public bool isAvailable = true;

    [Header("Flashlight Settings")]
    [SerializeField] private GameObject flashlightObject;
    [SerializeField] private GameObject lightObject;
    [SerializeField] private Animator animator;
    [SerializeField] private float lightObjectDelay = 1f;
    [SerializeField] private float turnOffDelay = 0.5f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip lightOnSound;
    [SerializeField] private AudioClip lightOffSound;

    private bool isFlashlightOn = false;
    private bool isAnimating = false;
    private Coroutine lightObjectCoroutine;

    void Start()
    {
        if (animator == null && flashlightObject != null)
        {
            animator = flashlightObject.GetComponent<Animator>();
        }

        if (flashlightObject != null)
        {
            flashlightObject.SetActive(false);
        }

        if (lightObject != null)
        {
            lightObject.SetActive(false);
        }
    }

    void Update()
    {
        // Если фонарик НЕ доступен - выходим сразу
        if (!isAvailable)
        {
            return;
        }

        // Проверяем нажатие F только если доступен и не анимируется
        if (Input.GetKeyDown(KeyCode.F) && !isAnimating)
        {
            ToggleFlashlight();
        }
    }

    void ToggleFlashlight()
    {
        isFlashlightOn = !isFlashlightOn;

        if (isFlashlightOn)
        {
            TurnOnFlashlight();
        }
        else
        {
            TurnOffFlashlight();
        }
    }

    void TurnOnFlashlight()
    {
        if (flashlightObject != null)
        {
            flashlightObject.SetActive(true);
        }

        if (animator != null)
        {
            animator.SetTrigger("flashlightOn");
        }

        lightObjectCoroutine = StartCoroutine(TurnOnLightObjectAfterDelay());
    }

    void TurnOffFlashlight()
    {
        if (lightObjectCoroutine != null)
        {
            StopCoroutine(lightObjectCoroutine);
        }

        if (lightObject != null)
        {
            lightObject.SetActive(false);

            if (audioSource != null && lightOffSound != null)
            {
                audioSource.PlayOneShot(lightOffSound);
            }
        }

        if (animator != null)
        {
            animator.SetTrigger("flashlightOff");
        }

        StartCoroutine(DeactivateAfterDelay());
    }

    IEnumerator TurnOnLightObjectAfterDelay()
    {
        yield return new WaitForSeconds(lightObjectDelay);

        if (lightObject != null && isFlashlightOn)
        {
            lightObject.SetActive(true);

            if (audioSource != null && lightOnSound != null)
            {
                audioSource.PlayOneShot(lightOnSound);
            }
        }
    }

    IEnumerator DeactivateAfterDelay()
    {
        isAnimating = true;
        yield return new WaitForSeconds(turnOffDelay);

        if (flashlightObject != null)
        {
            flashlightObject.SetActive(false);
        }

        isAnimating = false;
    }

    public void SetFlashlightAvailability(bool available)
    {
        isAvailable = available;

        if (!available && isFlashlightOn)
        {
            TurnOffFlashlight();
        }
    }
}
