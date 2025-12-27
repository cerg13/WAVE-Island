using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WaveIsland.Localization;

namespace WaveIsland.UI.Settings
{
    /// <summary>
    /// UI for selecting game language
    /// </summary>
    public class LanguageSelector : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TMP_Dropdown languageDropdown;
        [SerializeField] private Button[] languageButtons;
        [SerializeField] private Transform languageListContainer;
        [SerializeField] private GameObject languageItemPrefab;

        [Header("Display")]
        [SerializeField] private TextMeshProUGUI currentLanguageText;
        [SerializeField] private Image currentLanguageFlag;

        [Header("Flags")]
        [SerializeField] private Sprite[] languageFlags;

        private List<GameObject> languageItems = new List<GameObject>();
        private Dictionary<string, Sprite> flagSprites = new Dictionary<string, Sprite>();

        private void Start()
        {
            // Map flags to language codes
            string[] codes = { "ru", "en", "uk", "kk", "uz", "ky", "az", "tr" };
            for (int i = 0; i < codes.Length && i < languageFlags.Length; i++)
            {
                flagSprites[codes[i]] = languageFlags[i];
            }

            // Subscribe to language changes
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
            }

            SetupUI();
            UpdateCurrentLanguageDisplay();
        }

        private void OnDestroy()
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
            }
        }

        private void SetupUI()
        {
            var manager = LocalizationManager.Instance;
            if (manager == null) return;

            var languages = manager.SupportedLanguages;

            // Setup dropdown if available
            if (languageDropdown != null)
            {
                SetupDropdown(languages);
            }

            // Setup list if available
            if (languageListContainer != null && languageItemPrefab != null)
            {
                SetupList(languages);
            }

            // Setup individual buttons if available
            if (languageButtons != null)
            {
                SetupButtons(languages);
            }
        }

        private void SetupDropdown(List<LanguageInfo> languages)
        {
            languageDropdown.ClearOptions();

            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

            foreach (var lang in languages)
            {
                var option = new TMP_Dropdown.OptionData
                {
                    text = lang.nativeName
                };

                if (flagSprites.TryGetValue(lang.code, out Sprite flag))
                {
                    option.image = flag;
                }

                options.Add(option);
            }

            languageDropdown.AddOptions(options);

            // Set current selection
            int currentIndex = languages.FindIndex(l => l.code == LocalizationManager.Instance.CurrentLanguage);
            if (currentIndex >= 0)
            {
                languageDropdown.value = currentIndex;
            }

            languageDropdown.onValueChanged.AddListener(OnDropdownChanged);
        }

        private void SetupList(List<LanguageInfo> languages)
        {
            // Clear existing
            foreach (var item in languageItems)
            {
                Destroy(item);
            }
            languageItems.Clear();

            // Create items
            foreach (var lang in languages)
            {
                CreateLanguageItem(lang);
            }
        }

        private void CreateLanguageItem(LanguageInfo lang)
        {
            GameObject item = Instantiate(languageItemPrefab, languageListContainer);
            languageItems.Add(item);

            // Set text
            TextMeshProUGUI nameText = item.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = lang.nativeName;
            }

            // Set flag
            Image flagImage = item.transform.Find("Flag")?.GetComponent<Image>();
            if (flagImage != null && flagSprites.TryGetValue(lang.code, out Sprite flag))
            {
                flagImage.sprite = flag;
            }

            // Set button
            Button button = item.GetComponent<Button>();
            if (button != null)
            {
                string code = lang.code; // Capture for closure
                button.onClick.AddListener(() => SelectLanguage(code));
            }

            // Highlight current
            Image background = item.GetComponent<Image>();
            if (background != null)
            {
                bool isCurrent = lang.code == LocalizationManager.Instance?.CurrentLanguage;
                background.color = isCurrent ? new Color(0.3f, 0.6f, 1f, 0.5f) : Color.white;
            }
        }

        private void SetupButtons(List<LanguageInfo> languages)
        {
            for (int i = 0; i < languageButtons.Length && i < languages.Length; i++)
            {
                var button = languageButtons[i];
                var lang = languages[i];

                if (button == null) continue;

                // Set text
                TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = lang.nativeName;
                }

                // Set click handler
                string code = lang.code;
                button.onClick.AddListener(() => SelectLanguage(code));
            }
        }

        private void OnDropdownChanged(int index)
        {
            var manager = LocalizationManager.Instance;
            if (manager == null) return;

            var languages = manager.SupportedLanguages;
            if (index >= 0 && index < languages.Count)
            {
                SelectLanguage(languages[index].code);
            }
        }

        public void SelectLanguage(string langCode)
        {
            var manager = LocalizationManager.Instance;
            if (manager == null) return;

            manager.SetLanguage(langCode);
        }

        private void OnLanguageChanged(string newLang)
        {
            UpdateCurrentLanguageDisplay();

            // Update list selection
            if (languageListContainer != null)
            {
                var manager = LocalizationManager.Instance;
                var languages = manager.SupportedLanguages;

                for (int i = 0; i < languageItems.Count && i < languages.Count; i++)
                {
                    Image background = languageItems[i].GetComponent<Image>();
                    if (background != null)
                    {
                        bool isCurrent = languages[i].code == newLang;
                        background.color = isCurrent ? new Color(0.3f, 0.6f, 1f, 0.5f) : Color.white;
                    }
                }
            }

            // Update dropdown
            if (languageDropdown != null)
            {
                var manager = LocalizationManager.Instance;
                int index = manager.SupportedLanguages.FindIndex(l => l.code == newLang);
                if (index >= 0)
                {
                    languageDropdown.SetValueWithoutNotify(index);
                }
            }
        }

        private void UpdateCurrentLanguageDisplay()
        {
            var manager = LocalizationManager.Instance;
            if (manager == null) return;

            var langInfo = manager.GetCurrentLanguageInfo();
            if (langInfo == null) return;

            if (currentLanguageText != null)
            {
                currentLanguageText.text = langInfo.nativeName;
            }

            if (currentLanguageFlag != null && flagSprites.TryGetValue(langInfo.code, out Sprite flag))
            {
                currentLanguageFlag.sprite = flag;
            }
        }

        /// <summary>
        /// Cycle to next language (for quick toggle)
        /// </summary>
        public void CycleLanguage()
        {
            var manager = LocalizationManager.Instance;
            if (manager == null) return;

            var languages = manager.SupportedLanguages;
            int currentIndex = languages.FindIndex(l => l.code == manager.CurrentLanguage);
            int nextIndex = (currentIndex + 1) % languages.Count;

            SelectLanguage(languages[nextIndex].code);
        }
    }
}
