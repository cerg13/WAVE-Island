using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using WaveIsland.Core;
using WaveIsland.Data;

namespace WaveIsland.Spirits
{
    /// <summary>
    /// Manages spirit collection and gacha system
    /// </summary>
    public class SpiritManager : MonoBehaviour
    {
        public static SpiritManager Instance { get; private set; }

        [Header("Spirits Database")]
        [SerializeField] private List<SpiritData> allSpirits;

        [Header("Gacha Configuration")]
        [SerializeField] private int singlePullCost = 100;
        [SerializeField] private int tenPullCost = 900;
        [SerializeField] private float pityRate = 0.01f; // +1% per pull without rare

        [Header("Active Spirit")]
        private OwnedSpirit activeSpirit = null;
        private Dictionary<string, OwnedSpirit> ownedSpirits = new Dictionary<string, OwnedSpirit>();
        private int pityCounter = 0;

        public SpiritData ActiveSpirit => activeSpirit?.Data;
        public List<SpiritData> AllSpirits => allSpirits;

        public event System.Action<SpiritData> OnSpiritObtained;
        public event System.Action<SpiritData> OnSpiritLevelUp;
        public event System.Action<SpiritData> OnActiveSpiritChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            LoadOwnedSpirits();
        }

        private void LoadOwnedSpirits()
        {
            var playerData = GameManager.Instance.PlayerData;

            foreach (var spiritId in playerData.UnlockedSpiritIds)
            {
                var spiritData = GetSpiritById(spiritId);
                if (spiritData != null)
                {
                    ownedSpirits[spiritId] = new OwnedSpirit(spiritData);
                }
            }

            Debug.Log($"[SpiritManager] Loaded {ownedSpirits.Count} spirits");
        }

        /// <summary>
        /// Perform single gacha pull
        /// </summary>
        public SpiritData SinglePull()
        {
            var playerData = GameManager.Instance.PlayerData;

            if (playerData.Gems < singlePullCost)
            {
                Debug.LogWarning("[SpiritManager] Not enough gems");
                return null;
            }

            playerData.Gems -= singlePullCost;
            return PerformPull();
        }

        /// <summary>
        /// Perform 10-pull with discount
        /// </summary>
        public List<SpiritData> TenPull()
        {
            var playerData = GameManager.Instance.PlayerData;

            if (playerData.Gems < tenPullCost)
            {
                Debug.LogWarning("[SpiritManager] Not enough gems");
                return null;
            }

            playerData.Gems -= tenPullCost;

            var results = new List<SpiritData>();
            for (int i = 0; i < 10; i++)
            {
                results.Add(PerformPull());
            }

            return results;
        }

        private SpiritData PerformPull()
        {
            // Calculate rates with pity
            float pityBonus = pityCounter * pityRate;

            float roll = Random.value;
            SpiritRarity targetRarity;

            // Base rates: Common 60%, Uncommon 25%, Rare 10%, Epic 4%, Legendary 1%
            if (roll < 0.01f + pityBonus * 0.5f)
            {
                targetRarity = SpiritRarity.Legendary;
                pityCounter = 0;
            }
            else if (roll < 0.05f + pityBonus)
            {
                targetRarity = SpiritRarity.Epic;
                pityCounter = 0;
            }
            else if (roll < 0.15f + pityBonus * 0.5f)
            {
                targetRarity = SpiritRarity.Rare;
                pityCounter = 0;
            }
            else if (roll < 0.40f)
            {
                targetRarity = SpiritRarity.Uncommon;
                pityCounter++;
            }
            else
            {
                targetRarity = SpiritRarity.Common;
                pityCounter++;
            }

            // Get random spirit of target rarity
            var candidates = allSpirits.Where(s =>
                s.Rarity == targetRarity && !s.IikoExclusive).ToList();

            if (candidates.Count == 0)
            {
                candidates = allSpirits.Where(s => !s.IikoExclusive).ToList();
            }

            var spirit = candidates[Random.Range(0, candidates.Count)];

            // Add or level up
            AddSpirit(spirit);

            return spirit;
        }

        private void AddSpirit(SpiritData spirit)
        {
            var playerData = GameManager.Instance.PlayerData;

            if (ownedSpirits.ContainsKey(spirit.Id))
            {
                // Already owned - level up (duplicate)
                var owned = ownedSpirits[spirit.Id];
                if (owned.Level < spirit.MaxLevel)
                {
                    owned.AddDuplicate();
                    OnSpiritLevelUp?.Invoke(spirit);
                    Debug.Log($"[SpiritManager] {spirit.DisplayName} leveled up to {owned.Level}");
                }
                else
                {
                    // Convert to coins
                    int coinValue = GetDuplicateCoinValue(spirit.Rarity);
                    playerData.Coins += coinValue;
                    Debug.Log($"[SpiritManager] Max level duplicate, +{coinValue} coins");
                }
            }
            else
            {
                // New spirit!
                ownedSpirits[spirit.Id] = new OwnedSpirit(spirit);
                playerData.UnlockedSpiritIds.Add(spirit.Id);
                OnSpiritObtained?.Invoke(spirit);
                Debug.Log($"[SpiritManager] NEW SPIRIT: {spirit.DisplayName}!");
            }
        }

        /// <summary>
        /// Set active companion spirit
        /// </summary>
        public void SetActiveSpirit(string spiritId)
        {
            if (!ownedSpirits.ContainsKey(spiritId))
            {
                Debug.LogWarning($"[SpiritManager] Don't own spirit: {spiritId}");
                return;
            }

            activeSpirit = ownedSpirits[spiritId];
            OnActiveSpiritChanged?.Invoke(activeSpirit.Data);
            Debug.Log($"[SpiritManager] Active spirit: {activeSpirit.Data.DisplayName}");
        }

        /// <summary>
        /// Get ability bonus from active spirit
        /// </summary>
        public float GetAbilityBonus(SpiritAbilityType type)
        {
            if (activeSpirit == null) return 0f;

            var ability = activeSpirit.Data.Abilities.Find(a => a.Type == type);
            if (ability == null) return 0f;

            return ability.BaseValue + ability.ValuePerLevel * (activeSpirit.Level - 1);
        }

        /// <summary>
        /// Unlock spirit from iiko order
        /// </summary>
        public void UnlockFromIiko(string spiritId)
        {
            var spirit = GetSpiritById(spiritId);
            if (spirit != null)
            {
                AddSpirit(spirit);
            }
        }

        public List<OwnedSpirit> GetOwnedSpirits()
        {
            return ownedSpirits.Values.ToList();
        }

        private SpiritData GetSpiritById(string spiritId)
        {
            return allSpirits.Find(s => s.Id == spiritId);
        }

        private int GetDuplicateCoinValue(SpiritRarity rarity)
        {
            switch (rarity)
            {
                case SpiritRarity.Common: return 50;
                case SpiritRarity.Uncommon: return 100;
                case SpiritRarity.Rare: return 250;
                case SpiritRarity.Epic: return 500;
                case SpiritRarity.Legendary: return 1000;
                default: return 50;
            }
        }
    }

    /// <summary>
    /// Runtime data for owned spirit
    /// </summary>
    public class OwnedSpirit
    {
        public SpiritData Data { get; private set; }
        public int Level { get; private set; }
        public int Experience { get; private set; }
        public int Duplicates { get; private set; }

        public OwnedSpirit(SpiritData data)
        {
            Data = data;
            Level = 1;
            Experience = 0;
            Duplicates = 0;
        }

        public void AddDuplicate()
        {
            Duplicates++;

            // Each duplicate = 1 level (simplified)
            if (Level < Data.MaxLevel)
            {
                Level++;
            }
        }
    }
}
