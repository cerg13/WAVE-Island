using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using WaveIsland.Core;

namespace WaveIsland.Iiko
{
    /// <summary>
    /// Manages iiko restaurant integration
    /// </summary>
    public class IikoManager : MonoBehaviour
    {
        public static IikoManager Instance { get; private set; }

        public event Action<IikoOrderResult> OnOrderVerified;
        public event Action<IikoMilestone> OnMilestoneUnlocked;
        public event Action<IikoMilestone> OnMilestoneClaimed;
        public event Action<string> OnPhoneLinked;
        public event Action<string> OnError;

        [Header("API Configuration")]
        [SerializeField] private string apiBaseUrl = "https://api.waveisland.com";
        [SerializeField] private float requestTimeout = 30f;

        [Header("Cached Data")]
        private IikoPlayerData playerIikoData;
        private List<IikoMilestone> milestones = new List<IikoMilestone>();
        private List<IikoOrderHistory> orderHistory = new List<IikoOrderHistory>();

        private string authToken;
        private bool isInitialized = false;

        public bool IsPhoneLinked => !string.IsNullOrEmpty(playerIikoData?.LinkedPhone);
        public int TotalOrders => playerIikoData?.TotalOrders ?? 0;
        public float TotalSpent => playerIikoData?.TotalSpent ?? 0;

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

        /// <summary>
        /// Initialize with auth token
        /// </summary>
        public void Initialize(string token)
        {
            authToken = token;
            isInitialized = true;
            StartCoroutine(LoadPlayerIikoData());
        }

        /// <summary>
        /// Link phone number to account
        /// </summary>
        public void LinkPhone(string phoneNumber, Action<bool, string> callback = null)
        {
            if (!isInitialized)
            {
                callback?.Invoke(false, "Not initialized");
                return;
            }

            StartCoroutine(LinkPhoneCoroutine(phoneNumber, callback));
        }

        private IEnumerator LinkPhoneCoroutine(string phoneNumber, Action<bool, string> callback)
        {
            var json = JsonUtility.ToJson(new PhoneLinkRequest { phone = phoneNumber });

            using (var request = CreatePostRequest("/api/iiko/link-phone", json))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<PhoneLinkResponse>(request.downloadHandler.text);

                    if (response.success)
                    {
                        playerIikoData.LinkedPhone = response.phone;
                        playerIikoData.TotalOrders += response.importedOrders;

                        OnPhoneLinked?.Invoke(response.phone);
                        callback?.Invoke(true, $"Phone linked! Imported {response.importedOrders} orders");

                        // Apply imported rewards
                        if (response.rewards != null)
                        {
                            ApplyRewards(response.rewards);
                        }

                        Debug.Log($"[Iiko] Phone linked: {response.phone}, imported {response.importedOrders} orders");
                    }
                    else
                    {
                        callback?.Invoke(false, "Failed to link phone");
                    }
                }
                else
                {
                    var error = GetErrorMessage(request);
                    OnError?.Invoke(error);
                    callback?.Invoke(false, error);
                }
            }
        }

        /// <summary>
        /// Verify new orders and get rewards
        /// </summary>
        public void VerifyOrders(string orderCode = null, Action<IikoOrderResult> callback = null)
        {
            if (!isInitialized)
            {
                callback?.Invoke(null);
                return;
            }

            StartCoroutine(VerifyOrdersCoroutine(orderCode, callback));
        }

        private IEnumerator VerifyOrdersCoroutine(string orderCode, Action<IikoOrderResult> callback)
        {
            var json = JsonUtility.ToJson(new VerifyOrderRequest { orderCode = orderCode });

            using (var request = CreatePostRequest("/api/iiko/verify-order", json))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<VerifyOrderResponse>(request.downloadHandler.text);

                    var result = new IikoOrderResult
                    {
                        Verified = response.verified,
                        Message = response.message,
                        OrdersProcessed = response.ordersProcessed,
                        Rewards = response.totalRewards
                    };

                    if (response.verified && response.totalRewards != null)
                    {
                        ApplyRewards(response.totalRewards);
                        playerIikoData.TotalOrders += response.ordersProcessed;
                    }

                    OnOrderVerified?.Invoke(result);
                    callback?.Invoke(result);

                    Debug.Log($"[Iiko] Verify result: {response.ordersProcessed} orders processed");
                }
                else
                {
                    var error = GetErrorMessage(request);
                    OnError?.Invoke(error);
                    callback?.Invoke(null);
                }
            }
        }

        /// <summary>
        /// Get milestone bonuses
        /// </summary>
        public void GetMilestones(Action<List<IikoMilestone>> callback = null)
        {
            if (!isInitialized)
            {
                callback?.Invoke(new List<IikoMilestone>());
                return;
            }

            StartCoroutine(GetMilestonesCoroutine(callback));
        }

        private IEnumerator GetMilestonesCoroutine(Action<List<IikoMilestone>> callback)
        {
            using (var request = CreateGetRequest("/api/iiko/bonuses"))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<MilestonesResponse>(request.downloadHandler.text);

                    milestones.Clear();

                    // Add available milestones
                    if (response.milestones.available != null)
                    {
                        foreach (var m in response.milestones.available)
                        {
                            milestones.Add(new IikoMilestone
                            {
                                Id = m.id,
                                Name = m.name,
                                NameRu = m.nameRu,
                                Description = m.description,
                                DescriptionRu = m.descriptionRu,
                                OrdersRequired = m.ordersRequired,
                                Rewards = m.rewards,
                                Status = MilestoneStatus.Available
                            });
                        }
                    }

                    // Add progress milestones
                    if (response.milestones.progress != null)
                    {
                        foreach (var m in response.milestones.progress)
                        {
                            milestones.Add(new IikoMilestone
                            {
                                Id = m.id,
                                Name = m.name,
                                NameRu = m.nameRu,
                                Description = m.description,
                                DescriptionRu = m.descriptionRu,
                                OrdersRequired = m.ordersRequired,
                                CurrentProgress = m.currentProgress,
                                ProgressPercent = m.progressPercent,
                                Rewards = m.rewards,
                                Status = MilestoneStatus.InProgress
                            });
                        }
                    }

                    // Add claimed milestones
                    if (response.milestones.claimed != null)
                    {
                        foreach (var m in response.milestones.claimed)
                        {
                            milestones.Add(new IikoMilestone
                            {
                                Id = m.id,
                                Name = m.name,
                                NameRu = m.nameRu,
                                Description = m.description,
                                DescriptionRu = m.descriptionRu,
                                OrdersRequired = m.ordersRequired,
                                Rewards = m.rewards,
                                Status = MilestoneStatus.Claimed
                            });
                        }
                    }

                    playerIikoData.TotalOrders = response.totalOrders;

                    callback?.Invoke(milestones);
                    Debug.Log($"[Iiko] Loaded {milestones.Count} milestones");
                }
                else
                {
                    OnError?.Invoke(GetErrorMessage(request));
                    callback?.Invoke(new List<IikoMilestone>());
                }
            }
        }

        /// <summary>
        /// Claim a milestone reward
        /// </summary>
        public void ClaimMilestone(string milestoneId, Action<bool, IikoMilestone> callback = null)
        {
            if (!isInitialized)
            {
                callback?.Invoke(false, null);
                return;
            }

            StartCoroutine(ClaimMilestoneCoroutine(milestoneId, callback));
        }

        private IEnumerator ClaimMilestoneCoroutine(string milestoneId, Action<bool, IikoMilestone> callback)
        {
            var json = JsonUtility.ToJson(new ClaimMilestoneRequest { milestoneId = milestoneId });

            using (var request = CreatePostRequest("/api/iiko/claim-milestone", json))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<ClaimMilestoneResponse>(request.downloadHandler.text);

                    if (response.success)
                    {
                        // Update local milestone status
                        var milestone = milestones.Find(m => m.Id == milestoneId);
                        if (milestone != null)
                        {
                            milestone.Status = MilestoneStatus.Claimed;
                        }

                        // Apply rewards
                        if (response.rewards != null)
                        {
                            ApplyRewardsFromMilestone(response.rewards);
                        }

                        OnMilestoneClaimed?.Invoke(milestone);
                        callback?.Invoke(true, milestone);

                        Debug.Log($"[Iiko] Milestone claimed: {milestoneId}");
                    }
                    else
                    {
                        callback?.Invoke(false, null);
                    }
                }
                else
                {
                    OnError?.Invoke(GetErrorMessage(request));
                    callback?.Invoke(false, null);
                }
            }
        }

        /// <summary>
        /// Get order history
        /// </summary>
        public void GetOrderHistory(int limit = 20, int offset = 0, Action<List<IikoOrderHistory>> callback = null)
        {
            if (!isInitialized)
            {
                callback?.Invoke(new List<IikoOrderHistory>());
                return;
            }

            StartCoroutine(GetOrderHistoryCoroutine(limit, offset, callback));
        }

        private IEnumerator GetOrderHistoryCoroutine(int limit, int offset, Action<List<IikoOrderHistory>> callback)
        {
            using (var request = CreateGetRequest($"/api/iiko/orders?limit={limit}&offset={offset}"))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<OrderHistoryResponse>(request.downloadHandler.text);

                    orderHistory.Clear();
                    if (response.orders != null)
                    {
                        foreach (var o in response.orders)
                        {
                            orderHistory.Add(new IikoOrderHistory
                            {
                                OrderId = o.orderId,
                                Rewards = o.rewards,
                                ProcessedAt = o.processedAt
                            });
                        }
                    }

                    playerIikoData.TotalOrders = response.stats.totalOrders;
                    playerIikoData.TotalSpent = response.stats.totalSpent;

                    callback?.Invoke(orderHistory);
                }
                else
                {
                    OnError?.Invoke(GetErrorMessage(request));
                    callback?.Invoke(new List<IikoOrderHistory>());
                }
            }
        }

        private IEnumerator LoadPlayerIikoData()
        {
            playerIikoData = new IikoPlayerData();

            // Load milestones to get total orders
            yield return GetMilestonesCoroutine(null);
        }

        private void ApplyRewards(IikoRewards rewards)
        {
            var playerData = GameManager.Instance?.PlayerData;
            if (playerData == null) return;

            if (rewards.coins > 0)
            {
                playerData.Coins += rewards.coins;
            }
            if (rewards.exp > 0)
            {
                GameManager.Instance.AddExperience(rewards.exp);
            }
            if (rewards.gems > 0)
            {
                playerData.Gems += rewards.gems;
            }

            // Unlock recipes
            if (rewards.recipes != null)
            {
                foreach (var recipeId in rewards.recipes)
                {
                    if (!playerData.UnlockedRecipeIds.Contains(recipeId))
                    {
                        playerData.UnlockedRecipeIds.Add(recipeId);
                    }
                }
            }

            SaveSystem.SavePlayerData(playerData);
        }

        private void ApplyRewardsFromMilestone(MilestoneRewards rewards)
        {
            var playerData = GameManager.Instance?.PlayerData;
            if (playerData == null) return;

            if (rewards.coins > 0)
            {
                playerData.Coins += rewards.coins;
            }
            if (rewards.gems > 0)
            {
                playerData.Gems += rewards.gems;
            }

            // Unlock spirit
            if (!string.IsNullOrEmpty(rewards.spiritId))
            {
                if (!playerData.UnlockedSpiritIds.Contains(rewards.spiritId))
                {
                    playerData.UnlockedSpiritIds.Add(rewards.spiritId);
                }
            }

            SaveSystem.SavePlayerData(playerData);
        }

        private UnityWebRequest CreateGetRequest(string endpoint)
        {
            var request = UnityWebRequest.Get(apiBaseUrl + endpoint);
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = (int)requestTimeout;
            return request;
        }

        private UnityWebRequest CreatePostRequest(string endpoint, string json)
        {
            var request = new UnityWebRequest(apiBaseUrl + endpoint, "POST");
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = (int)requestTimeout;
            return request;
        }

        private string GetErrorMessage(UnityWebRequest request)
        {
            try
            {
                var errorResponse = JsonUtility.FromJson<ErrorResponse>(request.downloadHandler.text);
                return errorResponse?.error ?? request.error;
            }
            catch
            {
                return request.error;
            }
        }

        // Public getters
        public List<IikoMilestone> GetCachedMilestones() => milestones;
        public List<IikoOrderHistory> GetCachedOrderHistory() => orderHistory;
        public IikoPlayerData GetPlayerIikoData() => playerIikoData;
    }

    // Data classes

    [Serializable]
    public class IikoPlayerData
    {
        public string LinkedPhone;
        public int TotalOrders;
        public float TotalSpent;
    }

    [Serializable]
    public class IikoMilestone
    {
        public string Id;
        public string Name;
        public string NameRu;
        public string Description;
        public string DescriptionRu;
        public int OrdersRequired;
        public int CurrentProgress;
        public int ProgressPercent;
        public MilestoneRewards Rewards;
        public MilestoneStatus Status;
    }

    public enum MilestoneStatus
    {
        InProgress,
        Available,
        Claimed
    }

    [Serializable]
    public class IikoOrderResult
    {
        public bool Verified;
        public string Message;
        public int OrdersProcessed;
        public IikoRewards Rewards;
    }

    [Serializable]
    public class IikoOrderHistory
    {
        public string OrderId;
        public IikoRewards Rewards;
        public string ProcessedAt;
    }

    [Serializable]
    public class IikoRewards
    {
        public int coins;
        public int exp;
        public int gems;
        public List<string> recipes;
    }

    [Serializable]
    public class MilestoneRewards
    {
        public int coins;
        public int gems;
        public string spiritId;
        public bool exclusive;
    }

    // Request/Response classes

    [Serializable]
    public class PhoneLinkRequest
    {
        public string phone;
    }

    [Serializable]
    public class PhoneLinkResponse
    {
        public bool success;
        public string phone;
        public int importedOrders;
        public IikoRewards rewards;
    }

    [Serializable]
    public class VerifyOrderRequest
    {
        public string orderCode;
    }

    [Serializable]
    public class VerifyOrderResponse
    {
        public bool verified;
        public string message;
        public int ordersProcessed;
        public IikoRewards totalRewards;
    }

    [Serializable]
    public class MilestonesResponse
    {
        public int totalOrders;
        public MilestonesData milestones;
    }

    [Serializable]
    public class MilestonesData
    {
        public MilestoneJson[] available;
        public MilestoneJson[] claimed;
        public MilestoneProgressJson[] progress;
    }

    [Serializable]
    public class MilestoneJson
    {
        public string id;
        public string name;
        public string nameRu;
        public string description;
        public string descriptionRu;
        public int ordersRequired;
        public MilestoneRewards rewards;
    }

    [Serializable]
    public class MilestoneProgressJson : MilestoneJson
    {
        public int currentProgress;
        public int progressPercent;
    }

    [Serializable]
    public class ClaimMilestoneRequest
    {
        public string milestoneId;
    }

    [Serializable]
    public class ClaimMilestoneResponse
    {
        public bool success;
        public MilestoneRewards rewards;
    }

    [Serializable]
    public class OrderHistoryResponse
    {
        public OrderJson[] orders;
        public OrderStats stats;
    }

    [Serializable]
    public class OrderJson
    {
        public string orderId;
        public IikoRewards rewards;
        public string processedAt;
    }

    [Serializable]
    public class OrderStats
    {
        public int totalOrders;
        public float totalSpent;
    }

    [Serializable]
    public class ErrorResponse
    {
        public string error;
    }
}
