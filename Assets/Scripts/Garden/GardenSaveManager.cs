using UnityEngine;
using System.Collections.Generic;
using WaveIsland.Core;

namespace WaveIsland.Garden
{
    /// <summary>
    /// Handles saving and loading of garden-specific data
    /// Works with the main SaveSystem
    /// </summary>
    public class GardenSaveManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GardenManager gardenManager;
        [SerializeField] private GardenGrid gardenGrid;

        [Header("Auto-save")]
        [SerializeField] private bool enableAutoSave = true;
        [SerializeField] private float autoSaveInterval = 60f;

        private float autoSaveTimer = 0f;

        private void Start()
        {
            // Load garden state on start
            LoadGarden();
        }

        private void Update()
        {
            if (enableAutoSave)
            {
                autoSaveTimer += Time.deltaTime;
                if (autoSaveTimer >= autoSaveInterval)
                {
                    autoSaveTimer = 0f;
                    SaveGarden();
                }
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveGarden();
            }
        }

        private void OnApplicationQuit()
        {
            SaveGarden();
        }

        /// <summary>
        /// Save garden state
        /// </summary>
        public void SaveGarden()
        {
            var playerData = GameManager.Instance?.PlayerData;
            if (playerData == null)
            {
                Debug.LogWarning("[GardenSave] No player data to save");
                return;
            }

            // Update timestamps
            playerData.LastSaveAt = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Garden plots are already in playerData, just trigger save
            SaveSystem.SavePlayerData(playerData);

            Debug.Log("[GardenSave] Garden saved");
        }

        /// <summary>
        /// Load garden state
        /// </summary>
        public void LoadGarden()
        {
            var playerData = GameManager.Instance?.PlayerData;
            if (playerData == null)
            {
                Debug.LogWarning("[GardenSave] No player data to load");
                return;
            }

            // Ensure minimum plots exist
            EnsureMinimumPlots(playerData);

            // Refresh grid visual
            gardenGrid?.InitializeGrid();

            Debug.Log($"[GardenSave] Loaded {playerData.GardenPlots.Count} plots");
        }

        /// <summary>
        /// Ensure player has minimum garden plots
        /// </summary>
        private void EnsureMinimumPlots(PlayerData playerData)
        {
            int minPlots = Utils.Constants.BASE_GARDEN_PLOTS +
                           (playerData.GardenLevel - 1) * Utils.Constants.PLOTS_PER_GARDEN_LEVEL;

            while (playerData.GardenPlots.Count < minPlots)
            {
                playerData.GardenPlots.Add(new PlotData
                {
                    PlotIndex = playerData.GardenPlots.Count,
                    IsUnlocked = true,
                    State = PlotState.Empty
                });
            }
        }

        /// <summary>
        /// Reset garden to initial state (for debugging/testing)
        /// </summary>
        public void ResetGarden()
        {
            var playerData = GameManager.Instance?.PlayerData;
            if (playerData == null) return;

            playerData.GardenLevel = 1;
            playerData.GardenPlots.Clear();

            // Create initial plots
            for (int i = 0; i < Utils.Constants.BASE_GARDEN_PLOTS; i++)
            {
                playerData.GardenPlots.Add(new PlotData
                {
                    PlotIndex = i,
                    IsUnlocked = true,
                    State = PlotState.Empty
                });
            }

            // Add starter seeds
            playerData.Inventory.Clear();
            playerData.AddItem("seed_mint", 5);
            playerData.AddItem("seed_lime", 3);
            playerData.AddItem("seed_basil", 3);

            SaveGarden();
            gardenGrid?.InitializeGrid();

            Debug.Log("[GardenSave] Garden reset to initial state");
        }

        /// <summary>
        /// Export garden data for cloud sync
        /// </summary>
        public string ExportGardenData()
        {
            var playerData = GameManager.Instance?.PlayerData;
            if (playerData == null) return null;

            var gardenData = new GardenExportData
            {
                GardenLevel = playerData.GardenLevel,
                Plots = playerData.GardenPlots,
                ExportedAt = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            return JsonUtility.ToJson(gardenData);
        }

        /// <summary>
        /// Import garden data from cloud sync
        /// </summary>
        public bool ImportGardenData(string json)
        {
            try
            {
                var gardenData = JsonUtility.FromJson<GardenExportData>(json);
                var playerData = GameManager.Instance?.PlayerData;

                if (playerData == null || gardenData == null)
                    return false;

                playerData.GardenLevel = gardenData.GardenLevel;
                playerData.GardenPlots = gardenData.Plots;

                gardenGrid?.InitializeGrid();
                SaveGarden();

                Debug.Log("[GardenSave] Garden data imported");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GardenSave] Import failed: {e.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Data structure for garden export/import
    /// </summary>
    [System.Serializable]
    public class GardenExportData
    {
        public int GardenLevel;
        public List<PlotData> Plots;
        public long ExportedAt;
    }
}
