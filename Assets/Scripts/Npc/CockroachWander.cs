using System;
using UnityEngine;

namespace PussInHell.Npc
{
    [RequireComponent(typeof(Rigidbody))]
    public class CockroachWander : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float wanderTime = 2f;
        [SerializeField] private float idleTime = 2f;
        [SerializeField] private float directionChangeInterval = 0.5f;
        [SerializeField] private float turnSpeed = 5f;

        [Header("Surface")]
        [SerializeField] private float surfaceCheckDistance = 1f;
        [SerializeField] private LayerMask surfaceLayer;

        private Rigidbody body;
        private Vector3 wanderDirection;
        private float wanderTimer;
        private float idleTimer;
        private float directionChangeTimer;
        private bool isIdle;
        private bool isMoving;
        private Vector3 lastPosition;

        public bool IsMoving
        {
            get => isMoving;
            private set
            {
                if (isMoving == value) return;
                isMoving = value;
                MovingChanged?.Invoke(value);
            }
        }

        public event Action<bool> MovingChanged;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = true;
            body.constraints = RigidbodyConstraints.FreezeRotation;
            wanderTimer = wanderTime;
            directionChangeTimer = directionChangeInterval;
            lastPosition = transform.position;
            PickRandomDirection();
        }

        private void FixedUpdate()
        {
            if (!IsOnSurface())
            {
                IsMoving = false;
                return;
            }

            if (!isIdle && Vector3.Distance(transform.position, lastPosition) < 0.01f)
            {
                PickRandomDirection();
                wanderTimer = wanderTime;
            }
            lastPosition = transform.position;

            if (isIdle) Idle();
            else Wander();
        }

        private bool IsOnSurface()
        {
            return Physics.Raycast(transform.position, Vector3.down, surfaceCheckDistance, surfaceLayer);
        }

        private void Wander()
        {
            wanderTimer -= Time.fixedDeltaTime;
            directionChangeTimer -= Time.fixedDeltaTime;

            if (directionChangeTimer <= 0f)
            {
                wanderDirection = (wanderDirection + new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0f, UnityEngine.Random.Range(-0.5f, 0.5f))).normalized;
                directionChangeTimer = directionChangeInterval;
            }

            if (wanderTimer <= 0f)
            {
                isIdle = true;
                idleTimer = UnityEngine.Random.Range(idleTime * 0.5f, idleTime * 1.5f);
                IsMoving = false;
                return;
            }

            Vector3 move = wanderDirection * moveSpeed;
            body.MovePosition(body.position + move * Time.fixedDeltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(move, Vector3.up), Time.fixedDeltaTime * turnSpeed);
            IsMoving = true;
        }

        private void Idle()
        {
            IsMoving = false;
            idleTimer -= Time.fixedDeltaTime;
            if (idleTimer > 0f) return;

            isIdle = false;
            wanderTimer = UnityEngine.Random.Range(wanderTime * 0.5f, wanderTime * 1.5f);
            directionChangeTimer = directionChangeInterval;
            PickRandomDirection();
        }

        private void PickRandomDirection()
        {
            wanderDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f)).normalized;
        }

        private void OnCollisionStay(Collision collision)
        {
            if (((1 << collision.gameObject.layer) & surfaceLayer) == 0) return;

            Vector3 normal = collision.contacts[0].normal;
            if (Vector3.Dot(normal, Vector3.up) >= 0.5f) return;

            wanderDirection = Vector3.Reflect(wanderDirection, normal).normalized;
            wanderTimer = wanderTime;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, Vector3.down * surfaceCheckDistance);
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, wanderDirection * 2f);
        }
    }
}
