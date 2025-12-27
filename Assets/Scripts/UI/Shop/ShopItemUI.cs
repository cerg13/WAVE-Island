using UnityEngine;
using UnityEngine.UI;
using System;
using WaveIsland.Shop;

namespace WaveIsland.UI.Shop
{
    /// <summary>
    /// UI component for a single shop item
    /// </summary>
    public class ShopItemUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Text nameText;
        [SerializeField] private Text priceText;
        [SerializeField] private Text originalPriceText;
        [SerializeField] private Text ownedText;
        [SerializeField] private Button button;

        [Header("Status Indicators")]
        [SerializeField] private GameObject specialBadge;
        [SerializeField] private GameObject lockedOverlay;
        [SerializeField] private Text lockReasonText;
        [SerializeField] private Image rarityBorder;

        [Header("Sell Mode")]
        [SerializeField] private GameObject sellIndicator;

        public event Action OnClicked;

        private ShopItem shopItem;
        private bool isSellMode;

        private void Awake()
        {
            if (button == null)
                button = GetComponent<Button>();

            if (button != null)
                button.onClick.AddListener(() => OnClicked?.Invoke());
        }

        public void Setup(ShopItem item, bool sellMode = false)
        {
            shopItem = item;
            isSellMode = sellMode;

            // Icon
            if (iconImage != null)
            {
                iconImage.sprite = item.Icon;
                iconImage.enabled = item.Icon != null;
                iconImage.color = item.IsAvailable ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            }

            // Name
            if (nameText != null)
            {
                nameText.text = item.DisplayName;
                nameText.color = item.IsAvailable ? Color.white : Color.gray;
            }

            // Price
            if (priceText != null)
            {
                if (sellMode)
                {
                    // Show sell price
                    int sellPrice = GetSellPrice(item);
                    priceText.text = sellPrice > 0 ? $"+{sellPrice}" : "N/A";
                    priceText.color = sellPrice > 0 ? new Color(0.3f, 0.8f, 0.3f) : Color.gray;
                }
                else
                {
                    priceText.text = $"{item.Price}";
                    priceText.color = item.IsSpecial ? new Color(1f, 0.8f, 0f) : Color.white;
                }
            }

            // Original price (for specials)
            if (originalPriceText != null)
            {
                if (!sellMode && item.IsSpecial && item.OriginalPrice > item.Price)
                {
                    originalPriceText.gameObject.SetActive(true);
                    originalPriceText.text = $"<s>{item.OriginalPrice}</s>";
                }
                else
                {
                    originalPriceText.gameObject.SetActive(false);
                }
            }

            // Owned count
            if (ownedText != null)
            {
                if (item.PlayerOwned > 0)
                {
                    ownedText.gameObject.SetActive(true);
                    ownedText.text = $"x{item.PlayerOwned}";
                }
                else
                {
                    ownedText.gameObject.SetActive(false);
                }
            }

            // Special badge
            if (specialBadge != null)
            {
                specialBadge.SetActive(item.IsSpecial && !sellMode);
            }

            // Locked overlay
            if (lockedOverlay != null)
            {
                lockedOverlay.SetActive(!item.IsAvailable);
            }

            if (lockReasonText != null && !item.IsAvailable)
            {
                lockReasonText.text = $"Lv. {item.RequiredLevel}";
            }

            // Rarity border color
            if (rarityBorder != null)
            {
                rarityBorder.color = GetRarityColor(item.Rarity);
            }

            // Background color based on rarity
            if (backgroundImage != null)
            {
                backgroundImage.color = GetBackgroundColor(item.Rarity);
            }

            // Sell mode indicator
            if (sellIndicator != null)
            {
                sellIndicator.SetActive(sellMode && item.PlayerOwned > 0 && CanSell(item));
            }

            // Button interactability
            if (button != null)
            {
                if (sellMode)
                {
                    button.interactable = item.PlayerOwned > 0 && CanSell(item);
                }
                else
                {
                    button.interactable = true; // Can click to see details even if locked
                }
            }
        }

        private int GetSellPrice(ShopItem item)
        {
            if (item.ItemType == ShopItemType.Ingredient)
            {
                var ingredient = IngredientDatabase.GetIngredient(item.Id);
                return ingredient?.SellPrice ?? 0;
            }
            return 0; // Seeds can't be sold
        }

        private bool CanSell(ShopItem item)
        {
            return item.ItemType == ShopItemType.Ingredient;
        }

        private Color GetRarityColor(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Common: return new Color(0.7f, 0.7f, 0.7f);
                case ItemRarity.Uncommon: return new Color(0.3f, 0.8f, 0.3f);
                case ItemRarity.Rare: return new Color(0.3f, 0.5f, 1f);
                case ItemRarity.Epic: return new Color(0.6f, 0.2f, 0.8f);
                case ItemRarity.Legendary: return new Color(1f, 0.6f, 0f);
                default: return Color.white;
            }
        }

        private Color GetBackgroundColor(ItemRarity rarity)
        {
            var baseColor = GetRarityColor(rarity);
            return new Color(baseColor.r * 0.3f, baseColor.g * 0.3f, baseColor.b * 0.3f, 0.8f);
        }

        /// <summary>
        /// Refresh the display with updated data
        /// </summary>
        public void Refresh()
        {
            if (shopItem != null)
            {
                Setup(shopItem, isSellMode);
            }
        }
    }
}
