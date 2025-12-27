using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WaveIsland.Core
{
    /// <summary>
    /// Game initialization and bootstrap sequence
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap Instance { get; private set; }

        [Header("Bootstrap Settings")]
        [SerializeField] private bool skipIntro = false;
        [SerializeField] private string mainMenuScene = "MainMenu";
        [SerializeField] private string gameScene = "Game";

        [Header("Manager Prefabs")]
        [SerializeField] private GameObject gameManagerPrefab;
        [SerializeField] private GameObject audioManagerPrefab;
        [SerializeField] private GameObject saveManagerPrefab;
        [SerializeField] private GameObject localizationManagerPrefab;
        [SerializeField] private GameObject analyticsManagerPrefab;
        [SerializeField] private GameObject qualityManagerPrefab;
        [SerializeField] private GameObject errorHandlerPrefab;
        [SerializeField] private GameObject poolManagerPrefab;
        [SerializeField] private GameObject performanceMonitorPrefab;
        [SerializeField] private GameObject notificationManagerPrefab;

        [Header("UI Prefabs")]
        [SerializeField] private GameObject loadingScreenPrefab;

        [Header("Debug")]
        [SerializeField] private bool enableDebugMode = false;

        // Initialization state
        private bool isInitialized = false;
        private float initProgress = 0f;

        // Events
        public event Action<float> OnInitProgress;
        public event Action OnInitComplete;
        public event Action<string> OnInitError;

        public bool IsInitialized => isInitialized;
        public float InitProgress => initProgress;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            // Auto-create bootstrap if not exists
            if (Instance == null)
            {
                GameObject bootstrapObj = new GameObject("GameBootstrap");
                bootstrapObj.AddComponent<GameBootstrap>();
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                StartCoroutine(InitializeGame());
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #region Initialization

        private IEnumerator InitializeGame()
        {
            Debug.Log("GameBootstrap: Starting initialization...");
            float startTime = Time.realtimeSinceStartup;

            // Phase 1: Core Systems
            UpdateProgress(0.1f, "Инициализация ядра...");
            yield return InitializeCoreManagers();

            // Phase 2: Load Settings
            UpdateProgress(0.3f, "Загрузка настроек...");
            yield return LoadSettings();

            // Phase 3: Initialize Services
            UpdateProgress(0.5f, "Подключение сервисов...");
            yield return InitializeServices();

            // Phase 4: Load Data
            UpdateProgress(0.7f, "Загрузка данных...");
            yield return LoadGameData();

            // Phase 5: Final Setup
            UpdateProgress(0.9f, "Финальная настройка...");
            yield return FinalSetup();

            // Complete
            UpdateProgress(1f, "Готово!");

            float totalTime = Time.realtimeSinceStartup - startTime;
            Debug.Log($"GameBootstrap: Initialization complete in {totalTime:F2}s");

            isInitialized = true;
            OnInitComplete?.Invoke();

            // Load main scene
            if (!skipIntro)
            {
                yield return new WaitForSeconds(0.5f);
                LoadMainMenu();
            }
        }

        private IEnumerator InitializeCoreManagers()
        {
            // Error Handler (first, to catch any errors)
            SpawnManager(errorHandlerPrefab, "ErrorHandler");
            yield return null;

            // Pool Manager
            SpawnManager(poolManagerPrefab, "PoolManager");
            yield return null;

            // Performance Monitor
            if (enableDebugMode)
            {
                SpawnManager(performanceMonitorPrefab, "PerformanceMonitor");
            }
            yield return null;

            // Quality Manager
            SpawnManager(qualityManagerPrefab, "QualityManager");
            yield return null;

            // Audio Manager
            SpawnManager(audioManagerPrefab, "AudioManager");
            yield return null;

            // Localization Manager
            SpawnManager(localizationManagerPrefab, "LocalizationManager");
            yield return null;

            // Loading Screen (UI)
            SpawnManager(loadingScreenPrefab, "LoadingScreen");
            yield return null;
        }

        private IEnumerator LoadSettings()
        {
            // Quality settings
            var quality = QualityManager.Instance;
            if (quality != null)
            {
                // Auto-detect quality on first run
                if (!PlayerPrefs.HasKey("QualityLevel"))
                {
                    quality.AutoDetectQuality();
                }
            }
            yield return null;

            // Audio settings (loaded automatically in AudioManager)
            yield return null;

            // Language settings (loaded automatically in LocalizationManager)
            yield return null;
        }

        private IEnumerator InitializeServices()
        {
            // Save Manager
            SpawnManager(saveManagerPrefab, "SaveManager");
            yield return null;

            // Analytics Manager
            SpawnManager(analyticsManagerPrefab, "AnalyticsManager");
            yield return null;

            // Notification Manager
            SpawnManager(notificationManagerPrefab, "NotificationManager");
            yield return null;

            // Game Manager
            SpawnManager(gameManagerPrefab, "GameManager");
            yield return null;
        }

        private IEnumerator LoadGameData()
        {
            // Load saved game
            var saveManager = SaveManager.Instance;
            if (saveManager != null)
            {
                saveManager.Load();
            }
            yield return null;

            // Load game databases
            yield return LoadDatabases();
        }

        private IEnumerator LoadDatabases()
        {
            // These would typically be loaded by their respective managers
            // Plant Database
            yield return null;

            // Recipe Database
            yield return null;

            // Ingredient Database
            yield return null;

            // Spirit Database
            yield return null;

            // Achievement Data
            yield return null;
        }

        private IEnumerator FinalSetup()
        {
            // Setup complete
            yield return null;

            // Force garbage collection
            GC.Collect();
            yield return null;

            // Request permissions (Android/iOS)
            #if UNITY_ANDROID || UNITY_IOS
            RequestPermissions();
            #endif
            yield return null;
        }

        #endregion

        #region Manager Spawning

        private void SpawnManager(GameObject prefab, string fallbackName)
        {
            if (prefab != null)
            {
                var instance = Instantiate(prefab);
                instance.name = prefab.name;
            }
            else
            {
                // Create empty manager object for components to be added via code
                Debug.LogWarning($"GameBootstrap: {fallbackName} prefab not assigned, creating empty object");

                // Create basic managers programmatically if prefab not assigned
                CreateManagerFallback(fallbackName);
            }
        }

        private void CreateManagerFallback(string managerName)
        {
            GameObject obj = new GameObject(managerName);
            DontDestroyOnLoad(obj);

            switch (managerName)
            {
                case "ErrorHandler":
                    obj.AddComponent<ErrorHandler>();
                    break;
                case "PoolManager":
                    obj.AddComponent<PoolManager>();
                    break;
                case "PerformanceMonitor":
                    obj.AddComponent<PerformanceMonitor>();
                    break;
                case "QualityManager":
                    obj.AddComponent<QualityManager>();
                    break;
                case "AudioManager":
                    obj.AddComponent<Audio.AudioManager>();
                    break;
                case "LocalizationManager":
                    obj.AddComponent<Localization.LocalizationManager>();
                    break;
                case "SaveManager":
                    obj.AddComponent<SaveManager>();
                    break;
                case "AnalyticsManager":
                    obj.AddComponent<Analytics.AnalyticsManager>();
                    break;
                case "LoadingScreen":
                    obj.AddComponent<UI.LoadingScreen>();
                    break;
                default:
                    Destroy(obj);
                    break;
            }
        }

        #endregion

        #region Scene Loading

        public void LoadMainMenu()
        {
            var loading = UI.LoadingScreen.Instance;
            if (loading != null)
            {
                loading.LoadScene(mainMenuScene);
            }
            else
            {
                SceneManager.LoadScene(mainMenuScene);
            }
        }

        public void LoadGame()
        {
            var loading = UI.LoadingScreen.Instance;
            if (loading != null)
            {
                loading.LoadScene(gameScene);
            }
            else
            {
                SceneManager.LoadScene(gameScene);
            }
        }

        public void RestartGame()
        {
            // Clear saved data
            var saveManager = SaveManager.Instance;
            if (saveManager != null)
            {
                saveManager.DeleteAllSaves();
            }

            // Reload
            SceneManager.LoadScene(0);
        }

        #endregion

        #region Permissions

        private void RequestPermissions()
        {
            #if UNITY_ANDROID
            // Request notification permission on Android 13+
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
            {
                UnityEngine.Android.Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
            }
            #endif
        }

        #endregion

        #region Progress

        private void UpdateProgress(float progress, string status)
        {
            initProgress = progress;
            OnInitProgress?.Invoke(progress);

            Debug.Log($"GameBootstrap: {status} ({progress * 100:F0}%)");
        }

        #endregion

        #region Debug

        [ContextMenu("Reset All Data")]
        public void ResetAllData()
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("GameBootstrap: All PlayerPrefs data deleted");
        }

        [ContextMenu("Print System Info")]
        public void PrintSystemInfo()
        {
            Debug.Log($"Device: {SystemInfo.deviceModel}");
            Debug.Log($"OS: {SystemInfo.operatingSystem}");
            Debug.Log($"CPU: {SystemInfo.processorType} ({SystemInfo.processorCount} cores)");
            Debug.Log($"RAM: {SystemInfo.systemMemorySize}MB");
            Debug.Log($"GPU: {SystemInfo.graphicsDeviceName}");
            Debug.Log($"VRAM: {SystemInfo.graphicsMemorySize}MB");
        }

        #endregion
    }
}
