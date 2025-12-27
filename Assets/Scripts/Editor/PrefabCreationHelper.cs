using UnityEngine;
using UnityEditor;
using System.IO;
using WaveIsland.Core;
using WaveIsland.Audio;
using WaveIsland.Localization;
using WaveIsland.Analytics;

namespace WaveIsland.Editor
{
    /// <summary>
    /// Helper to create manager prefabs automatically
    /// Usage: Tools > WAVE Island > Create Prefabs
    /// </summary>
    public class PrefabCreationHelper : EditorWindow
    {
        private const string PREFABS_PATH = "Assets/Prefabs";
        private const string MANAGERS_PATH = "Assets/Prefabs/Managers";

        [MenuItem("Tools/WAVE Island/Create Manager Prefabs")]
        public static void CreateAllManagerPrefabs()
        {
            if (EditorUtility.DisplayDialog("Create Manager Prefabs",
                "This will create all manager prefabs in Assets/Prefabs/Managers/. Continue?",
                "Yes", "Cancel"))
            {
                // Create directories
                if (!Directory.Exists(MANAGERS_PATH))
                    Directory.CreateDirectory(MANAGERS_PATH);

                // Create each manager prefab
                CreateGameManagerPrefab();
                CreateSaveManagerPrefab();
                CreateAudioManagerPrefab();
                CreateLocalizationManagerPrefab();
                CreateAnalyticsManagerPrefab();
                CreateQualityManagerPrefab();
                CreateErrorHandlerPrefab();
                CreatePoolManagerPrefab();
                CreatePerformanceMonitorPrefab();
                CreateNotificationManagerPrefab();

                AssetDatabase.Refresh();

                Debug.Log("All manager prefabs created!");
                EditorUtility.DisplayDialog("Success",
                    "Manager prefabs created in Assets/Prefabs/Managers/", "OK");
            }
        }

        [MenuItem("Tools/WAVE Island/Show Prefab Helper")]
        public static void ShowWindow()
        {
            GetWindow<PrefabCreationHelper>("Prefab Helper");
        }

        #region Manager Prefab Creation

        private static void CreateGameManagerPrefab()
        {
            GameObject obj = new GameObject("GameManager");
            obj.AddComponent<GameManager>();

            string path = Path.Combine(MANAGERS_PATH, "GameManager.prefab");
            PrefabUtility.SaveAsPrefabAsset(obj, path);
            DestroyImmediate(obj);

            Debug.Log($"Created: {path}");
        }

        private static void CreateSaveManagerPrefab()
        {
            GameObject obj = new GameObject("SaveManager");
            obj.AddComponent<SaveManager>();

            string path = Path.Combine(MANAGERS_PATH, "SaveManager.prefab");
            PrefabUtility.SaveAsPrefabAsset(obj, path);
            DestroyImmediate(obj);

            Debug.Log($"Created: {path}");
        }

        private static void CreateAudioManagerPrefab()
        {
            GameObject obj = new GameObject("AudioManager");
            AudioManager audioMgr = obj.AddComponent<AudioManager>();

            // Add AudioSources
            GameObject musicSource = new GameObject("MusicSource");
            musicSource.transform.SetParent(obj.transform);
            AudioSource music = musicSource.AddComponent<AudioSource>();
            music.loop = true;
            music.playOnAwake = false;

            GameObject sfxSource = new GameObject("SFXSource");
            sfxSource.transform.SetParent(obj.transform);
            AudioSource sfx = sfxSource.AddComponent<AudioSource>();
            sfx.playOnAwake = false;

            string path = Path.Combine(MANAGERS_PATH, "AudioManager.prefab");
            PrefabUtility.SaveAsPrefabAsset(obj, path);
            DestroyImmediate(obj);

            Debug.Log($"Created: {path}");
        }

        private static void CreateLocalizationManagerPrefab()
        {
            GameObject obj = new GameObject("LocalizationManager");
            obj.AddComponent<LocalizationManager>();

            string path = Path.Combine(MANAGERS_PATH, "LocalizationManager.prefab");
            PrefabUtility.SaveAsPrefabAsset(obj, path);
            DestroyImmediate(obj);

            Debug.Log($"Created: {path}");
        }

        private static void CreateAnalyticsManagerPrefab()
        {
            GameObject obj = new GameObject("AnalyticsManager");
            obj.AddComponent<AnalyticsManager>();

            string path = Path.Combine(MANAGERS_PATH, "AnalyticsManager.prefab");
            PrefabUtility.SaveAsPrefabAsset(obj, path);
            DestroyImmediate(obj);

            Debug.Log($"Created: {path}");
        }

        private static void CreateQualityManagerPrefab()
        {
            GameObject obj = new GameObject("QualityManager");
            obj.AddComponent<QualityManager>();

            string path = Path.Combine(MANAGERS_PATH, "QualityManager.prefab");
            PrefabUtility.SaveAsPrefabAsset(obj, path);
            DestroyImmediate(obj);

            Debug.Log($"Created: {path}");
        }

        private static void CreateErrorHandlerPrefab()
        {
            GameObject obj = new GameObject("ErrorHandler");
            obj.AddComponent<ErrorHandler>();

            string path = Path.Combine(MANAGERS_PATH, "ErrorHandler.prefab");
            PrefabUtility.SaveAsPrefabAsset(obj, path);
            DestroyImmediate(obj);

            Debug.Log($"Created: {path}");
        }

        private static void CreatePoolManagerPrefab()
        {
            GameObject obj = new GameObject("PoolManager");
            // PoolManager is static, but we create prefab for consistency

            string path = Path.Combine(MANAGERS_PATH, "PoolManager.prefab");
            PrefabUtility.SaveAsPrefabAsset(obj, path);
            DestroyImmediate(obj);

            Debug.Log($"Created: {path}");
        }

        private static void CreatePerformanceMonitorPrefab()
        {
            GameObject obj = new GameObject("PerformanceMonitor");
            obj.AddComponent<PerformanceMonitor>();

            string path = Path.Combine(MANAGERS_PATH, "PerformanceMonitor.prefab");
            PrefabUtility.SaveAsPrefabAsset(obj, path);
            DestroyImmediate(obj);

            Debug.Log($"Created: {path}");
        }

        private static void CreateNotificationManagerPrefab()
        {
            GameObject obj = new GameObject("NotificationManager");
            obj.AddComponent<Notifications.NotificationManager>();

            string path = Path.Combine(MANAGERS_PATH, "NotificationManager.prefab");
            PrefabUtility.SaveAsPrefabAsset(obj, path);
            DestroyImmediate(obj);

            Debug.Log($"Created: {path}");
        }

        #endregion

        #region UI Prefab Creation

        [MenuItem("Tools/WAVE Island/Create UI Prefabs")]
        public static void CreateUIElements()
        {
            string uiPath = Path.Combine(PREFABS_PATH, "UI");
            if (!Directory.Exists(uiPath))
                Directory.CreateDirectory(uiPath);

            CreateLoadingScreenPrefab(uiPath);

            AssetDatabase.Refresh();
            Debug.Log("UI prefabs created!");
        }

        private static void CreateLoadingScreenPrefab(string path)
        {
            GameObject obj = new GameObject("LoadingScreen");

            Canvas canvas = obj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            obj.AddComponent<UnityEngine.UI.CanvasScaler>();
            obj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            obj.AddComponent<UI.LoadingScreen>();

            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(obj.transform);
            UnityEngine.UI.Image bgImage = bg.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.15f, 1f);
            RectTransform bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            string prefabPath = Path.Combine(path, "LoadingScreen.prefab");
            PrefabUtility.SaveAsPrefabAsset(obj, prefabPath);
            DestroyImmediate(obj);

            Debug.Log($"Created: {prefabPath}");
        }

        #endregion

        #region GUI

        private void OnGUI()
        {
            GUILayout.Label("WAVE Island Prefab Creator", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.Label("Manager Prefabs:", EditorStyles.boldLabel);

            if (GUILayout.Button("Create All Manager Prefabs", GUILayout.Height(50)))
            {
                CreateAllManagerPrefabs();
            }

            GUILayout.Space(20);

            GUILayout.Label("UI Prefabs:", EditorStyles.boldLabel);

            if (GUILayout.Button("Create UI Prefabs", GUILayout.Height(40)))
            {
                CreateUIElements();
            }

            GUILayout.Space(20);
            GUILayout.Label("Prefabs will be created in:\nAssets/Prefabs/Managers/\nAssets/Prefabs/UI/",
                EditorStyles.helpBox);
        }

        #endregion
    }
}
