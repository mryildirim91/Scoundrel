using UnityEngine;

namespace Scoundrel.UI.Mobile
{
    /// <summary>
    /// Lightweight performance monitor for debugging and profiling.
    /// Displays FPS, frame time, and memory usage in debug builds.
    /// </summary>
    public class PerformanceMonitor : MonoBehaviour
    {
        [Header("Display Settings")]
        [SerializeField] private bool _showInBuild = false;
        [SerializeField] private bool _showFPS = true;
        [SerializeField] private bool _showFrameTime = true;
        [SerializeField] private bool _showMemory = true;

        [Header("Position")]
        [SerializeField] private TextAnchor _anchor = TextAnchor.UpperRight;
        [SerializeField] private Vector2 _offset = new Vector2(10, 10);

        [Header("Update Rate")]
        [SerializeField] private float _updateInterval = 0.5f;

        [Header("Thresholds")]
        [SerializeField] private float _goodFPS = 55f;
        [SerializeField] private float _warningFPS = 30f;

        private float _deltaTime;
        private float _fps;
        private float _frameTimeMs;
        private float _memoryMB;
        private float _timeSinceLastUpdate;
        private int _frameCount;
        private float _accumulatedTime;

        private GUIStyle _style;
        private Rect _displayRect;

        private void Start()
        {
            // Only show in editor or debug builds
            if (!Debug.isDebugBuild && !_showInBuild && !Application.isEditor)
            {
                enabled = false;
                return;
            }

            InitializeStyle();
        }

        private void InitializeStyle()
        {
            _style = new GUIStyle
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = _anchor
            };
            _style.normal.textColor = Color.white;

            // Calculate display rect based on anchor
            float width = 200f;
            float height = 100f;

            float x = _offset.x;
            float y = _offset.y;

            if (_anchor == TextAnchor.UpperRight || _anchor == TextAnchor.MiddleRight || _anchor == TextAnchor.LowerRight)
            {
                x = Screen.width - width - _offset.x;
            }
            else if (_anchor == TextAnchor.UpperCenter || _anchor == TextAnchor.MiddleCenter || _anchor == TextAnchor.LowerCenter)
            {
                x = (Screen.width - width) / 2f;
            }

            if (_anchor == TextAnchor.LowerLeft || _anchor == TextAnchor.LowerCenter || _anchor == TextAnchor.LowerRight)
            {
                y = Screen.height - height - _offset.y;
            }
            else if (_anchor == TextAnchor.MiddleLeft || _anchor == TextAnchor.MiddleCenter || _anchor == TextAnchor.MiddleRight)
            {
                y = (Screen.height - height) / 2f;
            }

            _displayRect = new Rect(x, y, width, height);
        }

        private void Update()
        {
            _deltaTime += Time.unscaledDeltaTime;
            _accumulatedTime += Time.unscaledDeltaTime;
            _frameCount++;

            _timeSinceLastUpdate += Time.unscaledDeltaTime;

            if (_timeSinceLastUpdate >= _updateInterval)
            {
                // Calculate average FPS over the interval
                _fps = _frameCount / _accumulatedTime;
                _frameTimeMs = (_accumulatedTime / _frameCount) * 1000f;

                // Get memory usage (in MB)
                _memoryMB = System.GC.GetTotalMemory(false) / (1024f * 1024f);

                // Reset counters
                _timeSinceLastUpdate = 0f;
                _frameCount = 0;
                _accumulatedTime = 0f;
            }
        }

        private void OnGUI()
        {
            if (_style == null)
            {
                InitializeStyle();
            }

            // Recalculate rect in case screen size changed
            if (Event.current.type == EventType.Layout)
            {
                InitializeStyle();
            }

            // Build display text
            string text = "";

            if (_showFPS)
            {
                Color fpsColor = _fps >= _goodFPS ? Color.green : (_fps >= _warningFPS ? Color.yellow : Color.red);
                string colorHex = ColorUtility.ToHtmlStringRGB(fpsColor);
                text += $"<color=#{colorHex}>FPS: {_fps:F0}</color>\n";
            }

            if (_showFrameTime)
            {
                text += $"Frame: {_frameTimeMs:F1}ms\n";
            }

            if (_showMemory)
            {
                text += $"Memory: {_memoryMB:F1}MB";
            }

            // Draw background
            GUI.Box(_displayRect, "");

            // Draw text
            GUI.Label(_displayRect, text, _style);
        }

        /// <summary>
        /// Gets the current FPS.
        /// </summary>
        public float CurrentFPS => _fps;

        /// <summary>
        /// Gets the current frame time in milliseconds.
        /// </summary>
        public float CurrentFrameTimeMs => _frameTimeMs;

        /// <summary>
        /// Gets the current memory usage in MB.
        /// </summary>
        public float CurrentMemoryMB => _memoryMB;

        /// <summary>
        /// Toggles the performance monitor visibility.
        /// </summary>
        public void Toggle()
        {
            enabled = !enabled;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _updateInterval = Mathf.Max(0.1f, _updateInterval);
            _goodFPS = Mathf.Max(1f, _goodFPS);
            _warningFPS = Mathf.Max(1f, _warningFPS);
        }
#endif
    }
}
