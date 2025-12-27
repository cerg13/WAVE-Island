using UnityEngine;
using UnityEngine.UI;
using WaveIsland.Data;

namespace WaveIsland.UI.Garden
{
    /// <summary>
    /// UI item for seed selection list
    /// </summary>
    public class SeedListItem : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Text nameText;
        [SerializeField] private Text quantityText;
        [SerializeField] private Text growthTimeText;
        [SerializeField] private Button selectButton;
        [SerializeField] private Image rarityBorder;

        [Header("Rarity Colors")]
        [SerializeField] private Color commonColor = Color.white;
        [SerializeField] private Color uncommonColor = Color.green;
        [SerializeField] private Color rareColor = Color.blue;
        [SerializeField] private Color epicColor = new Color(0.5f, 0f, 0.5f);
        [SerializeField] private Color legendaryColor = new Color(1f, 0.8f, 0f);

        private PlantData plantData;
        private System.Action<PlantData> onSelectCallback;

        public void Setup(PlantData plant, int quantity, System.Action<PlantData> onSelect)
        {
            plantData = plant;
            onSelectCallback = onSelect;

            // Setup visuals
            if (iconImage != null && plant.SeedIcon != null)
            {
                iconImage.sprite = plant.SeedIcon;
            }

            if (nameText != null)
            {
                nameText.text = plant.DisplayName;
            }

            if (quantityText != null)
            {
                quantityText.text = $"x{quantity}";
            }

            if (growthTimeText != null)
            {
                growthTimeText.text = FormatGrowthTime(plant.GrowthTimeMinutes);
            }

            // Rarity border
            if (rarityBorder != null)
            {
                rarityBorder.color = GetRarityColor(plant.Rarity);
            }

            // Setup button
            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(OnClick);
            }
            else
            {
                // Add button component if not present
                var btn = gameObject.GetComponent<Button>();
                if (btn == null)
                {
                    btn = gameObject.AddComponent<Button>();
                }
                btn.onClick.AddListener(OnClick);
            }
        }

        private void OnClick()
        {
            onSelectCallback?.Invoke(plantData);
        }

        private string FormatGrowthTime(float minutes)
        {
            if (minutes < 60)
                return $"{Mathf.RoundToInt(minutes)}m";

            float hours = minutes / 60f;
            if (hours < 24)
                return $"{hours:F1}h";

            float days = hours / 24f;
            return $"{days:F1}d";
        }

        private Color GetRarityColor(IngredientRarity rarity)
        {
            switch (rarity)
            {
                case IngredientRarity.Common: return commonColor;
                case IngredientRarity.Uncommon: return uncommonColor;
                case IngredientRarity.Rare: return rareColor;
                case IngredientRarity.Epic: return epicColor;
                case IngredientRarity.Legendary: return legendaryColor;
                default: return commonColor;
            }
        }
    }
}
