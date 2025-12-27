using System;
using System.Collections.Generic;
using UnityEngine;

namespace WaveIsland.Localization
{
    /// <summary>
    /// Localization system supporting multiple languages
    /// </summary>
    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private string defaultLanguage = "ru";
        [SerializeField] private string localizationFolder = "Localization";

        [Header("Supported Languages")]
        [SerializeField] private List<LanguageInfo> supportedLanguages = new List<LanguageInfo>
        {
            new LanguageInfo { code = "ru", displayName = "Русский", nativeName = "Русский" },
            new LanguageInfo { code = "en", displayName = "English", nativeName = "English" },
            new LanguageInfo { code = "uk", displayName = "Українська", nativeName = "Українська" },
            new LanguageInfo { code = "kk", displayName = "Қазақша", nativeName = "Қазақша" },
            new LanguageInfo { code = "uz", displayName = "O'zbek", nativeName = "O'zbekcha" },
            new LanguageInfo { code = "ky", displayName = "Кыргызча", nativeName = "Кыргызча" },
            new LanguageInfo { code = "az", displayName = "Azərbaycan", nativeName = "Azərbaycanca" },
            new LanguageInfo { code = "tr", displayName = "Türkçe", nativeName = "Türkçe" }
        };

        // Current state
        private string currentLanguage;
        private Dictionary<string, string> translations = new Dictionary<string, string>();
        private Dictionary<string, LocalizationData> cachedLanguages = new Dictionary<string, LocalizationData>();

        // Events
        public event Action<string> OnLanguageChanged;

        public string CurrentLanguage => currentLanguage;
        public List<LanguageInfo> SupportedLanguages => supportedLanguages;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            // Load saved language or detect system language
            string savedLang = PlayerPrefs.GetString("Language", "");

            if (string.IsNullOrEmpty(savedLang))
            {
                savedLang = DetectSystemLanguage();
            }

            SetLanguage(savedLang);
        }

        /// <summary>
        /// Detect system language and map to supported language
        /// </summary>
        private string DetectSystemLanguage()
        {
            SystemLanguage systemLang = Application.systemLanguage;

            string langCode = systemLang switch
            {
                SystemLanguage.Russian => "ru",
                SystemLanguage.English => "en",
                SystemLanguage.Ukrainian => "uk",
                SystemLanguage.Turkish => "tr",
                // Unity doesn't have direct support for Central Asian languages
                // Default to Russian for CIS region
                _ => defaultLanguage
            };

            // Check if detected language is supported
            if (!IsLanguageSupported(langCode))
            {
                langCode = defaultLanguage;
            }

            return langCode;
        }

        /// <summary>
        /// Check if language is supported
        /// </summary>
        public bool IsLanguageSupported(string langCode)
        {
            return supportedLanguages.Exists(l => l.code == langCode);
        }

        /// <summary>
        /// Set current language
        /// </summary>
        public void SetLanguage(string langCode)
        {
            if (!IsLanguageSupported(langCode))
            {
                Debug.LogWarning($"LocalizationManager: Language '{langCode}' not supported, using default");
                langCode = defaultLanguage;
            }

            if (currentLanguage == langCode && translations.Count > 0)
                return;

            currentLanguage = langCode;
            LoadLanguage(langCode);

            PlayerPrefs.SetString("Language", langCode);
            PlayerPrefs.Save();

            OnLanguageChanged?.Invoke(langCode);

            Debug.Log($"LocalizationManager: Language set to {langCode}");
        }

        /// <summary>
        /// Load language file
        /// </summary>
        private void LoadLanguage(string langCode)
        {
            // Check cache
            if (cachedLanguages.TryGetValue(langCode, out LocalizationData cached))
            {
                ApplyTranslations(cached);
                return;
            }

            // Load from resources
            string path = $"{localizationFolder}/{langCode}";
            TextAsset asset = Resources.Load<TextAsset>(path);

            if (asset == null)
            {
                Debug.LogError($"LocalizationManager: Failed to load language file: {path}");

                // Fallback to default
                if (langCode != defaultLanguage)
                {
                    LoadLanguage(defaultLanguage);
                }
                return;
            }

            try
            {
                LocalizationData data = JsonUtility.FromJson<LocalizationData>(asset.text);
                cachedLanguages[langCode] = data;
                ApplyTranslations(data);
            }
            catch (Exception e)
            {
                Debug.LogError($"LocalizationManager: Failed to parse language file: {e.Message}");
            }
        }

        private void ApplyTranslations(LocalizationData data)
        {
            translations.Clear();

            if (data.entries == null) return;

            foreach (var entry in data.entries)
            {
                translations[entry.key] = entry.value;
            }

            Debug.Log($"LocalizationManager: Loaded {translations.Count} translations");
        }

        /// <summary>
        /// Get translated string by key
        /// </summary>
        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key))
                return "";

            if (translations.TryGetValue(key, out string value))
            {
                return value;
            }

            // Key not found - return key as fallback
            Debug.LogWarning($"LocalizationManager: Missing translation for key: {key}");
            return key;
        }

        /// <summary>
        /// Get translated string with format arguments
        /// </summary>
        public string Get(string key, params object[] args)
        {
            string template = Get(key);

            try
            {
                return string.Format(template, args);
            }
            catch
            {
                return template;
            }
        }

        /// <summary>
        /// Get translated string with named placeholders
        /// </summary>
        public string GetNamed(string key, Dictionary<string, object> replacements)
        {
            string text = Get(key);

            foreach (var kvp in replacements)
            {
                text = text.Replace($"{{{kvp.Key}}}", kvp.Value?.ToString() ?? "");
            }

            return text;
        }

        /// <summary>
        /// Get plural form based on count
        /// </summary>
        public string GetPlural(string key, int count)
        {
            // Russian plural rules: 1, 2-4, 5-20, 21, 22-24, etc.
            string suffix = GetPluralSuffix(count, currentLanguage);
            string pluralKey = $"{key}_{suffix}";

            if (translations.ContainsKey(pluralKey))
            {
                return Get(pluralKey, count);
            }

            return Get(key, count);
        }

        private string GetPluralSuffix(int count, string lang)
        {
            if (lang == "ru" || lang == "uk")
            {
                // Slavic plural rules
                int lastTwo = count % 100;
                int lastOne = count % 10;

                if (lastTwo >= 11 && lastTwo <= 19)
                    return "many"; // 11-19
                if (lastOne == 1)
                    return "one"; // 1, 21, 31...
                if (lastOne >= 2 && lastOne <= 4)
                    return "few"; // 2-4, 22-24...
                return "many"; // 0, 5-20, 25-30...
            }
            else
            {
                // English-like rules
                return count == 1 ? "one" : "other";
            }
        }

        /// <summary>
        /// Get language display name
        /// </summary>
        public string GetLanguageName(string langCode)
        {
            var lang = supportedLanguages.Find(l => l.code == langCode);
            return lang != null ? lang.displayName : langCode;
        }

        /// <summary>
        /// Get current language info
        /// </summary>
        public LanguageInfo GetCurrentLanguageInfo()
        {
            return supportedLanguages.Find(l => l.code == currentLanguage);
        }

        /// <summary>
        /// Check if a key exists
        /// </summary>
        public bool HasKey(string key)
        {
            return translations.ContainsKey(key);
        }

        /// <summary>
        /// Get all keys (for debugging)
        /// </summary>
        public IEnumerable<string> GetAllKeys()
        {
            return translations.Keys;
        }

        /// <summary>
        /// Reload current language
        /// </summary>
        public void Reload()
        {
            cachedLanguages.Remove(currentLanguage);
            LoadLanguage(currentLanguage);
            OnLanguageChanged?.Invoke(currentLanguage);
        }
    }

    #region Data Classes

    [Serializable]
    public class LanguageInfo
    {
        public string code;
        public string displayName;
        public string nativeName;
    }

    [Serializable]
    public class LocalizationData
    {
        public string language;
        public string version;
        public List<LocalizationEntry> entries;
    }

    [Serializable]
    public class LocalizationEntry
    {
        public string key;
        public string value;
    }

    #endregion

    #region Static Helper

    /// <summary>
    /// Static shortcut for localization
    /// </summary>
    public static class L
    {
        public static string Get(string key) =>
            LocalizationManager.Instance?.Get(key) ?? key;

        public static string Get(string key, params object[] args) =>
            LocalizationManager.Instance?.Get(key, args) ?? key;

        public static string Plural(string key, int count) =>
            LocalizationManager.Instance?.GetPlural(key, count) ?? key;
    }

    #endregion
}
