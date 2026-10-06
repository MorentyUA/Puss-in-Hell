using UnityEngine;

namespace PussInHell.Player
{
    public class PlayerFearEffects : MonoBehaviour
    {
        [SerializeField] private PlayerFearState fearState;
        [Tooltip("Objects enabled while the player is feared")]
        [SerializeField] private GameObject[] effects;

        private void Awake()
        {
            if (fearState == null) fearState = GetComponentInParent<PlayerFearState>();
            Apply(false);
        }

        private void OnEnable()
        {
            if (fearState != null) fearState.FearChanged += Apply;
        }

        private void OnDisable()
        {
            if (fearState != null) fearState.FearChanged -= Apply;
        }

        private void Apply(bool feared)
        {
            if (effects == null) return;
            foreach (var effect in effects)
                if (effect != null) effect.SetActive(feared);
        }
    }
}
