using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WaveIsland.Achievements;

namespace WaveIsland.UI.Achievements
{
    /// <summary>
    /// Controls the achievement panel UI
    /// </summary>
    public class AchievementUIController : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private GameObject achievementPanel;
        [SerializeField] private Button closeButton;

        [Header("Header")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Slider overallProgressSlider;
        [SerializeField] private TextMeshProUGUI achievementPointsText;

        [Header("Category Tabs")]
        [SerializeField] private Transform categoryTabContainer;
        [SerializeField] private GameObject categoryTabPrefab;

        [Header("Achievement List")]
        [SerializeField] private Transform achievementListContainer;
        [SerializeField] private GameObject achievementItemPrefab;
        [SerializeField] private ScrollRect achievementScrollRect;

        [Header("Filter Options")]
        [SerializeField] private Toggle showUnlockedToggle;
        [SerializeField] private Toggle showLockedToggle;
        [SerializeField] private Toggle showHiddenToggle;

        [Header("Detail Panel")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private Image detailIcon;
        [SerializeField] private TextMeshProUGUI detailName;
        [SerializeField] private TextMeshProUGUI detailDescription;
        [SerializeField] private TextMeshProUGUI detailProgress;
        [SerializeField] private Slider detailProgressSlider;
        [SerializeField] private TextMeshProUGUI detailRewards;
        [SerializeField] private GameObject detailUnlockedBadge;
        [SerializeField] private Button detailCloseButton;

        [Header("Colors")]
        [SerializeField] private Color unlockedColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f);
        [SerializeField] private Color hiddenColor = new Color(0.3f, 0.3f, 0.3f);
        [SerializeField] private Color tier1Color = new Color(0.7f, 0.7f, 0.7f);
        [SerializeField] private Color tier2Color = new Color(0.3f, 0.8f, 0.3f);
        [SerializeField] private Color tier3Color = new Color(0.3f, 0.5f, 1f);
        [SerializeField] private Color tier4Color = new Color(0.8f, 0.3f, 0.8f);

        // Runtime
        private List<GameObject> categoryTabs = new List<GameObject>();
        private List<GameObject> achievementItems = new List<GameObject>();
        private string currentCategory = "All";
        private AchievementData selectedAchievement;

        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (detailCloseButton != null)
                detailCloseButton.onClick.AddListener(HideDetail);

            // Filter toggles
            if (showUnlockedToggle != null)
                showUnlockedToggle.onValueChanged.AddListener(_ => RefreshList());
            if (showLockedToggle != null)
                showLockedToggle.onValueChanged.AddListener(_ => RefreshList());
            if (showHiddenToggle != null)
                showHiddenToggle.onValueChanged.AddListener(_ => RefreshList());
        }

        private void Start()
        {
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.OnAchievementUnlocked += OnAchievementUnlocked;
                AchievementManager.Instance.OnAchievementProgress += OnAchievementProgress;
            }

            Hide();
            HideDetail();
        }

        private void OnDestroy()
        {
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.OnAchievementUnlocked -= OnAchievementUnlocked;
                AchievementManager.Instance.OnAchievementProgress -= OnAchievementProgress;
            }
        }

        #region Show/Hide

        public void Show()
        {
            if (achievementPanel != null)
            {
                achievementPanel.SetActive(true);
                CreateCategoryTabs();
                RefreshList();
                UpdateHeader();
            }
        }

        public void Hide()
        {
            if (achievementPanel != null)
                achievementPanel.SetActive(false);
        }

        public void Toggle()
        {
            if (achievementPanel != null && achievementPanel.activeSelf)
                Hide();
            else
                Show();
        }

        #endregion

        #region Category Tabs

        private void CreateCategoryTabs()
        {
            // Clear existing
            foreach (var tab in categoryTabs)
            {
                Destroy(tab);
            }
            categoryTabs.Clear();

            if (categoryTabPrefab == null || categoryTabContainer == null) return;

            var manager = AchievementManager.Instance;
            if (manager == null) return;

            // "All" tab
            CreateCategoryTab("All", "Все", null);

            // Category tabs
            foreach (var category in manager.GetCategories())
            {
                CreateCategoryTab(category.id, category.nameRu ?? category.name, category.icon);
            }

            SelectCategory("All");
        }

        private void CreateCategoryTab(string categoryId, string displayName, string iconName)
        {
            GameObject tabObj = Instantiate(categoryTabPrefab, categoryTabContainer);
            categoryTabs.Add(tabObj);

            // Get components
            Button button = tabObj.GetComponent<Button>();
            TextMeshProUGUI text = tabObj.GetComponentInChildren<TextMeshProUGUI>();
            Image icon = tabObj.transform.Find("Icon")?.GetComponent<Image>();

            if (text != null)
            {
                var manager = AchievementManager.Instance;
                if (categoryId == "All")
                {
                    text.text = $"{displayName} ({manager.GetUnlockedCount()}/{manager.GetTotalCount()})";
                }
                else
                {
                    text.text = $"{displayName} ({manager.GetUnlockedCountByCategory(categoryId)}/{manager.GetTotalCountByCategory(categoryId)})";
                }
            }

            if (icon != null && !string.IsNullOrEmpty(iconName))
            {
                Sprite iconSprite = Resources.Load<Sprite>($"Icons/Categories/{iconName}");
                if (iconSprite != null)
                    icon.sprite = iconSprite;
            }

            if (button != null)
            {
                string catId = categoryId; // Capture for closure
                button.onClick.AddListener(() => SelectCategory(catId));
            }
        }

        private void SelectCategory(string categoryId)
        {
            currentCategory = categoryId;

            // Update tab visuals
            for (int i = 0; i < categoryTabs.Count; i++)
            {
                var tab = categoryTabs[i];
                var image = tab.GetComponent<Image>();
                if (image != null)
                {
                    bool isSelected = (i == 0 && categoryId == "All") ||
                                     (i > 0 && AchievementManager.Instance.GetCategories()[i - 1].id == categoryId);
                    image.color = isSelected ? unlockedColor : Color.white;
                }
            }

            RefreshList();
        }

        #endregion

        #region Achievement List

        private void RefreshList()
        {
            // Clear existing
            foreach (var item in achievementItems)
            {
                Destroy(item);
            }
            achievementItems.Clear();

            if (achievementItemPrefab == null || achievementListContainer == null) return;

            var manager = AchievementManager.Instance;
            if (manager == null) return;

            // Get achievements
            List<AchievementData> achievements;
            if (currentCategory == "All")
            {
                achievements = manager.GetAllAchievements();
            }
            else
            {
                achievements = manager.GetAchievementsByCategory(currentCategory);
            }

            // Apply filters
            achievements = FilterAchievements(achievements);

            // Sort: Unlocked first, then by tier, then by name
            achievements.Sort((a, b) =>
            {
                bool aUnlocked = manager.IsUnlocked(a.id);
                bool bUnlocked = manager.IsUnlocked(b.id);

                if (aUnlocked != bUnlocked)
                    return aUnlocked ? -1 : 1;

                if (a.tier != b.tier)
                    return a.tier.CompareTo(b.tier);

                return a.name.CompareTo(b.name);
            });

            // Create items
            foreach (var achievement in achievements)
            {
                CreateAchievementItem(achievement);
            }
        }

        private List<AchievementData> FilterAchievements(List<AchievementData> achievements)
        {
            var manager = AchievementManager.Instance;
            List<AchievementData> filtered = new List<AchievementData>();

            bool showUnlocked = showUnlockedToggle == null || showUnlockedToggle.isOn;
            bool showLocked = showLockedToggle == null || showLockedToggle.isOn;
            bool showHidden = showHiddenToggle != null && showHiddenToggle.isOn;

            foreach (var achievement in achievements)
            {
                bool isUnlocked = manager.IsUnlocked(achievement.id);

                // Hide hidden achievements unless unlocked or filter enabled
                if (achievement.isHidden && !isUnlocked && !showHidden)
                    continue;

                if (isUnlocked && !showUnlocked)
                    continue;

                if (!isUnlocked && !showLocked)
                    continue;

                filtered.Add(achievement);
            }

            return filtered;
        }

        private void CreateAchievementItem(AchievementData achievement)
        {
            GameObject itemObj = Instantiate(achievementItemPrefab, achievementListContainer);
            achievementItems.Add(itemObj);

            var manager = AchievementManager.Instance;
            bool isUnlocked = manager.IsUnlocked(achievement.id);

            // Get components
            Image background = itemObj.GetComponent<Image>();
            Image icon = itemObj.transform.Find("Icon")?.GetComponent<Image>();
            TextMeshProUGUI nameText = itemObj.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI descText = itemObj.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
            Slider progressSlider = itemObj.GetComponentInChildren<Slider>();
            TextMeshProUGUI progressText = itemObj.transform.Find("ProgressText")?.GetComponent<TextMeshProUGUI>();
            GameObject checkmark = itemObj.transform.Find("Checkmark")?.gameObject;
            Button button = itemObj.GetComponent<Button>();

            // Set data
            if (nameText != null)
            {
                if (achievement.isHidden && !isUnlocked)
                    nameText.text = "???";
                else
                    nameText.text = achievement.nameRu ?? achievement.name;
            }

            if (descText != null)
            {
                if (achievement.isHidden && !isUnlocked)
                    descText.text = "Скрытое достижение";
                else if (achievement.isSecret && !isUnlocked)
                    descText.text = "Секретное достижение - совершите открытие!";
                else
                    descText.text = achievement.descriptionRu ?? achievement.description;
            }

            if (icon != null)
            {
                if (achievement.isHidden && !isUnlocked)
                {
                    icon.color = hiddenColor;
                }
                else
                {
                    Sprite iconSprite = Resources.Load<Sprite>($"Icons/Achievements/{achievement.icon}");
                    if (iconSprite != null)
                        icon.sprite = iconSprite;

                    icon.color = isUnlocked ? Color.white : lockedColor;
                }
            }

            // Progress
            if (progressSlider != null)
            {
                if (isUnlocked)
                {
                    progressSlider.gameObject.SetActive(false);
                }
                else if (achievement.isHidden)
                {
                    progressSlider.gameObject.SetActive(false);
                }
                else
                {
                    progressSlider.gameObject.SetActive(true);
                    float progress = manager.GetProgressPercent(achievement.id);
                    progressSlider.value = progress;
                }
            }

            if (progressText != null)
            {
                if (isUnlocked)
                {
                    progressText.text = "Выполнено!";
                    progressText.color = unlockedColor;
                }
                else if (achievement.isHidden)
                {
                    progressText.text = "";
                }
                else
                {
                    int current = manager.GetProgress(achievement.id);
                    progressText.text = $"{current}/{achievement.targetValue}";
                }
            }

            // Checkmark
            if (checkmark != null)
            {
                checkmark.SetActive(isUnlocked);
            }

            // Background color by tier
            if (background != null)
            {
                Color tierColor = GetTierColor(achievement.tier);
                background.color = isUnlocked ? tierColor : new Color(tierColor.r * 0.5f, tierColor.g * 0.5f, tierColor.b * 0.5f, 0.5f);
            }

            // Click handler
            if (button != null)
            {
                AchievementData ach = achievement; // Capture for closure
                button.onClick.AddListener(() => ShowDetail(ach));
            }
        }

        private Color GetTierColor(int tier)
        {
            return tier switch
            {
                1 => tier1Color,
                2 => tier2Color,
                3 => tier3Color,
                4 => tier4Color,
                _ => tier1Color
            };
        }

        #endregion

        #region Header

        private void UpdateHeader()
        {
            var manager = AchievementManager.Instance;
            if (manager == null) return;

            if (titleText != null)
                titleText.text = "Достижения";

            if (progressText != null)
                progressText.text = $"{manager.GetUnlockedCount()} / {manager.GetTotalCount()}";

            if (overallProgressSlider != null)
            {
                float progress = (float)manager.GetUnlockedCount() / manager.GetTotalCount();
                overallProgressSlider.value = progress;
            }

            if (achievementPointsText != null)
            {
                int points = manager.GetTotalAchievementPoints();
                achievementPointsText.text = $"{points} очков";
            }
        }

        #endregion

        #region Detail Panel

        private void ShowDetail(AchievementData achievement)
        {
            if (detailPanel == null) return;

            selectedAchievement = achievement;
            detailPanel.SetActive(true);

            var manager = AchievementManager.Instance;
            bool isUnlocked = manager.IsUnlocked(achievement.id);

            // Icon
            if (detailIcon != null)
            {
                if (achievement.isHidden && !isUnlocked)
                {
                    detailIcon.color = hiddenColor;
                }
                else
                {
                    Sprite iconSprite = Resources.Load<Sprite>($"Icons/Achievements/{achievement.icon}");
                    if (iconSprite != null)
                        detailIcon.sprite = iconSprite;
                    detailIcon.color = isUnlocked ? Color.white : lockedColor;
                }
            }

            // Name
            if (detailName != null)
            {
                if (achievement.isHidden && !isUnlocked)
                    detailName.text = "???";
                else
                    detailName.text = achievement.nameRu ?? achievement.name;
            }

            // Description
            if (detailDescription != null)
            {
                if (achievement.isHidden && !isUnlocked)
                    detailDescription.text = "Это скрытое достижение. Продолжайте играть, чтобы открыть его!";
                else if (achievement.isSecret && !isUnlocked)
                    detailDescription.text = "Секретное достижение - совершите неожиданное открытие!";
                else
                    detailDescription.text = achievement.descriptionRu ?? achievement.description;
            }

            // Progress
            if (detailProgress != null)
            {
                if (isUnlocked)
                {
                    detailProgress.text = "Выполнено!";
                    detailProgress.color = unlockedColor;
                }
                else
                {
                    int current = manager.GetProgress(achievement.id);
                    detailProgress.text = $"Прогресс: {current} / {achievement.targetValue}";
                    detailProgress.color = Color.white;
                }
            }

            if (detailProgressSlider != null)
            {
                float progress = isUnlocked ? 1f : manager.GetProgressPercent(achievement.id);
                detailProgressSlider.value = progress;
            }

            // Rewards
            if (detailRewards != null)
            {
                detailRewards.text = FormatRewards(achievement.rewards);
            }

            // Unlocked badge
            if (detailUnlockedBadge != null)
            {
                detailUnlockedBadge.SetActive(isUnlocked);
            }
        }

        private void HideDetail()
        {
            if (detailPanel != null)
                detailPanel.SetActive(false);
            selectedAchievement = null;
        }

        private string FormatRewards(AchievementRewards rewards)
        {
            if (rewards == null) return "Нет наград";

            List<string> parts = new List<string>();

            if (rewards.coins > 0)
                parts.Add($"<color=#FFD700>{rewards.coins} монет</color>");

            if (rewards.gems > 0)
                parts.Add($"<color=#00BFFF>{rewards.gems} гемов</color>");

            if (rewards.exp > 0)
                parts.Add($"<color=#90EE90>{rewards.exp} опыта</color>");

            if (!string.IsNullOrEmpty(rewards.spiritId))
                parts.Add($"<color=#DDA0DD>Дух: {rewards.spiritId}</color>");

            if (!string.IsNullOrEmpty(rewards.recipeUnlock))
                parts.Add($"<color=#FFA07A>Рецепт: {rewards.recipeUnlock}</color>");

            return parts.Count > 0 ? string.Join("\n", parts) : "Нет наград";
        }

        #endregion

        #region Events

        private void OnAchievementUnlocked(AchievementData achievement)
        {
            // Refresh if panel is open
            if (achievementPanel != null && achievementPanel.activeSelf)
            {
                RefreshList();
                UpdateHeader();
                CreateCategoryTabs(); // Update counts

                // Update detail if viewing this achievement
                if (selectedAchievement != null && selectedAchievement.id == achievement.id)
                {
                    ShowDetail(achievement);
                }
            }
        }

        private void OnAchievementProgress(AchievementData achievement, int current, int target)
        {
            // Refresh if panel is open
            if (achievementPanel != null && achievementPanel.activeSelf)
            {
                RefreshList();

                // Update detail if viewing this achievement
                if (selectedAchievement != null && selectedAchievement.id == achievement.id)
                {
                    ShowDetail(achievement);
                }
            }
        }

        #endregion
    }
}
