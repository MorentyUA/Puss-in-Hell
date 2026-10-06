using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace PussInHell.Cinematics
{
    public class SinkBloodFill : MonoBehaviour
    {
        [Header("Surface")]
        [Tooltip("Flat blood mesh inside the basin; hidden until the fill starts")]
        [SerializeField] private Transform bloodSurface;

        [Header("Empty (local)")]
        [SerializeField] private Vector3 emptyLocalPosition;
        [SerializeField] private Vector3 emptyLocalScale = new Vector3(0.1f, 0.0005f, 0.2f);

        [Header("Full (local)")]
        [SerializeField] private Vector3 fullLocalPosition;
        [SerializeField] private Vector3 fullLocalScale = new Vector3(0.35f, 0.0005f, 0.55f);

        [Header("Timing")]
        [SerializeField] private float startDelay = 1.5f;
        [SerializeField] private float fillDuration = 6f;
        [SerializeField] private AnimationCurve fillCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip fillSound;

        [Header("Events")]
        public UnityEvent onFillStarted;
        public UnityEvent onFillCompleted;

        private Coroutine fillRoutine;

        public bool IsFilled { get; private set; }

        private void Awake()
        {
            ApplyState(emptyLocalPosition, emptyLocalScale, false);
        }

        public void StartFill()
        {
            if (IsFilled || fillRoutine != null || bloodSurface == null) return;
            fillRoutine = StartCoroutine(FillRoutine());
        }

        public void FillInstantly()
        {
            StopRoutine();
            ApplyState(fullLocalPosition, fullLocalScale, true);
            IsFilled = true;
        }

        public void ResetFill()
        {
            StopRoutine();
            IsFilled = false;
            ApplyState(emptyLocalPosition, emptyLocalScale, false);
        }

        private IEnumerator FillRoutine()
        {
            if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

            ApplyState(emptyLocalPosition, emptyLocalScale, true);
            if (audioSource != null && fillSound != null) audioSource.PlayOneShot(fillSound);
            onFillStarted?.Invoke();

            float elapsed = 0f;
            while (elapsed < fillDuration)
            {
                elapsed += Time.deltaTime;
                float t = fillCurve.Evaluate(Mathf.Clamp01(elapsed / fillDuration));
                bloodSurface.localPosition = Vector3.LerpUnclamped(emptyLocalPosition, fullLocalPosition, t);
                bloodSurface.localScale = Vector3.LerpUnclamped(emptyLocalScale, fullLocalScale, t);
                yield return null;
            }

            ApplyState(fullLocalPosition, fullLocalScale, true);
            IsFilled = true;
            fillRoutine = null;
            onFillCompleted?.Invoke();
        }

        private void StopRoutine()
        {
            if (fillRoutine == null) return;
            StopCoroutine(fillRoutine);
            fillRoutine = null;
        }

        private void ApplyState(Vector3 localPosition, Vector3 localScale, bool visible)
        {
            if (bloodSurface == null) return;
            bloodSurface.localPosition = localPosition;
            bloodSurface.localScale = localScale;
            bloodSurface.gameObject.SetActive(visible);
        }

#if UNITY_EDITOR
        [ContextMenu("Preview: Empty")]
        private void PreviewEmpty() => ApplyState(emptyLocalPosition, emptyLocalScale, true);

        [ContextMenu("Preview: Full")]
        private void PreviewFull() => ApplyState(fullLocalPosition, fullLocalScale, true);

        [ContextMenu("Capture current as Empty")]
        private void CaptureEmpty()
        {
            if (bloodSurface == null) return;
            emptyLocalPosition = bloodSurface.localPosition;
            emptyLocalScale = bloodSurface.localScale;
        }

        [ContextMenu("Capture current as Full")]
        private void CaptureFull()
        {
            if (bloodSurface == null) return;
            fullLocalPosition = bloodSurface.localPosition;
            fullLocalScale = bloodSurface.localScale;
        }
#endif
    }
}
