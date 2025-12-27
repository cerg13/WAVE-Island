using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using WaveIsland.Core;
using WaveIsland.Data;

namespace WaveIsland.Shop
{
    /// <summary>
    /// Manages the shop system - buying ingredients and seeds
    /// </summary>
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance { get; private set; }

        public event Action<ShopCategory> OnCategoryChanged;
        public event Action<string> OnItemPurchased;
        public event Action OnShopRefreshed;

        [Header("Shop Settings")]
        [SerializeField] private int dailySpecialsCount = 3;
        [SerializeField] private float specialDiscountPercent = 20f;
        [SerializeField] private int shopRefreshHour = 0; // Midnight

        private ShopCategory currentCategory = ShopCategory.Ingredients;
        private List<ShopItem> dailySpecials = new List<ShopItem>();
        private DateTime lastRefreshDate;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            CheckDailyRefresh();
        }

        /// <summary>
        /// Get all items available in the shop for a category
        /// </summary>
        public List<ShopItem> GetShopItems(ShopCategory category)
        {
            var items = new List<ShopItem>();

            switch (category)
            {
                case ShopCategory.Ingredients:
                    items = GetIngredientItems();
                    break;
                case ShopCategory.Seeds:
                    items = GetSeedItems();
                    break;
                case ShopCategory.DailySpecials:
                    items = dailySpecials;
                    break;
            }

            return items;
        }

        private List<ShopItem> GetIngredientItems()
        {
            var items = new List<ShopItem>();
            var ingredients = IngredientDatabase.GetAllIngredients();
            var playerData = GameManager.Instance?.PlayerData;

            foreach (var ingredient in ingredients)
            {
                if (!ingredient.CanBePurchased) continue;

                var item = new ShopItem
                {
                    Id = ingredient.Id,
                    DisplayName = ingredient.DisplayName,
                    DisplayNameRu = ingredient.DisplayNameRu,
                    Description = $"Category: {ingredient.Category}",
                    Icon = ingredient.Icon,
                    Price = ingredient.BuyPrice,
                    OriginalPrice = ingredient.BuyPrice,
                    Category = ShopCategory.Ingredients,
                    ItemType = ShopItemType.Ingredient,
                    Rarity = ingredient.Rarity,
                    IsAvailable = true,
                    PlayerOwned = playerData?.GetIngredientCount(ingredient.Id) ?? 0
                };

                // Check if it's a daily special
                var special = dailySpecials.Find(s => s.Id == ingredient.Id);
                if (special != null)
                {
                    item.Price = special.Price;
                    item.IsSpecial = true;
                }

                items.Add(item);
            }

            return items.OrderBy(i => i.Rarity).ThenBy(i => i.DisplayName).ToList();
        }

        private List<ShopItem> GetSeedItems()
        {
            var items = new List<ShopItem>();
            var plants = PlantDatabase.GetAllPlants();
            var playerData = GameManager.Instance?.PlayerData;

            foreach (var plant in plants)
            {
                var item = new ShopItem
                {
                    Id = $"seed_{plant.Id}",
                    DisplayName = $"{plant.DisplayName} Seeds",
                    DisplayNameRu = $"Семена {plant.DisplayNameRu}",
                    Description = $"Grow time: {plant.GrowthTime}s",
                    Icon = plant.SeedIcon ?? plant.FullyGrownSprite,
                    Price = plant.SeedPrice,
                    OriginalPrice = plant.SeedPrice,
                    Category = ShopCategory.Seeds,
                    ItemType = ShopItemType.Seed,
                    Rarity = plant.Rarity,
                    IsAvailable = plant.UnlockLevel <= (playerData?.Level ?? 1),
                    RequiredLevel = plant.UnlockLevel,
                    PlantId = plant.Id,
                    PlayerOwned = playerData?.GetSeedCount(plant.Id) ?? 0
                };

                // Check if it's a daily special
                var special = dailySpecials.Find(s => s.Id == item.Id);
                if (special != null)
                {
                    item.Price = special.Price;
                    item.IsSpecial = true;
                }

                items.Add(item);
            }

            return items.OrderBy(i => i.RequiredLevel).ThenBy(i => i.DisplayName).ToList();
        }

        /// <summary>
        /// Purchase an item from the shop
        /// </summary>
        public PurchaseResult PurchaseItem(ShopItem item, int quantity = 1)
        {
            var playerData = GameManager.Instance?.PlayerData;
            if (playerData == null)
            {
                return new PurchaseResult { Success = false, Message = "Player data not found" };
            }

            // Check availability
            if (!item.IsAvailable)
            {
                return new PurchaseResult
                {
                    Success = false,
                    Message = $"Requires level {item.RequiredLevel}"
                };
            }

            // Calculate total cost
            int totalCost = item.Price * quantity;

            // Check if player has enough coins
            if (playerData.Coins < totalCost)
            {
                return new PurchaseResult
                {
                    Success = false,
                    Message = "Not enough coins"
                };
            }

            // Deduct coins
            playerData.Coins -= totalCost;

            // Add item to inventory
            switch (item.ItemType)
            {
                case ShopItemType.Ingredient:
                    playerData.AddIngredient(item.Id, quantity);
                    break;
                case ShopItemType.Seed:
                    playerData.AddSeeds(item.PlantId, quantity);
                    break;
            }

            // Save
            SaveSystem.SavePlayerData(playerData);

            Debug.Log($"[Shop] Purchased {quantity}x {item.DisplayName} for {totalCost} coins");
            OnItemPurchased?.Invoke(item.Id);

            return new PurchaseResult
            {
                Success = true,
                Message = $"Purchased {quantity}x {item.DisplayName}",
                ItemId = item.Id,
                Quantity = quantity,
                TotalCost = totalCost
            };
        }

        /// <summary>
        /// Sell an item from inventory
        /// </summary>
        public SellResult SellItem(string itemId, ShopItemType itemType, int quantity = 1)
        {
            var playerData = GameManager.Instance?.PlayerData;
            if (playerData == null)
            {
                return new SellResult { Success = false, Message = "Player data not found" };
            }

            int sellPrice = 0;
            int currentOwned = 0;

            switch (itemType)
            {
                case ShopItemType.Ingredient:
                    var ingredient = IngredientDatabase.GetIngredient(itemId);
                    if (ingredient == null)
                    {
                        return new SellResult { Success = false, Message = "Invalid item" };
                    }
                    sellPrice = ingredient.SellPrice;
                    currentOwned = playerData.GetIngredientCount(itemId);
                    break;

                case ShopItemType.Seed:
                    // Seeds can't be sold back
                    return new SellResult { Success = false, Message = "Seeds cannot be sold" };
            }

            // Check if player has enough
            if (currentOwned < quantity)
            {
                return new SellResult
                {
                    Success = false,
                    Message = $"Not enough items (have {currentOwned})"
                };
            }

            // Remove from inventory
            int totalValue = sellPrice * quantity;

            if (itemType == ShopItemType.Ingredient)
            {
                playerData.RemoveIngredient(itemId, quantity);
            }

            // Add coins
            playerData.Coins += totalValue;

            // Save
            SaveSystem.SavePlayerData(playerData);

            Debug.Log($"[Shop] Sold {quantity}x {itemId} for {totalValue} coins");

            return new SellResult
            {
                Success = true,
                Message = $"Sold for {totalValue} coins",
                ItemId = itemId,
                Quantity = quantity,
                TotalValue = totalValue
            };
        }

        /// <summary>
        /// Change shop category
        /// </summary>
        public void SetCategory(ShopCategory category)
        {
            if (currentCategory != category)
            {
                currentCategory = category;
                OnCategoryChanged?.Invoke(category);
            }
        }

        public ShopCategory GetCurrentCategory() => currentCategory;

        /// <summary>
        /// Check and refresh daily specials if needed
        /// </summary>
        private void CheckDailyRefresh()
        {
            var now = DateTime.Now;
            var lastRefresh = PlayerPrefs.GetString("shop_last_refresh", "");

            if (string.IsNullOrEmpty(lastRefresh) ||
                !DateTime.TryParse(lastRefresh, out lastRefreshDate) ||
                lastRefreshDate.Date < now.Date)
            {
                RefreshDailySpecials();
            }
            else
            {
                // Load cached specials
                LoadDailySpecials();
            }
        }

        /// <summary>
        /// Refresh the daily specials
        /// </summary>
        public void RefreshDailySpecials()
        {
            dailySpecials.Clear();

            var allItems = new List<ShopItem>();
            allItems.AddRange(GetIngredientItems());
            allItems.AddRange(GetSeedItems());

            // Randomly select specials
            var random = new System.Random(DateTime.Now.DayOfYear);
            var shuffled = allItems.OrderBy(x => random.Next()).Take(dailySpecialsCount);

            foreach (var item in shuffled)
            {
                var special = new ShopItem
                {
                    Id = item.Id,
                    DisplayName = item.DisplayName,
                    DisplayNameRu = item.DisplayNameRu,
                    Description = item.Description,
                    Icon = item.Icon,
                    OriginalPrice = item.Price,
                    Price = Mathf.RoundToInt(item.Price * (1f - specialDiscountPercent / 100f)),
                    Category = ShopCategory.DailySpecials,
                    ItemType = item.ItemType,
                    Rarity = item.Rarity,
                    IsAvailable = true,
                    IsSpecial = true,
                    PlantId = item.PlantId
                };
                dailySpecials.Add(special);
            }

            // Save refresh date
            lastRefreshDate = DateTime.Now;
            PlayerPrefs.SetString("shop_last_refresh", lastRefreshDate.ToString());
            SaveDailySpecials();

            Debug.Log($"[Shop] Daily specials refreshed: {dailySpecials.Count} items");
            OnShopRefreshed?.Invoke();
        }

        private void SaveDailySpecials()
        {
            var ids = string.Join(",", dailySpecials.Select(s => s.Id));
            PlayerPrefs.SetString("shop_daily_specials", ids);
        }

        private void LoadDailySpecials()
        {
            var ids = PlayerPrefs.GetString("shop_daily_specials", "");
            if (string.IsNullOrEmpty(ids)) return;

            var itemIds = ids.Split(',');
            dailySpecials.Clear();

            var allItems = new List<ShopItem>();
            allItems.AddRange(GetIngredientItems());
            allItems.AddRange(GetSeedItems());

            foreach (var id in itemIds)
            {
                var item = allItems.Find(i => i.Id == id);
                if (item != null)
                {
                    item.OriginalPrice = item.Price;
                    item.Price = Mathf.RoundToInt(item.Price * (1f - specialDiscountPercent / 100f));
                    item.Category = ShopCategory.DailySpecials;
                    item.IsSpecial = true;
                    dailySpecials.Add(item);
                }
            }
        }

        /// <summary>
        /// Get time until next shop refresh
        /// </summary>
        public TimeSpan GetTimeUntilRefresh()
        {
            var tomorrow = DateTime.Today.AddDays(1).AddHours(shopRefreshHour);
            return tomorrow - DateTime.Now;
        }
    }

    public enum ShopCategory
    {
        Ingredients,
        Seeds,
        DailySpecials
    }

    public enum ShopItemType
    {
        Ingredient,
        Seed
    }

    [System.Serializable]
    public class ShopItem
    {
        public string Id;
        public string DisplayName;
        public string DisplayNameRu;
        public string Description;
        public Sprite Icon;
        public int Price;
        public int OriginalPrice;
        public ShopCategory Category;
        public ShopItemType ItemType;
        public ItemRarity Rarity;
        public bool IsAvailable;
        public bool IsSpecial;
        public int RequiredLevel;
        public string PlantId; // For seeds
        public int PlayerOwned;
    }

    public class PurchaseResult
    {
        public bool Success;
        public string Message;
        public string ItemId;
        public int Quantity;
        public int TotalCost;
    }

    public class SellResult
    {
        public bool Success;
        public string Message;
        public string ItemId;
        public int Quantity;
        public int TotalValue;
    }
}
