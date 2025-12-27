using UnityEngine;
using System.Collections.Generic;

namespace WaveIsland.Data
{
    /// <summary>
    /// ScriptableObject for recipe data
    /// Defines cocktails, dishes, and other craftable items
    /// </summary>
    [CreateAssetMenu(fileName = "New Recipe", menuName = "WAVE Island/Recipe")]
    public class RecipeData : ScriptableObject
    {
        [Header("Basic Info")]
        public string Id;
        public string DisplayName;
        public string DisplayNameRu;
        [TextArea(2, 4)]
        public string Description;

        [Header("Visual")]
        public Sprite Icon;
        public Sprite CraftingAnimation; // Optional animation sprite

        [Header("Category")]
        public RecipeCategory Category;
        public RecipeDifficulty Difficulty;
        public IngredientRarity Rarity;

        [Header("Ingredients")]
        public List<RecipeIngredient> Ingredients;

        [Header("Result")]
        public IngredientData ResultItem; // If recipe produces an item
        public int ResultQuantity = 1;

        [Header("Crafting")]
        [Tooltip("Craft time in seconds")]
        public float CraftTimeSeconds = 5f;
        public int RequiredSlots = 3;

        [Header("Rewards")]
        public int CoinsReward;
        public int ExperienceReward;

        [Header("Requirements")]
        public int UnlockLevel = 1;
        public bool IikoUnlockOnly; // Only unlocked by ordering in WAVE

        [Header("iiko Integration")]
        public string IikoMenuItemId; // Mapping to real menu item

        [Header("Hints")]
        [TextArea]
        public string HintLevel1; // "Contains citrus"
        [TextArea]
        public string HintLevel2; // "Use lime and mint"
        [TextArea]
        public string HintLevel3; // "Add white rum to lime and mint"

        /// <summary>
        /// Check if player has all ingredients
        /// </summary>
        public bool CanCraft(Core.PlayerData playerData)
        {
            foreach (var ing in Ingredients)
            {
                if (!playerData.HasItem(ing.Ingredient.Id, ing.Quantity))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Consume ingredients from player inventory
        /// </summary>
        public bool ConsumeIngredients(Core.PlayerData playerData)
        {
            if (!CanCraft(playerData))
                return false;

            foreach (var ing in Ingredients)
            {
                playerData.RemoveItem(ing.Ingredient.Id, ing.Quantity);
            }
            return true;
        }
    }

    [System.Serializable]
    public class RecipeIngredient
    {
        public IngredientData Ingredient;
        public int Quantity = 1;
    }

    public enum RecipeCategory
    {
        // Cocktails
        ClassicCocktail,    // 25 classic cocktails
        TropicalCocktail,   // 20 tropical/tulum style
        SignatureCocktail,  // 15 WAVE signature
        Mocktail,           // 10 non-alcoholic

        // Food
        Appetizer,          // 15 starters
        MainCourse,         // 25 main dishes
        Dessert,            // 8 desserts

        // Special
        Secret,             // Hidden recipes
        Seasonal,           // Limited time
        Legendary           // Ultra rare
    }

    public enum RecipeDifficulty
    {
        Easy,       // 2 slots, common ingredients
        Medium,     // 3 slots, some rare ingredients
        Hard,       // 4 slots, rare ingredients
        Legendary   // 5 slots, legendary ingredients
    }
}
