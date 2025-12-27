using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using WaveIsland.Data;

namespace WaveIsland.Alchemy
{
    /// <summary>
    /// Handles recipe matching logic
    /// Finds recipes based on ingredient combinations
    /// </summary>
    public static class RecipeMatcher
    {
        /// <summary>
        /// Find exact recipe match for given ingredients
        /// </summary>
        public static RecipeData FindExactMatch(List<IngredientData> ingredients)
        {
            if (ingredients == null || ingredients.Count < 2)
                return null;

            var ingredientIds = ingredients
                .Where(i => i != null)
                .Select(i => i.Id)
                .OrderBy(id => id)
                .ToList();

            return RecipeDatabase.FindRecipeByIngredients(ingredientIds);
        }

        /// <summary>
        /// Find exact recipe match by ingredient IDs
        /// </summary>
        public static RecipeData FindExactMatch(List<string> ingredientIds)
        {
            if (ingredientIds == null || ingredientIds.Count < 2)
                return null;

            var sortedIds = ingredientIds.OrderBy(id => id).ToList();
            return RecipeDatabase.FindRecipeByIngredients(sortedIds);
        }

        /// <summary>
        /// Find partial matches (recipes that could be completed with more ingredients)
        /// </summary>
        public static List<RecipeMatch> FindPartialMatches(List<string> ingredientIds, int maxResults = 5)
        {
            var matches = new List<RecipeMatch>();

            if (ingredientIds == null || ingredientIds.Count == 0)
                return matches;

            var sortedInput = ingredientIds.OrderBy(id => id).ToList();
            var allRecipes = RecipeDatabase.GetAllRecipes();

            foreach (var recipe in allRecipes)
            {
                var recipeIngredients = recipe.Ingredients
                    .Select(ri => ri.Ingredient.Id)
                    .OrderBy(id => id)
                    .ToList();

                // Check how many of our ingredients are in this recipe
                int matchCount = 0;
                foreach (var inputId in sortedInput)
                {
                    if (recipeIngredients.Contains(inputId))
                    {
                        matchCount++;
                    }
                }

                if (matchCount > 0)
                {
                    float matchRatio = (float)matchCount / recipeIngredients.Count;

                    // Only include if all our ingredients are in the recipe
                    bool allMatch = sortedInput.All(id => recipeIngredients.Contains(id));

                    if (allMatch)
                    {
                        matches.Add(new RecipeMatch
                        {
                            Recipe = recipe,
                            MatchedCount = matchCount,
                            TotalRequired = recipeIngredients.Count,
                            MatchRatio = matchRatio,
                            MissingIngredients = recipeIngredients.Except(sortedInput).ToList()
                        });
                    }
                }
            }

            // Sort by match ratio (closest to complete first)
            return matches
                .OrderByDescending(m => m.MatchRatio)
                .ThenBy(m => m.TotalRequired)
                .Take(maxResults)
                .ToList();
        }

        /// <summary>
        /// Find recipes that use a specific ingredient
        /// </summary>
        public static List<RecipeData> FindRecipesWithIngredient(string ingredientId)
        {
            var allRecipes = RecipeDatabase.GetAllRecipes();

            return allRecipes
                .Where(r => r.Ingredients.Any(ri => ri.Ingredient.Id == ingredientId))
                .ToList();
        }

        /// <summary>
        /// Find recipes player can craft right now
        /// </summary>
        public static List<RecipeData> FindCraftableRecipes(Core.PlayerData playerData)
        {
            var allRecipes = RecipeDatabase.GetAllRecipes();
            var craftable = new List<RecipeData>();

            foreach (var recipe in allRecipes)
            {
                bool canCraft = true;

                foreach (var ri in recipe.Ingredients)
                {
                    if (!playerData.HasItem(ri.Ingredient.Id, ri.Quantity))
                    {
                        canCraft = false;
                        break;
                    }
                }

                if (canCraft)
                {
                    craftable.Add(recipe);
                }
            }

            return craftable;
        }

        /// <summary>
        /// Get hint for undiscovered recipe
        /// </summary>
        public static string GetRecipeHint(RecipeData recipe, int hintLevel, Core.PlayerData playerData)
        {
            if (playerData.UnlockedRecipeIds.Contains(recipe.Id))
            {
                // Already discovered - show full recipe
                return string.Join(" + ", recipe.Ingredients.Select(ri => ri.Ingredient.DisplayName));
            }

            switch (hintLevel)
            {
                case 1:
                    return recipe.HintLevel1 ?? GetAutoHint1(recipe);
                case 2:
                    return recipe.HintLevel2 ?? GetAutoHint2(recipe);
                case 3:
                    return recipe.HintLevel3 ?? GetAutoHint3(recipe);
                default:
                    return "???";
            }
        }

        private static string GetAutoHint1(RecipeData recipe)
        {
            // Category-based hint
            var categories = recipe.Ingredients
                .Select(ri => ri.Ingredient.Category)
                .Distinct()
                .ToList();

            if (categories.Count == 1)
            {
                return $"Uses {categories[0]} ingredients";
            }

            return $"Combines {categories[0]} and {categories[1]}";
        }

        private static string GetAutoHint2(RecipeData recipe)
        {
            // First two ingredients
            if (recipe.Ingredients.Count >= 2)
            {
                return $"Contains {recipe.Ingredients[0].Ingredient.DisplayName} and {recipe.Ingredients[1].Ingredient.DisplayName}";
            }
            return "A special combination...";
        }

        private static string GetAutoHint3(RecipeData recipe)
        {
            // All but one ingredient
            var names = recipe.Ingredients.Take(recipe.Ingredients.Count - 1)
                .Select(ri => ri.Ingredient.DisplayName);

            return string.Join(", ", names) + " + ???";
        }

        /// <summary>
        /// Calculate similarity between two sets of ingredients (for suggestions)
        /// </summary>
        public static float CalculateSimilarity(List<string> set1, List<string> set2)
        {
            if (set1 == null || set2 == null || set1.Count == 0 || set2.Count == 0)
                return 0f;

            int intersection = set1.Intersect(set2).Count();
            int union = set1.Union(set2).Count();

            return (float)intersection / union; // Jaccard similarity
        }
    }

    /// <summary>
    /// Result of a partial recipe match
    /// </summary>
    public class RecipeMatch
    {
        public RecipeData Recipe;
        public int MatchedCount;
        public int TotalRequired;
        public float MatchRatio;
        public List<string> MissingIngredients;
    }
}
