using System;
using System.Collections;
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
        [SerializeField] private float speedMultiplier = 0.8f;
        [Tooltip("Seconds to slide into the stand point before the joint is created")]
        [SerializeField] private float attachDuration = 0.25f;

        private Rigidbody body;
        private FixedJoint joint;
        private Coroutine attachRoutine;
        private bool isAttaching;

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
            if (grabbable == null || IsGrabbing || isAttaching || grabbable.Body == null) return;

            Current = grabbable;
            Candidate = null;
            UpdateGrabPoints();
            grabbable.SetGrabbed(true);
            SetCollisionIgnored(grabbable, true);

            if (motor != null)
            {
                motor.Stop();
                motor.FacingTarget = grabbable.transform;
                motor.SpeedMultiplier = speedMultiplier;
                motor.AllowRun = false;
                motor.AllowJump = false;
            }

            attachRoutine = StartCoroutine(AttachRoutine(grabbable));
        }

        private IEnumerator AttachRoutine(Grabbable grabbable)
        {
            isAttaching = true;
            if (motor != null) motor.enabled = false;

            if (grabbable.GetStandPoint(transform.position, out Vector3 standPoint, out Vector3 normal))
            {
                Vector3 from = body.position;
                Quaternion fromRotation = body.rotation;
                Quaternion toRotation = Quaternion.LookRotation(-normal, Vector3.up);
                float elapsed = 0f;
                while (elapsed < attachDuration)
                {
                    elapsed += Time.fixedDeltaTime;
                    float t = Mathf.Clamp01(elapsed / attachDuration);
                    body.MovePosition(Vector3.Lerp(from, standPoint, t));
                    body.MoveRotation(Quaternion.Slerp(fromRotation, toRotation, t));
                    yield return new WaitForFixedUpdate();
                }
                body.position = standPoint;
                body.rotation = toRotation;
                body.linearVelocity = Vector3.zero;
            }

            joint = gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = grabbable.Body;
            joint.enableCollision = false;

            if (motor != null) motor.enabled = true;
            isAttaching = false;
            attachRoutine = null;
            UpdateGrabPoints();

            Grabbed?.Invoke(grabbable);
            onGrabbed?.Invoke();
        }

        public void Release()
        {
            if (!IsGrabbing) return;

            var released = Current;
            Current = null;

            if (attachRoutine != null)
            {
                StopCoroutine(attachRoutine);
                attachRoutine = null;
                isAttaching = false;
                if (motor != null) motor.enabled = true;
            }

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

            SetCollisionIgnored(released, false);
            released.SetGrabbed(false);
            Released?.Invoke(released);
            onReleased?.Invoke();
        }

        private void SetCollisionIgnored(Grabbable grabbable, bool ignored)
        {
            if (grabbable == null) return;
            foreach (var mine in GetComponentsInChildren<Collider>())
            {
                if (mine.isTrigger) continue;
                foreach (var theirs in grabbable.Colliders)
                    if (theirs != null && !theirs.isTrigger) Physics.IgnoreCollision(mine, theirs, ignored);
            }
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
