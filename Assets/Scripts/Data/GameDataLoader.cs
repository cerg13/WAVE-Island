using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace WaveIsland.Data
{
    /// <summary>
    /// Loads game data from JSON files in Resources
    /// Used for initial data population
    /// </summary>
    public static class GameDataLoader
    {
        private const string RECIPES_PATH = "GameData/recipes_data";
        private const string INGREDIENTS_PATH = "GameData/ingredients_data";

        private static RecipesJsonData cachedRecipes;
        private static IngredientsJsonData cachedIngredients;

        /// <summary>
        /// Load all recipes from JSON
        /// </summary>
        public static List<RecipeJsonData> LoadRecipes()
        {
            if (cachedRecipes != null)
                return cachedRecipes.recipes;

            var textAsset = Resources.Load<TextAsset>(RECIPES_PATH);
            if (textAsset == null)
            {
                Debug.LogError($"[GameDataLoader] Failed to load: {RECIPES_PATH}");
                return new List<RecipeJsonData>();
            }

            cachedRecipes = JsonUtility.FromJson<RecipesJsonData>(textAsset.text);
            Debug.Log($"[GameDataLoader] Loaded {cachedRecipes.recipes.Count} recipes");

            return cachedRecipes.recipes;
        }

        /// <summary>
        /// Load all ingredients from JSON
        /// </summary>
        public static List<IngredientJsonData> LoadIngredients()
        {
            if (cachedIngredients != null)
                return cachedIngredients.ingredients;

            var textAsset = Resources.Load<TextAsset>(INGREDIENTS_PATH);
            if (textAsset == null)
            {
                Debug.LogError($"[GameDataLoader] Failed to load: {INGREDIENTS_PATH}");
                return new List<IngredientJsonData>();
            }

            cachedIngredients = JsonUtility.FromJson<IngredientsJsonData>(textAsset.text);
            Debug.Log($"[GameDataLoader] Loaded {cachedIngredients.ingredients.Count} ingredients");

            return cachedIngredients.ingredients;
        }

        /// <summary>
        /// Get recipe by ID
        /// </summary>
        public static RecipeJsonData GetRecipe(string id)
        {
            var recipes = LoadRecipes();
            return recipes.Find(r => r.id == id);
        }

        /// <summary>
        /// Get ingredient by ID
        /// </summary>
        public static IngredientJsonData GetIngredient(string id)
        {
            var ingredients = LoadIngredients();
            return ingredients.Find(i => i.id == id);
        }

        /// <summary>
        /// Find recipe by exact ingredient match
        /// </summary>
        public static RecipeJsonData FindRecipeByIngredients(List<string> ingredientIds)
        {
            var recipes = LoadRecipes();
            var sortedInput = ingredientIds.OrderBy(id => id).ToList();

            foreach (var recipe in recipes)
            {
                var recipeIngredients = recipe.ingredients
                    .Select(i => i.id)
                    .OrderBy(id => id)
                    .ToList();

                if (sortedInput.SequenceEqual(recipeIngredients))
                {
                    return recipe;
                }
            }

            return null;
        }

        /// <summary>
        /// Get recipes by category
        /// </summary>
        public static List<RecipeJsonData> GetRecipesByCategory(string category)
        {
            var recipes = LoadRecipes();
            return recipes.Where(r => r.category == category).ToList();
        }

        /// <summary>
        /// Clear cache (for reloading)
        /// </summary>
        public static void ClearCache()
        {
            cachedRecipes = null;
            cachedIngredients = null;
        }
    }

    // JSON Data structures

    [System.Serializable]
    public class RecipesJsonData
    {
        public List<RecipeJsonData> recipes;
    }

    [System.Serializable]
    public class RecipeJsonData
    {
        public string id;
        public string displayName;
        public string displayNameRu;
        public string description;
        public string category;
        public string difficulty;
        public string rarity;
        public List<RecipeIngredientJson> ingredients;
        public float craftTimeSeconds;
        public int coinsReward;
        public int expReward;
        public int unlockLevel;
        public string hintLevel1;
        public string hintLevel2;
        public string hintLevel3;
        public string iikoMenuItemId;
    }

    [System.Serializable]
    public class RecipeIngredientJson
    {
        public string id;
        public int quantity;
    }

    [System.Serializable]
    public class IngredientsJsonData
    {
        public List<IngredientJsonData> ingredients;
    }

    [System.Serializable]
    public class IngredientJsonData
    {
        public string id;
        public string displayName;
        public string displayNameRu;
        public string category;
        public string rarity;
        public int buyPrice;
        public int sellPrice;
        public bool canBeGrown;
        public bool canBePurchased;
        public string sourcePlantId;
    }
}
