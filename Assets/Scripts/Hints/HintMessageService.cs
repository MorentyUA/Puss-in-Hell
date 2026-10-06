using System;
using System.Collections.Generic;
using UnityEngine;
using PussInHell.Localization;

namespace PussInHell.Hints
{
    public class HintMessageService : MonoBehaviour
    {
        public static HintMessageService Instance { get; private set; }

        [SerializeField] private float defaultMessageDuration = 5f;
        [SerializeField] private List<LocalizedMessage> localizedMessages = new List<LocalizedMessage>();

        private readonly Dictionary<string, LocalizedMessage> messages = new Dictionary<string, LocalizedMessage>();

        public string CurrentKey { get; private set; }

        public event Action<string, float> MessageRequested;
        public event Action<string> MessageTextChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            transform.SetParent(null, true);
            DontDestroyOnLoad(gameObject);

            foreach (var message in localizedMessages)
                if (!string.IsNullOrEmpty(message.key) && !messages.ContainsKey(message.key))
                    messages[message.key] = message;
        }

        private void OnEnable()
        {
            LanguageProvider.Changed += OnLanguageChanged;
        }

        private void OnDisable()
        {
            LanguageProvider.Changed -= OnLanguageChanged;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Show(string key, float duration = -1f)
        {
            string text = GetText(key);
            if (string.IsNullOrEmpty(text)) return;

            CurrentKey = key;
            MessageRequested?.Invoke(text, duration < 0f ? defaultMessageDuration : duration);
        }

        public void ShowRaw(string text, float duration = -1f)
        {
            CurrentKey = null;
            MessageRequested?.Invoke(text, duration < 0f ? defaultMessageDuration : duration);
        }

        public void ClearCurrent()
        {
            CurrentKey = null;
        }

        public string GetText(string key)
        {
            return messages.TryGetValue(key, out var message) ? message.GetText(LanguageProvider.Current) : key;
        }

        private void OnLanguageChanged(GameLanguage language)
        {
            if (string.IsNullOrEmpty(CurrentKey)) return;
            MessageTextChanged?.Invoke(GetText(CurrentKey));
        }
    }
}
