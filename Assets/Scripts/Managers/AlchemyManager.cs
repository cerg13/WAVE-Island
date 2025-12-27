using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using WaveIsland.Core;
using WaveIsland.Data;

namespace WaveIsland.Alchemy
{
    /// <summary>
    /// Manages crafting system (Alchemy)
    /// Handles recipe matching, crafting, and discovery
    /// </summary>
    public class AlchemyManager : MonoBehaviour
    {
        public static AlchemyManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private int baseCraftSlots = 3;
        [SerializeField] private float craftAnimationTime = 2f;

        [Header("Recipes Database")]
        [SerializeField] private List<RecipeData> allRecipes;

        [Header("Current Crafting")]
        private List<IngredientData> currentSlots = new List<IngredientData>();
        private bool isCrafting = false;
        private RecipeData currentCraft = null;
        private float craftTimer = 0f;

        public int MaxSlots => baseCraftSlots;
        public bool IsCrafting => isCrafting;
        public List<RecipeData> AllRecipes => allRecipes;

        public event System.Action<RecipeData> OnRecipeDiscovered;
        public event System.Action<RecipeData, bool> OnCraftComplete;
        public event System.Action OnSlotsChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (isCrafting)
            {
                craftTimer -= Time.deltaTime;
                if (craftTimer <= 0f)
                {
                    CompleteCraft();
                }
            }
        }

        /// <summary>
        /// Add ingredient to crafting slot
        /// </summary>
        public bool AddIngredient(IngredientData ingredient)
        {
            if (isCrafting)
            {
                Debug.LogWarning("[AlchemyManager] Cannot add while crafting");
                return false;
            }

            if (currentSlots.Count >= baseCraftSlots)
            {
                Debug.LogWarning("[AlchemyManager] All slots full");
                return false;
            }

            var playerData = GameManager.Instance.PlayerData;
            if (!playerData.HasItem(ingredient.Id))
            {
                Debug.LogWarning($"[AlchemyManager] Player doesn't have {ingredient.DisplayName}");
                return false;
            }

            currentSlots.Add(ingredient);
            OnSlotsChanged?.Invoke();

            Debug.Log($"[AlchemyManager] Added {ingredient.DisplayName} to slot");
            return true;
        }

        /// <summary>
        /// Remove ingredient from slot
        /// </summary>
        public bool RemoveIngredient(int slotIndex)
        {
            if (isCrafting) return false;
            if (slotIndex < 0 || slotIndex >= currentSlots.Count) return false;

            currentSlots.RemoveAt(slotIndex);
            OnSlotsChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Clear all crafting slots
        /// </summary>
        public void ClearSlots()
        {
            if (isCrafting) return;

            currentSlots.Clear();
            OnSlotsChanged?.Invoke();
        }

        /// <summary>
        /// Start crafting with current ingredients
        /// </summary>
        public bool StartCraft()
        {
            if (isCrafting) return false;
            if (currentSlots.Count < 2)
            {
                Debug.LogWarning("[AlchemyManager] Need at least 2 ingredients");
                return false;
            }

            var playerData = GameManager.Instance.PlayerData;

            // Check if player has all ingredients
            foreach (var ingredient in currentSlots)
            {
                if (!playerData.HasItem(ingredient.Id))
                {
                    Debug.LogWarning($"[AlchemyManager] Missing {ingredient.DisplayName}");
                    return false;
                }
            }

            // Find matching recipe
            currentCraft = FindMatchingRecipe();

            // Consume ingredients
            foreach (var ingredient in currentSlots)
            {
                playerData.RemoveItem(ingredient.Id);
            }

            isCrafting = true;
            craftTimer = currentCraft != null ? currentCraft.CraftTimeSeconds : craftAnimationTime;

            Debug.Log($"[AlchemyManager] Started crafting...");
            return true;
        }

        private void CompleteCraft()
        {
            isCrafting = false;
            var playerData = GameManager.Instance.PlayerData;

            if (currentCraft != null)
            {
                // Successful craft!
                bool isNewDiscovery = !playerData.UnlockedRecipeIds.Contains(currentCraft.Id);

                if (isNewDiscovery)
                {
                    playerData.UnlockedRecipeIds.Add(currentCraft.Id);
                    playerData.TotalRecipesDiscovered++;
                    OnRecipeDiscovered?.Invoke(currentCraft);

                    Debug.Log($"[AlchemyManager] NEW RECIPE: {currentCraft.DisplayName}!");
                }

                // Give rewards
                playerData.Coins += currentCraft.CoinsReward;
                GameManager.Instance.AddExperience(currentCraft.ExperienceReward);
                playerData.TotalCrafts++;

                // Add result item if any
                if (currentCraft.ResultItem != null)
                {
                    playerData.AddItem(currentCraft.ResultItem.Id, currentCraft.ResultQuantity);
                }

                OnCraftComplete?.Invoke(currentCraft, isNewDiscovery);
                Debug.Log($"[AlchemyManager] Crafted: {currentCraft.DisplayName}");
            }
            else
            {
                // Failed craft - no matching recipe
                OnCraftComplete?.Invoke(null, false);
                Debug.Log("[AlchemyManager] Craft failed - no matching recipe");
            }

            currentCraft = null;
            currentSlots.Clear();
            OnSlotsChanged?.Invoke();
        }

        /// <summary>
        /// Find recipe matching current ingredients
        /// </summary>
        private RecipeData FindMatchingRecipe()
        {
            var slotIds = currentSlots.Select(i => i.Id).OrderBy(id => id).ToList();

            foreach (var recipe in allRecipes)
            {
                var recipeIds = recipe.Ingredients
                    .Select(ri => ri.Ingredient.Id)
                    .OrderBy(id => id)
                    .ToList();

                if (slotIds.SequenceEqual(recipeIds))
                {
                    return recipe;
                }
            }

            return null;
        }

        /// <summary>
        /// Check if recipe can be crafted (player has ingredients)
        /// </summary>
        public bool CanCraftRecipe(RecipeData recipe)
        {
            var playerData = GameManager.Instance.PlayerData;
            return recipe.CanCraft(playerData);
        }

        /// <summary>
        /// Get all discovered recipes
        /// </summary>
        public List<RecipeData> GetDiscoveredRecipes()
        {
            var playerData = GameManager.Instance.PlayerData;
            return allRecipes.Where(r => playerData.UnlockedRecipeIds.Contains(r.Id)).ToList();
        }

        /// <summary>
        /// Get hint for undiscovered recipe
        /// </summary>
        public string GetHint(RecipeData recipe, int level)
        {
            switch (level)
            {
                case 1: return recipe.HintLevel1;
                case 2: return recipe.HintLevel2;
                case 3: return recipe.HintLevel3;
                default: return "???";
            }
        }

        /// <summary>
        /// Instantly unlock recipe (from iiko order)
        /// </summary>
        public void UnlockRecipeFromOrder(string iikoMenuItemId)
        {
            var recipe = allRecipes.Find(r => r.IikoMenuItemId == iikoMenuItemId);

            if (recipe != null)
            {
                var playerData = GameManager.Instance.PlayerData;

                if (!playerData.UnlockedRecipeIds.Contains(recipe.Id))
                {
                    playerData.UnlockedRecipeIds.Add(recipe.Id);
                    playerData.TotalRecipesDiscovered++;
                    OnRecipeDiscovered?.Invoke(recipe);

                    Debug.Log($"[AlchemyManager] iiko unlock: {recipe.DisplayName}");
                }
            }
        }
    }
}
