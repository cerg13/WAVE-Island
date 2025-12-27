using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using WaveIsland.Data;

namespace WaveIsland.Alchemy
{
    /// <summary>
    /// Single crafting slot that can hold an ingredient
    /// Supports drag-and-drop interaction
    /// </summary>
    public class CraftingSlot : MonoBehaviour, IDropHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI References")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image ingredientIcon;
        [SerializeField] private Image highlightOverlay;
        [SerializeField] private Image lockedOverlay;
        [SerializeField] private Text quantityText;
        [SerializeField] private GameObject addIcon;

        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color highlightColor = new Color(0.8f, 1f, 0.8f);
        [SerializeField] private Color filledColor = new Color(0.9f, 0.95f, 1f);
        [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        [Header("Animation")]
        [SerializeField] private float bounceScale = 1.1f;
        [SerializeField] private float bounceDuration = 0.15f;

        public int SlotIndex { get; private set; }
        public bool IsLocked { get; private set; }
        public bool HasIngredient => currentIngredient != null;
        public IngredientData CurrentIngredient => currentIngredient;

        private CraftingTable parentTable;
        private IngredientData currentIngredient;
        private bool isHighlighted = false;

        public event System.Action<int, IngredientData> OnIngredientDropped;
        public event System.Action<int> OnSlotClicked;

        /// <summary>
        /// Initialize the slot
        /// </summary>
        public void Initialize(int index, CraftingTable table)
        {
            SlotIndex = index;
            parentTable = table;

            SetupVisuals();
            ClearIngredient();
        }

        private void SetupVisuals()
        {
            // Create UI elements if not assigned
            if (backgroundImage == null)
            {
                var bgObj = new GameObject("Background");
                bgObj.transform.SetParent(transform);
                backgroundImage = bgObj.AddComponent<Image>();
            }

            if (ingredientIcon == null)
            {
                var iconObj = new GameObject("Icon");
                iconObj.transform.SetParent(transform);
                ingredientIcon = iconObj.AddComponent<Image>();
                ingredientIcon.preserveAspect = true;
            }

            if (highlightOverlay == null)
            {
                var highlightObj = new GameObject("Highlight");
                highlightObj.transform.SetParent(transform);
                highlightOverlay = highlightObj.AddComponent<Image>();
                highlightOverlay.color = new Color(1, 1, 1, 0);
            }

            UpdateVisuals();
        }

        /// <summary>
        /// Set ingredient in this slot
        /// </summary>
        public void SetIngredient(IngredientData ingredient)
        {
            currentIngredient = ingredient;

            if (ingredientIcon != null)
            {
                ingredientIcon.sprite = ingredient?.Icon;
                ingredientIcon.enabled = ingredient != null;
            }

            if (addIcon != null)
            {
                addIcon.SetActive(ingredient == null && !IsLocked);
            }

            UpdateVisuals();

            // Play bounce animation
            if (ingredient != null)
            {
                StartCoroutine(BounceAnimation());
            }
        }

        /// <summary>
        /// Clear the slot
        /// </summary>
        public void ClearIngredient()
        {
            currentIngredient = null;

            if (ingredientIcon != null)
            {
                ingredientIcon.sprite = null;
                ingredientIcon.enabled = false;
            }

            if (addIcon != null)
            {
                addIcon.SetActive(!IsLocked);
            }

            UpdateVisuals();
        }

        /// <summary>
        /// Set recipe result display (for result slot)
        /// </summary>
        public void SetRecipeResult(RecipeData recipe)
        {
            if (ingredientIcon != null)
            {
                ingredientIcon.sprite = recipe?.Icon;
                ingredientIcon.enabled = recipe != null;
            }

            if (recipe != null)
            {
                StartCoroutine(BounceAnimation());
            }
        }

        /// <summary>
        /// Set locked state
        /// </summary>
        public void SetLocked(bool locked)
        {
            IsLocked = locked;

            if (lockedOverlay != null)
            {
                lockedOverlay.gameObject.SetActive(locked);
            }

            if (addIcon != null)
            {
                addIcon.SetActive(!locked && currentIngredient == null);
            }

            UpdateVisuals();
        }

        /// <summary>
        /// Set highlight state (for drag over)
        /// </summary>
        public void SetHighlight(bool highlight)
        {
            isHighlighted = highlight;

            if (highlightOverlay != null)
            {
                highlightOverlay.color = highlight
                    ? new Color(highlightColor.r, highlightColor.g, highlightColor.b, 0.3f)
                    : new Color(1, 1, 1, 0);
            }

            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (backgroundImage == null) return;

            if (IsLocked)
            {
                backgroundImage.color = lockedColor;
            }
            else if (currentIngredient != null)
            {
                backgroundImage.color = filledColor;
            }
            else if (isHighlighted)
            {
                backgroundImage.color = highlightColor;
            }
            else
            {
                backgroundImage.color = normalColor;
            }
        }

        private System.Collections.IEnumerator BounceAnimation()
        {
            Vector3 originalScale = transform.localScale;
            Vector3 bounceScaleVec = originalScale * bounceScale;

            float elapsed = 0f;
            float halfDuration = bounceDuration / 2f;

            // Scale up
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                transform.localScale = Vector3.Lerp(originalScale, bounceScaleVec, t);
                yield return null;
            }

            // Scale down
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                transform.localScale = Vector3.Lerp(bounceScaleVec, originalScale, t);
                yield return null;
            }

            transform.localScale = originalScale;
        }

        // IDropHandler
        public void OnDrop(PointerEventData eventData)
        {
            if (IsLocked) return;
            if (HasIngredient) return;

            var dragHandler = eventData.pointerDrag?.GetComponent<IngredientDragHandler>();
            if (dragHandler != null && dragHandler.DraggedIngredient != null)
            {
                OnIngredientDropped?.Invoke(SlotIndex, dragHandler.DraggedIngredient);
            }
        }

        // IPointerClickHandler
        public void OnPointerClick(PointerEventData eventData)
        {
            if (IsLocked) return;

            OnSlotClicked?.Invoke(SlotIndex);
        }

        // IPointerEnterHandler
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (IsLocked) return;
            if (HasIngredient) return;

            // Check if dragging ingredient
            if (eventData.dragging)
            {
                var dragHandler = eventData.pointerDrag?.GetComponent<IngredientDragHandler>();
                if (dragHandler != null)
                {
                    SetHighlight(true);
                }
            }
        }

        // IPointerExitHandler
        public void OnPointerExit(PointerEventData eventData)
        {
            SetHighlight(false);
        }
    }
}
