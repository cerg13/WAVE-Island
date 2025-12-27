using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace WaveIsland.Core
{
    /// <summary>
    /// Manages graphics quality settings for optimal performance
    /// </summary>
    public class QualityManager : MonoBehaviour
    {
        public static QualityManager Instance { get; private set; }

        [Header("Quality Presets")]
        [SerializeField] private QualityPreset lowPreset;
        [SerializeField] private QualityPreset mediumPreset;
        [SerializeField] private QualityPreset highPreset;

        [Header("Auto Detection")]
        [SerializeField] private bool autoDetectQuality = true;
        [SerializeField] private int minFPSThreshold = 25;
        [SerializeField] private float qualityCheckInterval = 10f;

        // Current settings
        private QualityLevel currentLevel = QualityLevel.Medium;
        private QualityPreset currentPreset;
        private bool adaptiveQuality = false;

        // Performance tracking for adaptive quality
        private float[] fpsHistory = new float[10];
        private int fpsHistoryIndex = 0;
        private float lastQualityCheck;

        public event Action<QualityLevel> OnQualityChanged;

        public QualityLevel CurrentLevel => currentLevel;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDefaults();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Load saved quality or auto-detect
            int savedLevel = PlayerPrefs.GetInt("QualityLevel", -1);

            if (savedLevel >= 0)
            {
                SetQuality((QualityLevel)savedLevel);
            }
            else if (autoDetectQuality)
            {
                AutoDetectQuality();
            }
            else
            {
                SetQuality(QualityLevel.Medium);
            }
        }

        private void Update()
        {
            if (adaptiveQuality)
            {
                TrackFPS();
                CheckAdaptiveQuality();
            }
        }

        private void InitializeDefaults()
        {
            // Default presets if not assigned
            if (lowPreset == null)
            {
                lowPreset = new QualityPreset
                {
                    targetFPS = 30,
                    shadowQuality = ShadowQuality.Disable,
                    textureQuality = 2,
                    antiAliasing = 0,
                    particleLimit = 50,
                    renderScale = 0.75f,
                    lodBias = 0.5f,
                    enablePostProcessing = false,
                    enableBloom = false,
                    enableAmbientOcclusion = false
                };
            }

            if (mediumPreset == null)
            {
                mediumPreset = new QualityPreset
                {
                    targetFPS = 60,
                    shadowQuality = ShadowQuality.HardOnly,
                    textureQuality = 1,
                    antiAliasing = 2,
                    particleLimit = 100,
                    renderScale = 1f,
                    lodBias = 1f,
                    enablePostProcessing = true,
                    enableBloom = true,
                    enableAmbientOcclusion = false
                };
            }

            if (highPreset == null)
            {
                highPreset = new QualityPreset
                {
                    targetFPS = 60,
                    shadowQuality = ShadowQuality.All,
                    textureQuality = 0,
                    antiAliasing = 4,
                    particleLimit = 200,
                    renderScale = 1f,
                    lodBias = 1.5f,
                    enablePostProcessing = true,
                    enableBloom = true,
                    enableAmbientOcclusion = true
                };
            }
        }

        #region Quality Control

        /// <summary>
        /// Set quality level
        /// </summary>
        public void SetQuality(QualityLevel level)
        {
            currentLevel = level;

            currentPreset = level switch
            {
                QualityLevel.Low => lowPreset,
                QualityLevel.Medium => mediumPreset,
                QualityLevel.High => highPreset,
                _ => mediumPreset
            };

            ApplyPreset(currentPreset);

            PlayerPrefs.SetInt("QualityLevel", (int)level);
            PlayerPrefs.Save();

            OnQualityChanged?.Invoke(level);

            Debug.Log($"QualityManager: Quality set to {level}");
        }

        /// <summary>
        /// Auto-detect optimal quality based on device
        /// </summary>
        public void AutoDetectQuality()
        {
            QualityLevel recommended = QualityLevel.Medium;

            // Check system specs
            int ramGB = SystemInfo.systemMemorySize / 1024;
            int gpuMemMB = SystemInfo.graphicsMemorySize;
            int cpuCores = SystemInfo.processorCount;

            // High-end device
            if (ramGB >= 6 && gpuMemMB >= 2048 && cpuCores >= 6)
            {
                recommended = QualityLevel.High;
            }
            // Low-end device
            else if (ramGB < 3 || gpuMemMB < 1024 || cpuCores < 4)
            {
                recommended = QualityLevel.Low;
            }

            // Platform-specific adjustments
            #if UNITY_IOS
            // iOS devices generally handle high quality well
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal)
            {
                recommended = (QualityLevel)Mathf.Min((int)recommended + 1, 2);
            }
            #elif UNITY_ANDROID
            // Be more conservative on Android due to device variety
            recommended = (QualityLevel)Mathf.Max((int)recommended - 1, 0);
            #endif

            SetQuality(recommended);

            Debug.Log($"QualityManager: Auto-detected quality: {recommended} " +
                $"(RAM: {ramGB}GB, GPU: {gpuMemMB}MB, CPU: {cpuCores} cores)");
        }

        /// <summary>
        /// Apply quality preset
        /// </summary>
        private void ApplyPreset(QualityPreset preset)
        {
            // Target frame rate
            Application.targetFrameRate = preset.targetFPS;

            // Unity quality settings
            QualitySettings.shadowQuality = preset.shadowQuality;
            QualitySettings.globalTextureMipmapLimit = preset.textureQuality;
            QualitySettings.antiAliasing = preset.antiAliasing;
            QualitySettings.lodBias = preset.lodBias;

            // Render scale (if using URP)
            // Note: Requires URP-specific code
            // UniversalRenderPipeline.asset.renderScale = preset.renderScale;

            // Particle system limits
            ParticleSystem.maxParticleCount = preset.particleLimit;

            // V-Sync
            QualitySettings.vSyncCount = preset.targetFPS >= 60 ? 1 : 0;

            Debug.Log($"QualityManager: Applied preset - FPS: {preset.targetFPS}, " +
                $"Shadows: {preset.shadowQuality}, AA: {preset.antiAliasing}x");
        }

        #endregion

        #region Adaptive Quality

        /// <summary>
        /// Enable/disable adaptive quality
        /// </summary>
        public void SetAdaptiveQuality(bool enabled)
        {
            adaptiveQuality = enabled;
            PlayerPrefs.SetInt("AdaptiveQuality", enabled ? 1 : 0);
            PlayerPrefs.Save();

            Debug.Log($"QualityManager: Adaptive quality {(enabled ? "enabled" : "disabled")}");
        }

        private void TrackFPS()
        {
            float currentFPS = 1f / Time.unscaledDeltaTime;
            fpsHistory[fpsHistoryIndex] = currentFPS;
            fpsHistoryIndex = (fpsHistoryIndex + 1) % fpsHistory.Length;
        }

        private void CheckAdaptiveQuality()
        {
            if (Time.time - lastQualityCheck < qualityCheckInterval) return;

            lastQualityCheck = Time.time;

            float avgFPS = 0;
            for (int i = 0; i < fpsHistory.Length; i++)
                avgFPS += fpsHistory[i];
            avgFPS /= fpsHistory.Length;

            // Downgrade if FPS is too low
            if (avgFPS < minFPSThreshold && currentLevel > QualityLevel.Low)
            {
                SetQuality(currentLevel - 1);
                Debug.Log($"QualityManager: Adaptive downgrade to {currentLevel} (avg FPS: {avgFPS:F1})");
            }
            // Upgrade if FPS is stable at target
            else if (avgFPS >= currentPreset.targetFPS * 0.95f && currentLevel < QualityLevel.High)
            {
                // Only upgrade after consistent high performance
                SetQuality(currentLevel + 1);
                Debug.Log($"QualityManager: Adaptive upgrade to {currentLevel} (avg FPS: {avgFPS:F1})");
            }
        }

        #endregion

        #region Individual Settings

        /// <summary>
        /// Set shadows enabled
        /// </summary>
        public void SetShadowsEnabled(bool enabled)
        {
            QualitySettings.shadowQuality = enabled ? ShadowQuality.All : ShadowQuality.Disable;
            PlayerPrefs.SetInt("ShadowsEnabled", enabled ? 1 : 0);
        }

        /// <summary>
        /// Set anti-aliasing level (0, 2, 4, 8)
        /// </summary>
        public void SetAntiAliasing(int level)
        {
            QualitySettings.antiAliasing = Mathf.Clamp(level, 0, 8);
            PlayerPrefs.SetInt("AntiAliasing", level);
        }

        /// <summary>
        /// Set target frame rate
        /// </summary>
        public void SetTargetFrameRate(int fps)
        {
            Application.targetFrameRate = fps;
            PlayerPrefs.SetInt("TargetFPS", fps);
        }

        /// <summary>
        /// Set V-Sync enabled
        /// </summary>
        public void SetVSync(bool enabled)
        {
            QualitySettings.vSyncCount = enabled ? 1 : 0;
            PlayerPrefs.SetInt("VSync", enabled ? 1 : 0);
        }

        #endregion

        #region Battery Saver

        /// <summary>
        /// Enable battery saver mode (reduces quality to save battery)
        /// </summary>
        public void SetBatterySaverMode(bool enabled)
        {
            if (enabled)
            {
                Application.targetFrameRate = 30;
                QualitySettings.vSyncCount = 0;
                QualitySettings.shadowQuality = ShadowQuality.Disable;
                Screen.brightness = 0.5f;

                Debug.Log("QualityManager: Battery saver mode enabled");
            }
            else
            {
                SetQuality(currentLevel); // Restore quality settings
                Screen.brightness = 1f;

                Debug.Log("QualityManager: Battery saver mode disabled");
            }

            PlayerPrefs.SetInt("BatterySaver", enabled ? 1 : 0);
        }

        #endregion

        #region Queries

        /// <summary>
        /// Get quality level name
        /// </summary>
        public string GetQualityName(QualityLevel level)
        {
            return level switch
            {
                QualityLevel.Low => "Низкое",
                QualityLevel.Medium => "Среднее",
                QualityLevel.High => "Высокое",
                _ => "Среднее"
            };
        }

        /// <summary>
        /// Get current preset
        /// </summary>
        public QualityPreset GetCurrentPreset() => currentPreset;

        /// <summary>
        /// Check if device supports high quality
        /// </summary>
        public bool SupportsHighQuality()
        {
            return SystemInfo.systemMemorySize >= 4096 &&
                   SystemInfo.graphicsMemorySize >= 1024 &&
                   SystemInfo.processorCount >= 4;
        }

        #endregion
    }

    #region Data Classes

    public enum QualityLevel
    {
        Low = 0,
        Medium = 1,
        High = 2
    }

    [Serializable]
    public class QualityPreset
    {
        public int targetFPS = 60;
        public ShadowQuality shadowQuality = ShadowQuality.All;
        public int textureQuality = 0; // 0 = full, 1 = half, 2 = quarter
        public int antiAliasing = 4;
        public int particleLimit = 100;
        public float renderScale = 1f;
        public float lodBias = 1f;
        public bool enablePostProcessing = true;
        public bool enableBloom = true;
        public bool enableAmbientOcclusion = false;
    }

    #endregion
}
