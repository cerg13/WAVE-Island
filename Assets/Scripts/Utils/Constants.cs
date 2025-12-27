namespace WaveIsland.Utils
{
    /// <summary>
    /// Game constants and configuration values
    /// </summary>
    public static class Constants
    {
        // Game Version
        public const string GAME_VERSION = "1.0.0";
        public const int BUILD_NUMBER = 1;

        // Player Limits
        public const int MAX_PLAYER_LEVEL = 30;
        public const int STARTING_COINS = 500;
        public const int STARTING_GEMS = 10;
        public const int MAX_INVENTORY_STACKS = 100;

        // Garden
        public const int BASE_GARDEN_PLOTS = 4;
        public const int MAX_GARDEN_PLOTS = 20;
        public const int PLOTS_PER_GARDEN_LEVEL = 2;
        public const float WATER_BONUS_MULTIPLIER = 1.1f;
        public const float WITHER_WARNING_THRESHOLD = 0.75f;

        // Alchemy
        public const int BASE_CRAFT_SLOTS = 3;
        public const int MAX_CRAFT_SLOTS = 5;
        public const float MIN_CRAFT_TIME = 2f;
        public const int FAILED_CRAFT_XP = 5;

        // Spirits
        public const int SINGLE_PULL_COST = 100;
        public const int TEN_PULL_COST = 900;
        public const int MAX_SPIRIT_LEVEL = 10;
        public const float PITY_RATE_PER_PULL = 0.01f;

        // Economy
        public const float DAILY_BONUS_MULTIPLIER = 1.5f;
        public const int DAILY_BONUS_BASE_COINS = 100;
        public const int AD_REWARD_COINS = 50;

        // Timing (seconds)
        public const float AUTOSAVE_INTERVAL = 60f;
        public const float OFFLINE_PROGRESS_CAP_HOURS = 12f;
        public const float TUTORIAL_DELAY = 0.5f;

        // iiko Integration
        public const string IIKO_API_BASE_URL = "https://api-eu.iiko.services";
        public const int IIKO_ORDER_BONUS_MULTIPLIER = 3;
        public const int IIKO_RECIPE_UNLOCK_BONUS = 100;

        // Scenes
        public const string SCENE_LOADING = "Loading";
        public const string SCENE_MAIN_MENU = "MainMenu";
        public const string SCENE_GAME = "Game";

        // PlayerPrefs Keys
        public const string PREF_SOUND_ENABLED = "SoundEnabled";
        public const string PREF_MUSIC_ENABLED = "MusicEnabled";
        public const string PREF_NOTIFICATIONS_ENABLED = "NotificationsEnabled";
        public const string PREF_LANGUAGE = "Language";

        // Tags
        public const string TAG_PLAYER = "Player";
        public const string TAG_PLOT = "GardenPlot";
        public const string TAG_INGREDIENT = "Ingredient";

        // Layers
        public const string LAYER_UI = "UI";
        public const string LAYER_GARDEN = "Garden";
        public const string LAYER_ITEMS = "Items";

        // Animation Parameters
        public const string ANIM_GROW = "Grow";
        public const string ANIM_HARVEST = "Harvest";
        public const string ANIM_CRAFT = "Craft";
        public const string ANIM_POPUP = "Popup";
    }
}
