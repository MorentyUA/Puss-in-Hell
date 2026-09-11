using UnityEngine;
using UnityEngine.Rendering;
using FronkonGames.Glitches.Hacked;

public class GameRenderManager : MonoBehaviour
{
    public static GameRenderManager Instance;

    [Header("Hacked Effect")]
    public Hacked hackedFeature; // Теперь явно указываем в инспекторе

    [Header("Smooth")]
    public float smoothSpeed = 5f;

    private float currentIntensity = 0f;
    private float targetIntensity = 0f;
    private bool isHackedActive = false;

    void Awake()
    {
        Instance = this;

        // Проверяем, задан ли Hacked в инспекторе
        if (hackedFeature == null)
        {
            Debug.LogError("Hacked feature not assigned in GameRenderManager!");
        }
        else
        {
            // Убедимся, что эффект изначально выключен
            if (hackedFeature.settings != null)
                hackedFeature.settings.intensity = 0f;
        }
    }

    void Update()
    {
        // Плавно интерполируем к целевому значению
        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * smoothSpeed);

        ApplyIntensity(currentIntensity);

        // Управляем активностью эффекта ДО сброса targetIntensity,
        // иначе флаг никогда не станет true
        if (!isHackedActive && targetIntensity > 0f)
        {
            isHackedActive = true;
        }
        else if (isHackedActive && targetIntensity <= 0f && currentIntensity <= 0.01f)
        {
            isHackedActive = false;
        }

        // Сбрасываем targetIntensity — NPC будут добавлять в следующем кадре
        targetIntensity = 0f;
    }

    /// <summary>Активен ли сейчас глитч-эффект (кто-то из NPC в радиусе)</summary>
    public bool IsHackedActive => isHackedActive;

    public void ReportIntensity(float intensity)
    {
        if (intensity > targetIntensity)
            targetIntensity = intensity;
    }

    void ApplyIntensity(float value)
    {
        if (hackedFeature == null || hackedFeature.settings == null) return;

        hackedFeature.settings.intensity = value;
    }
}
