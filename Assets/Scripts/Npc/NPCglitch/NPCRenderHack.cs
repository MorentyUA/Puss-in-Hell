using UnityEngine;

public class NPCRenderTrigger : MonoBehaviour
{
    public float maxDetectionRadius = 10f;
    public float minDetectionRadius = 2f;

    [Range(0f, 1f)]
    public float hackPower = 1f; // индивидуальная сила NPC, максимум 1

    private Transform player;

    void Start()
    {
        var obj = GameObject.FindGameObjectWithTag("Player");
        if (obj != null)
            player = obj.transform;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= maxDetectionRadius)
        {
            float baseIntensity;

            if (distance <= minDetectionRadius)
                baseIntensity = 1f;
            else
            {
                float normalized = (distance - minDetectionRadius) / (maxDetectionRadius - minDetectionRadius);
                baseIntensity = Mathf.Lerp(1f, 0f, normalized);
            }

            // итоговая интенсивность NPC
            float finalIntensity = baseIntensity * hackPower;

            // отправляем менеджеру
            GameRenderManager.Instance.ReportIntensity(finalIntensity);
        }
    }
}
