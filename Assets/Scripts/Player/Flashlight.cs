using System;
using UnityEngine;

namespace PussInHell.Player
{
    public class Flashlight : MonoBehaviour
    {
        [Tooltip("Can the player use the flashlight right now")]
        [SerializeField] private bool isAvailable = true;
        [SerializeField] private KeyCode toggleKey = KeyCode.F;
        [Tooltip("Seconds after switching off before it can be toggled again")]
        [SerializeField] private float toggleCooldown = 0.5f;

        private float cooldownUntil;

        public bool IsOn { get; private set; }
        public bool IsAvailable => isAvailable;

        public event Action TurnedOn;
        public event Action TurnedOff;

        private void Update()
        {
            if (!isAvailable || Time.time < cooldownUntil) return;
            if (Input.GetKeyDown(toggleKey))
                Toggle();
        }

        public void Toggle()
        {
            if (IsOn) TurnOff();
            else TurnOn();
        }

        public void TurnOn()
        {
            if (IsOn || !isAvailable) return;
            IsOn = true;
            TurnedOn?.Invoke();
        }

        public void TurnOff()
        {
            if (!IsOn) return;
            IsOn = false;
            cooldownUntil = Time.time + toggleCooldown;
            TurnedOff?.Invoke();
        }

        public void SetAvailability(bool available)
        {
            isAvailable = available;
            if (!available) TurnOff();
        }
    }
}
