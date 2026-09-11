using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class VignetteRadiusTrigger : MonoBehaviour
{
    [Header("Радиус обнаружения")]
    public float maxDetectionRadius = 10f;
    public float minDetectionRadius = 2f;

    [Header("Значения виньетки")]
    public float vignetteInside = 0.5f;
    public float vignetteOutside = 0.3f;

    [Header("Скорость плавности")]
    [Range(0.1f, 10f)]
    public float smoothSpeed = 3f;

    private Transform player;
    private Volume globalVolume;
    private Vignette vignette;
    private float targetIntensity;
    private float currentIntensity;

    void Start()
    {
        // Находим игрока
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        // Находим Global Volume на сцене
        globalVolume = Object.FindFirstObjectByType<Volume>();
        if (globalVolume == null)
        {
            Debug.LogError("Global Volume не найден на сцене!");
            enabled = false;
            return;
        }

        // Получаем компонент Vignette
        if (!globalVolume.profile.TryGet<Vignette>(out vignette))
        {
            Debug.LogError("Vignette не найден в Volume Profile!");
            enabled = false;
            return;
        }

        // Устанавливаем начальное значение
        currentIntensity = vignetteOutside;
        vignette.intensity.Override(currentIntensity);
    }

    void Update()
    {
        if (player == null || globalVolume == null || vignette == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        bool isInside = distance <= maxDetectionRadius;

        // Определяем целевое значение
        if (distance <= minDetectionRadius)
            targetIntensity = vignetteInside;
        else if (isInside)
        {
            float t = (distance - minDetectionRadius) / (maxDetectionRadius - minDetectionRadius);
            targetIntensity = Mathf.Lerp(vignetteInside, vignetteOutside, t);
        }
        else
        {
            targetIntensity = vignetteOutside;
        }

        // Плавно интерполируем
        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * smoothSpeed);
        vignette.intensity.Override(currentIntensity);
    }

    // Визуализация в редакторе
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minDetectionRadius);
        Gizmos.color = new Color(1, 0.6f, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, maxDetectionRadius);
    }
}
