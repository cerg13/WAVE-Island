using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace WaveIsland.Core
{
    /// <summary>
    /// Optimized save system with encryption, compression, and cloud sync
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float autoSaveInterval = 60f;
        [SerializeField] private bool encryptSaves = true;
        [SerializeField] private bool compressSaves = true;
        [SerializeField] private int maxBackups = 3;

        [Header("Cloud Sync")]
        [SerializeField] private bool enableCloudSync = true;
        [SerializeField] private string cloudSaveEndpoint = "http://localhost:3000/api/save";

        // Encryption key (should be stored securely in production)
        private readonly string encryptionKey = "WaveIsland2024SecretKey!";

        // Save state
        private bool isDirty = false;
        private float lastSaveTime;
        private SaveData currentSave;
        private Queue<SaveData> saveBackups = new Queue<SaveData>();

        // Events
        public event Action OnSaveStarted;
        public event Action OnSaveCompleted;
        public event Action<string> OnSaveFailed;
        public event Action OnLoadCompleted;
        public event Action<string> OnLoadFailed;

        public bool IsDirty => isDirty;

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
            }
        }

        private void Start()
        {
            // Auto-load on start
            Load();

            // Start auto-save coroutine
            StartCoroutine(AutoSaveCoroutine());
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && isDirty)
            {
                Save();
            }
        }

        private void OnApplicationQuit()
        {
            if (isDirty)
            {
                Save();
            }
        }

        #region Auto Save

        private IEnumerator AutoSaveCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(autoSaveInterval);

                if (isDirty)
                {
                    Save();
                }
            }
        }

        /// <summary>
        /// Mark save data as dirty (needs saving)
        /// </summary>
        public void MarkDirty()
        {
            isDirty = true;
        }

        #endregion

        #region Save

        /// <summary>
        /// Save all game data
        /// </summary>
        public void Save()
        {
            OnSaveStarted?.Invoke();

            try
            {
                // Collect save data
                currentSave = CollectSaveData();

                // Create backup
                CreateBackup();

                // Serialize
                string json = JsonUtility.ToJson(currentSave, true);

                // Compress (optional)
                if (compressSaves)
                {
                    json = Compress(json);
                }

                // Encrypt (optional)
                if (encryptSaves)
                {
                    json = Encrypt(json);
                }

                // Write to file
                string savePath = GetSavePath();
                File.WriteAllText(savePath, json);

                // Update state
                isDirty = false;
                lastSaveTime = Time.time;

                // Cloud sync
                if (enableCloudSync)
                {
                    StartCoroutine(SyncToCloud(currentSave));
                }

                OnSaveCompleted?.Invoke();
                Debug.Log($"SaveManager: Game saved to {savePath}");
            }
            catch (Exception e)
            {
                OnSaveFailed?.Invoke(e.Message);
                Debug.LogError($"SaveManager: Save failed - {e.Message}");
            }
        }

        /// <summary>
        /// Save specific data module
        /// </summary>
        public void SaveModule<T>(string key, T data)
        {
            try
            {
                string json = JsonUtility.ToJson(data);

                if (encryptSaves)
                    json = Encrypt(json);

                PlayerPrefs.SetString($"Module_{key}", json);
                PlayerPrefs.Save();

                Debug.Log($"SaveManager: Module '{key}' saved");
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveManager: Failed to save module '{key}' - {e.Message}");
            }
        }

        private SaveData CollectSaveData()
        {
            var save = new SaveData
            {
                version = Application.version,
                timestamp = DateTime.UtcNow.ToString("O"),
                checksum = ""
            };

            // Player data
            var playerData = PlayerData.Instance;
            if (playerData != null)
            {
                save.player = new PlayerSaveData
                {
                    level = playerData.Level,
                    experience = playerData.Experience,
                    coins = playerData.Coins,
                    gems = playerData.Gems,
                    energy = playerData.Energy
                };
            }

            // Inventory
            var inventory = Inventory.InventoryManager.Instance;
            if (inventory != null)
            {
                save.inventory = new InventorySaveData
                {
                    items = inventory.GetAllItemsForSave()
                };
            }

            // Recipes
            var recipes = Alchemy.RecipeManager.Instance;
            if (recipes != null)
            {
                save.recipes = new RecipesSaveData
                {
                    discoveredIds = recipes.GetDiscoveredRecipeIds()
                };
            }

            // Achievements
            var achievements = Achievements.AchievementManager.Instance;
            if (achievements != null)
            {
                save.achievements = new AchievementsSaveData
                {
                    unlockedIds = achievements.GetUnlockedIds(),
                    stats = achievements.GetAllStats()
                };
            }

            // Spirits
            var spirits = Spirits.SpiritManager.Instance;
            if (spirits != null)
            {
                save.spirits = new SpiritsSaveData
                {
                    ownedIds = spirits.GetOwnedSpiritIds(),
                    activeId = spirits.GetActiveSpiritId()
                };
            }

            // Garden
            // TODO: Add garden state saving

            // Calculate checksum
            save.checksum = CalculateChecksum(save);

            return save;
        }

        private void CreateBackup()
        {
            if (currentSave == null) return;

            saveBackups.Enqueue(currentSave);

            while (saveBackups.Count > maxBackups)
            {
                saveBackups.Dequeue();
            }

            // Also save backup to file
            try
            {
                string backupPath = GetBackupPath();
                string json = JsonUtility.ToJson(currentSave);
                File.WriteAllText(backupPath, json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"SaveManager: Backup failed - {e.Message}");
            }
        }

        #endregion

        #region Load

        /// <summary>
        /// Load game data
        /// </summary>
        public void Load()
        {
            try
            {
                string savePath = GetSavePath();

                if (!File.Exists(savePath))
                {
                    Debug.Log("SaveManager: No save file found, starting fresh");
                    return;
                }

                string json = File.ReadAllText(savePath);

                // Decrypt
                if (encryptSaves)
                {
                    json = Decrypt(json);
                }

                // Decompress
                if (compressSaves)
                {
                    json = Decompress(json);
                }

                // Deserialize
                SaveData save = JsonUtility.FromJson<SaveData>(json);

                // Verify checksum
                string expectedChecksum = save.checksum;
                save.checksum = "";
                string actualChecksum = CalculateChecksum(save);

                if (expectedChecksum != actualChecksum)
                {
                    Debug.LogWarning("SaveManager: Checksum mismatch, save may be corrupted");
                    // Try to load from backup
                    LoadFromBackup();
                    return;
                }

                // Apply save data
                ApplySaveData(save);
                currentSave = save;

                OnLoadCompleted?.Invoke();
                Debug.Log($"SaveManager: Game loaded from {savePath}");
            }
            catch (Exception e)
            {
                OnLoadFailed?.Invoke(e.Message);
                Debug.LogError($"SaveManager: Load failed - {e.Message}");

                // Try backup
                LoadFromBackup();
            }
        }

        /// <summary>
        /// Load specific data module
        /// </summary>
        public T LoadModule<T>(string key) where T : class, new()
        {
            try
            {
                string json = PlayerPrefs.GetString($"Module_{key}", "");

                if (string.IsNullOrEmpty(json))
                    return new T();

                if (encryptSaves)
                    json = Decrypt(json);

                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveManager: Failed to load module '{key}' - {e.Message}");
                return new T();
            }
        }

        private void LoadFromBackup()
        {
            try
            {
                string backupPath = GetBackupPath();

                if (!File.Exists(backupPath))
                {
                    Debug.LogWarning("SaveManager: No backup available");
                    return;
                }

                string json = File.ReadAllText(backupPath);
                SaveData save = JsonUtility.FromJson<SaveData>(json);

                ApplySaveData(save);
                currentSave = save;

                Debug.Log("SaveManager: Loaded from backup");
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveManager: Backup load failed - {e.Message}");
            }
        }

        private void ApplySaveData(SaveData save)
        {
            // Player data
            var playerData = PlayerData.Instance;
            if (playerData != null && save.player != null)
            {
                playerData.SetLevel(save.player.level);
                playerData.SetExperience(save.player.experience);
                playerData.SetCoins(save.player.coins);
                playerData.SetGems(save.player.gems);
                playerData.SetEnergy(save.player.energy);
            }

            // Inventory
            var inventory = Inventory.InventoryManager.Instance;
            if (inventory != null && save.inventory != null)
            {
                inventory.LoadFromSave(save.inventory.items);
            }

            // Recipes
            var recipes = Alchemy.RecipeManager.Instance;
            if (recipes != null && save.recipes != null)
            {
                recipes.LoadDiscoveredRecipes(save.recipes.discoveredIds);
            }

            // Achievements
            var achievements = Achievements.AchievementManager.Instance;
            if (achievements != null && save.achievements != null)
            {
                achievements.LoadFromSave(save.achievements.unlockedIds, save.achievements.stats);
            }

            // Spirits
            var spirits = Spirits.SpiritManager.Instance;
            if (spirits != null && save.spirits != null)
            {
                spirits.LoadFromSave(save.spirits.ownedIds, save.spirits.activeId);
            }
        }

        #endregion

        #region Cloud Sync

        private IEnumerator SyncToCloud(SaveData save)
        {
            string json = JsonUtility.ToJson(save);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            using (var request = new UnityEngine.Networking.UnityWebRequest(cloudSaveEndpoint, "POST"))
            {
                request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                string token = PlayerPrefs.GetString("AuthToken", "");
                if (!string.IsNullOrEmpty(token))
                {
                    request.SetRequestHeader("Authorization", $"Bearer {token}");
                }

                yield return request.SendWebRequest();

                if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    Debug.Log("SaveManager: Cloud sync completed");
                }
                else
                {
                    Debug.LogWarning($"SaveManager: Cloud sync failed - {request.error}");
                }
            }
        }

        /// <summary>
        /// Load from cloud
        /// </summary>
        public void LoadFromCloud(Action<bool> callback)
        {
            StartCoroutine(LoadFromCloudCoroutine(callback));
        }

        private IEnumerator LoadFromCloudCoroutine(Action<bool> callback)
        {
            using (var request = UnityEngine.Networking.UnityWebRequest.Get(cloudSaveEndpoint))
            {
                string token = PlayerPrefs.GetString("AuthToken", "");
                if (!string.IsNullOrEmpty(token))
                {
                    request.SetRequestHeader("Authorization", $"Bearer {token}");
                }

                yield return request.SendWebRequest();

                if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    try
                    {
                        SaveData save = JsonUtility.FromJson<SaveData>(request.downloadHandler.text);
                        ApplySaveData(save);
                        currentSave = save;
                        callback?.Invoke(true);
                        Debug.Log("SaveManager: Loaded from cloud");
                    }
                    catch
                    {
                        callback?.Invoke(false);
                    }
                }
                else
                {
                    callback?.Invoke(false);
                    Debug.LogWarning($"SaveManager: Cloud load failed - {request.error}");
                }
            }
        }

        #endregion

        #region Encryption/Compression

        private string Encrypt(string plainText)
        {
            try
            {
                byte[] key = Encoding.UTF8.GetBytes(encryptionKey.PadRight(32).Substring(0, 32));
                byte[] iv = new byte[16];

                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter sw = new StreamWriter(cs))
                            {
                                sw.Write(plainText);
                            }
                        }
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch
            {
                return plainText; // Fallback to unencrypted
            }
        }

        private string Decrypt(string cipherText)
        {
            try
            {
                byte[] key = Encoding.UTF8.GetBytes(encryptionKey.PadRight(32).Substring(0, 32));
                byte[] iv = new byte[16];
                byte[] buffer = Convert.FromBase64String(cipherText);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (MemoryStream ms = new MemoryStream(buffer))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader sr = new StreamReader(cs))
                            {
                                return sr.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch
            {
                return cipherText; // Assume already decrypted
            }
        }

        private string Compress(string text)
        {
            // Simple RLE-like compression for JSON
            // In production, use GZip or LZ4
            return text; // Placeholder
        }

        private string Decompress(string text)
        {
            return text; // Placeholder
        }

        #endregion

        #region Utility

        private string GetSavePath()
        {
            return Path.Combine(Application.persistentDataPath, "save.dat");
        }

        private string GetBackupPath()
        {
            return Path.Combine(Application.persistentDataPath, "save_backup.dat");
        }

        private string CalculateChecksum(SaveData save)
        {
            string json = JsonUtility.ToJson(save);
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(json));
                return Convert.ToBase64String(bytes);
            }
        }

        /// <summary>
        /// Delete all save data
        /// </summary>
        public void DeleteAllSaves()
        {
            try
            {
                string savePath = GetSavePath();
                if (File.Exists(savePath))
                    File.Delete(savePath);

                string backupPath = GetBackupPath();
                if (File.Exists(backupPath))
                    File.Delete(backupPath);

                PlayerPrefs.DeleteAll();

                currentSave = null;
                saveBackups.Clear();

                Debug.Log("SaveManager: All saves deleted");
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveManager: Delete failed - {e.Message}");
            }
        }

        /// <summary>
        /// Get last save timestamp
        /// </summary>
        public DateTime? GetLastSaveTime()
        {
            if (currentSave != null && !string.IsNullOrEmpty(currentSave.timestamp))
            {
                return DateTime.Parse(currentSave.timestamp);
            }
            return null;
        }

        #endregion
    }

    #region Save Data Classes

    [Serializable]
    public class SaveData
    {
        public string version;
        public string timestamp;
        public string checksum;

        public PlayerSaveData player;
        public InventorySaveData inventory;
        public RecipesSaveData recipes;
        public AchievementsSaveData achievements;
        public SpiritsSaveData spirits;
        public GardenSaveData garden;
    }

    [Serializable]
    public class PlayerSaveData
    {
        public int level;
        public int experience;
        public int coins;
        public int gems;
        public int energy;
    }

    [Serializable]
    public class InventorySaveData
    {
        public List<InventoryItemSave> items;
    }

    [Serializable]
    public class InventoryItemSave
    {
        public string itemId;
        public int quantity;
    }

    [Serializable]
    public class RecipesSaveData
    {
        public List<string> discoveredIds;
    }

    [Serializable]
    public class AchievementsSaveData
    {
        public List<string> unlockedIds;
        public Dictionary<string, int> stats;
    }

    [Serializable]
    public class SpiritsSaveData
    {
        public List<string> ownedIds;
        public string activeId;
    }

    [Serializable]
    public class GardenSaveData
    {
        public List<PlotSaveData> plots;
    }

    [Serializable]
    public class PlotSaveData
    {
        public int plotIndex;
        public string cropId;
        public float growthProgress;
        public bool isWatered;
        public bool isFertilized;
    }

    #endregion
}
