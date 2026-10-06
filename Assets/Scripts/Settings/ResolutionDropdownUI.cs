using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

namespace PussInHell.Settings
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public class ResolutionDropdownUI : MonoBehaviour
    {
        [SerializeField] private bool applyOnChange = true;
        [SerializeField] private bool fullscreenMode = true;

        private const string KeyWidth = "ResolutionWidth";
        private const string KeyHeight = "ResolutionHeight";

        private TMP_Dropdown dropdown;
        private List<Vector2Int> resolutions;

        private void Awake()
        {
            dropdown = GetComponent<TMP_Dropdown>();
            resolutions = Screen.resolutions
                .Select(r => new Vector2Int(r.width, r.height))
                .Distinct()
                .OrderByDescending(r => r.x * r.y)
                .ToList();

            dropdown.ClearOptions();
            dropdown.AddOptions(resolutions.Select(r => $"{r.x} x {r.y}").ToList());
        }

        private void Start()
        {
            int index = IndexOf(PlayerPrefs.GetInt(KeyWidth, Screen.currentResolution.width), PlayerPrefs.GetInt(KeyHeight, Screen.currentResolution.height));
            if (index < 0) index = IndexOf(Screen.currentResolution.width, Screen.currentResolution.height);
            if (index < 0) index = 0;

            dropdown.SetValueWithoutNotify(index);
            dropdown.RefreshShownValue();
            Apply(index);

            dropdown.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDestroy()
        {
            dropdown.onValueChanged.RemoveListener(OnValueChanged);
        }

        private int IndexOf(int width, int height) => resolutions.FindIndex(r => r.x == width && r.y == height);

        private void OnValueChanged(int index)
        {
            if (applyOnChange) Apply(index);
        }

        public void Apply(int index)
        {
            if (index < 0 || index >= resolutions.Count) return;

            var resolution = resolutions[index];
            Screen.SetResolution(resolution.x, resolution.y, fullscreenMode ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
            PlayerPrefs.SetInt(KeyWidth, resolution.x);
            PlayerPrefs.SetInt(KeyHeight, resolution.y);
            PlayerPrefs.Save();
        }

        public void ApplyCurrent() => Apply(dropdown.value);

        public void SetFullscreen(bool fullscreen)
        {
            fullscreenMode = fullscreen;
            Apply(dropdown.value);
        }

        public Vector2Int CurrentResolution => resolutions[Mathf.Clamp(dropdown.value, 0, resolutions.Count - 1)];
    }
}
