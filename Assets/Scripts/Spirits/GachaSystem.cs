using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using WaveIsland.Core;
using WaveIsland.Data;

namespace WaveIsland.Spirits
{
    /// <summary>
    /// Enhanced gacha system with pity, banners, and guaranteed rates
    /// </summary>
    public class GachaSystem : MonoBehaviour
    {
        public static GachaSystem Instance { get; private set; }

        public event Action<GachaPullResult> OnPullComplete;
        public event Action<List<GachaPullResult>> OnMultiPullComplete;
        public event Action<SpiritJsonData> OnNewSpiritObtained;

        [Header("Pull Costs")]
        [SerializeField] private int singlePullGemCost = 100;
        [SerializeField] private int tenPullGemCost = 900;
        [SerializeField] private int singlePullTicketCost = 1;

        [Header("Pity System")]
        [SerializeField] private int softPityStart = 70;
        [SerializeField] private int hardPity = 90;
        [SerializeField] private float softPityRateIncrease = 0.06f;

        [Header("Base Rates")]
        [SerializeField] private float legendaryRate = 0.01f;
        [SerializeField] private float epicRate = 0.04f;
        [SerializeField] private float rareRate = 0.10f;
        [SerializeField] private float uncommonRate = 0.25f;

        [Header("Guaranteed")]
        [SerializeField] private int guaranteedRareEveryNPulls = 10;

        private int pullsSinceLastRare = 0;
        private int pullsSinceLastEpic = 0;
        private int totalPulls = 0;

        private const string PITY_SAVE_KEY = "gacha_pity";
        private const string PULLS_SAVE_KEY = "gacha_pulls";

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
            LoadPityData();
        }

        /// <summary>
        /// Perform a single pull with gems
        /// </summary>
        public GachaPullResult SinglePull()
        {
            var playerData = GameManager.Instance?.PlayerData;
            if (playerData == null) return null;

            if (playerData.Gems < singlePullGemCost)
            {
                Debug.LogWarning("[Gacha] Not enough gems for single pull");
                return new GachaPullResult { Success = false, Message = "Not enough gems" };
            }

            playerData.Gems -= singlePullGemCost;
            var result = PerformPull();

            SaveSystem.SavePlayerData(playerData);
            OnPullComplete?.Invoke(result);

            return result;
        }

        /// <summary>
        /// Perform a 10-pull with gems (discounted)
        /// </summary>
        public List<GachaPullResult> TenPull()
        {
            var playerData = GameManager.Instance?.PlayerData;
            if (playerData == null) return null;

            if (playerData.Gems < tenPullGemCost)
            {
                Debug.LogWarning("[Gacha] Not enough gems for 10-pull");
                return null;
            }

            playerData.Gems -= tenPullGemCost;

            var results = new List<GachaPullResult>();
            for (int i = 0; i < 10; i++)
            {
                results.Add(PerformPull());
            }

            // Guarantee at least one rare+ in 10-pull
            if (!results.Any(r => IsRareOrHigher(r.Spirit.rarity)))
            {
                // Replace last common with guaranteed rare
                var rareSpirits = SpiritDatabase.GetSpiritsByRarity("Rare");
                var gachaRare = rareSpirits.Where(s => !s.iikoExclusive).ToList();
                if (gachaRare.Count > 0)
                {
                    var guaranteed = gachaRare[UnityEngine.Random.Range(0, gachaRare.Count)];
                    results[9] = CreatePullResult(guaranteed, true);
                }
            }

            SaveSystem.SavePlayerData(playerData);
            SavePityData();

            OnMultiPullComplete?.Invoke(results);

            return results;
        }

        /// <summary>
        /// Perform a single pull with ticket
        /// </summary>
        public GachaPullResult TicketPull(int ticketCount = 1)
        {
            var playerData = GameManager.Instance?.PlayerData;
            if (playerData == null) return null;

            int tickets = playerData.GetItemCount("gacha_ticket");
            if (tickets < ticketCount)
            {
                Debug.LogWarning("[Gacha] Not enough tickets");
                return new GachaPullResult { Success = false, Message = "Not enough tickets" };
            }

            playerData.RemoveItem("gacha_ticket", ticketCount);
            var result = PerformPull();

            SaveSystem.SavePlayerData(playerData);
            OnPullComplete?.Invoke(result);

            return result;
        }

        private GachaPullResult PerformPull()
        {
            totalPulls++;
            pullsSinceLastRare++;
            pullsSinceLastEpic++;

            // Calculate rates with pity
            float currentLegendaryRate = legendaryRate;
            float currentEpicRate = epicRate;

            // Soft pity increases rates
            if (pullsSinceLastEpic >= softPityStart)
            {
                int pityPulls = pullsSinceLastEpic - softPityStart;
                currentLegendaryRate += pityPulls * softPityRateIncrease * 0.5f;
                currentEpicRate += pityPulls * softPityRateIncrease;
            }

            // Hard pity guarantees epic+
            if (pullsSinceLastEpic >= hardPity)
            {
                currentEpicRate = 1f;
            }

            // Guaranteed rare every N pulls
            bool forceRare = pullsSinceLastRare >= guaranteedRareEveryNPulls;

            // Roll
            float roll = UnityEngine.Random.value;
            string targetRarity;

            if (roll < currentLegendaryRate)
            {
                targetRarity = "Legendary";
                ResetPity(true);
            }
            else if (roll < currentLegendaryRate + currentEpicRate)
            {
                targetRarity = "Epic";
                ResetPity(false);
            }
            else if (roll < currentLegendaryRate + currentEpicRate + rareRate || forceRare)
            {
                targetRarity = "Rare";
                pullsSinceLastRare = 0;
            }
            else if (roll < currentLegendaryRate + currentEpicRate + rareRate + uncommonRate)
            {
                targetRarity = "Uncommon";
            }
            else
            {
                targetRarity = "Common";
            }

            // Get random spirit of target rarity
            var spirit = GetRandomSpiritOfRarity(targetRarity);
            return CreatePullResult(spirit, false);
        }

        private SpiritJsonData GetRandomSpiritOfRarity(string rarity)
        {
            var candidates = SpiritDatabase.GetSpiritsByRarity(rarity)
                .Where(s => !s.iikoExclusive)
                .ToList();

            if (candidates.Count == 0)
            {
                // Fallback to any gacha spirit
                candidates = SpiritDatabase.GetGachaSpirits();
            }

            if (candidates.Count == 0)
            {
                Debug.LogError("[Gacha] No spirits available!");
                return null;
            }

            return candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }

        private GachaPullResult CreatePullResult(SpiritJsonData spirit, bool guaranteed)
        {
            var playerData = GameManager.Instance?.PlayerData;
            bool isNew = !playerData.UnlockedSpiritIds.Contains(spirit.id);

            // Add spirit to player
            if (isNew)
            {
                playerData.UnlockedSpiritIds.Add(spirit.id);
                OnNewSpiritObtained?.Invoke(spirit);
                Debug.Log($"[Gacha] NEW SPIRIT: {spirit.displayName} ({spirit.rarity})!");
            }
            else
            {
                // Duplicate - convert to currency
                int duplicateValue = GetDuplicateValue(spirit.rarity);
                playerData.Coins += duplicateValue;
                Debug.Log($"[Gacha] Duplicate {spirit.displayName}, +{duplicateValue} coins");
            }

            return new GachaPullResult
            {
                Success = true,
                Spirit = spirit,
                IsNew = isNew,
                IsGuaranteed = guaranteed,
                Rarity = SpiritDatabase.ParseRarity(spirit.rarity)
            };
        }

        private void ResetPity(bool legendary)
        {
            pullsSinceLastEpic = 0;
            pullsSinceLastRare = 0;

            if (legendary)
            {
                Debug.Log("[Gacha] LEGENDARY! Pity reset");
            }
            else
            {
                Debug.Log("[Gacha] Epic! Pity reset");
            }

            SavePityData();
        }

        private int GetDuplicateValue(string rarity)
        {
            switch (rarity.ToLower())
            {
                case "common": return 25;
                case "uncommon": return 50;
                case "rare": return 100;
                case "epic": return 250;
                case "legendary": return 500;
                default: return 25;
            }
        }

        private bool IsRareOrHigher(string rarity)
        {
            var r = rarity.ToLower();
            return r == "rare" || r == "epic" || r == "legendary";
        }

        /// <summary>
        /// Get current pity counter
        /// </summary>
        public int GetPityCounter() => pullsSinceLastEpic;

        /// <summary>
        /// Get pulls until hard pity
        /// </summary>
        public int GetPullsUntilPity() => Mathf.Max(0, hardPity - pullsSinceLastEpic);

        /// <summary>
        /// Get total pulls made
        /// </summary>
        public int GetTotalPulls() => totalPulls;

        /// <summary>
        /// Get pull costs
        /// </summary>
        public int GetSinglePullCost() => singlePullGemCost;
        public int GetTenPullCost() => tenPullGemCost;

        /// <summary>
        /// Check if player can afford single pull
        /// </summary>
        public bool CanAffordSinglePull()
        {
            var playerData = GameManager.Instance?.PlayerData;
            return playerData != null && playerData.Gems >= singlePullGemCost;
        }

        /// <summary>
        /// Check if player can afford 10-pull
        /// </summary>
        public bool CanAffordTenPull()
        {
            var playerData = GameManager.Instance?.PlayerData;
            return playerData != null && playerData.Gems >= tenPullGemCost;
        }

        private void SavePityData()
        {
            PlayerPrefs.SetInt($"{PITY_SAVE_KEY}_rare", pullsSinceLastRare);
            PlayerPrefs.SetInt($"{PITY_SAVE_KEY}_epic", pullsSinceLastEpic);
            PlayerPrefs.SetInt(PULLS_SAVE_KEY, totalPulls);
            PlayerPrefs.Save();
        }

        private void LoadPityData()
        {
            pullsSinceLastRare = PlayerPrefs.GetInt($"{PITY_SAVE_KEY}_rare", 0);
            pullsSinceLastEpic = PlayerPrefs.GetInt($"{PITY_SAVE_KEY}_epic", 0);
            totalPulls = PlayerPrefs.GetInt(PULLS_SAVE_KEY, 0);
        }

        /// <summary>
        /// Reset pity (for testing)
        /// </summary>
        public void ResetAllPity()
        {
            pullsSinceLastRare = 0;
            pullsSinceLastEpic = 0;
            totalPulls = 0;
            SavePityData();
        }
    }

    /// <summary>
    /// Result of a gacha pull
    /// </summary>
    public class GachaPullResult
    {
        public bool Success;
        public string Message;
        public SpiritJsonData Spirit;
        public bool IsNew;
        public bool IsGuaranteed;
        public SpiritRarity Rarity;
    }
}
