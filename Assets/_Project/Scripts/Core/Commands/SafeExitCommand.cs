using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Scoundrel.Core.Data;
using Scoundrel.Core.Interfaces;
using UnityEngine;

namespace Scoundrel.Core.Commands
{
    /// <summary>
    /// Command for Safe Exit (The Scout's Departure).
    /// Available when exactly 1 card remains in the room.
    /// Free (no HP cost). The remaining card stays on the table.
    /// Deals new cards from the deck to fill the room back to 4 cards.
    /// Clears Tactical Retreat cooldown (counts as engaging with the room).
    /// </summary>
    public sealed class SafeExitCommand : ICommand
    {
        private readonly IRoomSystem _roomSystem;
        private readonly IDeckSystem _deckSystem;
        private readonly IPlayerState _playerState;
        private readonly IGameSettings _settings;

        private const int RequiredCardCount = 1;

        /// <summary>
        /// Creates a new SafeExitCommand.
        /// </summary>
        /// <param name="roomSystem">Room system service.</param>
        /// <param name="deckSystem">Deck system service.</param>
        /// <param name="playerState">Player state service.</param>
        /// <param name="settings">Game settings for SafeExitFillCount.</param>
        public SafeExitCommand(
            IRoomSystem roomSystem,
            IDeckSystem deckSystem,
            IPlayerState playerState,
            IGameSettings settings)
        {
            _roomSystem = roomSystem;
            _deckSystem = deckSystem;
            _playerState = playerState;
            _settings = settings;
        }

        /// <summary>
        /// Validates that Safe Exit is allowed:
        /// - Room has exactly 1 card
        /// - Deck has cards to deal
        /// </summary>
        public bool CanExecute()
        {
            if (_roomSystem.CardCount != RequiredCardCount)
            {
                Debug.LogWarning($"[SafeExitCommand] Cannot execute: Room has {_roomSystem.CardCount} cards (need exactly {RequiredCardCount}).");
                return false;
            }

            if (_deckSystem.IsEmpty)
            {
                Debug.LogWarning("[SafeExitCommand] Cannot execute: Deck is empty, cannot deal new cards.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Executes the Safe Exit:
        /// 1. The remaining card stays on the table (no removal)
        /// 2. Draw new cards from deck to fill the room
        /// 3. Re-enable Tactical Retreat (Safe Exit clears the cooldown)
        /// </summary>
        public UniTask ExecuteAsync()
        {
            Debug.Log($"[SafeExitCommand] Safe Exit with {_roomSystem.CardCount} card remaining.");

            // The 1 remaining card STAYS in the room (no clear, no return to deck)

            // Deal new cards to fill the room
            int cardsToDeal = _settings.SafeExitFillCount;
            List<CardData> newCards = _deckSystem.Draw(cardsToDeal);

            Debug.Log($"[SafeExitCommand] Drew {newCards.Count} new cards. Deck now has {_deckSystem.RemainingCards} cards.");

            // Add new cards to room alongside the existing card
            _roomSystem.AddCards(newCards);

            // Safe Exit clears Tactical Retreat cooldown
            // (counts as engaging with the room per GDD rules)
            _playerState.SetCanRun(true);

            return UniTask.CompletedTask;
        }
    }
}
