using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WaveIsland.Quests
{
    /// <summary>
    /// Manages daily quests - generates, tracks, and rewards
    /// </summary>
    public class DailyQuestManager : MonoBehaviour
    {
        public static DailyQuestManager Instance { get; private set; }

        [Header("Events")]
        public event Action<DailyQuest> OnQuestProgress;
        public event Action<DailyQuest> OnQuestCompleted;
        public event Action OnQuestsRefreshed;

        [Header("Settings")]
        [SerializeField] private int dailyQuestCount = 5;
        [SerializeField] private int refreshHour = 0; // Midnight

        // Current quests
        private List<DailyQuest> activeQuests = new List<DailyQuest>();
        private DateTime lastRefreshDate;
        private int completedTodayCount = 0;

        // Quest templates
        private List<QuestTemplate> questTemplates = new List<QuestTemplate>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeQuestTemplates();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            LoadProgress();
            CheckDailyRefresh();
        }

        #region Quest Templates

        private void InitializeQuestTemplates()
        {
            questTemplates = new List<QuestTemplate>
            {
                // Garden quests
                new QuestTemplate
                {
                    id = "harvest_plants",
                    type = QuestType.Harvest,
                    nameRu = "Собери {0} растений",
                    descriptionRu = "Соберите урожай с {0} растений",
                    minTarget = 3, maxTarget = 10,
                    baseCoins = 50, baseExp = 25,
                    difficulty = QuestDifficulty.Easy
                },
                new QuestTemplate
                {
                    id = "plant_seeds",
                    type = QuestType.Plant,
                    nameRu = "Посади {0} семян",
                    descriptionRu = "Посадите {0} новых растений",
                    minTarget = 3, maxTarget = 8,
                    baseCoins = 40, baseExp = 20,
                    difficulty = QuestDifficulty.Easy
                },
                new QuestTemplate
                {
                    id = "perfect_harvest",
                    type = QuestType.PerfectHarvest,
                    nameRu = "Идеальный урожай ({0})",
                    descriptionRu = "Соберите {0} растений без увядания",
                    minTarget = 2, maxTarget = 5,
                    baseCoins = 80, baseExp = 40,
                    difficulty = QuestDifficulty.Medium
                },

                // Crafting quests
                new QuestTemplate
                {
                    id = "craft_recipes",
                    type = QuestType.Craft,
                    nameRu = "Приготовь {0} рецептов",
                    descriptionRu = "Приготовьте любые {0} рецепта",
                    minTarget = 2, maxTarget = 6,
                    baseCoins = 60, baseExp = 30,
                    difficulty = QuestDifficulty.Easy
                },
                new QuestTemplate
                {
                    id = "craft_cocktails",
                    type = QuestType.CraftCategory,
                    category = "ClassicCocktail",
                    nameRu = "Классика ({0})",
                    descriptionRu = "Приготовьте {0} классических коктейля",
                    minTarget = 2, maxTarget = 4,
                    baseCoins = 70, baseExp = 35,
                    difficulty = QuestDifficulty.Medium
                },
                new QuestTemplate
                {
                    id = "craft_tropical",
                    type = QuestType.CraftCategory,
                    category = "TropicalCocktail",
                    nameRu = "Тропики ({0})",
                    descriptionRu = "Приготовьте {0} тропических коктейля",
                    minTarget = 2, maxTarget = 4,
                    baseCoins = 75, baseExp = 38,
                    difficulty = QuestDifficulty.Medium
                },
                new QuestTemplate
                {
                    id = "craft_appetizers",
                    type = QuestType.CraftCategory,
                    category = "Appetizer",
                    nameRu = "Закуски ({0})",
                    descriptionRu = "Приготовьте {0} закусок",
                    minTarget = 1, maxTarget = 3,
                    baseCoins = 65, baseExp = 32,
                    difficulty = QuestDifficulty.Medium
                },
                new QuestTemplate
                {
                    id = "discover_recipe",
                    type = QuestType.Discover,
                    nameRu = "Открой новый рецепт",
                    descriptionRu = "Откройте любой новый рецепт",
                    minTarget = 1, maxTarget = 1,
                    baseCoins = 150, baseExp = 75,
                    difficulty = QuestDifficulty.Hard
                },

                // Spirit quests
                new QuestTemplate
                {
                    id = "summon_spirit",
                    type = QuestType.Summon,
                    nameRu = "Призови духа",
                    descriptionRu = "Выполните {0} призыв(ов)",
                    minTarget = 1, maxTarget = 3,
                    baseCoins = 100, baseExp = 50,
                    difficulty = QuestDifficulty.Medium
                },

                // Economy quests
                new QuestTemplate
                {
                    id = "earn_coins",
                    type = QuestType.EarnCoins,
                    nameRu = "Заработай {0} монет",
                    descriptionRu = "Заработайте {0} монет любым способом",
                    minTarget = 200, maxTarget = 500,
                    baseCoins = 100, baseExp = 50,
                    difficulty = QuestDifficulty.Medium
                },
                new QuestTemplate
                {
                    id = "shop_purchase",
                    type = QuestType.ShopPurchase,
                    nameRu = "Покупка в магазине",
                    descriptionRu = "Совершите покупку в магазине",
                    minTarget = 1, maxTarget = 1,
                    baseCoins = 50, baseExp = 25,
                    difficulty = QuestDifficulty.Easy
                },

                // Combo quests
                new QuestTemplate
                {
                    id = "daily_combo",
                    type = QuestType.DailyCombo,
                    nameRu = "Дневной комбо",
                    descriptionRu = "Посадите, вырастите и соберите растение",
                    minTarget = 1, maxTarget = 1,
                    baseCoins = 120, baseExp = 60,
                    difficulty = QuestDifficulty.Medium
                },
                new QuestTemplate
                {
                    id = "master_chef",
                    type = QuestType.MasterChef,
                    nameRu = "Мастер-шеф",
                    descriptionRu = "Приготовьте 3 разных типа блюд",
                    minTarget = 3, maxTarget = 3,
                    baseCoins = 150, baseExp = 75,
                    difficulty = QuestDifficulty.Hard
                }
            };
        }

        #endregion

        #region Quest Generation

        private void GenerateDailyQuests()
        {
            activeQuests.Clear();
            completedTodayCount = 0;

            // Shuffle templates
            var shuffled = questTemplates.OrderBy(x => UnityEngine.Random.value).ToList();

            // Select quests with difficulty balance
            int easyCount = 2;
            int mediumCount = 2;
            int hardCount = 1;

            foreach (var template in shuffled)
            {
                if (activeQuests.Count >= dailyQuestCount) break;

                bool canAdd = false;
                if (template.difficulty == QuestDifficulty.Easy && easyCount > 0)
                {
                    easyCount--;
                    canAdd = true;
                }
                else if (template.difficulty == QuestDifficulty.Medium && mediumCount > 0)
                {
                    mediumCount--;
                    canAdd = true;
                }
                else if (template.difficulty == QuestDifficulty.Hard && hardCount > 0)
                {
                    hardCount--;
                    canAdd = true;
                }

                if (canAdd)
                {
                    activeQuests.Add(CreateQuestFromTemplate(template));
                }
            }

            // Fill remaining slots
            foreach (var template in shuffled)
            {
                if (activeQuests.Count >= dailyQuestCount) break;
                if (!activeQuests.Any(q => q.templateId == template.id))
                {
                    activeQuests.Add(CreateQuestFromTemplate(template));
                }
            }

            lastRefreshDate = DateTime.Today;
            SaveProgress();

            Debug.Log($"DailyQuestManager: Generated {activeQuests.Count} daily quests");
            OnQuestsRefreshed?.Invoke();
        }

        private DailyQuest CreateQuestFromTemplate(QuestTemplate template)
        {
            int target = UnityEngine.Random.Range(template.minTarget, template.maxTarget + 1);
            float difficultyMultiplier = template.difficulty switch
            {
                QuestDifficulty.Easy => 1f,
                QuestDifficulty.Medium => 1.5f,
                QuestDifficulty.Hard => 2.5f,
                _ => 1f
            };

            return new DailyQuest
            {
                templateId = template.id,
                type = template.type,
                category = template.category,
                name = string.Format(template.nameRu, target),
                description = string.Format(template.descriptionRu, target),
                targetValue = target,
                currentValue = 0,
                coinsReward = Mathf.RoundToInt(template.baseCoins * difficultyMultiplier * (target / (float)template.minTarget)),
                expReward = Mathf.RoundToInt(template.baseExp * difficultyMultiplier * (target / (float)template.minTarget)),
                gemsReward = template.difficulty == QuestDifficulty.Hard ? 10 : 0,
                difficulty = template.difficulty,
                isCompleted = false,
                isClaimed = false
            };
        }

        #endregion

        #region Quest Progress

        /// <summary>
        /// Report progress on a quest type
        /// </summary>
        public void ReportProgress(QuestType type, int amount = 1, string category = null)
        {
            foreach (var quest in activeQuests)
            {
                if (quest.isCompleted || quest.isClaimed) continue;
                if (quest.type != type) continue;
                if (!string.IsNullOrEmpty(quest.category) && quest.category != category) continue;

                quest.currentValue += amount;

                if (quest.currentValue >= quest.targetValue)
                {
                    quest.currentValue = quest.targetValue;
                    quest.isCompleted = true;
                    OnQuestCompleted?.Invoke(quest);
                    Debug.Log($"DailyQuestManager: Quest completed - {quest.name}");
                }
                else
                {
                    OnQuestProgress?.Invoke(quest);
                }
            }

            SaveProgress();
        }

        /// <summary>
        /// Claim rewards for a completed quest
        /// </summary>
        public bool ClaimReward(string templateId)
        {
            var quest = activeQuests.FirstOrDefault(q => q.templateId == templateId);
            if (quest == null || !quest.isCompleted || quest.isClaimed)
                return false;

            // Grant rewards
            var playerData = Core.PlayerData.Instance;
            if (playerData != null)
            {
                if (quest.coinsReward > 0)
                    playerData.AddCoins(quest.coinsReward);
                if (quest.gemsReward > 0)
                    playerData.AddGems(quest.gemsReward);
                if (quest.expReward > 0)
                    playerData.AddExperience(quest.expReward);
            }

            quest.isClaimed = true;
            completedTodayCount++;

            // Track for achievements
            Achievements.AchievementManager.Instance?.OnDailyTaskCompleted();

            SaveProgress();

            Debug.Log($"DailyQuestManager: Claimed reward for {quest.name}");
            return true;
        }

        /// <summary>
        /// Claim all completed quests
        /// </summary>
        public int ClaimAllRewards()
        {
            int claimedCount = 0;
            foreach (var quest in activeQuests)
            {
                if (quest.isCompleted && !quest.isClaimed)
                {
                    if (ClaimReward(quest.templateId))
                        claimedCount++;
                }
            }
            return claimedCount;
        }

        #endregion

        #region Queries

        public List<DailyQuest> GetActiveQuests()
        {
            return new List<DailyQuest>(activeQuests);
        }

        public int GetCompletedCount()
        {
            return activeQuests.Count(q => q.isCompleted);
        }

        public int GetClaimedCount()
        {
            return activeQuests.Count(q => q.isClaimed);
        }

        public int GetTotalCount()
        {
            return activeQuests.Count;
        }

        public bool HasUnclaimedRewards()
        {
            return activeQuests.Any(q => q.isCompleted && !q.isClaimed);
        }

        public TimeSpan GetTimeUntilRefresh()
        {
            DateTime nextRefresh = DateTime.Today.AddDays(1).AddHours(refreshHour);
            return nextRefresh - DateTime.Now;
        }

        #endregion

        #region Daily Refresh

        private void CheckDailyRefresh()
        {
            if (lastRefreshDate.Date < DateTime.Today)
            {
                GenerateDailyQuests();
            }
        }

        private void Update()
        {
            // Check every minute for refresh
            if (Time.frameCount % 3600 == 0) // ~60 seconds at 60fps
            {
                CheckDailyRefresh();
            }
        }

        /// <summary>
        /// Force refresh quests (for testing)
        /// </summary>
        [ContextMenu("Force Refresh Quests")]
        public void ForceRefresh()
        {
            GenerateDailyQuests();
        }

        #endregion

        #region Save/Load

        private const string SAVE_KEY = "DailyQuests";

        private void SaveProgress()
        {
            DailyQuestSaveData saveData = new DailyQuestSaveData
            {
                lastRefreshDate = lastRefreshDate.ToString("O"),
                completedTodayCount = completedTodayCount,
                quests = activeQuests
            };

            string json = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        private void LoadProgress()
        {
            if (!PlayerPrefs.HasKey(SAVE_KEY))
            {
                GenerateDailyQuests();
                return;
            }

            try
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                DailyQuestSaveData saveData = JsonUtility.FromJson<DailyQuestSaveData>(json);

                lastRefreshDate = DateTime.Parse(saveData.lastRefreshDate);
                completedTodayCount = saveData.completedTodayCount;
                activeQuests = saveData.quests ?? new List<DailyQuest>();

                Debug.Log($"DailyQuestManager: Loaded {activeQuests.Count} quests from {lastRefreshDate:d}");
            }
            catch (Exception e)
            {
                Debug.LogError($"DailyQuestManager: Failed to load progress: {e.Message}");
                GenerateDailyQuests();
            }
        }

        #endregion
    }

    #region Data Classes

    public enum QuestType
    {
        Harvest,
        Plant,
        PerfectHarvest,
        Craft,
        CraftCategory,
        Discover,
        Summon,
        EarnCoins,
        ShopPurchase,
        DailyCombo,
        MasterChef
    }

    public enum QuestDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    [Serializable]
    public class QuestTemplate
    {
        public string id;
        public QuestType type;
        public string category;
        public string nameRu;
        public string descriptionRu;
        public int minTarget;
        public int maxTarget;
        public int baseCoins;
        public int baseExp;
        public QuestDifficulty difficulty;
    }

    [Serializable]
    public class DailyQuest
    {
        public string templateId;
        public QuestType type;
        public string category;
        public string name;
        public string description;
        public int targetValue;
        public int currentValue;
        public int coinsReward;
        public int expReward;
        public int gemsReward;
        public QuestDifficulty difficulty;
        public bool isCompleted;
        public bool isClaimed;
    }

    [Serializable]
    public class DailyQuestSaveData
    {
        public string lastRefreshDate;
        public int completedTodayCount;
        public List<DailyQuest> quests;
    }

    #endregion
}
