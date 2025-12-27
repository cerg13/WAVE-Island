using UnityEngine;
using UnityEngine.UI;

namespace WaveIsland.UI
{
    /// <summary>
    /// Main UI Manager - handles all UI panels and navigation
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Main Panels")]
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject gardenPanel;
        [SerializeField] private GameObject alchemyPanel;
        [SerializeField] private GameObject spiritPanel;
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private GameObject recipeBookPanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("Popup Panels")]
        [SerializeField] private GameObject rewardPopup;
        [SerializeField] private GameObject confirmPopup;
        [SerializeField] private GameObject tutorialPopup;
        [SerializeField] private GameObject loadingOverlay;

        [Header("HUD Elements")]
        [SerializeField] private Text coinsText;
        [SerializeField] private Text gemsText;
        [SerializeField] private Text levelText;
        [SerializeField] private Slider expSlider;

        private GameObject currentPanel;

        public event System.Action<string> OnPanelOpened;
        public event System.Action<string> OnPanelClosed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            InitializeUI();
            ShowPanel(gardenPanel); // Default to garden
        }

        private void InitializeUI()
        {
            // Hide all panels initially
            HideAllPanels();

            // Show HUD
            if (hudPanel != null)
                hudPanel.SetActive(true);

            UpdateHUD();
        }

        /// <summary>
        /// Update HUD elements with current player data
        /// </summary>
        public void UpdateHUD()
        {
            var playerData = Core.GameManager.Instance?.PlayerData;
            if (playerData == null) return;

            if (coinsText != null)
                coinsText.text = FormatNumber(playerData.Coins);

            if (gemsText != null)
                gemsText.text = FormatNumber(playerData.Gems);

            if (levelText != null)
                levelText.text = $"Lv.{playerData.Level}";

            if (expSlider != null)
            {
                int expNeeded = 100 * playerData.Level * playerData.Level;
                expSlider.value = (float)playerData.Experience / expNeeded;
            }
        }

        /// <summary>
        /// Show specific panel
        /// </summary>
        public void ShowPanel(GameObject panel)
        {
            if (currentPanel != null)
            {
                currentPanel.SetActive(false);
                OnPanelClosed?.Invoke(currentPanel.name);
            }

            if (panel != null)
            {
                panel.SetActive(true);
                currentPanel = panel;
                OnPanelOpened?.Invoke(panel.name);
            }
        }

        public void ShowGarden() => ShowPanel(gardenPanel);
        public void ShowAlchemy() => ShowPanel(alchemyPanel);
        public void ShowSpirits() => ShowPanel(spiritPanel);
        public void ShowShop() => ShowPanel(shopPanel);
        public void ShowRecipeBook() => ShowPanel(recipeBookPanel);
        public void ShowSettings() => ShowPanel(settingsPanel);

        /// <summary>
        /// Show popup over current panel
        /// </summary>
        public void ShowPopup(GameObject popup)
        {
            if (popup != null)
                popup.SetActive(true);
        }

        public void HidePopup(GameObject popup)
        {
            if (popup != null)
                popup.SetActive(false);
        }

        /// <summary>
        /// Show reward popup
        /// </summary>
        public void ShowReward(string title, string description, Sprite icon = null)
        {
            if (rewardPopup != null)
            {
                // TODO: Set popup content
                rewardPopup.SetActive(true);
            }
        }

        /// <summary>
        /// Show confirmation dialog
        /// </summary>
        public void ShowConfirm(string message, System.Action onConfirm, System.Action onCancel = null)
        {
            if (confirmPopup != null)
            {
                // TODO: Set confirm content and callbacks
                confirmPopup.SetActive(true);
            }
        }

        /// <summary>
        /// Show/hide loading overlay
        /// </summary>
        public void SetLoading(bool isLoading)
        {
            if (loadingOverlay != null)
                loadingOverlay.SetActive(isLoading);
        }

        private void HideAllPanels()
        {
            if (gardenPanel != null) gardenPanel.SetActive(false);
            if (alchemyPanel != null) alchemyPanel.SetActive(false);
            if (spiritPanel != null) spiritPanel.SetActive(false);
            if (shopPanel != null) shopPanel.SetActive(false);
            if (recipeBookPanel != null) recipeBookPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);

            if (rewardPopup != null) rewardPopup.SetActive(false);
            if (confirmPopup != null) confirmPopup.SetActive(false);
            if (tutorialPopup != null) tutorialPopup.SetActive(false);
        }

        private string FormatNumber(int number)
        {
            if (number >= 1000000)
                return $"{number / 1000000f:F1}M";
            if (number >= 1000)
                return $"{number / 1000f:F1}K";
            return number.ToString();
        }
    }
}
