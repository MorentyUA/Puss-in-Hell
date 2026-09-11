using UnityEngine;

public class ObjectInteraction : MonoBehaviour
{
    [Header("Настройки взаимодействия")]
    [SerializeField] private float interactionRadius = 20f;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private Transform pressPoint; // Точка для расчета радиуса

    [Header("Настройки анимации")]
    [SerializeField] private string animationBoolName = "falling";
    [SerializeField] private bool useBool = true;

    [Header("Ссылки")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;

    private bool hasPlayed = false;
    private bool playerInRange = false;
    private Transform interactionPoint; // Точка от которой считается расстояние

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        // Ищем дочерний объект "Press Point"
        if (pressPoint == null)
        {
            Transform foundPoint = transform.Find("Press Point");
            if (foundPoint != null)
            {
                pressPoint = foundPoint;
            }
        }

        // Определяем от какой точки считать расстояние
        interactionPoint = pressPoint != null ? pressPoint : transform;
    }

    void Update()
    {
        if (player == null || hasPlayed)
            return;

        float distance = Vector3.Distance(interactionPoint.position, player.position);
        playerInRange = distance <= interactionRadius;

        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            PlayAnimation();
        }
    }

    void PlayAnimation()
    {
        if (animator != null && !hasPlayed)
        {
            if (useBool)
            {
                animator.SetBool(animationBoolName, true);
            }
            else
            {
                animator.SetTrigger(animationBoolName);
            }
            hasPlayed = true;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Определяем точку для визуализации
        Vector3 center = interactionPoint != null ? interactionPoint.position : transform.position;

        // Если Press Point не назначен, пытаемся найти в редакторе
        if (interactionPoint == null && pressPoint == null)
        {
            Transform foundPoint = transform.Find("Press Point");
            if (foundPoint != null)
            {
                center = foundPoint.position;
            }
        }

        Gizmos.color = playerInRange ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(center, interactionRadius);
    }
}
