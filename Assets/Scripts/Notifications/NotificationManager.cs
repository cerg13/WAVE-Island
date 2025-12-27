using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif
#if UNITY_IOS
using Unity.Notifications.iOS;
#endif

namespace WaveIsland.Notifications
{
    /// <summary>
    /// Manages local and push notifications
    /// </summary>
    public class NotificationManager : MonoBehaviour
    {
        public static NotificationManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool notificationsEnabled = true;
        [SerializeField] private bool plantReadyNotifications = true;
        [SerializeField] private bool dailyReminderNotifications = true;
        [SerializeField] private bool spiritSummonNotifications = true;
        [SerializeField] private bool shopRefreshNotifications = true;

        [Header("Daily Reminder")]
        [SerializeField] private int dailyReminderHour = 12;
        [SerializeField] private int dailyReminderMinute = 0;

        // Notification IDs
        private const string CHANNEL_ID = "wave_island_default";
        private const string CHANNEL_NAME = "WAVE Island";
        private const string CHANNEL_DESCRIPTION = "Game notifications";

        // Scheduled notification tracking
        private Dictionary<string, int> scheduledNotifications = new Dictionary<string, int>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            LoadSettings();

#if UNITY_ANDROID
            InitializeAndroid();
#endif

#if UNITY_IOS
            InitializeIOS();
#endif
        }

        #region Platform Initialization

#if UNITY_ANDROID
        private void InitializeAndroid()
        {
            var channel = new AndroidNotificationChannel()
            {
                Id = CHANNEL_ID,
                Name = CHANNEL_NAME,
                Importance = Importance.Default,
                Description = CHANNEL_DESCRIPTION,
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);

            // Check if app was opened from notification
            var notificationIntentData = AndroidNotificationCenter.GetLastNotificationIntent();
            if (notificationIntentData != null)
            {
                HandleNotificationIntent(notificationIntentData.Id, notificationIntentData.Notification);
            }
        }

        private void HandleNotificationIntent(int id, AndroidNotification notification)
        {
            Debug.Log($"NotificationManager: App opened from notification {id}");
            // Handle navigation based on notification data
        }
#endif

#if UNITY_IOS
        private void InitializeIOS()
        {
            StartCoroutine(RequestIOSAuthorization());
        }

        private System.Collections.IEnumerator RequestIOSAuthorization()
        {
            var authorizationOption = AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound;
            using (var req = new AuthorizationRequest(authorizationOption, true))
            {
                while (!req.IsFinished)
                {
                    yield return null;
                }

                if (req.Granted)
                {
                    Debug.Log("NotificationManager: iOS notifications authorized");
                }
                else
                {
                    Debug.Log("NotificationManager: iOS notifications denied");
                }
            }
        }
#endif

        #endregion

        #region Schedule Notifications

        /// <summary>
        /// Schedule a notification for when a plant is ready to harvest
        /// </summary>
        public void SchedulePlantReadyNotification(string plantId, string plantName, DateTime readyTime)
        {
            if (!notificationsEnabled || !plantReadyNotifications) return;

            string notificationKey = $"plant_{plantId}";
            CancelNotification(notificationKey);

            TimeSpan delay = readyTime - DateTime.Now;
            if (delay.TotalSeconds <= 0) return;

            string title = "Урожай готов!";
            string body = $"Ваш {plantName} готов к сбору!";

            int notificationId = ScheduleNotification(title, body, delay);
            scheduledNotifications[notificationKey] = notificationId;

            Debug.Log($"NotificationManager: Scheduled plant notification for {plantName} in {delay.TotalMinutes:F1} minutes");
        }

        /// <summary>
        /// Cancel a plant ready notification
        /// </summary>
        public void CancelPlantNotification(string plantId)
        {
            string notificationKey = $"plant_{plantId}";
            CancelNotification(notificationKey);
        }

        /// <summary>
        /// Schedule daily reminder notification
        /// </summary>
        public void ScheduleDailyReminder()
        {
            if (!notificationsEnabled || !dailyReminderNotifications) return;

            CancelNotification("daily_reminder");

            DateTime now = DateTime.Now;
            DateTime nextReminder = new DateTime(now.Year, now.Month, now.Day, dailyReminderHour, dailyReminderMinute, 0);

            if (nextReminder <= now)
            {
                nextReminder = nextReminder.AddDays(1);
            }

            TimeSpan delay = nextReminder - now;

            string title = "WAVE Island ждёт!";
            string body = "Загляните на остров - ваши растения соскучились!";

            int notificationId = ScheduleNotification(title, body, delay, true);
            scheduledNotifications["daily_reminder"] = notificationId;

            Debug.Log($"NotificationManager: Scheduled daily reminder for {nextReminder}");
        }

        /// <summary>
        /// Schedule shop refresh notification
        /// </summary>
        public void ScheduleShopRefreshNotification(DateTime refreshTime)
        {
            if (!notificationsEnabled || !shopRefreshNotifications) return;

            CancelNotification("shop_refresh");

            TimeSpan delay = refreshTime - DateTime.Now;
            if (delay.TotalSeconds <= 0) return;

            string title = "Магазин обновился!";
            string body = "Новые товары уже ждут вас!";

            int notificationId = ScheduleNotification(title, body, delay);
            scheduledNotifications["shop_refresh"] = notificationId;
        }

        /// <summary>
        /// Schedule spirit summon availability notification
        /// </summary>
        public void ScheduleFreeSummonNotification(DateTime availableTime)
        {
            if (!notificationsEnabled || !spiritSummonNotifications) return;

            CancelNotification("free_summon");

            TimeSpan delay = availableTime - DateTime.Now;
            if (delay.TotalSeconds <= 0) return;

            string title = "Бесплатный призыв доступен!";
            string body = "Попробуйте призвать нового духа!";

            int notificationId = ScheduleNotification(title, body, delay);
            scheduledNotifications["free_summon"] = notificationId;
        }

        /// <summary>
        /// Schedule a custom notification
        /// </summary>
        public void ScheduleCustomNotification(string key, string title, string body, TimeSpan delay)
        {
            if (!notificationsEnabled) return;

            CancelNotification(key);

            int notificationId = ScheduleNotification(title, body, delay);
            scheduledNotifications[key] = notificationId;
        }

        #endregion

        #region Core Notification Methods

        private int ScheduleNotification(string title, string body, TimeSpan delay, bool repeating = false)
        {
            int notificationId = 0;

#if UNITY_ANDROID
            var notification = new AndroidNotification
            {
                Title = title,
                Text = body,
                FireTime = DateTime.Now.Add(delay),
                SmallIcon = "icon_small",
                LargeIcon = "icon_large"
            };

            if (repeating)
            {
                notification.RepeatInterval = TimeSpan.FromDays(1);
            }

            notificationId = AndroidNotificationCenter.SendNotification(notification, CHANNEL_ID);
#endif

#if UNITY_IOS
            var timeTrigger = new iOSNotificationTimeIntervalTrigger()
            {
                TimeInterval = delay,
                Repeats = repeating
            };

            var notification = new iOSNotification()
            {
                Title = title,
                Body = body,
                ShowInForeground = true,
                ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
                CategoryIdentifier = "wave_island_notification",
                Trigger = timeTrigger
            };

            iOSNotificationCenter.ScheduleNotification(notification);
            notificationId = notification.GetHashCode();
#endif

            return notificationId;
        }

        private void CancelNotification(string key)
        {
            if (!scheduledNotifications.TryGetValue(key, out int notificationId))
                return;

#if UNITY_ANDROID
            AndroidNotificationCenter.CancelNotification(notificationId);
#endif

#if UNITY_IOS
            // iOS cancels by identifier, we'd need to track differently
            iOSNotificationCenter.RemoveAllScheduledNotifications();
#endif

            scheduledNotifications.Remove(key);
        }

        /// <summary>
        /// Cancel all scheduled notifications
        /// </summary>
        public void CancelAllNotifications()
        {
#if UNITY_ANDROID
            AndroidNotificationCenter.CancelAllNotifications();
#endif

#if UNITY_IOS
            iOSNotificationCenter.RemoveAllScheduledNotifications();
#endif

            scheduledNotifications.Clear();
            Debug.Log("NotificationManager: All notifications cancelled");
        }

        #endregion

        #region Settings

        public bool NotificationsEnabled
        {
            get => notificationsEnabled;
            set
            {
                notificationsEnabled = value;
                SaveSettings();
                if (!value)
                {
                    CancelAllNotifications();
                }
                else
                {
                    ScheduleDailyReminder();
                }
            }
        }

        public bool PlantNotificationsEnabled
        {
            get => plantReadyNotifications;
            set
            {
                plantReadyNotifications = value;
                SaveSettings();
            }
        }

        public bool DailyReminderEnabled
        {
            get => dailyReminderNotifications;
            set
            {
                dailyReminderNotifications = value;
                SaveSettings();
                if (value)
                {
                    ScheduleDailyReminder();
                }
                else
                {
                    CancelNotification("daily_reminder");
                }
            }
        }

        public bool SpiritNotificationsEnabled
        {
            get => spiritSummonNotifications;
            set
            {
                spiritSummonNotifications = value;
                SaveSettings();
            }
        }

        public bool ShopNotificationsEnabled
        {
            get => shopRefreshNotifications;
            set
            {
                shopRefreshNotifications = value;
                SaveSettings();
            }
        }

        private const string SETTINGS_KEY = "NotificationSettings";

        private void SaveSettings()
        {
            NotificationSettings settings = new NotificationSettings
            {
                enabled = notificationsEnabled,
                plantReady = plantReadyNotifications,
                dailyReminder = dailyReminderNotifications,
                spiritSummon = spiritSummonNotifications,
                shopRefresh = shopRefreshNotifications,
                dailyHour = dailyReminderHour,
                dailyMinute = dailyReminderMinute
            };

            string json = JsonUtility.ToJson(settings);
            PlayerPrefs.SetString(SETTINGS_KEY, json);
            PlayerPrefs.Save();
        }

        private void LoadSettings()
        {
            if (!PlayerPrefs.HasKey(SETTINGS_KEY)) return;

            try
            {
                string json = PlayerPrefs.GetString(SETTINGS_KEY);
                NotificationSettings settings = JsonUtility.FromJson<NotificationSettings>(json);

                notificationsEnabled = settings.enabled;
                plantReadyNotifications = settings.plantReady;
                dailyReminderNotifications = settings.dailyReminder;
                spiritSummonNotifications = settings.spiritSummon;
                shopRefreshNotifications = settings.shopRefresh;
                dailyReminderHour = settings.dailyHour;
                dailyReminderMinute = settings.dailyMinute;
            }
            catch (Exception e)
            {
                Debug.LogError($"NotificationManager: Failed to load settings: {e.Message}");
            }
        }

        #endregion

        #region Application Lifecycle

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // App going to background
                ScheduleDailyReminder();
            }
            else
            {
                // App coming to foreground
                ClearNotificationBadge();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                ClearNotificationBadge();
            }
        }

        private void ClearNotificationBadge()
        {
#if UNITY_IOS
            iOSNotificationCenter.ApplicationBadge = 0;
#endif

#if UNITY_ANDROID
            AndroidNotificationCenter.CancelAllDisplayedNotifications();
#endif
        }

        #endregion
    }

    [Serializable]
    public class NotificationSettings
    {
        public bool enabled = true;
        public bool plantReady = true;
        public bool dailyReminder = true;
        public bool spiritSummon = true;
        public bool shopRefresh = true;
        public int dailyHour = 12;
        public int dailyMinute = 0;
    }
}
