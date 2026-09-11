using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class HaloHighlighter : MonoBehaviour
{
    [Header("Настройки подсветки")]
    [Tooltip("Материал контура / подсветки. ОБЯЗАТЕЛЬНО перетащи сюда из проекта.")]
    public Material outlineMaterial;
    [Min(0f)]
    public float activationRadius = 3f;

    [Header("Объекты для подсветки")]
    [Tooltip("Объекты с Renderer (MeshRenderer/SkinnedMeshRenderer), на которых должна быть подсветка.")]
    public List<GameObject> targetObjects = new List<GameObject>();

    [Header("Управление активацией")]
    [Tooltip("Если включено — требуется нажать клавишу E, чтобы отключить подсветку навсегда.")]
    public bool requireInteractKey = true;
    public KeyCode interactKey = KeyCode.E;

    [Header("Игрок")]
    [Tooltip("Если оставить пустым, скрипт попробует найти объект с тегом Player.")]
    [SerializeField] private Transform player;

    [Header("События")]
    [Tooltip("Вызывается когда объект активирован (подсветка отключена навсегда)")]
    public UnityEvent onActivated;

    // Статическое событие для менеджеров (передаёт ссылку на активированный объект)
    public static event System.Action<HaloHighlighter> OnAnyActivated;

    // Внутренние списки
    private readonly List<Renderer> renderers = new List<Renderer>();
    private readonly List<Material[]> originalSharedMaterials = new List<Material[]>();

    private bool isActivated = false;
    private bool isInRange = false;

    /// <summary>
    /// Был ли объект активирован (подсветка отключена навсегда)
    /// </summary>
    public bool IsActivated => isActivated;

    private void Awake()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }

        if (player == null)
        {
            Debug.LogWarning($"{nameof(HaloHighlighter)}: объект с тегом Player не найден, скрипт отключён.");
            enabled = false;
            return;
        }

        if (outlineMaterial == null)
        {
            Debug.LogWarning($"{nameof(HaloHighlighter)}: не задан outlineMaterial, скрипт отключён.");
            enabled = false;
            return;
        }

        foreach (var obj in targetObjects)
        {
            if (obj == null) continue;

            Renderer rend = obj.GetComponent<Renderer>();
            if (rend != null)
            {
                renderers.Add(rend);
                originalSharedMaterials.Add(rend.sharedMaterials);
            }
        }

        if (renderers.Count == 0)
        {
            Debug.LogWarning($"{nameof(HaloHighlighter)}: ни один из выбранных объектов не имеет Renderer!");
        }
    }

    private void Update()
    {
        if (isActivated || player == null)
            return;

        float sqrDist = (player.position - transform.position).sqrMagnitude;
        float sqrRadius = activationRadius * activationRadius;
        bool currentlyInRange = sqrDist < sqrRadius;

        if (currentlyInRange != isInRange)
        {
            isInRange = currentlyInRange;
            if (isInRange)
                EnableOutline();
            else
                DisableOutline();
        }

        if (requireInteractKey && isInRange && Input.GetKeyDown(interactKey))
        {
            Activate();
        }
    }

    /// <summary>
    /// Активирует объект (отключает подсветку навсегда и вызывает события)
    /// </summary>
    public void Activate()
    {
        if (isActivated) return;

        DisableOutline();
        isActivated = true;

        // Вызываем UnityEvent (можно подписаться в инспекторе)
        onActivated?.Invoke();

        // Вызываем статическое событие (для менеджеров)
        OnAnyActivated?.Invoke(this);
    }

    /// <summary>
    /// Сбрасывает состояние активации (для повторного использования)
    /// </summary>
    public void ResetActivation()
    {
        isActivated = false;
        isInRange = false;
    }

    private void EnableOutline()
    {
        if (outlineMaterial == null) return;

        for (int i = 0; i < renderers.Count; i++)
        {
            var rend = renderers[i];
            if (rend == null) continue;

            var mats = rend.sharedMaterials;

            bool alreadyHasOutline = System.Array.Exists(mats, m => m == outlineMaterial);
            if (alreadyHasOutline) continue;

            var newMats = new Material[mats.Length + 1];
            for (int j = 0; j < mats.Length; j++)
                newMats[j] = mats[j];

            newMats[newMats.Length - 1] = outlineMaterial;
            rend.sharedMaterials = newMats;
        }
    }

    private void DisableOutline()
    {
        for (int i = 0; i < renderers.Count && i < originalSharedMaterials.Count; i++)
        {
            var rend = renderers[i];
            if (rend == null) continue;

            rend.sharedMaterials = originalSharedMaterials[i];
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isActivated ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRadius);
    }
}
