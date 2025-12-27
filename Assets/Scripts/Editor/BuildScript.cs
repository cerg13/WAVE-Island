using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace WaveIsland.Editor
{
    /// <summary>
    /// Automated build script for CI/CD and local builds
    /// Usage: Unity -quit -batchmode -executeMethod BuildScript.BuildAll
    /// </summary>
    public class BuildScript
    {
        private static readonly string[] Scenes = {
            "Assets/Scenes/Bootstrap.unity",
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Game.unity"
        };

        private static readonly string BuildPath = "Builds";
        private static readonly string AppName = "WAVEIsland";

        [MenuItem("Build/Build All Platforms")]
        public static void BuildAll()
        {
            BuildAndroid();
            BuildIOS();
        }

        [MenuItem("Build/Build Android")]
        public static void BuildAndroid()
        {
            Debug.Log("Building Android...");

            string outputPath = Path.Combine(BuildPath, "Android", $"{AppName}.apk");
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            // Set Android specific settings
            PlayerSettings.Android.bundleVersionCode = GetVersionCode();
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Android build succeeded: {outputPath} ({summary.totalSize} bytes)");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError("Android build failed");
            }
        }

        [MenuItem("Build/Build iOS")]
        public static void BuildIOS()
        {
            Debug.Log("Building iOS...");

            string outputPath = Path.Combine(BuildPath, "iOS");
            Directory.CreateDirectory(outputPath);

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = outputPath,
                target = BuildTarget.iOS,
                options = BuildOptions.None
            };

            // Set iOS specific settings
            PlayerSettings.iOS.buildNumber = GetVersionCode().ToString();
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"iOS build succeeded: {outputPath}");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError("iOS build failed");
            }
        }

        [MenuItem("Build/Build Android App Bundle")]
        public static void BuildAndroidAppBundle()
        {
            Debug.Log("Building Android App Bundle...");

            string outputPath = Path.Combine(BuildPath, "Android", $"{AppName}.aab");
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            // Enable App Bundle
            EditorUserBuildSettings.buildAppBundle = true;

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            PlayerSettings.Android.bundleVersionCode = GetVersionCode();
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            EditorUserBuildSettings.buildAppBundle = false;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Android App Bundle build succeeded: {outputPath}");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError("Android App Bundle build failed");
            }
        }

        [MenuItem("Build/Increment Version")]
        public static void IncrementVersion()
        {
            string currentVersion = PlayerSettings.bundleVersion;
            string[] parts = currentVersion.Split('.');

            if (parts.Length >= 3)
            {
                int patch = int.Parse(parts[2]);
                parts[2] = (patch + 1).ToString();
                PlayerSettings.bundleVersion = string.Join(".", parts);

                Debug.Log($"Version incremented to {PlayerSettings.bundleVersion}");
            }
        }

        [MenuItem("Build/Set Development Build")]
        public static void SetDevelopmentBuild()
        {
            EditorUserBuildSettings.development = true;
            EditorUserBuildSettings.allowDebugging = true;
            EditorUserBuildSettings.connectProfiler = true;

            Debug.Log("Development build settings enabled");
        }

        [MenuItem("Build/Set Production Build")]
        public static void SetProductionBuild()
        {
            EditorUserBuildSettings.development = false;
            EditorUserBuildSettings.allowDebugging = false;
            EditorUserBuildSettings.connectProfiler = false;

            Debug.Log("Production build settings enabled");
        }

        private static int GetVersionCode()
        {
            // Parse version string (e.g., "1.2.3") to version code (e.g., 10203)
            string version = PlayerSettings.bundleVersion;
            string[] parts = version.Split('.');

            int major = parts.Length > 0 ? int.Parse(parts[0]) : 1;
            int minor = parts.Length > 1 ? int.Parse(parts[1]) : 0;
            int patch = parts.Length > 2 ? int.Parse(parts[2]) : 0;

            return major * 10000 + minor * 100 + patch;
        }

        [MenuItem("Build/Print Build Info")]
        public static void PrintBuildInfo()
        {
            Debug.Log("=== Build Information ===");
            Debug.Log($"App Name: {PlayerSettings.productName}");
            Debug.Log($"Bundle ID: {PlayerSettings.applicationIdentifier}");
            Debug.Log($"Version: {PlayerSettings.bundleVersion}");
            Debug.Log($"Version Code: {GetVersionCode()}");
            Debug.Log($"Company: {PlayerSettings.companyName}");
            Debug.Log($"Target Platform: {EditorUserBuildSettings.activeBuildTarget}");
            Debug.Log($"Development: {EditorUserBuildSettings.development}");
            Debug.Log("========================");
        }
    }
}
