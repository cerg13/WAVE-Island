using UnityEngine;
using UnityEngine.UI;
using WaveIsland.Core;
using WaveIsland.Data;
using WaveIsland.Garden;

namespace WaveIsland.UI.Garden
{
    /// <summary>
    /// Popup that shows detailed info about a garden plot
    /// Can be positioned near the selected plot
    /// </summary>
    public class PlotInfoPopup : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private RectTransform popupRect;
        [SerializeField] private Vector2 offset = new Vector2(0, 100);
        [SerializeField] private Camera mainCamera;

        [Header("Content")]
        [SerializeField] private Text titleText;
        [SerializeField] private Image plantImage;
        [SerializeField] private Text plantNameText;
        [SerializeField] private Text statusText;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Text progressText;
        [SerializeField] private Text timeText;

        [Header("Stats")]
        [SerializeField] private Text harvestAmountText;
        [SerializeField] private Text seedChanceText;
        [SerializeField] private Image wateredIcon;

        [Header("Actions")]
        [SerializeField] private Button harvestButton;
        [SerializeField] private Button waterButton;
        [SerializeField] private Button plantButton;
        [SerializeField] private Button speedUpButton;
        [SerializeField] private Button closeButton;

        [Header("Speed Up")]
        [SerializeField] private int speedUpGemCost = 5;
        [SerializeField] private Text speedUpCostText;

        private int currentPlotIndex = -1;
        private PlotData currentPlot;
        private PlantData currentPlantData;
        private PlantGrowthController growthController;

        private void Awake()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            // Setup button listeners
            if (harvestButton != null)
                harvestButton.onClick.AddListener(OnHarvestClick);

            if (waterButton != null)
                waterButton.onClick.AddListener(OnWaterClick);

            if (plantButton != null)
                plantButton.onClick.AddListener(OnPlantClick);

            if (speedUpButton != null)
                speedUpButton.onClick.AddListener(OnSpeedUpClick);

            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            // Start hidden
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Show popup for a specific plot
        /// </summary>
        public void Show(int plotIndex, Vector3 worldPosition, PlantGrowthController growthCtrl)
        {
            currentPlotIndex = plotIndex;
            growthController = growthCtrl;

            var playerData = GameManager.Instance?.PlayerData;
            if (playerData == null || plotIndex >= playerData.GardenPlots.Count)
            {
                Hide();
                return;
            }

            currentPlot = playerData.GardenPlots[plotIndex];

            // Load plant data
            if (!string.IsNullOrEmpty(currentPlot.PlantId))
            {
                // TODO: Load from database
                currentPlantData = null;
            }

            // Position popup
            PositionPopup(worldPosition);

            // Update content
            UpdateContent();

            // Show
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Hide the popup
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            currentPlotIndex = -1;
            currentPlot = null;
            currentPlantData = null;
        }

        private void Update()
        {
            if (gameObject.activeSelf && currentPlotIndex >= 0)
            {
                UpdateContent();
            }
        }

        private void PositionPopup(Vector3 worldPosition)
        {
            if (popupRect == null || mainCamera == null) return;

            // Convert world to screen position
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition);

            // Apply offset
            screenPos += (Vector3)offset;

            // Clamp to screen
            Vector2 canvasSize = popupRect.parent.GetComponent<RectTransform>().sizeDelta;
            Vector2 popupSize = popupRect.sizeDelta;

            screenPos.x = Mathf.Clamp(screenPos.x, popupSize.x / 2, Screen.width - popupSize.x / 2);
            screenPos.y = Mathf.Clamp(screenPos.y, popupSize.y / 2, Screen.height - popupSize.y / 2);

            popupRect.position = screenPos;
        }

        private void UpdateContent()
        {
            if (currentPlot == null) return;

            // Title
            if (titleText != null)
                titleText.text = $"Plot #{currentPlotIndex + 1}";

            switch (currentPlot.State)
            {
                case PlotState.Empty:
                    UpdateEmptyState();
                    break;

                case PlotState.Planted:
                case PlotState.Growing:
                    UpdateGrowingState();
                    break;

                case PlotState.ReadyToHarvest:
                    UpdateReadyState();
                    break;

                case PlotState.Withering:
                    UpdateWitheringState();
                    break;

                case PlotState.Dead:
                    UpdateDeadState();
                    break;
            }

            // Speed up cost
            if (speedUpCostText != null)
                speedUpCostText.text = $"{speedUpGemCost}";
        }

        private void UpdateEmptyState()
        {
            if (plantImage != null)
                plantImage.gameObject.SetActive(false);

            if (plantNameText != null)
                plantNameText.text = "Empty";

            if (statusText != null)
                statusText.text = "Ready to plant";

            if (progressSlider != null)
                progressSlider.gameObject.SetActive(false);

            if (timeText != null)
                timeText.text = "";

            // Buttons
            SetButtonStates(plant: true, harvest: false, water: false, speedUp: false);
        }

        private void UpdateGrowingState()
        {
            if (plantImage != null && currentPlantData?.SeedIcon != null)
            {
                plantImage.gameObject.SetActive(true);
                plantImage.sprite = currentPlantData.SeedIcon;
            }

            if (plantNameText != null)
                plantNameText.text = currentPlantData?.DisplayName ?? currentPlot.PlantId;

            // Progress
            float progress = growthController?.GetGrowthProgress(currentPlotIndex) ?? 0f;
            string timeRemaining = growthController?.GetTimeRemainingFormatted(currentPlotIndex) ?? "";

            if (statusText != null)
                statusText.text = "Growing";

            if (progressSlider != null)
            {
                progressSlider.gameObject.SetActive(true);
                progressSlider.value = progress;
            }

            if (progressText != null)
                progressText.text = $"{Mathf.FloorToInt(progress * 100)}%";

            if (timeText != null)
                timeText.text = timeRemaining;

            // Harvest stats
            if (harvestAmountText != null && currentPlantData != null)
                harvestAmountText.text = $"{currentPlantData.MinHarvest}-{currentPlantData.MaxHarvest}";

            if (seedChanceText != null && currentPlantData != null)
                seedChanceText.text = $"{Mathf.RoundToInt(currentPlantData.SeedDropChance * 100)}%";

            // Watered indicator
            if (wateredIcon != null)
            {
                bool isWatered = currentPlot.LastWateredAt > 0 &&
                    (System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() - currentPlot.LastWateredAt) < 4 * 3600;
                wateredIcon.gameObject.SetActive(isWatered);
            }

            // Buttons
            bool canWater = currentPlot.LastWateredAt <= 0 ||
                (System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() - currentPlot.LastWateredAt) >= 4 * 3600;

            SetButtonStates(plant: false, harvest: false, water: canWater, speedUp: true);
        }

        private void UpdateReadyState()
        {
            if (plantImage != null && currentPlantData?.GrowthStageSprites != null &&
                currentPlantData.GrowthStageSprites.Length > 0)
            {
                plantImage.gameObject.SetActive(true);
                plantImage.sprite = currentPlantData.GrowthStageSprites[^1];
            }

            if (plantNameText != null)
                plantNameText.text = currentPlantData?.DisplayName ?? currentPlot.PlantId;

            if (statusText != null)
            {
                statusText.text = "Ready to harvest!";
                statusText.color = Color.green;
            }

            if (progressSlider != null)
            {
                progressSlider.gameObject.SetActive(true);
                progressSlider.value = 1f;
            }

            if (timeText != null)
                timeText.text = "Tap to harvest";

            // Buttons
            SetButtonStates(plant: false, harvest: true, water: false, speedUp: false);
        }

        private void UpdateWitheringState()
        {
            if (plantNameText != null)
                plantNameText.text = currentPlantData?.DisplayName ?? currentPlot.PlantId;

            if (statusText != null)
            {
                statusText.text = "Withering!";
                statusText.color = new Color(1f, 0.5f, 0f);
            }

            if (timeText != null)
                timeText.text = "Harvest now!";

            // Buttons
            SetButtonStates(plant: false, harvest: true, water: false, speedUp: false);
        }

        private void UpdateDeadState()
        {
            if (plantNameText != null)
                plantNameText.text = "Dead Plant";

            if (statusText != null)
            {
                statusText.text = "Plant has died";
                statusText.color = Color.gray;
            }

            if (progressSlider != null)
                progressSlider.gameObject.SetActive(false);

            // Only clear button would be active
            SetButtonStates(plant: false, harvest: false, water: false, speedUp: false);
        }

        private void SetButtonStates(bool plant, bool harvest, bool water, bool speedUp)
        {
            if (plantButton != null) plantButton.gameObject.SetActive(plant);
            if (harvestButton != null) harvestButton.gameObject.SetActive(harvest);
            if (waterButton != null) waterButton.gameObject.SetActive(water);
            if (speedUpButton != null) speedUpButton.gameObject.SetActive(speedUp);
        }

        // Button handlers
        private void OnHarvestClick()
        {
            GardenManager.Instance?.Harvest(currentPlotIndex);
            Hide();
        }

        private void OnWaterClick()
        {
            GardenManager.Instance?.WaterPlant(currentPlotIndex);
            UpdateContent();
        }

        private void OnPlantClick()
        {
            // TODO: Open seed selection
            Hide();
        }

        private void OnSpeedUpClick()
        {
            var playerData = GameManager.Instance?.PlayerData;
            if (playerData == null) return;

            if (playerData.Gems >= speedUpGemCost)
            {
                playerData.Gems -= speedUpGemCost;

                // Instantly complete growth
                if (currentPlot != null && !string.IsNullOrEmpty(currentPlot.PlantId))
                {
                    // Set planted time to past so it's ready now
                    currentPlot.PlantedAt = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() -
                        (long)(currentPlantData?.GrowthTimeSeconds ?? 0) - 1;

                    Debug.Log($"Speed up used! Plant is now ready.");
                    UpdateContent();
                }
            }
            else
            {
                Debug.Log("Not enough gems!");
                // TODO: Show gem shop
            }
        }
    }
}
