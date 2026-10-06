using UnityEngine;
using Cinemachine;
using PussInHell.Core;
using PussInHell.Player;

namespace PussInHell.Cameras
{
    public class CameraSwitchTrigger : MonoBehaviour
    {
        [Header("Cameras")]
        [SerializeField] private CinemachineVirtualCamera playerCamera;
        [SerializeField] private CinemachineVirtualCamera triggerCamera;
        [SerializeField] private int playerCameraPriority = 10;
        [SerializeField] private int triggerCameraPriority = 20;
        [Tooltip("Hard priority jump (100/0) instead of the configured priorities")]
        [SerializeField] private bool instantSwitch = true;

        [Header("Trigger")]
        [SerializeField] private string playerTag = PlayerLocator.Tag;
        [SerializeField] private bool disablePlayerControl = false;
        [SerializeField] private bool lockCursorOnExit = true;

        [Header("First Person Rig")]
        [Tooltip("Object holding the first-person camera; enabled only inside the zone")]
        [SerializeField] private GameObject fpsCameraObject;
        [SerializeField] private bool manageFPSCamera = true;

        [Header("Culling")]
        [Tooltip("Hide this layer from the main camera while in first person")]
        [SerializeField] private bool managePlayerLayer = true;
        [SerializeField] private string playerLayerName = "Player";

        private CinemachineBrain brain;
        private FirstPersonCamera fpsCamera;
        private Camera mainCamera;
        private int originalCullingMask;

        public bool IsPlayerInTrigger { get; private set; }

        private void Start()
        {
            mainCamera = Camera.main;
            brain = mainCamera != null ? mainCamera.GetComponent<CinemachineBrain>() : null;
            if (mainCamera == null || brain == null || playerCamera == null || triggerCamera == null)
            {
                enabled = false;
                return;
            }

            originalCullingMask = mainCamera.cullingMask;
            playerCamera.Priority = playerCameraPriority;
            triggerCamera.Priority = 0;

            if (fpsCameraObject != null)
            {
                fpsCamera = fpsCameraObject.GetComponentInChildren<FirstPersonCamera>(true);
                if (manageFPSCamera) fpsCameraObject.SetActive(false);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag) || IsPlayerInTrigger) return;
            IsPlayerInTrigger = true;
            SwitchToTriggerCamera();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(playerTag) || !IsPlayerInTrigger) return;
            IsPlayerInTrigger = false;
            SwitchToPlayerCamera();
        }

        public void ForceSwitchToTriggerCamera()
        {
            IsPlayerInTrigger = true;
            SwitchToTriggerCamera();
        }

        public void ForceSwitchToPlayerCamera()
        {
            IsPlayerInTrigger = false;
            SwitchToPlayerCamera();
        }

        public void SetInstantSwitch(bool instant) => instantSwitch = instant;

        private void SwitchToTriggerCamera()
        {
            if (manageFPSCamera && fpsCameraObject != null) fpsCameraObject.SetActive(true);
            SyncFpsCameraWithView();
            SetMotorCamera(false);

            if (managePlayerLayer)
            {
                int layer = LayerMask.NameToLayer(playerLayerName);
                if (layer != -1) mainCamera.cullingMask &= ~(1 << layer);
            }

            triggerCamera.Priority = instantSwitch ? 100 : triggerCameraPriority;
            playerCamera.Priority = instantSwitch ? 0 : playerCameraPriority - 10;

            if (disablePlayerControl) SetControl(false);
        }

        private void SwitchToPlayerCamera()
        {
            playerCamera.Priority = instantSwitch ? 100 : playerCameraPriority;
            triggerCamera.Priority = 0;

            if (managePlayerLayer) mainCamera.cullingMask = originalCullingMask;
            SetMotorCamera(true);
            SyncFpsCameraWithView();

            if (manageFPSCamera && fpsCameraObject != null) fpsCameraObject.SetActive(false);
            if (disablePlayerControl) SetControl(true);

            if (lockCursorOnExit)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void SyncFpsCameraWithView()
        {
            if (fpsCamera == null) return;

            Camera active = brain.OutputCamera != null ? brain.OutputCamera : Camera.main;
            if (active == null) return;

            Vector3 forward = active.transform.forward;
            float yaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
            float pitch = -Mathf.Asin(forward.y) * Mathf.Rad2Deg;
            fpsCamera.SetRotationDirectly(pitch, yaw);
        }

        private void SetMotorCamera(bool toMainCamera)
        {
            var player = PlayerLocator.Player;
            var motor = player != null ? player.GetComponent<PlayerMotor>() : null;
            if (motor == null) return;

            if (toMainCamera)
            {
                if (Camera.main != null) motor.SetCameraTransform(Camera.main.transform);
                return;
            }

            var camera = fpsCameraObject != null ? fpsCameraObject.GetComponentInChildren<Camera>(true) : null;
            if (camera != null) motor.SetCameraTransform(camera.transform);
        }

        private void SetControl(bool enabledControl)
        {
            if (enabledControl) PlayerControlLock.Unlock();
            else PlayerControlLock.Lock();

            var player = PlayerLocator.Player;
            var fps = player != null ? player.GetComponentInChildren<FirstPersonCamera>(true) : null;
            if (fps != null) fps.enabled = enabledControl;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = IsPlayerInTrigger ? Color.green : Color.cyan;
            Gizmos.DrawWireCube(transform.position, transform.localScale);
            if (triggerCamera == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, triggerCamera.transform.position);
        }
    }
}
