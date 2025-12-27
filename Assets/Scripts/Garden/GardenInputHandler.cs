using UnityEngine;
using UnityEngine.EventSystems;
using WaveIsland.Core;

namespace WaveIsland.Garden
{
    /// <summary>
    /// Handles touch/mouse input for the garden
    /// Supports both mobile touch and desktop mouse input
    /// </summary>
    public class GardenInputHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera gardenCamera;
        [SerializeField] private GardenGrid gardenGrid;
        [SerializeField] private GardenManager gardenManager;

        [Header("Settings")]
        [SerializeField] private LayerMask plotLayerMask;
        [SerializeField] private float dragThreshold = 10f;
        [SerializeField] private float longPressTime = 0.5f;

        [Header("Camera Control")]
        [SerializeField] private bool enableCameraControl = true;
        [SerializeField] private float panSpeed = 0.5f;
        [SerializeField] private float zoomSpeed = 0.5f;
        [SerializeField] private float minZoom = 3f;
        [SerializeField] private float maxZoom = 10f;
        [SerializeField] private Bounds cameraBounds;

        // Input state
        private Vector2 touchStartPosition;
        private float touchStartTime;
        private bool isDragging = false;
        private bool isLongPress = false;
        private int hoveredPlotIndex = -1;

        // Camera state
        private Vector3 lastPanPosition;
        private bool isPanning = false;

        public event System.Action<int> OnPlotTapped;
        public event System.Action<int> OnPlotLongPressed;
        public event System.Action<int> OnPlotHovered;

        private void Awake()
        {
            if (gardenCamera == null)
                gardenCamera = Camera.main;
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            HandleMouseInput();
#else
            HandleTouchInput();
#endif
        }

        private void HandleMouseInput()
        {
            // Check if over UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Vector2 mousePosition = Input.mousePosition;

            // Hover detection
            UpdateHover(mousePosition);

            // Click/drag
            if (Input.GetMouseButtonDown(0))
            {
                OnPointerDown(mousePosition);
            }
            else if (Input.GetMouseButton(0))
            {
                OnPointerHeld(mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                OnPointerUp(mousePosition);
            }

            // Zoom with scroll wheel
            if (enableCameraControl)
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (scroll != 0)
                {
                    Zoom(-scroll * zoomSpeed * 10f);
                }

                // Middle mouse pan
                if (Input.GetMouseButton(2))
                {
                    if (!isPanning)
                    {
                        isPanning = true;
                        lastPanPosition = mousePosition;
                    }
                    else
                    {
                        Vector3 delta = (Vector3)mousePosition - lastPanPosition;
                        Pan(-delta * panSpeed * 0.01f);
                        lastPanPosition = mousePosition;
                    }
                }
                else
                {
                    isPanning = false;
                }
            }
        }

        private void HandleTouchInput()
        {
            if (Input.touchCount == 0)
                return;

            // Check if over UI
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                return;

            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    OnPointerDown(touch.position);
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    OnPointerHeld(touch.position);
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    OnPointerUp(touch.position);
                    break;
            }

            // Two-finger zoom
            if (enableCameraControl && Input.touchCount == 2)
            {
                Touch touch0 = Input.GetTouch(0);
                Touch touch1 = Input.GetTouch(1);

                Vector2 touch0Prev = touch0.position - touch0.deltaPosition;
                Vector2 touch1Prev = touch1.position - touch1.deltaPosition;

                float prevMagnitude = (touch0Prev - touch1Prev).magnitude;
                float currentMagnitude = (touch0.position - touch1.position).magnitude;

                float difference = currentMagnitude - prevMagnitude;
                Zoom(-difference * zoomSpeed * 0.01f);
            }
        }

        private void OnPointerDown(Vector2 position)
        {
            touchStartPosition = position;
            touchStartTime = Time.time;
            isDragging = false;
            isLongPress = false;

            if (enableCameraControl)
            {
                lastPanPosition = position;
            }
        }

        private void OnPointerHeld(Vector2 position)
        {
            float distance = Vector2.Distance(position, touchStartPosition);
            float holdTime = Time.time - touchStartTime;

            // Check for drag
            if (distance > dragThreshold)
            {
                isDragging = true;

                if (enableCameraControl && Input.touchCount == 1)
                {
                    Vector3 delta = (Vector3)position - lastPanPosition;
                    Pan(-delta * panSpeed * 0.01f);
                    lastPanPosition = position;
                }
            }

            // Check for long press
            if (!isDragging && !isLongPress && holdTime >= longPressTime)
            {
                isLongPress = true;
                int plotIndex = GetPlotAtScreenPosition(touchStartPosition);
                if (plotIndex >= 0)
                {
                    OnPlotLongPressed?.Invoke(plotIndex);
                }
            }
        }

        private void OnPointerUp(Vector2 position)
        {
            if (!isDragging && !isLongPress)
            {
                // Simple tap
                int plotIndex = GetPlotAtScreenPosition(position);
                if (plotIndex >= 0)
                {
                    OnPlotTapped?.Invoke(plotIndex);
                }
            }

            isDragging = false;
            isLongPress = false;
        }

        private void UpdateHover(Vector2 screenPosition)
        {
            int plotIndex = GetPlotAtScreenPosition(screenPosition);

            if (plotIndex != hoveredPlotIndex)
            {
                hoveredPlotIndex = plotIndex;
                OnPlotHovered?.Invoke(plotIndex);
            }
        }

        /// <summary>
        /// Get plot index at screen position
        /// </summary>
        private int GetPlotAtScreenPosition(Vector2 screenPosition)
        {
            Ray ray = gardenCamera.ScreenPointToRay(screenPosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, plotLayerMask))
            {
                var plotVisual = hit.collider.GetComponent<PlotVisual>();
                if (plotVisual != null)
                {
                    return plotVisual.PlotIndex;
                }
            }

            // Try grid-based detection as fallback
            if (gardenGrid != null)
            {
                Vector3 worldPoint = gardenCamera.ScreenToWorldPoint(
                    new Vector3(screenPosition.x, screenPosition.y, gardenCamera.transform.position.y));

                return gardenGrid.GetPlotIndexAtPosition(worldPoint);
            }

            return -1;
        }

        /// <summary>
        /// Pan the camera
        /// </summary>
        private void Pan(Vector3 delta)
        {
            Vector3 newPosition = gardenCamera.transform.position + delta;

            // Clamp to bounds
            newPosition.x = Mathf.Clamp(newPosition.x, cameraBounds.min.x, cameraBounds.max.x);
            newPosition.z = Mathf.Clamp(newPosition.z, cameraBounds.min.z, cameraBounds.max.z);

            gardenCamera.transform.position = newPosition;
        }

        /// <summary>
        /// Zoom the camera
        /// </summary>
        private void Zoom(float delta)
        {
            if (gardenCamera.orthographic)
            {
                gardenCamera.orthographicSize = Mathf.Clamp(
                    gardenCamera.orthographicSize + delta,
                    minZoom,
                    maxZoom);
            }
            else
            {
                Vector3 pos = gardenCamera.transform.position;
                pos.y = Mathf.Clamp(pos.y + delta, minZoom, maxZoom);
                gardenCamera.transform.position = pos;
            }
        }

        /// <summary>
        /// Focus camera on a specific plot
        /// </summary>
        public void FocusOnPlot(int plotIndex)
        {
            if (gardenGrid == null) return;

            Vector3 plotPosition = gardenGrid.GetPlotPosition(plotIndex);
            Vector3 cameraPosition = gardenCamera.transform.position;

            cameraPosition.x = plotPosition.x;
            cameraPosition.z = plotPosition.z - 2f; // Offset to center view

            gardenCamera.transform.position = cameraPosition;
        }

        /// <summary>
        /// Reset camera to default position
        /// </summary>
        public void ResetCamera()
        {
            gardenCamera.transform.position = new Vector3(0, 10f, -5f);
            if (gardenCamera.orthographic)
            {
                gardenCamera.orthographicSize = 5f;
            }
        }
    }
}
