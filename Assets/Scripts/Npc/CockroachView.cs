using UnityEngine;

namespace PussInHell.Npc
{
    public class CockroachView : MonoBehaviour
    {
        [SerializeField] private CockroachWander wander;
        [SerializeField] private Animator animator;
        [SerializeField] private string moveParameter = "move";

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip[] scuttleClips;

        private void Awake()
        {
            if (wander == null) wander = GetComponentInParent<CockroachWander>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        private void OnEnable()
        {
            if (wander != null) wander.MovingChanged += OnMovingChanged;
        }

        private void OnDisable()
        {
            if (wander != null) wander.MovingChanged -= OnMovingChanged;
        }

        private void Update()
        {
            if (wander != null && wander.IsMoving) PlayScuttle();
        }

        private void OnMovingChanged(bool moving)
        {
            if (animator != null) animator.SetBool(moveParameter, moving);
        }

        private void PlayScuttle()
        {
            if (audioSource == null || scuttleClips == null || scuttleClips.Length == 0 || audioSource.isPlaying) return;
            var clip = scuttleClips[Random.Range(0, scuttleClips.Length)];
            if (clip != null) audioSource.PlayOneShot(clip);
        }
    }
}
