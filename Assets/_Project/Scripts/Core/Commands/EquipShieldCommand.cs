using Cysharp.Threading.Tasks;
using Scoundrel.Core.Data;
using Scoundrel.Core.Interfaces;
using UnityEngine;

namespace Scoundrel.Core.Commands
{
    /// <summary>
    /// Command to equip a shield card (Diamond).
    /// Sets the player's shield value and unlocks hearts.
    /// Note: Shield downgrade confirmation should be handled by UI before creating this command.
    /// </summary>
    public sealed class EquipShieldCommand : ICommand
    {
        private readonly CardData _card;
        private readonly IPlayerState _playerState;
        private readonly IRoomSystem _roomSystem;

        /// <summary>
        /// Creates a new EquipShieldCommand.
        /// </summary>
        /// <param name="card">The shield card (Diamond) to equip.</param>
        /// <param name="playerState">Player state service.</param>
        /// <param name="roomSystem">Room system service.</param>
        public EquipShieldCommand(
            CardData card,
            IPlayerState playerState,
            IRoomSystem roomSystem)
        {
            _card = card;
            _playerState = playerState;
            _roomSystem = roomSystem;
        }

        /// <summary>
        /// Validates that the card is a shield and exists in the room.
        /// </summary>
        public bool CanExecute()
        {
            if (!_card.IsShield)
            {
                Debug.LogWarning($"[EquipShieldCommand] Cannot execute: {_card} is not a shield.");
                return false;
            }

            if (!_roomSystem.ContainsCard(_card))
            {
                Debug.LogWarning($"[EquipShieldCommand] Cannot execute: {_card} is not in the room.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Executes the shield equip: sets shield value, removes card, unlocks hearts.
        /// </summary>
        public UniTask ExecuteAsync()
        {
            int oldShield = _playerState.ShieldValue;
            int newShield = _card.Value;

            Debug.Log($"[EquipShieldCommand] Equipping {_card}: Shield {oldShield} -> {newShield}");

            // Set new shield value
            _playerState.SetShield(newShield);

            // Remove card from room
            _roomSystem.RemoveCard(_card);

            // Equipping a shield unlocks hearts (non-heart interaction)
            _playerState.SetHeartLock(false);

            return UniTask.CompletedTask;
        }

        /// <summary>
        /// Checks if equipping this shield would be a downgrade.
        /// UI should use this to show confirmation dialog.
        /// </summary>
        public bool IsDowngrade()
        {
            return _card.Value < _playerState.ShieldValue;
        }
    }
}
