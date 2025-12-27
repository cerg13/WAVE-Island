using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace WaveIsland.Core
{
    /// <summary>
    /// Performance monitoring and profiling utility
    /// </summary>
    public class PerformanceMonitor : MonoBehaviour
    {
        public static PerformanceMonitor Instance { get; private set; }

        [Header("FPS Settings")]
        [SerializeField] private bool showFPS = true;
        [SerializeField] private float fpsUpdateInterval = 0.5f;
        [SerializeField] private int targetFPS = 60;

        [Header("Memory Settings")]
        [SerializeField] private bool trackMemory = true;
        [SerializeField] private float memoryUpdateInterval = 2f;
        [SerializeField] private long memoryWarningThresholdMB = 512;

        [Header("Debug UI")]
        [SerializeField] private bool showDebugUI = false;
        [SerializeField] private KeyCode toggleKey = KeyCode.F3;

        // FPS tracking
        private float fps;
        private float fpsAccumulator;
        private int fpsFrames;
        private float fpsNextUpdate;
        private float minFPS = float.MaxValue;
        private float maxFPS = 0;

        // Memory tracking
        private long currentMemoryMB;
        private long peakMemoryMB;
        private float memoryNextUpdate;

        // Frame time tracking
        private float[] frameTimes = new float[100];
        private int frameTimeIndex = 0;
        private float averageFrameTime;

        // Performance markers
        private Dictionary<string, Stopwatch> markers = new Dictionary<string, Stopwatch>();
        private Dictionary<string, PerformanceStats> stats = new Dictionary<string, PerformanceStats>();

        // Events
        public event Action<float> OnFPSUpdated;
        public event Action<long> OnMemoryWarning;
        public event Action<string, float> OnPerformanceIssue;

        private struct PerformanceStats
        {
            public int samples;
            public double totalMs;
            public double minMs;
            public double maxMs;
            public double AverageMs => samples > 0 ? totalMs / samples : 0;
        }

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

        private void Start()
        {
            Application.targetFrameRate = targetFPS;
        }

        private void Update()
        {
            UpdateFPS();

            if (trackMemory)
                UpdateMemory();

            TrackFrameTime();

            // Toggle debug UI
            if (Input.GetKeyDown(toggleKey))
            {
                showDebugUI = !showDebugUI;
            }
        }

        #region FPS Tracking

        private void UpdateFPS()
        {
            fpsAccumulator += Time.unscaledDeltaTime;
            fpsFrames++;

            if (Time.unscaledTime >= fpsNextUpdate)
            {
                fps = fpsFrames / fpsAccumulator;
                fpsAccumulator = 0;
                fpsFrames = 0;
                fpsNextUpdate = Time.unscaledTime + fpsUpdateInterval;

                // Track min/max
                if (fps < minFPS && fps > 0) minFPS = fps;
                if (fps > maxFPS) maxFPS = fps;

                OnFPSUpdated?.Invoke(fps);

                // Warn if FPS drops significantly
                if (fps < targetFPS * 0.5f)
                {
                    OnPerformanceIssue?.Invoke("LowFPS", fps);
                }
            }
        }

        private void TrackFrameTime()
        {
            frameTimes[frameTimeIndex] = Time.unscaledDeltaTime * 1000f;
            frameTimeIndex = (frameTimeIndex + 1) % frameTimes.Length;

            float sum = 0;
            for (int i = 0; i < frameTimes.Length; i++)
                sum += frameTimes[i];
            averageFrameTime = sum / frameTimes.Length;
        }

        #endregion

        #region Memory Tracking

        private void UpdateMemory()
        {
            if (Time.unscaledTime < memoryNextUpdate) return;

            memoryNextUpdate = Time.unscaledTime + memoryUpdateInterval;

            currentMemoryMB = GC.GetTotalMemory(false) / (1024 * 1024);

            if (currentMemoryMB > peakMemoryMB)
                peakMemoryMB = currentMemoryMB;

            if (currentMemoryMB > memoryWarningThresholdMB)
            {
                OnMemoryWarning?.Invoke(currentMemoryMB);
                Debug.LogWarning($"PerformanceMonitor: High memory usage: {currentMemoryMB}MB");
            }
        }

        /// <summary>
        /// Force garbage collection (use sparingly!)
        /// </summary>
        public void ForceGC()
        {
            var before = GC.GetTotalMemory(false);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var after = GC.GetTotalMemory(true);

            Debug.Log($"PerformanceMonitor: GC freed {(before - after) / 1024}KB");
        }

        #endregion

        #region Performance Markers

        /// <summary>
        /// Start a performance marker
        /// </summary>
        public void BeginMarker(string name)
        {
            if (!markers.ContainsKey(name))
            {
                markers[name] = new Stopwatch();
            }
            markers[name].Restart();
        }

        /// <summary>
        /// End a performance marker and record the time
        /// </summary>
        public double EndMarker(string name)
        {
            if (!markers.TryGetValue(name, out Stopwatch sw))
            {
                Debug.LogWarning($"PerformanceMonitor: Marker '{name}' was not started");
                return 0;
            }

            sw.Stop();
            double ms = sw.Elapsed.TotalMilliseconds;

            // Update stats
            if (!stats.ContainsKey(name))
            {
                stats[name] = new PerformanceStats { minMs = double.MaxValue };
            }

            var s = stats[name];
            s.samples++;
            s.totalMs += ms;
            if (ms < s.minMs) s.minMs = ms;
            if (ms > s.maxMs) s.maxMs = ms;
            stats[name] = s;

            // Warn if too slow
            if (ms > 16.67) // More than one frame at 60fps
            {
                OnPerformanceIssue?.Invoke(name, (float)ms);
            }

            return ms;
        }

        /// <summary>
        /// Measure action execution time
        /// </summary>
        public double Measure(string name, Action action)
        {
            BeginMarker(name);
            action();
            return EndMarker(name);
        }

        /// <summary>
        /// Get stats for a marker
        /// </summary>
        public (double avg, double min, double max, int samples) GetMarkerStats(string name)
        {
            if (stats.TryGetValue(name, out var s))
            {
                return (s.AverageMs, s.minMs, s.maxMs, s.samples);
            }
            return (0, 0, 0, 0);
        }

        /// <summary>
        /// Clear all marker stats
        /// </summary>
        public void ClearStats()
        {
            stats.Clear();
        }

        #endregion

        #region Debug UI

        private void OnGUI()
        {
            if (!showDebugUI) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.BeginVertical("box");

            // Title
            GUILayout.Label("<b>Performance Monitor</b>");
            GUILayout.Space(5);

            // FPS
            string fpsColor = fps >= targetFPS * 0.9f ? "green" : fps >= targetFPS * 0.5f ? "yellow" : "red";
            GUILayout.Label($"FPS: <color={fpsColor}>{fps:F1}</color> (min: {minFPS:F1}, max: {maxFPS:F1})");
            GUILayout.Label($"Frame Time: {averageFrameTime:F2}ms");

            GUILayout.Space(10);

            // Memory
            string memColor = currentMemoryMB < memoryWarningThresholdMB * 0.7f ? "green" :
                currentMemoryMB < memoryWarningThresholdMB ? "yellow" : "red";
            GUILayout.Label($"Memory: <color={memColor}>{currentMemoryMB}MB</color> (peak: {peakMemoryMB}MB)");

            if (GUILayout.Button("Force GC"))
            {
                ForceGC();
            }

            GUILayout.Space(10);

            // Performance markers
            GUILayout.Label("<b>Performance Markers:</b>");
            foreach (var kvp in stats)
            {
                var s = kvp.Value;
                GUILayout.Label($"{kvp.Key}: {s.AverageMs:F2}ms avg ({s.samples} samples)");
            }

            if (GUILayout.Button("Clear Stats"))
            {
                ClearStats();
            }

            GUILayout.Space(10);

            // Pool stats
            if (PoolManager.Instance != null)
            {
                GUILayout.Label("<b>Object Pools:</b>");
                GUILayout.Label(PoolManager.Instance.GetStats());
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        #endregion

        #region Utility

        /// <summary>
        /// Get current FPS
        /// </summary>
        public float GetFPS() => fps;

        /// <summary>
        /// Get current memory usage in MB
        /// </summary>
        public long GetMemoryMB() => currentMemoryMB;

        /// <summary>
        /// Check if device is low-end
        /// </summary>
        public bool IsLowEndDevice()
        {
            return SystemInfo.systemMemorySize < 2048 ||
                   SystemInfo.graphicsMemorySize < 512 ||
                   SystemInfo.processorCount < 4;
        }

        /// <summary>
        /// Get recommended quality level
        /// </summary>
        public int GetRecommendedQualityLevel()
        {
            int memGB = SystemInfo.systemMemorySize / 1024;
            int gpuMB = SystemInfo.graphicsMemorySize;

            if (memGB >= 6 && gpuMB >= 2048)
                return 2; // High
            else if (memGB >= 3 && gpuMB >= 1024)
                return 1; // Medium
            else
                return 0; // Low
        }

        /// <summary>
        /// Generate performance report
        /// </summary>
        public string GenerateReport()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== Performance Report ===");
            sb.AppendLine($"Date: {DateTime.Now}");
            sb.AppendLine();
            sb.AppendLine("--- Device Info ---");
            sb.AppendLine($"Device: {SystemInfo.deviceModel}");
            sb.AppendLine($"OS: {SystemInfo.operatingSystem}");
            sb.AppendLine($"CPU: {SystemInfo.processorType} ({SystemInfo.processorCount} cores)");
            sb.AppendLine($"RAM: {SystemInfo.systemMemorySize}MB");
            sb.AppendLine($"GPU: {SystemInfo.graphicsDeviceName}");
            sb.AppendLine($"VRAM: {SystemInfo.graphicsMemorySize}MB");
            sb.AppendLine();
            sb.AppendLine("--- Performance ---");
            sb.AppendLine($"FPS: {fps:F1} (min: {minFPS:F1}, max: {maxFPS:F1})");
            sb.AppendLine($"Avg Frame Time: {averageFrameTime:F2}ms");
            sb.AppendLine($"Memory: {currentMemoryMB}MB (peak: {peakMemoryMB}MB)");
            sb.AppendLine();
            sb.AppendLine("--- Markers ---");
            foreach (var kvp in stats)
            {
                var s = kvp.Value;
                sb.AppendLine($"{kvp.Key}: avg={s.AverageMs:F2}ms, min={s.minMs:F2}ms, max={s.maxMs:F2}ms ({s.samples} samples)");
            }

            return sb.ToString();
        }

        #endregion
    }
}
