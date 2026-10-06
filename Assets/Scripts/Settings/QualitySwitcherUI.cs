using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PussInHell.Settings
{
    public class QualitySwitcherUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI qualityText;
        [SerializeField] private TMP_Dropdown qualityDropdown;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button previousButton;

        private void Start()
        {
            if (qualityDropdown != null)
            {
                qualityDropdown.ClearOptions();
                qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
                qualityDropdown.SetValueWithoutNotify(CurrentLevel);
                qualityDropdown.RefreshShownValue();
                qualityDropdown.onValueChanged.AddListener(SetQuality);
            }

            if (nextButton != null) nextButton.onClick.AddListener(NextQuality);
            if (previousButton != null) previousButton.onClick.AddListener(PreviousQuality);

            Refresh();
        }

        private void OnDestroy()
        {
            if (qualityDropdown != null) qualityDropdown.onValueChanged.RemoveListener(SetQuality);
            if (nextButton != null) nextButton.onClick.RemoveListener(NextQuality);
            if (previousButton != null) previousButton.onClick.RemoveListener(PreviousQuality);
        }

        private static int CurrentLevel => GameSettings.Instance != null ? GameSettings.Instance.QualityLevel : QualitySettings.GetQualityLevel();

        public void NextQuality() => SetQuality((CurrentLevel + 1) % QualitySettings.names.Length);

        public void PreviousQuality() => SetQuality((CurrentLevel - 1 + QualitySettings.names.Length) % QualitySettings.names.Length);

        public void SetQuality(int level)
        {
            if (GameSettings.Instance != null) GameSettings.Instance.QualityLevel = level;
            else QualitySettings.SetQualityLevel(level, true);
            Refresh();
        }

        private void Refresh()
        {
            int level = CurrentLevel;
            if (qualityText != null) qualityText.text = QualitySettings.names[level];
            if (qualityDropdown != null)
            {
                qualityDropdown.SetValueWithoutNotify(level);
                qualityDropdown.RefreshShownValue();
            }
        }
    }
}
