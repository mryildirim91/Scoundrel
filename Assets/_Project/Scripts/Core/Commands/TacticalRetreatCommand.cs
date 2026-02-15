using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Scoundrel.Core.Data;
using Scoundrel.Core.Interfaces;
using UnityEngine;

namespace Scoundrel.Core.Commands
{
    /// <summary>
    /// Command for Tactical Retreat (The Coward's Toll).
    /// Available when exactly 4 cards are present in the room.
    /// Costs 1 HP, moves all 4 cards to the bottom of the deck,
    /// and disables consecutive retreats (cooldown).
    /// </summary>
    public sealed class TacticalRetreatCommand : ICommand
    {
        private readonly IPlayerState _playerState;
        private readonly IRoomSystem _roomSystem;
        private readonly IDeckSystem _deckSystem;

        private const int RequiredCardCount = 4;

        /// <summary>
        /// Creates a new TacticalRetreatCommand.
        /// </summary>
        /// <param name="playerState">Player state service.</param>
        /// <param name="roomSystem">Room system service.</param>
        /// <param name="deckSystem">Deck system service.</param>
        public TacticalRetreatCommand(
            IPlayerState playerState,
            IRoomSystem roomSystem,
            IDeckSystem deckSystem)
        {
            _playerState = playerState;
            _roomSystem = roomSystem;
            _deckSystem = deckSystem;
        }

        /// <summary>
        /// Validates that Tactical Retreat is allowed:
        /// - Room has exactly 4 cards
        /// - CanRun flag is true (not on cooldown from previous retreat)
        /// - Player has HP > 1 (can survive the cost)
        /// </summary>
        public bool CanExecute()
        {
            if (_roomSystem.CardCount != RequiredCardCount)
            {
                Debug.LogWarning($"[TacticalRetreatCommand] Cannot execute: Room has {_roomSystem.CardCount} cards (need exactly {RequiredCardCount}).");
                return false;
            }

            if (!_playerState.CanRun)
            {
                Debug.LogWarning("[TacticalRetreatCommand] Cannot execute: Tactical Retreat on cooldown (used last turn).");
                return false;
            }

            if (_playerState.CurrentHP <= 1)
            {
                Debug.LogWarning("[TacticalRetreatCommand] Cannot execute: Not enough HP to retreat (need > 1 HP).");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Executes the Tactical Retreat:
        /// 1. Pay HP cost (-1 HP)
        /// 2. Move all 4 cards from room to bottom of deck
        /// 3. Disable running (cooldown until player engages with next room)
        /// </summary>
        public UniTask ExecuteAsync()
        {
            Debug.Log($"[TacticalRetreatCommand] Retreating from room with {_roomSystem.CardCount} cards.");

            // Pay the run cost (-1 HP)
            _playerState.PayRunCost();

            // Move all remaining cards from room to bottom of deck
            List<CardData> roomCards = _roomSystem.ClearRoom();
            _deckSystem.ReturnToBottom(roomCards);

            Debug.Log($"[TacticalRetreatCommand] Moved {roomCards.Count} cards to deck bottom. Deck now has {_deckSystem.RemainingCards} cards.");

            // Disable Tactical Retreat until player engages with next room
            _playerState.SetCanRun(false);

            return UniTask.CompletedTask;
        }
    }
}
