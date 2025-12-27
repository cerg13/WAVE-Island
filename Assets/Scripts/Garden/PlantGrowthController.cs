using UnityEngine;
using System.Collections.Generic;
using WaveIsland.Core;
using WaveIsland.Data;

namespace WaveIsland.Garden
{
    /// <summary>
    /// Controls plant growth timing and state transitions
    /// Works with GardenManager to update plot states
    /// </summary>
    public class PlantGrowthController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float updateInterval = 1f; // Check every second
        [SerializeField] private bool enableOfflineProgress = true;

        [Header("References")]
        [SerializeField] private GardenManager gardenManager;
        [SerializeField] private GardenGrid gardenGrid;

        [Header("Notifications")]
        [SerializeField] private bool notifyOnReady = true;
        [SerializeField] private bool notifyOnWithering = true;

        private float updateTimer = 0f;
        private Dictionary<int, PlantGrowthData> growthTracking = new Dictionary<int, PlantGrowthData>();

        public event System.Action<int, PlantData> OnPlantReady;
        public event System.Action<int, PlantData> OnPlantWithering;
        public event System.Action<int> OnPlantDied;

        private void Start()
        {
            if (gardenManager == null)
                gardenManager = GardenManager.Instance;

            // Process offline progress on start
            if (enableOfflineProgress)
            {
                ProcessOfflineProgress();
            }

            // Subscribe to garden events
            if (gardenManager != null)
            {
                gardenManager.OnPlotStateChanged += HandlePlotStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (gardenManager != null)
            {
                gardenManager.OnPlotStateChanged -= HandlePlotStateChanged;
            }
        }

        private void Update()
        {
            updateTimer += Time.deltaTime;

            if (updateTimer >= updateInterval)
            {
                updateTimer = 0f;
                UpdateAllPlants();
            }
        }

        /// <summary>
        /// Update growth state for all plants
        /// </summary>
        private void UpdateAllPlants()
        {
            var playerData = GameManager.Instance?.PlayerData;
            if (playerData == null) return;

            long now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            for (int i = 0; i < playerData.GardenPlots.Count; i++)
            {
                var plot = playerData.GardenPlots[i];

                if (string.IsNullOrEmpty(plot.PlantId) || plot.PlantedAt <= 0)
                    continue;

                UpdatePlantGrowth(i, plot, now);
            }
        }

        private void UpdatePlantGrowth(int plotIndex, PlotData plot, long now)
        {
            // Get plant data
            var plantData = GetPlantData(plot.PlantId);
            if (plantData == null) return;

            float elapsed = now - plot.PlantedAt;
            float growthTime = plantData.GrowthTimeSeconds;

            // Apply watered bonus
            if (plot.LastWateredAt > 0)
            {
                growthTime /= plantData.WateredSpeedMultiplier;
            }

            PlotState newState = plot.State;

            if (elapsed >= growthTime)
            {
                // Plant is ready
                float timeSinceReady = elapsed - growthTime;

                if (timeSinceReady >= plantData.WitherTimeSeconds)
                {
                    // Plant died
                    newState = PlotState.Dead;

                    if (plot.State != PlotState.Dead)
                    {
                        OnPlantDied?.Invoke(plotIndex);
                        Debug.Log($"[Growth] Plant in plot {plotIndex} died!");
                    }
                }
                else if (timeSinceReady >= plantData.WitherTimeSeconds * 0.75f)
                {
                    // Withering warning
                    newState = PlotState.Withering;

                    if (plot.State != PlotState.Withering && notifyOnWithering)
                    {
                        OnPlantWithering?.Invoke(plotIndex, plantData);
                        Debug.Log($"[Growth] Plant in plot {plotIndex} is withering!");
                    }
                }
                else
                {
                    // Ready to harvest
                    newState = PlotState.ReadyToHarvest;

                    if (plot.State != PlotState.ReadyToHarvest && notifyOnReady)
                    {
                        OnPlantReady?.Invoke(plotIndex, plantData);
                        Debug.Log($"[Growth] Plant in plot {plotIndex} is ready!");
                    }
                }
            }
            else
            {
                // Still growing
                newState = PlotState.Growing;
            }

            // Update state if changed
            if (newState != plot.State)
            {
                plot.State = newState;
                gardenGrid?.UpdatePlotVisual(plotIndex, newState, plot.PlantId);
            }

            // Track growth progress
            if (!growthTracking.ContainsKey(plotIndex))
            {
                growthTracking[plotIndex] = new PlantGrowthData();
            }

            growthTracking[plotIndex].Progress = Mathf.Clamp01(elapsed / growthTime);
            growthTracking[plotIndex].TimeRemaining = Mathf.Max(0, growthTime - elapsed);
        }

        /// <summary>
        /// Process any growth that happened while offline
        /// </summary>
        private void ProcessOfflineProgress()
        {
            var playerData = GameManager.Instance?.PlayerData;
            if (playerData == null) return;

            long now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long lastSave = playerData.LastSaveAt;

            if (lastSave <= 0)
                return;

            float offlineTime = now - lastSave;
            float maxOfflineHours = Utils.Constants.OFFLINE_PROGRESS_CAP_HOURS;

            // Cap offline progress
            offlineTime = Mathf.Min(offlineTime, maxOfflineHours * 3600f);

            Debug.Log($"[Growth] Processing {offlineTime / 60f:F1} minutes of offline progress");

            // Update each plot
            for (int i = 0; i < playerData.GardenPlots.Count; i++)
            {
                var plot = playerData.GardenPlots[i];

                if (string.IsNullOrEmpty(plot.PlantId) || plot.PlantedAt <= 0)
                    continue;

                UpdatePlantGrowth(i, plot, now);
            }

            // Notify of any plants ready
            int readyCount = 0;
            foreach (var plot in playerData.GardenPlots)
            {
                if (plot.State == PlotState.ReadyToHarvest)
                    readyCount++;
            }

            if (readyCount > 0)
            {
                Debug.Log($"[Growth] {readyCount} plants ready to harvest!");
                // TODO: Show notification UI
            }
        }

        /// <summary>
        /// Get growth progress for a plot (0-1)
        /// </summary>
        public float GetGrowthProgress(int plotIndex)
        {
            if (growthTracking.TryGetValue(plotIndex, out var data))
            {
                return data.Progress;
            }
            return 0f;
        }

        /// <summary>
        /// Get time remaining until harvest (in seconds)
        /// </summary>
        public float GetTimeRemaining(int plotIndex)
        {
            if (growthTracking.TryGetValue(plotIndex, out var data))
            {
                return data.TimeRemaining;
            }
            return 0f;
        }

        /// <summary>
        /// Get formatted time remaining string
        /// </summary>
        public string GetTimeRemainingFormatted(int plotIndex)
        {
            float seconds = GetTimeRemaining(plotIndex);

            if (seconds <= 0)
                return "Ready!";

            if (seconds < 60)
                return $"{Mathf.CeilToInt(seconds)}s";

            if (seconds < 3600)
                return $"{Mathf.FloorToInt(seconds / 60)}m {Mathf.FloorToInt(seconds % 60)}s";

            float hours = seconds / 3600f;
            return $"{Mathf.FloorToInt(hours)}h {Mathf.FloorToInt((seconds % 3600) / 60)}m";
        }

        private void HandlePlotStateChanged(int plotIndex, PlotState state)
        {
            // Reset tracking when plot is cleared
            if (state == PlotState.Empty)
            {
                growthTracking.Remove(plotIndex);
            }
        }

        private PlantData GetPlantData(string plantId)
        {
            // TODO: Load from ScriptableObject database or Resources
            // For now, return null and let GardenManager handle it
            return null;
        }
    }

    /// <summary>
    /// Runtime tracking data for plant growth
    /// </summary>
    public class PlantGrowthData
    {
        public float Progress;
        public float TimeRemaining;
    }
}
