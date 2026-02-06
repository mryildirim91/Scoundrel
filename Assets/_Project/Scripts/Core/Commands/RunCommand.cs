using Cysharp.Threading.Tasks;
using Scoundrel.Core.Data;
using Scoundrel.Core.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace Scoundrel.Core.Commands
{
    /// <summary>
    /// Command to run from the current room (The Coward's Toll).
    /// Costs 1 HP, moves remaining cards to deck bottom, and disables running until next room.
    /// </summary>
    public sealed class RunCommand : ICommand
    {
        private readonly IPlayerState _playerState;
        private readonly IRoomSystem _roomSystem;
        private readonly IDeckSystem _deckSystem;

        private const int MinCardsToRun = 3;
        private const int MaxCardsToRun = 4;

        /// <summary>
        /// Creates a new RunCommand.
        /// </summary>
        /// <param name="playerState">Player state service.</param>
        /// <param name="roomSystem">Room system service.</param>
        /// <param name="deckSystem">Deck system service.</param>
        public RunCommand(
            IPlayerState playerState,
            IRoomSystem roomSystem,
            IDeckSystem deckSystem)
        {
            _playerState = playerState;
            _roomSystem = roomSystem;
            _deckSystem = deckSystem;
        }

        /// <summary>
        /// Validates that running is allowed:
        /// - CanRun flag is true (not consecutive run)
        /// - Room has 3-4 cards
        /// - Player has HP > 1 (can survive the cost)
        /// </summary>
        public bool CanExecute()
        {
            if (!_playerState.CanRun)
            {
                Debug.LogWarning("[RunCommand] Cannot execute: Running is disabled (used last turn).");
                return false;
            }

            int cardCount = _roomSystem.CardCount;
            if (cardCount < MinCardsToRun || cardCount > MaxCardsToRun)
            {
                Debug.LogWarning($"[RunCommand] Cannot execute: Room has {cardCount} cards (need {MinCardsToRun}-{MaxCardsToRun}).");
                return false;
            }

            // Player must be able to survive the run cost (optional: allow dying by running)
            // Based on design doc, running costs 1 HP but doesn't mention it can kill you
            // Being conservative here - requiring HP > 1 to run
            if (_playerState.CurrentHP <= 1)
            {
                Debug.LogWarning("[RunCommand] Cannot execute: Not enough HP to run (need > 1 HP).");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Executes the run: pays HP, moves cards to deck bottom, disables future runs.
        /// </summary>
        public UniTask ExecuteAsync()
        {
            Debug.Log($"[RunCommand] Running from room with {_roomSystem.CardCount} cards.");

            // Pay the run cost (-1 HP)
            _playerState.PayRunCost();

            // Move all remaining cards from room to bottom of deck
            List<CardData> roomCards = _roomSystem.ClearRoom();
            _deckSystem.ReturnToBottom(roomCards);

            Debug.Log($"[RunCommand] Moved {roomCards.Count} cards to deck bottom. Deck now has {_deckSystem.RemainingCards} cards.");

            // Disable running until next room is cleared/interacted
            _playerState.SetCanRun(false);

            return UniTask.CompletedTask;
        }
    }
}
