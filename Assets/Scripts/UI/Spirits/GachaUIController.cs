using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using WaveIsland.Core;
using WaveIsland.Spirits;
using WaveIsland.Data;

namespace WaveIsland.UI.Spirits
{
    /// <summary>
    /// UI controller for the gacha/summoning screen
    /// </summary>
    public class GachaUIController : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject gachaPanel;
        [SerializeField] private Button closeButton;

        [Header("Pull Buttons")]
        [SerializeField] private Button singlePullButton;
        [SerializeField] private Button tenPullButton;
        [SerializeField] private Text singlePullCostText;
        [SerializeField] private Text tenPullCostText;

        [Header("Player Resources")]
        [SerializeField] private Text gemsText;
        [SerializeField] private Text ticketsText;

        [Header("Pity Display")]
        [SerializeField] private Slider pitySlider;
        [SerializeField] private Text pityText;
        [SerializeField] private int hardPity = 90;

        [Header("Result Display")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private Transform resultContainer;
        [SerializeField] private GameObject spiritResultPrefab;
        [SerializeField] private Button resultCloseButton;
        [SerializeField] private Button pullAgainButton;

        [Header("Animation")]
        [SerializeField] private GameObject summonCircle;
        [SerializeField] private ParticleSystem summonParticles;
        [SerializeField] private float pullAnimationDuration = 2f;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip pullSound;
        [SerializeField] private AudioClip revealSound;
        [SerializeField] private AudioClip legendarySound;

        [Header("Rarity Colors")]
        [SerializeField] private Color commonColor = Color.gray;
        [SerializeField] private Color uncommonColor = Color.green;
        [SerializeField] private Color rareColor = Color.blue;
        [SerializeField] private Color epicColor = new Color(0.6f, 0.2f, 0.8f);
        [SerializeField] private Color legendaryColor = new Color(1f, 0.8f, 0f);

        private bool isPulling = false;
        private List<GachaPullResult> pendingResults = new List<GachaPullResult>();

        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void InitializeUI()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);

            if (singlePullButton != null)
                singlePullButton.onClick.AddListener(OnSinglePullClicked);

            if (tenPullButton != null)
                tenPullButton.onClick.AddListener(OnTenPullClicked);

            if (resultCloseButton != null)
                resultCloseButton.onClick.AddListener(CloseResults);

            if (pullAgainButton != null)
                pullAgainButton.onClick.AddListener(OnPullAgainClicked);

            // Initial state
            if (resultPanel != null)
                resultPanel.SetActive(false);

            if (summonCircle != null)
                summonCircle.SetActive(false);

            UpdateCostDisplay();
        }

        private void SubscribeToEvents()
        {
            if (GachaSystem.Instance != null)
            {
                GachaSystem.Instance.OnPullComplete += OnPullComplete;
                GachaSystem.Instance.OnMultiPullComplete += OnMultiPullComplete;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (GachaSystem.Instance != null)
            {
                GachaSystem.Instance.OnPullComplete -= OnPullComplete;
                GachaSystem.Instance.OnMultiPullComplete -= OnMultiPullComplete;
            }
        }

        public void Open()
        {
            if (gachaPanel != null)
                gachaPanel.SetActive(true);

            UpdateUI();
        }

        public void Close()
        {
            if (isPulling) return;

            if (gachaPanel != null)
                gachaPanel.SetActive(false);
        }

        private void UpdateUI()
        {
            UpdateResourceDisplay();
            UpdatePityDisplay();
            UpdateButtonStates();
        }

        private void UpdateResourceDisplay()
        {
            var playerData = GameManager.Instance?.PlayerData;

            if (gemsText != null)
                gemsText.text = $"{playerData?.Gems ?? 0}";

            if (ticketsText != null)
            {
                int tickets = playerData?.GetItemCount("gacha_ticket") ?? 0;
                ticketsText.text = $"{tickets}";
            }
        }

        private void UpdateCostDisplay()
        {
            if (GachaSystem.Instance == null) return;

            if (singlePullCostText != null)
                singlePullCostText.text = $"{GachaSystem.Instance.GetSinglePullCost()}";

            if (tenPullCostText != null)
                tenPullCostText.text = $"{GachaSystem.Instance.GetTenPullCost()}";
        }

        private void UpdatePityDisplay()
        {
            if (GachaSystem.Instance == null) return;

            int pity = GachaSystem.Instance.GetPityCounter();
            int untilPity = GachaSystem.Instance.GetPullsUntilPity();

            if (pitySlider != null)
            {
                pitySlider.maxValue = hardPity;
                pitySlider.value = pity;
            }

            if (pityText != null)
            {
                if (untilPity <= 20)
                {
                    pityText.text = $"<color=#FFD700>Pity: {pity}/{hardPity} (Guaranteed in {untilPity}!)</color>";
                }
                else
                {
                    pityText.text = $"Pity: {pity}/{hardPity}";
                }
            }
        }

        private void UpdateButtonStates()
        {
            if (GachaSystem.Instance == null) return;

            if (singlePullButton != null)
                singlePullButton.interactable = !isPulling && GachaSystem.Instance.CanAffordSinglePull();

            if (tenPullButton != null)
                tenPullButton.interactable = !isPulling && GachaSystem.Instance.CanAffordTenPull();
        }

        private void OnSinglePullClicked()
        {
            if (isPulling) return;

            StartCoroutine(PerformPullAnimation(false));
        }

        private void OnTenPullClicked()
        {
            if (isPulling) return;

            StartCoroutine(PerformPullAnimation(true));
        }

        private IEnumerator PerformPullAnimation(bool isTenPull)
        {
            isPulling = true;
            UpdateButtonStates();

            // Show summon circle
            if (summonCircle != null)
                summonCircle.SetActive(true);

            // Play particles
            if (summonParticles != null)
                summonParticles.Play();

            // Play sound
            PlaySound(pullSound);

            // Wait for animation
            yield return new WaitForSeconds(pullAnimationDuration);

            // Perform actual pull
            if (isTenPull)
            {
                var results = GachaSystem.Instance?.TenPull();
                if (results != null)
                {
                    pendingResults = results;
                }
            }
            else
            {
                var result = GachaSystem.Instance?.SinglePull();
                if (result != null && result.Success)
                {
                    pendingResults = new List<GachaPullResult> { result };
                }
            }

            // Hide summon circle
            if (summonCircle != null)
                summonCircle.SetActive(false);

            if (summonParticles != null)
                summonParticles.Stop();

            isPulling = false;
            UpdateUI();
        }

        private void OnPullComplete(GachaPullResult result)
        {
            if (result.Success)
            {
                ShowResults(new List<GachaPullResult> { result });
            }
        }

        private void OnMultiPullComplete(List<GachaPullResult> results)
        {
            ShowResults(results);
        }

        private void ShowResults(List<GachaPullResult> results)
        {
            if (resultPanel == null) return;

            // Clear previous results
            foreach (Transform child in resultContainer)
            {
                Destroy(child.gameObject);
            }

            // Show panel
            resultPanel.SetActive(true);

            // Check for legendary
            bool hasLegendary = false;
            foreach (var result in results)
            {
                if (result.Rarity == SpiritRarity.Legendary)
                {
                    hasLegendary = true;
                    break;
                }
            }

            PlaySound(hasLegendary ? legendarySound : revealSound);

            // Create result displays
            StartCoroutine(RevealResultsSequentially(results));
        }

        private IEnumerator RevealResultsSequentially(List<GachaPullResult> results)
        {
            // Sort by rarity (highest last for drama)
            var sorted = new List<GachaPullResult>(results);
            sorted.Sort((a, b) => a.Rarity.CompareTo(b.Rarity));

            foreach (var result in sorted)
            {
                CreateResultItem(result);
                yield return new WaitForSeconds(0.2f);
            }
        }

        private void CreateResultItem(GachaPullResult result)
        {
            GameObject itemObj;

            if (spiritResultPrefab != null)
            {
                itemObj = Instantiate(spiritResultPrefab, resultContainer);
            }
            else
            {
                itemObj = CreateBasicResultItem();
                itemObj.transform.SetParent(resultContainer);
            }

            // Setup display
            var icon = itemObj.GetComponentInChildren<Image>();
            var nameText = itemObj.GetComponentInChildren<Text>();
            var background = itemObj.GetComponent<Image>();

            if (nameText != null)
            {
                string newBadge = result.IsNew ? " <color=#FFD700>NEW!</color>" : "";
                nameText.text = $"{result.Spirit.displayName}{newBadge}";
            }

            if (background != null)
            {
                background.color = GetRarityColor(result.Rarity);
            }

            // Animation
            var rectTransform = itemObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                StartCoroutine(PopInAnimation(rectTransform));
            }
        }

        private GameObject CreateBasicResultItem()
        {
            var obj = new GameObject("SpiritResult");

            var bg = obj.AddComponent<Image>();
            bg.color = commonColor;

            var rect = obj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(120, 140);

            // Add name text
            var textObj = new GameObject("Name");
            textObj.transform.SetParent(obj.transform);
            var text = textObj.AddComponent<Text>();
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 14;

            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return obj;
        }

        private IEnumerator PopInAnimation(RectTransform rect)
        {
            Vector3 startScale = Vector3.zero;
            Vector3 endScale = Vector3.one;

            rect.localScale = startScale;

            float elapsed = 0;
            float duration = 0.3f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Overshoot animation
                float overshoot = 1.1f;
                float scale;
                if (t < 0.7f)
                {
                    scale = Mathf.Lerp(0, overshoot, t / 0.7f);
                }
                else
                {
                    scale = Mathf.Lerp(overshoot, 1f, (t - 0.7f) / 0.3f);
                }

                rect.localScale = Vector3.one * scale;
                yield return null;
            }

            rect.localScale = endScale;
        }

        private Color GetRarityColor(SpiritRarity rarity)
        {
            switch (rarity)
            {
                case SpiritRarity.Common: return commonColor;
                case SpiritRarity.Uncommon: return uncommonColor;
                case SpiritRarity.Rare: return rareColor;
                case SpiritRarity.Epic: return epicColor;
                case SpiritRarity.Legendary: return legendaryColor;
                default: return commonColor;
            }
        }

        private void CloseResults()
        {
            if (resultPanel != null)
                resultPanel.SetActive(false);

            pendingResults.Clear();
            UpdateUI();
        }

        private void OnPullAgainClicked()
        {
            CloseResults();

            // Auto-pull based on last action
            if (GachaSystem.Instance.CanAffordTenPull())
            {
                OnTenPullClicked();
            }
            else if (GachaSystem.Instance.CanAffordSinglePull())
            {
                OnSinglePullClicked();
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
}
