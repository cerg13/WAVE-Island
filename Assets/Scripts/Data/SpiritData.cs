using UnityEngine;
using System.Collections.Generic;

namespace WaveIsland.Data
{
    /// <summary>
    /// ScriptableObject for spirit/companion data
    /// Spirits provide passive bonuses and special abilities
    /// </summary>
    [CreateAssetMenu(fileName = "New Spirit", menuName = "WAVE Island/Spirit")]
    public class SpiritData : ScriptableObject
    {
        [Header("Basic Info")]
        public string Id;
        public string DisplayName;
        public string DisplayNameRu;
        [TextArea(2, 4)]
        public string Description;
        [TextArea(2, 4)]
        public string Lore;

        [Header("Visual")]
        public Sprite Icon;
        public Sprite FullArt;
        public RuntimeAnimatorController Animator;
        public Color AuraColor = Color.white;

        [Header("Rarity & Type")]
        public SpiritRarity Rarity;
        public SpiritElement Element;

        [Header("Stats (Level 1)")]
        public List<SpiritAbility> Abilities;

        [Header("Leveling")]
        public int MaxLevel = 10;
        public int[] ExpPerLevel; // XP needed for each level

        [Header("Obtaining")]
        public float GachaDropRate; // 0.01 = 1%
        public bool IikoExclusive; // Only from iiko orders
        public int UnlockOrderCount; // Number of orders needed

        [Header("Shisha Flavor")]
        public string ShishaFlavorName;
        public string[] FlavorNotes;
    }

    [System.Serializable]
    public class SpiritAbility
    {
        public string AbilityName;
        [TextArea]
        public string Description;
        public SpiritAbilityType Type;
        public float BaseValue; // Base effect value
        public float ValuePerLevel; // Additional per level
        public Sprite AbilityIcon;
    }

    public enum SpiritRarity
    {
        Common,     // 60% drop rate
        Uncommon,   // 25% drop rate
        Rare,       // 10% drop rate
        Epic,       // 4% drop rate
        Legendary   // 1% drop rate
    }

    public enum SpiritElement
    {
        Fire,       // Crafting speed bonus
        Water,      // Auto-watering
        Earth,      // Harvest bonus
        Air,        // Exploration luck
        Nature,     // Growth speed
        Moon,       // Night bonuses
        Sun,        // Day bonuses
        Smoke       // Shisha-related
    }

    public enum SpiritAbilityType
    {
        // Garden
        GrowthSpeedBonus,       // % faster growth
        HarvestBonus,           // +X items
        AutoWater,              // Auto water plants
        RareSeedChance,         // % more seed drops

        // Crafting
        CraftSpeedBonus,        // % faster craft
        IngredientSaver,        // % chance not to consume
        QualityBonus,           // +X coins from craft
        RecipeHint,             // Free hints

        // Economy
        ShopDiscount,           // % off shop prices
        SellBonus,              // +% sell price
        CoinBonus,              // +% coins from all sources
        ExpBonus,               // +% experience

        // Special
        DailyBonus,             // Extra daily rewards
        LuckBonus,              // +% gacha luck
        IikoBonus               // Extra iiko rewards
    }
}
