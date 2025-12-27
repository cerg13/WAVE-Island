using UnityEngine;

namespace WaveIsland.Core
{
    /// <summary>
    /// Main game manager - singleton pattern
    /// Controls game state, initialization, and high-level game flow
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.Loading;

        [Header("Player Data")]
        public PlayerData PlayerData { get; private set; }

        [Header("References")]
        [SerializeField] private GardenManager gardenManager;
        [SerializeField] private AlchemyManager alchemyManager;
        [SerializeField] private SpiritManager spiritManager;

        public GameState CurrentState => currentState;

        public event System.Action<GameState> OnGameStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        private void Initialize()
        {
            Debug.Log("[GameManager] Initializing WAVE Island...");

            // Load player data
            PlayerData = SaveSystem.LoadPlayerData();

            if (PlayerData == null)
            {
                PlayerData = new PlayerData();
                Debug.Log("[GameManager] Created new player data");
            }
            else
            {
                Debug.Log($"[GameManager] Loaded player: Level {PlayerData.Level}");
            }

            SetGameState(GameState.Playing);
        }

        public void SetGameState(GameState newState)
        {
            if (currentState == newState) return;

            var oldState = currentState;
            currentState = newState;

            Debug.Log($"[GameManager] State changed: {oldState} -> {newState}");
            OnGameStateChanged?.Invoke(newState);
        }

        public void SaveGame()
        {
            SaveSystem.SavePlayerData(PlayerData);
            Debug.Log("[GameManager] Game saved");
        }

        public void AddCoins(int amount)
        {
            PlayerData.Coins += amount;
            Debug.Log($"[GameManager] Added {amount} coins. Total: {PlayerData.Coins}");
        }

        public bool SpendCoins(int amount)
        {
            if (PlayerData.Coins < amount)
            {
                Debug.LogWarning($"[GameManager] Not enough coins: {PlayerData.Coins} < {amount}");
                return false;
            }

            PlayerData.Coins -= amount;
            return true;
        }

        public void AddExperience(int amount)
        {
            PlayerData.Experience += amount;
            CheckLevelUp();
        }

        private void CheckLevelUp()
        {
            int expNeeded = GetExpForLevel(PlayerData.Level + 1);

            while (PlayerData.Experience >= expNeeded && PlayerData.Level < 30)
            {
                PlayerData.Level++;
                PlayerData.Experience -= expNeeded;
                expNeeded = GetExpForLevel(PlayerData.Level + 1);

                Debug.Log($"[GameManager] Level Up! Now level {PlayerData.Level}");
                // TODO: Trigger level up event, show UI, unlock features
            }
        }

        private int GetExpForLevel(int level)
        {
            // Simple exponential curve
            return 100 * level * level;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveGame();
            }
        }

        private void OnApplicationQuit()
        {
            SaveGame();
        }
    }

    public enum GameState
    {
        Loading,
        MainMenu,
        Playing,
        Paused,
        Crafting,
        Shopping,
        Tutorial
    }
}
