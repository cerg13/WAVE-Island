using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WaveIsland.Events;

namespace WaveIsland.UI.Events
{
    /// <summary>
    /// Event banner that appears on the main screen when an event is active
    /// </summary>
    public class EventBanner : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject bannerContainer;
        [SerializeField] private Image bannerBackground;
        [SerializeField] private Image eventIcon;
        [SerializeField] private TextMeshProUGUI eventNameText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Button openEventButton;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private string showTrigger = "Show";
        [SerializeField] private string hideTrigger = "Hide";
        [SerializeField] private string pulseTrigger = "Pulse";

        [Header("Notification")]
        [SerializeField] private GameObject notificationBadge;
        [SerializeField] private TextMeshProUGUI badgeCountText;

        [Header("References")]
        [SerializeField] private EventUIController eventUIController;

        private bool isVisible = false;

        private void Start()
        {
            if (openEventButton != null)
                openEventButton.onClick.AddListener(OpenEventPanel);

            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnEventStarted += OnEventStarted;
                EventManager.Instance.OnEventEnded += OnEventEnded;
                EventManager.Instance.OnMissionCompleted += OnMissionCompleted;
            }

            CheckForActiveEvent();
        }

        private void OnDestroy()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.OnEventStarted -= OnEventStarted;
                EventManager.Instance.OnEventEnded -= OnEventEnded;
                EventManager.Instance.OnMissionCompleted -= OnMissionCompleted;
            }
        }

        private void Update()
        {
            if (isVisible)
            {
                UpdateTimer();
                UpdateNotificationBadge();
            }
        }

        #region Visibility

        private void CheckForActiveEvent()
        {
            var manager = EventManager.Instance;
            if (manager?.ActiveEvent != null)
            {
                ShowBanner(manager.ActiveEvent);
            }
            else
            {
                HideBanner();
            }
        }

        private void ShowBanner(GameEvent evt)
        {
            if (bannerContainer == null) return;

            isVisible = true;
            bannerContainer.SetActive(true);

            // Event name
            if (eventNameText != null)
                eventNameText.text = evt.nameRu;

            // Theme color
            if (bannerBackground != null && ColorUtility.TryParseHtmlString(evt.themeColor, out Color color))
            {
                bannerBackground.color = color;
            }

            // Animation
            if (animator != null)
                animator.SetTrigger(showTrigger);

            UpdateTimer();
            UpdateNotificationBadge();
        }

        private void HideBanner()
        {
            isVisible = false;

            if (animator != null)
            {
                animator.SetTrigger(hideTrigger);
            }
            else if (bannerContainer != null)
            {
                bannerContainer.SetActive(false);
            }
        }

        #endregion

        #region Updates

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
                timerText.text = $"{(int)remaining.TotalDays}д";
            }
            else if (remaining.TotalHours >= 1)
            {
                timerText.text = $"{(int)remaining.TotalHours}ч";
            }
            else
            {
                timerText.text = $"{remaining.Minutes}м";
            }
        }

        private void UpdateNotificationBadge()
        {
            if (notificationBadge == null) return;

            var manager = EventManager.Instance;
            if (manager == null)
            {
                notificationBadge.SetActive(false);
                return;
            }

            bool hasUnclaimed = manager.HasUnclaimedMissions() || manager.HasUnclaimedTiers();
            notificationBadge.SetActive(hasUnclaimed);

            if (hasUnclaimed && badgeCountText != null)
            {
                int count = 0;
                foreach (var m in manager.GetActiveMissions())
                {
                    if (m.isCompleted && !m.isClaimed)
                        count++;
                }
                badgeCountText.text = count > 0 ? count.ToString() : "!";
            }
        }

        #endregion

        #region Actions

        private void OpenEventPanel()
        {
            if (eventUIController != null)
            {
                eventUIController.Show();
            }
        }

        public void PlayPulseAnimation()
        {
            if (animator != null)
                animator.SetTrigger(pulseTrigger);
        }

        #endregion

        #region Event Handlers

        private void OnEventStarted(GameEvent evt)
        {
            ShowBanner(evt);
        }

        private void OnEventEnded(GameEvent evt)
        {
            HideBanner();
        }

        private void OnMissionCompleted(EventMission mission)
        {
            PlayPulseAnimation();
            UpdateNotificationBadge();
        }

        #endregion
    }
}
