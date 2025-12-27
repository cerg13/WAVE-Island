using UnityEngine;

namespace WaveIsland.Data
{
    /// <summary>
    /// ScriptableObject for plant/crop data
    /// Defines growth mechanics for garden plants
    /// </summary>
    [CreateAssetMenu(fileName = "New Plant", menuName = "WAVE Island/Plant")]
    public class PlantData : ScriptableObject
    {
        [Header("Basic Info")]
        public string Id;
        public string DisplayName;
        public string DisplayNameRu;
        [TextArea(2, 4)]
        public string Description;

        [Header("Visual")]
        public Sprite SeedIcon;
        public Sprite[] GrowthStageSprites; // Stages: Planted, Growing, Flowering, Ready

        [Header("Growth")]
        [Tooltip("Growth time in minutes")]
        public float GrowthTimeMinutes = 30f;
        [Tooltip("Time until withering after ready (in hours)")]
        public float WitherTimeHours = 24f;

        [Header("Harvest")]
        public IngredientData HarvestIngredient;
        public int MinHarvest = 1;
        public int MaxHarvest = 3;
        [Range(0f, 1f)]
        public float SeedDropChance = 0.2f;

        [Header("Requirements")]
        public int UnlockLevel = 1;
        public PlantCategory Category;
        public IngredientRarity Rarity;

        [Header("Economy")]
        public int SeedBuyPrice;
        public int SeedSellPrice;

        [Header("Bonuses")]
        [Tooltip("Bonus harvest if watered")]
        public int WateredBonusHarvest = 1;
        [Tooltip("Growth speed multiplier if watered")]
        public float WateredSpeedMultiplier = 1.1f;

        /// <summary>
        /// Get growth time in seconds
        /// </summary>
        public float GrowthTimeSeconds => GrowthTimeMinutes * 60f;

        /// <summary>
        /// Get wither time in seconds
        /// </summary>
        public float WitherTimeSeconds => WitherTimeHours * 3600f;

        /// <summary>
        /// Calculate random harvest amount
        /// </summary>
        public int GetHarvestAmount(bool wasWatered = false)
        {
            int amount = Random.Range(MinHarvest, MaxHarvest + 1);
            if (wasWatered) amount += WateredBonusHarvest;
            return amount;
        }

        /// <summary>
        /// Check if seed drops on harvest
        /// </summary>
        public bool RollSeedDrop()
        {
            return Random.value <= SeedDropChance;
        }
    }

    public enum PlantCategory
    {
        Herbs,      // 15-30 min
        Citrus,     // 1-2 hours
        Tropical,   // 2-4 hours
        Rare,       // 8-12 hours
        Legendary   // 24-48 hours
    }
}
