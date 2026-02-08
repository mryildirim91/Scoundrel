using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Scoundrel.Core;
using Scoundrel.Core.Data;
using Scoundrel.Core.Interfaces;
using Sisus.Init;
using UnityEngine;

namespace Scoundrel.UI.Views
{
    /// <summary>
    /// Manages the visual display of the 4-card room.
    /// Uses a 2x2 grid layout to display cards.
    /// </summary>
    public class RoomView : MonoBehaviour<GameBootstrapper>
    {
        [Header("Card Slots")]
        [SerializeField] private CardView[] _cardSlots = new CardView[4];

        [Header("HUD Reference (for previews)")]
        [SerializeField] private HUDController _hudController;

        private GameBootstrapper _gameBootstrapper;
        private ICardDatabase _cardDatabase;
        private IGameEvents _events;
        private IPlayerState _playerState;
        private IGameManager _gameManager;
        private ICommandProcessor _commandProcessor;

        protected override void Init(GameBootstrapper gameBootstrapper)
        {
            _gameBootstrapper = gameBootstrapper;
            _cardDatabase = gameBootstrapper.CardDatabase;
            _events = gameBootstrapper.Events;
            _playerState = gameBootstrapper.PlayerState;
            _gameManager = gameBootstrapper.GameManager;
            _commandProcessor = gameBootstrapper.CommandProcessor;

            // Initialize card views with full dependencies (including input handler support)
            foreach (var slot in _cardSlots)
            {
                if (slot != null)
                {
                    slot.Initialize(
                        _cardDatabase,
                        _commandProcessor,
                        _gameManager,
                        OnDamagePreviewStart,
                        OnDamagePreviewEnd,
                        OnShieldEfficiencyPreview);
                }
            }

            Debug.Log("[RoomView] Initialized via Init(args)");
        }

        private void OnDamagePreviewStart(int damage)
        {
            if (_hudController != null)
            {
                _hudController.ShowDamagePreview(damage);
            }
        }

        private void OnDamagePreviewEnd()
        {
            if (_hudController != null)
            {
                _hudController.ClearPreviews();
            }
        }

        private void OnShieldEfficiencyPreview(bool show)
        {
            if (_hudController != null)
            {
                _hudController.ShowReducedShieldEfficiency(show);
            }
        }
        
        protected override void OnAwake()
        {
            base.OnAwake();
            
            // Initialize card slots with click handlers
            for (int i = 0; i < _cardSlots.Length; i++)
            {
                if (_cardSlots[i] != null)
                {
                    _cardSlots[i].OnCardClicked += HandleCardClicked;
                }
            }
        }

        private void Start()
        {
            // Subscribe to events
            SubscribeToEvents();

            // Initial state - hide all cards
            HideAllCards();
        }

        private void OnDestroy()
        {
            // Unsubscribe from card clicks
            for (int i = 0; i < _cardSlots.Length; i++)
            {
                if (_cardSlots[i] != null)
                {
                    _cardSlots[i].OnCardClicked -= HandleCardClicked;
                }
            }

            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            if (_events == null) return;

            _events.OnRoomDealt += HandleRoomDealt;
            _events.OnCardRemovedFromRoom += HandleCardRemoved;
            _events.OnRoomCleared += HandleRoomCleared;
            _events.OnGameStateChanged += HandleGameStateChanged;
        }

        private void UnsubscribeFromEvents()
        {
            if (_events == null) return;

            _events.OnRoomDealt -= HandleRoomDealt;
            _events.OnCardRemovedFromRoom -= HandleCardRemoved;
            _events.OnRoomCleared -= HandleRoomCleared;
            _events.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void HandleRoomDealt(IReadOnlyList<CardData> cards)
        {
            Debug.Log($"[RoomView] Room dealt with {cards.Count} cards");

            // Hide all cards first
            HideAllCards();

            // Show dealt cards
            for (int i = 0; i < cards.Count && i < _cardSlots.Length; i++)
            {
                if (_cardSlots[i] != null)
                {
                    _cardSlots[i].SetCard(cards[i], i);
                }
            }
        }

        private void HandleCardRemoved(CardData card)
        {
            Debug.Log($"[RoomView] Card removed: {card}");

            // Find and hide the card view
            foreach (var slot in _cardSlots)
            {
                if (slot != null && slot.gameObject.activeSelf && slot.CardData.Equals(card))
                {
                    slot.Hide();
                    break;
                }
            }
        }

        private void HandleRoomCleared()
        {
            Debug.Log("[RoomView] Room cleared");
            HideAllCards();
        }

        private void HandleGameStateChanged(Core.Enums.GameState newState)
        {
            // Enable/disable card interaction based on game state
            bool canInteract = newState == Core.Enums.GameState.PlayerTurn;

            foreach (var slot in _cardSlots)
            {
                if (slot != null && slot.gameObject.activeSelf)
                {
                    slot.SetInteractable(canInteract);
                }
            }
        }

        private void HandleCardClicked(CardView cardView)
        {
            Debug.Log($"[RoomView] Card clicked: {cardView.CardData}");

            // Forward to game manager
            if (_gameManager != null && _gameManager.CanAcceptInput)
            {
                _gameManager.HandleCardInteractionAsync(cardView.CardData).Forget();
            }
        }

        private void HideAllCards()
        {
            foreach (var slot in _cardSlots)
            {
                if (slot != null)
                {
                    slot.Hide();
                }
            }
        }

        /// <summary>
        /// Gets a card view by index.
        /// </summary>
        public CardView GetCardView(int index)
        {
            if (index >= 0 && index < _cardSlots.Length)
            {
                return _cardSlots[index];
            }
            return null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Ensure array size is 4
            if (_cardSlots == null || _cardSlots.Length != 4)
            {
                System.Array.Resize(ref _cardSlots, 4);
            }
        }
#endif
    }
}
