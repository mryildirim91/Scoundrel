using System;
using Cysharp.Threading.Tasks;
using Scoundrel.Core.Combat;
using Scoundrel.Core.Commands;
using Scoundrel.Core.Data;
using Scoundrel.Core.Interfaces;
using UnityEngine;

namespace Scoundrel.Core.Services
{
    /// <summary>
    /// Service responsible for validating and executing game commands.
    /// Acts as the central point for all player actions.
    /// </summary>
    public sealed class CommandProcessor : ICommandProcessor
    {
        private readonly IPlayerState _playerState;
        private readonly IRoomSystem _roomSystem;
        private readonly IDeckSystem _deckSystem;
        private readonly IDamageCalculator _spadesDamageCalculator;
        private readonly IDamageCalculator _clubsDamageCalculator;

        private bool _isProcessing;

        /// <summary>
        /// Creates a new CommandProcessor with injected dependencies.
        /// </summary>
        public CommandProcessor(
            IPlayerState playerState,
            IRoomSystem roomSystem,
            IDeckSystem deckSystem)
        {
            _playerState = playerState ?? throw new ArgumentNullException(nameof(playerState));
            _roomSystem = roomSystem ?? throw new ArgumentNullException(nameof(roomSystem));
            _deckSystem = deckSystem ?? throw new ArgumentNullException(nameof(deckSystem));

            // Create damage calculators
            _spadesDamageCalculator = new SpadesDamageCalculator();
            _clubsDamageCalculator = new ClubsDamageCalculator();
        }

        /// <summary>
        /// Whether a command is currently being processed.
        /// Used to prevent concurrent command execution.
        /// </summary>
        public bool IsProcessing => _isProcessing;

        /// <summary>
        /// Creates and executes a command for the given card.
        /// Automatically determines the appropriate command based on card type.
        /// </summary>
        /// <param name="card">The card the player interacted with.</param>
        /// <returns>True if command executed successfully, false otherwise.</returns>
        public async UniTask<bool> ProcessCardInteractionAsync(CardData card)
        {
            if (_isProcessing)
            {
                Debug.LogWarning("[CommandProcessor] Already processing a command.");
                return false;
            }

            ICommand command = CreateCommandForCard(card);
            if (command == null)
            {
                Debug.LogError($"[CommandProcessor] Could not create command for card: {card}");
                return false;
            }

            return await ExecuteCommandAsync(command);
        }

        /// <summary>
        /// Creates and executes a run command.
        /// </summary>
        /// <returns>True if command executed successfully, false otherwise.</returns>
        public async UniTask<bool> ProcessRunAsync()
        {
            if (_isProcessing)
            {
                Debug.LogWarning("[CommandProcessor] Already processing a command.");
                return false;
            }

            var command = new RunCommand(_playerState, _roomSystem, _deckSystem);
            return await ExecuteCommandAsync(command);
        }

        /// <summary>
        /// Executes a command if valid.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>True if executed successfully, false otherwise.</returns>
        public async UniTask<bool> ExecuteCommandAsync(ICommand command)
        {
            if (_isProcessing)
            {
                Debug.LogWarning("[CommandProcessor] Already processing a command.");
                return false;
            }

            if (!command.CanExecute())
            {
                Debug.LogWarning("[CommandProcessor] Command validation failed.");
                return false;
            }

            _isProcessing = true;

            try
            {
                await command.ExecuteAsync();
                Debug.Log("[CommandProcessor] Command executed successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CommandProcessor] Command execution failed: {ex.Message}");
                return false;
            }
            finally
            {
                _isProcessing = false;
            }
        }

        /// <summary>
        /// Creates the appropriate command for a card based on its type.
        /// </summary>
        private ICommand CreateCommandForCard(CardData card)
        {
            return card.Type switch
            {
                Enums.CardType.Monster => new FightMonsterCommand(
                    card,
                    _playerState,
                    _roomSystem,
                    _spadesDamageCalculator,
                    _clubsDamageCalculator),

                Enums.CardType.Shield => new EquipShieldCommand(
                    card,
                    _playerState,
                    _roomSystem),

                Enums.CardType.Potion => new DrinkPotionCommand(
                    card,
                    _playerState,
                    _roomSystem),

                _ => null
            };
        }

        /// <summary>
        /// Creates a FightMonsterCommand for preview purposes (e.g., damage preview on hold).
        /// </summary>
        public FightMonsterCommand CreateFightCommand(CardData card)
        {
            if (!card.IsMonster) return null;

            return new FightMonsterCommand(
                card,
                _playerState,
                _roomSystem,
                _spadesDamageCalculator,
                _clubsDamageCalculator);
        }

        /// <summary>
        /// Creates an EquipShieldCommand for preview purposes (e.g., downgrade check).
        /// </summary>
        public EquipShieldCommand CreateEquipShieldCommand(CardData card)
        {
            if (!card.IsShield) return null;

            return new EquipShieldCommand(
                card,
                _playerState,
                _roomSystem);
        }

        /// <summary>
        /// Calculates the damage that would be dealt by a monster card.
        /// Used for damage preview UI.
        /// </summary>
        public int CalculateDamagePreview(CardData card)
        {
            if (!card.IsMonster) return 0;

            IDamageCalculator calculator = card.IsSpade
                ? _spadesDamageCalculator
                : _clubsDamageCalculator;

            return calculator.Calculate(card.Value, _playerState.ShieldValue);
        }
    }
}
