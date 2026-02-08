using System;
using Scoundrel.Core.Data;
using Scoundrel.Core.Interfaces;
using Scoundrel.UI.Input;
using UnityEngine;
using UnityEngine.UI;

namespace Scoundrel.UI.Views
{
    /// <summary>
    /// Visual representation of a card in the room.
    /// Handles display and interaction.
    /// </summary>
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Button))]
    public class CardView : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Image _cardImage;
        [SerializeField] private Button _button;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private CardInputHandler _inputHandler;

        [Header("Visual Settings")]
        [SerializeField] private float _disabledAlpha = 0.5f;

        private CardData _cardData;
        private ICardDatabase _cardDatabase;
        private int _slotIndex;
        private bool _useInputHandler;

        /// <summary>
        /// The card data this view represents.
        /// </summary>
        public CardData CardData => _cardData;

        /// <summary>
        /// The slot index in the room (0-3).
        /// </summary>
        public int SlotIndex => _slotIndex;

        /// <summary>
        /// Event fired when this card is clicked.
        /// </summary>
        public event Action<CardView> OnCardClicked;

        private void Awake()
        {
            // Auto-get components if not assigned
            if (_cardImage == null) _cardImage = GetComponent<Image>();
            if (_button == null) _button = GetComponent<Button>();
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();

            // Setup button click
            _button.onClick.AddListener(HandleClick);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(HandleClick);

            if (_inputHandler != null)
            {
                _inputHandler.OnCardClicked -= HandleInputHandlerClick;
            }
        }

        /// <summary>
        /// Initializes the card view with database reference.
        /// </summary>
        public void Initialize(ICardDatabase cardDatabase)
        {
            _cardDatabase = cardDatabase;
        }

        /// <summary>
        /// Initializes the card view with full dependencies including input handler support.
        /// </summary>
        public void Initialize(
            ICardDatabase cardDatabase,
            ICommandProcessor commandProcessor,
            IGameManager gameManager,
            Action<int> onDamagePreviewStart,
            Action onDamagePreviewEnd,
            Action<bool> onShieldEfficiencyPreview)
        {
            _cardDatabase = cardDatabase;

            // Setup input handler if available
            if (_inputHandler == null)
            {
                _inputHandler = GetComponent<CardInputHandler>();
            }

            if (_inputHandler != null)
            {
                _useInputHandler = true;
                _inputHandler.Initialize(
                    commandProcessor,
                    gameManager,
                    onDamagePreviewStart,
                    onDamagePreviewEnd,
                    onShieldEfficiencyPreview);
                _inputHandler.OnCardClicked += HandleInputHandlerClick;
            }
        }

        private void HandleInputHandlerClick(CardData card)
        {
            OnCardClicked?.Invoke(this);
        }

        /// <summary>
        /// Sets up this card view with card data.
        /// </summary>
        public void SetCard(CardData card, int slotIndex)
        {
            _cardData = card;
            _slotIndex = slotIndex;

            // Update sprite
            if (_cardDatabase != null)
            {
                Sprite sprite = _cardDatabase.GetCardSprite(card);
                if (sprite != null)
                {
                    _cardImage.sprite = sprite;
                }
            }

            // Update input handler
            if (_inputHandler != null)
            {
                _inputHandler.SetCard(card, slotIndex);
            }

            // Ensure visible
            gameObject.SetActive(true);
            SetInteractable(true);
        }

        /// <summary>
        /// Shows the card back (face down).
        /// </summary>
        public void ShowCardBack()
        {
            if (_cardDatabase != null && _cardDatabase.CardBack != null)
            {
                _cardImage.sprite = _cardDatabase.CardBack;
            }
        }

        /// <summary>
        /// Sets whether this card is interactable.
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            // Only set button interactable if not using input handler for clicks
            _button.interactable = _useInputHandler ? false : interactable;

            // Update input handler
            if (_inputHandler != null)
            {
                _inputHandler.SetInteractable(interactable);
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = interactable ? 1f : _disabledAlpha;
            }
        }

        /// <summary>
        /// Hides this card view.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Shows this card view.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }

        private void HandleClick()
        {
            OnCardClicked?.Invoke(this);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_cardImage == null) _cardImage = GetComponent<Image>();
            if (_button == null) _button = GetComponent<Button>();
        }
#endif
    }
}
