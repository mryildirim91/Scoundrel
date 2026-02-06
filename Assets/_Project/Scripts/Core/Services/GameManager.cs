using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Scoundrel.Core.Data;
using Scoundrel.Core.Enums;
using Scoundrel.Core.Interfaces;
using UnityEngine;

namespace Scoundrel.Core.Services
{
    /// <summary>
    /// Main game orchestrator implementing a state machine for game flow.
    /// Coordinates all game systems: deck, room, player state, and commands.
    ///
    /// State Flow:
    /// Initialize → Deal → PlayerTurn ←→ Processing → Deal (loop)
    ///                         ↓
    ///                    GameOver (Win/Loss)
    /// </summary>
    public sealed class GameManager : IGameManager
    {
        private readonly IGameSettings _settings;
        private readonly GameEvents _events;
        private readonly IPlayerState _playerState;
        private readonly IDeckSystem _deckSystem;
        private readonly IRoomSystem _roomSystem;
        private readonly ICommandProcessor _commandProcessor;

        private GameState _currentState;
        private GameResult _result;

        public GameState CurrentState => _currentState;
        public GameResult Result => _result;
        public bool IsGameActive => _currentState != GameState.GameOver;
        public bool CanAcceptInput => _currentState == GameState.PlayerTurn;

        /// <summary>
        /// Creates a new GameManager with all required dependencies.
        /// </summary>
        public GameManager(
            IGameSettings settings,
            GameEvents events,
            IPlayerState playerState,
            IDeckSystem deckSystem,
            IRoomSystem roomSystem,
            ICommandProcessor commandProcessor)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _playerState = playerState ?? throw new ArgumentNullException(nameof(playerState));
            _deckSystem = deckSystem ?? throw new ArgumentNullException(nameof(deckSystem));
            _roomSystem = roomSystem ?? throw new ArgumentNullException(nameof(roomSystem));
            _commandProcessor = commandProcessor ?? throw new ArgumentNullException(nameof(commandProcessor));

            _currentState = GameState.Initializing;
            _result = GameResult.None;

            // Subscribe to room cleared event for automatic dealing
            _events.OnRoomCleared += OnRoomCleared;
        }

        /// <summary>
        /// Starts a new game. Initializes all systems and deals the first room.
        /// </summary>
        public async UniTask StartGameAsync()
        {
            Debug.Log("[GameManager] Starting new game...");

            // Transition to Initializing state
            SetState(GameState.Initializing);
            _result = GameResult.None;

            // Reset all systems
            _playerState.Reset();
            _deckSystem.Reset(); // This initializes and shuffles

            Debug.Log($"[GameManager] Game initialized. Deck: {_deckSystem.RemainingCards} cards, HP: {_playerState.CurrentHP}");

            // Deal first room
            await DealRoomAsync();
        }

        /// <summary>
        /// Handles player interaction with a card in the room.
        /// </summary>
        public async UniTask HandleCardInteractionAsync(CardData card)
        {
            if (!CanAcceptInput)
            {
                Debug.LogWarning($"[GameManager] Cannot accept input in state: {_currentState}");
                return;
            }

            Debug.Log($"[GameManager] Player selected: {card}");

            // Transition to Processing state
            SetState(GameState.Processing);

            // Fire card interacted event for UI feedback
            _events.RaiseCardInteracted(card);

            // Execute the appropriate command via CommandProcessor
            bool success = await _commandProcessor.ProcessCardInteractionAsync(card);

            if (!success)
            {
                Debug.LogWarning("[GameManager] Command execution failed, returning to PlayerTurn");
                SetState(GameState.PlayerTurn);
                return;
            }

            // Check for game end conditions after processing
            await CheckGameEndConditionsAsync();
        }

        /// <summary>
        /// Handles the Run action.
        /// </summary>
        public async UniTask HandleRunAsync()
        {
            if (!CanAcceptInput)
            {
                Debug.LogWarning($"[GameManager] Cannot accept input in state: {_currentState}");
                return;
            }

            Debug.Log("[GameManager] Player chose to Run");

            // Transition to Processing state
            SetState(GameState.Processing);

            // Execute run command
            bool success = await _commandProcessor.ProcessRunAsync();

            if (!success)
            {
                Debug.LogWarning("[GameManager] Run command failed, returning to PlayerTurn");
                SetState(GameState.PlayerTurn);
                return;
            }

            // Check for game end conditions (running costs HP)
            await CheckGameEndConditionsAsync();
        }

        /// <summary>
        /// Restarts the game.
        /// </summary>
        public async UniTask RestartGameAsync()
        {
            Debug.Log("[GameManager] Restarting game...");
            await StartGameAsync();
        }

        /// <summary>
        /// Deals cards to fill the room.
        /// </summary>
        private async UniTask DealRoomAsync()
        {
            SetState(GameState.Dealing);

            // Calculate how many cards to deal
            int cardsToDeal = _settings.RoomSize;

            // Draw cards from deck
            List<CardData> cards = _deckSystem.Draw(cardsToDeal);

            if (cards.Count == 0)
            {
                // No cards left to deal - check win condition
                Debug.Log("[GameManager] No cards left to deal");
                await CheckGameEndConditionsAsync();
                return;
            }

            Debug.Log($"[GameManager] Dealing {cards.Count} cards to room");

            // Set cards in room (this fires OnRoomDealt event)
            _roomSystem.SetCards(cards);

            // Small delay for dealing animation (can be expanded later)
            await UniTask.Delay(100);

            // Transition to PlayerTurn
            SetState(GameState.PlayerTurn);

            Debug.Log("[GameManager] Waiting for player input...");
        }

        /// <summary>
        /// Checks win/loss conditions and handles game end.
        /// </summary>
        private async UniTask CheckGameEndConditionsAsync()
        {
            // Check for defeat (HP <= 0)
            if (!_playerState.IsAlive)
            {
                Debug.Log("[GameManager] Player defeated!");
                await EndGameAsync(GameResult.Defeat);
                return;
            }

            // Check for victory (deck empty AND room empty)
            if (_deckSystem.IsEmpty && _roomSystem.IsEmpty)
            {
                Debug.Log("[GameManager] Player victorious!");
                await EndGameAsync(GameResult.Victory);
                return;
            }

            // If room is empty but deck has cards, deal new room
            if (_roomSystem.IsEmpty && !_deckSystem.IsEmpty)
            {
                // Re-enable running for new room (if it was disabled)
                _playerState.SetCanRun(true);

                await DealRoomAsync();
                return;
            }

            // Continue playing - return to PlayerTurn
            SetState(GameState.PlayerTurn);
        }

        /// <summary>
        /// Handles game end state.
        /// </summary>
        private UniTask EndGameAsync(GameResult result)
        {
            _result = result;
            SetState(GameState.GameOver);

            Debug.Log($"[GameManager] Game Over: {result}");

            // Fire game ended event
            _events.RaiseGameEnded(result);

            return UniTask.CompletedTask;
        }

        /// <summary>
        /// Sets the current game state and fires event.
        /// </summary>
        private void SetState(GameState newState)
        {
            if (_currentState == newState) return;

            GameState previousState = _currentState;
            _currentState = newState;

            Debug.Log($"[GameManager] State: {previousState} -> {newState}");

            _events.RaiseGameStateChanged(newState);
        }

        /// <summary>
        /// Called when the room is cleared (all cards removed).
        /// This handles the case where player clears room card by card.
        /// </summary>
        private void OnRoomCleared()
        {
            // Only process if we're in a valid state
            if (_currentState == GameState.Processing)
            {
                // The CheckGameEndConditionsAsync will handle dealing new cards
                // This event is informational - actual logic is in the command flow
                Debug.Log("[GameManager] Room cleared event received");
            }
        }

        /// <summary>
        /// Cleanup when GameManager is destroyed.
        /// </summary>
        public void Dispose()
        {
            _events.OnRoomCleared -= OnRoomCleared;
        }
    }
}
