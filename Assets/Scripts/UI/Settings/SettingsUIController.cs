using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WaveIsland.Core;

namespace WaveIsland.UI.Settings
{
    /// <summary>
    /// UI controller for game settings panel
    /// </summary>
    public class SettingsUIController : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Button closeButton;

        [Header("Tabs")]
        [SerializeField] private Transform tabContainer;
        [SerializeField] private Button audioTabButton;
        [SerializeField] private Button graphicsTabButton;
        [SerializeField] private Button gameplayTabButton;
        [SerializeField] private Button notificationsTabButton;
        [SerializeField] private Button accountTabButton;

        [Header("Tab Panels")]
        [SerializeField] private GameObject audioPanel;
        [SerializeField] private GameObject graphicsPanel;
        [SerializeField] private GameObject gameplayPanel;
        [SerializeField] private GameObject notificationsPanel;
        [SerializeField] private GameObject accountPanel;

        [Header("Audio Controls")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private TextMeshProUGUI masterVolumeText;
        [SerializeField] private Toggle masterMuteToggle;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private TextMeshProUGUI musicVolumeText;
        [SerializeField] private Toggle musicMuteToggle;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private TextMeshProUGUI sfxVolumeText;
        [SerializeField] private Toggle sfxMuteToggle;

        [Header("Graphics Controls")]
        [SerializeField] private TMP_Dropdown qualityDropdown;
        [SerializeField] private Toggle vSyncToggle;
        [SerializeField] private TMP_Dropdown frameRateDropdown;
        [SerializeField] private Toggle particlesToggle;
        [SerializeField] private Toggle shadowsToggle;
        [SerializeField] private Toggle postProcessingToggle;

        [Header("Gameplay Controls")]
        [SerializeField] private TMP_Dropdown languageDropdown;
        [SerializeField] private Toggle vibrationToggle;
        [SerializeField] private Toggle autoSaveToggle;
        [SerializeField] private Toggle tutorialHintsToggle;
        [SerializeField] private Toggle damageNumbersToggle;
        [SerializeField] private Slider cameraZoomSlider;
        [SerializeField] private TextMeshProUGUI cameraZoomText;
        [SerializeField] private Slider dragSensitivitySlider;
        [SerializeField] private TextMeshProUGUI dragSensitivityText;

        [Header("Notification Controls")]
        [SerializeField] private Toggle notificationsEnabledToggle;
        [SerializeField] private Toggle plantNotificationsToggle;
        [SerializeField] private Toggle dailyReminderToggle;
        [SerializeField] private Toggle eventNotificationsToggle;

        [Header("Account Controls")]
        [SerializeField] private TextMeshProUGUI playerIdText;
        [SerializeField] private TextMeshProUGUI versionText;
        [SerializeField] private Button linkAccountButton;
        [SerializeField] private Button logoutButton;
        [SerializeField] private Button deleteAccountButton;
        [SerializeField] private Toggle analyticsToggle;
        [SerializeField] private Toggle personalizedAdsToggle;

        [Header("Actions")]
        [SerializeField] private Button resetDefaultsButton;
        [SerializeField] private Button supportButton;
        [SerializeField] private Button privacyPolicyButton;
        [SerializeField] private Button termsOfServiceButton;

        [Header("Tab Colors")]
        [SerializeField] private Color selectedTabColor = Color.white;
        [SerializeField] private Color normalTabColor = new Color(0.7f, 0.7f, 0.7f);

        // Runtime
        private SettingsTab currentTab = SettingsTab.Audio;
        private List<Button> tabButtons = new List<Button>();
        private List<GameObject> tabPanels = new List<GameObject>();

        private enum SettingsTab
        {
            Audio,
            Graphics,
            Gameplay,
            Notifications,
            Account
        }

        private void Awake()
        {
            SetupButtons();
            SetupTabs();
        }

        private void Start()
        {
            Hide();
        }

        private void SetupButtons()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (resetDefaultsButton != null)
                resetDefaultsButton.onClick.AddListener(ResetToDefaults);

            if (supportButton != null)
                supportButton.onClick.AddListener(OpenSupport);

            if (privacyPolicyButton != null)
                privacyPolicyButton.onClick.AddListener(OpenPrivacyPolicy);

            if (termsOfServiceButton != null)
                termsOfServiceButton.onClick.AddListener(OpenTermsOfService);

            if (linkAccountButton != null)
                linkAccountButton.onClick.AddListener(LinkAccount);

            if (logoutButton != null)
                logoutButton.onClick.AddListener(Logout);

            if (deleteAccountButton != null)
                deleteAccountButton.onClick.AddListener(DeleteAccount);
        }

        private void SetupTabs()
        {
            tabButtons.Clear();
            tabPanels.Clear();

            if (audioTabButton != null)
            {
                tabButtons.Add(audioTabButton);
                audioTabButton.onClick.AddListener(() => SelectTab(SettingsTab.Audio));
            }
            if (graphicsTabButton != null)
            {
                tabButtons.Add(graphicsTabButton);
                graphicsTabButton.onClick.AddListener(() => SelectTab(SettingsTab.Graphics));
            }
            if (gameplayTabButton != null)
            {
                tabButtons.Add(gameplayTabButton);
                gameplayTabButton.onClick.AddListener(() => SelectTab(SettingsTab.Gameplay));
            }
            if (notificationsTabButton != null)
            {
                tabButtons.Add(notificationsTabButton);
                notificationsTabButton.onClick.AddListener(() => SelectTab(SettingsTab.Notifications));
            }
            if (accountTabButton != null)
            {
                tabButtons.Add(accountTabButton);
                accountTabButton.onClick.AddListener(() => SelectTab(SettingsTab.Account));
            }

            if (audioPanel != null) tabPanels.Add(audioPanel);
            if (graphicsPanel != null) tabPanels.Add(graphicsPanel);
            if (gameplayPanel != null) tabPanels.Add(gameplayPanel);
            if (notificationsPanel != null) tabPanels.Add(notificationsPanel);
            if (accountPanel != null) tabPanels.Add(accountPanel);
        }

        #region Show/Hide

        public void Show()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
                LoadCurrentSettings();
                SelectTab(SettingsTab.Audio);
            }
        }

        public void Hide()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }

        public void Toggle()
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
                Hide();
            else
                Show();
        }

        #endregion

        #region Tabs

        private void SelectTab(SettingsTab tab)
        {
            currentTab = tab;

            // Hide all panels
            foreach (var panel in tabPanels)
            {
                if (panel != null)
                    panel.SetActive(false);
            }

            // Show selected panel
            switch (tab)
            {
                case SettingsTab.Audio:
                    if (audioPanel != null) audioPanel.SetActive(true);
                    break;
                case SettingsTab.Graphics:
                    if (graphicsPanel != null) graphicsPanel.SetActive(true);
                    break;
                case SettingsTab.Gameplay:
                    if (gameplayPanel != null) gameplayPanel.SetActive(true);
                    break;
                case SettingsTab.Notifications:
                    if (notificationsPanel != null) notificationsPanel.SetActive(true);
                    break;
                case SettingsTab.Account:
                    if (accountPanel != null) accountPanel.SetActive(true);
                    break;
            }

            UpdateTabVisuals();
        }

        private void UpdateTabVisuals()
        {
            for (int i = 0; i < tabButtons.Count; i++)
            {
                var image = tabButtons[i].GetComponent<Image>();
                if (image != null)
                {
                    bool isSelected = i == (int)currentTab;
                    image.color = isSelected ? selectedTabColor : normalTabColor;
                }
            }
        }

        #endregion

        #region Load Settings

        private void LoadCurrentSettings()
        {
            var settings = SettingsManager.Instance;
            if (settings == null) return;

            // Audio
            SetupAudioControls(settings);

            // Graphics
            SetupGraphicsControls(settings);

            // Gameplay
            SetupGameplayControls(settings);

            // Notifications
            SetupNotificationControls(settings);

            // Account
            SetupAccountControls();
        }

        private void SetupAudioControls(SettingsManager settings)
        {
            // Master
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = settings.MasterVolume;
                masterVolumeSlider.onValueChanged.RemoveAllListeners();
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }
            UpdateVolumeText(masterVolumeText, settings.MasterVolume);

            if (masterMuteToggle != null)
            {
                masterMuteToggle.isOn = !settings.MasterMuted;
                masterMuteToggle.onValueChanged.RemoveAllListeners();
                masterMuteToggle.onValueChanged.AddListener(v => settings.MasterMuted = !v);
            }

            // Music
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = settings.MusicVolume;
                musicVolumeSlider.onValueChanged.RemoveAllListeners();
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }
            UpdateVolumeText(musicVolumeText, settings.MusicVolume);

            if (musicMuteToggle != null)
            {
                musicMuteToggle.isOn = !settings.MusicMuted;
                musicMuteToggle.onValueChanged.RemoveAllListeners();
                musicMuteToggle.onValueChanged.AddListener(v => settings.MusicMuted = !v);
            }

            // SFX
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = settings.SFXVolume;
                sfxVolumeSlider.onValueChanged.RemoveAllListeners();
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
            UpdateVolumeText(sfxVolumeText, settings.SFXVolume);

            if (sfxMuteToggle != null)
            {
                sfxMuteToggle.isOn = !settings.SFXMuted;
                sfxMuteToggle.onValueChanged.RemoveAllListeners();
                sfxMuteToggle.onValueChanged.AddListener(v => settings.SFXMuted = !v);
            }
        }

        private void SetupGraphicsControls(SettingsManager settings)
        {
            // Quality
            if (qualityDropdown != null)
            {
                qualityDropdown.ClearOptions();
                qualityDropdown.AddOptions(new List<string> { "Низкое", "Среднее", "Высокое", "Ультра" });
                qualityDropdown.value = (int)settings.GraphicsQuality;
                qualityDropdown.onValueChanged.RemoveAllListeners();
                qualityDropdown.onValueChanged.AddListener(v => settings.GraphicsQuality = (GraphicsQuality)v);
            }

            // VSync
            if (vSyncToggle != null)
            {
                vSyncToggle.isOn = settings.VSync;
                vSyncToggle.onValueChanged.RemoveAllListeners();
                vSyncToggle.onValueChanged.AddListener(v => settings.VSync = v);
            }

            // Frame Rate
            if (frameRateDropdown != null)
            {
                frameRateDropdown.ClearOptions();
                frameRateDropdown.AddOptions(new List<string> { "30 FPS", "60 FPS", "120 FPS" });
                int index = settings.TargetFrameRate switch
                {
                    30 => 0,
                    60 => 1,
                    120 => 2,
                    _ => 1
                };
                frameRateDropdown.value = index;
                frameRateDropdown.onValueChanged.RemoveAllListeners();
                frameRateDropdown.onValueChanged.AddListener(v =>
                {
                    int fps = v switch { 0 => 30, 1 => 60, 2 => 120, _ => 60 };
                    settings.TargetFrameRate = fps;
                });
            }

            // Particles
            if (particlesToggle != null)
            {
                particlesToggle.isOn = settings.ParticleEffects;
                particlesToggle.onValueChanged.RemoveAllListeners();
                particlesToggle.onValueChanged.AddListener(v => settings.ParticleEffects = v);
            }

            // Shadows
            if (shadowsToggle != null)
            {
                shadowsToggle.isOn = settings.Shadows;
                shadowsToggle.onValueChanged.RemoveAllListeners();
                shadowsToggle.onValueChanged.AddListener(v => settings.Shadows = v);
            }

            // Post Processing
            if (postProcessingToggle != null)
            {
                postProcessingToggle.isOn = settings.PostProcessing;
                postProcessingToggle.onValueChanged.RemoveAllListeners();
                postProcessingToggle.onValueChanged.AddListener(v => settings.PostProcessing = v);
            }
        }

        private void SetupGameplayControls(SettingsManager settings)
        {
            // Language
            if (languageDropdown != null)
            {
                var languages = settings.GetAvailableLanguages();
                languageDropdown.ClearOptions();
                List<string> options = new List<string>();
                int selectedIndex = 0;
                for (int i = 0; i < languages.Length; i++)
                {
                    options.Add(settings.GetLanguageDisplayName(languages[i]));
                    if (languages[i] == settings.Language)
                        selectedIndex = i;
                }
                languageDropdown.AddOptions(options);
                languageDropdown.value = selectedIndex;
                languageDropdown.onValueChanged.RemoveAllListeners();
                languageDropdown.onValueChanged.AddListener(v => settings.Language = languages[v]);
            }

            // Vibration
            if (vibrationToggle != null)
            {
                vibrationToggle.interactable = settings.IsVibrationSupported();
                vibrationToggle.isOn = settings.Vibration;
                vibrationToggle.onValueChanged.RemoveAllListeners();
                vibrationToggle.onValueChanged.AddListener(v => settings.Vibration = v);
            }

            // Auto Save
            if (autoSaveToggle != null)
            {
                autoSaveToggle.isOn = settings.AutoSave;
                autoSaveToggle.onValueChanged.RemoveAllListeners();
                autoSaveToggle.onValueChanged.AddListener(v => settings.AutoSave = v);
            }

            // Tutorial Hints
            if (tutorialHintsToggle != null)
            {
                tutorialHintsToggle.isOn = settings.ShowTutorialHints;
                tutorialHintsToggle.onValueChanged.RemoveAllListeners();
                tutorialHintsToggle.onValueChanged.AddListener(v => settings.ShowTutorialHints = v);
            }

            // Damage Numbers
            if (damageNumbersToggle != null)
            {
                damageNumbersToggle.isOn = settings.ShowDamageNumbers;
                damageNumbersToggle.onValueChanged.RemoveAllListeners();
                damageNumbersToggle.onValueChanged.AddListener(v => settings.ShowDamageNumbers = v);
            }

            // Camera Zoom
            if (cameraZoomSlider != null)
            {
                cameraZoomSlider.minValue = 0.1f;
                cameraZoomSlider.maxValue = 2f;
                cameraZoomSlider.value = settings.CameraZoomSensitivity;
                cameraZoomSlider.onValueChanged.RemoveAllListeners();
                cameraZoomSlider.onValueChanged.AddListener(OnCameraZoomChanged);
            }
            UpdateSensitivityText(cameraZoomText, settings.CameraZoomSensitivity);

            // Drag Sensitivity
            if (dragSensitivitySlider != null)
            {
                dragSensitivitySlider.minValue = 0.1f;
                dragSensitivitySlider.maxValue = 2f;
                dragSensitivitySlider.value = settings.DragSensitivity;
                dragSensitivitySlider.onValueChanged.RemoveAllListeners();
                dragSensitivitySlider.onValueChanged.AddListener(OnDragSensitivityChanged);
            }
            UpdateSensitivityText(dragSensitivityText, settings.DragSensitivity);
        }

        private void SetupNotificationControls(SettingsManager settings)
        {
            // Master toggle
            if (notificationsEnabledToggle != null)
            {
                notificationsEnabledToggle.isOn = settings.NotificationsEnabled;
                notificationsEnabledToggle.onValueChanged.RemoveAllListeners();
                notificationsEnabledToggle.onValueChanged.AddListener(v =>
                {
                    settings.NotificationsEnabled = v;
                    UpdateNotificationSubtoggles(v);
                });
            }

            // Plant notifications
            if (plantNotificationsToggle != null)
            {
                plantNotificationsToggle.isOn = settings.PlantNotifications;
                plantNotificationsToggle.interactable = settings.NotificationsEnabled;
                plantNotificationsToggle.onValueChanged.RemoveAllListeners();
                plantNotificationsToggle.onValueChanged.AddListener(v => settings.PlantNotifications = v);
            }

            // Daily reminder
            if (dailyReminderToggle != null)
            {
                dailyReminderToggle.isOn = settings.DailyReminderNotifications;
                dailyReminderToggle.interactable = settings.NotificationsEnabled;
                dailyReminderToggle.onValueChanged.RemoveAllListeners();
                dailyReminderToggle.onValueChanged.AddListener(v => settings.DailyReminderNotifications = v);
            }

            // Event notifications
            if (eventNotificationsToggle != null)
            {
                eventNotificationsToggle.isOn = settings.EventNotifications;
                eventNotificationsToggle.interactable = settings.NotificationsEnabled;
                eventNotificationsToggle.onValueChanged.RemoveAllListeners();
                eventNotificationsToggle.onValueChanged.AddListener(v => settings.EventNotifications = v);
            }
        }

        private void UpdateNotificationSubtoggles(bool enabled)
        {
            if (plantNotificationsToggle != null)
                plantNotificationsToggle.interactable = enabled;
            if (dailyReminderToggle != null)
                dailyReminderToggle.interactable = enabled;
            if (eventNotificationsToggle != null)
                eventNotificationsToggle.interactable = enabled;
        }

        private void SetupAccountControls()
        {
            var settings = SettingsManager.Instance;

            // Player ID
            if (playerIdText != null)
            {
                string playerId = PlayerPrefs.GetString("PlayerId", "Гость");
                playerIdText.text = $"ID: {playerId}";
            }

            // Version
            if (versionText != null)
            {
                versionText.text = $"Версия: {Application.version}";
            }

            // Analytics
            if (analyticsToggle != null && settings != null)
            {
                analyticsToggle.isOn = settings.AnalyticsEnabled;
                analyticsToggle.onValueChanged.RemoveAllListeners();
                analyticsToggle.onValueChanged.AddListener(v => settings.AnalyticsEnabled = v);
            }

            // Personalized Ads
            if (personalizedAdsToggle != null && settings != null)
            {
                personalizedAdsToggle.isOn = settings.PersonalizedAds;
                personalizedAdsToggle.onValueChanged.RemoveAllListeners();
                personalizedAdsToggle.onValueChanged.AddListener(v => settings.PersonalizedAds = v);
            }
        }

        #endregion

        #region Value Changed Handlers

        private void OnMasterVolumeChanged(float value)
        {
            var settings = SettingsManager.Instance;
            if (settings != null)
                settings.MasterVolume = value;
            UpdateVolumeText(masterVolumeText, value);
        }

        private void OnMusicVolumeChanged(float value)
        {
            var settings = SettingsManager.Instance;
            if (settings != null)
                settings.MusicVolume = value;
            UpdateVolumeText(musicVolumeText, value);
        }

        private void OnSFXVolumeChanged(float value)
        {
            var settings = SettingsManager.Instance;
            if (settings != null)
                settings.SFXVolume = value;
            UpdateVolumeText(sfxVolumeText, value);
        }

        private void OnCameraZoomChanged(float value)
        {
            var settings = SettingsManager.Instance;
            if (settings != null)
                settings.CameraZoomSensitivity = value;
            UpdateSensitivityText(cameraZoomText, value);
        }

        private void OnDragSensitivityChanged(float value)
        {
            var settings = SettingsManager.Instance;
            if (settings != null)
                settings.DragSensitivity = value;
            UpdateSensitivityText(dragSensitivityText, value);
        }

        private void UpdateVolumeText(TextMeshProUGUI text, float value)
        {
            if (text != null)
                text.text = $"{Mathf.RoundToInt(value * 100)}%";
        }

        private void UpdateSensitivityText(TextMeshProUGUI text, float value)
        {
            if (text != null)
                text.text = $"{value:F1}x";
        }

        #endregion

        #region Actions

        private void ResetToDefaults()
        {
            var settings = SettingsManager.Instance;
            if (settings != null)
            {
                settings.ResetToDefaults();
                LoadCurrentSettings();
                Debug.Log("SettingsUI: Reset to defaults");
            }
        }

        private void OpenSupport()
        {
            Application.OpenURL("https://waveisland.game/support");
        }

        private void OpenPrivacyPolicy()
        {
            Application.OpenURL("https://waveisland.game/privacy");
        }

        private void OpenTermsOfService()
        {
            Application.OpenURL("https://waveisland.game/terms");
        }

        private void LinkAccount()
        {
            Debug.Log("SettingsUI: Link account requested");
            // TODO: Implement account linking (Google Play, Game Center, etc.)
        }

        private void Logout()
        {
            Debug.Log("SettingsUI: Logout requested");
            // TODO: Implement logout
        }

        private void DeleteAccount()
        {
            Debug.Log("SettingsUI: Delete account requested");
            // TODO: Show confirmation dialog and implement account deletion
        }

        #endregion
    }
}
