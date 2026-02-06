using Cysharp.Threading.Tasks;
using Scoundrel.Core.Combat;
using Scoundrel.Core.Data;
using Scoundrel.Core.Interfaces;
using UnityEngine;

namespace Scoundrel.Core.Commands
{
    /// <summary>
    /// Command to fight a monster card (Spades or Clubs).
    /// Calculates damage based on suit affinity and applies it to player.
    /// Also unlocks hearts (clears overdose state) after fighting.
    /// </summary>
    public sealed class FightMonsterCommand : ICommand
    {
        private readonly CardData _card;
        private readonly IPlayerState _playerState;
        private readonly IRoomSystem _roomSystem;
        private readonly IDamageCalculator _spadesDamageCalculator;
        private readonly IDamageCalculator _clubsDamageCalculator;

        /// <summary>
        /// Creates a new FightMonsterCommand.
        /// </summary>
        /// <param name="card">The monster card to fight.</param>
        /// <param name="playerState">Player state service.</param>
        /// <param name="roomSystem">Room system service.</param>
        /// <param name="spadesDamageCalculator">Calculator for Spade damage (100% block).</param>
        /// <param name="clubsDamageCalculator">Calculator for Club damage (50% block).</param>
        public FightMonsterCommand(
            CardData card,
            IPlayerState playerState,
            IRoomSystem roomSystem,
            IDamageCalculator spadesDamageCalculator,
            IDamageCalculator clubsDamageCalculator)
        {
            _card = card;
            _playerState = playerState;
            _roomSystem = roomSystem;
            _spadesDamageCalculator = spadesDamageCalculator;
            _clubsDamageCalculator = clubsDamageCalculator;
        }

        /// <summary>
        /// Validates that the card is a monster and exists in the room.
        /// </summary>
        public bool CanExecute()
        {
            if (!_card.IsMonster)
            {
                Debug.LogWarning($"[FightMonsterCommand] Cannot execute: {_card} is not a monster.");
                return false;
            }

            if (!_roomSystem.ContainsCard(_card))
            {
                Debug.LogWarning($"[FightMonsterCommand] Cannot execute: {_card} is not in the room.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Executes the fight: calculates and applies damage, removes card, unlocks hearts.
        /// </summary>
        public UniTask ExecuteAsync()
        {
            // Select appropriate damage calculator based on suit
            IDamageCalculator calculator = _card.IsSpade
                ? _spadesDamageCalculator
                : _clubsDamageCalculator;

            // Calculate damage
            int damage = calculator.Calculate(_card.Value, _playerState.ShieldValue);

            Debug.Log($"[FightMonsterCommand] Fighting {_card}: {damage} damage to player.");

            // Apply damage to player
            if (damage > 0)
            {
                _playerState.TakeDamage(damage);
            }

            // Remove card from room
            _roomSystem.RemoveCard(_card);

            // Fighting a monster unlocks hearts (non-heart interaction)
            _playerState.SetHeartLock(false);

            return UniTask.CompletedTask;
        }
    }
}
