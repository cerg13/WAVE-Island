using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace WaveIsland.Data
{
    /// <summary>
    /// Database for loading and accessing spirit data from JSON
    /// </summary>
    public static class SpiritDatabase
    {
        private const string SPIRITS_PATH = "GameData/spirits_data";

        private static SpiritsJsonData cachedData;
        private static Dictionary<string, SpiritJsonData> spiritLookup;
        private static bool isInitialized = false;

        /// <summary>
        /// Initialize the database
        /// </summary>
        public static void Initialize()
        {
            if (isInitialized) return;

            LoadSpirits();
            isInitialized = true;
        }

        private static void LoadSpirits()
        {
            var textAsset = Resources.Load<TextAsset>(SPIRITS_PATH);
            if (textAsset == null)
            {
                Debug.LogError($"[SpiritDatabase] Failed to load: {SPIRITS_PATH}");
                cachedData = new SpiritsJsonData { spirits = new List<SpiritJsonData>() };
                spiritLookup = new Dictionary<string, SpiritJsonData>();
                return;
            }

            cachedData = JsonUtility.FromJson<SpiritsJsonData>(textAsset.text);
            spiritLookup = new Dictionary<string, SpiritJsonData>();

            foreach (var spirit in cachedData.spirits)
            {
                spiritLookup[spirit.id] = spirit;
            }

            Debug.Log($"[SpiritDatabase] Loaded {cachedData.spirits.Count} spirits");
        }

        /// <summary>
        /// Get all spirits
        /// </summary>
        public static List<SpiritJsonData> GetAllSpirits()
        {
            Initialize();
            return cachedData.spirits;
        }

        /// <summary>
        /// Get spirit by ID
        /// </summary>
        public static SpiritJsonData GetSpirit(string id)
        {
            Initialize();
            return spiritLookup.TryGetValue(id, out var spirit) ? spirit : null;
        }

        /// <summary>
        /// Get spirits by rarity
        /// </summary>
        public static List<SpiritJsonData> GetSpiritsByRarity(string rarity)
        {
            Initialize();
            return cachedData.spirits.Where(s => s.rarity == rarity).ToList();
        }

        /// <summary>
        /// Get spirits by element
        /// </summary>
        public static List<SpiritJsonData> GetSpiritsByElement(string element)
        {
            Initialize();
            return cachedData.spirits.Where(s => s.element == element).ToList();
        }

        /// <summary>
        /// Get gacha-pullable spirits (non-iiko exclusive)
        /// </summary>
        public static List<SpiritJsonData> GetGachaSpirits()
        {
            Initialize();
            return cachedData.spirits.Where(s => !s.iikoExclusive && s.gachaDropRate > 0).ToList();
        }

        /// <summary>
        /// Get iiko-exclusive spirits
        /// </summary>
        public static List<SpiritJsonData> GetIikoExclusiveSpirits()
        {
            Initialize();
            return cachedData.spirits.Where(s => s.iikoExclusive).ToList();
        }

        /// <summary>
        /// Get total spirit count
        /// </summary>
        public static int GetTotalSpiritCount()
        {
            Initialize();
            return cachedData.spirits.Count;
        }

        /// <summary>
        /// Clear cache for reloading
        /// </summary>
        public static void ClearCache()
        {
            cachedData = null;
            spiritLookup = null;
            isInitialized = false;
        }

        /// <summary>
        /// Convert rarity string to enum
        /// </summary>
        public static SpiritRarity ParseRarity(string rarity)
        {
            switch (rarity.ToLower())
            {
                case "common": return SpiritRarity.Common;
                case "uncommon": return SpiritRarity.Uncommon;
                case "rare": return SpiritRarity.Rare;
                case "epic": return SpiritRarity.Epic;
                case "legendary": return SpiritRarity.Legendary;
                default: return SpiritRarity.Common;
            }
        }

        /// <summary>
        /// Convert element string to enum
        /// </summary>
        public static SpiritElement ParseElement(string element)
        {
            switch (element.ToLower())
            {
                case "fire": return SpiritElement.Fire;
                case "water": return SpiritElement.Water;
                case "earth": return SpiritElement.Earth;
                case "air": return SpiritElement.Air;
                case "nature": return SpiritElement.Nature;
                case "moon": return SpiritElement.Moon;
                case "sun": return SpiritElement.Sun;
                case "smoke": return SpiritElement.Smoke;
                default: return SpiritElement.Fire;
            }
        }

        /// <summary>
        /// Convert ability type string to enum
        /// </summary>
        public static SpiritAbilityType ParseAbilityType(string type)
        {
            switch (type)
            {
                case "GrowthSpeedBonus": return SpiritAbilityType.GrowthSpeedBonus;
                case "HarvestBonus": return SpiritAbilityType.HarvestBonus;
                case "AutoWater": return SpiritAbilityType.AutoWater;
                case "RareSeedChance": return SpiritAbilityType.RareSeedChance;
                case "CraftSpeedBonus": return SpiritAbilityType.CraftSpeedBonus;
                case "IngredientSaver": return SpiritAbilityType.IngredientSaver;
                case "QualityBonus": return SpiritAbilityType.QualityBonus;
                case "RecipeHint": return SpiritAbilityType.RecipeHint;
                case "ShopDiscount": return SpiritAbilityType.ShopDiscount;
                case "SellBonus": return SpiritAbilityType.SellBonus;
                case "CoinBonus": return SpiritAbilityType.CoinBonus;
                case "ExpBonus": return SpiritAbilityType.ExpBonus;
                case "DailyBonus": return SpiritAbilityType.DailyBonus;
                case "LuckBonus": return SpiritAbilityType.LuckBonus;
                case "IikoBonus": return SpiritAbilityType.IikoBonus;
                default: return SpiritAbilityType.CoinBonus;
            }
        }
    }

    // JSON Data structures for spirits

    [System.Serializable]
    public class SpiritsJsonData
    {
        public List<SpiritJsonData> spirits;
    }

    [System.Serializable]
    public class SpiritJsonData
    {
        public string id;
        public string displayName;
        public string displayNameRu;
        public string description;
        public string lore;
        public string rarity;
        public string element;
        public List<SpiritAbilityJson> abilities;
        public int maxLevel;
        public float gachaDropRate;
        public bool iikoExclusive;
        public int unlockOrderCount;
        public string shishaFlavorName;
        public string[] flavorNotes;
    }

    [System.Serializable]
    public class SpiritAbilityJson
    {
        public string name;
        public string description;
        public string type;
        public float baseValue;
        public float valuePerLevel;
    }
}
