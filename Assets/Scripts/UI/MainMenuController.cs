using System;
using UnityEngine;
using UnityEngine.UI;
using WaveIsland.Core;
using WaveIsland.Localization;

namespace WaveIsland.UI
{
    /// <summary>
    /// Main menu controller - handles navigation to all game areas
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject profilePanel;
        [SerializeField] private GameObject dailyRewardPanel;

        [Header("Navigation Buttons")]
        [SerializeField] private Button gardenButton;
        [SerializeField] private Button alchemyButton;
        [SerializeField] private Button shopButton;
        [SerializeField] private Button spiritsButton;
        [SerializeField] private Button achievementsButton;
        [SerializeField] private Button leaderboardButton;
        [SerializeField] private Button questsButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button iikoButton;

        [Header("Profile UI")]
        [SerializeField] private Text playerNameText;
        [SerializeField] private Text levelText;
        [SerializeField] private Text coinsText;
        [SerializeField] private Text gemsText;
        [SerializeField] private Image experienceBar;
        [SerializeField] private Image avatarImage;

        [Header("Daily Info")]
        [SerializeField] private Text dailyQuestCountText;
        [SerializeField] private Text notificationBadge;
        [SerializeField] private GameObject newContentIndicator;

        [Header("Event Banner")]
        [SerializeField] private GameObject eventBanner;
        [SerializeField] private Text eventTitleText;
        [SerializeField] private Text eventTimeText;
        [SerializeField] private Button eventButton;

        [Header("Audio")]
        [SerializeField] private AudioSource buttonClickSound;

        private PlayerData playerData;
        private bool initialized = false;

        private void Start()
        {
            Initialize();
        }

        private void OnEnable()
        {
            if (initialized)
            {
                RefreshUI();
            }
        }

        private void Initialize()
        {
            // Get player data
            playerData = GameManager.Instance?.GetPlayerData();

            if (playerData == null)
            {
                Debug.LogError("MainMenuController: PlayerData not found!");
                return;
            }

            // Setup button listeners
            SetupButtons();

            // Initial UI refresh
            RefreshUI();

            // Check daily login
            CheckDailyLogin();

            // Check active events
            CheckActiveEvents();

            // Mark as initialized
            initialized = true;

            Debug.Log("MainMenuController initialized");
        }

        private void SetupButtons()
        {
            if (gardenButton != null)
                gardenButton.onClick.AddListener(() => OnNavigate("Garden"));

            if (alchemyButton != null)
                alchemyButton.onClick.AddListener(() => OnNavigate("Alchemy"));

            if (shopButton != null)
                shopButton.onClick.AddListener(() => OnNavigate("Shop"));

            if (spiritsButton != null)
                spiritsButton.onClick.AddListener(() => OnNavigate("Spirits"));

            if (achievementsButton != null)
                achievementsButton.onClick.AddListener(() => OnNavigate("Achievements"));

            if (leaderboardButton != null)
                leaderboardButton.onClick.AddListener(() => OnNavigate("Leaderboard"));

            if (questsButton != null)
                questsButton.onClick.AddListener(() => OnNavigate("Quests"));

            if (settingsButton != null)
                settingsButton.onClick.AddListener(() => OnNavigate("Settings"));

            if (iikoButton != null)
                iikoButton.onClick.AddListener(() => OnNavigate("Iiko"));

            if (eventButton != null)
                eventButton.onClick.AddListener(OnEventBannerClick);
        }

        private void RefreshUI()
        {
            if (playerData == null) return;

            // Update player info
            UpdatePlayerInfo();

            // Update resources
            UpdateResources();

            // Update daily info
            UpdateDailyInfo();

            // Update notifications
            UpdateNotifications();
        }

        private void UpdatePlayerInfo()
        {
            if (playerNameText != null)
                playerNameText.text = playerData.playerName;

            if (levelText != null)
                levelText.text = $"{L.Get("common.level")} {playerData.level}";

            if (experienceBar != null)
            {
                float expProgress = (float)playerData.experience / playerData.GetExperienceForNextLevel();
                experienceBar.fillAmount = expProgress;
            }

            // Avatar can be set based on player preference
            if (avatarImage != null && !string.IsNullOrEmpty(playerData.avatarUrl))
            {
                // Load avatar sprite
                // TODO: Implement avatar loading from Resources or URL
            }
        }

        private void UpdateResources()
        {
            if (coinsText != null)
                coinsText.text = FormatNumber(playerData.coins);

            if (gemsText != null)
                gemsText.text = FormatNumber(playerData.gems);
        }

        private void UpdateDailyInfo()
        {
            // Get daily quest count
            // TODO: Get from DailyQuestManager
            if (dailyQuestCountText != null)
            {
                dailyQuestCountText.text = "3/5"; // Example
            }
        }

        private void UpdateNotifications()
        {
            int totalNotifications = 0;

            // Count unclaimed rewards, ready harvests, etc.
            // TODO: Implement notification counting

            if (notificationBadge != null)
            {
                if (totalNotifications > 0)
                {
                    notificationBadge.gameObject.SetActive(true);
                    notificationBadge.text = totalNotifications > 99 ? "99+" : totalNotifications.ToString();
                }
                else
                {
                    notificationBadge.gameObject.SetActive(false);
                }
            }
        }

        private void CheckDailyLogin()
        {
            DateTime lastLogin = playerData.lastLoginTime;
            DateTime now = DateTime.Now;

            if (lastLogin.Date < now.Date)
            {
                // New day - show daily reward
                if (dailyRewardPanel != null)
                {
                    dailyRewardPanel.SetActive(true);
                }

                // Update login streak
                if ((now - lastLogin).TotalDays == 1)
                {
                    // Consecutive day
                    playerData.loginStreak++;
                }
                else
                {
                    // Streak broken
                    playerData.loginStreak = 1;
                }

                playerData.lastLoginTime = now;
                playerData.totalLogins++;

                // Save
                SaveManager.Instance?.SavePlayerData(playerData);
            }
        }

        private void CheckActiveEvents()
        {
            // TODO: Get from EventManager
            // For now, hide event banner
            if (eventBanner != null)
            {
                eventBanner.SetActive(false);
            }
        }

        private void OnNavigate(string destination)
        {
            PlayButtonSound();

            Debug.Log($"Navigating to: {destination}");

            // Use SceneLoader for scene transitions
            switch (destination)
            {
                case "Garden":
                    SceneLoader.Instance?.LoadScene("Game", SceneLoader.SceneArea.Garden);
                    break;
                case "Alchemy":
                    SceneLoader.Instance?.LoadScene("Game", SceneLoader.SceneArea.Alchemy);
                    break;
                case "Shop":
                    UIManager.Instance?.ShowShop();
                    break;
                case "Spirits":
                    UIManager.Instance?.ShowSpirits();
                    break;
                case "Achievements":
                    UIManager.Instance?.ShowAchievements();
                    break;
                case "Leaderboard":
                    UIManager.Instance?.ShowLeaderboard();
                    break;
                case "Quests":
                    UIManager.Instance?.ShowQuests();
                    break;
                case "Settings":
                    UIManager.Instance?.ShowSettings();
                    break;
                case "Iiko":
                    UIManager.Instance?.ShowIiko();
                    break;
            }
        }

        private void OnEventBannerClick()
        {
            PlayButtonSound();
            // TODO: Open event UI
            Debug.Log("Event banner clicked");
        }

        public void OnProfileClick()
        {
            PlayButtonSound();

            if (profilePanel != null)
            {
                profilePanel.SetActive(!profilePanel.activeSelf);
            }
        }

        public void OnDailyRewardClaim()
        {
            PlayButtonSound();

            // TODO: Implement daily reward claim
            if (dailyRewardPanel != null)
            {
                dailyRewardPanel.SetActive(false);
            }
        }

        private void PlayButtonSound()
        {
            if (buttonClickSound != null)
            {
                buttonClickSound.Play();
            }
            else
            {
                // Fallback to AudioManager
                Audio.AudioManager.Instance?.PlayButtonClick();
            }
        }

        private string FormatNumber(int number)
        {
            if (number >= 1000000)
                return $"{number / 1000000f:F1}M";
            if (number >= 1000)
                return $"{number / 1000f:F1}K";
            return number.ToString();
        }

        #region Public Methods

        public void RefreshPlayerData()
        {
            playerData = GameManager.Instance?.GetPlayerData();
            RefreshUI();
        }

        public void ShowNotification(string message, float duration = 3f)
        {
            // TODO: Show toast notification
            Debug.Log($"Notification: {message}");
        }

        #endregion

        #region Debug

        [ContextMenu("Debug: Add 100 Coins")]
        private void DebugAddCoins()
        {
            if (playerData != null)
            {
                playerData.coins += 100;
                UpdateResources();
            }
        }

        [ContextMenu("Debug: Add 10 Gems")]
        private void DebugAddGems()
        {
            if (playerData != null)
            {
                playerData.gems += 10;
                UpdateResources();
            }
        }

        [ContextMenu("Debug: Level Up")]
        private void DebugLevelUp()
        {
            if (playerData != null)
            {
                playerData.level++;
                playerData.experience = 0;
                UpdatePlayerInfo();
            }
        }

        #endregion
    }
}
