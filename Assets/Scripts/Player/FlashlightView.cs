using System.Collections;
using UnityEngine;

namespace PussInHell.Player
{
    public class FlashlightView : MonoBehaviour
    {
        [SerializeField] private Flashlight flashlight;

        [Header("Objects")]
        [SerializeField] private GameObject flashlightObject;
        [SerializeField] private GameObject lightObject;
        [SerializeField] private Animator animator;
        [SerializeField] private string onTrigger = "flashlightOn";
        [SerializeField] private string offTrigger = "flashlightOff";

        [Header("Timing")]
        [Tooltip("Delay between the raise animation and the light switching on")]
        [SerializeField] private float lightObjectDelay = 1f;
        [Tooltip("Delay between the lower animation and hiding the flashlight")]
        [SerializeField] private float turnOffDelay = 0.5f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip lightOnSound;
        [SerializeField] private AudioClip lightOffSound;

        private Coroutine lightOnRoutine;
        private Coroutine hideRoutine;

        private void Awake()
        {
            if (flashlight == null) flashlight = GetComponentInParent<Flashlight>();
            if (animator == null && flashlightObject != null) animator = flashlightObject.GetComponent<Animator>();
            if (flashlightObject != null) flashlightObject.SetActive(false);
            if (lightObject != null) lightObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (flashlight == null) return;
            flashlight.TurnedOn += OnTurnedOn;
            flashlight.TurnedOff += OnTurnedOff;
        }

        private void OnDisable()
        {
            if (flashlight == null) return;
            flashlight.TurnedOn -= OnTurnedOn;
            flashlight.TurnedOff -= OnTurnedOff;
        }

        private void OnTurnedOn()
        {
            if (hideRoutine != null) { StopCoroutine(hideRoutine); hideRoutine = null; }
            if (flashlightObject != null) flashlightObject.SetActive(true);
            if (animator != null) animator.SetTrigger(onTrigger);
            lightOnRoutine = StartCoroutine(LightOnAfterDelay());
        }

        private void OnTurnedOff()
        {
            if (lightOnRoutine != null) { StopCoroutine(lightOnRoutine); lightOnRoutine = null; }

            if (lightObject != null)
            {
                lightObject.SetActive(false);
                PlaySound(lightOffSound);
            }

            if (animator != null) animator.SetTrigger(offTrigger);
            hideRoutine = StartCoroutine(HideAfterDelay());
        }

        private IEnumerator LightOnAfterDelay()
        {
            yield return new WaitForSeconds(lightObjectDelay);
            lightOnRoutine = null;
            if (lightObject == null || !flashlight.IsOn) yield break;

            lightObject.SetActive(true);
            PlaySound(lightOnSound);
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(turnOffDelay);
            hideRoutine = null;
            if (flashlightObject != null) flashlightObject.SetActive(false);
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
        }
    }
}
