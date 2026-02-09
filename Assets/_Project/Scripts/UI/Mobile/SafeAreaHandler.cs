using UnityEngine;

namespace Scoundrel.UI.Mobile
{
    /// <summary>
    /// Automatically adjusts RectTransform to respect the device safe area.
    /// Handles notches, home indicators, and other screen cutouts.
    /// Attach this to UI panels that should respect safe area boundaries.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaHandler : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Apply safe area to top of screen (for notches)")]
        [SerializeField] private bool _applyTop = true;

        [Tooltip("Apply safe area to bottom of screen (for home indicators)")]
        [SerializeField] private bool _applyBottom = true;

        [Tooltip("Apply safe area to left side of screen")]
        [SerializeField] private bool _applyLeft = true;

        [Tooltip("Apply safe area to right side of screen")]
        [SerializeField] private bool _applyRight = true;

        [Header("Debug")]
        [SerializeField] private bool _simulateSafeArea = false;
        [SerializeField] private Rect _simulatedSafeArea = new Rect(0, 100, 1080, 2140);

        private RectTransform _rectTransform;
        private Rect _lastSafeArea = Rect.zero;
        private Vector2Int _lastScreenSize = Vector2Int.zero;
        private ScreenOrientation _lastOrientation = ScreenOrientation.AutoRotation;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            ApplySafeArea();
        }

        private void Update()
        {
            // Check if safe area or screen has changed
            Rect safeArea = GetSafeArea();
            Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);

            if (safeArea != _lastSafeArea ||
                screenSize != _lastScreenSize ||
                Screen.orientation != _lastOrientation)
            {
                ApplySafeArea();
            }
        }

        private Rect GetSafeArea()
        {
#if UNITY_EDITOR
            if (_simulateSafeArea)
            {
                return _simulatedSafeArea;
            }
#endif
            return Screen.safeArea;
        }

        private void ApplySafeArea()
        {
            Rect safeArea = GetSafeArea();
            Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);

            // Store for comparison
            _lastSafeArea = safeArea;
            _lastScreenSize = screenSize;
            _lastOrientation = Screen.orientation;

            // Avoid division by zero
            if (screenSize.x == 0 || screenSize.y == 0)
            {
                return;
            }

            // Calculate anchors based on safe area
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            // Normalize to 0-1 range
            anchorMin.x /= screenSize.x;
            anchorMin.y /= screenSize.y;
            anchorMax.x /= screenSize.x;
            anchorMax.y /= screenSize.y;

            // Apply only selected sides
            if (!_applyLeft) anchorMin.x = 0;
            if (!_applyBottom) anchorMin.y = 0;
            if (!_applyRight) anchorMax.x = 1;
            if (!_applyTop) anchorMax.y = 1;

            // Apply to RectTransform
            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;

            Debug.Log($"[SafeAreaHandler] Applied safe area: {safeArea} -> anchors ({anchorMin}, {anchorMax})");
        }

        /// <summary>
        /// Forces a refresh of the safe area calculation.
        /// </summary>
        public void Refresh()
        {
            _lastSafeArea = Rect.zero;
            ApplySafeArea();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying && _rectTransform != null)
            {
                ApplySafeArea();
            }
        }
#endif
    }
}
