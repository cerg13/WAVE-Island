using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WaveIsland.Achievements;

namespace WaveIsland.UI.Achievements
{
    /// <summary>
    /// UI component for a single achievement item in the list
    /// </summary>
    public class AchievementItemUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image tierIndicator;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private GameObject checkmarkIcon;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private GameObject secretIcon;
        [SerializeField] private Button button;

        [Header("Reward Icons")]
        [SerializeField] private GameObject rewardsContainer;
        [SerializeField] private GameObject coinRewardIcon;
        [SerializeField] private TextMeshProUGUI coinRewardText;
        [SerializeField] private GameObject gemRewardIcon;
        [SerializeField] private TextMeshProUGUI gemRewardText;
        [SerializeField] private GameObject expRewardIcon;
        [SerializeField] private TextMeshProUGUI expRewardText;

        [Header("Colors")]
        [SerializeField] private Color unlockedBgColor = new Color(0.2f, 0.3f, 0.2f);
        [SerializeField] private Color lockedBgColor = new Color(0.15f, 0.15f, 0.15f);
        [SerializeField] private Color hiddenBgColor = new Color(0.1f, 0.1f, 0.1f);

        private AchievementData achievementData;
        private System.Action<AchievementData> onClickCallback;

        public void Setup(AchievementData achievement, bool isUnlocked, int currentProgress, System.Action<AchievementData> onClick)
        {
            achievementData = achievement;
            onClickCallback = onClick;

            bool isHidden = achievement.isHidden && !isUnlocked;
            bool isSecret = achievement.isSecret && !isUnlocked;

            // Background
            if (backgroundImage != null)
            {
                if (isUnlocked)
                    backgroundImage.color = unlockedBgColor;
                else if (isHidden)
                    backgroundImage.color = hiddenBgColor;
                else
                    backgroundImage.color = lockedBgColor;
            }

            // Icon
            if (iconImage != null)
            {
                if (isHidden)
                {
                    iconImage.enabled = false;
                }
                else
                {
                    iconImage.enabled = true;
                    Sprite iconSprite = Resources.Load<Sprite>($"Icons/Achievements/{achievement.icon}");
                    if (iconSprite != null)
                        iconImage.sprite = iconSprite;

                    iconImage.color = isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f);
                }
            }

            // Tier indicator
            if (tierIndicator != null)
            {
                tierIndicator.color = GetTierColor(achievement.tier);
            }

            // Name
            if (nameText != null)
            {
                if (isHidden)
                    nameText.text = "???";
                else
                    nameText.text = achievement.nameRu ?? achievement.name;

                nameText.color = isUnlocked ? Color.white : new Color(0.7f, 0.7f, 0.7f);
            }

            // Description
            if (descriptionText != null)
            {
                if (isHidden)
                    descriptionText.text = "Скрытое достижение";
                else if (isSecret)
                    descriptionText.text = "Совершите неожиданное открытие...";
                else
                    descriptionText.text = achievement.descriptionRu ?? achievement.description;
            }

            // Progress
            SetupProgress(achievement, isUnlocked, isHidden, currentProgress);

            // Status icons
            if (checkmarkIcon != null)
                checkmarkIcon.SetActive(isUnlocked);

            if (lockIcon != null)
                lockIcon.SetActive(!isUnlocked && !isHidden);

            if (secretIcon != null)
                secretIcon.SetActive(isSecret);

            // Rewards
            SetupRewards(achievement.rewards, isHidden);

            // Button
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnClick);
            }
        }

        private void SetupProgress(AchievementData achievement, bool isUnlocked, bool isHidden, int currentProgress)
        {
            if (progressSlider != null)
            {
                if (isUnlocked || isHidden || achievement.targetValue <= 1)
                {
                    progressSlider.gameObject.SetActive(false);
                }
                else
                {
                    progressSlider.gameObject.SetActive(true);
                    progressSlider.maxValue = achievement.targetValue;
                    progressSlider.value = currentProgress;
                }
            }

            if (progressText != null)
            {
                if (isUnlocked)
                {
                    progressText.text = "<color=#4CAF50>Выполнено!</color>";
                }
                else if (isHidden)
                {
                    progressText.text = "";
                }
                else if (achievement.targetValue <= 1)
                {
                    progressText.text = "";
                }
                else
                {
                    float percent = (float)currentProgress / achievement.targetValue * 100f;
                    progressText.text = $"{currentProgress}/{achievement.targetValue} ({percent:F0}%)";
                }
            }
        }

        private void SetupRewards(AchievementRewards rewards, bool isHidden)
        {
            if (rewardsContainer != null)
                rewardsContainer.SetActive(!isHidden && rewards != null);

            if (rewards == null || isHidden) return;

            // Coins
            if (coinRewardIcon != null)
            {
                bool hasCoins = rewards.coins > 0;
                coinRewardIcon.SetActive(hasCoins);
                if (coinRewardText != null && hasCoins)
                    coinRewardText.text = rewards.coins.ToString();
            }

            // Gems
            if (gemRewardIcon != null)
            {
                bool hasGems = rewards.gems > 0;
                gemRewardIcon.SetActive(hasGems);
                if (gemRewardText != null && hasGems)
                    gemRewardText.text = rewards.gems.ToString();
            }

            // Exp
            if (expRewardIcon != null)
            {
                bool hasExp = rewards.exp > 0;
                expRewardIcon.SetActive(hasExp);
                if (expRewardText != null && hasExp)
                    expRewardText.text = rewards.exp.ToString();
            }
        }

        private Color GetTierColor(int tier)
        {
            return tier switch
            {
                1 => new Color(0.7f, 0.7f, 0.7f),      // Gray
                2 => new Color(0.3f, 0.8f, 0.3f),      // Green
                3 => new Color(0.3f, 0.5f, 1f),        // Blue
                4 => new Color(0.8f, 0.3f, 0.8f),      // Purple
                _ => new Color(0.7f, 0.7f, 0.7f)
            };
        }

        private void OnClick()
        {
            onClickCallback?.Invoke(achievementData);
        }

        /// <summary>
        /// Update progress without full refresh
        /// </summary>
        public void UpdateProgress(int newProgress)
        {
            if (achievementData == null) return;

            if (progressSlider != null && progressSlider.gameObject.activeSelf)
            {
                progressSlider.value = newProgress;
            }

            if (progressText != null && !string.IsNullOrEmpty(progressText.text))
            {
                float percent = (float)newProgress / achievementData.targetValue * 100f;
                progressText.text = $"{newProgress}/{achievementData.targetValue} ({percent:F0}%)";
            }
        }

        /// <summary>
        /// Play unlock animation
        /// </summary>
        public void PlayUnlockAnimation()
        {
            // Simple scale animation
            LeanTween.cancel(gameObject);
            transform.localScale = Vector3.one * 1.1f;
            LeanTween.scale(gameObject, Vector3.one, 0.3f).setEaseOutBack();

            // Update visuals
            if (checkmarkIcon != null)
                checkmarkIcon.SetActive(true);

            if (lockIcon != null)
                lockIcon.SetActive(false);

            if (backgroundImage != null)
                backgroundImage.color = unlockedBgColor;

            if (iconImage != null)
                iconImage.color = Color.white;

            if (nameText != null)
                nameText.color = Color.white;

            if (progressText != null)
                progressText.text = "<color=#4CAF50>Выполнено!</color>";

            if (progressSlider != null)
                progressSlider.gameObject.SetActive(false);
        }
    }
}
