using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using WaveIsland.Iiko;

namespace WaveIsland.UI.Iiko
{
    /// <summary>
    /// UI controller for iiko integration screen
    /// </summary>
    public class IikoUIController : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject iikoPanel;
        [SerializeField] private Button closeButton;

        [Header("Phone Linking")]
        [SerializeField] private GameObject phoneLinkSection;
        [SerializeField] private InputField phoneInput;
        [SerializeField] private Button linkPhoneButton;
        [SerializeField] private Text linkStatusText;
        [SerializeField] private GameObject linkedPhoneDisplay;
        [SerializeField] private Text linkedPhoneText;

        [Header("Stats Display")]
        [SerializeField] private Text totalOrdersText;
        [SerializeField] private Text totalSpentText;
        [SerializeField] private Slider loyaltyProgressBar;
        [SerializeField] private Text loyaltyLevelText;

        [Header("Verify Orders")]
        [SerializeField] private Button verifyOrdersButton;
        [SerializeField] private InputField orderCodeInput;
        [SerializeField] private Text verifyResultText;
        [SerializeField] private GameObject verifyingIndicator;

        [Header("Milestones")]
        [SerializeField] private Transform milestonesContainer;
        [SerializeField] private GameObject milestonePrefab;
        [SerializeField] private Button refreshMilestonesButton;

        [Header("Order History")]
        [SerializeField] private Transform orderHistoryContainer;
        [SerializeField] private GameObject orderHistoryItemPrefab;
        [SerializeField] private Button loadMoreOrdersButton;
        [SerializeField] private Text noOrdersText;

        [Header("Rewards Display")]
        [SerializeField] private GameObject rewardsPopup;
        [SerializeField] private Text rewardsText;
        [SerializeField] private Button claimRewardsButton;

        [Header("WAVE Info")]
        [SerializeField] private Button openWaveWebsiteButton;
        [SerializeField] private Button openWaveAppButton;
        [SerializeField] private string waveWebsiteUrl = "https://wave.ru";
        [SerializeField] private string waveAppDeepLink = "wave://";

        private List<MilestoneUI> milestoneUIs = new List<MilestoneUI>();
        private int currentOrderOffset = 0;
        private const int ORDERS_PER_PAGE = 10;

        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void InitializeUI()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);

            if (linkPhoneButton != null)
                linkPhoneButton.onClick.AddListener(OnLinkPhoneClicked);

            if (verifyOrdersButton != null)
                verifyOrdersButton.onClick.AddListener(OnVerifyOrdersClicked);

            if (refreshMilestonesButton != null)
                refreshMilestonesButton.onClick.AddListener(RefreshMilestones);

            if (loadMoreOrdersButton != null)
                loadMoreOrdersButton.onClick.AddListener(LoadMoreOrders);

            if (claimRewardsButton != null)
                claimRewardsButton.onClick.AddListener(CloseRewardsPopup);

            if (openWaveWebsiteButton != null)
                openWaveWebsiteButton.onClick.AddListener(OpenWaveWebsite);

            if (openWaveAppButton != null)
                openWaveAppButton.onClick.AddListener(OpenWaveApp);

            // Initial states
            if (rewardsPopup != null)
                rewardsPopup.SetActive(false);

            if (verifyingIndicator != null)
                verifyingIndicator.SetActive(false);
        }

        private void SubscribeToEvents()
        {
            if (IikoManager.Instance != null)
            {
                IikoManager.Instance.OnPhoneLinked += OnPhoneLinked;
                IikoManager.Instance.OnOrderVerified += OnOrderVerified;
                IikoManager.Instance.OnMilestoneClaimed += OnMilestoneClaimed;
                IikoManager.Instance.OnError += OnError;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (IikoManager.Instance != null)
            {
                IikoManager.Instance.OnPhoneLinked -= OnPhoneLinked;
                IikoManager.Instance.OnOrderVerified -= OnOrderVerified;
                IikoManager.Instance.OnMilestoneClaimed -= OnMilestoneClaimed;
                IikoManager.Instance.OnError -= OnError;
            }
        }

        public void Open()
        {
            if (iikoPanel != null)
                iikoPanel.SetActive(true);

            RefreshUI();
            RefreshMilestones();
            LoadOrderHistory();
        }

        public void Close()
        {
            if (iikoPanel != null)
                iikoPanel.SetActive(false);
        }

        private void RefreshUI()
        {
            var playerData = IikoManager.Instance?.GetPlayerIikoData();
            bool isLinked = IikoManager.Instance?.IsPhoneLinked ?? false;

            // Phone linking section
            if (phoneLinkSection != null)
                phoneLinkSection.SetActive(!isLinked);

            if (linkedPhoneDisplay != null)
                linkedPhoneDisplay.SetActive(isLinked);

            if (linkedPhoneText != null && playerData != null)
                linkedPhoneText.text = FormatPhone(playerData.LinkedPhone);

            // Stats
            if (totalOrdersText != null)
                totalOrdersText.text = $"{playerData?.TotalOrders ?? 0}";

            if (totalSpentText != null)
                totalSpentText.text = $"{playerData?.TotalSpent ?? 0:N0} ₽";

            // Loyalty level (based on orders)
            int orders = playerData?.TotalOrders ?? 0;
            int loyaltyLevel = GetLoyaltyLevel(orders);
            int nextLevelOrders = GetNextLevelOrders(loyaltyLevel);

            if (loyaltyLevelText != null)
                loyaltyLevelText.text = GetLoyaltyLevelName(loyaltyLevel);

            if (loyaltyProgressBar != null)
            {
                int prevLevelOrders = GetNextLevelOrders(loyaltyLevel - 1);
                float progress = nextLevelOrders > prevLevelOrders
                    ? (float)(orders - prevLevelOrders) / (nextLevelOrders - prevLevelOrders)
                    : 1f;
                loyaltyProgressBar.value = progress;
            }

            // Verify button state
            if (verifyOrdersButton != null)
                verifyOrdersButton.interactable = isLinked;
        }

        private void OnLinkPhoneClicked()
        {
            string phone = phoneInput?.text?.Trim();

            if (string.IsNullOrEmpty(phone))
            {
                ShowLinkStatus("Please enter your phone number", false);
                return;
            }

            ShowLinkStatus("Linking...", true);
            linkPhoneButton.interactable = false;

            IikoManager.Instance?.LinkPhone(phone, (success, message) =>
            {
                linkPhoneButton.interactable = true;
                ShowLinkStatus(message, success);

                if (success)
                {
                    RefreshUI();
                    RefreshMilestones();
                }
            });
        }

        private void ShowLinkStatus(string message, bool isSuccess)
        {
            if (linkStatusText != null)
            {
                linkStatusText.text = message;
                linkStatusText.color = isSuccess ? Color.green : Color.red;
            }
        }

        private void OnVerifyOrdersClicked()
        {
            string orderCode = orderCodeInput?.text?.Trim();

            if (verifyingIndicator != null)
                verifyingIndicator.SetActive(true);

            if (verifyResultText != null)
                verifyResultText.text = "Checking for new orders...";

            verifyOrdersButton.interactable = false;

            IikoManager.Instance?.VerifyOrders(orderCode, (result) =>
            {
                if (verifyingIndicator != null)
                    verifyingIndicator.SetActive(false);

                verifyOrdersButton.interactable = true;

                if (result != null)
                {
                    if (result.Verified)
                    {
                        ShowRewardsPopup(result);
                        RefreshUI();
                        RefreshMilestones();
                        LoadOrderHistory();
                    }
                    else
                    {
                        if (verifyResultText != null)
                            verifyResultText.text = result.Message ?? "No new orders found";
                    }
                }
            });
        }

        private void RefreshMilestones()
        {
            IikoManager.Instance?.GetMilestones((milestones) =>
            {
                DisplayMilestones(milestones);
            });
        }

        private void DisplayMilestones(List<IikoMilestone> milestones)
        {
            if (milestonesContainer == null) return;

            // Clear existing
            foreach (var ui in milestoneUIs)
            {
                if (ui != null)
                    Destroy(ui.gameObject);
            }
            milestoneUIs.Clear();

            // Sort: available first, then in progress, then claimed
            milestones.Sort((a, b) =>
            {
                int orderA = a.Status == MilestoneStatus.Available ? 0 :
                             a.Status == MilestoneStatus.InProgress ? 1 : 2;
                int orderB = b.Status == MilestoneStatus.Available ? 0 :
                             b.Status == MilestoneStatus.InProgress ? 1 : 2;
                return orderA.CompareTo(orderB);
            });

            foreach (var milestone in milestones)
            {
                CreateMilestoneUI(milestone);
            }
        }

        private void CreateMilestoneUI(IikoMilestone milestone)
        {
            GameObject itemObj;

            if (milestonePrefab != null)
            {
                itemObj = Instantiate(milestonePrefab, milestonesContainer);
            }
            else
            {
                itemObj = CreateBasicMilestoneUI();
                itemObj.transform.SetParent(milestonesContainer);
            }

            var ui = itemObj.GetComponent<MilestoneUI>();
            if (ui == null)
            {
                ui = itemObj.AddComponent<MilestoneUI>();
            }

            ui.Setup(milestone);
            ui.OnClaimClicked += () => OnClaimMilestoneClicked(milestone);

            milestoneUIs.Add(ui);
        }

        private GameObject CreateBasicMilestoneUI()
        {
            var obj = new GameObject("Milestone");

            var bg = obj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            var layout = obj.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);

            var rect = obj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300, 100);

            return obj;
        }

        private void OnClaimMilestoneClicked(IikoMilestone milestone)
        {
            if (milestone.Status != MilestoneStatus.Available) return;

            IikoManager.Instance?.ClaimMilestone(milestone.Id, (success, claimedMilestone) =>
            {
                if (success)
                {
                    RefreshMilestones();
                    RefreshUI();
                }
            });
        }

        private void LoadOrderHistory()
        {
            currentOrderOffset = 0;
            ClearOrderHistory();
            LoadMoreOrders();
        }

        private void ClearOrderHistory()
        {
            if (orderHistoryContainer == null) return;

            foreach (Transform child in orderHistoryContainer)
            {
                Destroy(child.gameObject);
            }
        }

        private void LoadMoreOrders()
        {
            IikoManager.Instance?.GetOrderHistory(ORDERS_PER_PAGE, currentOrderOffset, (orders) =>
            {
                if (orders.Count == 0 && currentOrderOffset == 0)
                {
                    if (noOrdersText != null)
                        noOrdersText.gameObject.SetActive(true);
                    return;
                }

                if (noOrdersText != null)
                    noOrdersText.gameObject.SetActive(false);

                foreach (var order in orders)
                {
                    CreateOrderHistoryItem(order);
                }

                currentOrderOffset += orders.Count;

                if (loadMoreOrdersButton != null)
                    loadMoreOrdersButton.gameObject.SetActive(orders.Count >= ORDERS_PER_PAGE);
            });
        }

        private void CreateOrderHistoryItem(IikoOrderHistory order)
        {
            if (orderHistoryContainer == null) return;

            GameObject itemObj;

            if (orderHistoryItemPrefab != null)
            {
                itemObj = Instantiate(orderHistoryItemPrefab, orderHistoryContainer);
            }
            else
            {
                itemObj = new GameObject("OrderHistory");
                itemObj.transform.SetParent(orderHistoryContainer);
                var text = itemObj.AddComponent<Text>();
                text.text = $"Order: +{order.Rewards?.coins ?? 0} coins";
            }

            var texts = itemObj.GetComponentsInChildren<Text>();
            if (texts.Length >= 2)
            {
                texts[0].text = order.ProcessedAt;
                texts[1].text = $"+{order.Rewards?.coins ?? 0} coins, +{order.Rewards?.exp ?? 0} XP";
            }
        }

        private void ShowRewardsPopup(IikoOrderResult result)
        {
            if (rewardsPopup == null) return;

            rewardsPopup.SetActive(true);

            if (rewardsText != null)
            {
                var rewards = result.Rewards;
                string text = $"<b>{result.OrdersProcessed} order(s) verified!</b>\n\n";

                if (rewards != null)
                {
                    if (rewards.coins > 0)
                        text += $"+{rewards.coins} Coins\n";
                    if (rewards.exp > 0)
                        text += $"+{rewards.exp} XP\n";
                    if (rewards.gems > 0)
                        text += $"+{rewards.gems} Gems\n";
                    if (rewards.recipes != null && rewards.recipes.Count > 0)
                        text += $"+{rewards.recipes.Count} Recipe(s) unlocked!";
                }

                rewardsText.text = text;
            }
        }

        private void CloseRewardsPopup()
        {
            if (rewardsPopup != null)
                rewardsPopup.SetActive(false);
        }

        private void OpenWaveWebsite()
        {
            Application.OpenURL(waveWebsiteUrl);
        }

        private void OpenWaveApp()
        {
            Application.OpenURL(waveAppDeepLink);
        }

        // Event handlers

        private void OnPhoneLinked(string phone)
        {
            RefreshUI();
        }

        private void OnOrderVerified(IikoOrderResult result)
        {
            // Already handled in callback
        }

        private void OnMilestoneClaimed(IikoMilestone milestone)
        {
            Debug.Log($"[IikoUI] Milestone claimed: {milestone.Name}");
        }

        private void OnError(string error)
        {
            Debug.LogError($"[IikoUI] Error: {error}");

            if (verifyResultText != null)
                verifyResultText.text = $"Error: {error}";
        }

        // Utility methods

        private string FormatPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone)) return "";

            // Format as +7 (XXX) XXX-XX-XX
            var clean = phone.Replace("+", "").Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
            if (clean.Length == 11)
            {
                return $"+{clean[0]} ({clean.Substring(1, 3)}) {clean.Substring(4, 3)}-{clean.Substring(7, 2)}-{clean.Substring(9, 2)}";
            }
            return phone;
        }

        private int GetLoyaltyLevel(int orders)
        {
            if (orders >= 25) return 4;
            if (orders >= 10) return 3;
            if (orders >= 5) return 2;
            if (orders >= 1) return 1;
            return 0;
        }

        private int GetNextLevelOrders(int level)
        {
            switch (level)
            {
                case 0: return 1;
                case 1: return 5;
                case 2: return 10;
                case 3: return 25;
                default: return 100;
            }
        }

        private string GetLoyaltyLevelName(int level)
        {
            switch (level)
            {
                case 0: return "Newcomer";
                case 1: return "Regular";
                case 2: return "Loyal";
                case 3: return "VIP";
                case 4: return "Legend";
                default: return "Unknown";
            }
        }
    }

    /// <summary>
    /// UI component for a milestone item
    /// </summary>
    public class MilestoneUI : MonoBehaviour
    {
        [SerializeField] private Text nameText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text rewardText;
        [SerializeField] private Slider progressBar;
        [SerializeField] private Text progressText;
        [SerializeField] private Button claimButton;
        [SerializeField] private Image statusIcon;
        [SerializeField] private GameObject claimedOverlay;

        public event System.Action OnClaimClicked;

        private IikoMilestone milestone;

        private void Awake()
        {
            if (claimButton != null)
                claimButton.onClick.AddListener(() => OnClaimClicked?.Invoke());
        }

        public void Setup(IikoMilestone data)
        {
            milestone = data;

            if (nameText != null)
                nameText.text = data.Name;

            if (descriptionText != null)
                descriptionText.text = data.Description;

            if (rewardText != null)
            {
                string rewards = "";
                if (data.Rewards.coins > 0) rewards += $"+{data.Rewards.coins} coins ";
                if (data.Rewards.gems > 0) rewards += $"+{data.Rewards.gems} gems ";
                if (!string.IsNullOrEmpty(data.Rewards.spiritId)) rewards += $"+ Spirit!";
                rewardText.text = rewards;
            }

            // Progress
            if (progressBar != null)
            {
                progressBar.gameObject.SetActive(data.Status == MilestoneStatus.InProgress);
                if (data.Status == MilestoneStatus.InProgress)
                {
                    progressBar.value = data.ProgressPercent / 100f;
                }
            }

            if (progressText != null)
            {
                if (data.Status == MilestoneStatus.InProgress)
                {
                    progressText.text = $"{data.CurrentProgress}/{data.OrdersRequired}";
                    progressText.gameObject.SetActive(true);
                }
                else
                {
                    progressText.gameObject.SetActive(false);
                }
            }

            // Claim button
            if (claimButton != null)
            {
                claimButton.gameObject.SetActive(data.Status == MilestoneStatus.Available);
            }

            // Claimed overlay
            if (claimedOverlay != null)
            {
                claimedOverlay.SetActive(data.Status == MilestoneStatus.Claimed);
            }

            // Status icon color
            if (statusIcon != null)
            {
                switch (data.Status)
                {
                    case MilestoneStatus.Available:
                        statusIcon.color = Color.yellow;
                        break;
                    case MilestoneStatus.InProgress:
                        statusIcon.color = Color.white;
                        break;
                    case MilestoneStatus.Claimed:
                        statusIcon.color = Color.green;
                        break;
                }
            }
        }
    }
}
