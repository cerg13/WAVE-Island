using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using WaveIsland.Core;
using WaveIsland.Data;
using WaveIsland.Spirits;

namespace WaveIsland.UI.Spirits
{
    /// <summary>
    /// UI for viewing spirit collection/album
    /// </summary>
    public class SpiritCollectionUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject collectionPanel;
        [SerializeField] private Button closeButton;

        [Header("Tabs")]
        [SerializeField] private Button ownedTabButton;
        [SerializeField] private Button allTabButton;
        [SerializeField] private Color activeTabColor = Color.white;
        [SerializeField] private Color inactiveTabColor = new Color(0.7f, 0.7f, 0.7f);

        [Header("Filter")]
        [SerializeField] private Dropdown elementFilter;
        [SerializeField] private Dropdown rarityFilter;

        [Header("Spirit Grid")]
        [SerializeField] private Transform spiritGridContent;
        [SerializeField] private GameObject spiritCardPrefab;
        [SerializeField] private ScrollRect scrollRect;

        [Header("Collection Stats")]
        [SerializeField] private Text collectionProgressText;
        [SerializeField] private Slider collectionProgressBar;
        [SerializeField] private Text totalBonusText;

        [Header("Active Spirit")]
        [SerializeField] private Image activeSpiritIcon;
        [SerializeField] private Text activeSpiritName;
        [SerializeField] private Text activeSpiritBonus;
        [SerializeField] private Button changeActiveButton;

        [Header("Spirit Detail Panel")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private Image detailIcon;
        [SerializeField] private Image detailRarityBorder;
        [SerializeField] private Text detailName;
        [SerializeField] private Text detailRarity;
        [SerializeField] private Text detailElement;
        [SerializeField] private Text detailDescription;
        [SerializeField] private Text detailLore;
        [SerializeField] private Transform detailAbilitiesContainer;
        [SerializeField] private GameObject abilityItemPrefab;
        [SerializeField] private Button setActiveButton;
        [SerializeField] private Button detailCloseButton;
        [SerializeField] private Text detailLevelText;

        [Header("Rarity Colors")]
        [SerializeField] private Color commonColor = Color.gray;
        [SerializeField] private Color uncommonColor = Color.green;
        [SerializeField] private Color rareColor = Color.blue;
        [SerializeField] private Color epicColor = new Color(0.6f, 0.2f, 0.8f);
        [SerializeField] private Color legendaryColor = new Color(1f, 0.8f, 0f);

        private bool showOwnedOnly = true;
        private string selectedElement = "All";
        private string selectedRarity = "All";
        private SpiritJsonData selectedSpirit;
        private List<SpiritCardUI> spiritCards = new List<SpiritCardUI>();

        private void Start()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);

            if (ownedTabButton != null)
                ownedTabButton.onClick.AddListener(() => SetTab(true));

            if (allTabButton != null)
                allTabButton.onClick.AddListener(() => SetTab(false));

            if (detailCloseButton != null)
                detailCloseButton.onClick.AddListener(CloseDetail);

            if (setActiveButton != null)
                setActiveButton.onClick.AddListener(OnSetActiveClicked);

            if (changeActiveButton != null)
                changeActiveButton.onClick.AddListener(() => SetTab(true));

            // Setup filters
            SetupFilters();

            // Initial state
            if (detailPanel != null)
                detailPanel.SetActive(false);
        }

        private void SetupFilters()
        {
            if (elementFilter != null)
            {
                elementFilter.ClearOptions();
                var elements = new List<string> { "All", "Fire", "Water", "Earth", "Air", "Nature", "Moon", "Sun", "Smoke" };
                elementFilter.AddOptions(elements);
                elementFilter.onValueChanged.AddListener(OnElementFilterChanged);
            }

            if (rarityFilter != null)
            {
                rarityFilter.ClearOptions();
                var rarities = new List<string> { "All", "Common", "Uncommon", "Rare", "Epic", "Legendary" };
                rarityFilter.AddOptions(rarities);
                rarityFilter.onValueChanged.AddListener(OnRarityFilterChanged);
            }
        }

        public void Open()
        {
            if (collectionPanel != null)
                collectionPanel.SetActive(true);

            RefreshCollection();
            UpdateStats();
            UpdateActiveSpirit();
        }

        public void Close()
        {
            if (collectionPanel != null)
                collectionPanel.SetActive(false);
        }

        private void SetTab(bool ownedOnly)
        {
            showOwnedOnly = ownedOnly;

            // Update tab visuals
            if (ownedTabButton != null)
            {
                var colors = ownedTabButton.colors;
                colors.normalColor = ownedOnly ? activeTabColor : inactiveTabColor;
                ownedTabButton.colors = colors;
            }

            if (allTabButton != null)
            {
                var colors = allTabButton.colors;
                colors.normalColor = !ownedOnly ? activeTabColor : inactiveTabColor;
                allTabButton.colors = colors;
            }

            RefreshCollection();
        }

        private void OnElementFilterChanged(int index)
        {
            selectedElement = elementFilter.options[index].text;
            RefreshCollection();
        }

        private void OnRarityFilterChanged(int index)
        {
            selectedRarity = rarityFilter.options[index].text;
            RefreshCollection();
        }

        private void RefreshCollection()
        {
            if (spiritGridContent == null) return;

            // Clear existing cards
            foreach (var card in spiritCards)
            {
                if (card != null)
                    Destroy(card.gameObject);
            }
            spiritCards.Clear();

            // Get spirits
            var allSpirits = SpiritDatabase.GetAllSpirits();
            var playerData = GameManager.Instance?.PlayerData;
            var ownedIds = playerData?.UnlockedSpiritIds ?? new List<string>();

            // Filter
            var filtered = allSpirits.Where(s =>
            {
                // Owned filter
                if (showOwnedOnly && !ownedIds.Contains(s.id))
                    return false;

                // Element filter
                if (selectedElement != "All" && s.element != selectedElement)
                    return false;

                // Rarity filter
                if (selectedRarity != "All" && s.rarity != selectedRarity)
                    return false;

                return true;
            }).ToList();

            // Sort by rarity then name
            filtered = filtered
                .OrderBy(s => GetRarityOrder(s.rarity))
                .ThenBy(s => s.displayName)
                .ToList();

            // Create cards
            foreach (var spirit in filtered)
            {
                CreateSpiritCard(spirit, ownedIds.Contains(spirit.id));
            }

            // Reset scroll
            if (scrollRect != null)
                scrollRect.verticalNormalizedPosition = 1f;
        }

        private void CreateSpiritCard(SpiritJsonData spirit, bool isOwned)
        {
            GameObject cardObj;

            if (spiritCardPrefab != null)
            {
                cardObj = Instantiate(spiritCardPrefab, spiritGridContent);
            }
            else
            {
                cardObj = CreateBasicCard();
                cardObj.transform.SetParent(spiritGridContent);
            }

            var card = cardObj.GetComponent<SpiritCardUI>();
            if (card == null)
            {
                card = cardObj.AddComponent<SpiritCardUI>();
            }

            card.Setup(spirit, isOwned, GetRarityColor(spirit.rarity));
            card.OnClicked += () => ShowSpiritDetail(spirit, isOwned);

            spiritCards.Add(card);
        }

        private GameObject CreateBasicCard()
        {
            var obj = new GameObject("SpiritCard");

            var bg = obj.AddComponent<Image>();
            var rect = obj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, 120);

            var btn = obj.AddComponent<Button>();

            // Name text
            var textObj = new GameObject("Name");
            textObj.transform.SetParent(obj.transform);
            var text = textObj.AddComponent<Text>();
            text.alignment = TextAnchor.LowerCenter;
            text.fontSize = 12;

            return obj;
        }

        private void ShowSpiritDetail(SpiritJsonData spirit, bool isOwned)
        {
            selectedSpirit = spirit;

            if (detailPanel == null) return;
            detailPanel.SetActive(true);

            // Basic info
            if (detailName != null)
                detailName.text = isOwned ? spirit.displayName : "???";

            if (detailRarity != null)
            {
                detailRarity.text = spirit.rarity;
                detailRarity.color = GetRarityColor(spirit.rarity);
            }

            if (detailElement != null)
                detailElement.text = spirit.element;

            if (detailDescription != null)
                detailDescription.text = isOwned ? spirit.description : "Undiscovered spirit";

            if (detailLore != null)
            {
                detailLore.text = isOwned ? spirit.lore : "";
                detailLore.gameObject.SetActive(isOwned);
            }

            if (detailRarityBorder != null)
                detailRarityBorder.color = GetRarityColor(spirit.rarity);

            // Level (if owned)
            if (detailLevelText != null)
            {
                if (isOwned)
                {
                    // Get level from owned data
                    var owned = SpiritManager.Instance?.GetOwnedSpirits()
                        .Find(o => o.Data.Id == spirit.id);
                    int level = owned?.Level ?? 1;
                    detailLevelText.text = $"Level {level}/{spirit.maxLevel}";
                    detailLevelText.gameObject.SetActive(true);
                }
                else
                {
                    detailLevelText.gameObject.SetActive(false);
                }
            }

            // Abilities
            ShowAbilities(spirit, isOwned);

            // Set active button
            if (setActiveButton != null)
            {
                setActiveButton.gameObject.SetActive(isOwned);
            }
        }

        private void ShowAbilities(SpiritJsonData spirit, bool isOwned)
        {
            if (detailAbilitiesContainer == null) return;

            // Clear existing
            foreach (Transform child in detailAbilitiesContainer)
            {
                Destroy(child.gameObject);
            }

            if (!isOwned)
            {
                // Show locked abilities
                CreateAbilityItem("???", "Unlock this spirit to see abilities");
                return;
            }

            foreach (var ability in spirit.abilities)
            {
                CreateAbilityItem(ability.name, ability.description);
            }
        }

        private void CreateAbilityItem(string name, string description)
        {
            GameObject itemObj;

            if (abilityItemPrefab != null)
            {
                itemObj = Instantiate(abilityItemPrefab, detailAbilitiesContainer);
            }
            else
            {
                itemObj = new GameObject("Ability");
                itemObj.transform.SetParent(detailAbilitiesContainer);

                var text = itemObj.AddComponent<Text>();
                text.fontSize = 14;
            }

            var texts = itemObj.GetComponentsInChildren<Text>();
            if (texts.Length >= 2)
            {
                texts[0].text = name;
                texts[1].text = description;
            }
            else if (texts.Length == 1)
            {
                texts[0].text = $"{name}: {description}";
            }
        }

        private void CloseDetail()
        {
            if (detailPanel != null)
                detailPanel.SetActive(false);

            selectedSpirit = null;
        }

        private void OnSetActiveClicked()
        {
            if (selectedSpirit == null) return;

            SpiritManager.Instance?.SetActiveSpirit(selectedSpirit.id);
            UpdateActiveSpirit();
            CloseDetail();

            Debug.Log($"[SpiritCollection] Set active spirit: {selectedSpirit.displayName}");
        }

        private void UpdateActiveSpirit()
        {
            var activeSpirit = SpiritManager.Instance?.ActiveSpirit;

            if (activeSpirit != null)
            {
                if (activeSpiritName != null)
                    activeSpiritName.text = activeSpirit.DisplayName;

                if (activeSpiritIcon != null)
                {
                    activeSpiritIcon.sprite = activeSpirit.Icon;
                    activeSpiritIcon.enabled = activeSpirit.Icon != null;
                }

                if (activeSpiritBonus != null)
                {
                    var bonuses = activeSpirit.Abilities
                        .Select(a => $"+{a.BaseValue}% {a.AbilityName}")
                        .ToList();
                    activeSpiritBonus.text = string.Join(", ", bonuses);
                }
            }
            else
            {
                if (activeSpiritName != null)
                    activeSpiritName.text = "No active spirit";

                if (activeSpiritIcon != null)
                    activeSpiritIcon.enabled = false;

                if (activeSpiritBonus != null)
                    activeSpiritBonus.text = "Select a spirit for bonuses";
            }
        }

        private void UpdateStats()
        {
            var allSpirits = SpiritDatabase.GetAllSpirits();
            var playerData = GameManager.Instance?.PlayerData;
            var ownedCount = playerData?.UnlockedSpiritIds?.Count ?? 0;
            int totalCount = allSpirits.Count;

            if (collectionProgressText != null)
                collectionProgressText.text = $"{ownedCount}/{totalCount}";

            if (collectionProgressBar != null)
            {
                collectionProgressBar.maxValue = totalCount;
                collectionProgressBar.value = ownedCount;
            }

            // Calculate total bonus from collection
            if (totalBonusText != null)
            {
                float totalBonus = CalculateCollectionBonus();
                totalBonusText.text = $"Collection Bonus: +{totalBonus:F0}%";
            }
        }

        private float CalculateCollectionBonus()
        {
            var playerData = GameManager.Instance?.PlayerData;
            if (playerData == null) return 0;

            float bonus = 0;
            foreach (var spiritId in playerData.UnlockedSpiritIds)
            {
                var spirit = SpiritDatabase.GetSpirit(spiritId);
                if (spirit != null)
                {
                    // Small bonus for each owned spirit
                    bonus += GetRarityBonus(spirit.rarity);
                }
            }

            return bonus;
        }

        private float GetRarityBonus(string rarity)
        {
            switch (rarity.ToLower())
            {
                case "common": return 0.5f;
                case "uncommon": return 1f;
                case "rare": return 2f;
                case "epic": return 5f;
                case "legendary": return 10f;
                default: return 0.5f;
            }
        }

        private int GetRarityOrder(string rarity)
        {
            switch (rarity.ToLower())
            {
                case "legendary": return 0;
                case "epic": return 1;
                case "rare": return 2;
                case "uncommon": return 3;
                case "common": return 4;
                default: return 5;
            }
        }

        private Color GetRarityColor(string rarity)
        {
            switch (rarity.ToLower())
            {
                case "common": return commonColor;
                case "uncommon": return uncommonColor;
                case "rare": return rareColor;
                case "epic": return epicColor;
                case "legendary": return legendaryColor;
                default: return commonColor;
            }
        }
    }

    /// <summary>
    /// UI component for a spirit card in the collection
    /// </summary>
    public class SpiritCardUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image rarityBorder;
        [SerializeField] private Text nameText;
        [SerializeField] private GameObject lockedOverlay;
        [SerializeField] private Button button;

        public event System.Action OnClicked;

        private SpiritJsonData spirit;
        private bool isOwned;

        private void Awake()
        {
            if (button == null)
                button = GetComponent<Button>();

            if (button != null)
                button.onClick.AddListener(() => OnClicked?.Invoke());
        }

        public void Setup(SpiritJsonData spiritData, bool owned, Color rarityColor)
        {
            spirit = spiritData;
            isOwned = owned;

            if (nameText != null)
            {
                nameText.text = owned ? spirit.displayName : "???";
                nameText.color = owned ? Color.white : Color.gray;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = owned ? rarityColor * 0.5f : new Color(0.2f, 0.2f, 0.2f, 0.8f);
            }

            if (rarityBorder != null)
            {
                rarityBorder.color = rarityColor;
            }

            if (iconImage != null)
            {
                iconImage.color = owned ? Color.white : new Color(0.3f, 0.3f, 0.3f);
            }

            if (lockedOverlay != null)
            {
                lockedOverlay.SetActive(!owned);
            }
        }
    }
}
