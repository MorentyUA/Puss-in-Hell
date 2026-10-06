using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using Cinemachine;
using PussInHell.Core;

namespace PussInHell.Cinematics
{
    public class CutscenePlayer : MonoBehaviour
    {
        [Header("Playback")]
        [SerializeField] private PlayableDirector timeline;
        [SerializeField] private CinemachineVirtualCamera cutsceneCamera;
        [SerializeField] private int cameraPriority = 100;
        [Tooltip("Switch to the cutscene camera with a hard cut instead of the Brain blend")]
        [SerializeField] private bool cutToCamera = false;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private bool stopAudioOnEnd = true;

        [Header("Skip")]
        [SerializeField] private bool allowSkip = true;
        [SerializeField] private KeyCode skipKey = KeyCode.Escape;

        [Header("Events")]
        public UnityEvent onCutsceneStarted;
        public UnityEvent onCutsceneEnded;

        private CinemachineBrain brain;
        private CinemachineBlendDefinition originalBlend;

        public bool IsPlaying { get; private set; }

        private void Start()
        {
            if (timeline != null) timeline.stopped += OnTimelineStopped;
            if (cutsceneCamera != null) cutsceneCamera.Priority = 0;

            if (Camera.main != null) brain = Camera.main.GetComponent<CinemachineBrain>();
            if (brain != null) originalBlend = brain.m_DefaultBlend;
        }

        private void OnDestroy()
        {
            if (timeline != null) timeline.stopped -= OnTimelineStopped;
            if (IsPlaying && brain != null) brain.m_DefaultBlend = originalBlend;
        }

        private void Update()
        {
            if (IsPlaying && allowSkip && Input.GetKeyDown(skipKey)) Skip();
        }

        public void Play()
        {
            if (IsPlaying) return;
            IsPlaying = true;

            CutsceneState.IsPlaying = true;
            PlayerControlLock.Lock();
            SwitchToCutsceneCamera();

            if (audioSource != null) audioSource.Play();
            if (timeline != null) timeline.Play();

            onCutsceneStarted?.Invoke();

            if (timeline == null) End();
        }

        public void Skip()
        {
            if (!IsPlaying) return;
            CutsceneState.BlockMenuForOneFrame();

            if (timeline != null) timeline.Stop();
            else End();
        }

        public void Stop()
        {
            Skip();
        }

        private void OnTimelineStopped(PlayableDirector director)
        {
            if (director == timeline && IsPlaying) End();
        }

        private void End()
        {
            IsPlaying = false;

            RestorePreviousCamera();
            if (stopAudioOnEnd && audioSource != null) audioSource.Stop();

            PlayerControlLock.Unlock();
            CutsceneState.IsPlaying = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            onCutsceneEnded?.Invoke();
        }

        private void SwitchToCutsceneCamera()
        {
            if (cutsceneCamera == null) return;

            if (cutToCamera && brain != null)
                brain.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.Cut, 0f);

            cutsceneCamera.Priority = cameraPriority;
        }

        private void RestorePreviousCamera()
        {
            if (cutsceneCamera == null) return;

            cutsceneCamera.Priority = 0;
            if (cutToCamera && brain != null) StartCoroutine(RestoreBlend());
        }

        private IEnumerator RestoreBlend()
        {
            yield return null;
            yield return null;
            if (brain != null) brain.m_DefaultBlend = originalBlend;
        }
    }
}
