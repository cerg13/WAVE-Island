using System;
using UnityEngine;
using UnityEngine.Audio;

namespace WaveIsland.Core
{
    /// <summary>
    /// Manages game settings - audio, graphics, language, controls
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }

        [Header("Events")]
        public event Action OnSettingsChanged;
        public event Action<string> OnLanguageChanged;

        [Header("Audio")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private string masterVolumeParam = "MasterVolume";
        [SerializeField] private string musicVolumeParam = "MusicVolume";
        [SerializeField] private string sfxVolumeParam = "SFXVolume";

        // Current settings
        private GameSettings currentSettings;

        // Keys for PlayerPrefs
        private const string SETTINGS_KEY = "GameSettings";

        public GameSettings Settings => currentSettings;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadSettings();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            ApplyAllSettings();
        }

        #region Audio Settings

        public float MasterVolume
        {
            get => currentSettings.masterVolume;
            set
            {
                currentSettings.masterVolume = Mathf.Clamp01(value);
                ApplyAudioSettings();
                SaveSettings();
            }
        }

        public float MusicVolume
        {
            get => currentSettings.musicVolume;
            set
            {
                currentSettings.musicVolume = Mathf.Clamp01(value);
                ApplyAudioSettings();
                SaveSettings();
            }
        }

        public float SFXVolume
        {
            get => currentSettings.sfxVolume;
            set
            {
                currentSettings.sfxVolume = Mathf.Clamp01(value);
                ApplyAudioSettings();
                SaveSettings();
            }
        }

        public bool MasterMuted
        {
            get => currentSettings.masterMuted;
            set
            {
                currentSettings.masterMuted = value;
                ApplyAudioSettings();
                SaveSettings();
            }
        }

        public bool MusicMuted
        {
            get => currentSettings.musicMuted;
            set
            {
                currentSettings.musicMuted = value;
                ApplyAudioSettings();
                SaveSettings();
            }
        }

        public bool SFXMuted
        {
            get => currentSettings.sfxMuted;
            set
            {
                currentSettings.sfxMuted = value;
                ApplyAudioSettings();
                SaveSettings();
            }
        }

        private void ApplyAudioSettings()
        {
            if (audioMixer == null) return;

            // Convert 0-1 to decibels (-80 to 0)
            float masterDb = currentSettings.masterMuted ? -80f : VolumeToDecibels(currentSettings.masterVolume);
            float musicDb = currentSettings.musicMuted ? -80f : VolumeToDecibels(currentSettings.musicVolume);
            float sfxDb = currentSettings.sfxMuted ? -80f : VolumeToDecibels(currentSettings.sfxVolume);

            audioMixer.SetFloat(masterVolumeParam, masterDb);
            audioMixer.SetFloat(musicVolumeParam, musicDb);
            audioMixer.SetFloat(sfxVolumeParam, sfxDb);
        }

        private float VolumeToDecibels(float volume)
        {
            // Logarithmic conversion for natural volume perception
            if (volume <= 0.0001f) return -80f;
            return Mathf.Log10(volume) * 20f;
        }

        #endregion

        #region Graphics Settings

        public GraphicsQuality GraphicsQuality
        {
            get => currentSettings.graphicsQuality;
            set
            {
                currentSettings.graphicsQuality = value;
                ApplyGraphicsSettings();
                SaveSettings();
            }
        }

        public bool VSync
        {
            get => currentSettings.vSync;
            set
            {
                currentSettings.vSync = value;
                ApplyGraphicsSettings();
                SaveSettings();
            }
        }

        public int TargetFrameRate
        {
            get => currentSettings.targetFrameRate;
            set
            {
                currentSettings.targetFrameRate = value;
                ApplyGraphicsSettings();
                SaveSettings();
            }
        }

        public bool ParticleEffects
        {
            get => currentSettings.particleEffects;
            set
            {
                currentSettings.particleEffects = value;
                SaveSettings();
                OnSettingsChanged?.Invoke();
            }
        }

        public bool Shadows
        {
            get => currentSettings.shadows;
            set
            {
                currentSettings.shadows = value;
                ApplyGraphicsSettings();
                SaveSettings();
            }
        }

        public bool PostProcessing
        {
            get => currentSettings.postProcessing;
            set
            {
                currentSettings.postProcessing = value;
                SaveSettings();
                OnSettingsChanged?.Invoke();
            }
        }

        private void ApplyGraphicsSettings()
        {
            // Quality level
            int qualityLevel = currentSettings.graphicsQuality switch
            {
                GraphicsQuality.Low => 0,
                GraphicsQuality.Medium => 2,
                GraphicsQuality.High => 4,
                GraphicsQuality.Ultra => 5,
                _ => 2
            };
            QualitySettings.SetQualityLevel(qualityLevel, true);

            // VSync
            QualitySettings.vSyncCount = currentSettings.vSync ? 1 : 0;

            // Frame rate
            Application.targetFrameRate = currentSettings.targetFrameRate;

            // Shadows
            if (currentSettings.shadows)
            {
                QualitySettings.shadows = ShadowQuality.All;
            }
            else
            {
                QualitySettings.shadows = ShadowQuality.Disable;
            }

            OnSettingsChanged?.Invoke();
        }

        #endregion

        #region Gameplay Settings

        public string Language
        {
            get => currentSettings.language;
            set
            {
                currentSettings.language = value;
                SaveSettings();
                OnLanguageChanged?.Invoke(value);
            }
        }

        public bool Vibration
        {
            get => currentSettings.vibration;
            set
            {
                currentSettings.vibration = value;
                SaveSettings();
            }
        }

        public bool AutoSave
        {
            get => currentSettings.autoSave;
            set
            {
                currentSettings.autoSave = value;
                SaveSettings();
            }
        }

        public bool ShowTutorialHints
        {
            get => currentSettings.showTutorialHints;
            set
            {
                currentSettings.showTutorialHints = value;
                SaveSettings();
            }
        }

        public bool ShowDamageNumbers
        {
            get => currentSettings.showDamageNumbers;
            set
            {
                currentSettings.showDamageNumbers = value;
                SaveSettings();
            }
        }

        public float CameraZoomSensitivity
        {
            get => currentSettings.cameraZoomSensitivity;
            set
            {
                currentSettings.cameraZoomSensitivity = Mathf.Clamp(value, 0.1f, 2f);
                SaveSettings();
            }
        }

        public float DragSensitivity
        {
            get => currentSettings.dragSensitivity;
            set
            {
                currentSettings.dragSensitivity = Mathf.Clamp(value, 0.1f, 2f);
                SaveSettings();
            }
        }

        #endregion

        #region Notification Settings

        public bool NotificationsEnabled
        {
            get => currentSettings.notificationsEnabled;
            set
            {
                currentSettings.notificationsEnabled = value;
                SaveSettings();
            }
        }

        public bool PlantNotifications
        {
            get => currentSettings.plantNotifications;
            set
            {
                currentSettings.plantNotifications = value;
                SaveSettings();
            }
        }

        public bool DailyReminderNotifications
        {
            get => currentSettings.dailyReminderNotifications;
            set
            {
                currentSettings.dailyReminderNotifications = value;
                SaveSettings();
            }
        }

        public bool EventNotifications
        {
            get => currentSettings.eventNotifications;
            set
            {
                currentSettings.eventNotifications = value;
                SaveSettings();
            }
        }

        #endregion

        #region Privacy Settings

        public bool AnalyticsEnabled
        {
            get => currentSettings.analyticsEnabled;
            set
            {
                currentSettings.analyticsEnabled = value;
                SaveSettings();
            }
        }

        public bool PersonalizedAds
        {
            get => currentSettings.personalizedAds;
            set
            {
                currentSettings.personalizedAds = value;
                SaveSettings();
            }
        }

        #endregion

        #region Apply All

        private void ApplyAllSettings()
        {
            ApplyAudioSettings();
            ApplyGraphicsSettings();
            OnSettingsChanged?.Invoke();
        }

        #endregion

        #region Save/Load

        private void SaveSettings()
        {
            string json = JsonUtility.ToJson(currentSettings);
            PlayerPrefs.SetString(SETTINGS_KEY, json);
            PlayerPrefs.Save();
        }

        private void LoadSettings()
        {
            if (PlayerPrefs.HasKey(SETTINGS_KEY))
            {
                try
                {
                    string json = PlayerPrefs.GetString(SETTINGS_KEY);
                    currentSettings = JsonUtility.FromJson<GameSettings>(json);
                    Debug.Log("SettingsManager: Loaded settings");
                }
                catch (Exception e)
                {
                    Debug.LogError($"SettingsManager: Failed to load settings: {e.Message}");
                    currentSettings = GetDefaultSettings();
                }
            }
            else
            {
                currentSettings = GetDefaultSettings();
                Debug.Log("SettingsManager: Created default settings");
            }
        }

        private GameSettings GetDefaultSettings()
        {
            return new GameSettings
            {
                // Audio
                masterVolume = 1f,
                musicVolume = 0.7f,
                sfxVolume = 1f,
                masterMuted = false,
                musicMuted = false,
                sfxMuted = false,

                // Graphics
                graphicsQuality = GraphicsQuality.Medium,
                vSync = true,
                targetFrameRate = 60,
                particleEffects = true,
                shadows = true,
                postProcessing = true,

                // Gameplay
                language = "ru",
                vibration = true,
                autoSave = true,
                showTutorialHints = true,
                showDamageNumbers = true,
                cameraZoomSensitivity = 1f,
                dragSensitivity = 1f,

                // Notifications
                notificationsEnabled = true,
                plantNotifications = true,
                dailyReminderNotifications = true,
                eventNotifications = true,

                // Privacy
                analyticsEnabled = true,
                personalizedAds = true
            };
        }

        /// <summary>
        /// Reset all settings to defaults
        /// </summary>
        public void ResetToDefaults()
        {
            currentSettings = GetDefaultSettings();
            ApplyAllSettings();
            SaveSettings();
            Debug.Log("SettingsManager: Reset to defaults");
        }

        #endregion

        #region Utility

        /// <summary>
        /// Check if device supports vibration
        /// </summary>
        public bool IsVibrationSupported()
        {
#if UNITY_ANDROID || UNITY_IOS
            return true;
#else
            return false;
#endif
        }

        /// <summary>
        /// Trigger haptic feedback if enabled
        /// </summary>
        public void TriggerHaptic()
        {
            if (!currentSettings.vibration) return;

#if UNITY_ANDROID
            Handheld.Vibrate();
#elif UNITY_IOS
            // iOS haptic feedback would use native plugin
#endif
        }

        /// <summary>
        /// Get available languages
        /// </summary>
        public string[] GetAvailableLanguages()
        {
            return new[] { "ru", "en", "zh", "es", "de", "fr", "ja", "ko" };
        }

        /// <summary>
        /// Get language display name
        /// </summary>
        public string GetLanguageDisplayName(string code)
        {
            return code switch
            {
                "ru" => "Русский",
                "en" => "English",
                "zh" => "中文",
                "es" => "Español",
                "de" => "Deutsch",
                "fr" => "Français",
                "ja" => "日本語",
                "ko" => "한국어",
                _ => code
            };
        }

        #endregion
    }

    #region Data Classes

    public enum GraphicsQuality
    {
        Low,
        Medium,
        High,
        Ultra
    }

    [Serializable]
    public class GameSettings
    {
        // Audio
        public float masterVolume;
        public float musicVolume;
        public float sfxVolume;
        public bool masterMuted;
        public bool musicMuted;
        public bool sfxMuted;

        // Graphics
        public GraphicsQuality graphicsQuality;
        public bool vSync;
        public int targetFrameRate;
        public bool particleEffects;
        public bool shadows;
        public bool postProcessing;

        // Gameplay
        public string language;
        public bool vibration;
        public bool autoSave;
        public bool showTutorialHints;
        public bool showDamageNumbers;
        public float cameraZoomSensitivity;
        public float dragSensitivity;

        // Notifications
        public bool notificationsEnabled;
        public bool plantNotifications;
        public bool dailyReminderNotifications;
        public bool eventNotifications;

        // Privacy
        public bool analyticsEnabled;
        public bool personalizedAds;
    }

    #endregion
}
