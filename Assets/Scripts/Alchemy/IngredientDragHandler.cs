using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using WaveIsland.Data;

namespace WaveIsland.Alchemy
{
    /// <summary>
    /// Handles drag-and-drop for ingredients
    /// Attached to ingredient items in the inventory panel
    /// </summary>
    public class IngredientDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [Header("References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Text quantityText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Drag Settings")]
        [SerializeField] private float dragAlpha = 0.7f;
        [SerializeField] private Vector2 dragOffset = new Vector2(0, 30);

        public IngredientData DraggedIngredient { get; private set; }
        public int Quantity { get; private set; }

        private Canvas rootCanvas;
        private RectTransform rectTransform;
        private Vector3 originalPosition;
        private Transform originalParent;
        private int originalSiblingIndex;
        private bool isDragging = false;

        public event System.Action<IngredientData> OnIngredientClicked;
        public event System.Action<IngredientData> OnDragStarted;
        public event System.Action<IngredientData, bool> OnDragEnded;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            // Find root canvas
            rootCanvas = GetComponentInParent<Canvas>();
            while (rootCanvas != null && !rootCanvas.isRootCanvas)
            {
                rootCanvas = rootCanvas.transform.parent?.GetComponentInParent<Canvas>();
            }
        }

        /// <summary>
        /// Setup the ingredient display
        /// </summary>
        public void Setup(IngredientData ingredient, int quantity)
        {
            DraggedIngredient = ingredient;
            Quantity = quantity;

            if (iconImage != null)
            {
                iconImage.sprite = ingredient?.Icon;
                iconImage.enabled = ingredient != null;
            }

            if (quantityText != null)
            {
                quantityText.text = quantity > 1 ? $"x{quantity}" : "";
            }
        }

        /// <summary>
        /// Update quantity display
        /// </summary>
        public void UpdateQuantity(int newQuantity)
        {
            Quantity = newQuantity;

            if (quantityText != null)
            {
                quantityText.text = newQuantity > 1 ? $"x{newQuantity}" : "";
            }

            // Disable if no more items
            if (newQuantity <= 0)
            {
                gameObject.SetActive(false);
            }
        }

        // IBeginDragHandler
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (DraggedIngredient == null || Quantity <= 0)
            {
                eventData.pointerDrag = null;
                return;
            }

            isDragging = true;

            // Store original state
            originalPosition = rectTransform.position;
            originalParent = transform.parent;
            originalSiblingIndex = transform.GetSiblingIndex();

            // Move to root canvas for proper layering
            if (rootCanvas != null)
            {
                transform.SetParent(rootCanvas.transform);
            }

            // Visual feedback
            canvasGroup.alpha = dragAlpha;
            canvasGroup.blocksRaycasts = false;

            OnDragStarted?.Invoke(DraggedIngredient);
        }

        // IDragHandler
        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            // Follow pointer with offset
            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rootCanvas.transform as RectTransform,
                eventData.position + dragOffset,
                rootCanvas.worldCamera,
                out position);

            rectTransform.position = rootCanvas.transform.TransformPoint(position);
        }

        // IEndDragHandler
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            isDragging = false;

            // Check if dropped on valid target
            bool wasDroppedOnSlot = false;

            // Raycast to find what we're over
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
            {
                var slot = result.gameObject.GetComponent<CraftingSlot>();
                if (slot != null && !slot.IsLocked && !slot.HasIngredient)
                {
                    wasDroppedOnSlot = true;
                    break;
                }
            }

            // Reset visual
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            // Return to original parent
            transform.SetParent(originalParent);
            transform.SetSiblingIndex(originalSiblingIndex);
            rectTransform.position = originalPosition;

            OnDragEnded?.Invoke(DraggedIngredient, wasDroppedOnSlot);
        }

        // IPointerClickHandler
        public void OnPointerClick(PointerEventData eventData)
        {
            if (isDragging) return;
            if (DraggedIngredient == null || Quantity <= 0) return;

            OnIngredientClicked?.Invoke(DraggedIngredient);
        }
    }
}
