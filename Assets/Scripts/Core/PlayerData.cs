using System;
using System.Collections.Generic;
using UnityEngine;

namespace WaveIsland.Core
{
    /// <summary>
    /// Player data container - serializable for save/load
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        [Header("Basic Info")]
        public string PlayerId;
        public string PlayerName;
        public int Level;
        public int Experience;
        public int Coins;
        public int Gems;

        [Header("Timestamps")]
        public long CreatedAt;
        public long LastLoginAt;
        public long LastSaveAt;

        [Header("Progress")]
        public int GardenLevel;
        public int TotalRecipesDiscovered;
        public int TotalCrafts;
        public int TotalHarvests;

        [Header("Inventory")]
        public List<InventoryItem> Inventory;
        public List<string> UnlockedRecipeIds;
        public List<string> UnlockedSpiritIds;

        [Header("Garden")]
        public List<PlotData> GardenPlots;

        [Header("iiko Integration")]
        public string IikoUserId;
        public int TotalOrdersInWave;
        public long LastOrderTimestamp;

        public PlayerData()
        {
            PlayerId = Guid.NewGuid().ToString();
            PlayerName = "Island Explorer";
            Level = 1;
            Experience = 0;
            Coins = 500; // Starting coins
            Gems = 10; // Starting gems

            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            LastLoginAt = CreatedAt;
            LastSaveAt = CreatedAt;

            GardenLevel = 1;
            TotalRecipesDiscovered = 0;
            TotalCrafts = 0;
            TotalHarvests = 0;

            Inventory = new List<InventoryItem>();
            UnlockedRecipeIds = new List<string>();
            UnlockedSpiritIds = new List<string>();
            GardenPlots = new List<PlotData>();

            // Initialize starting garden plots
            for (int i = 0; i < 4; i++)
            {
                GardenPlots.Add(new PlotData { PlotIndex = i, IsUnlocked = true });
            }

            // Add starter ingredients
            AddStarterItems();
        }

        private void AddStarterItems()
        {
            // Give player some starting seeds
            Inventory.Add(new InventoryItem { ItemId = "seed_mint", Quantity = 5 });
            Inventory.Add(new InventoryItem { ItemId = "seed_lime", Quantity = 3 });
            Inventory.Add(new InventoryItem { ItemId = "seed_basil", Quantity = 3 });
        }

        public int GetItemCount(string itemId)
        {
            var item = Inventory.Find(i => i.ItemId == itemId);
            return item?.Quantity ?? 0;
        }

        public bool HasItem(string itemId, int quantity = 1)
        {
            return GetItemCount(itemId) >= quantity;
        }

        public bool RemoveItem(string itemId, int quantity = 1)
        {
            var item = Inventory.Find(i => i.ItemId == itemId);
            if (item == null || item.Quantity < quantity)
                return false;

            item.Quantity -= quantity;
            if (item.Quantity <= 0)
                Inventory.Remove(item);

            return true;
        }

        public void AddItem(string itemId, int quantity = 1)
        {
            var item = Inventory.Find(i => i.ItemId == itemId);
            if (item != null)
            {
                item.Quantity += quantity;
            }
            else
            {
                Inventory.Add(new InventoryItem { ItemId = itemId, Quantity = quantity });
            }
        }

        // Ingredient-specific methods
        public int GetIngredientCount(string ingredientId)
        {
            return GetItemCount($"ingredient_{ingredientId}");
        }

        public void AddIngredient(string ingredientId, int quantity = 1)
        {
            AddItem($"ingredient_{ingredientId}", quantity);
        }

        public bool RemoveIngredient(string ingredientId, int quantity = 1)
        {
            return RemoveItem($"ingredient_{ingredientId}", quantity);
        }

        public bool HasIngredient(string ingredientId, int quantity = 1)
        {
            return HasItem($"ingredient_{ingredientId}", quantity);
        }

        // Seed-specific methods
        public int GetSeedCount(string plantId)
        {
            return GetItemCount($"seed_{plantId}");
        }

        public void AddSeeds(string plantId, int quantity = 1)
        {
            AddItem($"seed_{plantId}", quantity);
        }

        public bool RemoveSeeds(string plantId, int quantity = 1)
        {
            return RemoveItem($"seed_{plantId}", quantity);
        }

        public bool HasSeeds(string plantId, int quantity = 1)
        {
            return HasItem($"seed_{plantId}", quantity);
        }
    }

    [Serializable]
    public class InventoryItem
    {
        public string ItemId;
        public int Quantity;
    }

    [Serializable]
    public class PlotData
    {
        public int PlotIndex;
        public bool IsUnlocked;
        public string PlantId;
        public long PlantedAt;
        public long LastWateredAt;
        public PlotState State;
    }

    public enum PlotState
    {
        Empty,
        Planted,
        Growing,
        ReadyToHarvest,
        Withering,
        Dead
    }
}
