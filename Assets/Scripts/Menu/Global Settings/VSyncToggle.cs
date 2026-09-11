using UnityEngine;
using UnityEngine.UI;
using GabrielBissonnette.SAD;

[RequireComponent(typeof(Toggle))]
public class VSyncToggle : MonoBehaviour
{
    [SerializeField] private Toggle toggle;

    private void Reset()
    {
        toggle = GetComponent<Toggle>();
    }

    private void OnEnable()
    {
        if (GlobalSettingsManager.Instance != null)
        {
            // Устанавливаем текущее значение
            toggle.isOn = GlobalSettingsManager.Instance.VSyncEnabled;

            // Подписываемся на изменения
            toggle.onValueChanged.AddListener(OnToggleChanged);
        }
    }

    private void OnDisable()
    {
        if (toggle != null)
            toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool isOn)
    {
        if (GlobalSettingsManager.Instance != null)
        {
            GlobalSettingsManager.Instance.VSyncEnabled = isOn;
        }
    }

    // Дополнительно: можно вызвать принудительно (например, из другого скрипта)
    public void ToggleVSync()
    {
        if (GlobalSettingsManager.Instance != null)
        {
            GlobalSettingsManager.Instance.VSyncEnabled = !GlobalSettingsManager.Instance.VSyncEnabled;
            toggle.isOn = GlobalSettingsManager.Instance.VSyncEnabled;
        }
    }
}
