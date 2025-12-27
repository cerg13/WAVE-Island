using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WaveIsland.Social;

namespace WaveIsland.UI.Social
{
    /// <summary>
    /// UI controller for leaderboard display
    /// </summary>
    public class LeaderboardUIController : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject leaderboardPanel;
        [SerializeField] private Button closeButton;

        [Header("Tabs")]
        [SerializeField] private Transform tabContainer;
        [SerializeField] private GameObject tabPrefab;

        [Header("Content")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Transform leaderboardListContainer;
        [SerializeField] private GameObject leaderboardEntryPrefab;
        [SerializeField] private ScrollRect scrollRect;

        [Header("Player Info")]
        [SerializeField] private GameObject playerInfoPanel;
        [SerializeField] private TextMeshProUGUI playerRankText;
        [SerializeField] private TextMeshProUGUI playerScoreText;

        [Header("Loading")]
        [SerializeField] private GameObject loadingIndicator;
        [SerializeField] private TextMeshProUGUI errorText;

        [Header("Refresh")]
        [SerializeField] private Button refreshButton;
        [SerializeField] private TextMeshProUGUI lastUpdatedText;

        [Header("Colors")]
        [SerializeField] private Color goldColor = new Color(1f, 0.84f, 0f);
        [SerializeField] private Color silverColor = new Color(0.75f, 0.75f, 0.75f);
        [SerializeField] private Color bronzeColor = new Color(0.8f, 0.5f, 0.2f);
        [SerializeField] private Color currentPlayerColor = new Color(0.3f, 0.6f, 1f, 0.5f);

        // Tab configuration
        private readonly (LeaderboardType type, string name)[] tabs = new[]
        {
            (LeaderboardType.Level, "Уровень"),
            (LeaderboardType.TotalCrafts, "Крафты"),
            (LeaderboardType.RecipesDiscovered, "Рецепты"),
            (LeaderboardType.TotalHarvests, "Урожай"),
            (LeaderboardType.SpiritCollection, "Духи"),
            (LeaderboardType.Weekly, "Неделя")
        };

        // Runtime
        private List<GameObject> tabButtons = new List<GameObject>();
        private List<GameObject> entryItems = new List<GameObject>();
        private LeaderboardType currentType = LeaderboardType.Level;
        private bool isLoading = false;

        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (refreshButton != null)
                refreshButton.onClick.AddListener(Refresh);
        }

        private void Start()
        {
            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.OnLeaderboardLoaded += OnLeaderboardLoaded;
                LeaderboardManager.Instance.OnPlayerRankUpdated += OnPlayerRankUpdated;
                LeaderboardManager.Instance.OnError += OnError;
            }

            CreateTabs();
            Hide();
        }

        private void OnDestroy()
        {
            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.OnLeaderboardLoaded -= OnLeaderboardLoaded;
                LeaderboardManager.Instance.OnPlayerRankUpdated -= OnPlayerRankUpdated;
                LeaderboardManager.Instance.OnError -= OnError;
            }
        }

        #region Show/Hide

        public void Show()
        {
            if (leaderboardPanel != null)
            {
                leaderboardPanel.SetActive(true);
                SelectTab(currentType);
            }
        }

        public void Hide()
        {
            if (leaderboardPanel != null)
                leaderboardPanel.SetActive(false);
        }

        public void Toggle()
        {
            if (leaderboardPanel != null && leaderboardPanel.activeSelf)
                Hide();
            else
                Show();
        }

        #endregion

        #region Tabs

        private void CreateTabs()
        {
            foreach (var tab in tabButtons)
            {
                Destroy(tab);
            }
            tabButtons.Clear();

            if (tabPrefab == null || tabContainer == null) return;

            foreach (var (type, name) in tabs)
            {
                CreateTab(type, name);
            }
        }

        private void CreateTab(LeaderboardType type, string name)
        {
            GameObject tabObj = Instantiate(tabPrefab, tabContainer);
            tabButtons.Add(tabObj);

            TextMeshProUGUI text = tabObj.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.text = name;

            Button button = tabObj.GetComponent<Button>();
            if (button != null)
            {
                LeaderboardType tabType = type; // Capture for closure
                button.onClick.AddListener(() => SelectTab(tabType));
            }
        }

        private void SelectTab(LeaderboardType type)
        {
            currentType = type;
            UpdateTabVisuals();
            LoadLeaderboard(type);
        }

        private void UpdateTabVisuals()
        {
            for (int i = 0; i < tabButtons.Count && i < tabs.Length; i++)
            {
                var button = tabButtons[i].GetComponent<Button>();
                var image = tabButtons[i].GetComponent<Image>();

                if (image != null)
                {
                    bool isSelected = tabs[i].type == currentType;
                    image.color = isSelected ? Color.white : new Color(0.7f, 0.7f, 0.7f);
                }
            }
        }

        #endregion

        #region Load Leaderboard

        private void LoadLeaderboard(LeaderboardType type)
        {
            ShowLoading(true);

            if (titleText != null)
            {
                string title = GetLeaderboardTitle(type);
                titleText.text = title;
            }

            var manager = LeaderboardManager.Instance;
            if (manager != null)
            {
                manager.GetLeaderboard(type);
            }
            else
            {
                // Offline mode - show mock data
                ShowMockData(type);
            }
        }

        private void Refresh()
        {
            LoadLeaderboard(currentType);
        }

        private string GetLeaderboardTitle(LeaderboardType type)
        {
            return type switch
            {
                LeaderboardType.Level => "Рейтинг по уровню",
                LeaderboardType.TotalCrafts => "Больше всего крафтов",
                LeaderboardType.RecipesDiscovered => "Открыто рецептов",
                LeaderboardType.TotalHarvests => "Собрано урожая",
                LeaderboardType.SpiritCollection => "Коллекция духов",
                LeaderboardType.Weekly => "Лучшие за неделю",
                _ => "Лидерборд"
            };
        }

        #endregion

        #region Display

        private void DisplayLeaderboard(LeaderboardData data)
        {
            ClearEntries();

            if (data.entries == null || data.entries.Count == 0)
            {
                ShowError("Нет данных");
                return;
            }

            foreach (var entry in data.entries)
            {
                CreateEntryItem(entry);
            }

            // Player info
            UpdatePlayerInfo(data);

            // Last updated
            if (lastUpdatedText != null)
            {
                lastUpdatedText.text = $"Обновлено: {data.lastUpdated:HH:mm}";
            }

            ShowLoading(false);
        }

        private void CreateEntryItem(LeaderboardEntry entry)
        {
            if (leaderboardEntryPrefab == null || leaderboardListContainer == null) return;

            GameObject itemObj = Instantiate(leaderboardEntryPrefab, leaderboardListContainer);
            entryItems.Add(itemObj);

            // Get components
            Image background = itemObj.GetComponent<Image>();
            TextMeshProUGUI rankText = itemObj.transform.Find("Rank")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI nameText = itemObj.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI scoreText = itemObj.transform.Find("Score")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI levelText = itemObj.transform.Find("Level")?.GetComponent<TextMeshProUGUI>();
            Image avatar = itemObj.transform.Find("Avatar")?.GetComponent<Image>();
            Image rankIcon = itemObj.transform.Find("RankIcon")?.GetComponent<Image>();

            // Rank
            if (rankText != null)
            {
                rankText.text = $"#{entry.rank}";
                rankText.color = GetRankColor(entry.rank);
            }

            // Rank icon (top 3)
            if (rankIcon != null)
            {
                rankIcon.gameObject.SetActive(entry.rank <= 3);
                rankIcon.color = GetRankColor(entry.rank);
            }

            // Name
            if (nameText != null)
            {
                nameText.text = entry.displayName;
                if (entry.isCurrentPlayer)
                    nameText.text += " (Вы)";
            }

            // Score
            if (scoreText != null)
                scoreText.text = FormatScore(entry.score);

            // Level
            if (levelText != null)
                levelText.text = $"Ур. {entry.level}";

            // Background for current player
            if (background != null && entry.isCurrentPlayer)
            {
                background.color = currentPlayerColor;
            }
        }

        private Color GetRankColor(int rank)
        {
            return rank switch
            {
                1 => goldColor,
                2 => silverColor,
                3 => bronzeColor,
                _ => Color.white
            };
        }

        private string FormatScore(int score)
        {
            if (score >= 1000000)
                return $"{score / 1000000f:F1}M";
            if (score >= 1000)
                return $"{score / 1000f:F1}K";
            return score.ToString();
        }

        private void UpdatePlayerInfo(LeaderboardData data)
        {
            if (playerInfoPanel == null) return;

            if (data.playerRank > 0)
            {
                playerInfoPanel.SetActive(true);

                if (playerRankText != null)
                    playerRankText.text = $"Ваш ранг: #{data.playerRank}";

                // Find player entry for score
                var playerEntry = data.entries?.Find(e => e.isCurrentPlayer);
                if (playerEntry != null && playerScoreText != null)
                {
                    playerScoreText.text = $"Очки: {FormatScore(playerEntry.score)}";
                }
            }
            else
            {
                playerInfoPanel.SetActive(false);
            }
        }

        private void ClearEntries()
        {
            foreach (var item in entryItems)
            {
                Destroy(item);
            }
            entryItems.Clear();
        }

        #endregion

        #region Loading/Error

        private void ShowLoading(bool show)
        {
            isLoading = show;

            if (loadingIndicator != null)
                loadingIndicator.SetActive(show);

            if (errorText != null)
                errorText.gameObject.SetActive(false);

            if (refreshButton != null)
                refreshButton.interactable = !show;
        }

        private void ShowError(string message)
        {
            ShowLoading(false);

            if (errorText != null)
            {
                errorText.gameObject.SetActive(true);
                errorText.text = message;
            }
        }

        #endregion

        #region Mock Data (Offline)

        private void ShowMockData(LeaderboardType type)
        {
            List<LeaderboardEntry> mockEntries = new List<LeaderboardEntry>();

            string[] names = { "ТропическийМастер", "КоктейльКинг", "ОстровнойПовар", "БарменПро", "МохитоГуру" };

            for (int i = 0; i < 10; i++)
            {
                bool isPlayer = i == 4;
                mockEntries.Add(new LeaderboardEntry
                {
                    rank = i + 1,
                    displayName = isPlayer ? "Вы" : names[i % names.Length] + (i / 5 + 1),
                    score = 1000 - (i * 80) + UnityEngine.Random.Range(-20, 20),
                    level = 15 - i / 2,
                    isCurrentPlayer = isPlayer
                });
            }

            LeaderboardData mockData = new LeaderboardData
            {
                type = type,
                entries = mockEntries,
                playerRank = 5,
                totalPlayers = 1250,
                lastUpdated = System.DateTime.Now
            };

            DisplayLeaderboard(mockData);
        }

        #endregion

        #region Events

        private void OnLeaderboardLoaded(LeaderboardData data)
        {
            if (data.type == currentType)
            {
                DisplayLeaderboard(data);
            }
        }

        private void OnPlayerRankUpdated(int rank)
        {
            if (playerRankText != null)
            {
                playerRankText.text = $"Ваш ранг: #{rank}";
            }
        }

        private void OnError(string error)
        {
            ShowError(error);
        }

        #endregion
    }
}
