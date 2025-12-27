using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using WaveIsland.Core;
using WaveIsland.Data;

namespace WaveIsland.Alchemy
{
    /// <summary>
    /// Main crafting table component
    /// Manages crafting slots, recipe matching, and crafting process
    /// </summary>
    public class CraftingTable : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private int maxSlots = 5;
        [SerializeField] private int unlockedSlots = 3;
        [SerializeField] private float craftingTime = 2f;

        [Header("Slot References")]
        [SerializeField] private List<CraftingSlot> slots;
        [SerializeField] private CraftingSlot resultSlot;

        [Header("Visual")]
        [SerializeField] private GameObject craftingEffectPrefab;
        [SerializeField] private ParticleSystem successParticles;
        [SerializeField] private ParticleSystem failParticles;

        [Header("Audio")]
        [SerializeField] private AudioClip addIngredientSound;
        [SerializeField] private AudioClip craftingSound;
        [SerializeField] private AudioClip successSound;
        [SerializeField] private AudioClip failSound;
        [SerializeField] private AudioClip discoverySound;

        // State
        private List<IngredientData> currentIngredients = new List<IngredientData>();
        private RecipeData matchedRecipe = null;
        private bool isCrafting = false;
        private float craftTimer = 0f;

        private AudioSource audioSource;

        // Events
        public event System.Action<IngredientData, int> OnIngredientAdded;
        public event System.Action<int> OnIngredientRemoved;
        public event System.Action OnCraftingStarted;
        public event System.Action<RecipeData, bool> OnCraftingComplete;
        public event System.Action<RecipeData> OnRecipeDiscovered;
        public event System.Action OnSlotsChanged;

        public bool IsCrafting => isCrafting;
        public int UnlockedSlots => unlockedSlots;
        public int UsedSlots => currentIngredients.Count;
        public RecipeData MatchedRecipe => matchedRecipe;
        public float CraftProgress => isCrafting ? 1f - (craftTimer / craftingTime) : 0f;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        private void Start()
        {
            InitializeSlots();
            UpdateSlotStates();
        }

        private void Update()
        {
            if (isCrafting)
            {
                craftTimer -= Time.deltaTime;
                if (craftTimer <= 0f)
                {
                    CompleteCrafting();
                }
            }
        }

        /// <summary>
        /// Initialize crafting slots
        /// </summary>
        private void InitializeSlots()
        {
            // Create slots if not assigned
            if (slots == null || slots.Count == 0)
            {
                slots = new List<CraftingSlot>();
                for (int i = 0; i < maxSlots; i++)
                {
                    var slotObj = new GameObject($"CraftingSlot_{i}");
                    slotObj.transform.SetParent(transform);
                    var slot = slotObj.AddComponent<CraftingSlot>();
                    slot.Initialize(i, this);
                    slots.Add(slot);
                }
            }

            // Subscribe to slot events
            foreach (var slot in slots)
            {
                slot.OnIngredientDropped += HandleIngredientDropped;
                slot.OnSlotClicked += HandleSlotClicked;
            }
        }

        /// <summary>
        /// Update which slots are locked/unlocked
        /// </summary>
        private void UpdateSlotStates()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].SetLocked(i >= unlockedSlots);
            }
        }

        /// <summary>
        /// Add ingredient to the next available slot
        /// </summary>
        public bool AddIngredient(IngredientData ingredient)
        {
            if (isCrafting)
            {
                Debug.LogWarning("[CraftingTable] Cannot add ingredient while crafting");
                return false;
            }

            if (currentIngredients.Count >= unlockedSlots)
            {
                Debug.LogWarning("[CraftingTable] All slots are full");
                return false;
            }

            // Check if player has this ingredient
            var playerData = GameManager.Instance?.PlayerData;
            if (playerData == null || !playerData.HasItem(ingredient.Id))
            {
                Debug.LogWarning($"[CraftingTable] Player doesn't have {ingredient.DisplayName}");
                return false;
            }

            // Add to slot
            int slotIndex = currentIngredients.Count;
            currentIngredients.Add(ingredient);
            slots[slotIndex].SetIngredient(ingredient);

            // Play sound
            PlaySound(addIngredientSound);

            // Check for recipe match
            CheckRecipeMatch();

            OnIngredientAdded?.Invoke(ingredient, slotIndex);
            OnSlotsChanged?.Invoke();

            Debug.Log($"[CraftingTable] Added {ingredient.DisplayName} to slot {slotIndex}");
            return true;
        }

        /// <summary>
        /// Add ingredient to specific slot
        /// </summary>
        public bool AddIngredientToSlot(IngredientData ingredient, int slotIndex)
        {
            if (isCrafting) return false;
            if (slotIndex < 0 || slotIndex >= unlockedSlots) return false;
            if (slotIndex < currentIngredients.Count && slots[slotIndex].HasIngredient) return false;

            var playerData = GameManager.Instance?.PlayerData;
            if (playerData == null || !playerData.HasItem(ingredient.Id)) return false;

            // Fill gaps if needed
            while (currentIngredients.Count < slotIndex)
            {
                currentIngredients.Add(null);
            }

            if (slotIndex < currentIngredients.Count)
            {
                currentIngredients[slotIndex] = ingredient;
            }
            else
            {
                currentIngredients.Add(ingredient);
            }

            slots[slotIndex].SetIngredient(ingredient);

            PlaySound(addIngredientSound);
            CheckRecipeMatch();

            OnIngredientAdded?.Invoke(ingredient, slotIndex);
            OnSlotsChanged?.Invoke();

            return true;
        }

        /// <summary>
        /// Remove ingredient from slot
        /// </summary>
        public bool RemoveIngredient(int slotIndex)
        {
            if (isCrafting) return false;
            if (slotIndex < 0 || slotIndex >= currentIngredients.Count) return false;

            currentIngredients.RemoveAt(slotIndex);
            slots[slotIndex].ClearIngredient();

            // Shift remaining ingredients
            for (int i = slotIndex; i < currentIngredients.Count; i++)
            {
                slots[i].SetIngredient(currentIngredients[i]);
                slots[i + 1].ClearIngredient();
            }

            CheckRecipeMatch();

            OnIngredientRemoved?.Invoke(slotIndex);
            OnSlotsChanged?.Invoke();

            return true;
        }

        /// <summary>
        /// Clear all slots
        /// </summary>
        public void ClearAllSlots()
        {
            if (isCrafting) return;

            currentIngredients.Clear();
            foreach (var slot in slots)
            {
                slot.ClearIngredient();
            }

            matchedRecipe = null;
            OnSlotsChanged?.Invoke();
        }

        /// <summary>
        /// Start crafting process
        /// </summary>
        public bool StartCrafting()
        {
            if (isCrafting)
            {
                Debug.LogWarning("[CraftingTable] Already crafting");
                return false;
            }

            if (currentIngredients.Count < 2)
            {
                Debug.LogWarning("[CraftingTable] Need at least 2 ingredients");
                return false;
            }

            // Verify player has all ingredients
            var playerData = GameManager.Instance?.PlayerData;
            foreach (var ingredient in currentIngredients)
            {
                if (ingredient == null) continue;
                if (!playerData.HasItem(ingredient.Id))
                {
                    Debug.LogWarning($"[CraftingTable] Missing {ingredient.DisplayName}");
                    return false;
                }
            }

            // Consume ingredients
            foreach (var ingredient in currentIngredients)
            {
                if (ingredient != null)
                {
                    playerData.RemoveItem(ingredient.Id);
                }
            }

            // Start crafting
            isCrafting = true;
            craftTimer = matchedRecipe?.CraftTimeSeconds ?? craftingTime;

            PlaySound(craftingSound);
            OnCraftingStarted?.Invoke();

            Debug.Log("[CraftingTable] Crafting started...");
            return true;
        }

        /// <summary>
        /// Complete the crafting process
        /// </summary>
        private void CompleteCrafting()
        {
            isCrafting = false;

            var playerData = GameManager.Instance?.PlayerData;
            bool isNewDiscovery = false;

            if (matchedRecipe != null)
            {
                // Success!
                isNewDiscovery = !playerData.UnlockedRecipeIds.Contains(matchedRecipe.Id);

                if (isNewDiscovery)
                {
                    playerData.UnlockedRecipeIds.Add(matchedRecipe.Id);
                    playerData.TotalRecipesDiscovered++;

                    PlaySound(discoverySound);
                    successParticles?.Play();

                    OnRecipeDiscovered?.Invoke(matchedRecipe);
                    Debug.Log($"[CraftingTable] NEW RECIPE DISCOVERED: {matchedRecipe.DisplayName}!");
                }
                else
                {
                    PlaySound(successSound);
                    successParticles?.Play();
                }

                // Give rewards
                playerData.Coins += matchedRecipe.CoinsReward;
                GameManager.Instance.AddExperience(matchedRecipe.ExperienceReward);
                playerData.TotalCrafts++;

                // Add result item if any
                if (matchedRecipe.ResultItem != null)
                {
                    playerData.AddItem(matchedRecipe.ResultItem.Id, matchedRecipe.ResultQuantity);
                }

                // Show result
                resultSlot?.SetRecipeResult(matchedRecipe);

                Debug.Log($"[CraftingTable] Crafted: {matchedRecipe.DisplayName}");
            }
            else
            {
                // Failed - no matching recipe
                PlaySound(failSound);
                failParticles?.Play();

                // Small consolation XP
                GameManager.Instance.AddExperience(5);

                Debug.Log("[CraftingTable] Craft failed - no matching recipe");
            }

            OnCraftingComplete?.Invoke(matchedRecipe, isNewDiscovery);

            // Clear slots
            ClearAllSlots();
        }

        /// <summary>
        /// Check if current ingredients match any recipe
        /// </summary>
        private void CheckRecipeMatch()
        {
            if (currentIngredients.Count < 2)
            {
                matchedRecipe = null;
                return;
            }

            // Get ingredient IDs (excluding nulls)
            var ingredientIds = currentIngredients
                .Where(i => i != null)
                .Select(i => i.Id)
                .ToList();

            // Find matching recipe
            matchedRecipe = RecipeDatabase.FindRecipeByIngredients(ingredientIds);

            if (matchedRecipe != null)
            {
                Debug.Log($"[CraftingTable] Recipe match found: {matchedRecipe.DisplayName}");
            }
        }

        /// <summary>
        /// Get hint for current ingredients
        /// </summary>
        public string GetHintForCurrentIngredients()
        {
            if (currentIngredients.Count < 2)
                return "Add more ingredients...";

            if (matchedRecipe != null)
            {
                var playerData = GameManager.Instance?.PlayerData;
                if (playerData.UnlockedRecipeIds.Contains(matchedRecipe.Id))
                {
                    return matchedRecipe.DisplayName;
                }
                else
                {
                    return "Unknown recipe...";
                }
            }

            return "No recipe match";
        }

        /// <summary>
        /// Unlock additional slot
        /// </summary>
        public void UnlockSlot()
        {
            if (unlockedSlots < maxSlots)
            {
                unlockedSlots++;
                UpdateSlotStates();
                Debug.Log($"[CraftingTable] Unlocked slot {unlockedSlots}");
            }
        }

        // Event handlers
        private void HandleIngredientDropped(int slotIndex, IngredientData ingredient)
        {
            AddIngredientToSlot(ingredient, slotIndex);
        }

        private void HandleSlotClicked(int slotIndex)
        {
            if (slots[slotIndex].HasIngredient)
            {
                RemoveIngredient(slotIndex);
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        /// <summary>
        /// Get current ingredients list
        /// </summary>
        public List<IngredientData> GetCurrentIngredients()
        {
            return currentIngredients.Where(i => i != null).ToList();
        }
    }
}
