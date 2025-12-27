using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using WaveIsland.Tutorial;

namespace WaveIsland.UI.Tutorial
{
    /// <summary>
    /// UI controller for tutorial overlays and highlights
    /// </summary>
    public class TutorialUIController : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private CanvasGroup panelCanvasGroup;

        [Header("Dialogue Box")]
        [SerializeField] private GameObject dialogueBox;
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Image characterImage;
        [SerializeField] private Button continueButton;
        [SerializeField] private Text continueButtonText;

        [Header("Highlight")]
        [SerializeField] private GameObject highlightMask;
        [SerializeField] private RectTransform highlightRect;
        [SerializeField] private Image highlightBorder;
        [SerializeField] private float highlightPadding = 20f;

        [Header("Arrow Pointer")]
        [SerializeField] private GameObject arrowPointer;
        [SerializeField] private float arrowBobSpeed = 2f;
        [SerializeField] private float arrowBobAmount = 10f;

        [Header("Progress")]
        [SerializeField] private Slider progressBar;
        [SerializeField] private Text progressText;

        [Header("Skip Button")]
        [SerializeField] private Button skipButton;
        [SerializeField] private GameObject skipConfirmPanel;
        [SerializeField] private Button skipConfirmYes;
        [SerializeField] private Button skipConfirmNo;

        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.2f;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Highlight Targets")]
        [SerializeField] private RectTransform gardenButtonTarget;
        [SerializeField] private RectTransform gardenPlotTarget;
        [SerializeField] private RectTransform craftingButtonTarget;
        [SerializeField] private RectTransform craftingSlotTarget;
        [SerializeField] private RectTransform craftButtonTarget;
        [SerializeField] private RectTransform recipeBookButtonTarget;
        [SerializeField] private RectTransform shopButtonTarget;
        [SerializeField] private RectTransform inventoryButtonTarget;
        [SerializeField] private RectTransform spiritButtonTarget;

        private TutorialStep currentStep;
        private Coroutine arrowAnimation;
        private Coroutine fadeCoroutine;

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
            // Continue button
            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueClicked);

            // Skip button
            if (skipButton != null)
                skipButton.onClick.AddListener(ShowSkipConfirm);

            if (skipConfirmYes != null)
                skipConfirmYes.onClick.AddListener(OnSkipConfirmed);

            if (skipConfirmNo != null)
                skipConfirmNo.onClick.AddListener(HideSkipConfirm);

            // Initial state
            if (tutorialPanel != null)
                tutorialPanel.SetActive(false);

            if (skipConfirmPanel != null)
                skipConfirmPanel.SetActive(false);

            if (highlightMask != null)
                highlightMask.SetActive(false);

            if (arrowPointer != null)
                arrowPointer.SetActive(false);
        }

        private void SubscribeToEvents()
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnStepStarted += OnStepStarted;
                TutorialManager.Instance.OnStepCompleted += OnStepCompleted;
                TutorialManager.Instance.OnTutorialCompleted += OnTutorialCompleted;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnStepStarted -= OnStepStarted;
                TutorialManager.Instance.OnStepCompleted -= OnStepCompleted;
                TutorialManager.Instance.OnTutorialCompleted -= OnTutorialCompleted;
            }
        }

        private void OnStepStarted(TutorialStep step)
        {
            currentStep = step;
            ShowStep(step);
        }

        private void OnStepCompleted(TutorialStep step)
        {
            // Hide current step
            HideHighlight();
        }

        private void OnTutorialCompleted()
        {
            HideTutorial();
            ShowCompletionMessage();
        }

        private void ShowStep(TutorialStep step)
        {
            // Show panel
            if (tutorialPanel != null)
                tutorialPanel.SetActive(true);

            // Fade in
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeIn());

            // Set text
            if (titleText != null)
                titleText.text = step.Title;

            if (descriptionText != null)
                descriptionText.text = step.Description;

            // Character image
            if (characterImage != null && step.CustomImage != null)
            {
                characterImage.sprite = step.CustomImage;
                characterImage.gameObject.SetActive(true);
            }
            else if (characterImage != null)
            {
                characterImage.gameObject.SetActive(false);
            }

            // Continue button text
            if (continueButtonText != null)
            {
                if (step.RequiredAction == TutorialAction.TapToContinue)
                {
                    continueButtonText.text = "Continue";
                    if (continueButton != null)
                        continueButton.gameObject.SetActive(true);
                }
                else
                {
                    if (continueButton != null)
                        continueButton.gameObject.SetActive(false);
                }
            }

            // Position dialogue
            PositionDialogue(step);

            // Show highlight
            if (step.HighlightTarget != TutorialHighlight.None)
            {
                ShowHighlight(step.HighlightTarget, step.HighlightOffset);
            }
            else
            {
                HideHighlight();
            }

            // Update progress
            UpdateProgress();
        }

        private void PositionDialogue(TutorialStep step)
        {
            if (dialogueBox == null) return;

            var rectTransform = dialogueBox.GetComponent<RectTransform>();
            if (rectTransform == null) return;

            // Default position (bottom center)
            Vector2 position = step.DialoguePosition;
            if (position == Vector2.zero)
            {
                position = new Vector2(0, -200);
            }

            rectTransform.anchoredPosition = position;
        }

        private void ShowHighlight(TutorialHighlight target, Vector2 offset)
        {
            RectTransform targetRect = GetHighlightTarget(target);

            if (targetRect == null)
            {
                HideHighlight();
                return;
            }

            // Show mask
            if (highlightMask != null)
                highlightMask.SetActive(true);

            // Position and size highlight
            if (highlightRect != null)
            {
                Vector3[] corners = new Vector3[4];
                targetRect.GetWorldCorners(corners);

                // Convert to local space
                var canvas = GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    RectTransform canvasRect = canvas.GetComponent<RectTransform>();

                    Vector2 min = Vector2.positiveInfinity;
                    Vector2 max = Vector2.negativeInfinity;

                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 localPoint;
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            canvasRect,
                            RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[i]),
                            canvas.worldCamera,
                            out localPoint);

                        min = Vector2.Min(min, localPoint);
                        max = Vector2.Max(max, localPoint);
                    }

                    Vector2 center = (min + max) / 2f + offset;
                    Vector2 size = max - min + Vector2.one * highlightPadding * 2;

                    highlightRect.anchoredPosition = center;
                    highlightRect.sizeDelta = size;
                }
            }

            // Show arrow
            ShowArrow(targetRect, offset);
        }

        private void HideHighlight()
        {
            if (highlightMask != null)
                highlightMask.SetActive(false);

            HideArrow();
        }

        private void ShowArrow(RectTransform target, Vector2 offset)
        {
            if (arrowPointer == null) return;

            arrowPointer.SetActive(true);

            // Position arrow above target
            var arrowRect = arrowPointer.GetComponent<RectTransform>();
            if (arrowRect != null && target != null)
            {
                Vector3 targetPos = target.position;
                targetPos.y += target.rect.height / 2 + 50; // Above target
                arrowRect.position = targetPos;
            }

            // Start animation
            if (arrowAnimation != null)
                StopCoroutine(arrowAnimation);
            arrowAnimation = StartCoroutine(AnimateArrow());
        }

        private void HideArrow()
        {
            if (arrowPointer != null)
                arrowPointer.SetActive(false);

            if (arrowAnimation != null)
            {
                StopCoroutine(arrowAnimation);
                arrowAnimation = null;
            }
        }

        private IEnumerator AnimateArrow()
        {
            if (arrowPointer == null) yield break;

            var rectTransform = arrowPointer.GetComponent<RectTransform>();
            Vector2 startPos = rectTransform.anchoredPosition;

            float time = 0;
            while (true)
            {
                time += Time.deltaTime * arrowBobSpeed;
                float yOffset = Mathf.Sin(time) * arrowBobAmount;
                rectTransform.anchoredPosition = startPos + Vector2.up * yOffset;
                yield return null;
            }
        }

        private RectTransform GetHighlightTarget(TutorialHighlight target)
        {
            switch (target)
            {
                case TutorialHighlight.GardenButton: return gardenButtonTarget;
                case TutorialHighlight.GardenPlot: return gardenPlotTarget;
                case TutorialHighlight.CraftingButton: return craftingButtonTarget;
                case TutorialHighlight.CraftingSlot: return craftingSlotTarget;
                case TutorialHighlight.CraftButton: return craftButtonTarget;
                case TutorialHighlight.RecipeBookButton: return recipeBookButtonTarget;
                case TutorialHighlight.ShopButton: return shopButtonTarget;
                case TutorialHighlight.InventoryButton: return inventoryButtonTarget;
                case TutorialHighlight.SpiritButton: return spiritButtonTarget;
                default: return null;
            }
        }

        private void UpdateProgress()
        {
            if (TutorialManager.Instance == null) return;

            float progress = TutorialManager.Instance.GetProgress();

            if (progressBar != null)
                progressBar.value = progress;

            if (progressText != null)
                progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
        }

        private void OnContinueClicked()
        {
            if (currentStep != null && currentStep.RequiredAction == TutorialAction.TapToContinue)
            {
                TutorialManager.Instance?.CompleteCurrentStep();
            }
        }

        private void ShowSkipConfirm()
        {
            if (skipConfirmPanel != null)
                skipConfirmPanel.SetActive(true);
        }

        private void HideSkipConfirm()
        {
            if (skipConfirmPanel != null)
                skipConfirmPanel.SetActive(false);
        }

        private void OnSkipConfirmed()
        {
            HideSkipConfirm();
            TutorialManager.Instance?.SkipTutorial();
        }

        private void HideTutorial()
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeOut());
        }

        private IEnumerator FadeIn()
        {
            if (panelCanvasGroup == null) yield break;

            panelCanvasGroup.alpha = 0;
            float elapsed = 0;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float t = fadeCurve.Evaluate(elapsed / fadeInDuration);
                panelCanvasGroup.alpha = t;
                yield return null;
            }

            panelCanvasGroup.alpha = 1;
        }

        private IEnumerator FadeOut()
        {
            if (panelCanvasGroup == null) yield break;

            float startAlpha = panelCanvasGroup.alpha;
            float elapsed = 0;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = fadeCurve.Evaluate(elapsed / fadeOutDuration);
                panelCanvasGroup.alpha = startAlpha * (1 - t);
                yield return null;
            }

            panelCanvasGroup.alpha = 0;

            if (tutorialPanel != null)
                tutorialPanel.SetActive(false);
        }

        private void ShowCompletionMessage()
        {
            // Could show a special completion popup here
            Debug.Log("[TutorialUI] Tutorial completed!");
        }

        /// <summary>
        /// Set a custom highlight target at runtime
        /// </summary>
        public void SetHighlightTarget(TutorialHighlight type, RectTransform target)
        {
            switch (type)
            {
                case TutorialHighlight.GardenButton: gardenButtonTarget = target; break;
                case TutorialHighlight.GardenPlot: gardenPlotTarget = target; break;
                case TutorialHighlight.CraftingButton: craftingButtonTarget = target; break;
                case TutorialHighlight.CraftingSlot: craftingSlotTarget = target; break;
                case TutorialHighlight.CraftButton: craftButtonTarget = target; break;
                case TutorialHighlight.RecipeBookButton: recipeBookButtonTarget = target; break;
                case TutorialHighlight.ShopButton: shopButtonTarget = target; break;
                case TutorialHighlight.InventoryButton: inventoryButtonTarget = target; break;
                case TutorialHighlight.SpiritButton: spiritButtonTarget = target; break;
            }
        }
    }
}
