using UnityEngine;

namespace PussInHell.Player
{
    [System.Serializable]
    public class FootstepLayer
    {
        [Tooltip("Ground layers that use this sound set")]
        public LayerMask layer;
        public AudioSource source;
        public AudioClip[] clips;

        [HideInInspector] public int currentIndex;
    }

    public class PlayerFootsteps : MonoBehaviour
    {
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private float probeDistance = 1f;
        [SerializeField] private FootstepLayer[] footstepLayers;

        private void Awake()
        {
            if (motor == null) motor = GetComponentInParent<PlayerMotor>();
        }

        public void PlayFootstep()
        {
            if (motor == null || !motor.IsGrounded || motor.GroundCheck == null) return;

            var ray = new Ray(motor.GroundCheck.position + Vector3.up * 0.1f, Vector3.down);
            if (!Physics.Raycast(ray, out RaycastHit hit, probeDistance)) return;

            int hitLayer = hit.collider.gameObject.layer;
            foreach (var layer in footstepLayers)
            {
                if ((layer.layer.value & (1 << hitLayer)) == 0) continue;
                Play(layer);
                return;
            }
        }

        private static void Play(FootstepLayer layer)
        {
            if (layer.source == null || layer.clips == null || layer.clips.Length == 0) return;

            layer.source.clip = layer.clips[layer.currentIndex];
            layer.source.Play();
            layer.currentIndex = (layer.currentIndex + 1) % layer.clips.Length;
        }
    }
}
