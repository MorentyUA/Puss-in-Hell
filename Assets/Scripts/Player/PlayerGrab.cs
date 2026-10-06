using System;
using UnityEngine;
using UnityEngine.Events;
using PussInHell.Core;
using PussInHell.Interaction;

namespace PussInHell.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerGrab : MonoBehaviour
    {
        [SerializeField] private PlayerMotor motor;

        [Header("Detection")]
        [Tooltip("Objects with Grabbable closer than this can be grabbed")]
        [SerializeField] private float grabRadius = 2.2f;
        [Tooltip("Object must be within this angle of the facing direction")]
        [SerializeField] private float maxGrabAngle = 75f;
        [Tooltip("Grab is dropped when the object gets farther than this")]
        [SerializeField] private float releaseDistance = 3.5f;

        [Header("Input")]
        [SerializeField] private KeyCode grabKey = KeyCode.E;

        [Header("While Grabbing")]
        [Range(0.1f, 1f)]
        [SerializeField] private float speedMultiplier = 0.55f;

        private Rigidbody body;
        private FixedJoint joint;

        public Grabbable Current { get; private set; }
        public Grabbable Candidate { get; private set; }
        public bool IsGrabbing => Current != null;
        public Vector3 LeftHandPoint { get; private set; }
        public Vector3 RightHandPoint { get; private set; }
        public Vector3 FaceNormal { get; private set; }

        public event Action<Grabbable> Grabbed;
        public event Action<Grabbable> Released;
        public UnityEvent onGrabbed;
        public UnityEvent onReleased;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            if (motor == null) motor = GetComponent<PlayerMotor>();
        }

        private void OnDisable()
        {
            Release();
        }

        private void Update()
        {
            if (IsGrabbing)
            {
                bool tooFar = Vector3.Distance(transform.position, Current.transform.position) > releaseDistance;
                if (tooFar || PlayerControlLock.IsLocked || Input.GetKeyDown(grabKey))
                {
                    Release();
                    return;
                }

                UpdateGrabPoints();
                return;
            }

            if (motor != null && !motor.enabled) { Candidate = null; return; }

            Candidate = Grabbable.FindNearest(transform.position, transform.forward, grabRadius, maxGrabAngle);
            if (Candidate != null && Input.GetKeyDown(grabKey))
                Grab(Candidate);
        }

        public void Grab(Grabbable grabbable)
        {
            if (grabbable == null || IsGrabbing || grabbable.Body == null) return;

            Current = grabbable;
            Candidate = null;
            UpdateGrabPoints();

            joint = gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = grabbable.Body;
            joint.enableCollision = false;

            if (motor != null)
            {
                motor.FacingTarget = grabbable.transform;
                motor.SpeedMultiplier = speedMultiplier;
                motor.AllowRun = false;
                motor.AllowJump = false;
            }

            grabbable.SetGrabbed(true);
            Grabbed?.Invoke(grabbable);
            onGrabbed?.Invoke();
        }

        public void Release()
        {
            if (!IsGrabbing) return;

            var released = Current;
            Current = null;

            if (joint != null)
            {
                Destroy(joint);
                joint = null;
            }

            if (motor != null)
            {
                motor.FacingTarget = null;
                motor.SpeedMultiplier = 1f;
                motor.AllowRun = true;
                motor.AllowJump = true;
            }

            released.SetGrabbed(false);
            Released?.Invoke(released);
            onReleased?.Invoke();
        }

        private void UpdateGrabPoints()
        {
            if (Current == null) return;
            if (!Current.GetGrabPoints(transform.position, out Vector3 left, out Vector3 right, out Vector3 normal)) return;
            LeftHandPoint = left;
            RightHandPoint = right;
            FaceNormal = normal;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = IsGrabbing ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, grabRadius);
            if (!IsGrabbing) return;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(LeftHandPoint, 0.05f);
            Gizmos.DrawSphere(RightHandPoint, 0.05f);
        }
    }
}
