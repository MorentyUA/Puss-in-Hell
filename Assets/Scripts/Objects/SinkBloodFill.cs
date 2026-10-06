using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Заполняет раковину кровью: поднимает плоский диск (bloodSurface) со дна чаши до кромки,
/// одновременно расширяя его, потому что чаша расширяется кверху.
/// Запуск — StartFill() (например, из события onCutsceneStarted у HaloHighlighterCutsceneManager).
/// </summary>
public class SinkBloodFill : MonoBehaviour
{
    [Header("Поверхность крови")]
    [Tooltip("Плоский диск внутри чаши. Выключен до начала заливки.")]
    [SerializeField] private Transform bloodSurface;

    [Header("Пустая раковина (локальные координаты диска)")]
    [SerializeField] private Vector3 emptyLocalPosition;
    [SerializeField] private Vector3 emptyLocalScale = new Vector3(0.1f, 0.0005f, 0.2f);

    [Header("Полная раковина (локальные координаты диска)")]
    [SerializeField] private Vector3 fullLocalPosition;
    [SerializeField] private Vector3 fullLocalScale = new Vector3(0.35f, 0.0005f, 0.55f);

    [Header("Тайминг")]
    [Tooltip("Пауза после StartFill() перед тем, как кровь появится")]
    [SerializeField] private float startDelay = 1.5f;
    [SerializeField] private float fillDuration = 6f;
    [SerializeField] private AnimationCurve fillCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Звук (опционально)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fillSound;

    [Header("События")]
    public UnityEvent onFillStarted;
    public UnityEvent onFillCompleted;

    private Coroutine fillCoroutine;
    private bool isFilled;

    public bool IsFilled => isFilled;

    private void Awake()
    {
        if (bloodSurface != null)
        {
            bloodSurface.localPosition = emptyLocalPosition;
            bloodSurface.localScale = emptyLocalScale;
            bloodSurface.gameObject.SetActive(false);
        }
    }

    /// <summary>Начать заливку. Повторный вызов во время заливки игнорируется.</summary>
    public void StartFill()
    {
        if (isFilled || fillCoroutine != null || bloodSurface == null) return;
        fillCoroutine = StartCoroutine(FillRoutine());
    }

    /// <summary>Мгновенно заполнить (для чекпоинтов / отладки).</summary>
    public void FillInstantly()
    {
        if (fillCoroutine != null)
        {
            StopCoroutine(fillCoroutine);
            fillCoroutine = null;
        }

        if (bloodSurface == null) return;

        bloodSurface.gameObject.SetActive(true);
        bloodSurface.localPosition = fullLocalPosition;
        bloodSurface.localScale = fullLocalScale;
        isFilled = true;
    }

    /// <summary>Сбросить в пустое состояние.</summary>
    public void ResetFill()
    {
        if (fillCoroutine != null)
        {
            StopCoroutine(fillCoroutine);
            fillCoroutine = null;
        }

        isFilled = false;

        if (bloodSurface == null) return;

        bloodSurface.localPosition = emptyLocalPosition;
        bloodSurface.localScale = emptyLocalScale;
        bloodSurface.gameObject.SetActive(false);
    }

    private IEnumerator FillRoutine()
    {
        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        bloodSurface.localPosition = emptyLocalPosition;
        bloodSurface.localScale = emptyLocalScale;
        bloodSurface.gameObject.SetActive(true);

        if (audioSource != null && fillSound != null)
            audioSource.PlayOneShot(fillSound);

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

        bloodSurface.localPosition = fullLocalPosition;
        bloodSurface.localScale = fullLocalScale;
        isFilled = true;
        fillCoroutine = null;

        onFillCompleted?.Invoke();
    }

#if UNITY_EDITOR
    [ContextMenu("Preview: Empty")]
    private void PreviewEmpty()
    {
        if (bloodSurface == null) return;
        bloodSurface.gameObject.SetActive(true);
        bloodSurface.localPosition = emptyLocalPosition;
        bloodSurface.localScale = emptyLocalScale;
    }

    [ContextMenu("Preview: Full")]
    private void PreviewFull()
    {
        if (bloodSurface == null) return;
        bloodSurface.gameObject.SetActive(true);
        bloodSurface.localPosition = fullLocalPosition;
        bloodSurface.localScale = fullLocalScale;
    }

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
