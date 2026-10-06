using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Через delay секунд после Play() мгновенно переносит target в точку destination
/// (резкий «рывок» монстра к окну и т.п.). Удобно вешать на событие CameraDollyShot.onStarted.
/// </summary>
public class DelayedTeleport : MonoBehaviour
{
    [Header("Кого и куда")]
    [SerializeField] private Transform target;
    [Tooltip("Точка назначения: позиция и, если включено ниже, поворот")]
    [SerializeField] private Transform destination;
    [SerializeField] private bool applyDestinationRotation = true;

    [Header("Тайминг")]
    [Tooltip("Секунд от Play() до рывка")]
    [SerializeField] private float delay = 4f;
    [Tooltip("Сработать только один раз за сцену")]
    [SerializeField] private bool once = true;

    [Header("Анимация (опционально)")]
    [SerializeField] private Animator animator;
    [Tooltip("Триггер аниматора в момент рывка. Пусто — не дёргать.")]
    [SerializeField] private string animatorTrigger = "";

    [Header("Звук (опционально)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip teleportSound;

    [Header("События")]
    public UnityEvent onTeleported;

    private Coroutine routine;
    private bool hasFired;

    public bool HasFired => hasFired;

    public void Play()
    {
        if (once && hasFired) return;
        if (routine != null) return;
        if (target == null || destination == null) return;

        routine = StartCoroutine(Routine());
    }

    public void Cancel()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    /// <summary>Рывок прямо сейчас, без задержки.</summary>
    public void TeleportNow()
    {
        Cancel();
        DoTeleport();
    }

    private IEnumerator Routine()
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        routine = null;
        DoTeleport();
    }

    private void DoTeleport()
    {
        if (target == null || destination == null) return;

        if (applyDestinationRotation)
            target.SetPositionAndRotation(destination.position, destination.rotation);
        else
            target.position = destination.position;

        var rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (animator != null && !string.IsNullOrEmpty(animatorTrigger))
            animator.SetTrigger(animatorTrigger);

        if (audioSource != null && teleportSound != null)
            audioSource.PlayOneShot(teleportSound);

        hasFired = true;
        onTeleported?.Invoke();
    }

    private void OnDrawGizmosSelected()
    {
        if (target == null || destination == null) return;

        Gizmos.color = hasFired ? Color.green : Color.magenta;
        Gizmos.DrawLine(target.position, destination.position);
        Gizmos.DrawWireSphere(destination.position, 0.3f);
        Gizmos.DrawRay(destination.position, destination.forward * 1.5f);
    }
}
