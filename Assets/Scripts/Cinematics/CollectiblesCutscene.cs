using System.Collections.Generic;
using UnityEngine;
using PussInHell.Interaction;

namespace PussInHell.Cinematics
{
    [RequireComponent(typeof(CutscenePlayer))]
    public class CollectiblesCutscene : MonoBehaviour
    {
        [SerializeField] private CutscenePlayer player;
        [Tooltip("The cutscene starts once every listed interactable has been activated")]
        [UnityEngine.Serialization.FormerlySerializedAs("requiredHighlighters")]
        [SerializeField] private List<Interactable> requiredInteractables = new List<Interactable>();
        [SerializeField] private bool triggerOnce = true;

        private readonly HashSet<Interactable> activated = new HashSet<Interactable>();

        public bool HasTriggered { get; private set; }
        public int ActivatedCount => activated.Count;
        public int RequiredCount => requiredInteractables.Count;

        private void Awake()
        {
            if (player == null) player = GetComponent<CutscenePlayer>();
        }

        private void OnEnable()
        {
            Interactable.AnyActivated += OnInteractableActivated;
        }

        private void OnDisable()
        {
            Interactable.AnyActivated -= OnInteractableActivated;
        }

        private void Start()
        {
            foreach (var interactable in requiredInteractables)
                if (interactable != null && interactable.IsActivated) activated.Add(interactable);
            CheckAllActivated();
        }

        private void OnInteractableActivated(Interactable interactable)
        {
            if (!requiredInteractables.Contains(interactable)) return;
            activated.Add(interactable);
            CheckAllActivated();
        }

        private void CheckAllActivated()
        {
            if (triggerOnce && HasTriggered) return;
            if (player == null || player.IsPlaying) return;

            foreach (var interactable in requiredInteractables)
                if (interactable != null && !activated.Contains(interactable)) return;

            HasTriggered = true;
            player.Play();
        }

        public void ResetProgress()
        {
            activated.Clear();
            HasTriggered = false;
            foreach (var interactable in requiredInteractables)
                if (interactable != null) interactable.ResetActivation();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            foreach (var interactable in requiredInteractables)
            {
                if (interactable == null) continue;
                Gizmos.color = interactable.IsActivated ? Color.green : Color.yellow;
                Gizmos.DrawLine(transform.position, interactable.transform.position);
                Gizmos.DrawWireSphere(interactable.transform.position, 0.3f);
            }
        }
#endif
    }
}
