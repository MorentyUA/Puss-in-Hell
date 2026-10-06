using System;
using UnityEngine;

namespace PussInHell.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMotor : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float runSpeed = 10f;
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float deceleration = 15f;
        [SerializeField] private float runDeceleration = 8f;
        [SerializeField] private float jumpForce = 8f;
        [Range(0f, 1f)]
        [SerializeField] private float airControl = 0.3f;

        [Header("Rotation")]
        [SerializeField] private float rotationSpeed = 8f;
        [SerializeField] private float runRotationMultiplier = 1.3f;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Camera")]
        [Tooltip("Follow Camera.main every frame (CinemachineBrain output). Off: use the transform below.")]
        [SerializeField] private bool useMainCamera = true;
        [SerializeField] private Transform cameraTransform;

        [Header("Input")]
        [SerializeField] private KeyCode runKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;

        private Rigidbody body;
        private Vector3 currentVelocity;
        private Vector3 targetDirection;
        private float currentMaxSpeed;
        private bool wasRunning;

        public bool IsGrounded { get; private set; }
        public bool IsWalking { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsMoving => IsWalking || IsRunning;
        public Transform GroundCheck => groundCheck;
        public Vector3 PlanarVelocity => new Vector3(currentVelocity.x, 0f, currentVelocity.z);

        public event Action Jumped;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.freezeRotation = true;
            body.linearDamping = 0f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            currentMaxSpeed = walkSpeed;
            RefreshCamera();
        }

        private void Update()
        {
            IsGrounded = groundCheck != null && Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
            RefreshCamera();

            if (cameraTransform == null)
            {
                targetDirection = Vector3.zero;
                IsWalking = false;
                IsRunning = false;
                return;
            }

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            targetDirection = (forward * vertical + right * horizontal).normalized;

            bool hasInput = targetDirection.sqrMagnitude > 0.01f;
            IsRunning = hasInput && IsGrounded && Input.GetKey(runKey);
            IsWalking = hasInput && IsGrounded;
            if (hasInput) wasRunning = IsRunning;

            float targetSpeed = IsRunning ? runSpeed : walkSpeed;
            currentMaxSpeed = Mathf.Lerp(currentMaxSpeed, targetSpeed, acceleration * Time.deltaTime);

            if (IsGrounded && Input.GetKeyDown(jumpKey))
                Jump();
        }

        private void FixedUpdate()
        {
            Move();
            Rotate();
        }

        private void RefreshCamera()
        {
            if (useMainCamera && Camera.main != null)
                cameraTransform = Camera.main.transform;
        }

        private void Move()
        {
            Vector3 targetVelocity = targetDirection * currentMaxSpeed;
            bool hasInput = targetDirection.sqrMagnitude > 0.01f;
            float transition = hasInput ? acceleration : (wasRunning ? runDeceleration : deceleration);
            if (!IsGrounded) transition *= airControl;

            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, transition * Time.fixedDeltaTime);
            body.linearVelocity = new Vector3(currentVelocity.x, body.linearVelocity.y, currentVelocity.z);
        }

        private void Rotate()
        {
            if (targetDirection.sqrMagnitude < 0.01f) return;

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            float speed = IsRunning ? rotationSpeed * runRotationMultiplier : rotationSpeed;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * Time.fixedDeltaTime);
        }

        private void Jump()
        {
            body.linearVelocity = new Vector3(body.linearVelocity.x, 0f, body.linearVelocity.z);
            body.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            Jumped?.Invoke();
        }

        public void Stop()
        {
            targetDirection = Vector3.zero;
            currentVelocity = Vector3.zero;
            IsWalking = false;
            IsRunning = false;
            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
        }

        public void SetCameraTransform(Transform camera)
        {
            cameraTransform = camera;
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null) return;
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
