using System;
using UnityEngine;
using PussInHell.Npc;

namespace PussInHell.Player
{
    public class PlayerFearState : MonoBehaviour
    {
        [Tooltip("Glitch intensity (0..1) at which the player becomes feared")]
        [Range(0f, 1f)]
        [SerializeField] private float fearThreshold = 0.1f;

        public bool IsFeared { get; private set; }

        public event Action<bool> FearChanged;

        private void Update()
        {
            var hub = GlitchIntensityHub.Instance;
            bool feared = hub != null && hub.CurrentIntensity >= fearThreshold;
            if (feared == IsFeared) return;

            IsFeared = feared;
            FearChanged?.Invoke(feared);
        }
    }
}
