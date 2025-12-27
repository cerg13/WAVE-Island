using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using WaveIsland.Core;
using WaveIsland.Garden;
using WaveIsland.Data;

namespace WaveIsland.UI.Garden
{
    /// <summary>
    /// Main controller for Garden UI
    /// Manages seed selection, plot info, and garden actions
    /// </summary>
    public class GardenUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GardenManager gardenManager;
        [SerializeField] private GardenGrid gardenGrid;
        [SerializeField] private PlantGrowthController growthController;

        [Header("Main Panels")]
        [SerializeField] private GameObject gardenPanel;
        [SerializeField] private GameObject seedSelectionPanel;
        [SerializeField] private GameObject plotInfoPanel;

        [Header("Seed Selection")]
        [SerializeField] private Transform seedListContent;
        [SerializeField] private GameObject seedItemPrefab;
        [SerializeField] private Button closeSeedPanelButton;

        [Header("Plot Info")]
        [SerializeField] private Text plotTitleText;
        [SerializeField] private Image plantIcon;
        [SerializeField] private Text plantNameText;
        [SerializeField] private Text growthStatusText;
        [SerializeField] private Slider growthProgressBar;
        [SerializeField] private Text timeRemainingText;
        [SerializeField] private Button harvestButton;
        [SerializeField] private Button waterButton;
        [SerializeField] private Button plantButton;
        [SerializeField] private Button removePlantButton;

        [Header("Garden Status")]
        [SerializeField] private Text gardenLevelText;
        [SerializeField] private Text plotCountText;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button harvestAllButton;
        [SerializeField] private Button waterAllButton;

        [Header("Seeds Database")]
        [SerializeField] private List<PlantData> availablePlants;

        private int selectedPlotIndex = -1;
        private PlantData selectedSeed = null;
        private bool isPlantingMode = false;

        private void Start()
        {
            InitializeUI();
            SetupEventListeners();
            RefreshGardenStatus();
        }

        private void Update()
        {
            // Update selected plot info
            if (selectedPlotIndex >= 0 && plotInfoPanel.activeSelf)
            {
                UpdatePlotInfo();
            }
        }

        private void InitializeUI()
        {
            // Hide panels initially
            if (seedSelectionPanel != null)
                seedSelectionPanel.SetActive(false);

            if (plotInfoPanel != null)
                plotInfoPanel.SetActive(false);

            // Setup buttons
            if (harvestButton != null)
                harvestButton.onClick.AddListener(OnHarvestClicked);

            if (waterButton != null)
                waterButton.onClick.AddListener(OnWaterClicked);

            if (plantButton != null)
                plantButton.onClick.AddListener(OnPlantClicked);

            if (removePlantButton != null)
                removePlantButton.onClick.AddListener(OnRemovePlantClicked);

            if (harvestAllButton != null)
                harvestAllButton.onClick.AddListener(OnHarvestAllClicked);

            if (waterAllButton != null)
                waterAllButton.onClick.AddListener(OnWaterAllClicked);

            if (upgradeButton != null)
                upgradeButton.onClick.AddListener(OnUpgradeClicked);

            if (closeSeedPanelButton != null)
                closeSeedPanelButton.onClick.AddListener(CloseSeedSelection);
        }

        private void SetupEventListeners()
        {
            if (gardenGrid != null)
            {
                gardenGrid.OnPlotClicked += OnPlotClicked;
            }

            if (gardenManager != null)
            {
                gardenManager.OnPlotStateChanged += OnPlotStateChanged;
                gardenManager.OnHarvest += OnHarvestComplete;
            }
        }

        private void OnDestroy()
        {
            if (gardenGrid != null)
            {
                gardenGrid.OnPlotClicked -= OnPlotClicked;
            }

            if (gardenManager != null)
            {
                gardenManager.OnPlotStateChanged -= OnPlotStateChanged;
                gardenManager.OnHarvest -= OnHarvestComplete;
            }
        }

        /// <summary>
        /// Refresh the overall garden status display
        /// </summary>
        public void RefreshGardenStatus()
        {
            var playerData = GameManager.Instance?.PlayerData;
            if (playerData == null) return;

            // Garden level
            if (gardenLevelText != null)
                gardenLevelText.text = $"Garden Lv.{playerData.GardenLevel}";

            // Plot count
            int unlockedPlots = 0;
            int readyPlots = 0;

            foreach (var plot in playerData.GardenPlots)
            {
                if (plot.IsUnlocked) unlockedPlots++;
                if (plot.State == PlotState.ReadyToHarvest) readyPlots++;
            }

            if (plotCountText != null)
                plotCountText.text = $"Plots: {unlockedPlots}/20";

            // Harvest all button
            if (harvestAllButton != null)
                harvestAllButton.interactable = readyPlots > 0;

            // Upgrade button
            if (upgradeButton != null)
            {
                int upgradeCost = GetUpgradeCost(playerData.GardenLevel);
                upgradeButton.interactable = playerData.Coins >= upgradeCost && playerData.GardenLevel < 8;
            }
        }

        /// <summary>
        /// Handle plot click
        /// </summary>
        private void OnPlotClicked(int plotIndex)
        {
            if (isPlantingMode && selectedSeed != null)
            {
                // Try to plant
                TryPlantSeed(plotIndex);
            }
            else
            {
                // Show plot info
                SelectPlot(plotIndex);
            }
        }

        /// <summary>
        /// Select and show info for a plot
        /// </summary>
        private void SelectPlot(int plotIndex)
        {
            selectedPlotIndex = plotIndex;

            var playerData = GameManager.Instance?.PlayerData;
            if (playerData == null || plotIndex >= playerData.GardenPlots.Count)
            {
                plotInfoPanel?.SetActive(false);
                return;
            }

            var plot = playerData.GardenPlots[plotIndex];

            // Show plot info panel
            plotInfoPanel?.SetActive(true);

            // Update info
            UpdatePlotInfo();
        }

        /// <summary>
        /// Update the plot info panel
        /// </summary>
        private void UpdatePlotInfo()
        {
            var playerData = GameManager.Instance?.PlayerData;
            if (playerData == null || selectedPlotIndex >= playerData.GardenPlots.Count)
                return;

            var plot = playerData.GardenPlots[selectedPlotIndex];

            // Title
            if (plotTitleText != null)
                plotTitleText.text = $"Plot #{selectedPlotIndex + 1}";

            // State-specific display
            switch (plot.State)
            {
                case PlotState.Empty:
                    ShowEmptyPlotInfo();
                    break;

                case PlotState.Planted:
                case PlotState.Growing:
                    ShowGrowingPlotInfo(plot);
                    break;

                case PlotState.ReadyToHarvest:
                    ShowReadyPlotInfo(plot);
                    break;

                case PlotState.Withering:
                    ShowWitheringPlotInfo(plot);
                    break;

                case PlotState.Dead:
                    ShowDeadPlotInfo();
                    break;
            }
        }

        private void ShowEmptyPlotInfo()
        {
            if (plantNameText != null)
                plantNameText.text = "Empty Plot";

            if (growthStatusText != null)
                growthStatusText.text = "Ready to plant";

            if (growthProgressBar != null)
                growthProgressBar.gameObject.SetActive(false);

            if (timeRemainingText != null)
                timeRemainingText.text = "";

            // Buttons
            if (plantButton != null) plantButton.gameObject.SetActive(true);
            if (harvestButton != null) harvestButton.gameObject.SetActive(false);
            if (waterButton != null) waterButton.gameObject.SetActive(false);
            if (removePlantButton != null) removePlantButton.gameObject.SetActive(false);
        }

        private void ShowGrowingPlotInfo(PlotData plot)
        {
            // Get plant data
            var plantData = GetPlantData(plot.PlantId);

            if (plantNameText != null)
                plantNameText.text = plantData?.DisplayName ?? plot.PlantId;

            if (plantIcon != null && plantData != null)
            {
                plantIcon.sprite = plantData.SeedIcon;
                plantIcon.enabled = true;
            }

            // Growth progress
            float progress = growthController?.GetGrowthProgress(selectedPlotIndex) ?? 0f;

            if (growthStatusText != null)
                growthStatusText.text = $"Growing... {Mathf.FloorToInt(progress * 100)}%";

            if (growthProgressBar != null)
            {
                growthProgressBar.gameObject.SetActive(true);
                growthProgressBar.value = progress;
            }

            if (timeRemainingText != null)
            {
                string time = growthController?.GetTimeRemainingFormatted(selectedPlotIndex) ?? "";
                timeRemainingText.text = time;
            }

            // Buttons
            if (plantButton != null) plantButton.gameObject.SetActive(false);
            if (harvestButton != null) harvestButton.gameObject.SetActive(false);
            if (waterButton != null)
            {
                waterButton.gameObject.SetActive(true);
                // Disable if recently watered
                bool canWater = CanWater(plot);
                waterButton.interactable = canWater;
            }
            if (removePlantButton != null) removePlantButton.gameObject.SetActive(true);
        }

        private void ShowReadyPlotInfo(PlotData plot)
        {
            var plantData = GetPlantData(plot.PlantId);

            if (plantNameText != null)
                plantNameText.text = plantData?.DisplayName ?? plot.PlantId;

            if (growthStatusText != null)
                growthStatusText.text = "Ready to harvest!";

            if (growthProgressBar != null)
            {
                growthProgressBar.gameObject.SetActive(true);
                growthProgressBar.value = 1f;
            }

            if (timeRemainingText != null)
                timeRemainingText.text = "Harvest now!";

            // Buttons
            if (plantButton != null) plantButton.gameObject.SetActive(false);
            if (harvestButton != null)
            {
                harvestButton.gameObject.SetActive(true);
                harvestButton.interactable = true;
            }
            if (waterButton != null) waterButton.gameObject.SetActive(false);
            if (removePlantButton != null) removePlantButton.gameObject.SetActive(false);
        }

        private void ShowWitheringPlotInfo(PlotData plot)
        {
            var plantData = GetPlantData(plot.PlantId);

            if (plantNameText != null)
                plantNameText.text = plantData?.DisplayName ?? plot.PlantId;

            if (growthStatusText != null)
                growthStatusText.text = "Withering! Harvest soon!";

            if (timeRemainingText != null)
                timeRemainingText.text = "Hurry!";

            // Buttons - same as ready
            if (plantButton != null) plantButton.gameObject.SetActive(false);
            if (harvestButton != null)
            {
                harvestButton.gameObject.SetActive(true);
                harvestButton.interactable = true;
            }
            if (waterButton != null) waterButton.gameObject.SetActive(false);
            if (removePlantButton != null) removePlantButton.gameObject.SetActive(false);
        }

        private void ShowDeadPlotInfo()
        {
            if (plantNameText != null)
                plantNameText.text = "Dead Plant";

            if (growthStatusText != null)
                growthStatusText.text = "The plant has died";

            if (timeRemainingText != null)
                timeRemainingText.text = "";

            // Only show remove button
            if (plantButton != null) plantButton.gameObject.SetActive(false);
            if (harvestButton != null) harvestButton.gameObject.SetActive(false);
            if (waterButton != null) waterButton.gameObject.SetActive(false);
            if (removePlantButton != null)
            {
                removePlantButton.gameObject.SetActive(true);
                removePlantButton.GetComponentInChildren<Text>().text = "Clear";
            }
        }

        // Button handlers
        private void OnPlantClicked()
        {
            OpenSeedSelection();
        }

        private void OnHarvestClicked()
        {
            if (selectedPlotIndex >= 0)
            {
                gardenManager?.Harvest(selectedPlotIndex);
            }
        }

        private void OnWaterClicked()
        {
            if (selectedPlotIndex >= 0)
            {
                gardenManager?.WaterPlant(selectedPlotIndex);
                UpdatePlotInfo();
            }
        }

        private void OnRemovePlantClicked()
        {
            // Show confirmation dialog
            UIManager.Instance?.ShowConfirm(
                "Remove this plant?",
                () => {
                    // TODO: Implement remove plant in GardenManager
                    Debug.Log($"Remove plant from plot {selectedPlotIndex}");
                    RefreshGardenStatus();
                }
            );
        }

        private void OnHarvestAllClicked()
        {
            var playerData = GameManager.Instance?.PlayerData;
            if (playerData == null) return;

            int harvested = 0;
            for (int i = 0; i < playerData.GardenPlots.Count; i++)
            {
                if (playerData.GardenPlots[i].State == PlotState.ReadyToHarvest ||
                    playerData.GardenPlots[i].State == PlotState.Withering)
                {
                    if (gardenManager.Harvest(i))
                        harvested++;
                }
            }

            if (harvested > 0)
            {
                Debug.Log($"Harvested {harvested} plants!");
                // TODO: Show harvest summary popup
            }

            RefreshGardenStatus();
        }

        private void OnWaterAllClicked()
        {
            var playerData = GameManager.Instance?.PlayerData;
            if (playerData == null) return;

            int watered = 0;
            for (int i = 0; i < playerData.GardenPlots.Count; i++)
            {
                var plot = playerData.GardenPlots[i];
                if ((plot.State == PlotState.Growing || plot.State == PlotState.Planted) &&
                    CanWater(plot))
                {
                    if (gardenManager.WaterPlant(i))
                        watered++;
                }
            }

            if (watered > 0)
            {
                Debug.Log($"Watered {watered} plants!");
            }
        }

        private void OnUpgradeClicked()
        {
            var playerData = GameManager.Instance?.PlayerData;
            if (playerData == null) return;

            int cost = GetUpgradeCost(playerData.GardenLevel);

            UIManager.Instance?.ShowConfirm(
                $"Upgrade garden for {cost} coins?",
                () => {
                    if (GameManager.Instance.SpendCoins(cost))
                    {
                        playerData.GardenLevel++;
                        gardenGrid?.InitializeGrid();
                        RefreshGardenStatus();
                        Debug.Log($"Garden upgraded to level {playerData.GardenLevel}!");
                    }
                }
            );
        }

        // Seed selection
        private void OpenSeedSelection()
        {
            if (seedSelectionPanel == null) return;

            seedSelectionPanel.SetActive(true);
            PopulateSeedList();
        }

        private void CloseSeedSelection()
        {
            seedSelectionPanel?.SetActive(false);
            isPlantingMode = false;
            selectedSeed = null;
            gardenGrid?.HighlightEmptyPlots(false);
        }

        private void PopulateSeedList()
        {
            if (seedListContent == null) return;

            // Clear existing items
            foreach (Transform child in seedListContent)
            {
                Destroy(child.gameObject);
            }

            var playerData = GameManager.Instance?.PlayerData;
            if (playerData == null) return;

            // Get seeds from inventory
            foreach (var item in playerData.Inventory)
            {
                if (!item.ItemId.StartsWith("seed_")) continue;

                string plantId = item.ItemId.Replace("seed_", "");
                var plantData = GetPlantData(plantId);

                if (plantData != null)
                {
                    CreateSeedItem(plantData, item.Quantity);
                }
            }
        }

        private void CreateSeedItem(PlantData plantData, int quantity)
        {
            GameObject item = seedItemPrefab != null
                ? Instantiate(seedItemPrefab, seedListContent)
                : new GameObject("SeedItem");

            // Setup UI
            var seedUI = item.GetComponent<SeedListItem>();
            if (seedUI == null)
            {
                seedUI = item.AddComponent<SeedListItem>();
            }

            seedUI.Setup(plantData, quantity, OnSeedSelected);
        }

        private void OnSeedSelected(PlantData seed)
        {
            selectedSeed = seed;
            isPlantingMode = true;

            // Highlight empty plots
            gardenGrid?.HighlightEmptyPlots(true);

            // Hide seed panel
            seedSelectionPanel?.SetActive(false);

            Debug.Log($"Selected seed: {seed.DisplayName}. Click empty plot to plant.");
        }

        private void TryPlantSeed(int plotIndex)
        {
            if (selectedSeed == null) return;

            bool success = gardenManager?.PlantSeed(plotIndex, selectedSeed.Id) ?? false;

            if (success)
            {
                Debug.Log($"Planted {selectedSeed.DisplayName} in plot {plotIndex}");

                // Exit planting mode if no more seeds
                var playerData = GameManager.Instance?.PlayerData;
                if (!playerData.HasItem($"seed_{selectedSeed.Id}"))
                {
                    isPlantingMode = false;
                    selectedSeed = null;
                    gardenGrid?.HighlightEmptyPlots(false);
                }

                RefreshGardenStatus();
            }
        }

        // Event handlers
        private void OnPlotStateChanged(int plotIndex, PlotState state)
        {
            if (plotIndex == selectedPlotIndex)
            {
                UpdatePlotInfo();
            }
            RefreshGardenStatus();
        }

        private void OnHarvestComplete(int plotIndex, IngredientData ingredient, int amount)
        {
            // Show harvest popup
            UIManager.Instance?.ShowReward(
                "Harvest!",
                $"+{amount} {ingredient.DisplayName}",
                ingredient.Icon
            );

            RefreshGardenStatus();
        }

        // Helpers
        private bool CanWater(PlotData plot)
        {
            if (plot.LastWateredAt <= 0) return true;

            long now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            float timeSinceWater = now - plot.LastWateredAt;

            // Can water every 4 hours
            return timeSinceWater >= 4 * 3600;
        }

        private int GetUpgradeCost(int currentLevel)
        {
            int[] costs = { 0, 500, 1500, 3000, 5000, 8000, 12000, 20000 };
            return currentLevel < costs.Length ? costs[currentLevel] : 99999;
        }

        private PlantData GetPlantData(string plantId)
        {
            return availablePlants?.Find(p => p.Id == plantId);
        }
    }
}
