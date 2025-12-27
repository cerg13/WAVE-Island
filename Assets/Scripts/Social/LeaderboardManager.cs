using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace WaveIsland.Social
{
    /// <summary>
    /// Manages leaderboards and player rankings
    /// </summary>
    public class LeaderboardManager : MonoBehaviour
    {
        public static LeaderboardManager Instance { get; private set; }

        [Header("Events")]
        public event Action<LeaderboardData> OnLeaderboardLoaded;
        public event Action<int> OnPlayerRankUpdated;
        public event Action<string> OnError;

        [Header("Settings")]
        [SerializeField] private string apiBaseUrl = "http://localhost:3000/api";
        [SerializeField] private float cacheTime = 300f; // 5 minutes

        // Cached data
        private Dictionary<LeaderboardType, LeaderboardData> cachedLeaderboards = new Dictionary<LeaderboardType, LeaderboardData>();
        private Dictionary<LeaderboardType, float> lastFetchTime = new Dictionary<LeaderboardType, float>();

        // Local player data
        private int localPlayerRank = -1;
        private LeaderboardEntry localPlayerEntry;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #region Fetch Leaderboards

        /// <summary>
        /// Get leaderboard data (from cache or server)
        /// </summary>
        public void GetLeaderboard(LeaderboardType type, int limit = 100, bool forceRefresh = false)
        {
            // Check cache
            if (!forceRefresh && cachedLeaderboards.TryGetValue(type, out LeaderboardData cached))
            {
                if (lastFetchTime.TryGetValue(type, out float lastTime))
                {
                    if (Time.time - lastTime < cacheTime)
                    {
                        OnLeaderboardLoaded?.Invoke(cached);
                        return;
                    }
                }
            }

            StartCoroutine(FetchLeaderboardRoutine(type, limit));
        }

        private IEnumerator FetchLeaderboardRoutine(LeaderboardType type, int limit)
        {
            string typeStr = type.ToString().ToLower();
            string url = $"{apiBaseUrl}/leaderboard/{typeStr}?limit={limit}";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                // Add auth token
                string token = PlayerPrefs.GetString("AuthToken", "");
                if (!string.IsNullOrEmpty(token))
                {
                    request.SetRequestHeader("Authorization", $"Bearer {token}");
                }

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        LeaderboardResponse response = JsonUtility.FromJson<LeaderboardResponse>(request.downloadHandler.text);

                        LeaderboardData data = new LeaderboardData
                        {
                            type = type,
                            entries = response.entries,
                            playerRank = response.playerRank,
                            totalPlayers = response.totalPlayers,
                            lastUpdated = DateTime.Now
                        };

                        // Cache
                        cachedLeaderboards[type] = data;
                        lastFetchTime[type] = Time.time;

                        // Update local rank
                        if (response.playerRank > 0)
                        {
                            localPlayerRank = response.playerRank;
                            OnPlayerRankUpdated?.Invoke(localPlayerRank);
                        }

                        OnLeaderboardLoaded?.Invoke(data);
                    }
                    catch (Exception e)
                    {
                        OnError?.Invoke($"Failed to parse leaderboard: {e.Message}");
                    }
                }
                else
                {
                    OnError?.Invoke($"Failed to fetch leaderboard: {request.error}");

                    // Return cached data if available
                    if (cachedLeaderboards.TryGetValue(type, out LeaderboardData cached))
                    {
                        OnLeaderboardLoaded?.Invoke(cached);
                    }
                }
            }
        }

        #endregion

        #region Submit Score

        /// <summary>
        /// Submit player score to leaderboard
        /// </summary>
        public void SubmitScore(LeaderboardType type, int score)
        {
            StartCoroutine(SubmitScoreRoutine(type, score));
        }

        private IEnumerator SubmitScoreRoutine(LeaderboardType type, int score)
        {
            string typeStr = type.ToString().ToLower();
            string url = $"{apiBaseUrl}/leaderboard/{typeStr}/submit";

            ScoreSubmission submission = new ScoreSubmission { score = score };
            string jsonBody = JsonUtility.ToJson(submission);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                string token = PlayerPrefs.GetString("AuthToken", "");
                if (!string.IsNullOrEmpty(token))
                {
                    request.SetRequestHeader("Authorization", $"Bearer {token}");
                }

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        SubmitResponse response = JsonUtility.FromJson<SubmitResponse>(request.downloadHandler.text);
                        localPlayerRank = response.newRank;
                        OnPlayerRankUpdated?.Invoke(localPlayerRank);

                        // Invalidate cache
                        lastFetchTime.Remove(type);

                        Debug.Log($"LeaderboardManager: Score submitted, new rank: {localPlayerRank}");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"LeaderboardManager: Failed to parse response: {e.Message}");
                    }
                }
                else
                {
                    Debug.LogError($"LeaderboardManager: Failed to submit score: {request.error}");
                }
            }
        }

        #endregion

        #region Queries

        public int GetLocalPlayerRank()
        {
            return localPlayerRank;
        }

        public LeaderboardEntry GetLocalPlayerEntry()
        {
            return localPlayerEntry;
        }

        public LeaderboardData GetCachedLeaderboard(LeaderboardType type)
        {
            return cachedLeaderboards.TryGetValue(type, out LeaderboardData data) ? data : null;
        }

        #endregion

        #region Auto-Submit

        /// <summary>
        /// Calculate and submit player's current scores
        /// </summary>
        public void SubmitAllScores()
        {
            var playerData = Core.PlayerData.Instance;
            if (playerData == null) return;

            // Total Level
            SubmitScore(LeaderboardType.Level, playerData.Level);

            // Total Crafts
            var achievements = Achievements.AchievementManager.Instance;
            if (achievements != null)
            {
                SubmitScore(LeaderboardType.TotalCrafts, achievements.GetStat("TotalCrafts"));
                SubmitScore(LeaderboardType.RecipesDiscovered, achievements.GetStat("RecipesDiscovered"));
                SubmitScore(LeaderboardType.TotalHarvests, achievements.GetStat("TotalHarvests"));
                SubmitScore(LeaderboardType.SpiritCollection, achievements.GetStat("UniqueSpirits"));
            }
        }

        #endregion
    }

    #region Data Classes

    public enum LeaderboardType
    {
        Level,
        TotalCrafts,
        RecipesDiscovered,
        TotalHarvests,
        SpiritCollection,
        Weekly,
        IikoOrders
    }

    [Serializable]
    public class LeaderboardEntry
    {
        public int rank;
        public string oderId;
        public string displayName;
        public int score;
        public int level;
        public string avatarUrl;
        public bool isCurrentPlayer;
    }

    [Serializable]
    public class LeaderboardData
    {
        public LeaderboardType type;
        public List<LeaderboardEntry> entries;
        public int playerRank;
        public int totalPlayers;
        public DateTime lastUpdated;
    }

    [Serializable]
    public class LeaderboardResponse
    {
        public List<LeaderboardEntry> entries;
        public int playerRank;
        public int totalPlayers;
    }

    [Serializable]
    public class ScoreSubmission
    {
        public int score;
    }

    [Serializable]
    public class SubmitResponse
    {
        public int newRank;
        public bool isNewHighScore;
    }

    #endregion
}
