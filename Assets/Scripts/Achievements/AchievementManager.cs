using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WaveIsland.Achievements
{
    /// <summary>
    /// Manages achievement tracking, unlocking, and rewards
    /// </summary>
    public class AchievementManager : MonoBehaviour
    {
        public static AchievementManager Instance { get; private set; }

        [Header("Events")]
        public event Action<AchievementData> OnAchievementUnlocked;
        public event Action<AchievementData, int, int> OnAchievementProgress; // achievement, current, target
        public event Action OnAchievementsLoaded;

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;

        // Achievement data
        private List<AchievementData> allAchievements = new List<AchievementData>();
        private List<AchievementCategory> categories = new List<AchievementCategory>();
        private Dictionary<string, AchievementData> achievementById = new Dictionary<string, AchievementData>();

        // Player progress
        private HashSet<string> unlockedAchievements = new HashSet<string>();
        private Dictionary<string, int> achievementProgress = new Dictionary<string, int>();
        private Dictionary<string, int> playerStats = new Dictionary<string, int>();

        // Session stats (reset each session)
        private Dictionary<string, int> sessionStats = new Dictionary<string, int>();
        private DateTime sessionStartTime;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadAchievementData();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            sessionStartTime = DateTime.Now;
            LoadProgress();
        }

        #region Data Loading

        private void LoadAchievementData()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("GameData/achievements_data");
            if (jsonFile == null)
            {
                Debug.LogError("AchievementManager: achievements_data.json not found!");
                return;
            }

            try
            {
                AchievementsWrapper wrapper = JsonUtility.FromJson<AchievementsWrapper>(jsonFile.text);

                allAchievements = wrapper.achievements;
                categories = wrapper.categories;

                foreach (var achievement in allAchievements)
                {
                    achievementById[achievement.id] = achievement;
                }

                Debug.Log($"AchievementManager: Loaded {allAchievements.Count} achievements in {categories.Count} categories");
                OnAchievementsLoaded?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"AchievementManager: Failed to parse achievements data: {e.Message}");
            }
        }

        #endregion

        #region Stat Tracking

        /// <summary>
        /// Increment a tracked stat and check for achievement unlocks
        /// </summary>
        public void IncrementStat(string statName, int amount = 1)
        {
            if (!playerStats.ContainsKey(statName))
                playerStats[statName] = 0;

            playerStats[statName] += amount;

            // Also track session stats
            if (!sessionStats.ContainsKey(statName))
                sessionStats[statName] = 0;
            sessionStats[statName] += amount;

            if (debugMode)
                Debug.Log($"AchievementManager: Stat '{statName}' = {playerStats[statName]}");

            CheckAchievementsForStat(statName);
            SaveProgress();
        }

        /// <summary>
        /// Set a stat to a specific value
        /// </summary>
        public void SetStat(string statName, int value)
        {
            playerStats[statName] = value;
            CheckAchievementsForStat(statName);
            SaveProgress();
        }

        /// <summary>
        /// Get current value of a stat
        /// </summary>
        public int GetStat(string statName)
        {
            return playerStats.TryGetValue(statName, out int value) ? value : 0;
        }

        /// <summary>
        /// Get session-specific stat
        /// </summary>
        public int GetSessionStat(string statName)
        {
            return sessionStats.TryGetValue(statName, out int value) ? value : 0;
        }

        #endregion

        #region Achievement Checking

        private void CheckAchievementsForStat(string statName)
        {
            foreach (var achievement in allAchievements)
            {
                if (IsUnlocked(achievement.id)) continue;
                if (achievement.trackingStat != statName) continue;

                CheckAchievement(achievement);
            }
        }

        private void CheckAchievement(AchievementData achievement)
        {
            if (IsUnlocked(achievement.id)) return;

            bool shouldUnlock = false;
            int currentProgress = 0;

            switch (achievement.type)
            {
                case "Counter":
                    currentProgress = GetStat(achievement.trackingStat);
                    shouldUnlock = currentProgress >= achievement.targetValue;
                    break;

                case "OneTime":
                    currentProgress = GetStat(achievement.trackingStat);
                    shouldUnlock = currentProgress >= 1;
                    break;

                case "RecipeSpecific":
                    currentProgress = GetRecipeSpecificProgress(achievement);
                    shouldUnlock = currentProgress >= achievement.targetValue;
                    break;

                case "CategorySpecific":
                    currentProgress = GetCategorySpecificProgress(achievement);
                    shouldUnlock = currentProgress >= achievement.targetValue;
                    break;

                case "Session":
                    currentProgress = GetSessionStat(achievement.trackingStat);
                    shouldUnlock = currentProgress >= achievement.targetValue;
                    break;
            }

            // Update progress tracking
            if (!achievementProgress.ContainsKey(achievement.id) ||
                achievementProgress[achievement.id] != currentProgress)
            {
                achievementProgress[achievement.id] = currentProgress;
                OnAchievementProgress?.Invoke(achievement, currentProgress, achievement.targetValue);
            }

            if (shouldUnlock)
            {
                UnlockAchievement(achievement);
            }
        }

        private int GetRecipeSpecificProgress(AchievementData achievement)
        {
            if (string.IsNullOrEmpty(achievement.recipeId)) return 0;
            return GetStat($"Recipe_{achievement.recipeId}_Crafted");
        }

        private int GetCategorySpecificProgress(AchievementData achievement)
        {
            if (string.IsNullOrEmpty(achievement.category)) return 0;

            // Count unique recipes discovered in category
            int count = 0;
            foreach (var kvp in playerStats)
            {
                if (kvp.Key.StartsWith($"Recipe_{achievement.category}_") && kvp.Value > 0)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Check all achievements (call periodically or after major events)
        /// </summary>
        public void CheckAllAchievements()
        {
            foreach (var achievement in allAchievements)
            {
                if (!IsUnlocked(achievement.id))
                {
                    CheckAchievement(achievement);
                }
            }
        }

        #endregion

        #region Achievement Unlocking

        private void UnlockAchievement(AchievementData achievement)
        {
            if (IsUnlocked(achievement.id)) return;

            unlockedAchievements.Add(achievement.id);

            // Grant rewards
            GrantRewards(achievement.rewards);

            Debug.Log($"Achievement Unlocked: {achievement.name}!");
            OnAchievementUnlocked?.Invoke(achievement);

            SaveProgress();
        }

        /// <summary>
        /// Force unlock an achievement (for special cases like iiko milestones)
        /// </summary>
        public void ForceUnlock(string achievementId)
        {
            if (achievementById.TryGetValue(achievementId, out AchievementData achievement))
            {
                UnlockAchievement(achievement);
            }
        }

        private void GrantRewards(AchievementRewards rewards)
        {
            if (rewards == null) return;

            var playerData = Core.PlayerData.Instance;
            if (playerData == null) return;

            if (rewards.coins > 0)
            {
                playerData.AddCoins(rewards.coins);
            }

            if (rewards.gems > 0)
            {
                playerData.AddGems(rewards.gems);
            }

            if (rewards.exp > 0)
            {
                playerData.AddExperience(rewards.exp);
            }

            // Spirit rewards would be handled by SpiritManager
            if (!string.IsNullOrEmpty(rewards.spiritId))
            {
                // SpiritManager.Instance?.AddSpirit(rewards.spiritId);
                Debug.Log($"AchievementManager: Spirit reward '{rewards.spiritId}' - implement in SpiritManager");
            }

            // Recipe unlock
            if (!string.IsNullOrEmpty(rewards.recipeUnlock))
            {
                IncrementStat($"RecipeUnlocked_{rewards.recipeUnlock}");
            }
        }

        #endregion

        #region Queries

        public bool IsUnlocked(string achievementId)
        {
            return unlockedAchievements.Contains(achievementId);
        }

        public int GetProgress(string achievementId)
        {
            return achievementProgress.TryGetValue(achievementId, out int progress) ? progress : 0;
        }

        public float GetProgressPercent(string achievementId)
        {
            if (!achievementById.TryGetValue(achievementId, out AchievementData achievement))
                return 0f;

            int progress = GetProgress(achievementId);
            return achievement.targetValue > 0 ? (float)progress / achievement.targetValue : 0f;
        }

        public List<AchievementData> GetAllAchievements()
        {
            return new List<AchievementData>(allAchievements);
        }

        public List<AchievementData> GetAchievementsByCategory(string categoryId)
        {
            return allAchievements.Where(a => a.category == categoryId).ToList();
        }

        public List<AchievementData> GetUnlockedAchievements()
        {
            return allAchievements.Where(a => IsUnlocked(a.id)).ToList();
        }

        public List<AchievementData> GetLockedAchievements()
        {
            return allAchievements.Where(a => !IsUnlocked(a.id)).ToList();
        }

        public List<AchievementCategory> GetCategories()
        {
            return new List<AchievementCategory>(categories);
        }

        public AchievementData GetAchievement(string achievementId)
        {
            return achievementById.TryGetValue(achievementId, out AchievementData achievement) ? achievement : null;
        }

        public int GetUnlockedCount()
        {
            return unlockedAchievements.Count;
        }

        public int GetTotalCount()
        {
            return allAchievements.Count;
        }

        public int GetUnlockedCountByCategory(string categoryId)
        {
            return allAchievements.Count(a => a.category == categoryId && IsUnlocked(a.id));
        }

        public int GetTotalCountByCategory(string categoryId)
        {
            return allAchievements.Count(a => a.category == categoryId);
        }

        /// <summary>
        /// Get total achievement points earned
        /// </summary>
        public int GetTotalAchievementPoints()
        {
            int points = 0;
            foreach (var achievement in GetUnlockedAchievements())
            {
                points += achievement.achievementPoints;
            }
            return points;
        }

        /// <summary>
        /// Get recently unlocked achievements (for display)
        /// </summary>
        public List<AchievementData> GetRecentlyUnlocked(int count = 5)
        {
            // In a full implementation, we'd track unlock timestamps
            return GetUnlockedAchievements().TakeLast(count).ToList();
        }

        #endregion

        #region Convenience Methods for Common Stats

        // Garden stats
        public void OnPlantHarvested(string plantId)
        {
            IncrementStat("TotalHarvests");
            IncrementStat($"Plant_{plantId}_Harvested");
        }

        public void OnPlantPlanted(string plantId)
        {
            IncrementStat("TotalPlanted");
            IncrementStat($"Plant_{plantId}_Planted");
        }

        public void OnPlantWithered()
        {
            IncrementStat("PlantsWithered");
        }

        // Crafting stats
        public void OnRecipeCrafted(string recipeId, string category)
        {
            IncrementStat("TotalCrafts");
            IncrementStat($"Recipe_{recipeId}_Crafted");
            IncrementStat($"Category_{category}_Crafted");
        }

        public void OnRecipeDiscovered(string recipeId, string category)
        {
            IncrementStat("RecipesDiscovered");
            IncrementStat($"Recipe_{category}_{recipeId}_Discovered");
        }

        public void OnCraftFailed()
        {
            IncrementStat("FailedCrafts");
        }

        public void OnPerfectCraft()
        {
            IncrementStat("PerfectCrafts");
        }

        // Spirit stats
        public void OnSpiritSummoned(string spiritId, string rarity)
        {
            IncrementStat("TotalSummons");
            IncrementStat($"Spirit_{spiritId}_Summoned");
            IncrementStat($"Rarity_{rarity}_Summoned");

            // Track unique spirits
            if (GetStat($"Spirit_{spiritId}_Summoned") == 1)
            {
                IncrementStat("UniqueSpirits");
            }
        }

        public void OnSpiritLevelUp(string spiritId, int newLevel)
        {
            IncrementStat("SpiritLevelUps");
            SetStat($"Spirit_{spiritId}_Level", newLevel);

            if (newLevel >= 10)
            {
                IncrementStat("SpiritsMaxLevel");
            }
        }

        // Economy stats
        public void OnCoinsEarned(int amount)
        {
            IncrementStat("TotalCoinsEarned", amount);
        }

        public void OnCoinsSpent(int amount)
        {
            IncrementStat("TotalCoinsSpent", amount);
        }

        public void OnGemsSpent(int amount)
        {
            IncrementStat("TotalGemsSpent", amount);
        }

        public void OnShopPurchase()
        {
            IncrementStat("ShopPurchases");
        }

        // iiko stats
        public void OnIikoOrderCompleted()
        {
            IncrementStat("IikoOrders");
        }

        public void OnIikoMilestoneClaimed(string milestoneId)
        {
            IncrementStat($"IikoMilestone_{milestoneId}");
            IncrementStat("IikoMilestonesClaimed");
        }

        // Daily stats
        public void OnDailyLogin(int streak)
        {
            IncrementStat("TotalLogins");
            SetStat("LoginStreak", streak);
        }

        public void OnDailyTaskCompleted()
        {
            IncrementStat("DailyTasksCompleted");
        }

        // Progress stats
        public void OnLevelUp(int newLevel)
        {
            SetStat("PlayerLevel", newLevel);
        }

        public void OnGardenExpanded(int totalPlots)
        {
            SetStat("GardenPlots", totalPlots);
        }

        #endregion

        #region Save/Load

        private const string SAVE_KEY = "AchievementProgress";

        private void SaveProgress()
        {
            AchievementSaveData saveData = new AchievementSaveData
            {
                unlockedIds = unlockedAchievements.ToList(),
                stats = playerStats.Select(kvp => new StatEntry { key = kvp.Key, value = kvp.Value }).ToList(),
                progress = achievementProgress.Select(kvp => new StatEntry { key = kvp.Key, value = kvp.Value }).ToList()
            };

            string json = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        private void LoadProgress()
        {
            if (!PlayerPrefs.HasKey(SAVE_KEY)) return;

            try
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                AchievementSaveData saveData = JsonUtility.FromJson<AchievementSaveData>(json);

                unlockedAchievements = new HashSet<string>(saveData.unlockedIds);

                playerStats.Clear();
                foreach (var entry in saveData.stats)
                {
                    playerStats[entry.key] = entry.value;
                }

                achievementProgress.Clear();
                foreach (var entry in saveData.progress)
                {
                    achievementProgress[entry.key] = entry.value;
                }

                Debug.Log($"AchievementManager: Loaded {unlockedAchievements.Count} unlocked achievements");
            }
            catch (Exception e)
            {
                Debug.LogError($"AchievementManager: Failed to load progress: {e.Message}");
            }
        }

        /// <summary>
        /// Reset all achievement progress (for testing)
        /// </summary>
        [ContextMenu("Reset All Progress")]
        public void ResetAllProgress()
        {
            unlockedAchievements.Clear();
            playerStats.Clear();
            achievementProgress.Clear();
            sessionStats.Clear();
            PlayerPrefs.DeleteKey(SAVE_KEY);
            Debug.Log("AchievementManager: All progress reset");
        }

        #endregion
    }

    #region Data Classes

    [Serializable]
    public class AchievementsWrapper
    {
        public List<AchievementData> achievements;
        public List<AchievementCategory> categories;
    }

    [Serializable]
    public class AchievementData
    {
        public string id;
        public string name;
        public string nameRu;
        public string description;
        public string descriptionRu;
        public string category;
        public string type;
        public int targetValue;
        public string trackingStat;
        public string recipeId;
        public string icon;
        public int tier;
        public bool isHidden;
        public bool isSecret;
        public int achievementPoints;
        public AchievementRewards rewards;
    }

    [Serializable]
    public class AchievementRewards
    {
        public int coins;
        public int gems;
        public int exp;
        public string spiritId;
        public string recipeUnlock;
    }

    [Serializable]
    public class AchievementCategory
    {
        public string id;
        public string name;
        public string nameRu;
        public string icon;
        public int order;
    }

    [Serializable]
    public class AchievementSaveData
    {
        public List<string> unlockedIds;
        public List<StatEntry> stats;
        public List<StatEntry> progress;
    }

    [Serializable]
    public class StatEntry
    {
        public string key;
        public int value;
    }

    #endregion
}
