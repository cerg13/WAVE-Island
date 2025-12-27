using System;
using System.Collections.Generic;
using UnityEngine;

namespace WaveIsland.Core
{
    /// <summary>
    /// Global error handling and crash reporting
    /// </summary>
    public class ErrorHandler : MonoBehaviour
    {
        public static ErrorHandler Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool logToFile = true;
        [SerializeField] private bool showErrorDialog = true;
        [SerializeField] private int maxLogEntries = 100;
        [SerializeField] private string logFileName = "error_log.txt";

        [Header("UI")]
        [SerializeField] private GameObject errorDialogPrefab;

        // Error log
        private List<ErrorLogEntry> errorLog = new List<ErrorLogEntry>();
        private string logFilePath;

        // Events
        public event Action<ErrorLogEntry> OnError;
        public event Action<string> OnCriticalError;

        // Stats
        private int errorCount = 0;
        private int warningCount = 0;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            logFilePath = System.IO.Path.Combine(Application.persistentDataPath, logFileName);

            // Subscribe to Unity's error handling
            Application.logMessageReceived += HandleLog;
            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;

            Debug.Log("ErrorHandler: Initialized");
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
            AppDomain.CurrentDomain.UnhandledException -= HandleUnhandledException;
        }

        private void OnApplicationQuit()
        {
            SaveLog();
        }

        #region Log Handling

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            // Only process errors, warnings, and exceptions
            if (type == LogType.Log) return;

            ErrorSeverity severity = type switch
            {
                LogType.Error => ErrorSeverity.Error,
                LogType.Exception => ErrorSeverity.Critical,
                LogType.Warning => ErrorSeverity.Warning,
                LogType.Assert => ErrorSeverity.Error,
                _ => ErrorSeverity.Info
            };

            LogError(logString, stackTrace, severity, "Unity");
        }

        private void HandleUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = args.ExceptionObject as Exception;
            if (e != null)
            {
                LogError(e.Message, e.StackTrace, ErrorSeverity.Critical, "Unhandled");

                if (args.IsTerminating)
                {
                    // Save log before crash
                    SaveLog();
                }
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Log an error
        /// </summary>
        public void LogError(string message, string stackTrace = "", ErrorSeverity severity = ErrorSeverity.Error, string source = "Game")
        {
            var entry = new ErrorLogEntry
            {
                timestamp = DateTime.UtcNow,
                severity = severity,
                source = source,
                message = message,
                stackTrace = stackTrace,
                deviceInfo = GetDeviceInfo(),
                gameState = GetGameState()
            };

            // Add to log
            errorLog.Add(entry);

            // Trim log if too large
            while (errorLog.Count > maxLogEntries)
            {
                errorLog.RemoveAt(0);
            }

            // Update counts
            if (severity == ErrorSeverity.Error || severity == ErrorSeverity.Critical)
                errorCount++;
            else if (severity == ErrorSeverity.Warning)
                warningCount++;

            // Notify listeners
            OnError?.Invoke(entry);

            // Handle critical errors
            if (severity == ErrorSeverity.Critical)
            {
                HandleCriticalError(entry);
            }

            // Log to file
            if (logToFile)
            {
                AppendToLogFile(entry);
            }
        }

        /// <summary>
        /// Log an exception
        /// </summary>
        public void LogException(Exception e, string context = "")
        {
            string message = string.IsNullOrEmpty(context)
                ? e.Message
                : $"{context}: {e.Message}";

            LogError(message, e.StackTrace, ErrorSeverity.Critical, "Exception");
        }

        /// <summary>
        /// Show error dialog to user
        /// </summary>
        public void ShowError(string title, string message, Action onDismiss = null)
        {
            if (!showErrorDialog) return;

            // Log the error
            LogError($"{title}: {message}", "", ErrorSeverity.Error, "User");

            // Show dialog
            ShowErrorDialog(title, message, onDismiss);
        }

        /// <summary>
        /// Show network error
        /// </summary>
        public void ShowNetworkError(Action onRetry = null)
        {
            ShowErrorWithRetry(
                Localization.L.Get("error.network"),
                "Проверьте подключение к интернету и попробуйте снова.",
                onRetry
            );
        }

        /// <summary>
        /// Show error with retry option
        /// </summary>
        public void ShowErrorWithRetry(string title, string message, Action onRetry)
        {
            // Implementation depends on your UI system
            Debug.LogError($"Error: {title} - {message}");

            // TODO: Show actual dialog with retry button
            onRetry?.Invoke();
        }

        #endregion

        #region Critical Error Handling

        private void HandleCriticalError(ErrorLogEntry entry)
        {
            Debug.LogError($"CRITICAL ERROR: {entry.message}");

            // Save log immediately
            SaveLog();

            // Send to analytics
            SendCrashReport(entry);

            // Notify
            OnCriticalError?.Invoke(entry.message);

            // Show dialog
            if (showErrorDialog)
            {
                ShowErrorDialog(
                    "Критическая ошибка",
                    "Произошла непредвиденная ошибка. Приложение будет перезапущено.",
                    () => RestartGame()
                );
            }
        }

        private void SendCrashReport(ErrorLogEntry entry)
        {
            // Send to analytics
            var analytics = Analytics.AnalyticsManager.Instance;
            if (analytics != null)
            {
                analytics.TrackError(entry.severity.ToString(), entry.message);
            }

            // TODO: Send to crash reporting service (Firebase Crashlytics, Sentry, etc.)
        }

        private void RestartGame()
        {
            // Clear caches
            Resources.UnloadUnusedAssets();
            GC.Collect();

            // Reload main scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }

        #endregion

        #region Log File

        private void AppendToLogFile(ErrorLogEntry entry)
        {
            try
            {
                string logLine = $"[{entry.timestamp:yyyy-MM-dd HH:mm:ss}] [{entry.severity}] [{entry.source}] {entry.message}\n";

                if (!string.IsNullOrEmpty(entry.stackTrace))
                {
                    logLine += $"Stack: {entry.stackTrace}\n";
                }

                logLine += "---\n";

                System.IO.File.AppendAllText(logFilePath, logLine);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"ErrorHandler: Failed to write to log file: {e.Message}");
            }
        }

        private void SaveLog()
        {
            try
            {
                var fullLog = new System.Text.StringBuilder();
                fullLog.AppendLine($"=== Error Log - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} ===");
                fullLog.AppendLine($"Device: {SystemInfo.deviceModel}");
                fullLog.AppendLine($"OS: {SystemInfo.operatingSystem}");
                fullLog.AppendLine($"App Version: {Application.version}");
                fullLog.AppendLine($"Errors: {errorCount}, Warnings: {warningCount}");
                fullLog.AppendLine("===");
                fullLog.AppendLine();

                foreach (var entry in errorLog)
                {
                    fullLog.AppendLine($"[{entry.timestamp:yyyy-MM-dd HH:mm:ss}] [{entry.severity}] [{entry.source}]");
                    fullLog.AppendLine($"Message: {entry.message}");
                    if (!string.IsNullOrEmpty(entry.stackTrace))
                    {
                        fullLog.AppendLine($"Stack: {entry.stackTrace}");
                    }
                    fullLog.AppendLine();
                }

                System.IO.File.WriteAllText(logFilePath, fullLog.ToString());
                Debug.Log($"ErrorHandler: Log saved to {logFilePath}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"ErrorHandler: Failed to save log: {e.Message}");
            }
        }

        /// <summary>
        /// Get log file contents
        /// </summary>
        public string GetLogContents()
        {
            try
            {
                if (System.IO.File.Exists(logFilePath))
                {
                    return System.IO.File.ReadAllText(logFilePath);
                }
            }
            catch { }

            return "";
        }

        /// <summary>
        /// Clear log
        /// </summary>
        public void ClearLog()
        {
            errorLog.Clear();
            errorCount = 0;
            warningCount = 0;

            try
            {
                if (System.IO.File.Exists(logFilePath))
                {
                    System.IO.File.Delete(logFilePath);
                }
            }
            catch { }
        }

        #endregion

        #region Helpers

        private string GetDeviceInfo()
        {
            return $"{SystemInfo.deviceModel} | {SystemInfo.operatingSystem} | RAM: {SystemInfo.systemMemorySize}MB";
        }

        private string GetGameState()
        {
            var state = new System.Text.StringBuilder();

            // Player data
            var playerData = PlayerData.Instance;
            if (playerData != null)
            {
                state.Append($"Level: {playerData.Level}, ");
                state.Append($"Coins: {playerData.Coins}");
            }

            // Current scene
            state.Append($", Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");

            return state.ToString();
        }

        private void ShowErrorDialog(string title, string message, Action onDismiss)
        {
            // Simple implementation - in production use your UI system
            Debug.LogError($"ERROR DIALOG: {title}\n{message}");

            // TODO: Instantiate actual error dialog UI
            // For now, just invoke callback after delay
            if (onDismiss != null)
            {
                StartCoroutine(InvokeDelayed(onDismiss, 1f));
            }
        }

        private System.Collections.IEnumerator InvokeDelayed(Action action, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            action?.Invoke();
        }

        #endregion

        #region Stats

        public int GetErrorCount() => errorCount;
        public int GetWarningCount() => warningCount;
        public List<ErrorLogEntry> GetRecentErrors(int count = 10)
        {
            int start = Mathf.Max(0, errorLog.Count - count);
            return errorLog.GetRange(start, Mathf.Min(count, errorLog.Count - start));
        }

        #endregion
    }

    #region Data Classes

    public enum ErrorSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }

    [Serializable]
    public class ErrorLogEntry
    {
        public DateTime timestamp;
        public ErrorSeverity severity;
        public string source;
        public string message;
        public string stackTrace;
        public string deviceInfo;
        public string gameState;
    }

    #endregion
}
