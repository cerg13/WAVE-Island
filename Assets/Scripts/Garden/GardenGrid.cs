using UnityEngine;
using System.Collections.Generic;
using WaveIsland.Core;

namespace WaveIsland.Garden
{
    /// <summary>
    /// Manages the visual grid layout of garden plots
    /// Handles plot placement, selection, and visual updates
    /// </summary>
    public class GardenGrid : MonoBehaviour
    {
        [Header("Grid Configuration")]
        [SerializeField] private int columns = 4;
        [SerializeField] private float cellSize = 2f;
        [SerializeField] private float cellSpacing = 0.2f;
        [SerializeField] private Vector3 gridOrigin = Vector3.zero;

        [Header("Prefabs")]
        [SerializeField] private GameObject plotPrefab;
        [SerializeField] private GameObject lockedPlotPrefab;

        [Header("Visual Settings")]
        [SerializeField] private Material normalMaterial;
        [SerializeField] private Material selectedMaterial;
        [SerializeField] private Material readyMaterial;
        [SerializeField] private Material witheringMaterial;

        private List<PlotVisual> plotVisuals = new List<PlotVisual>();
        private PlotVisual selectedPlot = null;

        public event System.Action<int> OnPlotSelected;
        public event System.Action<int> OnPlotClicked;

        private void Start()
        {
            InitializeGrid();
        }

        /// <summary>
        /// Initialize the visual grid based on player's garden data
        /// </summary>
        public void InitializeGrid()
        {
            ClearGrid();

            var playerData = GameManager.Instance.PlayerData;
            int maxPlots = GetMaxPlotsForLevel(playerData.GardenLevel);

            for (int i = 0; i < maxPlots; i++)
            {
                bool isUnlocked = i < playerData.GardenPlots.Count &&
                                  playerData.GardenPlots[i].IsUnlocked;

                CreatePlotVisual(i, isUnlocked);
            }

            // Show next locked plots as preview
            int previewCount = Mathf.Min(2, 20 - maxPlots);
            for (int i = 0; i < previewCount; i++)
            {
                CreateLockedPlotPreview(maxPlots + i);
            }
        }

        private void CreatePlotVisual(int index, bool isUnlocked)
        {
            Vector3 position = GetPlotPosition(index);

            GameObject prefab = isUnlocked ? plotPrefab : lockedPlotPrefab;
            if (prefab == null)
            {
                // Create simple placeholder if no prefab
                prefab = GameObject.CreatePrimitive(PrimitiveType.Quad);
                prefab.transform.localScale = new Vector3(cellSize, cellSize, 1f);
            }

            GameObject plotObj = Instantiate(prefab, position, Quaternion.Euler(90, 0, 0), transform);
            plotObj.name = $"Plot_{index}";

            var plotVisual = plotObj.GetComponent<PlotVisual>();
            if (plotVisual == null)
            {
                plotVisual = plotObj.AddComponent<PlotVisual>();
            }

            plotVisual.Initialize(index, isUnlocked, this);
            plotVisuals.Add(plotVisual);

            // Set initial state from saved data
            if (isUnlocked && index < GameManager.Instance.PlayerData.GardenPlots.Count)
            {
                var plotData = GameManager.Instance.PlayerData.GardenPlots[index];
                plotVisual.UpdateState(plotData.State);
            }
        }

        private void CreateLockedPlotPreview(int index)
        {
            Vector3 position = GetPlotPosition(index);

            GameObject preview = lockedPlotPrefab != null
                ? Instantiate(lockedPlotPrefab, position, Quaternion.Euler(90, 0, 0), transform)
                : GameObject.CreatePrimitive(PrimitiveType.Quad);

            preview.name = $"LockedPlot_{index}";
            preview.transform.position = position;
            preview.transform.localScale = new Vector3(cellSize * 0.8f, cellSize * 0.8f, 1f);

            // Make it semi-transparent
            var renderer = preview.GetComponent<Renderer>();
            if (renderer != null)
            {
                var color = renderer.material.color;
                color.a = 0.3f;
                renderer.material.color = color;
            }
        }

        /// <summary>
        /// Calculate world position for a plot index
        /// </summary>
        public Vector3 GetPlotPosition(int index)
        {
            int row = index / columns;
            int col = index % columns;

            float x = gridOrigin.x + col * (cellSize + cellSpacing);
            float z = gridOrigin.z - row * (cellSize + cellSpacing);

            return new Vector3(x, gridOrigin.y, z);
        }

        /// <summary>
        /// Get plot index from world position
        /// </summary>
        public int GetPlotIndexAtPosition(Vector3 worldPosition)
        {
            float relX = worldPosition.x - gridOrigin.x;
            float relZ = gridOrigin.z - worldPosition.z;

            int col = Mathf.FloorToInt(relX / (cellSize + cellSpacing));
            int row = Mathf.FloorToInt(relZ / (cellSize + cellSpacing));

            if (col < 0 || col >= columns || row < 0)
                return -1;

            return row * columns + col;
        }

        /// <summary>
        /// Select a plot visually
        /// </summary>
        public void SelectPlot(int index)
        {
            // Deselect previous
            if (selectedPlot != null)
            {
                selectedPlot.SetSelected(false);
            }

            // Select new
            if (index >= 0 && index < plotVisuals.Count)
            {
                selectedPlot = plotVisuals[index];
                selectedPlot.SetSelected(true);
                OnPlotSelected?.Invoke(index);
            }
            else
            {
                selectedPlot = null;
            }
        }

        /// <summary>
        /// Called by PlotVisual when clicked
        /// </summary>
        public void OnPlotClickedInternal(int index)
        {
            SelectPlot(index);
            OnPlotClicked?.Invoke(index);
        }

        /// <summary>
        /// Update visual state of a specific plot
        /// </summary>
        public void UpdatePlotVisual(int index, PlotState state, string plantId = null)
        {
            if (index >= 0 && index < plotVisuals.Count)
            {
                plotVisuals[index].UpdateState(state, plantId);
            }
        }

        /// <summary>
        /// Update all plot visuals from current data
        /// </summary>
        public void RefreshAllPlots()
        {
            var plots = GameManager.Instance.PlayerData.GardenPlots;

            for (int i = 0; i < plotVisuals.Count && i < plots.Count; i++)
            {
                plotVisuals[i].UpdateState(plots[i].State, plots[i].PlantId);
            }
        }

        private void ClearGrid()
        {
            foreach (var plot in plotVisuals)
            {
                if (plot != null)
                    Destroy(plot.gameObject);
            }
            plotVisuals.Clear();

            // Also clear locked previews
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }

        private int GetMaxPlotsForLevel(int level)
        {
            return Utils.Constants.BASE_GARDEN_PLOTS +
                   (level - 1) * Utils.Constants.PLOTS_PER_GARDEN_LEVEL;
        }

        /// <summary>
        /// Highlight plots that can accept a specific seed
        /// </summary>
        public void HighlightEmptyPlots(bool highlight)
        {
            foreach (var plot in plotVisuals)
            {
                if (plot.IsUnlocked && plot.CurrentState == PlotState.Empty)
                {
                    plot.SetHighlighted(highlight);
                }
            }
        }

        /// <summary>
        /// Highlight plots ready for harvest
        /// </summary>
        public void HighlightReadyPlots(bool highlight)
        {
            foreach (var plot in plotVisuals)
            {
                if (plot.CurrentState == PlotState.ReadyToHarvest)
                {
                    plot.SetHighlighted(highlight);
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            int maxPlots = 20;
            for (int i = 0; i < maxPlots; i++)
            {
                Vector3 pos = GetPlotPosition(i);
                Gizmos.DrawWireCube(pos, new Vector3(cellSize, 0.1f, cellSize));
            }
        }
#endif
    }
}
