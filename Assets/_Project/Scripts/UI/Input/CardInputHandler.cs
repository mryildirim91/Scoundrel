using System;
using Scoundrel.Core.Data;
using Scoundrel.Core.Enums;
using Scoundrel.Core.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Scoundrel.UI.Input
{
    /// <summary>
    /// Handles card input events including click, hold, and drag.
    /// Shows damage preview when holding on monster cards.
    /// Optimized for mobile touch input with debouncing and drag threshold.
    /// </summary>
    public class CardInputHandler : MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [Header("Settings")]
        [SerializeField] private float _holdThreshold = 0.2f;

        [Header("Mobile Optimization")]
        [Tooltip("Minimum time between accepting consecutive taps (prevents double-tap)")]
        [SerializeField] private float _tapDebounceTime = 0.15f;

        [Tooltip("Maximum distance finger can move and still be considered a tap")]
        [SerializeField] private float _tapMaxMovement = 30f;

        private CardData _cardData;
        private int _slotIndex;
        private ICommandProcessor _commandProcessor;
        private IGameManager _gameManager;
        private Action<int> _onDamagePreviewStart;
        private Action _onDamagePreviewEnd;
        private Action<bool> _onShieldEfficiencyPreview;

        private bool _isPointerDown;
        private bool _isHolding;
        private float _pointerDownTime;
        private bool _isInteractable = true;

        // Mobile optimization fields
        private static float _lastTapTime;
        private static int _lastTapInstanceId;
        private Vector2 _pointerDownPosition;
        private bool _hasMoved;

        /// <summary>
        /// Event fired when this card is clicked (quick tap).
        /// </summary>
        public event Action<CardData> OnCardClicked;

        /// <summary>
        /// Initializes the input handler with dependencies.
        /// </summary>
        public void Initialize(
            ICommandProcessor commandProcessor,
            IGameManager gameManager,
            Action<int> onDamagePreviewStart,
            Action onDamagePreviewEnd,
            Action<bool> onShieldEfficiencyPreview)
        {
            _commandProcessor = commandProcessor;
            _gameManager = gameManager;
            _onDamagePreviewStart = onDamagePreviewStart;
            _onDamagePreviewEnd = onDamagePreviewEnd;
            _onShieldEfficiencyPreview = onShieldEfficiencyPreview;
        }

        /// <summary>
        /// Sets the card data for this handler.
        /// </summary>
        public void SetCard(CardData card, int slotIndex)
        {
            _cardData = card;
            _slotIndex = slotIndex;
        }

        /// <summary>
        /// Sets whether this card is interactable.
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            _isInteractable = interactable;

            if (!interactable)
            {
                ClearHoldState();
            }
        }

        private void Update()
        {
            if (!_isPointerDown || !_isInteractable) return;

            // Check for hold threshold
            if (!_isHolding && Time.unscaledTime - _pointerDownTime >= _holdThreshold)
            {
                _isHolding = true;
                ShowPreview();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_isInteractable) return;
            if (_gameManager != null && !_gameManager.CanAcceptInput) return;

            _isPointerDown = true;
            _pointerDownTime = Time.unscaledTime;
            _pointerDownPosition = eventData.position;
            _hasMoved = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isInteractable) return;

            bool wasHolding = _isHolding;
            float heldDuration = Time.unscaledTime - _pointerDownTime;

            // Check if finger moved too much (not a tap)
            float movement = Vector2.Distance(_pointerDownPosition, eventData.position);
            _hasMoved = movement > _tapMaxMovement;

            ClearHoldState();

            // If not holding (quick tap), treat as click
            if (!wasHolding && heldDuration < _holdThreshold && !_hasMoved)
            {
                // Check debounce - prevent double-taps on the same card
                int instanceId = GetInstanceID();
                if (CanAcceptTap(instanceId))
                {
                    if (_gameManager != null && _gameManager.CanAcceptInput)
                    {
                        _lastTapTime = Time.unscaledTime;
                        _lastTapInstanceId = instanceId;
                        OnCardClicked?.Invoke(_cardData);
                    }
                }
            }
        }

        private bool CanAcceptTap(int instanceId)
        {
            // Different card = always accept
            if (instanceId != _lastTapInstanceId)
            {
                return true;
            }

            // Same card = check debounce time
            return Time.unscaledTime - _lastTapTime >= _tapDebounceTime;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Optional: Show tooltip or highlight
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // Clear preview if pointer exits while holding
            if (_isHolding)
            {
                ClearHoldState();
            }
        }

        private void ShowPreview()
        {
            if (_cardData.IsMonster)
            {
                // Show damage preview
                int damage = _commandProcessor?.CalculateDamagePreview(_cardData) ?? _cardData.Value;
                _onDamagePreviewStart?.Invoke(damage);

                // Show reduced shield efficiency for Clubs
                if (_cardData.Suit == CardSuit.Clubs)
                {
                    _onShieldEfficiencyPreview?.Invoke(true);
                }
            }
        }

        private void ClearHoldState()
        {
            bool wasHolding = _isHolding;
            _isPointerDown = false;
            _isHolding = false;

            if (wasHolding)
            {
                _onDamagePreviewEnd?.Invoke();
                _onShieldEfficiencyPreview?.Invoke(false);
            }
        }

        private void OnDisable()
        {
            ClearHoldState();
        }
    }
}
