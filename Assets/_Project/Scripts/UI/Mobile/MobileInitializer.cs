using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

namespace Scoundrel.UI.Mobile
{
    /// <summary>
    /// Initializes mobile-specific settings on game start.
    /// Handles portrait mode lock, target frame rate, and other mobile optimizations.
    /// Uses the New Input System for touch handling.
    /// </summary>
    public class MobileInitializer : MonoBehaviour
    {
        [Header("Screen Settings")]
        [Tooltip("Lock the game to portrait orientation")]
        [SerializeField] private bool _lockToPortrait = true;

        [Tooltip("Target frame rate for the game (0 = platform default)")]
        [SerializeField] private int _targetFrameRate = 60;

        [Tooltip("Allow the screen to sleep when idle")]
        [SerializeField] private bool _allowScreenSleep = false;

        [Header("Quality Settings")]
        [Tooltip("Enable enhanced touch support (New Input System)")]
        [SerializeField] private bool _enableEnhancedTouch = true;

        [Tooltip("VSync count (0 = off, 1 = every v-blank, 2 = every other)")]
        [SerializeField] private int _vSyncCount = 0;

        [Header("Performance")]
        [Tooltip("Reduce rendering when the app is in background")]
        [SerializeField] private bool _runInBackground = false;

        private void Awake()
        {
            ApplySettings();
        }

        private void ApplySettings()
        {
            // Portrait mode lock
            if (_lockToPortrait)
            {
                Screen.orientation = ScreenOrientation.Portrait;
                Screen.autorotateToPortrait = true;
                Screen.autorotateToPortraitUpsideDown = false;
                Screen.autorotateToLandscapeLeft = false;
                Screen.autorotateToLandscapeRight = false;

                Debug.Log("[MobileInitializer] Locked to portrait orientation");
            }

            // Target frame rate
            if (_targetFrameRate > 0)
            {
                Application.targetFrameRate = _targetFrameRate;
                Debug.Log($"[MobileInitializer] Target frame rate set to {_targetFrameRate}");
            }

            // VSync
            QualitySettings.vSyncCount = _vSyncCount;

            // Screen sleep
            Screen.sleepTimeout = _allowScreenSleep ? SleepTimeout.SystemSetting : SleepTimeout.NeverSleep;

            // Enhanced Touch (New Input System) - enables multi-touch support
            if (_enableEnhancedTouch && !EnhancedTouchSupport.enabled)
            {
                EnhancedTouchSupport.Enable();
                Debug.Log("[MobileInitializer] Enhanced Touch enabled");
            }

            // Background running
            Application.runInBackground = _runInBackground;

            Debug.Log("[MobileInitializer] Mobile settings applied");
        }

        /// <summary>
        /// Reapplies all mobile settings. Call this after settings changes.
        /// </summary>
        public void RefreshSettings()
        {
            ApplySettings();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Clamp values
            _targetFrameRate = Mathf.Max(0, _targetFrameRate);
            _vSyncCount = Mathf.Clamp(_vSyncCount, 0, 4);
        }
#endif
    }
}
