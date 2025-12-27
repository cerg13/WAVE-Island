using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace WaveIsland.Data
{
    /// <summary>
    /// Database for accessing ingredient data
    /// </summary>
    [CreateAssetMenu(fileName = "IngredientDatabase", menuName = "WAVE Island/Database/Ingredients")]
    public class IngredientDatabase : ScriptableObject
    {
        private static IngredientDatabase _instance;
        public static IngredientDatabase Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<IngredientDatabase>("IngredientDatabase");
                }
                return _instance;
            }
        }

        [Header("All Ingredients")]
        [SerializeField] private List<IngredientData> allIngredients = new List<IngredientData>();

        /// <summary>
        /// Get ingredient by ID
        /// </summary>
        public static IngredientData GetIngredient(string ingredientId)
        {
            return Instance?.allIngredients.Find(i => i.Id == ingredientId);
        }

        /// <summary>
        /// Get all ingredients
        /// </summary>
        public static List<IngredientData> GetAllIngredients()
        {
            return Instance?.allIngredients ?? new List<IngredientData>();
        }

        /// <summary>
        /// Get ingredients by category
        /// </summary>
        public static List<IngredientData> GetIngredientsByCategory(IngredientCategory category)
        {
            return Instance?.allIngredients.Where(i => i.Category == category).ToList()
                ?? new List<IngredientData>();
        }

        /// <summary>
        /// Get purchasable ingredients
        /// </summary>
        public static List<IngredientData> GetPurchasableIngredients()
        {
            return Instance?.allIngredients.Where(i => i.CanBePurchased).ToList()
                ?? new List<IngredientData>();
        }

        /// <summary>
        /// Get growable ingredients
        /// </summary>
        public static List<IngredientData> GetGrowableIngredients()
        {
            return Instance?.allIngredients.Where(i => i.CanBeGrown).ToList()
                ?? new List<IngredientData>();
        }
    }
}
