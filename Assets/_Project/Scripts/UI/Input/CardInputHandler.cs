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
    /// </summary>
    public class CardInputHandler : MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [Header("Settings")]
        [SerializeField] private float _holdThreshold = 0.2f;

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
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isInteractable) return;

            bool wasHolding = _isHolding;
            float heldDuration = Time.unscaledTime - _pointerDownTime;

            ClearHoldState();

            // If not holding (quick tap), treat as click
            if (!wasHolding && heldDuration < _holdThreshold)
            {
                if (_gameManager != null && _gameManager.CanAcceptInput)
                {
                    OnCardClicked?.Invoke(_cardData);
                }
            }
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
