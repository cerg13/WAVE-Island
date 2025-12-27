using UnityEngine;
using TMPro;

namespace WaveIsland.Localization
{
    /// <summary>
    /// Component for automatic text localization
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizedText : MonoBehaviour
    {
        [Header("Localization")]
        [SerializeField] private string localizationKey;
        [SerializeField] private bool updateOnEnable = true;

        [Header("Formatting")]
        [SerializeField] private bool useFormatting = false;
        [SerializeField] private string[] formatArgs;

        private TextMeshProUGUI textComponent;
        private string originalKey;

        private void Awake()
        {
            textComponent = GetComponent<TextMeshProUGUI>();
            originalKey = localizationKey;
        }

        private void OnEnable()
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
            }

            if (updateOnEnable)
            {
                UpdateText();
            }
        }

        private void OnDisable()
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
            }
        }

        private void OnLanguageChanged(string newLanguage)
        {
            UpdateText();
        }

        /// <summary>
        /// Update text with current localization
        /// </summary>
        public void UpdateText()
        {
            if (textComponent == null || string.IsNullOrEmpty(localizationKey))
                return;

            string text;

            if (useFormatting && formatArgs != null && formatArgs.Length > 0)
            {
                text = L.Get(localizationKey, formatArgs);
            }
            else
            {
                text = L.Get(localizationKey);
            }

            textComponent.text = text;
        }

        /// <summary>
        /// Set localization key at runtime
        /// </summary>
        public void SetKey(string key)
        {
            localizationKey = key;
            UpdateText();
        }

        /// <summary>
        /// Set format arguments and update
        /// </summary>
        public void SetArgs(params string[] args)
        {
            formatArgs = args;
            useFormatting = args != null && args.Length > 0;
            UpdateText();
        }

        /// <summary>
        /// Set key and arguments
        /// </summary>
        public void SetKeyWithArgs(string key, params string[] args)
        {
            localizationKey = key;
            SetArgs(args);
        }

        /// <summary>
        /// Reset to original key
        /// </summary>
        public void Reset()
        {
            localizationKey = originalKey;
            formatArgs = null;
            useFormatting = false;
            UpdateText();
        }

        public string Key => localizationKey;
    }

    /// <summary>
    /// Component for localizing multiple text elements
    /// </summary>
    public class LocalizedTextGroup : MonoBehaviour
    {
        [System.Serializable]
        public class TextEntry
        {
            public TextMeshProUGUI textComponent;
            public string localizationKey;
        }

        [SerializeField] private TextEntry[] entries;

        private void OnEnable()
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
            }

            UpdateAll();
        }

        private void OnDisable()
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
            }
        }

        private void OnLanguageChanged(string lang)
        {
            UpdateAll();
        }

        public void UpdateAll()
        {
            if (entries == null) return;

            foreach (var entry in entries)
            {
                if (entry.textComponent != null && !string.IsNullOrEmpty(entry.localizationKey))
                {
                    entry.textComponent.text = L.Get(entry.localizationKey);
                }
            }
        }
    }
}
