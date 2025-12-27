using System;
using System.Collections.Generic;
using UnityEngine;

namespace WaveIsland.Analytics
{
    /// <summary>
    /// Analytics manager for tracking player behavior and game metrics
    /// </summary>
    public class AnalyticsManager : MonoBehaviour
    {
        public static AnalyticsManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool analyticsEnabled = true;
        [SerializeField] private bool debugMode = false;
        [SerializeField] private float batchInterval = 30f;
        [SerializeField] private int maxBatchSize = 50;

        [Header("Privacy")]
        [SerializeField] private bool collectPersonalData = false;

        // Event queue
        private List<AnalyticsEvent> eventQueue = new List<AnalyticsEvent>();
        private float lastBatchTime;

        // Session data
        private string sessionId;
        private DateTime sessionStartTime;
        private int sessionEventCount;

        // User properties
        private Dictionary<string, object> userProperties = new Dictionary<string, object>();

        // Events
        public event Action<AnalyticsEvent> OnEventLogged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSession();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Load analytics consent
            analyticsEnabled = PlayerPrefs.GetInt("AnalyticsEnabled", 1) == 1;

            // Track session start
            TrackEvent("session_start", new Dictionary<string, object>
            {
                { "device_model", SystemInfo.deviceModel },
                { "os", SystemInfo.operatingSystem },
                { "app_version", Application.version }
            });
        }

        private void Update()
        {
            // Batch send events periodically
            if (Time.time - lastBatchTime >= batchInterval && eventQueue.Count > 0)
            {
                SendBatch();
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                TrackEvent("app_background");
                SendBatch();
            }
            else
            {
                TrackEvent("app_foreground");
            }
        }

        private void OnApplicationQuit()
        {
            TrackSessionEnd();
            SendBatch();
        }

        #region Session Management

        private void InitializeSession()
        {
            sessionId = Guid.NewGuid().ToString();
            sessionStartTime = DateTime.UtcNow;
            sessionEventCount = 0;

            Debug.Log($"Analytics: Session started - {sessionId}");
        }

        private void TrackSessionEnd()
        {
            var sessionDuration = (DateTime.UtcNow - sessionStartTime).TotalSeconds;

            TrackEvent("session_end", new Dictionary<string, object>
            {
                { "duration_seconds", sessionDuration },
                { "event_count", sessionEventCount }
            });
        }

        #endregion

        #region Event Tracking

        /// <summary>
        /// Track a simple event
        /// </summary>
        public void TrackEvent(string eventName)
        {
            TrackEvent(eventName, null);
        }

        /// <summary>
        /// Track an event with parameters
        /// </summary>
        public void TrackEvent(string eventName, Dictionary<string, object> parameters)
        {
            if (!analyticsEnabled) return;

            var analyticsEvent = new AnalyticsEvent
            {
                name = eventName,
                timestamp = DateTime.UtcNow,
                sessionId = sessionId,
                parameters = parameters ?? new Dictionary<string, object>()
            };

            // Add common parameters
            analyticsEvent.parameters["event_id"] = Guid.NewGuid().ToString();
            analyticsEvent.parameters["session_event_index"] = sessionEventCount++;

            eventQueue.Add(analyticsEvent);

            if (debugMode)
            {
                Debug.Log($"Analytics: {eventName} - {JsonUtility.ToJson(analyticsEvent)}");
            }

            OnEventLogged?.Invoke(analyticsEvent);

            // Send immediately if batch is full
            if (eventQueue.Count >= maxBatchSize)
            {
                SendBatch();
            }
        }

        #endregion

        #region Predefined Events

        /// <summary>
        /// Track level up
        /// </summary>
        public void TrackLevelUp(int newLevel)
        {
            TrackEvent("level_up", new Dictionary<string, object>
            {
                { "level", newLevel }
            });
        }

        /// <summary>
        /// Track recipe crafted
        /// </summary>
        public void TrackCraft(string recipeId, string category, bool isNew)
        {
            TrackEvent("craft", new Dictionary<string, object>
            {
                { "recipe_id", recipeId },
                { "category", category },
                { "is_new_discovery", isNew }
            });
        }

        /// <summary>
        /// Track harvest
        /// </summary>
        public void TrackHarvest(string cropId, int quantity, bool isPerfect)
        {
            TrackEvent("harvest", new Dictionary<string, object>
            {
                { "crop_id", cropId },
                { "quantity", quantity },
                { "is_perfect", isPerfect }
            });
        }

        /// <summary>
        /// Track purchase
        /// </summary>
        public void TrackPurchase(string itemId, string currency, int amount)
        {
            TrackEvent("purchase", new Dictionary<string, object>
            {
                { "item_id", itemId },
                { "currency", currency },
                { "amount", amount }
            });
        }

        /// <summary>
        /// Track spirit summon
        /// </summary>
        public void TrackSummon(string spiritId, string rarity, bool isNew)
        {
            TrackEvent("spirit_summon", new Dictionary<string, object>
            {
                { "spirit_id", spiritId },
                { "rarity", rarity },
                { "is_new", isNew }
            });
        }

        /// <summary>
        /// Track achievement unlocked
        /// </summary>
        public void TrackAchievement(string achievementId)
        {
            TrackEvent("achievement_unlocked", new Dictionary<string, object>
            {
                { "achievement_id", achievementId }
            });
        }

        /// <summary>
        /// Track quest completed
        /// </summary>
        public void TrackQuestComplete(string questId, string difficulty)
        {
            TrackEvent("quest_complete", new Dictionary<string, object>
            {
                { "quest_id", questId },
                { "difficulty", difficulty }
            });
        }

        /// <summary>
        /// Track screen view
        /// </summary>
        public void TrackScreenView(string screenName)
        {
            TrackEvent("screen_view", new Dictionary<string, object>
            {
                { "screen_name", screenName }
            });
        }

        /// <summary>
        /// Track tutorial step
        /// </summary>
        public void TrackTutorial(string step, bool completed)
        {
            TrackEvent("tutorial", new Dictionary<string, object>
            {
                { "step", step },
                { "completed", completed }
            });
        }

        /// <summary>
        /// Track iiko order
        /// </summary>
        public void TrackIikoOrder(string orderId, int itemCount, int bonusCoins)
        {
            TrackEvent("iiko_order", new Dictionary<string, object>
            {
                { "order_id", orderId },
                { "item_count", itemCount },
                { "bonus_coins", bonusCoins }
            });
        }

        /// <summary>
        /// Track error
        /// </summary>
        public void TrackError(string errorType, string message)
        {
            TrackEvent("error", new Dictionary<string, object>
            {
                { "error_type", errorType },
                { "message", message }
            });
        }

        #endregion

        #region User Properties

        /// <summary>
        /// Set user property
        /// </summary>
        public void SetUserProperty(string key, object value)
        {
            userProperties[key] = value;

            if (debugMode)
            {
                Debug.Log($"Analytics: User property set - {key}: {value}");
            }
        }

        /// <summary>
        /// Update user properties from player data
        /// </summary>
        public void UpdateUserProperties()
        {
            var playerData = Core.PlayerData.Instance;
            if (playerData == null) return;

            SetUserProperty("player_level", playerData.Level);
            SetUserProperty("total_coins", playerData.Coins);
            SetUserProperty("total_gems", playerData.Gems);

            var achievements = Achievements.AchievementManager.Instance;
            if (achievements != null)
            {
                SetUserProperty("total_crafts", achievements.GetStat("TotalCrafts"));
                SetUserProperty("recipes_discovered", achievements.GetStat("RecipesDiscovered"));
            }
        }

        #endregion

        #region Batch Sending

        private void SendBatch()
        {
            if (eventQueue.Count == 0) return;

            var batch = new AnalyticsBatch
            {
                sessionId = sessionId,
                userId = GetUserId(),
                events = new List<AnalyticsEvent>(eventQueue),
                userProperties = new Dictionary<string, object>(userProperties),
                timestamp = DateTime.UtcNow
            };

            // Send to backend (placeholder - integrate with actual analytics service)
            StartCoroutine(SendBatchCoroutine(batch));

            eventQueue.Clear();
            lastBatchTime = Time.time;
        }

        private System.Collections.IEnumerator SendBatchCoroutine(AnalyticsBatch batch)
        {
            // In production, send to Firebase Analytics, Amplitude, or custom backend
            // For now, just log

            if (debugMode)
            {
                Debug.Log($"Analytics: Sending batch with {batch.events.Count} events");
            }

            // Simulate network delay
            yield return new WaitForSeconds(0.1f);

            // TODO: Implement actual analytics backend integration
            // Example with UnityWebRequest:
            /*
            string json = JsonUtility.ToJson(batch);
            using (var request = new UnityWebRequest(analyticsEndpoint, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                yield return request.SendWebRequest();
            }
            */
        }

        private string GetUserId()
        {
            // Get or create persistent user ID
            string userId = PlayerPrefs.GetString("AnalyticsUserId", "");

            if (string.IsNullOrEmpty(userId))
            {
                userId = Guid.NewGuid().ToString();
                PlayerPrefs.SetString("AnalyticsUserId", userId);
                PlayerPrefs.Save();
            }

            return userId;
        }

        #endregion

        #region Privacy Controls

        /// <summary>
        /// Enable or disable analytics
        /// </summary>
        public void SetAnalyticsEnabled(bool enabled)
        {
            analyticsEnabled = enabled;
            PlayerPrefs.SetInt("AnalyticsEnabled", enabled ? 1 : 0);
            PlayerPrefs.Save();

            if (enabled)
            {
                InitializeSession();
            }
            else
            {
                eventQueue.Clear();
            }

            Debug.Log($"Analytics: {(enabled ? "Enabled" : "Disabled")}");
        }

        /// <summary>
        /// Check if analytics is enabled
        /// </summary>
        public bool IsAnalyticsEnabled() => analyticsEnabled;

        /// <summary>
        /// Delete all analytics data
        /// </summary>
        public void DeleteUserData()
        {
            PlayerPrefs.DeleteKey("AnalyticsUserId");
            eventQueue.Clear();
            userProperties.Clear();

            Debug.Log("Analytics: User data deleted");
        }

        #endregion

        #region Debug

        /// <summary>
        /// Get current event queue size
        /// </summary>
        public int GetQueueSize() => eventQueue.Count;

        /// <summary>
        /// Force send all queued events
        /// </summary>
        public void FlushEvents()
        {
            SendBatch();
        }

        #endregion
    }

    #region Data Classes

    [Serializable]
    public class AnalyticsEvent
    {
        public string name;
        public DateTime timestamp;
        public string sessionId;
        public Dictionary<string, object> parameters;
    }

    [Serializable]
    public class AnalyticsBatch
    {
        public string sessionId;
        public string userId;
        public List<AnalyticsEvent> events;
        public Dictionary<string, object> userProperties;
        public DateTime timestamp;
    }

    #endregion
}
