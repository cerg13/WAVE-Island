using UnityEngine;
using System.IO;

namespace WaveIsland.Core
{
    /// <summary>
    /// Handles saving and loading player data to persistent storage
    /// </summary>
    public static class SaveSystem
    {
        private const string SAVE_FILE_NAME = "player_data.json";
        private const string BACKUP_FILE_NAME = "player_data_backup.json";

        private static string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        private static string BackupPath => Path.Combine(Application.persistentDataPath, BACKUP_FILE_NAME);

        /// <summary>
        /// Save player data to file
        /// </summary>
        public static void SavePlayerData(PlayerData data)
        {
            if (data == null)
            {
                Debug.LogError("[SaveSystem] Cannot save null player data");
                return;
            }

            try
            {
                // Create backup of existing save
                if (File.Exists(SavePath))
                {
                    File.Copy(SavePath, BackupPath, true);
                }

                data.LastSaveAt = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SavePath, json);

                Debug.Log($"[SaveSystem] Saved to: {SavePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveSystem] Save failed: {e.Message}");
            }
        }

        /// <summary>
        /// Load player data from file
        /// </summary>
        public static PlayerData LoadPlayerData()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    string json = File.ReadAllText(SavePath);
                    var data = JsonUtility.FromJson<PlayerData>(json);

                    data.LastLoginAt = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                    Debug.Log($"[SaveSystem] Loaded from: {SavePath}");
                    return data;
                }

                // Try loading backup
                if (File.Exists(BackupPath))
                {
                    Debug.LogWarning("[SaveSystem] Main save not found, loading backup...");
                    string json = File.ReadAllText(BackupPath);
                    var data = JsonUtility.FromJson<PlayerData>(json);
                    data.LastLoginAt = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    return data;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveSystem] Load failed: {e.Message}");
            }

            Debug.Log("[SaveSystem] No save file found, will create new player data");
            return null;
        }

        /// <summary>
        /// Delete all save data
        /// </summary>
        public static void DeleteSaveData()
        {
            try
            {
                if (File.Exists(SavePath))
                    File.Delete(SavePath);

                if (File.Exists(BackupPath))
                    File.Delete(BackupPath);

                Debug.Log("[SaveSystem] Save data deleted");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveSystem] Delete failed: {e.Message}");
            }
        }

        /// <summary>
        /// Check if save exists
        /// </summary>
        public static bool HasSaveData()
        {
            return File.Exists(SavePath) || File.Exists(BackupPath);
        }
    }
}
