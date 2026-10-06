using UnityEngine;
using TMPro;
using PussInHell.Core;

namespace PussInHell.Interaction
{
    public class HintSpriteView : MonoBehaviour
    {
        [SerializeField] private ProximityHint hint;
        [SerializeField] private SpriteRenderer hintSprite;
        [SerializeField] private TextMeshPro hintText;

        [Header("Look")]
        [Range(0f, 1f)]
        [SerializeField] private float visibleAlpha = 0.5f;
        [SerializeField] private float fadeSpeed = 3f;
        [SerializeField] private float rotationSpeed = 5f;

        [Header("Pulse")]
        [SerializeField] private float pulseScale = 1.2f;
        [SerializeField] private float pulseSpeed = 3f;
        [SerializeField] private float pulseAmount = 0.02f;
        [SerializeField] private float scaleLerpSpeed = 4f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip interactSound;

        private float currentAlpha;
        private float textAlpha;
        private Vector3 originalScale;

        private void Awake()
        {
            if (hint == null) hint = GetComponentInParent<ProximityHint>();
            if (audioSource == null) audioSource = GetComponent<AudioSource>();

            if (hintSprite != null)
            {
                originalScale = hintSprite.transform.localScale;
                SetAlpha(0f);
            }
            if (hintText != null) textAlpha = hintText.color.a;
        }

        private void OnEnable()
        {
            if (hint != null) hint.Interacted += OnInteracted;
        }

        private void OnDisable()
        {
            if (hint != null) hint.Interacted -= OnInteracted;
        }

        private void Update()
        {
            if (hint == null || hintSprite == null || hint.HasInteracted) return;

            float targetAlpha = hint.IsVisible ? visibleAlpha : 0f;
            currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);
            SetAlpha(currentAlpha);

            if (currentAlpha > 0.01f) FacePlayer();

            var spriteTransform = hintSprite.transform;
            if (hint.IsNear && currentAlpha > 0.1f)
            {
                spriteTransform.localScale = Vector3.Lerp(spriteTransform.localScale, originalScale * pulseScale, Time.deltaTime * scaleLerpSpeed);
                spriteTransform.localScale *= 1f - Mathf.Abs(Mathf.Sin(Time.time * pulseSpeed)) * pulseAmount;
            }
            else
            {
                spriteTransform.localScale = Vector3.Lerp(spriteTransform.localScale, originalScale, Time.deltaTime * scaleLerpSpeed);
            }
        }

        private void FacePlayer()
        {
            var player = PlayerLocator.Player;
            if (player == null) return;

            Vector3 direction = player.position - hintSprite.transform.position;
            direction.y = 0f;
            if (direction == Vector3.zero) return;

            hintSprite.transform.rotation = Quaternion.Slerp(hintSprite.transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotationSpeed);
        }

        private void SetAlpha(float alpha)
        {
            var color = hintSprite.color;
            color.a = alpha;
            hintSprite.color = color;

            if (hintText == null) return;
            var textColor = hintText.color;
            textColor.a = alpha * textAlpha;
            hintText.color = textColor;
        }

        private void OnInteracted()
        {
            if (audioSource != null && interactSound != null) audioSource.PlayOneShot(interactSound);
            if (hintSprite != null) hintSprite.enabled = false;
            if (hintText != null) hintText.gameObject.SetActive(false);
        }
    }
}
