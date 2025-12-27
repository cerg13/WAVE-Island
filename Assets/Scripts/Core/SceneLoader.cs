using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using WaveIsland.UI;

namespace WaveIsland.Core
{
    /// <summary>
    /// Handles scene loading with transitions and loading screens
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        public enum SceneArea
        {
            MainMenu,
            Garden,
            Alchemy,
            Village,
            Temple
        }

        [Header("Scene Names")]
        [SerializeField] private string bootstrapScene = "Bootstrap";
        [SerializeField] private string mainMenuScene = "MainMenu";
        [SerializeField] private string gameScene = "Game";

        [Header("Loading")]
        [SerializeField] private LoadingScreen loadingScreenPrefab;
        [SerializeField] private float minimumLoadingTime = 1f;

        // State
        private LoadingScreen currentLoadingScreen;
        private bool isLoading = false;
        private SceneArea currentArea = SceneArea.MainMenu;

        // Events
        public event Action<string> OnSceneLoadStart;
        public event Action<string> OnSceneLoadComplete;
        public event Action<SceneArea> OnAreaChange;

        public bool IsLoading => isLoading;
        public SceneArea CurrentArea => currentArea;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #region Public Methods

        /// <summary>
        /// Load a scene by name
        /// </summary>
        public void LoadScene(string sceneName)
        {
            if (isLoading)
            {
                Debug.LogWarning("SceneLoader: Already loading a scene");
                return;
            }

            StartCoroutine(LoadSceneAsync(sceneName));
        }

        /// <summary>
        /// Load game scene with specific area
        /// </summary>
        public void LoadScene(string sceneName, SceneArea area)
        {
            currentArea = area;
            LoadScene(sceneName);
            OnAreaChange?.Invoke(area);
        }

        /// <summary>
        /// Load main menu
        /// </summary>
        public void LoadMainMenu()
        {
            LoadScene(mainMenuScene, SceneArea.MainMenu);
        }

        /// <summary>
        /// Load game scene (default to Garden)
        /// </summary>
        public void LoadGame()
        {
            LoadScene(gameScene, SceneArea.Garden);
        }

        /// <summary>
        /// Reload current scene
        /// </summary>
        public void ReloadCurrentScene()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            LoadScene(currentScene.name);
        }

        /// <summary>
        /// Navigate to area within game scene
        /// </summary>
        public void NavigateToArea(SceneArea area)
        {
            if (currentArea == area)
            {
                Debug.Log($"Already in area: {area}");
                return;
            }

            currentArea = area;
            OnAreaChange?.Invoke(area);

            // If not in game scene, load it
            Scene currentScene = SceneManager.GetActiveScene();
            if (currentScene.name != gameScene)
            {
                LoadScene(gameScene, area);
            }
        }

        #endregion

        #region Scene Loading

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            isLoading = true;
            float startTime = Time.realtimeSinceStartup;

            // Notify listeners
            OnSceneLoadStart?.Invoke(sceneName);

            // Show loading screen
            ShowLoadingScreen();

            // Wait a frame
            yield return null;

            // Start loading scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            // Update progress
            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

                if (currentLoadingScreen != null)
                {
                    currentLoadingScreen.SetProgress(progress);
                }

                // Scene is ready
                if (asyncLoad.progress >= 0.9f)
                {
                    // Ensure minimum loading time for smooth UX
                    float elapsedTime = Time.realtimeSinceStartup - startTime;
                    if (elapsedTime < minimumLoadingTime)
                    {
                        yield return new WaitForSeconds(minimumLoadingTime - elapsedTime);
                    }

                    // Complete progress
                    if (currentLoadingScreen != null)
                    {
                        currentLoadingScreen.SetProgress(1f);
                    }

                    // Activate scene
                    asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }

            // Hide loading screen
            yield return HideLoadingScreen();

            // Notify completion
            OnSceneLoadComplete?.Invoke(sceneName);

            isLoading = false;

            Debug.Log($"Scene loaded: {sceneName} in {Time.realtimeSinceStartup - startTime:F2}s");
        }

        #endregion

        #region Loading Screen

        private void ShowLoadingScreen()
        {
            if (currentLoadingScreen != null)
            {
                currentLoadingScreen.Show();
                return;
            }

            // Create loading screen if prefab exists
            if (loadingScreenPrefab != null)
            {
                currentLoadingScreen = Instantiate(loadingScreenPrefab);
                DontDestroyOnLoad(currentLoadingScreen.gameObject);
                currentLoadingScreen.Show();
            }
            else
            {
                Debug.LogWarning("SceneLoader: No loading screen prefab assigned");
            }
        }

        private IEnumerator HideLoadingScreen()
        {
            if (currentLoadingScreen != null)
            {
                yield return currentLoadingScreen.Hide();
            }
        }

        #endregion

        #region Utility

        /// <summary>
        /// Get current scene name
        /// </summary>
        public string GetCurrentSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        /// <summary>
        /// Check if in main menu
        /// </summary>
        public bool IsInMainMenu()
        {
            return GetCurrentSceneName() == mainMenuScene;
        }

        /// <summary>
        /// Check if in game
        /// </summary>
        public bool IsInGame()
        {
            return GetCurrentSceneName() == gameScene;
        }

        #endregion

        #region Debug

        [ContextMenu("Debug: Load Main Menu")]
        private void DebugLoadMainMenu()
        {
            LoadMainMenu();
        }

        [ContextMenu("Debug: Load Game")]
        private void DebugLoadGame()
        {
            LoadGame();
        }

        [ContextMenu("Debug: Reload Scene")]
        private void DebugReloadScene()
        {
            ReloadCurrentScene();
        }

        #endregion
    }
}
