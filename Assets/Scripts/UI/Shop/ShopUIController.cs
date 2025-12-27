using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using WaveIsland.Core;
using WaveIsland.Shop;

namespace WaveIsland.UI.Shop
{
    /// <summary>
    /// Main UI controller for the Shop screen
    /// </summary>
    public class ShopUIController : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private Button closeButton;

        [Header("Category Tabs")]
        [SerializeField] private Button ingredientsTabButton;
        [SerializeField] private Button seedsTabButton;
        [SerializeField] private Button specialsTabButton;
        [SerializeField] private Color activeTabColor = Color.white;
        [SerializeField] private Color inactiveTabColor = new Color(0.7f, 0.7f, 0.7f);

        [Header("Items Grid")]
        [SerializeField] private Transform itemsContainer;
        [SerializeField] private GameObject shopItemPrefab;
        [SerializeField] private ScrollRect scrollRect;

        [Header("Player Info")]
        [SerializeField] private Text coinsText;
        [SerializeField] private Text levelText;

        [Header("Daily Specials")]
        [SerializeField] private Text refreshTimerText;
        [SerializeField] private GameObject specialsBanner;

        [Header("Purchase Modal")]
        [SerializeField] private GameObject purchaseModal;
        [SerializeField] private Image modalItemIcon;
        [SerializeField] private Text modalItemName;
        [SerializeField] private Text modalItemDescription;
        [SerializeField] private Text modalPriceText;
        [SerializeField] private Text modalOwnedText;
        [SerializeField] private Button modalBuyOneButton;
        [SerializeField] private Button modalBuyFiveButton;
        [SerializeField] private Button modalBuyTenButton;
        [SerializeField] private Button modalCloseButton;
        [SerializeField] private Text modalResultText;

        [Header("Sell Mode")]
        [SerializeField] private Toggle sellModeToggle;
        [SerializeField] private Text sellModeLabel;

        private List<ShopItemUI> itemUIs = new List<ShopItemUI>();
        private ShopItem selectedItem;
        private bool isSellMode = false;

        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void InitializeUI()
        {
            // Close button
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);

            // Category tabs
            if (ingredientsTabButton != null)
                ingredientsTabButton.onClick.AddListener(() => SelectCategory(ShopCategory.Ingredients));

            if (seedsTabButton != null)
                seedsTabButton.onClick.AddListener(() => SelectCategory(ShopCategory.Seeds));

            if (specialsTabButton != null)
                specialsTabButton.onClick.AddListener(() => SelectCategory(ShopCategory.DailySpecials));

            // Purchase modal
            if (modalCloseButton != null)
                modalCloseButton.onClick.AddListener(ClosePurchaseModal);

            if (modalBuyOneButton != null)
                modalBuyOneButton.onClick.AddListener(() => OnBuyClicked(1));

            if (modalBuyFiveButton != null)
                modalBuyFiveButton.onClick.AddListener(() => OnBuyClicked(5));

            if (modalBuyTenButton != null)
                modalBuyTenButton.onClick.AddListener(() => OnBuyClicked(10));

            // Sell mode toggle
            if (sellModeToggle != null)
            {
                sellModeToggle.onValueChanged.AddListener(OnSellModeChanged);
            }

            // Initial state
            if (purchaseModal != null)
                purchaseModal.SetActive(false);

            if (specialsBanner != null)
                specialsBanner.SetActive(false);
        }

        private void SubscribeToEvents()
        {
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OnCategoryChanged += OnCategoryChanged;
                ShopManager.Instance.OnItemPurchased += OnItemPurchased;
                ShopManager.Instance.OnShopRefreshed += RefreshShop;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.OnCategoryChanged -= OnCategoryChanged;
                ShopManager.Instance.OnItemPurchased -= OnItemPurchased;
                ShopManager.Instance.OnShopRefreshed -= RefreshShop;
            }
        }

        private void Update()
        {
            if (shopPanel != null && shopPanel.activeSelf)
            {
                UpdateRefreshTimer();
            }
        }

        public void Open()
        {
            if (shopPanel != null)
                shopPanel.SetActive(true);

            RefreshShop();
            SelectCategory(ShopCategory.Ingredients);
            UpdatePlayerInfo();
        }

        public void Close()
        {
            if (shopPanel != null)
                shopPanel.SetActive(false);
        }

        private void SelectCategory(ShopCategory category)
        {
            ShopManager.Instance?.SetCategory(category);

            // Update tab visuals
            UpdateTabVisuals(category);

            // Show/hide specials banner
            if (specialsBanner != null)
                specialsBanner.SetActive(category == ShopCategory.DailySpecials);

            // Refresh items
            RefreshItems(category);
        }

        private void UpdateTabVisuals(ShopCategory category)
        {
            if (ingredientsTabButton != null)
            {
                var colors = ingredientsTabButton.colors;
                colors.normalColor = category == ShopCategory.Ingredients ? activeTabColor : inactiveTabColor;
                ingredientsTabButton.colors = colors;
            }

            if (seedsTabButton != null)
            {
                var colors = seedsTabButton.colors;
                colors.normalColor = category == ShopCategory.Seeds ? activeTabColor : inactiveTabColor;
                seedsTabButton.colors = colors;
            }

            if (specialsTabButton != null)
            {
                var colors = specialsTabButton.colors;
                colors.normalColor = category == ShopCategory.DailySpecials ? activeTabColor : inactiveTabColor;
                specialsTabButton.colors = colors;
            }
        }

        private void RefreshShop()
        {
            var category = ShopManager.Instance?.GetCurrentCategory() ?? ShopCategory.Ingredients;
            RefreshItems(category);
            UpdatePlayerInfo();
        }

        private void RefreshItems(ShopCategory category)
        {
            if (itemsContainer == null) return;

            // Clear existing items
            foreach (var item in itemUIs)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
            itemUIs.Clear();

            // Get items for category
            var items = ShopManager.Instance?.GetShopItems(category);
            if (items == null) return;

            // Create UI items
            foreach (var item in items)
            {
                CreateItemUI(item);
            }

            // Reset scroll
            if (scrollRect != null)
                scrollRect.verticalNormalizedPosition = 1f;
        }

        private void CreateItemUI(ShopItem item)
        {
            GameObject itemObj;

            if (shopItemPrefab != null)
            {
                itemObj = Instantiate(shopItemPrefab, itemsContainer);
            }
            else
            {
                // Create basic item if no prefab
                itemObj = CreateBasicItemUI();
                itemObj.transform.SetParent(itemsContainer);
            }

            var itemUI = itemObj.GetComponent<ShopItemUI>();
            if (itemUI == null)
            {
                itemUI = itemObj.AddComponent<ShopItemUI>();
            }

            itemUI.Setup(item, isSellMode);
            itemUI.OnClicked += () => OnItemClicked(item);

            itemUIs.Add(itemUI);
        }

        private GameObject CreateBasicItemUI()
        {
            var obj = new GameObject("ShopItem");

            // Background
            var bg = obj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Layout
            var layout = obj.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 5;
            layout.childAlignment = TextAnchor.MiddleCenter;

            // Size
            var rectTransform = obj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(150, 180);

            return obj;
        }

        private void OnItemClicked(ShopItem item)
        {
            selectedItem = item;
            ShowPurchaseModal(item);
        }

        private void ShowPurchaseModal(ShopItem item)
        {
            if (purchaseModal == null) return;

            purchaseModal.SetActive(true);

            // Set item info
            if (modalItemIcon != null)
            {
                modalItemIcon.sprite = item.Icon;
                modalItemIcon.enabled = item.Icon != null;
            }

            if (modalItemName != null)
                modalItemName.text = item.DisplayName;

            if (modalItemDescription != null)
                modalItemDescription.text = item.Description;

            if (modalOwnedText != null)
                modalOwnedText.text = $"Owned: {item.PlayerOwned}";

            // Price display
            if (modalPriceText != null)
            {
                if (isSellMode)
                {
                    var sellPrice = GetSellPrice(item);
                    modalPriceText.text = $"Sell: {sellPrice} coins each";
                }
                else
                {
                    if (item.IsSpecial && item.OriginalPrice > item.Price)
                    {
                        modalPriceText.text = $"<color=#FF0000><s>{item.OriginalPrice}</s></color> {item.Price} coins";
                    }
                    else
                    {
                        modalPriceText.text = $"{item.Price} coins";
                    }
                }
            }

            // Update button labels for sell mode
            if (isSellMode)
            {
                SetButtonText(modalBuyOneButton, "Sell 1");
                SetButtonText(modalBuyFiveButton, "Sell 5");
                SetButtonText(modalBuyTenButton, "Sell All");

                // Disable buttons if not enough items
                if (modalBuyOneButton != null) modalBuyOneButton.interactable = item.PlayerOwned >= 1;
                if (modalBuyFiveButton != null) modalBuyFiveButton.interactable = item.PlayerOwned >= 5;
                if (modalBuyTenButton != null) modalBuyTenButton.interactable = item.PlayerOwned >= 1;
            }
            else
            {
                SetButtonText(modalBuyOneButton, "Buy 1");
                SetButtonText(modalBuyFiveButton, "Buy 5");
                SetButtonText(modalBuyTenButton, "Buy 10");

                // Check if player can afford
                var coins = GameManager.Instance?.PlayerData?.Coins ?? 0;
                if (modalBuyOneButton != null) modalBuyOneButton.interactable = item.IsAvailable && coins >= item.Price;
                if (modalBuyFiveButton != null) modalBuyFiveButton.interactable = item.IsAvailable && coins >= item.Price * 5;
                if (modalBuyTenButton != null) modalBuyTenButton.interactable = item.IsAvailable && coins >= item.Price * 10;
            }

            // Clear result text
            if (modalResultText != null)
                modalResultText.text = "";
        }

        private void SetButtonText(Button button, string text)
        {
            if (button == null) return;
            var txt = button.GetComponentInChildren<Text>();
            if (txt != null) txt.text = text;
        }

        private int GetSellPrice(ShopItem item)
        {
            if (item.ItemType == ShopItemType.Ingredient)
            {
                var ingredient = IngredientDatabase.GetIngredient(item.Id);
                return ingredient?.SellPrice ?? 0;
            }
            return 0; // Seeds can't be sold
        }

        private void ClosePurchaseModal()
        {
            if (purchaseModal != null)
                purchaseModal.SetActive(false);

            selectedItem = null;
        }

        private void OnBuyClicked(int quantity)
        {
            if (selectedItem == null) return;

            if (isSellMode)
            {
                // Sell mode - sell items
                int sellQuantity = quantity == 10 ? selectedItem.PlayerOwned : quantity;
                var result = ShopManager.Instance?.SellItem(selectedItem.Id, selectedItem.ItemType, sellQuantity);

                if (result != null)
                {
                    ShowResult(result.Success, result.Message);

                    if (result.Success)
                    {
                        selectedItem.PlayerOwned -= result.Quantity;
                        UpdateModalOwned();
                    }
                }
            }
            else
            {
                // Buy mode - purchase items
                var result = ShopManager.Instance?.PurchaseItem(selectedItem, quantity);

                if (result != null)
                {
                    ShowResult(result.Success, result.Message);

                    if (result.Success)
                    {
                        selectedItem.PlayerOwned += result.Quantity;
                        UpdateModalOwned();
                    }
                }
            }

            UpdatePlayerInfo();
        }

        private void ShowResult(bool success, string message)
        {
            if (modalResultText != null)
            {
                modalResultText.text = message;
                modalResultText.color = success ? Color.green : Color.red;
            }
        }

        private void UpdateModalOwned()
        {
            if (modalOwnedText != null && selectedItem != null)
            {
                modalOwnedText.text = $"Owned: {selectedItem.PlayerOwned}";
            }

            // Update button states
            if (isSellMode)
            {
                if (modalBuyOneButton != null) modalBuyOneButton.interactable = selectedItem.PlayerOwned >= 1;
                if (modalBuyFiveButton != null) modalBuyFiveButton.interactable = selectedItem.PlayerOwned >= 5;
                if (modalBuyTenButton != null) modalBuyTenButton.interactable = selectedItem.PlayerOwned >= 1;
            }
        }

        private void UpdatePlayerInfo()
        {
            var playerData = GameManager.Instance?.PlayerData;

            if (coinsText != null)
                coinsText.text = $"{playerData?.Coins ?? 0}";

            if (levelText != null)
                levelText.text = $"Lv. {playerData?.Level ?? 1}";
        }

        private void UpdateRefreshTimer()
        {
            if (refreshTimerText == null) return;

            var timeLeft = ShopManager.Instance?.GetTimeUntilRefresh() ?? TimeSpan.Zero;
            refreshTimerText.text = $"Refresh in: {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}";
        }

        private void OnSellModeChanged(bool isOn)
        {
            isSellMode = isOn;

            if (sellModeLabel != null)
                sellModeLabel.text = isOn ? "SELL MODE" : "BUY MODE";

            // Refresh items with new mode
            var category = ShopManager.Instance?.GetCurrentCategory() ?? ShopCategory.Ingredients;
            RefreshItems(category);

            // Close modal if open
            ClosePurchaseModal();
        }

        private void OnCategoryChanged(ShopCategory category)
        {
            RefreshItems(category);
        }

        private void OnItemPurchased(string itemId)
        {
            // Refresh current view
            var category = ShopManager.Instance?.GetCurrentCategory() ?? ShopCategory.Ingredients;
            RefreshItems(category);
        }
    }
}
