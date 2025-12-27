using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using WaveIsland.Core;
using System.IO;

namespace WaveIsland.Editor
{
    /// <summary>
    /// Helper to automatically create and setup game scenes
    /// Usage: Tools > WAVE Island > Setup Scenes
    /// </summary>
    public class SceneSetupHelper : EditorWindow
    {
        private const string SCENES_PATH = "Assets/Scenes";
        private const string PREFABS_PATH = "Assets/Prefabs";

        [MenuItem("Tools/WAVE Island/Setup Scenes")]
        public static void ShowWindow()
        {
            GetWindow<SceneSetupHelper>("Scene Setup");
        }

        [MenuItem("Tools/WAVE Island/Create All Scenes")]
        public static void CreateAllScenes()
        {
            if (EditorUtility.DisplayDialog("Create Scenes",
                "This will create Bootstrap, MainMenu, and Game scenes. Continue?",
                "Yes", "Cancel"))
            {
                CreateBootstrapScene();
                CreateMainMenuScene();
                CreateGameScene();

                Debug.Log("All scenes created successfully!");
                EditorUtility.DisplayDialog("Success", "All scenes created! Check Assets/Scenes/", "OK");
            }
        }

        [MenuItem("Tools/WAVE Island/Create Bootstrap Scene")]
        public static void CreateBootstrapScene()
        {
            // Create directory if needed
            if (!Directory.Exists(SCENES_PATH))
                Directory.CreateDirectory(SCENES_PATH);

            // Create new scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Create GameBootstrap
            GameObject bootstrap = new GameObject("GameBootstrap");
            bootstrap.AddComponent<GameBootstrap>();

            // Create Loading UI
            CreateLoadingUI();

            // Create EventSystem
            CreateEventSystem();

            // Save scene
            string scenePath = Path.Combine(SCENES_PATH, "Bootstrap.unity");
            EditorSceneManager.SaveScene(newScene, scenePath);

            Debug.Log($"Bootstrap scene created: {scenePath}");
        }

        [MenuItem("Tools/WAVE Island/Create MainMenu Scene")]
        public static void CreateMainMenuScene()
        {
            if (!Directory.Exists(SCENES_PATH))
                Directory.CreateDirectory(SCENES_PATH);

            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Create Canvas
            GameObject canvas = CreateUICanvas("MainCanvas");

            // Create MainMenuController
            GameObject controller = new GameObject("MainMenuController");
            controller.AddComponent<UI.MainMenuController>();

            // Create EventSystem
            CreateEventSystem();

            // Add sample background
            CreateMainMenuUI(canvas);

            string scenePath = Path.Combine(SCENES_PATH, "MainMenu.unity");
            EditorSceneManager.SaveScene(newScene, scenePath);

            Debug.Log($"MainMenu scene created: {scenePath}");
        }

        [MenuItem("Tools/WAVE Island/Create Game Scene")]
        public static void CreateGameScene()
        {
            if (!Directory.Exists(SCENES_PATH))
                Directory.CreateDirectory(SCENES_PATH);

            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Create UI Canvas
            GameObject canvas = CreateUICanvas("GameCanvas");

            // Create Garden Area
            CreateGardenArea();

            // Create Alchemy Area
            CreateAlchemyArea();

            // Create Managers
            CreateGameManagers();

            string scenePath = Path.Combine(SCENES_PATH, "Game.unity");
            EditorSceneManager.SaveScene(newScene, scenePath);

            Debug.Log($"Game scene created: {scenePath}");
        }

        #region Helper Methods

        private static GameObject CreateUICanvas(string name)
        {
            GameObject canvasObj = new GameObject(name);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            UnityEngine.UI.CanvasScaler scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            return canvasObj;
        }

        private static void CreateEventSystem()
        {
            if (GameObject.Find("EventSystem") != null)
                return;

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        private static void CreateLoadingUI()
        {
            GameObject loadingCanvas = CreateUICanvas("LoadingCanvas");

            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(loadingCanvas.transform);
            UnityEngine.UI.Image bgImage = bg.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.15f);
            RectTransform bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // Logo placeholder
            GameObject logo = new GameObject("Logo");
            logo.transform.SetParent(loadingCanvas.transform);
            UnityEngine.UI.Image logoImage = logo.AddComponent<UnityEngine.UI.Image>();
            logoImage.color = Color.white;
            RectTransform logoRect = logo.GetComponent<RectTransform>();
            logoRect.sizeDelta = new Vector2(400, 200);
            logoRect.anchoredPosition = new Vector2(0, 100);

            // Progress bar
            GameObject progressBar = CreateProgressBar();
            progressBar.transform.SetParent(loadingCanvas.transform);

            // Status text
            GameObject statusText = new GameObject("StatusText");
            statusText.transform.SetParent(loadingCanvas.transform);
            UnityEngine.UI.Text text = statusText.AddComponent<UnityEngine.UI.Text>();
            text.text = "Загрузка...";
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 24;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            RectTransform textRect = statusText.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(600, 50);
            textRect.anchoredPosition = new Vector2(0, -200);
        }

        private static GameObject CreateProgressBar()
        {
            GameObject progressBar = new GameObject("ProgressBar");

            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(progressBar.transform);
            UnityEngine.UI.Image bgImage = bg.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.25f);
            RectTransform bgRect = bg.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(600, 30);

            // Fill
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(progressBar.transform);
            UnityEngine.UI.Image fillImage = fill.AddComponent<UnityEngine.UI.Image>();
            fillImage.color = new Color(0.2f, 0.8f, 0.4f);
            fillImage.type = UnityEngine.UI.Image.Type.Filled;
            fillImage.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.sizeDelta = new Vector2(600, 30);

            RectTransform rect = progressBar.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, -100);

            return progressBar;
        }

        private static void CreateMainMenuUI(GameObject canvas)
        {
            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(canvas.transform);
            UnityEngine.UI.Image bgImage = bg.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = new Color(0.15f, 0.3f, 0.4f);
            RectTransform bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // Header
            GameObject header = new GameObject("Header");
            header.transform.SetParent(canvas.transform);
            RectTransform headerRect = header.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 1);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.pivot = new Vector2(0.5f, 1);
            headerRect.sizeDelta = new Vector2(0, 150);

            // Navigation Panel
            CreateNavigationButtons(canvas);
        }

        private static void CreateNavigationButtons(GameObject canvas)
        {
            GameObject navPanel = new GameObject("NavigationPanel");
            navPanel.transform.SetParent(canvas.transform);
            RectTransform navRect = navPanel.AddComponent<RectTransform>();
            navRect.anchorMin = new Vector2(0.5f, 0.5f);
            navRect.anchorMax = new Vector2(0.5f, 0.5f);
            navRect.sizeDelta = new Vector2(800, 600);

            UnityEngine.UI.GridLayoutGroup grid = navPanel.AddComponent<UnityEngine.UI.GridLayoutGroup>();
            grid.cellSize = new Vector2(250, 100);
            grid.spacing = new Vector2(20, 20);
            grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;

            string[] buttons = { "Garden", "Alchemy", "Shop", "Spirits", "Achievements", "Leaderboard", "Quests", "Settings", "iiko" };

            foreach (string btnName in buttons)
            {
                CreateButton(navPanel, btnName);
            }
        }

        private static void CreateButton(GameObject parent, string name)
        {
            GameObject button = new GameObject(name + "Button");
            button.transform.SetParent(parent.transform);

            UnityEngine.UI.Image img = button.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0.3f, 0.6f, 0.8f);

            UnityEngine.UI.Button btn = button.AddComponent<UnityEngine.UI.Button>();

            GameObject text = new GameObject("Text");
            text.transform.SetParent(button.transform);
            UnityEngine.UI.Text txtComp = text.AddComponent<UnityEngine.UI.Text>();
            txtComp.text = name;
            txtComp.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txtComp.fontSize = 24;
            txtComp.alignment = TextAnchor.MiddleCenter;
            txtComp.color = Color.white;

            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }

        private static void CreateGardenArea()
        {
            GameObject gardenArea = new GameObject("GardenArea");

            // Add GardenGrid component
            gardenArea.AddComponent<Garden.GardenGrid>();

            // Create plots container
            GameObject plots = new GameObject("Plots");
            plots.transform.SetParent(gardenArea.transform);
        }

        private static void CreateAlchemyArea()
        {
            GameObject alchemyArea = new GameObject("AlchemyArea");
            alchemyArea.SetActive(false); // Start disabled

            // Add CraftingTable
            GameObject craftingTable = new GameObject("CraftingTable");
            craftingTable.transform.SetParent(alchemyArea.transform);
            craftingTable.AddComponent<Alchemy.CraftingTable>();
        }

        private static void CreateGameManagers()
        {
            GameObject managers = new GameObject("Managers");

            GameObject gardenMgr = new GameObject("GardenManager");
            gardenMgr.transform.SetParent(managers.transform);
            gardenMgr.AddComponent<Managers.GardenManager>();

            GameObject alchemyMgr = new GameObject("AlchemyManager");
            alchemyMgr.transform.SetParent(managers.transform);
            alchemyMgr.AddComponent<Managers.AlchemyManager>();

            GameObject spiritMgr = new GameObject("SpiritManager");
            spiritMgr.transform.SetParent(managers.transform);
            spiritMgr.AddComponent<Managers.SpiritManager>();

            GameObject shopMgr = new GameObject("ShopManager");
            shopMgr.transform.SetParent(managers.transform);
            shopMgr.AddComponent<Shop.ShopManager>();
        }

        #endregion

        #region GUI

        private void OnGUI()
        {
            GUILayout.Label("WAVE Island Scene Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("Create Bootstrap Scene", GUILayout.Height(40)))
            {
                CreateBootstrapScene();
            }

            if (GUILayout.Button("Create MainMenu Scene", GUILayout.Height(40)))
            {
                CreateMainMenuScene();
            }

            if (GUILayout.Button("Create Game Scene", GUILayout.Height(40)))
            {
                CreateGameScene();
            }

            GUILayout.Space(20);

            if (GUILayout.Button("Create ALL Scenes", GUILayout.Height(50)))
            {
                CreateAllScenes();
            }

            GUILayout.Space(20);
            GUILayout.Label("Scenes will be created in: Assets/Scenes/", EditorStyles.helpBox);
        }

        #endregion
    }
}
