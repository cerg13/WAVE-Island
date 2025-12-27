using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using WaveIsland.Core;
using WaveIsland.Data;
using WaveIsland.Alchemy;

namespace WaveIsland.UI.Alchemy
{
    /// <summary>
    /// Main controller for the Alchemy/Crafting UI
    /// </summary>
    public class AlchemyUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CraftingTable craftingTable;

        [Header("Main Panels")]
        [SerializeField] private GameObject alchemyPanel;
        [SerializeField] private GameObject ingredientPanel;
        [SerializeField] private GameObject recipeBookPanel;
        [SerializeField] private GameObject craftingResultPanel;

        [Header("Crafting Area")]
        [SerializeField] private Transform slotsContainer;
        [SerializeField] private Image resultPreviewIcon;
        [SerializeField] private Text resultPreviewText;
        [SerializeField] private Button craftButton;
        [SerializeField] private Button clearButton;
        [SerializeField] private Slider craftingProgressBar;
        [SerializeField] private Text craftingStatusText;

        [Header("Ingredient List")]
        [SerializeField] private Transform ingredientListContent;
        [SerializeField] private GameObject ingredientItemPrefab;
        [SerializeField] private Dropdown categoryFilter;
        [SerializeField] private InputField searchField;

        [Header("Result Display")]
        [SerializeField] private Image resultIcon;
        [SerializeField] private Text resultNameText;
        [SerializeField] private Text resultDescriptionText;
        [SerializeField] private Text rewardText;
        [SerializeField] private GameObject newDiscoveryBadge;
        [SerializeField] private Button closeResultButton;

        [Header("Navigation")]
        [SerializeField] private Button recipeBookButton;
        [SerializeField] private Button backButton;

        [Header("Hints")]
        [SerializeField] private Text hintText;
        [SerializeField] private Button buyHintButton;
        [SerializeField] private Text hintCostText;

        private List<IngredientDragHandler> ingredientItems = new List<IngredientDragHandler>();
        private IngredientCategory? currentFilter = null;
        private string searchQuery = "";

        private void Start()
        {
            InitializeUI();
            SetupEventListeners();
            RefreshIngredientList();
        }

        private void Update()
        {
            if (craftingTable != null && craftingTable.IsCrafting)
            {
                UpdateCraftingProgress();
            }
        }

        private void InitializeUI()
        {
            // Hide panels
            if (craftingResultPanel != null)
                craftingResultPanel.SetActive(false);

            if (recipeBookPanel != null)
                recipeBookPanel.SetActive(false);

            // Setup buttons
            if (craftButton != null)
            {
                craftButton.onClick.AddListener(OnCraftClicked);
                craftButton.interactable = false;
            }

            if (clearButton != null)
                clearButton.onClick.AddListener(OnClearClicked);

            if (recipeBookButton != null)
                recipeBookButton.onClick.AddListener(OnRecipeBookClicked);

            if (closeResultButton != null)
                closeResultButton.onClick.AddListener(CloseResultPanel);

            if (buyHintButton != null)
                buyHintButton.onClick.AddListener(OnBuyHintClicked);

            // Setup filter dropdown
            if (categoryFilter != null)
            {
                categoryFilter.ClearOptions();
                categoryFilter.AddOptions(new List<string> {
                    "All",
                    "Herbs",
                    "Citrus",
                    "Tropical",
                    "Spirits",
                    "Mixers",
                    "Sweeteners"
                });
                categoryFilter.onValueChanged.AddListener(OnFilterChanged);
            }

            // Setup search
            if (searchField != null)
            {
                searchField.onValueChanged.AddListener(OnSearchChanged);
            }

            UpdateCraftButton();
            UpdateHintDisplay();
        }

        private void SetupEventListeners()
        {
            if (craftingTable != null)
            {
                craftingTable.OnSlotsChanged += OnSlotsChanged;
                craftingTable.OnCraftingStarted += OnCraftingStarted;
                craftingTable.OnCraftingComplete += OnCraftingComplete;
                craftingTable.OnRecipeDiscovered += OnRecipeDiscovered;
            }
        }

        private void OnDestroy()
        {
            if (craftingTable != null)
            {
                craftingTable.OnSlotsChanged -= OnSlotsChanged;
                craftingTable.OnCraftingStarted -= OnCraftingStarted;
                craftingTable.OnCraftingComplete -= OnCraftingComplete;
                craftingTable.OnRecipeDiscovered -= OnRecipeDiscovered;
            }
        }

        /// <summary>
        /// Refresh the ingredient list based on player inventory
        /// </summary>
        public void RefreshIngredientList()
        {
            if (ingredientListContent == null) return;

            // Clear existing items
            foreach (var item in ingredientItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            ingredientItems.Clear();

            var playerData = GameManager.Instance?.PlayerData;
            if (playerData == null) return;

            // Get ingredients from inventory
            foreach (var invItem in playerData.Inventory)
            {
                // Skip seeds
                if (invItem.ItemId.StartsWith("seed_")) continue;

                var ingredientData = IngredientDatabase.GetIngredient(invItem.ItemId);
                if (ingredientData == null) continue;

                // Apply filters
                if (currentFilter.HasValue && ingredientData.Category != currentFilter.Value)
                    continue;

                if (!string.IsNullOrEmpty(searchQuery) &&
                    !ingredientData.DisplayName.ToLower().Contains(searchQuery.ToLower()))
                    continue;

                CreateIngredientItem(ingredientData, invItem.Quantity);
            }
        }

        private void CreateIngredientItem(IngredientData ingredient, int quantity)
        {
            GameObject itemObj;

            if (ingredientItemPrefab != null)
            {
                itemObj = Instantiate(ingredientItemPrefab, ingredientListContent);
            }
            else
            {
                itemObj = new GameObject($"Ingredient_{ingredient.Id}");
                itemObj.transform.SetParent(ingredientListContent);
            }

            var dragHandler = itemObj.GetComponent<IngredientDragHandler>();
            if (dragHandler == null)
            {
                dragHandler = itemObj.AddComponent<IngredientDragHandler>();
            }

            dragHandler.Setup(ingredient, quantity);
            dragHandler.OnIngredientClicked += OnIngredientClicked;
            dragHandler.OnDragEnded += OnIngredientDragEnded;

            ingredientItems.Add(dragHandler);
        }

        private void OnIngredientClicked(IngredientData ingredient)
        {
            // Quick add to crafting table
            craftingTable?.AddIngredient(ingredient);
            RefreshIngredientList();
        }

        private void OnIngredientDragEnded(IngredientData ingredient, bool wasDropped)
        {
            if (wasDropped)
            {
                RefreshIngredientList();
            }
        }

        private void OnSlotsChanged()
        {
            UpdateCraftButton();
            UpdateResultPreview();
            UpdateHintDisplay();
            RefreshIngredientList();
        }

        private void UpdateCraftButton()
        {
            if (craftButton == null) return;

            bool canCraft = craftingTable != null &&
                           craftingTable.UsedSlots >= 2 &&
                           !craftingTable.IsCrafting;

            craftButton.interactable = canCraft;

            // Update button text based on recipe match
            var buttonText = craftButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                if (craftingTable.MatchedRecipe != null)
                {
                    buttonText.text = "CRAFT!";
                }
                else if (craftingTable.UsedSlots >= 2)
                {
                    buttonText.text = "Try Craft";
                }
                else
                {
                    buttonText.text = "Add Ingredients";
                }
            }
        }

        private void UpdateResultPreview()
        {
            if (resultPreviewIcon == null || resultPreviewText == null) return;

            var recipe = craftingTable?.MatchedRecipe;
            var playerData = GameManager.Instance?.PlayerData;

            if (recipe != null)
            {
                bool isDiscovered = playerData.UnlockedRecipeIds.Contains(recipe.Id);

                if (isDiscovered)
                {
                    resultPreviewIcon.sprite = recipe.Icon;
                    resultPreviewIcon.enabled = recipe.Icon != null;
                    resultPreviewText.text = recipe.DisplayName;
                }
                else
                {
                    resultPreviewIcon.enabled = false;
                    resultPreviewText.text = "???";
                }
            }
            else
            {
                resultPreviewIcon.enabled = false;
                resultPreviewText.text = craftingTable?.UsedSlots >= 2 ? "???" : "...";
            }
        }

        private void UpdateHintDisplay()
        {
            if (hintText == null) return;

            string hint = craftingTable?.GetHintForCurrentIngredients() ?? "";
            hintText.text = hint;

            // Show buy hint button if recipe exists but not discovered
            if (buyHintButton != null)
            {
                var recipe = craftingTable?.MatchedRecipe;
                var playerData = GameManager.Instance?.PlayerData;

                bool showHintButton = recipe != null &&
                                     !playerData.UnlockedRecipeIds.Contains(recipe.Id);

                buyHintButton.gameObject.SetActive(showHintButton);
            }
        }

        private void OnCraftClicked()
        {
            craftingTable?.StartCrafting();
        }

        private void OnClearClicked()
        {
            craftingTable?.ClearAllSlots();
        }

        private void OnCraftingStarted()
        {
            if (craftButton != null)
                craftButton.interactable = false;

            if (clearButton != null)
                clearButton.interactable = false;

            if (craftingProgressBar != null)
            {
                craftingProgressBar.gameObject.SetActive(true);
                craftingProgressBar.value = 0;
            }

            if (craftingStatusText != null)
            {
                craftingStatusText.text = "Crafting...";
                craftingStatusText.gameObject.SetActive(true);
            }
        }

        private void UpdateCraftingProgress()
        {
            if (craftingProgressBar != null && craftingTable != null)
            {
                craftingProgressBar.value = craftingTable.CraftProgress;
            }
        }

        private void OnCraftingComplete(RecipeData recipe, bool isNewDiscovery)
        {
            if (clearButton != null)
                clearButton.interactable = true;

            if (craftingProgressBar != null)
                craftingProgressBar.gameObject.SetActive(false);

            if (craftingStatusText != null)
                craftingStatusText.gameObject.SetActive(false);

            // Show result
            ShowCraftingResult(recipe, isNewDiscovery);

            UpdateCraftButton();
        }

        private void ShowCraftingResult(RecipeData recipe, bool isNewDiscovery)
        {
            if (craftingResultPanel == null) return;

            craftingResultPanel.SetActive(true);

            if (recipe != null)
            {
                if (resultIcon != null)
                {
                    resultIcon.sprite = recipe.Icon;
                    resultIcon.enabled = recipe.Icon != null;
                }

                if (resultNameText != null)
                    resultNameText.text = recipe.DisplayName;

                if (resultDescriptionText != null)
                    resultDescriptionText.text = recipe.Description;

                if (rewardText != null)
                    rewardText.text = $"+{recipe.CoinsReward} coins, +{recipe.ExperienceReward} XP";

                if (newDiscoveryBadge != null)
                    newDiscoveryBadge.SetActive(isNewDiscovery);
            }
            else
            {
                // Failed craft
                if (resultIcon != null)
                    resultIcon.enabled = false;

                if (resultNameText != null)
                    resultNameText.text = "Craft Failed";

                if (resultDescriptionText != null)
                    resultDescriptionText.text = "No matching recipe found. Try different combinations!";

                if (rewardText != null)
                    rewardText.text = "+5 XP";

                if (newDiscoveryBadge != null)
                    newDiscoveryBadge.SetActive(false);
            }
        }

        private void CloseResultPanel()
        {
            if (craftingResultPanel != null)
                craftingResultPanel.SetActive(false);

            RefreshIngredientList();
        }

        private void OnRecipeDiscovered(RecipeData recipe)
        {
            // Could show special animation/notification here
            Debug.Log($"[AlchemyUI] Discovered: {recipe.DisplayName}!");
        }

        private void OnRecipeBookClicked()
        {
            // Show recipe book (handled by RecipeBookUI)
            if (recipeBookPanel != null)
                recipeBookPanel.SetActive(true);
        }

        private void OnBuyHintClicked()
        {
            var playerData = GameManager.Instance?.PlayerData;
            int hintCost = 100; // Base cost

            if (playerData.Coins >= hintCost)
            {
                playerData.Coins -= hintCost;
                // Show next level hint
                UpdateHintDisplay();
            }
            else
            {
                Debug.Log("Not enough coins for hint!");
            }
        }

        private void OnFilterChanged(int index)
        {
            switch (index)
            {
                case 0: currentFilter = null; break;
                case 1: currentFilter = IngredientCategory.Herb; break;
                case 2: currentFilter = IngredientCategory.Citrus; break;
                case 3: currentFilter = IngredientCategory.Tropical; break;
                case 4: currentFilter = IngredientCategory.Spirit; break;
                case 5: currentFilter = IngredientCategory.Mixer; break;
                case 6: currentFilter = IngredientCategory.Sweetener; break;
            }

            RefreshIngredientList();
        }

        private void OnSearchChanged(string query)
        {
            searchQuery = query;
            RefreshIngredientList();
        }
    }
}
