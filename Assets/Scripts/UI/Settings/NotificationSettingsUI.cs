using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WaveIsland.Notifications;

namespace WaveIsland.UI.Settings
{
    /// <summary>
    /// UI controller for notification settings
    /// </summary>
    public class NotificationSettingsUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Button closeButton;

        [Header("Master Toggle")]
        [SerializeField] private Toggle masterToggle;
        [SerializeField] private TextMeshProUGUI masterStatusText;

        [Header("Individual Toggles")]
        [SerializeField] private Toggle plantReadyToggle;
        [SerializeField] private Toggle dailyReminderToggle;
        [SerializeField] private Toggle spiritSummonToggle;
        [SerializeField] private Toggle shopRefreshToggle;

        [Header("Daily Reminder Time")]
        [SerializeField] private TMP_Dropdown hourDropdown;
        [SerializeField] private TMP_Dropdown minuteDropdown;

        [Header("Test Buttons (Debug)")]
        [SerializeField] private Button testPlantNotificationButton;
        [SerializeField] private Button testDailyNotificationButton;
        [SerializeField] private Button cancelAllButton;

        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            // Master toggle
            if (masterToggle != null)
                masterToggle.onValueChanged.AddListener(OnMasterToggleChanged);

            // Individual toggles
            if (plantReadyToggle != null)
                plantReadyToggle.onValueChanged.AddListener(OnPlantToggleChanged);

            if (dailyReminderToggle != null)
                dailyReminderToggle.onValueChanged.AddListener(OnDailyToggleChanged);

            if (spiritSummonToggle != null)
                spiritSummonToggle.onValueChanged.AddListener(OnSpiritToggleChanged);

            if (shopRefreshToggle != null)
                shopRefreshToggle.onValueChanged.AddListener(OnShopToggleChanged);

            // Time dropdowns
            if (hourDropdown != null)
            {
                hourDropdown.ClearOptions();
                for (int i = 0; i < 24; i++)
                {
                    hourDropdown.options.Add(new TMP_Dropdown.OptionData(i.ToString("00")));
                }
                hourDropdown.onValueChanged.AddListener(OnTimeChanged);
            }

            if (minuteDropdown != null)
            {
                minuteDropdown.ClearOptions();
                for (int i = 0; i < 60; i += 15)
                {
                    minuteDropdown.options.Add(new TMP_Dropdown.OptionData(i.ToString("00")));
                }
                minuteDropdown.onValueChanged.AddListener(OnTimeChanged);
            }

            // Debug buttons
            if (testPlantNotificationButton != null)
                testPlantNotificationButton.onClick.AddListener(TestPlantNotification);

            if (testDailyNotificationButton != null)
                testDailyNotificationButton.onClick.AddListener(TestDailyNotification);

            if (cancelAllButton != null)
                cancelAllButton.onClick.AddListener(CancelAllNotifications);
        }

        private void Start()
        {
            Hide();
        }

        public void Show()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
                RefreshUI();
            }
        }

        public void Hide()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }

        public void Toggle()
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
                Hide();
            else
                Show();
        }

        private void RefreshUI()
        {
            var manager = NotificationManager.Instance;
            if (manager == null) return;

            // Master toggle
            if (masterToggle != null)
            {
                masterToggle.SetIsOnWithoutNotify(manager.NotificationsEnabled);
            }

            UpdateMasterStatusText(manager.NotificationsEnabled);

            // Individual toggles
            if (plantReadyToggle != null)
            {
                plantReadyToggle.SetIsOnWithoutNotify(manager.PlantNotificationsEnabled);
                plantReadyToggle.interactable = manager.NotificationsEnabled;
            }

            if (dailyReminderToggle != null)
            {
                dailyReminderToggle.SetIsOnWithoutNotify(manager.DailyReminderEnabled);
                dailyReminderToggle.interactable = manager.NotificationsEnabled;
            }

            if (spiritSummonToggle != null)
            {
                spiritSummonToggle.SetIsOnWithoutNotify(manager.SpiritNotificationsEnabled);
                spiritSummonToggle.interactable = manager.NotificationsEnabled;
            }

            if (shopRefreshToggle != null)
            {
                shopRefreshToggle.SetIsOnWithoutNotify(manager.ShopNotificationsEnabled);
                shopRefreshToggle.interactable = manager.NotificationsEnabled;
            }

            // Time dropdowns
            bool timeInteractable = manager.NotificationsEnabled && manager.DailyReminderEnabled;

            if (hourDropdown != null)
            {
                hourDropdown.SetValueWithoutNotify(12); // Default to noon
                hourDropdown.interactable = timeInteractable;
            }

            if (minuteDropdown != null)
            {
                minuteDropdown.SetValueWithoutNotify(0);
                minuteDropdown.interactable = timeInteractable;
            }
        }

        private void UpdateMasterStatusText(bool enabled)
        {
            if (masterStatusText != null)
            {
                masterStatusText.text = enabled ? "Уведомления включены" : "Уведомления выключены";
                masterStatusText.color = enabled ? Color.green : Color.gray;
            }
        }

        #region Toggle Handlers

        private void OnMasterToggleChanged(bool value)
        {
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.NotificationsEnabled = value;
            }

            UpdateMasterStatusText(value);

            // Update interactability of child toggles
            if (plantReadyToggle != null) plantReadyToggle.interactable = value;
            if (dailyReminderToggle != null) dailyReminderToggle.interactable = value;
            if (spiritSummonToggle != null) spiritSummonToggle.interactable = value;
            if (shopRefreshToggle != null) shopRefreshToggle.interactable = value;

            bool timeInteractable = value && (dailyReminderToggle?.isOn ?? false);
            if (hourDropdown != null) hourDropdown.interactable = timeInteractable;
            if (minuteDropdown != null) minuteDropdown.interactable = timeInteractable;
        }

        private void OnPlantToggleChanged(bool value)
        {
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.PlantNotificationsEnabled = value;
            }
        }

        private void OnDailyToggleChanged(bool value)
        {
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.DailyReminderEnabled = value;
            }

            bool masterEnabled = NotificationManager.Instance?.NotificationsEnabled ?? false;
            if (hourDropdown != null) hourDropdown.interactable = masterEnabled && value;
            if (minuteDropdown != null) minuteDropdown.interactable = masterEnabled && value;
        }

        private void OnSpiritToggleChanged(bool value)
        {
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.SpiritNotificationsEnabled = value;
            }
        }

        private void OnShopToggleChanged(bool value)
        {
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShopNotificationsEnabled = value;
            }
        }

        private void OnTimeChanged(int value)
        {
            // Time would need to be saved and used when scheduling
            // For now, just schedule daily reminder with new time
            if (NotificationManager.Instance != null && NotificationManager.Instance.DailyReminderEnabled)
            {
                NotificationManager.Instance.ScheduleDailyReminder();
            }
        }

        #endregion

        #region Debug Methods

        private void TestPlantNotification()
        {
            if (NotificationManager.Instance != null)
            {
                // Schedule a test notification in 10 seconds
                NotificationManager.Instance.SchedulePlantReadyNotification(
                    "test",
                    "Тестовое растение",
                    System.DateTime.Now.AddSeconds(10)
                );
                Debug.Log("Test plant notification scheduled for 10 seconds from now");
            }
        }

        private void TestDailyNotification()
        {
            if (NotificationManager.Instance != null)
            {
                // Schedule a test notification in 10 seconds
                NotificationManager.Instance.ScheduleCustomNotification(
                    "test_daily",
                    "Тестовое ежедневное",
                    "Это тестовое уведомление!",
                    System.TimeSpan.FromSeconds(10)
                );
                Debug.Log("Test daily notification scheduled for 10 seconds from now");
            }
        }

        private void CancelAllNotifications()
        {
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.CancelAllNotifications();
                Debug.Log("All notifications cancelled");
            }
        }

        #endregion
    }
}
