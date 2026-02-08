using System;
using Scoundrel.Core.Interfaces;
using UnityEngine;

namespace Scoundrel.Core.Services
{
    /// <summary>
    /// Manages player state including HP, Shield, and RunAvailable.
    /// Fires events via GameEvents when state changes.
    /// </summary>
    public sealed class PlayerState : IPlayerState
    {
        private readonly IGameSettings _settings;
        private readonly GameEvents _events;

        private int _currentHP;
        private int _shieldValue;
        private bool _canRun;

        public int CurrentHP => _currentHP;
        public int MaxHP => _settings.MaxHP;
        public int ShieldValue => _shieldValue;
        public bool CanRun => _canRun;
        public bool IsAlive => _currentHP > 0;

        public PlayerState(IGameSettings settings, GameEvents events)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _events = events ?? throw new ArgumentNullException(nameof(events));

            Reset();
        }

        /// <summary>
        /// Reduces HP by the specified amount. Clamps to 0.
        /// </summary>
        public void TakeDamage(int amount)
        {
            if (amount <= 0) return;

            int previousHP = _currentHP;
            _currentHP = Mathf.Max(0, _currentHP - amount);

            int delta = _currentHP - previousHP; // Will be negative
            _events.RaiseHPChanged(_currentHP, delta);

            Debug.Log($"[PlayerState] Took {amount} damage. HP: {previousHP} -> {_currentHP}");
        }

        /// <summary>
        /// Increases HP by the specified amount. Clamps to MaxHP.
        /// </summary>
        public void Heal(int amount)
        {
            if (amount <= 0) return;

            int previousHP = _currentHP;
            _currentHP = Mathf.Min(_settings.MaxHP, _currentHP + amount);

            int delta = _currentHP - previousHP; // Will be positive or zero if at max
            _events.RaiseHPChanged(_currentHP, delta);

            Debug.Log($"[PlayerState] Healed {amount}. HP: {previousHP} -> {_currentHP}");
        }

        /// <summary>
        /// Sets the shield value (from Diamond cards).
        /// </summary>
        public void SetShield(int value)
        {
            if (value < 0)
            {
                Debug.LogWarning($"[PlayerState] Attempted to set negative shield: {value}");
                value = 0;
            }

            int previousShield = _shieldValue;
            _shieldValue = value;

            _events.RaiseShieldChanged(_shieldValue);

            Debug.Log($"[PlayerState] Shield: {previousShield} -> {_shieldValue}");
        }

        /// <summary>
        /// Sets whether the player can run.
        /// </summary>
        public void SetCanRun(bool canRun)
        {
            if (_canRun == canRun) return;

            _canRun = canRun;
            _events.RaiseRunAvailableChanged(_canRun);

            Debug.Log($"[PlayerState] CanRun: {_canRun}");
        }

        /// <summary>
        /// Pays the run cost (deducts HP for running).
        /// </summary>
        public void PayRunCost()
        {
            TakeDamage(_settings.RunCost);
        }

        /// <summary>
        /// Resets player state to initial values for a new game.
        /// </summary>
        public void Reset()
        {
            _currentHP = _settings.StartingHP;
            _shieldValue = _settings.StartingShield;
            _canRun = true;

            // Fire initial state events
            _events.RaiseHPChanged(_currentHP, 0);
            _events.RaiseShieldChanged(_shieldValue);
            _events.RaiseRunAvailableChanged(_canRun);

            Debug.Log($"[PlayerState] Reset - HP: {_currentHP}, Shield: {_shieldValue}");
        }
    }
}
