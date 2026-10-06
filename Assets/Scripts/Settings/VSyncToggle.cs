using UnityEngine;
using UnityEngine.UI;

namespace PussInHell.Settings
{
    [RequireComponent(typeof(Toggle))]
    public class VSyncToggle : MonoBehaviour
    {
        [SerializeField] private Toggle toggle;

        private void Reset()
        {
            toggle = GetComponent<Toggle>();
        }

        private void Awake()
        {
            if (toggle == null) toggle = GetComponent<Toggle>();
        }

        private void OnEnable()
        {
            if (GameSettings.Instance != null) toggle.SetIsOnWithoutNotify(GameSettings.Instance.VSyncEnabled);
            toggle.onValueChanged.AddListener(OnToggleChanged);
        }

        private void OnDisable()
        {
            toggle.onValueChanged.RemoveListener(OnToggleChanged);
        }

        private void OnToggleChanged(bool isOn)
        {
            if (GameSettings.Instance != null) GameSettings.Instance.VSyncEnabled = isOn;
            else QualitySettings.vSyncCount = isOn ? 1 : 0;
        }

        public void ToggleVSync()
        {
            toggle.isOn = !toggle.isOn;
        }
    }
}
