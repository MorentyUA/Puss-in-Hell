using UnityEngine;
using TMPro;
using System.Collections;

public class InteractiveHint : MonoBehaviour
{
    [Header("Настройки радиуса")]
    [SerializeField] private float interactionRadius = 3f;
    [SerializeField] private float disappearDistance = 1f;
    [SerializeField] private float pulseDistance = 2f;
    [SerializeField] private float pulseScale = 1.2f;
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private float pulseAmount = 0.02f;

    [Header("Ссылки")]
    [SerializeField] private SpriteRenderer hintSprite;
    [SerializeField] private Transform player;
    [SerializeField] private TextMeshPro hintText;

    [Header("Настройки анимации")]
    [SerializeField] private float fadeSpeed = 3f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Одноразовый интеракт")]
    [SerializeField] private bool isOneTimeInteract = false;
    [SerializeField] private string messageKey = "hint_default"; // КЛЮЧ ЛОКАЛИЗАЦИИ
    [SerializeField] private float messageDuration = 5f;

    [Header("Звук")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip interactSound;

    private float targetAlpha = 0f;
    private float currentAlpha = 0f;
    private Color originalTextColor;
    private Vector3 originalScale;
    private bool enteredPulseZone = false;
    private bool hasInteracted = false;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        if (hintSprite != null)
        {
            Color color = hintSprite.color;
            color.a = 0f;
            hintSprite.color = color;
            currentAlpha = 0f;
            originalScale = hintSprite.transform.localScale;
        }

        if (hintText != null)
        {
            originalTextColor = hintText.color;
            Color textColor = hintText.color;
            textColor.a = 0f;
            hintText.color = textColor;
        }

        if (isOneTimeInteract && audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (hasInteracted || player == null || hintSprite == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // ИСПРАВЛЕНО: проверка на нажатие E теперь работает во всём радиусе взаимодействия
        if (isOneTimeInteract && distance <= pulseDistance && Input.GetKeyDown(KeyCode.E))
        {
            ActivateOneTimeInteract();
            return;
        }

        targetAlpha = distance < disappearDistance ? 0f :
                      distance <= interactionRadius ? 0.5f : 0f;

        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);

        Color spriteColor = hintSprite.color;
        spriteColor.a = currentAlpha;
        hintSprite.color = spriteColor;

        if (hintText != null)
        {
            Color textColor = hintText.color;
            textColor.a = currentAlpha * originalTextColor.a;
            hintText.color = textColor;
        }

        if (currentAlpha > 0.01f)
        {
            Vector3 dir = player.position - hintSprite.transform.position;
            dir.y = 0;
            if (dir != Vector3.zero)
            {
                Quaternion rot = Quaternion.LookRotation(dir);
                hintSprite.transform.rotation = Quaternion.Slerp(hintSprite.transform.rotation, rot, Time.deltaTime * rotationSpeed);
            }
        }

        enteredPulseZone = distance <= pulseDistance && distance > disappearDistance;

        if (enteredPulseZone && currentAlpha > 0.1f)
        {
            Vector3 targetScale = originalScale * pulseScale;
            hintSprite.transform.localScale = Vector3.Lerp(hintSprite.transform.localScale, targetScale, Time.deltaTime * 4f);
            float pulse = 1f - Mathf.Abs(Mathf.Sin(Time.time * pulseSpeed)) * pulseAmount;
            hintSprite.transform.localScale *= pulse;
        }
        else
        {
            hintSprite.transform.localScale = Vector3.Lerp(hintSprite.transform.localScale, originalScale, Time.deltaTime * 4f);
        }
    }

    void ActivateOneTimeInteract()
    {
        hasInteracted = true;


        if (audioSource != null && interactSound != null)
            audioSource.PlayOneShot(interactSound);

        if (hintSprite != null) hintSprite.enabled = false;
        if (hintText != null) hintText.gameObject.SetActive(false);

        if (HintMessageManager.Instance == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(messageKey))
        {
            return;
        }

        HintMessageManager.Instance.ShowMessage(messageKey, messageDuration);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, interactionRadius);
        Gizmos.color = Color.red;    Gizmos.DrawWireSphere(transform.position, disappearDistance);
        Gizmos.color = Color.green;  Gizmos.DrawWireSphere(transform.position, pulseDistance);
    }
}
