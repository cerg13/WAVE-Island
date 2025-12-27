using UnityEngine;

namespace WaveIsland.Data
{
    /// <summary>
    /// ScriptableObject for ingredient data
    /// Used for all ingredients: grown items, purchased items, etc.
    /// </summary>
    [CreateAssetMenu(fileName = "New Ingredient", menuName = "WAVE Island/Ingredient")]
    public class IngredientData : ScriptableObject
    {
        [Header("Basic Info")]
        public string Id;
        public string DisplayName;
        public string DisplayNameRu;
        [TextArea(2, 4)]
        public string Description;

        [Header("Visual")]
        public Sprite Icon;
        public Color TintColor = Color.white;

        [Header("Category")]
        public IngredientCategory Category;
        public IngredientRarity Rarity;

        [Header("Economy")]
        public int BuyPrice;
        public int SellPrice;

        [Header("Source")]
        public bool CanBeGrown;
        public bool CanBePurchased;
        public bool IikoOnly; // Only unlocked via iiko order

        [Header("Related Items")]
        public PlantData SourcePlant; // If grown from a plant
    }

    public enum IngredientCategory
    {
        Herb,           // Mint, Basil, Rosemary
        Citrus,         // Lime, Lemon, Orange
        Tropical,       // Mango, Pineapple, Passion fruit
        Spirit,         // Rum, Tequila, Vodka
        Mixer,          // Soda, Tonic, Cola
        Sweetener,      // Sugar, Honey, Agave
        Spice,          // Chili, Ginger, Cinnamon
        Dairy,          // Milk, Cream, Yogurt
        Meat,           // Chicken, Beef, Pork
        Seafood,        // Fish, Shrimp, Octopus
        Vegetable,      // Tomato, Onion, Pepper
        Special         // Rare/Legendary items
    }

    public enum IngredientRarity
    {
        Common,     // Easy to obtain
        Uncommon,   // Moderate effort
        Rare,       // Difficult to obtain
        Epic,       // Very rare
        Legendary   // Exclusive/Limited
    }
}
