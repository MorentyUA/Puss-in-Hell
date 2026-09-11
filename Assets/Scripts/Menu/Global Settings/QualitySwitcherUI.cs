using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GabrielBissonnette.SAD
{
    /// <summary>
    /// UI компонент для переключения уровня качества графики
    /// Можно использовать с кнопками или Dropdown
    /// </summary>
    public class QualitySwitcherUI : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Текст для отображения текущего уровня качества")]
        [SerializeField] TextMeshProUGUI qualityText;

        [Tooltip("Dropdown для выбора качества (опционально)")]
        [SerializeField] TMP_Dropdown qualityDropdown;

        [Tooltip("Кнопки для переключения качества (опционально)")]
        [SerializeField] Button nextButton;
        [SerializeField] Button previousButton;

        private void Start()
        {
            // Инициализируем UI
            InitializeUI();
            UpdateQualityText();
        }

        private void InitializeUI()
        {
            // Настраиваем Dropdown если есть
            if (qualityDropdown != null)
            {
                qualityDropdown.ClearOptions();
                qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(
                    GlobalSettingsManager.Instance.GetAllQualityLevels()
                ));
                qualityDropdown.value = GlobalSettingsManager.Instance.QualityLevel;
                qualityDropdown.onValueChanged.AddListener(OnDropdownChanged);
            }

            // Настраиваем кнопки если есть
            if (nextButton != null)
                nextButton.onClick.AddListener(NextQuality);

            if (previousButton != null)
                previousButton.onClick.AddListener(PreviousQuality);
        }

        /// <summary>
        /// Переключить на следующий уровень качества
        /// </summary>
        public void NextQuality()
        {
            GlobalSettingsManager.Instance.CycleQualityLevel();
            UpdateQualityText();
            UpdateDropdown();
        }

        /// <summary>
        /// Переключить на предыдущий уровень качества
        /// </summary>
        public void PreviousQuality()
        {
            GlobalSettingsManager.Instance.CycleQualityLevelBack();
            UpdateQualityText();
            UpdateDropdown();
        }

        /// <summary>
        /// Обработчик изменения Dropdown
        /// </summary>
        private void OnDropdownChanged(int index)
        {
            GlobalSettingsManager.Instance.QualityLevel = index;
            UpdateQualityText();
        }

        /// <summary>
        /// Обновить текст с текущим уровнем качества
        /// </summary>
        private void UpdateQualityText()
        {
            if (qualityText != null)
            {
                qualityText.text = GlobalSettingsManager.Instance.QualityLevelName;
            }
        }

        /// <summary>
        /// Обновить Dropdown (если используется)
        /// </summary>
        private void UpdateDropdown()
        {
            if (qualityDropdown != null)
            {
                qualityDropdown.value = GlobalSettingsManager.Instance.QualityLevel;
            }
        }

        /// <summary>
        /// Установить определенный уровень качества
        /// </summary>
        public void SetQuality(int level)
        {
            GlobalSettingsManager.Instance.QualityLevel = level;
            UpdateQualityText();
            UpdateDropdown();
        }

        private void OnDestroy()
        {
            // Очищаем слушатели
            if (qualityDropdown != null)
                qualityDropdown.onValueChanged.RemoveListener(OnDropdownChanged);

            if (nextButton != null)
                nextButton.onClick.RemoveListener(NextQuality);

            if (previousButton != null)
                previousButton.onClick.RemoveListener(PreviousQuality);
        }
    }
}
