using Cysharp.Threading.Tasks;
using Scoundrel.Core.Data;
using Scoundrel.Core.Interfaces;
using UnityEngine;

namespace Scoundrel.Core.Commands
{
    /// <summary>
    /// Command to drink a potion card (Heart).
    /// Heals the player (capped at MaxHP).
    /// </summary>
    public sealed class DrinkPotionCommand : ICommand
    {
        private readonly CardData _card;
        private readonly IPlayerState _playerState;
        private readonly IRoomSystem _roomSystem;

        /// <summary>
        /// Creates a new DrinkPotionCommand.
        /// </summary>
        /// <param name="card">The potion card (Heart) to drink.</param>
        /// <param name="playerState">Player state service.</param>
        /// <param name="roomSystem">Room system service.</param>
        public DrinkPotionCommand(
            CardData card,
            IPlayerState playerState,
            IRoomSystem roomSystem)
        {
            _card = card;
            _playerState = playerState;
            _roomSystem = roomSystem;
        }

        /// <summary>
        /// Validates that the card is a potion and exists in the room.
        /// </summary>
        public bool CanExecute()
        {
            if (!_card.IsPotion)
            {
                Debug.LogWarning($"[DrinkPotionCommand] Cannot execute: {_card} is not a potion.");
                return false;
            }

            if (!_roomSystem.ContainsCard(_card))
            {
                Debug.LogWarning($"[DrinkPotionCommand] Cannot execute: {_card} is not in the room.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Executes the potion: heals player, removes card.
        /// </summary>
        public UniTask ExecuteAsync()
        {
            int healAmount = _card.Value;
            int oldHP = _playerState.CurrentHP;

            Debug.Log($"[DrinkPotionCommand] Drinking {_card}: Healing for {healAmount}");

            // Heal the player (clamped to MaxHP by PlayerState)
            _playerState.Heal(healAmount);

            int newHP = _playerState.CurrentHP;
            Debug.Log($"[DrinkPotionCommand] HP: {oldHP} -> {newHP}");

            // Remove card from room
            _roomSystem.RemoveCard(_card);

            return UniTask.CompletedTask;
        }
    }
}
