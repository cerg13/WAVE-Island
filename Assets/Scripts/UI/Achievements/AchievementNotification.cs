using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WaveIsland.Achievements;

namespace WaveIsland.UI.Achievements
{
    /// <summary>
    /// Shows popup notifications when achievements are unlocked
    /// </summary>
    public class AchievementNotification : MonoBehaviour
    {
        public static AchievementNotification Instance { get; private set; }

        [Header("Notification Panel")]
        [SerializeField] private GameObject notificationPanel;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform panelTransform;

        [Header("Content")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI rewardsText;
        [SerializeField] private Image tierBorder;

        [Header("Animation")]
        [SerializeField] private float slideInDuration = 0.5f;
        [SerializeField] private float displayDuration = 3f;
        [SerializeField] private float slideOutDuration = 0.3f;
        [SerializeField] private float slideDistance = 200f;
        [SerializeField] private AnimationCurve slideInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve slideOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Effects")]
        [SerializeField] private ParticleSystem celebrationParticles;
        [SerializeField] private AudioClip unlockSound;
        [SerializeField] private AudioClip legendaryUnlockSound;

        [Header("Colors")]
        [SerializeField] private Color tier1Color = new Color(0.7f, 0.7f, 0.7f);
        [SerializeField] private Color tier2Color = new Color(0.3f, 0.8f, 0.3f);
        [SerializeField] private Color tier3Color = new Color(0.3f, 0.5f, 1f);
        [SerializeField] private Color tier4Color = new Color(0.8f, 0.3f, 0.8f);

        // Queue for multiple achievements
        private Queue<AchievementData> pendingNotifications = new Queue<AchievementData>();
        private bool isShowingNotification = false;
        private Vector2 originalPosition;
        private AudioSource audioSource;

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
                return;
            }

            if (panelTransform != null)
            {
                originalPosition = panelTransform.anchoredPosition;
            }

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            HideImmediate();
        }

        private void Start()
        {
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.OnAchievementUnlocked += QueueNotification;
            }
        }

        private void OnDestroy()
        {
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.OnAchievementUnlocked -= QueueNotification;
            }
        }

        #region Queue Management

        public void QueueNotification(AchievementData achievement)
        {
            pendingNotifications.Enqueue(achievement);

            if (!isShowingNotification)
            {
                ShowNextNotification();
            }
        }

        private void ShowNextNotification()
        {
            if (pendingNotifications.Count == 0)
            {
                isShowingNotification = false;
                return;
            }

            isShowingNotification = true;
            AchievementData achievement = pendingNotifications.Dequeue();
            StartCoroutine(ShowNotificationRoutine(achievement));
        }

        #endregion

        #region Display

        private IEnumerator ShowNotificationRoutine(AchievementData achievement)
        {
            SetupContent(achievement);

            // Play effects
            PlayEffects(achievement);

            // Slide in
            yield return StartCoroutine(SlideIn());

            // Display
            yield return new WaitForSeconds(displayDuration);

            // Slide out
            yield return StartCoroutine(SlideOut());

            // Show next if queued
            ShowNextNotification();
        }

        private void SetupContent(AchievementData achievement)
        {
            if (notificationPanel != null)
                notificationPanel.SetActive(true);

            if (headerText != null)
                headerText.text = "Достижение разблокировано!";

            if (nameText != null)
                nameText.text = achievement.nameRu ?? achievement.name;

            if (iconImage != null)
            {
                Sprite iconSprite = Resources.Load<Sprite>($"Icons/Achievements/{achievement.icon}");
                if (iconSprite != null)
                    iconImage.sprite = iconSprite;
                iconImage.color = Color.white;
            }

            if (rewardsText != null)
            {
                rewardsText.text = FormatRewardsShort(achievement.rewards);
            }

            if (tierBorder != null)
            {
                tierBorder.color = GetTierColor(achievement.tier);
            }
        }

        private string FormatRewardsShort(AchievementRewards rewards)
        {
            if (rewards == null) return "";

            List<string> parts = new List<string>();

            if (rewards.coins > 0)
                parts.Add($"+{rewards.coins} <sprite name=\"coin\">");

            if (rewards.gems > 0)
                parts.Add($"+{rewards.gems} <sprite name=\"gem\">");

            if (rewards.exp > 0)
                parts.Add($"+{rewards.exp} XP");

            return string.Join("  ", parts);
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

        private void PlayEffects(AchievementData achievement)
        {
            // Particles
            if (celebrationParticles != null)
            {
                var main = celebrationParticles.main;
                main.startColor = GetTierColor(achievement.tier);
                celebrationParticles.Play();
            }

            // Sound
            if (audioSource != null)
            {
                AudioClip clip = achievement.tier >= 3 ? legendaryUnlockSound : unlockSound;
                if (clip != null)
                {
                    audioSource.PlayOneShot(clip);
                }
            }
        }

        #endregion

        #region Animation

        private IEnumerator SlideIn()
        {
            if (panelTransform == null || canvasGroup == null) yield break;

            Vector2 startPos = originalPosition + new Vector2(slideDistance, 0);
            Vector2 endPos = originalPosition;

            float elapsed = 0f;

            while (elapsed < slideInDuration)
            {
                elapsed += Time.deltaTime;
                float t = slideInCurve.Evaluate(elapsed / slideInDuration);

                panelTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                canvasGroup.alpha = t;

                yield return null;
            }

            panelTransform.anchoredPosition = endPos;
            canvasGroup.alpha = 1f;
        }

        private IEnumerator SlideOut()
        {
            if (panelTransform == null || canvasGroup == null) yield break;

            Vector2 startPos = originalPosition;
            Vector2 endPos = originalPosition + new Vector2(slideDistance, 0);

            float elapsed = 0f;

            while (elapsed < slideOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = slideOutCurve.Evaluate(elapsed / slideOutDuration);

                panelTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                canvasGroup.alpha = 1f - t;

                yield return null;
            }

            HideImmediate();
        }

        private void HideImmediate()
        {
            if (notificationPanel != null)
                notificationPanel.SetActive(false);

            if (panelTransform != null)
                panelTransform.anchoredPosition = originalPosition;

            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Skip current notification
        /// </summary>
        public void Skip()
        {
            StopAllCoroutines();
            HideImmediate();
            ShowNextNotification();
        }

        /// <summary>
        /// Clear all pending notifications
        /// </summary>
        public void ClearQueue()
        {
            pendingNotifications.Clear();
        }

        /// <summary>
        /// Check if currently showing a notification
        /// </summary>
        public bool IsShowing => isShowingNotification;

        /// <summary>
        /// Get number of pending notifications
        /// </summary>
        public int PendingCount => pendingNotifications.Count;

        #endregion
    }
}
