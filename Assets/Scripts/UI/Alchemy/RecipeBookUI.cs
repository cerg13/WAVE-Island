using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using WaveIsland.Core;
using WaveIsland.Data;
using WaveIsland.Alchemy;

namespace WaveIsland.UI.Alchemy
{
    /// <summary>
    /// Recipe Book UI - shows all recipes (discovered and undiscovered)
    /// </summary>
    public class RecipeBookUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject bookPanel;
        [SerializeField] private Button closeButton;

        [Header("Categories")]
        [SerializeField] private Transform categoryTabsContainer;
        [SerializeField] private GameObject categoryTabPrefab;

        [Header("Recipe List")]
        [SerializeField] private Transform recipeListContent;
        [SerializeField] private GameObject recipeItemPrefab;
        [SerializeField] private ScrollRect scrollRect;

        [Header("Recipe Detail")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private Image recipeIcon;
        [SerializeField] private Text recipeName;
        [SerializeField] private Text recipeDescription;
        [SerializeField] private Text difficultyText;
        [SerializeField] private Transform ingredientsContainer;
        [SerializeField] private GameObject ingredientIconPrefab;
        [SerializeField] private Text rewardsText;
        [SerializeField] private Button craftButton;
        [SerializeField] private Button closeDetailButton;

        [Header("Stats")]
        [SerializeField] private Text discoveredCountText;
        [SerializeField] private Slider discoveryProgressBar;

        [Header("Hint System")]
        [SerializeField] private Text hintText;
        [SerializeField] private Button buyHintButton;
        [SerializeField] private Text hintCostText;
        [SerializeField] private int[] hintCosts = { 100, 200, 300, 400 };

        private RecipeCategory currentCategory = RecipeCategory.ClassicCocktail;
        private RecipeData selectedRecipe = null;
        private int currentHintLevel = 0;
        private List<RecipeItemUI> recipeItems = new List<RecipeItemUI>();
        private Dictionary<RecipeCategory, Button> categoryButtons = new Dictionary<RecipeCategory, Button>();

        private void Start()
        {
            InitializeUI();
            SetupCategoryTabs();
            RefreshRecipeList();
            UpdateStats();
        }

        private void InitializeUI()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);

            if (closeDetailButton != null)
                closeDetailButton.onClick.AddListener(CloseDetail);

            if (craftButton != null)
                craftButton.onClick.AddListener(OnCraftClicked);

            if (buyHintButton != null)
                buyHintButton.onClick.AddListener(OnBuyHintClicked);

            if (detailPanel != null)
                detailPanel.SetActive(false);
        }

        private void SetupCategoryTabs()
        {
            if (categoryTabsContainer == null) return;

            var categories = new List<(RecipeCategory cat, string name)>
            {
                (RecipeCategory.ClassicCocktail, "Classic"),
                (RecipeCategory.TropicalCocktail, "Tropical"),
                (RecipeCategory.SignatureCocktail, "Signature"),
                (RecipeCategory.Mocktail, "Mocktails"),
                (RecipeCategory.Appetizer, "Appetizers"),
                (RecipeCategory.MainCourse, "Main"),
                (RecipeCategory.Dessert, "Desserts")
            };

            foreach (var (cat, name) in categories)
            {
                GameObject tabObj;

                if (categoryTabPrefab != null)
                {
                    tabObj = Instantiate(categoryTabPrefab, categoryTabsContainer);
                }
                else
                {
                    tabObj = new GameObject(name);
                    tabObj.transform.SetParent(categoryTabsContainer);
                    tabObj.AddComponent<Image>();
                    var btn = tabObj.AddComponent<Button>();
                    var txt = new GameObject("Text").AddComponent<Text>();
                    txt.transform.SetParent(tabObj.transform);
                    txt.text = name;
                }

                var button = tabObj.GetComponent<Button>();
                var category = cat; // Capture for lambda

                button.onClick.AddListener(() => SelectCategory(category));
                categoryButtons[cat] = button;

                var label = tabObj.GetComponentInChildren<Text>();
                if (label != null) label.text = name;
            }

            // Select first category
            SelectCategory(RecipeCategory.ClassicCocktail);
        }

        public void Open()
        {
            if (bookPanel != null)
                bookPanel.SetActive(true);

            RefreshRecipeList();
            UpdateStats();
        }

        public void Close()
        {
            if (bookPanel != null)
                bookPanel.SetActive(false);
        }

        private void SelectCategory(RecipeCategory category)
        {
            currentCategory = category;

            // Update tab visuals
            foreach (var kvp in categoryButtons)
            {
                var colors = kvp.Value.colors;
                colors.normalColor = kvp.Key == category ? Color.white : new Color(0.8f, 0.8f, 0.8f);
                kvp.Value.colors = colors;
            }

            RefreshRecipeList();
        }

        private void RefreshRecipeList()
        {
            if (recipeListContent == null) return;

            // Clear existing
            foreach (var item in recipeItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            recipeItems.Clear();

            // Get recipes for category
            var recipes = RecipeDatabase.GetRecipesByCategory(currentCategory);
            var playerData = GameManager.Instance?.PlayerData;

            // Sort: discovered first, then by difficulty
            recipes = recipes
                .OrderByDescending(r => playerData.UnlockedRecipeIds.Contains(r.Id))
                .ThenBy(r => r.Difficulty)
                .ThenBy(r => r.DisplayName)
                .ToList();

            foreach (var recipe in recipes)
            {
                CreateRecipeItem(recipe, playerData);
            }
        }

        private void CreateRecipeItem(RecipeData recipe, PlayerData playerData)
        {
            GameObject itemObj;

            if (recipeItemPrefab != null)
            {
                itemObj = Instantiate(recipeItemPrefab, recipeListContent);
            }
            else
            {
                itemObj = new GameObject($"Recipe_{recipe.Id}");
                itemObj.transform.SetParent(recipeListContent);
            }

            var itemUI = itemObj.GetComponent<RecipeItemUI>();
            if (itemUI == null)
            {
                itemUI = itemObj.AddComponent<RecipeItemUI>();
            }

            bool isDiscovered = playerData.UnlockedRecipeIds.Contains(recipe.Id);
            bool canCraft = recipe.CanCraft(playerData);

            itemUI.Setup(recipe, isDiscovered, canCraft);
            itemUI.OnClicked += () => ShowRecipeDetail(recipe);

            recipeItems.Add(itemUI);
        }

        private void ShowRecipeDetail(RecipeData recipe)
        {
            selectedRecipe = recipe;
            currentHintLevel = 0;

            if (detailPanel != null)
                detailPanel.SetActive(true);

            var playerData = GameManager.Instance?.PlayerData;
            bool isDiscovered = playerData.UnlockedRecipeIds.Contains(recipe.Id);

            // Set basic info
            if (recipeName != null)
                recipeName.text = isDiscovered ? recipe.DisplayName : "???";

            if (recipeDescription != null)
                recipeDescription.text = isDiscovered ? recipe.Description : "Undiscovered recipe";

            if (recipeIcon != null)
            {
                recipeIcon.sprite = isDiscovered ? recipe.Icon : null;
                recipeIcon.enabled = isDiscovered && recipe.Icon != null;
            }

            if (difficultyText != null)
                difficultyText.text = $"Difficulty: {recipe.Difficulty}";

            // Show ingredients
            ShowIngredients(recipe, isDiscovered);

            // Rewards
            if (rewardsText != null)
            {
                rewardsText.text = isDiscovered
                    ? $"Rewards: {recipe.CoinsReward} coins, {recipe.ExperienceReward} XP"
                    : "Rewards: ???";
            }

            // Craft button
            if (craftButton != null)
            {
                bool canCraft = isDiscovered && recipe.CanCraft(playerData);
                craftButton.interactable = canCraft;
                craftButton.GetComponentInChildren<Text>().text = canCraft ? "Craft Now" : "Missing Ingredients";
            }

            // Hint system
            UpdateHintUI(recipe, isDiscovered);
        }

        private void ShowIngredients(RecipeData recipe, bool isDiscovered)
        {
            if (ingredientsContainer == null) return;

            // Clear existing
            foreach (Transform child in ingredientsContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var ri in recipe.Ingredients)
            {
                GameObject iconObj;

                if (ingredientIconPrefab != null)
                {
                    iconObj = Instantiate(ingredientIconPrefab, ingredientsContainer);
                }
                else
                {
                    iconObj = new GameObject("Ingredient");
                    iconObj.transform.SetParent(ingredientsContainer);
                    iconObj.AddComponent<Image>();
                }

                var image = iconObj.GetComponent<Image>();
                var text = iconObj.GetComponentInChildren<Text>();

                if (isDiscovered)
                {
                    if (image != null)
                    {
                        image.sprite = ri.Ingredient.Icon;
                        image.enabled = ri.Ingredient.Icon != null;
                    }

                    if (text != null)
                        text.text = ri.Quantity > 1 ? $"x{ri.Quantity}" : "";
                }
                else
                {
                    if (image != null)
                    {
                        image.enabled = false;
                    }

                    if (text != null)
                        text.text = "?";
                }
            }
        }

        private void UpdateHintUI(RecipeData recipe, bool isDiscovered)
        {
            if (buyHintButton == null) return;

            if (isDiscovered)
            {
                buyHintButton.gameObject.SetActive(false);
                if (hintText != null)
                    hintText.text = string.Join(" + ",
                        recipe.Ingredients.Select(ri => ri.Ingredient.DisplayName));
            }
            else
            {
                buyHintButton.gameObject.SetActive(true);

                string hint = RecipeMatcher.GetRecipeHint(recipe, currentHintLevel,
                    GameManager.Instance.PlayerData);

                if (hintText != null)
                    hintText.text = hint;

                if (hintCostText != null && currentHintLevel < hintCosts.Length)
                    hintCostText.text = $"{hintCosts[currentHintLevel]} coins";

                buyHintButton.interactable = currentHintLevel < hintCosts.Length;
            }
        }

        private void OnBuyHintClicked()
        {
            if (selectedRecipe == null) return;
            if (currentHintLevel >= hintCosts.Length) return;

            var playerData = GameManager.Instance?.PlayerData;
            int cost = hintCosts[currentHintLevel];

            if (playerData.Coins >= cost)
            {
                playerData.Coins -= cost;
                currentHintLevel++;

                UpdateHintUI(selectedRecipe, false);
                Debug.Log($"[RecipeBook] Bought hint level {currentHintLevel}");
            }
            else
            {
                Debug.Log("[RecipeBook] Not enough coins for hint");
            }
        }

        private void CloseDetail()
        {
            if (detailPanel != null)
                detailPanel.SetActive(false);

            selectedRecipe = null;
        }

        private void OnCraftClicked()
        {
            if (selectedRecipe == null) return;

            // TODO: Navigate to crafting table with recipe pre-selected
            Debug.Log($"[RecipeBook] Craft: {selectedRecipe.DisplayName}");
            CloseDetail();
            Close();
        }

        private void UpdateStats()
        {
            var playerData = GameManager.Instance?.PlayerData;
            int totalRecipes = RecipeDatabase.GetTotalRecipeCount();
            int discovered = playerData?.TotalRecipesDiscovered ?? 0;

            if (discoveredCountText != null)
                discoveredCountText.text = $"{discovered}/{totalRecipes}";

            if (discoveryProgressBar != null)
                discoveryProgressBar.value = totalRecipes > 0 ? (float)discovered / totalRecipes : 0f;
        }
    }

    /// <summary>
    /// UI component for a single recipe item in the list
    /// </summary>
    public class RecipeItemUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Text nameText;
        [SerializeField] private Image difficultyIndicator;
        [SerializeField] private GameObject discoveredBadge;
        [SerializeField] private GameObject craftableBadge;
        [SerializeField] private Image lockOverlay;
        [SerializeField] private Button button;

        public event System.Action OnClicked;

        private RecipeData recipe;

        private void Awake()
        {
            if (button == null)
                button = GetComponent<Button>();

            if (button != null)
                button.onClick.AddListener(() => OnClicked?.Invoke());
        }

        public void Setup(RecipeData recipeData, bool isDiscovered, bool canCraft)
        {
            recipe = recipeData;

            if (iconImage != null)
            {
                iconImage.sprite = isDiscovered ? recipe.Icon : null;
                iconImage.enabled = isDiscovered && recipe.Icon != null;
                iconImage.color = isDiscovered ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            }

            if (nameText != null)
            {
                nameText.text = isDiscovered ? recipe.DisplayName : "???";
                nameText.color = isDiscovered ? Color.white : Color.gray;
            }

            if (discoveredBadge != null)
                discoveredBadge.SetActive(isDiscovered);

            if (craftableBadge != null)
                craftableBadge.SetActive(canCraft);

            if (lockOverlay != null)
                lockOverlay.gameObject.SetActive(!isDiscovered);

            if (difficultyIndicator != null)
            {
                difficultyIndicator.color = GetDifficultyColor(recipe.Difficulty);
            }
        }

        private Color GetDifficultyColor(RecipeDifficulty difficulty)
        {
            switch (difficulty)
            {
                case RecipeDifficulty.Easy: return Color.green;
                case RecipeDifficulty.Medium: return Color.yellow;
                case RecipeDifficulty.Hard: return new Color(1f, 0.5f, 0f);
                case RecipeDifficulty.Legendary: return new Color(1f, 0f, 1f);
                default: return Color.white;
            }
        }
    }
}
