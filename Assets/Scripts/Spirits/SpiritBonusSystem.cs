using UnityEngine;
using System;
using System.Collections.Generic;
using WaveIsland.Core;
using WaveIsland.Data;

namespace WaveIsland.Spirits
{
    /// <summary>
    /// System that calculates and applies spirit bonuses to gameplay
    /// </summary>
    public class SpiritBonusSystem : MonoBehaviour
    {
        public static SpiritBonusSystem Instance { get; private set; }

        public event Action OnBonusesChanged;

        // Cached bonus values
        private Dictionary<SpiritAbilityType, float> activeBonus = new Dictionary<SpiritAbilityType, float>();
        private Dictionary<SpiritAbilityType, float> collectionBonus = new Dictionary<SpiritAbilityType, float>();

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
            SubscribeToEvents();
            RecalculateBonuses();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            if (SpiritManager.Instance != null)
            {
                SpiritManager.Instance.OnActiveSpiritChanged += OnActiveSpiritChanged;
                SpiritManager.Instance.OnSpiritObtained += OnSpiritObtained;
                SpiritManager.Instance.OnSpiritLevelUp += OnSpiritLevelUp;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (SpiritManager.Instance != null)
            {
                SpiritManager.Instance.OnActiveSpiritChanged -= OnActiveSpiritChanged;
                SpiritManager.Instance.OnSpiritObtained -= OnSpiritObtained;
                SpiritManager.Instance.OnSpiritLevelUp -= OnSpiritLevelUp;
            }
        }

        private void OnActiveSpiritChanged(SpiritData spirit)
        {
            RecalculateBonuses();
        }

        private void OnSpiritObtained(SpiritData spirit)
        {
            RecalculateBonuses();
        }

        private void OnSpiritLevelUp(SpiritData spirit)
        {
            RecalculateBonuses();
        }

        /// <summary>
        /// Recalculate all bonuses
        /// </summary>
        public void RecalculateBonuses()
        {
            activeBonus.Clear();
            collectionBonus.Clear();

            // Calculate active spirit bonus
            CalculateActiveSpiritBonus();

            // Calculate collection bonus
            CalculateCollectionBonus();

            OnBonusesChanged?.Invoke();
            Debug.Log("[SpiritBonusSystem] Bonuses recalculated");
        }

        private void CalculateActiveSpiritBonus()
        {
            if (SpiritManager.Instance == null) return;

            var activeSpirit = SpiritManager.Instance.ActiveSpirit;
            if (activeSpirit == null) return;

            // Get owned spirit data for level
            var ownedSpirits = SpiritManager.Instance.GetOwnedSpirits();
            var owned = ownedSpirits.Find(o => o.Data.Id == activeSpirit.Id);
            int level = owned?.Level ?? 1;

            foreach (var ability in activeSpirit.Abilities)
            {
                float bonus = ability.BaseValue + ability.ValuePerLevel * (level - 1);

                if (activeBonus.ContainsKey(ability.Type))
                {
                    activeBonus[ability.Type] += bonus;
                }
                else
                {
                    activeBonus[ability.Type] = bonus;
                }
            }
        }

        private void CalculateCollectionBonus()
        {
            var playerData = GameManager.Instance?.PlayerData;
            if (playerData == null) return;

            // Small bonus for each owned spirit (collection reward)
            int commonCount = 0, uncommonCount = 0, rareCount = 0, epicCount = 0, legendaryCount = 0;

            foreach (var spiritId in playerData.UnlockedSpiritIds)
            {
                var spirit = SpiritDatabase.GetSpirit(spiritId);
                if (spirit == null) continue;

                switch (spirit.rarity.ToLower())
                {
                    case "common": commonCount++; break;
                    case "uncommon": uncommonCount++; break;
                    case "rare": rareCount++; break;
                    case "epic": epicCount++; break;
                    case "legendary": legendaryCount++; break;
                }
            }

            // Collection bonuses (small global bonus for owning spirits)
            float coinBonus = commonCount * 0.5f + uncommonCount * 1f + rareCount * 2f + epicCount * 3f + legendaryCount * 5f;
            float expBonus = commonCount * 0.25f + uncommonCount * 0.5f + rareCount * 1f + epicCount * 2f + legendaryCount * 3f;

            if (coinBonus > 0)
                collectionBonus[SpiritAbilityType.CoinBonus] = coinBonus;

            if (expBonus > 0)
                collectionBonus[SpiritAbilityType.ExpBonus] = expBonus;

            // Set completion bonuses
            int totalOwned = playerData.UnlockedSpiritIds.Count;
            int totalSpirits = SpiritDatabase.GetTotalSpiritCount();

            if (totalOwned >= totalSpirits)
            {
                // 100% completion bonus!
                collectionBonus[SpiritAbilityType.LuckBonus] = 25f;
                Debug.Log("[SpiritBonusSystem] 100% collection complete! +25% luck bonus");
            }
            else if (totalOwned >= totalSpirits * 0.75f)
            {
                collectionBonus[SpiritAbilityType.LuckBonus] = 10f;
            }
            else if (totalOwned >= totalSpirits * 0.5f)
            {
                collectionBonus[SpiritAbilityType.LuckBonus] = 5f;
            }
        }

        /// <summary>
        /// Get total bonus for ability type (active + collection)
        /// </summary>
        public float GetTotalBonus(SpiritAbilityType type)
        {
            float total = 0;

            if (activeBonus.TryGetValue(type, out float active))
                total += active;

            if (collectionBonus.TryGetValue(type, out float collection))
                total += collection;

            return total;
        }

        /// <summary>
        /// Get only active spirit bonus
        /// </summary>
        public float GetActiveBonus(SpiritAbilityType type)
        {
            return activeBonus.TryGetValue(type, out float value) ? value : 0;
        }

        /// <summary>
        /// Get only collection bonus
        /// </summary>
        public float GetCollectionBonus(SpiritAbilityType type)
        {
            return collectionBonus.TryGetValue(type, out float value) ? value : 0;
        }

        // Convenience methods for common bonuses

        /// <summary>
        /// Get growth speed multiplier (1.0 = no bonus)
        /// </summary>
        public float GetGrowthSpeedMultiplier()
        {
            return 1f + GetTotalBonus(SpiritAbilityType.GrowthSpeedBonus) / 100f;
        }

        /// <summary>
        /// Get crafting speed multiplier
        /// </summary>
        public float GetCraftSpeedMultiplier()
        {
            return 1f + GetTotalBonus(SpiritAbilityType.CraftSpeedBonus) / 100f;
        }

        /// <summary>
        /// Get harvest bonus (additional items)
        /// </summary>
        public int GetHarvestBonus()
        {
            return Mathf.FloorToInt(GetTotalBonus(SpiritAbilityType.HarvestBonus));
        }

        /// <summary>
        /// Get coin bonus multiplier
        /// </summary>
        public float GetCoinMultiplier()
        {
            return 1f + GetTotalBonus(SpiritAbilityType.CoinBonus) / 100f;
        }

        /// <summary>
        /// Get experience bonus multiplier
        /// </summary>
        public float GetExpMultiplier()
        {
            return 1f + GetTotalBonus(SpiritAbilityType.ExpBonus) / 100f;
        }

        /// <summary>
        /// Get shop discount percentage
        /// </summary>
        public float GetShopDiscount()
        {
            return GetTotalBonus(SpiritAbilityType.ShopDiscount);
        }

        /// <summary>
        /// Get sell bonus percentage
        /// </summary>
        public float GetSellBonus()
        {
            return GetTotalBonus(SpiritAbilityType.SellBonus);
        }

        /// <summary>
        /// Check if ingredient should be saved
        /// </summary>
        public bool ShouldSaveIngredient()
        {
            float chance = GetTotalBonus(SpiritAbilityType.IngredientSaver);
            return UnityEngine.Random.value * 100f < chance;
        }

        /// <summary>
        /// Check if auto-water triggers
        /// </summary>
        public bool ShouldAutoWater()
        {
            float chance = GetTotalBonus(SpiritAbilityType.AutoWater);
            return UnityEngine.Random.value * 100f < chance;
        }

        /// <summary>
        /// Get rare seed chance bonus
        /// </summary>
        public float GetRareSeedChance()
        {
            return GetTotalBonus(SpiritAbilityType.RareSeedChance);
        }

        /// <summary>
        /// Get free hint count
        /// </summary>
        public int GetFreeHints()
        {
            return Mathf.FloorToInt(GetTotalBonus(SpiritAbilityType.RecipeHint));
        }

        /// <summary>
        /// Get quality bonus (extra coins)
        /// </summary>
        public int GetQualityBonus()
        {
            return Mathf.FloorToInt(GetTotalBonus(SpiritAbilityType.QualityBonus));
        }

        /// <summary>
        /// Get luck bonus for gacha
        /// </summary>
        public float GetLuckBonus()
        {
            return GetTotalBonus(SpiritAbilityType.LuckBonus);
        }

        /// <summary>
        /// Get iiko reward bonus
        /// </summary>
        public float GetIikoBonus()
        {
            return GetTotalBonus(SpiritAbilityType.IikoBonus);
        }

        /// <summary>
        /// Get daily bonus multiplier
        /// </summary>
        public float GetDailyBonusMultiplier()
        {
            return 1f + GetTotalBonus(SpiritAbilityType.DailyBonus) / 100f;
        }

        /// <summary>
        /// Get summary of all active bonuses
        /// </summary>
        public List<BonusSummary> GetBonusSummary()
        {
            var summary = new List<BonusSummary>();

            foreach (SpiritAbilityType type in Enum.GetValues(typeof(SpiritAbilityType)))
            {
                float total = GetTotalBonus(type);
                if (total > 0)
                {
                    summary.Add(new BonusSummary
                    {
                        Type = type,
                        ActiveValue = GetActiveBonus(type),
                        CollectionValue = GetCollectionBonus(type),
                        TotalValue = total
                    });
                }
            }

            return summary;
        }
    }

    [Serializable]
    public class BonusSummary
    {
        public SpiritAbilityType Type;
        public float ActiveValue;
        public float CollectionValue;
        public float TotalValue;
    }
}
