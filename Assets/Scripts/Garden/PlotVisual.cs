using UnityEngine;
using UnityEngine.EventSystems;
using WaveIsland.Core;
using WaveIsland.Data;

namespace WaveIsland.Garden
{
    /// <summary>
    /// Visual representation of a single garden plot
    /// Handles rendering, animations, and input for the plot
    /// </summary>
    public class PlotVisual : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer soilRenderer;
        [SerializeField] private SpriteRenderer plantRenderer;
        [SerializeField] private SpriteRenderer highlightRenderer;
        [SerializeField] private GameObject readyIndicator;
        [SerializeField] private GameObject waterDroplet;
        [SerializeField] private ParticleSystem harvestParticles;

        [Header("Sprites")]
        [SerializeField] private Sprite emptyPlotSprite;
        [SerializeField] private Sprite plantedPlotSprite;

        [Header("Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = new Color(1f, 1f, 0.5f);
        [SerializeField] private Color highlightColor = new Color(0.5f, 1f, 0.5f);
        [SerializeField] private Color witheringColor = new Color(1f, 0.7f, 0.5f);
        [SerializeField] private Color deadColor = new Color(0.5f, 0.4f, 0.3f);

        public int PlotIndex { get; private set; }
        public bool IsUnlocked { get; private set; }
        public PlotState CurrentState { get; private set; }
        public string CurrentPlantId { get; private set; }

        private GardenGrid parentGrid;
        private bool isSelected = false;
        private bool isHighlighted = false;
        private bool isHovered = false;

        // Growth progress tracking
        private float growthProgress = 0f;
        private PlantData currentPlantData;

        public void Initialize(int index, bool unlocked, GardenGrid grid)
        {
            PlotIndex = index;
            IsUnlocked = unlocked;
            parentGrid = grid;
            CurrentState = PlotState.Empty;

            SetupVisuals();
        }

        private void SetupVisuals()
        {
            // Create renderers if not assigned
            if (soilRenderer == null)
            {
                var soilObj = new GameObject("Soil");
                soilObj.transform.SetParent(transform);
                soilObj.transform.localPosition = Vector3.zero;
                soilRenderer = soilObj.AddComponent<SpriteRenderer>();
                soilRenderer.sprite = emptyPlotSprite;
            }

            if (plantRenderer == null)
            {
                var plantObj = new GameObject("Plant");
                plantObj.transform.SetParent(transform);
                plantObj.transform.localPosition = new Vector3(0, 0.1f, 0);
                plantRenderer = plantObj.AddComponent<SpriteRenderer>();
                plantRenderer.sortingOrder = 1;
            }

            if (highlightRenderer == null)
            {
                var highlightObj = new GameObject("Highlight");
                highlightObj.transform.SetParent(transform);
                highlightObj.transform.localPosition = new Vector3(0, 0.05f, 0);
                highlightRenderer = highlightObj.AddComponent<SpriteRenderer>();
                highlightRenderer.sortingOrder = -1;
                highlightRenderer.enabled = false;
            }

            // Add collider for clicks
            var collider = GetComponent<Collider>();
            if (collider == null)
            {
                var boxCollider = gameObject.AddComponent<BoxCollider>();
                boxCollider.size = new Vector3(2f, 0.5f, 2f);
            }

            UpdateVisuals();
        }

        private void Update()
        {
            if (CurrentState == PlotState.Growing)
            {
                UpdateGrowthVisual();
            }
        }

        /// <summary>
        /// Update the state and appearance of the plot
        /// </summary>
        public void UpdateState(PlotState state, string plantId = null)
        {
            CurrentState = state;
            CurrentPlantId = plantId;

            // Load plant data if planted
            if (!string.IsNullOrEmpty(plantId))
            {
                // TODO: Load from Resources or ScriptableObject database
                // currentPlantData = PlantDatabase.GetPlant(plantId);
            }

            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (!IsUnlocked)
            {
                // Locked plot appearance
                if (soilRenderer != null)
                {
                    soilRenderer.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
                }
                return;
            }

            // Update soil
            if (soilRenderer != null)
            {
                soilRenderer.sprite = CurrentState == PlotState.Empty ? emptyPlotSprite : plantedPlotSprite;
            }

            // Update plant
            if (plantRenderer != null)
            {
                switch (CurrentState)
                {
                    case PlotState.Empty:
                        plantRenderer.enabled = false;
                        break;

                    case PlotState.Planted:
                    case PlotState.Growing:
                        plantRenderer.enabled = true;
                        UpdatePlantSprite();
                        plantRenderer.color = normalColor;
                        break;

                    case PlotState.ReadyToHarvest:
                        plantRenderer.enabled = true;
                        UpdatePlantSprite();
                        plantRenderer.color = normalColor;
                        if (readyIndicator != null)
                            readyIndicator.SetActive(true);
                        break;

                    case PlotState.Withering:
                        plantRenderer.enabled = true;
                        plantRenderer.color = witheringColor;
                        break;

                    case PlotState.Dead:
                        plantRenderer.enabled = true;
                        plantRenderer.color = deadColor;
                        break;
                }
            }

            // Update indicators
            if (readyIndicator != null)
            {
                readyIndicator.SetActive(CurrentState == PlotState.ReadyToHarvest);
            }

            UpdateHighlight();
        }

        private void UpdatePlantSprite()
        {
            if (currentPlantData == null || currentPlantData.GrowthStageSprites == null)
                return;

            int stageIndex = GetGrowthStageIndex();
            if (stageIndex < currentPlantData.GrowthStageSprites.Length)
            {
                plantRenderer.sprite = currentPlantData.GrowthStageSprites[stageIndex];
            }
        }

        private int GetGrowthStageIndex()
        {
            if (CurrentState == PlotState.ReadyToHarvest)
                return currentPlantData.GrowthStageSprites.Length - 1;

            if (CurrentState == PlotState.Planted)
                return 0;

            // Growing - interpolate based on progress
            int maxStage = currentPlantData.GrowthStageSprites.Length - 2;
            return Mathf.Clamp(Mathf.FloorToInt(growthProgress * maxStage), 0, maxStage);
        }

        private void UpdateGrowthVisual()
        {
            // Calculate current growth progress
            var playerData = GameManager.Instance.PlayerData;
            if (PlotIndex >= playerData.GardenPlots.Count)
                return;

            var plotData = playerData.GardenPlots[PlotIndex];
            if (plotData.PlantedAt <= 0 || currentPlantData == null)
                return;

            long now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            float elapsed = now - plotData.PlantedAt;
            growthProgress = Mathf.Clamp01(elapsed / currentPlantData.GrowthTimeSeconds);

            // Update scale based on growth
            float scale = Mathf.Lerp(0.3f, 1f, growthProgress);
            plantRenderer.transform.localScale = new Vector3(scale, scale, 1f);

            // Update sprite for growth stage
            UpdatePlantSprite();
        }

        /// <summary>
        /// Set selected state
        /// </summary>
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            UpdateHighlight();
        }

        /// <summary>
        /// Set highlighted state (for planting mode, etc.)
        /// </summary>
        public void SetHighlighted(bool highlighted)
        {
            isHighlighted = highlighted;
            UpdateHighlight();
        }

        private void UpdateHighlight()
        {
            if (highlightRenderer == null) return;

            if (isSelected)
            {
                highlightRenderer.enabled = true;
                highlightRenderer.color = selectedColor;
            }
            else if (isHighlighted)
            {
                highlightRenderer.enabled = true;
                highlightRenderer.color = highlightColor;
            }
            else if (isHovered && IsUnlocked)
            {
                highlightRenderer.enabled = true;
                highlightRenderer.color = new Color(normalColor.r, normalColor.g, normalColor.b, 0.3f);
            }
            else
            {
                highlightRenderer.enabled = false;
            }
        }

        /// <summary>
        /// Play harvest animation
        /// </summary>
        public void PlayHarvestEffect()
        {
            if (harvestParticles != null)
            {
                harvestParticles.Play();
            }

            // Simple scale animation
            StartCoroutine(HarvestAnimation());
        }

        private System.Collections.IEnumerator HarvestAnimation()
        {
            float duration = 0.3f;
            float elapsed = 0f;
            Vector3 originalScale = plantRenderer.transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Pop and shrink
                float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.3f;
                plantRenderer.transform.localScale = originalScale * scale * (1f - t);

                yield return null;
            }

            plantRenderer.transform.localScale = originalScale;
        }

        /// <summary>
        /// Show water droplet effect
        /// </summary>
        public void ShowWaterEffect()
        {
            if (waterDroplet != null)
            {
                StartCoroutine(WaterAnimation());
            }
        }

        private System.Collections.IEnumerator WaterAnimation()
        {
            waterDroplet.SetActive(true);

            yield return new WaitForSeconds(1f);

            waterDroplet.SetActive(false);
        }

        // Input handlers
        public void OnPointerClick(PointerEventData eventData)
        {
            if (IsUnlocked)
            {
                parentGrid?.OnPlotClickedInternal(PlotIndex);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            UpdateHighlight();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            UpdateHighlight();
        }
    }
}
