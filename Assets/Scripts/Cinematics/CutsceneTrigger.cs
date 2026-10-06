using UnityEngine;
using PussInHell.Core;

namespace PussInHell.Cinematics
{
    [RequireComponent(typeof(CutscenePlayer))]
    public class CutsceneTrigger : MonoBehaviour
    {
        [SerializeField] private CutscenePlayer player;
        [SerializeField] private bool triggerOnce = true;
        [SerializeField] private string playerTag = PlayerLocator.Tag;

        public bool HasTriggered { get; private set; }

        private void Awake()
        {
            if (player == null) player = GetComponent<CutscenePlayer>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;
            if (triggerOnce && HasTriggered) return;
            if (player == null || player.IsPlaying) return;

            HasTriggered = true;
            player.Play();
        }

        public void ResetTrigger()
        {
            HasTriggered = false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = HasTriggered ? Color.green : Color.yellow;
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }
    }
}
