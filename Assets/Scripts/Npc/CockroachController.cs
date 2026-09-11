using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(Animator))]
public class CockroachController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float wanderTime = 2f;
    public float idleTime = 2f;
    public float surfaceCheckDistance = 1f; // Distance to check for valid surface below
    public float directionChangeInterval = 0.5f; // How often to adjust direction
    public LayerMask surfaceLayer; // Layer for surfaces to walk on

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip[] scuttleClips; // Sounds for movement

    private Animator animator;
    private Rigidbody rb;
    private Vector3 wanderDirection;
    private float wanderTimer;
    private float idleTimer;
    private float directionChangeTimer;
    private bool isIdle = false;
    private Vector3 lastPosition; // To detect if stuck

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true; // Enable gravity for normal physics
        rb.constraints = RigidbodyConstraints.FreezeRotation; // Prevent tipping over
        wanderTimer = wanderTime;
        directionChangeTimer = directionChangeInterval;
        lastPosition = transform.position;

        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError("Cockroach has no Collider component!");
        }
        else if (!col.enabled)
        {
            Debug.LogWarning("Cockroach Collider is disabled! Enabling it.");
            col.enabled = true;
        }
    }

    void FixedUpdate()
    {
        // Check if on valid surface
        if (!IsOnValidSurface())
        {
            animator.SetBool("move", false); // Stop moving if not on valid surface
            return;
        }

        // Check if stuck
        if (Vector3.Distance(transform.position, lastPosition) < 0.01f && !isIdle)
        {
            wanderDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
            wanderTimer = wanderTime;
        }
        lastPosition = transform.position;

        if (isIdle)
        {
            HandleIdle();
        }
        else
        {
            Wander();
        }
    }

    bool IsOnValidSurface()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, surfaceCheckDistance, surfaceLayer))
        {
            return true;
        }
        // Убрали Debug.LogWarning
        return false;
    }

    void Wander()
    {
        wanderTimer -= Time.fixedDeltaTime;
        directionChangeTimer -= Time.fixedDeltaTime;

        // Adjust direction to avoid circles
        if (directionChangeTimer <= 0f)
        {
            wanderDirection = (wanderDirection + new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f))).normalized;
            directionChangeTimer = directionChangeInterval;
        }

        if (wanderTimer > 0f)
        {
            Vector3 move = wanderDirection.normalized * moveSpeed;
            rb.MovePosition(rb.position + move * Time.fixedDeltaTime);

            // Orient forward direction
            if (move.magnitude > 0.1f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(move, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.fixedDeltaTime * 5f);
                animator.SetBool("move", true); // Trigger 'tarakan' animation
                PlayScuttleSound();
            }
        }
        else
        {
            isIdle = true;
            idleTimer = Random.Range(idleTime * 0.5f, idleTime * 1.5f);
            animator.SetBool("move", false); // Trigger 'stay' animation
        }
    }

    void HandleIdle()
    {
        idleTimer -= Time.fixedDeltaTime;
        if (idleTimer <= 0f)
        {
            isIdle = false;
            wanderTimer = Random.Range(wanderTime * 0.5f, wanderTime * 1.5f);
            wanderDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
            directionChangeTimer = directionChangeInterval;
        }
        animator.SetBool("move", false); // Trigger 'stay' animation
    }

    void OnCollisionStay(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & surfaceLayer) != 0)
        {
            // Adjust direction to avoid getting stuck
            Vector3 collisionNormal = collision.contacts[0].normal;
            if (Vector3.Dot(collisionNormal, Vector3.up) < 0.5f) // If hitting a steep surface (e.g., wall)
            {
                wanderDirection = Vector3.Reflect(wanderDirection, collisionNormal).normalized;
                wanderTimer = wanderTime;
            }
        }
    }

    void PlayScuttleSound()
    {
        if (scuttleClips == null || scuttleClips.Length == 0 || audioSource == null) return;

        if (!audioSource.isPlaying)
        {
            AudioClip clip = scuttleClips[Random.Range(0, scuttleClips.Length)];
            if (clip != null)
                audioSource.PlayOneShot(clip);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Visualize surface check ray
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, Vector3.down * surfaceCheckDistance);

        // Visualize wander direction
        Gizmos.color = Color.green;
        if (wanderDirection != Vector3.zero)
        {
            Gizmos.DrawRay(transform.position, wanderDirection * 2f);
        }
    }
}
