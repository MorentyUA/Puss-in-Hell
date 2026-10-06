using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using Cinemachine;
using PussInHell.Core;

namespace PussInHell.Cinematics
{
    public class CameraDollyShot : MonoBehaviour
    {
        [Header("Camera")]
        [Tooltip("Virtual camera without Follow/LookAt; this script drives its transform")]
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [SerializeField] private int activePriority = 100;

        [Header("Path")]
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;
        [SerializeField] private float duration = 5f;
        [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Transitions")]
        [Tooltip("Hard cut to the start pose instead of the Brain blend")]
        [SerializeField] private bool cutOnStart = true;
        [Tooltip("Hard cut back to the gameplay camera instead of the Brain blend")]
        [SerializeField] private bool cutOnEnd = true;

        [Header("Before Start")]
        [Tooltip("Timeline to stop first if it is still playing; its owner finishes its own cutscene")]
        [SerializeField] private PlayableDirector directorToStop;

        [Header("Skip")]
        [SerializeField] private bool allowSkip = false;
        [SerializeField] private KeyCode skipKey = KeyCode.Escape;

        [Header("Events")]
        public UnityEvent onStarted;
        public UnityEvent onFinished;

        private CinemachineBrain brain;
        private CinemachineBlendDefinition originalBlend;
        private bool blendCaptured;
        private Coroutine shotRoutine;

        public bool IsPlaying { get; private set; }

        private void Start()
        {
            if (virtualCamera != null) virtualCamera.Priority = 0;
            CaptureBrain();
        }

        private void Update()
        {
            if (IsPlaying && allowSkip && Input.GetKeyDown(skipKey)) Skip();
        }

        private void OnDestroy()
        {
            if (IsPlaying && brain != null && blendCaptured) brain.m_DefaultBlend = originalBlend;
        }

        private void CaptureBrain()
        {
            if (brain == null && Camera.main != null) brain = Camera.main.GetComponent<CinemachineBrain>();
            if (brain != null && !blendCaptured)
            {
                originalBlend = brain.m_DefaultBlend;
                blendCaptured = true;
            }
        }

        public void Play()
        {
            if (IsPlaying) return;
            if (virtualCamera == null || startPoint == null || endPoint == null) return;

            if (directorToStop != null && directorToStop.state == PlayState.Playing) directorToStop.Stop();

            CaptureBrain();
            IsPlaying = true;
            CutsceneState.IsPlaying = true;
            PlayerControlLock.Lock();

            shotRoutine = StartCoroutine(ShotRoutine());
        }

        public void Skip()
        {
            if (!IsPlaying) return;

            if (shotRoutine != null)
            {
                StopCoroutine(shotRoutine);
                shotRoutine = null;
            }

            CutsceneState.BlockMenuForOneFrame();
            Finish();
        }

        private IEnumerator ShotRoutine()
        {
            if (cutOnStart && brain != null)
                brain.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.Cut, 0f);

            virtualCamera.transform.SetPositionAndRotation(startPoint.position, startPoint.rotation);
            virtualCamera.Priority = activePriority;
            onStarted?.Invoke();

            yield return null;
            yield return null;
            if (cutOnStart && brain != null && blendCaptured) brain.m_DefaultBlend = originalBlend;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = moveCurve.Evaluate(Mathf.Clamp01(elapsed / duration));
                virtualCamera.transform.SetPositionAndRotation(
                    Vector3.LerpUnclamped(startPoint.position, endPoint.position, t),
                    Quaternion.SlerpUnclamped(startPoint.rotation, endPoint.rotation, t));
                yield return null;
            }

            virtualCamera.transform.SetPositionAndRotation(endPoint.position, endPoint.rotation);
            shotRoutine = null;
            Finish();
        }

        private void Finish()
        {
            if (cutOnEnd && brain != null)
            {
                brain.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.Cut, 0f);
                StartCoroutine(RestoreBlend());
            }

            virtualCamera.Priority = 0;
            IsPlaying = false;

            PlayerControlLock.Unlock();
            CutsceneState.IsPlaying = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            onFinished?.Invoke();
        }

        private IEnumerator RestoreBlend()
        {
            yield return null;
            yield return null;
            if (brain != null && blendCaptured) brain.m_DefaultBlend = originalBlend;
        }

        private void OnDrawGizmos()
        {
            if (startPoint == null || endPoint == null) return;
            Gizmos.color = IsPlaying ? Color.green : Color.cyan;
            Gizmos.DrawLine(startPoint.position, endPoint.position);
            Gizmos.DrawWireSphere(startPoint.position, 0.25f);
            Gizmos.DrawWireSphere(endPoint.position, 0.25f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(startPoint.position, startPoint.forward * 2f);
            Gizmos.DrawRay(endPoint.position, endPoint.forward * 2f);
        }
    }
}
