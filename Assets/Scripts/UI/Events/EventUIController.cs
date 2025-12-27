using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WaveIsland.Events;

namespace WaveIsland.UI.Events
{
    /// <summary>
    /// UI controller for event panel
    /// </summary>
    public class EventUIController : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject eventPanel;
        [SerializeField] private Button closeButton;

        [Header("No Event State")]
        [SerializeField] private GameObject noEventPanel;
        [SerializeField] private TextMeshProUGUI noEventText;
        [SerializeField] private Transform upcomingEventsContainer;
        [SerializeField] private GameObject upcomingEventPrefab;

        [Header("Event Header")]
        [SerializeField] private GameObject eventContentPanel;
        [SerializeField] private Image bannerImage;
        [SerializeField] private TextMeshProUGUI eventTitleText;
        [SerializeField] private TextMeshProUGUI eventDescriptionText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Image themeAccentImage;

        [Header("Points Progress")]
        [SerializeField] private TextMeshProUGUI pointsText;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Transform rewardTiersContainer;
        [SerializeField] private GameObject rewardTierPrefab;

        [Header("Missions")]
        [SerializeField] private Transform missionsContainer;
        [SerializeField] private GameObject missionPrefab;
        [SerializeField] private Button claimAllButton;
        [SerializeField] private TextMeshProUGUI claimAllText;

        [Header("Colors")]
        [SerializeField] private Color completedColor = new Color(0.3f, 0.8f, 0.3f);
        [SerializeField] private Color claimedColor = new Color(0.5f, 0.5f, 0.5f);
        [SerializeField] private Color lockedTierColor = new Color(0.4f, 0.4f, 0.4f);
        [SerializeField] private Color unlockedTierColor = new Color(1f, 0.84f, 0f);

        // Runtime
        private List<GameObject> missionItems = new List<GameObject>();
        private List<GameObject> tierItems = new List<GameObject>();
        private List<GameObject> upcomingItems = new List<GameObject>();

        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (claimAllButton != null)
                claimAllButton.onClick.AddListener(ClaimAll);
        }

        private void Start()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnEventStarted += OnEventStarted;
                EventManager.Instance.OnEventEnded += OnEventEnded;
                EventManager.Instance.OnMissionProgress += OnMissionProgress;
                EventManager.Instance.OnMissionCompleted += OnMissionCompleted;
                EventManager.Instance.OnEventPointsChanged += OnPointsChanged;
            }

            Hide();
        }

        private void OnDestroy()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnEventStarted -= OnEventStarted;
                EventManager.Instance.OnEventEnded -= OnEventEnded;
                EventManager.Instance.OnMissionProgress -= OnMissionProgress;
                EventManager.Instance.OnMissionCompleted -= OnMissionCompleted;
                EventManager.Instance.OnEventPointsChanged -= OnPointsChanged;
            }
        }

        private void Update()
        {
            if (eventPanel != null && eventPanel.activeSelf)
            {
                UpdateTimer();
            }
        }

        #region Show/Hide

        public void Show()
        {
            if (eventPanel != null)
            {
                eventPanel.SetActive(true);
                RefreshUI();
            }
        }

        public void Hide()
        {
            if (eventPanel != null)
                eventPanel.SetActive(false);
        }

        public void Toggle()
        {
            if (eventPanel != null && eventPanel.activeSelf)
                Hide();
            else
                Show();
        }

        #endregion

        #region UI Refresh

        private void RefreshUI()
        {
            var manager = EventManager.Instance;
            if (manager == null) return;

            var activeEvent = manager.ActiveEvent;

            if (activeEvent != null)
            {
                ShowActiveEvent(activeEvent);
            }
            else
            {
                ShowNoEvent();
            }
        }

        private void ShowNoEvent()
        {
            if (noEventPanel != null)
                noEventPanel.SetActive(true);
            if (eventContentPanel != null)
                eventContentPanel.SetActive(false);

            if (noEventText != null)
                noEventText.text = "Сейчас нет активных событий";

            // Show upcoming events
            ShowUpcomingEvents();
        }

        private void ShowUpcomingEvents()
        {
            ClearUpcomingItems();

            var manager = EventManager.Instance;
            if (manager == null || upcomingEventPrefab == null || upcomingEventsContainer == null)
                return;

            var upcoming = manager.GetUpcomingEvents();

            foreach (var evt in upcoming)
            {
                CreateUpcomingEventItem(evt);
            }
        }

        private void CreateUpcomingEventItem(GameEvent evt)
        {
            GameObject item = Instantiate(upcomingEventPrefab, upcomingEventsContainer);
            upcomingItems.Add(item);

            TextMeshProUGUI nameText = item.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI dateText = item.transform.Find("Date")?.GetComponent<TextMeshProUGUI>();
            Image icon = item.transform.Find("Icon")?.GetComponent<Image>();

            if (nameText != null)
                nameText.text = evt.nameRu;

            if (dateText != null)
            {
                if (System.DateTime.TryParse(evt.startDate, out System.DateTime start))
                {
                    dateText.text = $"Начало: {start:dd.MM}";
                }
            }

            if (icon != null && ColorUtility.TryParseHtmlString(evt.themeColor, out Color color))
            {
                icon.color = color;
            }
        }

        private void ShowActiveEvent(GameEvent evt)
        {
            if (noEventPanel != null)
                noEventPanel.SetActive(false);
            if (eventContentPanel != null)
                eventContentPanel.SetActive(true);

            // Header
            if (eventTitleText != null)
                eventTitleText.text = evt.nameRu;

            if (eventDescriptionText != null)
                eventDescriptionText.text = evt.descriptionRu;

            // Theme color
            if (themeAccentImage != null && ColorUtility.TryParseHtmlString(evt.themeColor, out Color themeColor))
            {
                themeAccentImage.color = themeColor;
            }

            // Banner
            if (bannerImage != null && !string.IsNullOrEmpty(evt.bannerImage))
            {
                Sprite banner = Resources.Load<Sprite>(evt.bannerImage);
                if (banner != null)
                    bannerImage.sprite = banner;
            }

            UpdateTimer();
            UpdatePointsProgress();
            CreateRewardTiers();
            CreateMissionItems();
            UpdateClaimAllButton();
        }

        private void UpdateTimer()
        {
            if (timerText == null) return;

            var manager = EventManager.Instance;
            if (manager == null || manager.ActiveEvent == null)
            {
                timerText.text = "";
                return;
            }

            var remaining = manager.GetTimeRemaining();
            if (remaining.TotalDays >= 1)
            {
                timerText.text = $"Осталось: {(int)remaining.TotalDays}д {remaining.Hours}ч";
            }
            else
            {
                timerText.text = $"Осталось: {remaining.Hours:00}:{remaining.Minutes:00}:{remaining.Seconds:00}";
            }
        }

        private void UpdatePointsProgress()
        {
            var manager = EventManager.Instance;
            if (manager == null || manager.ActiveEvent == null) return;

            int points = manager.EventPoints;
            var tiers = manager.ActiveEvent.rewardTiers;

            if (pointsText != null)
                pointsText.text = $"{points} очков";

            if (progressSlider != null && tiers != null && tiers.Count > 0)
            {
                int maxPoints = tiers[tiers.Count - 1].requiredPoints;
                progressSlider.maxValue = maxPoints;
                progressSlider.value = Mathf.Min(points, maxPoints);
            }
        }

        private void CreateRewardTiers()
        {
            ClearTierItems();

            var manager = EventManager.Instance;
            if (manager == null || manager.ActiveEvent?.rewardTiers == null) return;
            if (rewardTierPrefab == null || rewardTiersContainer == null) return;

            var tiers = manager.ActiveEvent.rewardTiers;

            for (int i = 0; i < tiers.Count; i++)
            {
                CreateTierItem(tiers[i], i);
            }
        }

        private void CreateTierItem(EventRewardTier tier, int index)
        {
            GameObject item = Instantiate(rewardTierPrefab, rewardTiersContainer);
            tierItems.Add(item);

            var manager = EventManager.Instance;
            bool isClaimed = manager.IsTierClaimed(index);
            bool isUnlocked = manager.EventPoints >= tier.requiredPoints;

            Image background = item.GetComponent<Image>();
            TextMeshProUGUI pointsText = item.transform.Find("Points")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI rewardText = item.transform.Find("Reward")?.GetComponent<TextMeshProUGUI>();
            Button claimButton = item.transform.Find("ClaimButton")?.GetComponent<Button>();
            GameObject checkmark = item.transform.Find("Checkmark")?.gameObject;
            GameObject lockIcon = item.transform.Find("LockIcon")?.gameObject;

            // Points required
            if (pointsText != null)
                pointsText.text = $"{tier.requiredPoints}";

            // Rewards text
            if (rewardText != null)
            {
                List<string> rewards = new List<string>();
                if (tier.coins > 0) rewards.Add($"{tier.coins} <sprite name=\"coin\">");
                if (tier.gems > 0) rewards.Add($"{tier.gems} <sprite name=\"gem\">");
                if (!string.IsNullOrEmpty(tier.specialReward)) rewards.Add("+ Бонус!");
                rewardText.text = string.Join(" ", rewards);
            }

            // Visual state
            if (background != null)
            {
                if (isClaimed)
                    background.color = claimedColor;
                else if (isUnlocked)
                    background.color = unlockedTierColor;
                else
                    background.color = lockedTierColor;
            }

            // Claim button
            if (claimButton != null)
            {
                claimButton.gameObject.SetActive(isUnlocked && !isClaimed);
                int tierIndex = index;
                claimButton.onClick.AddListener(() => ClaimTier(tierIndex));
            }

            // Checkmark
            if (checkmark != null)
                checkmark.SetActive(isClaimed);

            // Lock
            if (lockIcon != null)
                lockIcon.SetActive(!isUnlocked && !isClaimed);
        }

        private void CreateMissionItems()
        {
            ClearMissionItems();

            var manager = EventManager.Instance;
            if (manager == null) return;
            if (missionPrefab == null || missionsContainer == null) return;

            var missions = manager.GetActiveMissions();

            // Sort: unclaimed completed first
            missions.Sort((a, b) =>
            {
                if (a.isCompleted && !a.isClaimed && !(b.isCompleted && !b.isClaimed)) return -1;
                if (b.isCompleted && !b.isClaimed && !(a.isCompleted && !a.isClaimed)) return 1;
                if (a.isClaimed && !b.isClaimed) return 1;
                if (b.isClaimed && !a.isClaimed) return -1;
                return 0;
            });

            foreach (var mission in missions)
            {
                CreateMissionItem(mission);
            }
        }

        private void CreateMissionItem(EventMission mission)
        {
            GameObject item = Instantiate(missionPrefab, missionsContainer);
            missionItems.Add(item);

            Image background = item.GetComponent<Image>();
            TextMeshProUGUI nameText = item.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI descText = item.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
            Slider progressSlider = item.GetComponentInChildren<Slider>();
            TextMeshProUGUI progressText = item.transform.Find("ProgressText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI rewardText = item.transform.Find("RewardText")?.GetComponent<TextMeshProUGUI>();
            Button claimButton = item.transform.Find("ClaimButton")?.GetComponent<Button>();
            GameObject checkmark = item.transform.Find("Checkmark")?.gameObject;

            // Name & Description
            if (nameText != null)
                nameText.text = mission.nameRu;

            if (descText != null)
                descText.text = mission.descriptionRu;

            // Progress
            if (progressSlider != null)
            {
                progressSlider.maxValue = mission.targetValue;
                progressSlider.value = mission.currentValue;
                progressSlider.gameObject.SetActive(!mission.isClaimed);
            }

            if (progressText != null)
            {
                if (mission.isClaimed)
                    progressText.text = "Получено!";
                else if (mission.isCompleted)
                    progressText.text = "Выполнено!";
                else
                    progressText.text = $"{mission.currentValue}/{mission.targetValue}";
            }

            // Reward
            if (rewardText != null)
                rewardText.text = $"+{mission.pointsReward} очков";

            // Claim button
            if (claimButton != null)
            {
                claimButton.gameObject.SetActive(mission.isCompleted && !mission.isClaimed);
                string missionId = mission.id;
                claimButton.onClick.AddListener(() => ClaimMission(missionId));
            }

            // Checkmark
            if (checkmark != null)
                checkmark.SetActive(mission.isClaimed);

            // Background color
            if (background != null)
            {
                if (mission.isClaimed)
                    background.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
                else if (mission.isCompleted)
                    background.color = new Color(0.2f, 0.5f, 0.2f, 0.8f);
                else
                    background.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            }
        }

        private void UpdateClaimAllButton()
        {
            if (claimAllButton == null) return;

            var manager = EventManager.Instance;
            bool hasUnclaimed = manager?.HasUnclaimedMissions() ?? false;

            claimAllButton.interactable = hasUnclaimed;

            if (claimAllText != null)
            {
                claimAllText.text = hasUnclaimed ? "Забрать всё" : "Нет наград";
            }
        }

        #endregion

        #region Clear Items

        private void ClearMissionItems()
        {
            foreach (var item in missionItems)
                Destroy(item);
            missionItems.Clear();
        }

        private void ClearTierItems()
        {
            foreach (var item in tierItems)
                Destroy(item);
            tierItems.Clear();
        }

        private void ClearUpcomingItems()
        {
            foreach (var item in upcomingItems)
                Destroy(item);
            upcomingItems.Clear();
        }

        #endregion

        #region Actions

        private void ClaimMission(string missionId)
        {
            var manager = EventManager.Instance;
            if (manager != null && manager.ClaimMissionReward(missionId))
            {
                RefreshUI();
            }
        }

        private void ClaimTier(int tierIndex)
        {
            var manager = EventManager.Instance;
            if (manager != null && manager.ClaimRewardTier(tierIndex))
            {
                RefreshUI();
            }
        }

        private void ClaimAll()
        {
            var manager = EventManager.Instance;
            if (manager == null) return;

            var missions = manager.GetActiveMissions();
            bool anyClaimed = false;

            foreach (var mission in missions)
            {
                if (mission.isCompleted && !mission.isClaimed)
                {
                    if (manager.ClaimMissionReward(mission.id))
                        anyClaimed = true;
                }
            }

            if (anyClaimed)
            {
                RefreshUI();
            }
        }

        #endregion

        #region Event Handlers

        private void OnEventStarted(GameEvent evt)
        {
            if (eventPanel != null && eventPanel.activeSelf)
                RefreshUI();
        }

        private void OnEventEnded(GameEvent evt)
        {
            if (eventPanel != null && eventPanel.activeSelf)
                RefreshUI();
        }

        private void OnMissionProgress(EventMission mission)
        {
            if (eventPanel != null && eventPanel.activeSelf)
                RefreshUI();
        }

        private void OnMissionCompleted(EventMission mission)
        {
            if (eventPanel != null && eventPanel.activeSelf)
                RefreshUI();
        }

        private void OnPointsChanged(int points)
        {
            if (eventPanel != null && eventPanel.activeSelf)
            {
                UpdatePointsProgress();
                CreateRewardTiers();
            }
        }

        #endregion
    }
}
