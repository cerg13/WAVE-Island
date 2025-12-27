using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace WaveIsland.Data
{
    /// <summary>
    /// Database for accessing plant data
    /// Loads from ScriptableObjects or Resources
    /// </summary>
    [CreateAssetMenu(fileName = "PlantDatabase", menuName = "WAVE Island/Database/Plants")]
    public class PlantDatabase : ScriptableObject
    {
        private static PlantDatabase _instance;
        public static PlantDatabase Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<PlantDatabase>("PlantDatabase");

                    if (_instance == null)
                    {
                        Debug.LogError("[PlantDatabase] Failed to load from Resources!");
                    }
                }
                return _instance;
            }
        }

        [Header("All Plants")]
        [SerializeField] private List<PlantData> allPlants = new List<PlantData>();

        /// <summary>
        /// Get plant by ID
        /// </summary>
        public static PlantData GetPlant(string plantId)
        {
            return Instance?.allPlants.Find(p => p.Id == plantId);
        }

        /// <summary>
        /// Get all plants
        /// </summary>
        public static List<PlantData> GetAllPlants()
        {
            return Instance?.allPlants ?? new List<PlantData>();
        }

        /// <summary>
        /// Get plants by category
        /// </summary>
        public static List<PlantData> GetPlantsByCategory(PlantCategory category)
        {
            return Instance?.allPlants.Where(p => p.Category == category).ToList()
                ?? new List<PlantData>();
        }

        /// <summary>
        /// Get plants available at a certain level
        /// </summary>
        public static List<PlantData> GetPlantsForLevel(int level)
        {
            return Instance?.allPlants.Where(p => p.UnlockLevel <= level).ToList()
                ?? new List<PlantData>();
        }

        /// <summary>
        /// Get plants by rarity
        /// </summary>
        public static List<PlantData> GetPlantsByRarity(IngredientRarity rarity)
        {
            return Instance?.allPlants.Where(p => p.Rarity == rarity).ToList()
                ?? new List<PlantData>();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor helper to auto-populate from folder
        /// </summary>
        [ContextMenu("Load Plants from Folder")]
        private void LoadPlantsFromFolder()
        {
            allPlants.Clear();

            var plants = Resources.LoadAll<PlantData>("Plants");
            allPlants.AddRange(plants);

            Debug.Log($"[PlantDatabase] Loaded {allPlants.Count} plants");
        }
#endif
    }
}
