using System;
using UnityEngine;

namespace PussInHell.Player
{
    public class PlayerIdleBehaviour : MonoBehaviour
    {
        [SerializeField] private PlayerMotor motor;
        [Tooltip("Seconds of standing still before an idle action fires")]
        [SerializeField] private float afkTriggerTime = 10f;
        [Tooltip("Seconds to wait after an idle action before counting again")]
        [SerializeField] private float afkCooldown = 10f;

        private float idleTimer;
        private float cooldownTimer;

        public event Action IdleTriggered;

        private void Awake()
        {
            if (motor == null) motor = GetComponentInParent<PlayerMotor>();
        }

        private void OnEnable()
        {
            if (motor != null) motor.Jumped += ResetTimer;
        }

        private void OnDisable()
        {
            if (motor != null) motor.Jumped -= ResetTimer;
        }

        private void Update()
        {
            if (motor == null) return;

            bool active = motor.IsMoving || !motor.IsGrounded;
            if (active)
            {
                idleTimer = 0f;
                return;
            }

            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
                return;
            }

            idleTimer += Time.deltaTime;
            if (idleTimer < afkTriggerTime) return;

            idleTimer = 0f;
            cooldownTimer = afkCooldown;
            IdleTriggered?.Invoke();
        }

        public void ResetTimer()
        {
            idleTimer = 0f;
        }
    }
}
