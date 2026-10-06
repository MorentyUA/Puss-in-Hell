using UnityEngine;

namespace PussInHell.Cameras
{
    public class FirstPersonCamera : MonoBehaviour
    {
        [Header("Mouse")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float smoothTime = 0.05f;

        [Header("Limits")]
        [SerializeField] private float minVerticalAngle = -80f;
        [SerializeField] private float maxVerticalAngle = 80f;

        [Header("References")]
        [Tooltip("Rotated around Y by the mouse. Empty: parent transform.")]
        [SerializeField] private Transform playerBody;

        [Header("Head Bob")]
        [SerializeField] private bool enableHeadBob = false;
        [SerializeField] private float bobFrequency = 2f;
        [SerializeField] private float bobHorizontalAmplitude = 0.05f;
        [SerializeField] private float bobVerticalAmplitude = 0.1f;

        [Header("Cursor")]
        [SerializeField] private bool manageCursor = true;

        private float targetPitch;
        private float targetYaw;
        private float pitch;
        private float yaw;
        private float pitchVelocity;
        private float yawVelocity;
        private float bobTimer;
        private Vector3 originalLocalPosition;

        public float Pitch => pitch;
        public float Yaw => yaw;

        private void Start()
        {
            if (playerBody == null) playerBody = transform.parent;
            if (playerBody == null)
            {
                enabled = false;
                return;
            }

            originalLocalPosition = transform.localPosition;
            if (manageCursor) LockCursor(true);
        }

        private void Update()
        {
            targetYaw += Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            targetPitch = Mathf.Clamp(targetPitch - Input.GetAxisRaw("Mouse Y") * mouseSensitivity, minVerticalAngle, maxVerticalAngle);

            if (!manageCursor) return;
            if (Input.GetKeyDown(KeyCode.Escape)) LockCursor(false);
            if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None) LockCursor(true);
        }

        private void LateUpdate()
        {
            pitch = Mathf.SmoothDamp(pitch, targetPitch, ref pitchVelocity, smoothTime);
            yaw = Mathf.SmoothDamp(yaw, targetYaw, ref yawVelocity, smoothTime);

            transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            playerBody.rotation = Quaternion.Euler(0f, yaw, 0f);

            ApplyHeadBob();
        }

        private void ApplyHeadBob()
        {
            if (!enableHeadBob) return;

            bool moving = Input.GetAxisRaw("Horizontal") != 0f || Input.GetAxisRaw("Vertical") != 0f;
            if (moving)
            {
                bobTimer += Time.deltaTime * bobFrequency;
                transform.localPosition = originalLocalPosition + new Vector3(Mathf.Sin(bobTimer) * bobHorizontalAmplitude, Mathf.Cos(bobTimer * 2f) * bobVerticalAmplitude, 0f);
            }
            else
            {
                bobTimer = 0f;
                transform.localPosition = Vector3.Lerp(transform.localPosition, originalLocalPosition, Time.deltaTime * 5f);
            }
        }

        public void SetRotationDirectly(float newPitch, float newYaw)
        {
            targetPitch = pitch = newPitch;
            targetYaw = yaw = newYaw;
            pitchVelocity = 0f;
            yawVelocity = 0f;

            transform.localRotation = Quaternion.Euler(newPitch, 0f, 0f);
            if (playerBody != null) playerBody.rotation = Quaternion.Euler(0f, newYaw, 0f);
        }

        public void SetSensitivity(float sensitivity) => mouseSensitivity = sensitivity;

        public void AddRecoil(float recoilPitch, float recoilYaw)
        {
            targetPitch = Mathf.Clamp(targetPitch + recoilPitch, minVerticalAngle, maxVerticalAngle);
            targetYaw += recoilYaw;
        }

        public void SetHeadBob(bool enabledBob)
        {
            enableHeadBob = enabledBob;
            if (!enabledBob) transform.localPosition = originalLocalPosition;
        }

        private static void LockCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
