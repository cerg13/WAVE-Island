using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WaveIsland.Quests;

namespace WaveIsland.UI.Quests
{
    /// <summary>
    /// UI controller for daily quests panel
    /// </summary>
    public class DailyQuestUIController : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject questPanel;
        [SerializeField] private Button closeButton;

        [Header("Header")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Slider overallProgressSlider;

        [Header("Quest List")]
        [SerializeField] private Transform questListContainer;
        [SerializeField] private GameObject questItemPrefab;

        [Header("Claim All")]
        [SerializeField] private Button claimAllButton;
        [SerializeField] private TextMeshProUGUI claimAllText;

        [Header("Bonus Reward")]
        [SerializeField] private GameObject bonusRewardPanel;
        [SerializeField] private TextMeshProUGUI bonusRewardText;
        [SerializeField] private Button claimBonusButton;
        [SerializeField] private Image[] bonusStars;

        [Header("Colors")]
        [SerializeField] private Color easyColor = new Color(0.3f, 0.8f, 0.3f);
        [SerializeField] private Color mediumColor = new Color(0.9f, 0.7f, 0.2f);
        [SerializeField] private Color hardColor = new Color(0.9f, 0.3f, 0.3f);
        [SerializeField] private Color completedColor = new Color(0.5f, 0.5f, 0.5f);

        // Runtime
        private List<GameObject> questItems = new List<GameObject>();
        private bool bonusClaimed = false;

        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (claimAllButton != null)
                claimAllButton.onClick.AddListener(ClaimAll);

            if (claimBonusButton != null)
                claimBonusButton.onClick.AddListener(ClaimBonus);
        }

        private void Start()
        {
            if (DailyQuestManager.Instance != null)
            {
                DailyQuestManager.Instance.OnQuestProgress += OnQuestProgress;
                DailyQuestManager.Instance.OnQuestCompleted += OnQuestCompleted;
                DailyQuestManager.Instance.OnQuestsRefreshed += OnQuestsRefreshed;
            }

            Hide();
        }

        private void OnDestroy()
        {
            if (DailyQuestManager.Instance != null)
            {
                DailyQuestManager.Instance.OnQuestProgress -= OnQuestProgress;
                DailyQuestManager.Instance.OnQuestCompleted -= OnQuestCompleted;
                DailyQuestManager.Instance.OnQuestsRefreshed -= OnQuestsRefreshed;
            }
        }

        private void Update()
        {
            if (questPanel != null && questPanel.activeSelf)
            {
                UpdateTimer();
            }
        }

        #region Show/Hide

        public void Show()
        {
            if (questPanel != null)
            {
                questPanel.SetActive(true);
                RefreshUI();
            }
        }

        public void Hide()
        {
            if (questPanel != null)
                questPanel.SetActive(false);
        }

        public void Toggle()
        {
            if (questPanel != null && questPanel.activeSelf)
                Hide();
            else
                Show();
        }

        #endregion

        #region UI Refresh

        private void RefreshUI()
        {
            var manager = DailyQuestManager.Instance;
            if (manager == null) return;

            UpdateHeader();
            CreateQuestItems();
            UpdateClaimAllButton();
            UpdateBonusReward();
        }

        private void UpdateHeader()
        {
            var manager = DailyQuestManager.Instance;
            if (manager == null) return;

            if (titleText != null)
                titleText.text = "Ежедневные задания";

            if (progressText != null)
            {
                int completed = manager.GetCompletedCount();
                int total = manager.GetTotalCount();
                progressText.text = $"{completed}/{total} выполнено";
            }

            if (overallProgressSlider != null)
            {
                float progress = (float)manager.GetCompletedCount() / manager.GetTotalCount();
                overallProgressSlider.value = progress;
            }

            UpdateTimer();
        }

        private void UpdateTimer()
        {
            if (timerText == null) return;

            var manager = DailyQuestManager.Instance;
            if (manager == null) return;

            var timeLeft = manager.GetTimeUntilRefresh();
            timerText.text = $"Обновление через: {timeLeft.Hours:00}:{timeLeft.Minutes:00}:{timeLeft.Seconds:00}";
        }

        private void CreateQuestItems()
        {
            // Clear existing
            foreach (var item in questItems)
            {
                Destroy(item);
            }
            questItems.Clear();

            if (questItemPrefab == null || questListContainer == null) return;

            var manager = DailyQuestManager.Instance;
            if (manager == null) return;

            var quests = manager.GetActiveQuests();

            // Sort: unclaimed completed first, then in progress, then claimed
            quests.Sort((a, b) =>
            {
                if (a.isCompleted && !a.isClaimed && !(b.isCompleted && !b.isClaimed)) return -1;
                if (b.isCompleted && !b.isClaimed && !(a.isCompleted && !a.isClaimed)) return 1;
                if (a.isClaimed && !b.isClaimed) return 1;
                if (b.isClaimed && !a.isClaimed) return -1;
                return 0;
            });

            foreach (var quest in quests)
            {
                CreateQuestItem(quest);
            }
        }

        private void CreateQuestItem(DailyQuest quest)
        {
            GameObject itemObj = Instantiate(questItemPrefab, questListContainer);
            questItems.Add(itemObj);

            // Get components
            Image background = itemObj.GetComponent<Image>();
            Image difficultyBar = itemObj.transform.Find("DifficultyBar")?.GetComponent<Image>();
            TextMeshProUGUI nameText = itemObj.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI descText = itemObj.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
            Slider progressSlider = itemObj.GetComponentInChildren<Slider>();
            TextMeshProUGUI progressText = itemObj.transform.Find("ProgressText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI rewardText = itemObj.transform.Find("RewardText")?.GetComponent<TextMeshProUGUI>();
            Button claimButton = itemObj.transform.Find("ClaimButton")?.GetComponent<Button>();
            GameObject checkmark = itemObj.transform.Find("Checkmark")?.gameObject;

            // Name
            if (nameText != null)
                nameText.text = quest.name;

            // Description
            if (descText != null)
                descText.text = quest.description;

            // Difficulty color
            Color diffColor = GetDifficultyColor(quest.difficulty);
            if (difficultyBar != null)
                difficultyBar.color = diffColor;

            // Progress
            if (progressSlider != null)
            {
                progressSlider.maxValue = quest.targetValue;
                progressSlider.value = quest.currentValue;
                progressSlider.gameObject.SetActive(!quest.isClaimed);
            }

            if (progressText != null)
            {
                if (quest.isClaimed)
                    progressText.text = "Получено!";
                else if (quest.isCompleted)
                    progressText.text = "Выполнено!";
                else
                    progressText.text = $"{quest.currentValue}/{quest.targetValue}";
            }

            // Rewards
            if (rewardText != null)
            {
                List<string> rewards = new List<string>();
                if (quest.coinsReward > 0)
                    rewards.Add($"{quest.coinsReward} <sprite name=\"coin\">");
                if (quest.gemsReward > 0)
                    rewards.Add($"{quest.gemsReward} <sprite name=\"gem\">");
                if (quest.expReward > 0)
                    rewards.Add($"{quest.expReward} XP");
                rewardText.text = string.Join("  ", rewards);
            }

            // Claim button
            if (claimButton != null)
            {
                claimButton.gameObject.SetActive(quest.isCompleted && !quest.isClaimed);
                string templateId = quest.templateId; // Capture for closure
                claimButton.onClick.AddListener(() => ClaimQuest(templateId));
            }

            // Checkmark
            if (checkmark != null)
                checkmark.SetActive(quest.isClaimed);

            // Background
            if (background != null)
            {
                if (quest.isClaimed)
                    background.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
                else if (quest.isCompleted)
                    background.color = new Color(0.2f, 0.5f, 0.2f, 0.8f);
                else
                    background.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            }
        }

        private Color GetDifficultyColor(QuestDifficulty difficulty)
        {
            return difficulty switch
            {
                QuestDifficulty.Easy => easyColor,
                QuestDifficulty.Medium => mediumColor,
                QuestDifficulty.Hard => hardColor,
                _ => easyColor
            };
        }

        private void UpdateClaimAllButton()
        {
            if (claimAllButton == null) return;

            var manager = DailyQuestManager.Instance;
            bool hasUnclaimed = manager?.HasUnclaimedRewards() ?? false;

            claimAllButton.interactable = hasUnclaimed;

            if (claimAllText != null)
            {
                claimAllText.text = hasUnclaimed ? "Забрать всё" : "Нет наград";
            }
        }

        private void UpdateBonusReward()
        {
            if (bonusRewardPanel == null) return;

            var manager = DailyQuestManager.Instance;
            if (manager == null) return;

            int completed = manager.GetClaimedCount();
            int total = manager.GetTotalCount();

            // Show stars for progress
            if (bonusStars != null)
            {
                for (int i = 0; i < bonusStars.Length && i < total; i++)
                {
                    bonusStars[i].color = i < completed ? Color.yellow : Color.gray;
                }
            }

            // Bonus available when all quests completed
            bool bonusAvailable = completed >= total && !bonusClaimed;

            if (claimBonusButton != null)
                claimBonusButton.interactable = bonusAvailable;

            if (bonusRewardText != null)
            {
                if (bonusClaimed)
                    bonusRewardText.text = "Бонус получен!";
                else if (bonusAvailable)
                    bonusRewardText.text = "Бонус доступен!";
                else
                    bonusRewardText.text = $"Выполните все задания ({completed}/{total})";
            }
        }

        #endregion

        #region Actions

        private void ClaimQuest(string templateId)
        {
            var manager = DailyQuestManager.Instance;
            if (manager == null) return;

            if (manager.ClaimReward(templateId))
            {
                RefreshUI();
            }
        }

        private void ClaimAll()
        {
            var manager = DailyQuestManager.Instance;
            if (manager == null) return;

            int claimed = manager.ClaimAllRewards();
            if (claimed > 0)
            {
                RefreshUI();
            }
        }

        private void ClaimBonus()
        {
            if (bonusClaimed) return;

            var manager = DailyQuestManager.Instance;
            if (manager == null) return;

            if (manager.GetClaimedCount() >= manager.GetTotalCount())
            {
                // Grant bonus reward
                var playerData = Core.PlayerData.Instance;
                if (playerData != null)
                {
                    playerData.AddCoins(200);
                    playerData.AddGems(20);
                    playerData.AddExperience(100);
                }

                bonusClaimed = true;
                UpdateBonusReward();

                Debug.Log("DailyQuestUI: Claimed daily bonus reward!");
            }
        }

        #endregion

        #region Events

        private void OnQuestProgress(DailyQuest quest)
        {
            if (questPanel != null && questPanel.activeSelf)
            {
                RefreshUI();
            }
        }

        private void OnQuestCompleted(DailyQuest quest)
        {
            if (questPanel != null && questPanel.activeSelf)
            {
                RefreshUI();
            }
        }

        private void OnQuestsRefreshed()
        {
            bonusClaimed = false;
            if (questPanel != null && questPanel.activeSelf)
            {
                RefreshUI();
            }
        }

        #endregion
    }
}
