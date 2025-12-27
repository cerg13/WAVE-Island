using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;
using WaveIsland.Core;

namespace WaveIsland.Editor
{
    /// <summary>
    /// Validates project setup before build
    /// Usage: Tools > WAVE Island > Validate Project
    /// </summary>
    public class ProjectValidator : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<ValidationResult> results = new List<ValidationResult>();

        private class ValidationResult
        {
            public string category;
            public string message;
            public MessageType type;
        }

        [MenuItem("Tools/WAVE Island/Validate Project")]
        public static void ShowWindow()
        {
            GetWindow<ProjectValidator>("Project Validator").Show();
        }

        [MenuItem("Tools/WAVE Island/Validate Project (Quick)", priority = 1)]
        public static void ValidateQuick()
        {
            ProjectValidator window = GetWindow<ProjectValidator>("Project Validator");
            window.RunValidation();
        }

        private void OnGUI()
        {
            GUILayout.Label("WAVE Island Project Validator", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("Run Full Validation", GUILayout.Height(40)))
            {
                RunValidation();
            }

            GUILayout.Space(10);

            if (results.Count > 0)
            {
                int errors = 0, warnings = 0, infos = 0;

                foreach (var result in results)
                {
                    if (result.type == MessageType.Error) errors++;
                    else if (result.type == MessageType.Warning) warnings++;
                    else infos++;
                }

                GUILayout.Label($"Results: {errors} Errors, {warnings} Warnings, {infos} Info", EditorStyles.boldLabel);
                GUILayout.Space(5);

                scrollPosition = GUILayout.BeginScrollView(scrollPosition);

                foreach (var result in results)
                {
                    EditorGUILayout.HelpBox($"[{result.category}] {result.message}", result.type);
                }

                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label("Click 'Run Full Validation' to check project setup.", EditorStyles.helpBox);
            }
        }

        private void RunValidation()
        {
            results.Clear();

            AddResult("Validation", "Starting project validation...", MessageType.Info);

            ValidateScenes();
            ValidatePrefabs();
            ValidateBuildSettings();
            ValidatePlayerSettings();
            ValidateResources();
            ValidateScripts();

            int errors = 0;
            foreach (var result in results)
            {
                if (result.type == MessageType.Error) errors++;
            }

            if (errors == 0)
            {
                AddResult("Summary", "✅ Validation complete! No blocking issues found.", MessageType.Info);
            }
            else
            {
                AddResult("Summary", $"❌ Validation found {errors} error(s). Please fix before building.", MessageType.Error);
            }

            Repaint();
        }

        private void ValidateScenes()
        {
            AddResult("Scenes", "Validating scenes...", MessageType.Info);

            // Check if scenes exist
            string[] requiredScenes = { "Bootstrap", "MainMenu", "Game" };
            string scenesPath = "Assets/Scenes";

            if (!Directory.Exists(scenesPath))
            {
                AddResult("Scenes", "Assets/Scenes/ folder not found! Create it and add scenes.", MessageType.Error);
                return;
            }

            foreach (string sceneName in requiredScenes)
            {
                string scenePath = Path.Combine(scenesPath, sceneName + ".unity");
                if (!File.Exists(scenePath))
                {
                    AddResult("Scenes", $"Required scene missing: {sceneName}.unity", MessageType.Error);
                }
                else
                {
                    AddResult("Scenes", $"✓ Found {sceneName}.unity", MessageType.Info);
                }
            }

            // Check build settings
            var scenesInBuild = EditorBuildSettings.scenes;
            if (scenesInBuild.Length == 0)
            {
                AddResult("Scenes", "No scenes in Build Settings! Add Bootstrap, MainMenu, Game.", MessageType.Error);
            }
            else
            {
                AddResult("Scenes", $"Build Settings: {scenesInBuild.Length} scene(s) configured", MessageType.Info);

                if (scenesInBuild.Length > 0 && !scenesInBuild[0].path.Contains("Bootstrap"))
                {
                    AddResult("Scenes", "Bootstrap scene should be first in Build Settings!", MessageType.Warning);
                }
            }
        }

        private void ValidatePrefabs()
        {
            AddResult("Prefabs", "Validating prefabs...", MessageType.Info);

            string managersPath = "Assets/Prefabs/Managers";

            if (!Directory.Exists(managersPath))
            {
                AddResult("Prefabs", "Managers folder not found. Run 'Create Manager Prefabs'.", MessageType.Warning);
                return;
            }

            string[] requiredManagers = {
                "GameManager", "SaveManager", "AudioManager",
                "LocalizationManager", "AnalyticsManager", "QualityManager",
                "ErrorHandler", "PoolManager", "PerformanceMonitor", "NotificationManager"
            };

            int found = 0;
            foreach (string manager in requiredManagers)
            {
                string prefabPath = Path.Combine(managersPath, manager + ".prefab");
                if (File.Exists(prefabPath))
                {
                    found++;
                }
                else
                {
                    AddResult("Prefabs", $"Missing manager prefab: {manager}.prefab", MessageType.Warning);
                }
            }

            AddResult("Prefabs", $"Found {found}/{requiredManagers.Length} manager prefabs", MessageType.Info);

            // Check UI prefabs
            string uiPath = "Assets/Prefabs/UI";
            if (Directory.Exists(uiPath))
            {
                if (File.Exists(Path.Combine(uiPath, "LoadingScreen.prefab")))
                {
                    AddResult("Prefabs", "✓ LoadingScreen prefab found", MessageType.Info);
                }
                else
                {
                    AddResult("Prefabs", "LoadingScreen.prefab missing", MessageType.Warning);
                }
            }
        }

        private void ValidateBuildSettings()
        {
            AddResult("Build", "Validating build settings...", MessageType.Info);

            // Check bundle identifier
            string bundleId = PlayerSettings.applicationIdentifier;
            if (string.IsNullOrEmpty(bundleId) || bundleId == "com.Company.ProductName")
            {
                AddResult("Build", "Bundle Identifier not set! Should be: com.wave.island", MessageType.Error);
            }
            else if (bundleId != "com.wave.island")
            {
                AddResult("Build", $"Bundle Identifier is '{bundleId}'. Expected: com.wave.island", MessageType.Warning);
            }
            else
            {
                AddResult("Build", "✓ Bundle Identifier correct", MessageType.Info);
            }

            // Check version
            string version = PlayerSettings.bundleVersion;
            if (string.IsNullOrEmpty(version) || version == "0.1")
            {
                AddResult("Build", "Version not set properly. Should be 1.0.0+", MessageType.Warning);
            }
            else
            {
                AddResult("Build", $"Version: {version}", MessageType.Info);
            }

            // Check company name
            if (PlayerSettings.companyName == "DefaultCompany")
            {
                AddResult("Build", "Company Name should be changed from 'DefaultCompany'", MessageType.Warning);
            }

            // Check product name
            if (PlayerSettings.productName == "New Unity Project")
            {
                AddResult("Build", "Product Name should be 'WAVE Island'", MessageType.Warning);
            }
        }

        private void ValidatePlayerSettings()
        {
            AddResult("Player Settings", "Validating player settings...", MessageType.Info);

            // Check scripting backend
#if UNITY_ANDROID
            var backend = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android);
            if (backend != ScriptingImplementation.IL2CPP)
            {
                AddResult("Player Settings", "Android: Scripting Backend should be IL2CPP for better performance", MessageType.Warning);
            }
            else
            {
                AddResult("Player Settings", "✓ Android: IL2CPP configured", MessageType.Info);
            }

            // Check architecture
            var arch = PlayerSettings.Android.targetArchitectures;
            if ((arch & AndroidArchitecture.ARM64) == 0)
            {
                AddResult("Player Settings", "Android: ARM64 architecture required for Play Store", MessageType.Error);
            }
#endif

#if UNITY_IOS
            var iosBackend = PlayerSettings.GetScriptingBackend(BuildTargetGroup.iOS);
            if (iosBackend != ScriptingImplementation.IL2CPP)
            {
                AddResult("Player Settings", "iOS: IL2CPP required", MessageType.Error);
            }

            // Check minimum iOS version
            if (PlayerSettings.iOS.targetOSVersionString != "13.0")
            {
                AddResult("Player Settings", "iOS: Minimum OS Version should be 13.0", MessageType.Warning);
            }
#endif

            // Check API compatibility
            var apiLevel = PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Standalone);
            if (apiLevel == ApiCompatibilityLevel.NET_2_0)
            {
                AddResult("Player Settings", "Consider using .NET 4.x for better compatibility", MessageType.Info);
            }
        }

        private void ValidateResources()
        {
            AddResult("Resources", "Validating resources...", MessageType.Info);

            // Check localization files
            string locPath = "Assets/Resources/Localization";
            if (!Directory.Exists(locPath))
            {
                AddResult("Resources", "Localization folder not found!", MessageType.Error);
            }
            else
            {
                string[] languages = { "ru.json", "en.json" };
                foreach (string lang in languages)
                {
                    if (!File.Exists(Path.Combine(locPath, lang)))
                    {
                        AddResult("Resources", $"Missing localization: {lang}", MessageType.Warning);
                    }
                }
            }

            // Check game data
            string dataPath = "Assets/Resources/GameData";
            if (!Directory.Exists(dataPath))
            {
                AddResult("Resources", "GameData folder not found!", MessageType.Warning);
            }
            else
            {
                string[] dataFiles = { "ingredients_data.json", "recipes_data.json", "spirits_data.json", "achievements_data.json" };
                int foundData = 0;
                foreach (string file in dataFiles)
                {
                    if (File.Exists(Path.Combine(dataPath, file)))
                    {
                        foundData++;
                    }
                }
                AddResult("Resources", $"Found {foundData}/{dataFiles.Length} data files", MessageType.Info);
            }
        }

        private void ValidateScripts()
        {
            AddResult("Scripts", "Validating scripts...", MessageType.Info);

            // Check if core scripts exist
            string[] coreScripts = {
                "Assets/Scripts/Core/GameManager.cs",
                "Assets/Scripts/Core/SaveManager.cs",
                "Assets/Scripts/Core/GameBootstrap.cs",
                "Assets/Scripts/Core/SceneLoader.cs",
                "Assets/Scripts/UI/MainMenuController.cs"
            };

            int foundScripts = 0;
            foreach (string scriptPath in coreScripts)
            {
                if (File.Exists(scriptPath))
                {
                    foundScripts++;
                }
                else
                {
                    AddResult("Scripts", $"Core script missing: {Path.GetFileName(scriptPath)}", MessageType.Error);
                }
            }

            AddResult("Scripts", $"Found {foundScripts}/{coreScripts.Length} core scripts", MessageType.Info);

            // Check for compilation errors
            if (EditorUtility.scriptCompilationFailed)
            {
                AddResult("Scripts", "⚠️ Script compilation errors detected! Fix before building.", MessageType.Error);
            }
            else
            {
                AddResult("Scripts", "✓ No compilation errors", MessageType.Info);
            }
        }

        private void AddResult(string category, string message, MessageType type)
        {
            results.Add(new ValidationResult
            {
                category = category,
                message = message,
                type = type
            });

            // Also log to console
            string logMessage = $"[{category}] {message}";
            if (type == MessageType.Error)
                Debug.LogError(logMessage);
            else if (type == MessageType.Warning)
                Debug.LogWarning(logMessage);
            else
                Debug.Log(logMessage);
        }
    }
}
