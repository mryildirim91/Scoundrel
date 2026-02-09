using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Scoundrel.UI.Mobile
{
    /// <summary>
    /// Optimizes touch input handling for mobile devices using the New Input System.
    /// Reduces input latency and prevents accidental double-taps.
    /// </summary>
    public class TouchInputOptimizer : MonoBehaviour
    {
        [Header("Input Settings")]
        [Tooltip("Minimum time between accepting consecutive taps on the same target (prevents double-tap)")]
        [SerializeField] private float _tapDebounceTime = 0.15f;

        [Tooltip("Maximum distance a touch can move and still be considered a tap")]
        [SerializeField] private float _tapMaxMovement = 20f;

        [Tooltip("Time before a touch is considered a hold instead of a tap")]
        [SerializeField] private float _holdThreshold = 0.3f;

        [Header("Performance")]
        [Tooltip("Process touches per frame limit (0 = unlimited)")]
        [SerializeField] private int _maxTouchesPerFrame = 2;

        private float _lastTapTime;
        private GameObject _lastTapTarget;
        private Vector2 _touchStartPosition;
        private float _touchStartTime;
        private bool _isTouchActive;
        private bool _holdEventFired;

        // For mouse fallback in editor
        private InputAction _pointerPositionAction;
        private InputAction _pointerPressAction;

        /// <summary>
        /// Gets whether a touch is currently active.
        /// </summary>
        public bool IsTouchActive => _isTouchActive;

        /// <summary>
        /// Gets the current touch position.
        /// </summary>
        public Vector2 CurrentTouchPosition { get; private set; }

        /// <summary>
        /// Event fired when a valid tap is detected.
        /// </summary>
        public event System.Action<Vector2, GameObject> OnTap;

        /// <summary>
        /// Event fired when a hold is detected.
        /// </summary>
        public event System.Action<Vector2, GameObject> OnHoldStart;

        /// <summary>
        /// Event fired when a hold ends.
        /// </summary>
        public event System.Action<Vector2, GameObject> OnHoldEnd;

        private void Awake()
        {
            // Setup mouse/pointer input actions for editor and standalone
            _pointerPositionAction = new InputAction("PointerPosition", binding: "<Pointer>/position");
            _pointerPressAction = new InputAction("PointerPress", binding: "<Pointer>/press");
        }

        private void OnEnable()
        {
            // Enable Enhanced Touch for mobile
            EnhancedTouchSupport.Enable();

            _pointerPositionAction.Enable();
            _pointerPressAction.Enable();

            // Subscribe to touch events
            Touch.onFingerDown += OnFingerDown;
            Touch.onFingerMove += OnFingerMove;
            Touch.onFingerUp += OnFingerUp;
        }

        private void OnDisable()
        {
            // Unsubscribe from touch events
            Touch.onFingerDown -= OnFingerDown;
            Touch.onFingerMove -= OnFingerMove;
            Touch.onFingerUp -= OnFingerUp;

            _pointerPositionAction.Disable();
            _pointerPressAction.Disable();

            EnhancedTouchSupport.Disable();
        }

        private void OnDestroy()
        {
            _pointerPositionAction?.Dispose();
            _pointerPressAction?.Dispose();
        }

        private void Update()
        {
            // Check for hold while touch is active
            if (_isTouchActive && !_holdEventFired)
            {
                float touchDuration = Time.unscaledTime - _touchStartTime;
                if (touchDuration >= _holdThreshold)
                {
                    _holdEventFired = true;
                    GameObject target = GetTargetUnderPosition(CurrentTouchPosition);
                    OnHoldStart?.Invoke(CurrentTouchPosition, target);
                }
            }

            // Fallback: Handle mouse in editor when no touches
            if (Touch.activeTouches.Count == 0)
            {
                ProcessMouseInput();
            }
        }

        private void ProcessMouseInput()
        {
            if (Mouse.current == null) return;

            Vector2 mousePos = _pointerPositionAction.ReadValue<Vector2>();
            bool isPressed = _pointerPressAction.ReadValue<float>() > 0.5f;

            if (isPressed && !_isTouchActive)
            {
                HandleTouchBegan(mousePos);
            }
            else if (isPressed && _isTouchActive)
            {
                CurrentTouchPosition = mousePos;
            }
            else if (!isPressed && _isTouchActive)
            {
                HandleTouchEnded(mousePos);
            }
        }

        private void OnFingerDown(Finger finger)
        {
            // Limit touches processed
            if (_maxTouchesPerFrame > 0 && finger.index >= _maxTouchesPerFrame)
                return;

            HandleTouchBegan(finger.screenPosition);
        }

        private void OnFingerMove(Finger finger)
        {
            if (!_isTouchActive) return;

            CurrentTouchPosition = finger.screenPosition;
        }

        private void OnFingerUp(Finger finger)
        {
            if (!_isTouchActive) return;

            HandleTouchEnded(finger.screenPosition);
        }

        private void HandleTouchBegan(Vector2 position)
        {
            _isTouchActive = true;
            _touchStartPosition = position;
            _touchStartTime = Time.unscaledTime;
            _holdEventFired = false;
            CurrentTouchPosition = position;
        }

        private void HandleTouchEnded(Vector2 position)
        {
            if (!_isTouchActive) return;

            _isTouchActive = false;

            float touchDuration = Time.unscaledTime - _touchStartTime;
            float touchMovement = Vector2.Distance(_touchStartPosition, position);

            GameObject target = GetTargetUnderPosition(position);

            // If it was a hold, fire hold end
            if (_holdEventFired)
            {
                OnHoldEnd?.Invoke(position, target);
                return;
            }

            // Check if this is a valid tap
            if (touchDuration < _holdThreshold && touchMovement <= _tapMaxMovement)
            {
                // Check debounce
                if (CanAcceptTap(target))
                {
                    _lastTapTime = Time.unscaledTime;
                    _lastTapTarget = target;
                    OnTap?.Invoke(position, target);
                }
            }
        }

        private bool CanAcceptTap(GameObject target)
        {
            // Different target = always accept
            if (target != _lastTapTarget)
            {
                return true;
            }

            // Same target = check debounce time
            return Time.unscaledTime - _lastTapTime >= _tapDebounceTime;
        }

        private GameObject GetTargetUnderPosition(Vector2 position)
        {
            if (EventSystem.current == null) return null;

            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = position
            };

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            return results.Count > 0 ? results[0].gameObject : null;
        }

        /// <summary>
        /// Resets the debounce state, allowing immediate taps.
        /// </summary>
        public void ResetDebounce()
        {
            _lastTapTime = 0;
            _lastTapTarget = null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _tapDebounceTime = Mathf.Max(0, _tapDebounceTime);
            _tapMaxMovement = Mathf.Max(0, _tapMaxMovement);
            _holdThreshold = Mathf.Max(0.1f, _holdThreshold);
            _maxTouchesPerFrame = Mathf.Max(0, _maxTouchesPerFrame);
        }
#endif
    }
}
