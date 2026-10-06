using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace PussInHell.Core
{
    public class DelayedEvent : MonoBehaviour
    {
        [SerializeField] private float delay = 1f;
        [Tooltip("Ignore Play() while a previous delay is still running")]
        [SerializeField] private bool ignoreWhileWaiting = true;

        public UnityEvent onFired;

        private Coroutine routine;

        public bool IsWaiting => routine != null;

        public void Play()
        {
            if (routine != null)
            {
                if (ignoreWhileWaiting) return;
                StopCoroutine(routine);
            }
            routine = StartCoroutine(Routine());
        }

        public void Cancel()
        {
            if (routine == null) return;
            StopCoroutine(routine);
            routine = null;
        }

        private IEnumerator Routine()
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);
            routine = null;
            onFired?.Invoke();
        }
    }
}
