using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace GabrielBissonnette.SAD
{
    /// <summary>
    /// UI компонент для выбора разрешения экрана через Dropdown
    /// Автоматически подключается к GlobalSettingsManager
    /// </summary>
    [RequireComponent(typeof(TMP_Dropdown))]
    public class ResolutionDropdownUI : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool applyOnChange = true;
        [SerializeField] private bool fullscreenMode = true;

        private TMP_Dropdown dropdown;
        private Resolution[] resolutions;
        private List<Resolution> filteredResolutions;

        private void Awake()
        {
            dropdown = GetComponent<TMP_Dropdown>();
            InitializeResolutions();
        }

        private void Start()
        {
            // Загружаем сохраненное разрешение
            LoadCurrentResolution();

            // Подписываемся на изменение dropdown
            dropdown.onValueChanged.AddListener(OnResolutionChanged);
        }

        private void OnDestroy()
        {
            dropdown.onValueChanged.RemoveListener(OnResolutionChanged);
        }

        /// <summary>
        /// Инициализация списка разрешений
        /// </summary>
        private void InitializeResolutions()
        {
            // Получаем все доступные разрешения
            resolutions = Screen.resolutions;

            // Фильтруем дубликаты (одинаковые разрешения с разной частотой обновления)
            filteredResolutions = resolutions
                .Select(res => new Resolution { width = res.width, height = res.height })
                .Distinct()
                .OrderByDescending(res => res.width * res.height)
                .ToList();

            // Очищаем dropdown и добавляем новые опции
            dropdown.ClearOptions();

            List<string> options = new List<string>();
            foreach (var res in filteredResolutions)
            {
                string option = $"{res.width} x {res.height}";
                options.Add(option);
            }

            dropdown.AddOptions(options);
        }

        /// <summary>
        /// Загружает текущее разрешение из PlayerPrefs или устанавливает разрешение экрана
        /// </summary>
        private void LoadCurrentResolution()
        {
            int savedWidth = PlayerPrefs.GetInt("ResolutionWidth", Screen.currentResolution.width);
            int savedHeight = PlayerPrefs.GetInt("ResolutionHeight", Screen.currentResolution.height);

            // Находим индекс сохраненного разрешения
            int index = filteredResolutions.FindIndex(res =>
                res.width == savedWidth && res.height == savedHeight);

            // Если не нашли, ищем текущее разрешение экрана
            if (index == -1)
            {
                index = filteredResolutions.FindIndex(res =>
                    res.width == Screen.currentResolution.width &&
                    res.height == Screen.currentResolution.height);
            }

            // Если всё равно не нашли, берем первое (максимальное)
            if (index == -1)
                index = 0;

            dropdown.value = index;
            dropdown.RefreshShownValue();

            // Применяем разрешение
            ApplyResolution(index);
        }

        /// <summary>
        /// Вызывается при изменении выбранного разрешения
        /// </summary>
        private void OnResolutionChanged(int index)
        {
            if (applyOnChange)
            {
                ApplyResolution(index);
            }
        }

        /// <summary>
        /// Применяет выбранное разрешение
        /// </summary>
        public void ApplyResolution(int index)
        {
            if (index < 0 || index >= filteredResolutions.Count)
                return;

            Resolution resolution = filteredResolutions[index];

            // Применяем разрешение
            Screen.SetResolution(
                resolution.width,
                resolution.height,
                fullscreenMode ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed
            );

            // Сохраняем в PlayerPrefs
            SaveResolution(resolution.width, resolution.height);

        }

        /// <summary>
        /// Сохраняет разрешение в PlayerPrefs
        /// </summary>
        private void SaveResolution(int width, int height)
        {
            PlayerPrefs.SetInt("ResolutionWidth", width);
            PlayerPrefs.SetInt("ResolutionHeight", height);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Применить выбранное разрешение (можно вызвать из кнопки "Apply")
        /// </summary>
        public void ApplyCurrentResolution()
        {
            ApplyResolution(dropdown.value);
        }

        /// <summary>
        /// Переключение между полноэкранным и оконным режимом
        /// </summary>
        public void SetFullscreen(bool isFullscreen)
        {
            fullscreenMode = isFullscreen;
            ApplyResolution(dropdown.value);
        }

        /// <summary>
        /// Получить текущее выбранное разрешение
        /// </summary>
        public Resolution GetCurrentResolution()
        {
            return filteredResolutions[dropdown.value];
        }

        #region Debug Methods

        [ContextMenu("Print Current Resolution")]
        private void PrintCurrentResolution()
        {
            Resolution res = GetCurrentResolution();
        }

        [ContextMenu("Reset to Native Resolution")]
        private void ResetToNative()
        {
            int index = filteredResolutions.FindIndex(res =>
                res.width == Screen.currentResolution.width &&
                res.height == Screen.currentResolution.height);

            if (index != -1)
            {
                dropdown.value = index;
                ApplyResolution(index);
            }
        }

        #endregion
    }
}
