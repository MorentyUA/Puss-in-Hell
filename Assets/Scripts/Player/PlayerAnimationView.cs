using UnityEngine;

namespace PussInHell.Player
{
    public class PlayerAnimationView : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private PlayerFearState fearState;
        [SerializeField] private PlayerIdleBehaviour idleBehaviour;

        [Header("Animator Parameters")]
        [SerializeField] private string walkParameter = "walk";
        [SerializeField] private string runParameter = "run";
        [SerializeField] private string jumpTrigger = "jump";
        [SerializeField] private string fearParameter = "fear";
        [SerializeField] private string[] idleTriggers = { "afk", "afk2" };

        private void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (motor == null) motor = GetComponentInParent<PlayerMotor>();
            if (fearState == null) fearState = GetComponentInParent<PlayerFearState>();
            if (idleBehaviour == null) idleBehaviour = GetComponentInParent<PlayerIdleBehaviour>();
        }

        private void OnEnable()
        {
            if (motor != null) motor.Jumped += OnJumped;
            if (fearState != null) fearState.FearChanged += OnFearChanged;
            if (idleBehaviour != null) idleBehaviour.IdleTriggered += OnIdleTriggered;
        }

        private void OnDisable()
        {
            if (motor != null) motor.Jumped -= OnJumped;
            if (fearState != null) fearState.FearChanged -= OnFearChanged;
            if (idleBehaviour != null) idleBehaviour.IdleTriggered -= OnIdleTriggered;
        }

        private void Update()
        {
            if (animator == null || motor == null) return;
            animator.SetBool(walkParameter, motor.IsWalking);
            animator.SetBool(runParameter, motor.IsRunning);
        }

        public void ResetLocomotion()
        {
            if (animator == null) return;
            animator.SetBool(walkParameter, false);
            animator.SetBool(runParameter, false);
        }

        private void OnJumped()
        {
            if (animator != null) animator.SetTrigger(jumpTrigger);
        }

        private void OnFearChanged(bool feared)
        {
            if (animator != null) animator.SetBool(fearParameter, feared);
        }

        private void OnIdleTriggered()
        {
            if (animator == null || idleTriggers == null || idleTriggers.Length == 0) return;
            animator.SetTrigger(idleTriggers[Random.Range(0, idleTriggers.Length)]);
        }
    }
}
