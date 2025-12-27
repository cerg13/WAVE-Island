using UnityEngine;
using System.Collections.Generic;
using WaveIsland.Core;
using WaveIsland.Data;

namespace WaveIsland.Garden
{
    /// <summary>
    /// Manages all garden-related functionality
    /// Handles planting, growing, harvesting, and plot management
    /// </summary>
    public class GardenManager : MonoBehaviour
    {
        public static GardenManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private int baseMaxPlots = 4;
        [SerializeField] private int plotsPerLevel = 2;

        [Header("Plants Database")]
        [SerializeField] private List<PlantData> allPlants;

        [Header("Active Plots")]
        private List<GardenPlot> plots = new List<GardenPlot>();

        public event System.Action<int, PlotState> OnPlotStateChanged;
        public event System.Action<int, IngredientData, int> OnHarvest;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            InitializePlots();
        }

        private void Update()
        {
            UpdateAllPlots();
        }

        private void InitializePlots()
        {
            var playerData = GameManager.Instance.PlayerData;

            foreach (var plotData in playerData.GardenPlots)
            {
                var plot = new GardenPlot(plotData, GetPlantById(plotData.PlantId));
                plots.Add(plot);
            }

            Debug.Log($"[GardenManager] Initialized {plots.Count} plots");
        }

        private void UpdateAllPlots()
        {
            foreach (var plot in plots)
            {
                if (plot.State == PlotState.Growing || plot.State == PlotState.ReadyToHarvest)
                {
                    var oldState = plot.State;
                    plot.UpdateGrowth();

                    if (oldState != plot.State)
                    {
                        OnPlotStateChanged?.Invoke(plot.PlotIndex, plot.State);
                    }
                }
            }
        }

        /// <summary>
        /// Plant a seed in a plot
        /// </summary>
        public bool PlantSeed(int plotIndex, string plantId)
        {
            if (plotIndex < 0 || plotIndex >= plots.Count)
            {
                Debug.LogWarning($"[GardenManager] Invalid plot index: {plotIndex}");
                return false;
            }

            var plot = plots[plotIndex];
            if (plot.State != PlotState.Empty)
            {
                Debug.LogWarning($"[GardenManager] Plot {plotIndex} is not empty");
                return false;
            }

            var plant = GetPlantById(plantId);
            if (plant == null)
            {
                Debug.LogError($"[GardenManager] Plant not found: {plantId}");
                return false;
            }

            var playerData = GameManager.Instance.PlayerData;
            string seedId = $"seed_{plantId}";

            if (!playerData.HasItem(seedId))
            {
                Debug.LogWarning($"[GardenManager] No seeds for {plantId}");
                return false;
            }

            // Consume seed
            playerData.RemoveItem(seedId);

            // Plant
            plot.Plant(plant);

            // Update player data
            var plotData = playerData.GardenPlots[plotIndex];
            plotData.PlantId = plantId;
            plotData.PlantedAt = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            plotData.State = PlotState.Planted;

            OnPlotStateChanged?.Invoke(plotIndex, PlotState.Planted);
            Debug.Log($"[GardenManager] Planted {plantId} in plot {plotIndex}");

            return true;
        }

        /// <summary>
        /// Water a plant
        /// </summary>
        public bool WaterPlant(int plotIndex)
        {
            if (plotIndex < 0 || plotIndex >= plots.Count)
                return false;

            var plot = plots[plotIndex];
            if (plot.State != PlotState.Growing && plot.State != PlotState.Planted)
                return false;

            plot.Water();

            var playerData = GameManager.Instance.PlayerData;
            playerData.GardenPlots[plotIndex].LastWateredAt =
                System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            Debug.Log($"[GardenManager] Watered plot {plotIndex}");
            return true;
        }

        /// <summary>
        /// Harvest a ready plant
        /// </summary>
        public bool Harvest(int plotIndex)
        {
            if (plotIndex < 0 || plotIndex >= plots.Count)
                return false;

            var plot = plots[plotIndex];
            if (plot.State != PlotState.ReadyToHarvest)
            {
                Debug.LogWarning($"[GardenManager] Plot {plotIndex} is not ready");
                return false;
            }

            var playerData = GameManager.Instance.PlayerData;
            var plant = plot.CurrentPlant;

            // Calculate harvest
            int harvestAmount = plant.GetHarvestAmount(plot.IsWatered);
            bool seedDrop = plant.RollSeedDrop();

            // Add to inventory
            playerData.AddItem(plant.HarvestIngredient.Id, harvestAmount);

            if (seedDrop)
            {
                playerData.AddItem($"seed_{plant.Id}");
                Debug.Log($"[GardenManager] Bonus seed dropped!");
            }

            // Clear plot
            plot.Clear();

            var plotData = playerData.GardenPlots[plotIndex];
            plotData.PlantId = null;
            plotData.PlantedAt = 0;
            plotData.State = PlotState.Empty;

            // Stats
            playerData.TotalHarvests++;
            GameManager.Instance.AddExperience(10);

            OnHarvest?.Invoke(plotIndex, plant.HarvestIngredient, harvestAmount);
            OnPlotStateChanged?.Invoke(plotIndex, PlotState.Empty);

            Debug.Log($"[GardenManager] Harvested {harvestAmount}x {plant.HarvestIngredient.DisplayName}");

            return true;
        }

        /// <summary>
        /// Get maximum plots for current level
        /// </summary>
        public int GetMaxPlots()
        {
            int level = GameManager.Instance.PlayerData.GardenLevel;
            return baseMaxPlots + (level - 1) * plotsPerLevel;
        }

        private PlantData GetPlantById(string plantId)
        {
            if (string.IsNullOrEmpty(plantId)) return null;
            return allPlants.Find(p => p.Id == plantId);
        }
    }

    /// <summary>
    /// Runtime garden plot state
    /// </summary>
    public class GardenPlot
    {
        public int PlotIndex { get; private set; }
        public PlotState State { get; private set; }
        public PlantData CurrentPlant { get; private set; }
        public float GrowthProgress { get; private set; }
        public bool IsWatered { get; private set; }

        private long plantedAt;
        private long lastWateredAt;

        public GardenPlot(PlotData data, PlantData plant)
        {
            PlotIndex = data.PlotIndex;
            State = data.State;
            CurrentPlant = plant;
            plantedAt = data.PlantedAt;
            lastWateredAt = data.LastWateredAt;

            if (plant != null && plantedAt > 0)
            {
                UpdateGrowth();
            }
        }

        public void Plant(PlantData plant)
        {
            CurrentPlant = plant;
            State = PlotState.Planted;
            plantedAt = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            GrowthProgress = 0f;
            IsWatered = false;
        }

        public void Water()
        {
            IsWatered = true;
            lastWateredAt = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public void UpdateGrowth()
        {
            if (CurrentPlant == null || plantedAt == 0) return;

            long now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            float elapsed = now - plantedAt;

            float growthTime = CurrentPlant.GrowthTimeSeconds;
            if (IsWatered)
            {
                growthTime /= CurrentPlant.WateredSpeedMultiplier;
            }

            GrowthProgress = Mathf.Clamp01(elapsed / growthTime);

            if (GrowthProgress >= 1f)
            {
                if (State != PlotState.ReadyToHarvest)
                {
                    State = PlotState.ReadyToHarvest;
                }

                // Check withering
                float timeSinceReady = elapsed - growthTime;
                if (timeSinceReady > CurrentPlant.WitherTimeSeconds)
                {
                    State = PlotState.Dead;
                }
                else if (timeSinceReady > CurrentPlant.WitherTimeSeconds * 0.75f)
                {
                    State = PlotState.Withering;
                }
            }
            else
            {
                State = PlotState.Growing;
            }
        }

        public void Clear()
        {
            CurrentPlant = null;
            State = PlotState.Empty;
            plantedAt = 0;
            GrowthProgress = 0f;
            IsWatered = false;
        }
    }
}
