using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WaveIsland.Events
{
    /// <summary>
    /// Manages seasonal and special events
    /// </summary>
    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance { get; private set; }

        [Header("Events")]
        public event Action<GameEvent> OnEventStarted;
        public event Action<GameEvent> OnEventEnded;
        public event Action<EventMission> OnMissionProgress;
        public event Action<EventMission> OnMissionCompleted;
        public event Action<int> OnEventPointsChanged;

        [Header("Settings")]
        [SerializeField] private TextAsset eventsDataFile;
        [SerializeField] private float checkIntervalSeconds = 60f;

        // Event data
        private List<GameEvent> allEvents = new List<GameEvent>();
        private GameEvent activeEvent;
        private List<EventMission> activeMissions = new List<EventMission>();
        private int currentEventPoints = 0;
        private List<int> claimedRewardTiers = new List<int>();

        // Timing
        private float lastCheckTime;

        public GameEvent ActiveEvent => activeEvent;
        public int EventPoints => currentEventPoints;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadEventsData();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            LoadProgress();
            CheckForActiveEvent();
        }

        private void Update()
        {
            if (Time.time - lastCheckTime > checkIntervalSeconds)
            {
                lastCheckTime = Time.time;
                CheckForActiveEvent();
            }
        }

        #region Load Events

        private void LoadEventsData()
        {
            if (eventsDataFile == null)
            {
                CreateDefaultEvents();
                return;
            }

            try
            {
                EventsDataWrapper wrapper = JsonUtility.FromJson<EventsDataWrapper>(eventsDataFile.text);
                allEvents = wrapper.events ?? new List<GameEvent>();
                Debug.Log($"EventManager: Loaded {allEvents.Count} events");
            }
            catch (Exception e)
            {
                Debug.LogError($"EventManager: Failed to load events: {e.Message}");
                CreateDefaultEvents();
            }
        }

        private void CreateDefaultEvents()
        {
            allEvents = new List<GameEvent>
            {
                // Summer Beach Party
                new GameEvent
                {
                    id = "summer_beach_2024",
                    nameRu = "Пляжная вечеринка",
                    descriptionRu = "Летний пляжный фестиваль! Готовьте тропические коктейли и получайте эксклюзивные награды.",
                    type = EventType.Seasonal,
                    startDate = "2024-06-21",
                    endDate = "2024-07-21",
                    themeColor = "#FF6B35",
                    bannerImage = "Events/summer_banner",
                    missions = new List<EventMission>
                    {
                        new EventMission { id = "summer_cocktails", nameRu = "Тропический мастер", descriptionRu = "Приготовьте 20 тропических коктейлей", targetValue = 20, pointsReward = 100, missionType = MissionType.CraftCategory, category = "TropicalCocktail" },
                        new EventMission { id = "summer_harvest", nameRu = "Летний урожай", descriptionRu = "Соберите 50 фруктов", targetValue = 50, pointsReward = 150, missionType = MissionType.Harvest },
                        new EventMission { id = "summer_spirit", nameRu = "Пляжный дух", descriptionRu = "Призовите 5 духов", targetValue = 5, pointsReward = 200, missionType = MissionType.Summon }
                    },
                    rewardTiers = new List<EventRewardTier>
                    {
                        new EventRewardTier { requiredPoints = 100, coins = 500, gems = 0, specialReward = "" },
                        new EventRewardTier { requiredPoints = 300, coins = 1000, gems = 50, specialReward = "" },
                        new EventRewardTier { requiredPoints = 500, coins = 2000, gems = 100, specialReward = "summer_avatar_frame" }
                    }
                },

                // Halloween
                new GameEvent
                {
                    id = "halloween_2024",
                    nameRu = "Хэллоуин",
                    descriptionRu = "Жуткий праздник на острове! Создавайте мистические напитки и собирайте призрачные награды.",
                    type = EventType.Seasonal,
                    startDate = "2024-10-25",
                    endDate = "2024-11-05",
                    themeColor = "#8B00FF",
                    bannerImage = "Events/halloween_banner",
                    missions = new List<EventMission>
                    {
                        new EventMission { id = "halloween_crafts", nameRu = "Зелья ведьмы", descriptionRu = "Приготовьте 15 любых рецептов", targetValue = 15, pointsReward = 100, missionType = MissionType.Craft },
                        new EventMission { id = "halloween_discover", nameRu = "Тайные рецепты", descriptionRu = "Откройте 3 новых рецепта", targetValue = 3, pointsReward = 250, missionType = MissionType.Discover },
                        new EventMission { id = "halloween_spirits", nameRu = "Призыв духов", descriptionRu = "Призовите 10 духов", targetValue = 10, pointsReward = 300, missionType = MissionType.Summon }
                    },
                    rewardTiers = new List<EventRewardTier>
                    {
                        new EventRewardTier { requiredPoints = 150, coins = 666, gems = 0, specialReward = "" },
                        new EventRewardTier { requiredPoints = 400, coins = 1500, gems = 66, specialReward = "" },
                        new EventRewardTier { requiredPoints = 650, coins = 3000, gems = 150, specialReward = "halloween_spirit_skin" }
                    }
                },

                // New Year
                new GameEvent
                {
                    id = "newyear_2025",
                    nameRu = "Новый Год 2025",
                    descriptionRu = "Праздничное настроение на острове! Готовьте праздничные напитки и встречайте Новый Год вместе!",
                    type = EventType.Seasonal,
                    startDate = "2024-12-20",
                    endDate = "2025-01-10",
                    themeColor = "#00CED1",
                    bannerImage = "Events/newyear_banner",
                    missions = new List<EventMission>
                    {
                        new EventMission { id = "ny_hot_drinks", nameRu = "Горячие напитки", descriptionRu = "Приготовьте 10 горячих напитков", targetValue = 10, pointsReward = 150, missionType = MissionType.CraftCategory, category = "HotDrink" },
                        new EventMission { id = "ny_daily", nameRu = "Ежедневная активность", descriptionRu = "Выполните 15 ежедневных заданий", targetValue = 15, pointsReward = 200, missionType = MissionType.DailyQuests },
                        new EventMission { id = "ny_coins", nameRu = "Праздничный заработок", descriptionRu = "Заработайте 5000 монет", targetValue = 5000, pointsReward = 250, missionType = MissionType.EarnCoins }
                    },
                    rewardTiers = new List<EventRewardTier>
                    {
                        new EventRewardTier { requiredPoints = 100, coins = 2025, gems = 0, specialReward = "" },
                        new EventRewardTier { requiredPoints = 350, coins = 2500, gems = 100, specialReward = "" },
                        new EventRewardTier { requiredPoints = 600, coins = 5000, gems = 200, specialReward = "newyear_island_theme" }
                    }
                },

                // Valentine's Day
                new GameEvent
                {
                    id = "valentines_2025",
                    nameRu = "День Святого Валентина",
                    descriptionRu = "Романтическая атмосфера на острове! Готовьте напитки любви!",
                    type = EventType.Seasonal,
                    startDate = "2025-02-10",
                    endDate = "2025-02-18",
                    themeColor = "#FF69B4",
                    bannerImage = "Events/valentines_banner",
                    missions = new List<EventMission>
                    {
                        new EventMission { id = "val_cocktails", nameRu = "Напитки любви", descriptionRu = "Приготовьте 12 коктейлей", targetValue = 12, pointsReward = 120, missionType = MissionType.CraftCategory, category = "ClassicCocktail" },
                        new EventMission { id = "val_desserts", nameRu = "Сладости", descriptionRu = "Приготовьте 8 десертов", targetValue = 8, pointsReward = 150, missionType = MissionType.CraftCategory, category = "Dessert" },
                        new EventMission { id = "val_summon", nameRu = "Духи любви", descriptionRu = "Призовите 7 духов", targetValue = 7, pointsReward = 180, missionType = MissionType.Summon }
                    },
                    rewardTiers = new List<EventRewardTier>
                    {
                        new EventRewardTier { requiredPoints = 100, coins = 500, gems = 25, specialReward = "" },
                        new EventRewardTier { requiredPoints = 280, coins = 1500, gems = 75, specialReward = "" },
                        new EventRewardTier { requiredPoints = 450, coins = 3000, gems = 150, specialReward = "valentines_avatar" }
                    }
                }
            };

            Debug.Log($"EventManager: Created {allEvents.Count} default events");
        }

        #endregion

        #region Event Check

        private void CheckForActiveEvent()
        {
            DateTime now = DateTime.Now;
            GameEvent newActiveEvent = null;

            foreach (var evt in allEvents)
            {
                if (DateTime.TryParse(evt.startDate, out DateTime start) &&
                    DateTime.TryParse(evt.endDate, out DateTime end))
                {
                    if (now >= start && now <= end)
                    {
                        newActiveEvent = evt;
                        break;
                    }
                }
            }

            // Event changed?
            if (newActiveEvent?.id != activeEvent?.id)
            {
                if (activeEvent != null)
                {
                    EndEvent(activeEvent);
                }

                if (newActiveEvent != null)
                {
                    StartEvent(newActiveEvent);
                }
                else
                {
                    activeEvent = null;
                    activeMissions.Clear();
                }
            }
        }

        private void StartEvent(GameEvent evt)
        {
            activeEvent = evt;
            activeMissions = evt.missions?.Select(m => new EventMission
            {
                id = m.id,
                nameRu = m.nameRu,
                descriptionRu = m.descriptionRu,
                targetValue = m.targetValue,
                currentValue = 0,
                pointsReward = m.pointsReward,
                missionType = m.missionType,
                category = m.category,
                isCompleted = false,
                isClaimed = false
            }).ToList() ?? new List<EventMission>();

            currentEventPoints = 0;
            claimedRewardTiers.Clear();

            LoadEventProgress();

            Debug.Log($"EventManager: Started event '{evt.nameRu}'");
            OnEventStarted?.Invoke(evt);
        }

        private void EndEvent(GameEvent evt)
        {
            Debug.Log($"EventManager: Ended event '{evt.nameRu}'");
            OnEventEnded?.Invoke(evt);
            ClearEventProgress();
        }

        #endregion

        #region Mission Progress

        public void ReportProgress(MissionType type, int amount = 1, string category = null)
        {
            if (activeEvent == null || activeMissions == null) return;

            foreach (var mission in activeMissions)
            {
                if (mission.isCompleted || mission.isClaimed) continue;
                if (mission.missionType != type) continue;
                if (!string.IsNullOrEmpty(mission.category) && mission.category != category) continue;

                mission.currentValue += amount;

                if (mission.currentValue >= mission.targetValue)
                {
                    mission.currentValue = mission.targetValue;
                    mission.isCompleted = true;
                    OnMissionCompleted?.Invoke(mission);
                    Debug.Log($"EventManager: Mission completed - {mission.nameRu}");
                }
                else
                {
                    OnMissionProgress?.Invoke(mission);
                }
            }

            SaveEventProgress();
        }

        public bool ClaimMissionReward(string missionId)
        {
            var mission = activeMissions?.FirstOrDefault(m => m.id == missionId);
            if (mission == null || !mission.isCompleted || mission.isClaimed)
                return false;

            mission.isClaimed = true;
            currentEventPoints += mission.pointsReward;

            OnEventPointsChanged?.Invoke(currentEventPoints);
            SaveEventProgress();

            Debug.Log($"EventManager: Claimed mission '{mission.nameRu}', +{mission.pointsReward} points");
            return true;
        }

        public bool ClaimRewardTier(int tierIndex)
        {
            if (activeEvent == null || activeEvent.rewardTiers == null) return false;
            if (tierIndex < 0 || tierIndex >= activeEvent.rewardTiers.Count) return false;
            if (claimedRewardTiers.Contains(tierIndex)) return false;

            var tier = activeEvent.rewardTiers[tierIndex];
            if (currentEventPoints < tier.requiredPoints) return false;

            // Grant rewards
            var playerData = Core.PlayerData.Instance;
            if (playerData != null)
            {
                if (tier.coins > 0)
                    playerData.AddCoins(tier.coins);
                if (tier.gems > 0)
                    playerData.AddGems(tier.gems);
            }

            // Special reward
            if (!string.IsNullOrEmpty(tier.specialReward))
            {
                GrantSpecialReward(tier.specialReward);
            }

            claimedRewardTiers.Add(tierIndex);
            SaveEventProgress();

            Debug.Log($"EventManager: Claimed tier {tierIndex} reward");
            return true;
        }

        private void GrantSpecialReward(string rewardId)
        {
            // Store special rewards for later use (avatars, themes, etc.)
            string key = $"SpecialReward_{rewardId}";
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
            Debug.Log($"EventManager: Granted special reward '{rewardId}'");
        }

        #endregion

        #region Queries

        public List<EventMission> GetActiveMissions()
        {
            return activeMissions?.ToList() ?? new List<EventMission>();
        }

        public TimeSpan GetTimeRemaining()
        {
            if (activeEvent == null) return TimeSpan.Zero;

            if (DateTime.TryParse(activeEvent.endDate, out DateTime end))
            {
                var remaining = end - DateTime.Now;
                return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
            }

            return TimeSpan.Zero;
        }

        public int GetCompletedMissionsCount()
        {
            return activeMissions?.Count(m => m.isCompleted) ?? 0;
        }

        public int GetClaimedMissionsCount()
        {
            return activeMissions?.Count(m => m.isClaimed) ?? 0;
        }

        public bool HasUnclaimedMissions()
        {
            return activeMissions?.Any(m => m.isCompleted && !m.isClaimed) ?? false;
        }

        public bool HasUnclaimedTiers()
        {
            if (activeEvent?.rewardTiers == null) return false;

            for (int i = 0; i < activeEvent.rewardTiers.Count; i++)
            {
                if (!claimedRewardTiers.Contains(i) &&
                    currentEventPoints >= activeEvent.rewardTiers[i].requiredPoints)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsTierClaimed(int tierIndex)
        {
            return claimedRewardTiers.Contains(tierIndex);
        }

        public bool HasSpecialReward(string rewardId)
        {
            return PlayerPrefs.GetInt($"SpecialReward_{rewardId}", 0) == 1;
        }

        public List<GameEvent> GetUpcomingEvents()
        {
            DateTime now = DateTime.Now;
            return allEvents.Where(e =>
            {
                if (DateTime.TryParse(e.startDate, out DateTime start))
                {
                    return start > now;
                }
                return false;
            }).OrderBy(e => DateTime.Parse(e.startDate)).ToList();
        }

        #endregion

        #region Save/Load

        private const string EVENT_SAVE_KEY = "EventProgress";

        private void SaveEventProgress()
        {
            if (activeEvent == null) return;

            var saveData = new EventSaveData
            {
                eventId = activeEvent.id,
                points = currentEventPoints,
                claimedTiers = claimedRewardTiers,
                missions = activeMissions
            };

            string json = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString(EVENT_SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        private void LoadEventProgress()
        {
            if (activeEvent == null || !PlayerPrefs.HasKey(EVENT_SAVE_KEY)) return;

            try
            {
                string json = PlayerPrefs.GetString(EVENT_SAVE_KEY);
                var saveData = JsonUtility.FromJson<EventSaveData>(json);

                if (saveData.eventId == activeEvent.id)
                {
                    currentEventPoints = saveData.points;
                    claimedRewardTiers = saveData.claimedTiers ?? new List<int>();

                    // Restore mission progress
                    if (saveData.missions != null)
                    {
                        foreach (var savedMission in saveData.missions)
                        {
                            var mission = activeMissions.FirstOrDefault(m => m.id == savedMission.id);
                            if (mission != null)
                            {
                                mission.currentValue = savedMission.currentValue;
                                mission.isCompleted = savedMission.isCompleted;
                                mission.isClaimed = savedMission.isClaimed;
                            }
                        }
                    }

                    Debug.Log($"EventManager: Loaded progress for '{activeEvent.nameRu}'");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"EventManager: Failed to load progress: {e.Message}");
            }
        }

        private void ClearEventProgress()
        {
            PlayerPrefs.DeleteKey(EVENT_SAVE_KEY);
            PlayerPrefs.Save();
        }

        private void LoadProgress()
        {
            // General progress loading
        }

        #endregion
    }

    #region Data Classes

    public enum EventType
    {
        Seasonal,
        Special,
        Collaboration,
        Weekend
    }

    public enum MissionType
    {
        Craft,
        CraftCategory,
        Harvest,
        Discover,
        Summon,
        DailyQuests,
        EarnCoins,
        Login
    }

    [Serializable]
    public class GameEvent
    {
        public string id;
        public string nameRu;
        public string descriptionRu;
        public EventType type;
        public string startDate;
        public string endDate;
        public string themeColor;
        public string bannerImage;
        public List<EventMission> missions;
        public List<EventRewardTier> rewardTiers;
    }

    [Serializable]
    public class EventMission
    {
        public string id;
        public string nameRu;
        public string descriptionRu;
        public MissionType missionType;
        public string category;
        public int targetValue;
        public int currentValue;
        public int pointsReward;
        public bool isCompleted;
        public bool isClaimed;
    }

    [Serializable]
    public class EventRewardTier
    {
        public int requiredPoints;
        public int coins;
        public int gems;
        public string specialReward;
    }

    [Serializable]
    public class EventsDataWrapper
    {
        public List<GameEvent> events;
    }

    [Serializable]
    public class EventSaveData
    {
        public string eventId;
        public int points;
        public List<int> claimedTiers;
        public List<EventMission> missions;
    }

    #endregion
}
