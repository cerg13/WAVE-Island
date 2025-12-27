using System;
using System.Collections.Generic;
using UnityEngine;

namespace WaveIsland.Core
{
    /// <summary>
    /// Generic object pooling system for performance optimization
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        private readonly T prefab;
        private readonly Transform parent;
        private readonly Queue<T> available = new Queue<T>();
        private readonly List<T> all = new List<T>();
        private readonly int maxSize;
        private readonly Action<T> onGet;
        private readonly Action<T> onRelease;

        public int AvailableCount => available.Count;
        public int TotalCount => all.Count;
        public int ActiveCount => all.Count - available.Count;

        public ObjectPool(T prefab, Transform parent = null, int initialSize = 10, int maxSize = 100,
            Action<T> onGet = null, Action<T> onRelease = null)
        {
            this.prefab = prefab;
            this.parent = parent;
            this.maxSize = maxSize;
            this.onGet = onGet;
            this.onRelease = onRelease;

            // Pre-warm pool
            for (int i = 0; i < initialSize; i++)
            {
                CreateNew();
            }
        }

        private T CreateNew()
        {
            if (all.Count >= maxSize)
            {
                Debug.LogWarning($"ObjectPool: Max size ({maxSize}) reached for {prefab.name}");
                return null;
            }

            T instance = UnityEngine.Object.Instantiate(prefab, parent);
            instance.gameObject.SetActive(false);
            all.Add(instance);
            available.Enqueue(instance);
            return instance;
        }

        public T Get()
        {
            T instance;

            if (available.Count > 0)
            {
                instance = available.Dequeue();
            }
            else
            {
                instance = CreateNew();
                if (instance == null)
                {
                    // Pool exhausted, find oldest active
                    return null;
                }
                available.Dequeue(); // Remove from available since CreateNew adds it
            }

            instance.gameObject.SetActive(true);
            onGet?.Invoke(instance);
            return instance;
        }

        public T Get(Vector3 position, Quaternion rotation)
        {
            T instance = Get();
            if (instance != null)
            {
                instance.transform.position = position;
                instance.transform.rotation = rotation;
            }
            return instance;
        }

        public void Release(T instance)
        {
            if (instance == null) return;

            instance.gameObject.SetActive(false);
            onRelease?.Invoke(instance);

            if (!available.Contains(instance))
            {
                available.Enqueue(instance);
            }
        }

        public void ReleaseAll()
        {
            foreach (var instance in all)
            {
                if (instance != null && instance.gameObject.activeSelf)
                {
                    Release(instance);
                }
            }
        }

        public void Clear()
        {
            foreach (var instance in all)
            {
                if (instance != null)
                {
                    UnityEngine.Object.Destroy(instance.gameObject);
                }
            }
            all.Clear();
            available.Clear();
        }
    }

    /// <summary>
    /// Manager for multiple object pools
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        [Header("Pool Settings")]
        [SerializeField] private int defaultInitialSize = 10;
        [SerializeField] private int defaultMaxSize = 100;

        private Dictionary<string, object> pools = new Dictionary<string, object>();
        private Dictionary<string, Transform> poolContainers = new Dictionary<string, Transform>();

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

        /// <summary>
        /// Create or get a pool for a prefab
        /// </summary>
        public ObjectPool<T> GetPool<T>(T prefab, int initialSize = -1, int maxSize = -1) where T : Component
        {
            string key = prefab.name;

            if (pools.TryGetValue(key, out object existing))
            {
                return existing as ObjectPool<T>;
            }

            // Create container
            if (!poolContainers.TryGetValue(key, out Transform container))
            {
                GameObject containerObj = new GameObject($"Pool_{key}");
                containerObj.transform.SetParent(transform);
                container = containerObj.transform;
                poolContainers[key] = container;
            }

            // Create pool
            var pool = new ObjectPool<T>(
                prefab,
                container,
                initialSize > 0 ? initialSize : defaultInitialSize,
                maxSize > 0 ? maxSize : defaultMaxSize
            );

            pools[key] = pool;
            return pool;
        }

        /// <summary>
        /// Get an instance from pool
        /// </summary>
        public T Spawn<T>(T prefab) where T : Component
        {
            return GetPool(prefab).Get();
        }

        public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            return GetPool(prefab).Get(position, rotation);
        }

        /// <summary>
        /// Return instance to pool
        /// </summary>
        public void Despawn<T>(T instance) where T : Component
        {
            if (instance == null) return;

            string key = instance.name.Replace("(Clone)", "").Trim();

            if (pools.TryGetValue(key, out object pool))
            {
                (pool as ObjectPool<T>)?.Release(instance);
            }
            else
            {
                // Not from pool, just destroy
                Destroy(instance.gameObject);
            }
        }

        /// <summary>
        /// Clear all pools
        /// </summary>
        public void ClearAll()
        {
            foreach (var pool in pools.Values)
            {
                var method = pool.GetType().GetMethod("Clear");
                method?.Invoke(pool, null);
            }
            pools.Clear();

            foreach (var container in poolContainers.Values)
            {
                if (container != null)
                    Destroy(container.gameObject);
            }
            poolContainers.Clear();
        }

        /// <summary>
        /// Get pool statistics
        /// </summary>
        public string GetStats()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== Pool Statistics ===");

            foreach (var kvp in pools)
            {
                var pool = kvp.Value;
                var available = (int)pool.GetType().GetProperty("AvailableCount")?.GetValue(pool);
                var total = (int)pool.GetType().GetProperty("TotalCount")?.GetValue(pool);
                var active = (int)pool.GetType().GetProperty("ActiveCount")?.GetValue(pool);

                sb.AppendLine($"{kvp.Key}: {active}/{total} active ({available} available)");
            }

            return sb.ToString();
        }
    }
}
