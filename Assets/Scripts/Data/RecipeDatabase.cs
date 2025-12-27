using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace WaveIsland.Data
{
    /// <summary>
    /// Database for accessing recipe data
    /// </summary>
    [CreateAssetMenu(fileName = "RecipeDatabase", menuName = "WAVE Island/Database/Recipes")]
    public class RecipeDatabase : ScriptableObject
    {
        private static RecipeDatabase _instance;
        public static RecipeDatabase Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<RecipeDatabase>("RecipeDatabase");
                }
                return _instance;
            }
        }

        [Header("All Recipes")]
        [SerializeField] private List<RecipeData> allRecipes = new List<RecipeData>();

        /// <summary>
        /// Get recipe by ID
        /// </summary>
        public static RecipeData GetRecipe(string recipeId)
        {
            return Instance?.allRecipes.Find(r => r.Id == recipeId);
        }

        /// <summary>
        /// Get all recipes
        /// </summary>
        public static List<RecipeData> GetAllRecipes()
        {
            return Instance?.allRecipes ?? new List<RecipeData>();
        }

        /// <summary>
        /// Get recipes by category
        /// </summary>
        public static List<RecipeData> GetRecipesByCategory(RecipeCategory category)
        {
            return Instance?.allRecipes.Where(r => r.Category == category).ToList()
                ?? new List<RecipeData>();
        }

        /// <summary>
        /// Find recipe by ingredients (exact match)
        /// </summary>
        public static RecipeData FindRecipeByIngredients(List<string> ingredientIds)
        {
            if (Instance == null) return null;

            var sortedInput = ingredientIds.OrderBy(id => id).ToList();

            foreach (var recipe in Instance.allRecipes)
            {
                var recipeIngredients = recipe.Ingredients
                    .Select(ri => ri.Ingredient.Id)
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
        /// Get recipe by iiko menu item ID
        /// </summary>
        public static RecipeData GetRecipeByIikoId(string iikoMenuItemId)
        {
            return Instance?.allRecipes.Find(r => r.IikoMenuItemId == iikoMenuItemId);
        }

        /// <summary>
        /// Get total recipe count
        /// </summary>
        public static int GetTotalRecipeCount()
        {
            return Instance?.allRecipes.Count ?? 0;
        }
    }
}
