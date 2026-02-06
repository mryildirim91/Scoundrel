using System;
using System.Collections.Generic;
using Scoundrel.Core.Data;
using Scoundrel.Core.Enums;
using Scoundrel.Core.Interfaces;

namespace Scoundrel.Core.Services
{
    /// <summary>
    /// Central event bus for decoupled communication between game systems.
    /// UI components subscribe to these events to update visuals without tight coupling to game logic.
    /// Implements the Observer pattern.
    /// </summary>
    public sealed class GameEvents : IGameEvents
    {
        public event Action<int, int> OnHPChanged;
        public event Action<int> OnShieldChanged;
        public event Action<bool> OnHeartLockChanged;
        public event Action<bool> OnRunAvailableChanged;
        public event Action<CardData> OnCardInteracted;
        public event Action<GameState> OnGameStateChanged;
        public event Action<GameResult> OnGameEnded;
        public event Action<IReadOnlyList<CardData>> OnRoomDealt;
        public event Action OnRoomCleared;
        public event Action<int> OnDeckCountChanged;
        public event Action<CardData> OnCardRemovedFromRoom;

        /// <summary>
        /// Raises the HP changed event.
        /// </summary>
        /// <param name="newHP">The new HP value.</param>
        /// <param name="delta">The change amount (positive for heal, negative for damage).</param>
        public void RaiseHPChanged(int newHP, int delta)
        {
            OnHPChanged?.Invoke(newHP, delta);
        }

        /// <summary>
        /// Raises the shield changed event.
        /// </summary>
        /// <param name="newShield">The new shield value.</param>
        public void RaiseShieldChanged(int newShield)
        {
            OnShieldChanged?.Invoke(newShield);
        }

        /// <summary>
        /// Raises the heart lock changed event (overdose mechanic).
        /// </summary>
        /// <param name="isLocked">Whether hearts are now locked.</param>
        public void RaiseHeartLockChanged(bool isLocked)
        {
            OnHeartLockChanged?.Invoke(isLocked);
        }

        /// <summary>
        /// Raises the run available changed event.
        /// </summary>
        /// <param name="canRun">Whether the player can now run.</param>
        public void RaiseRunAvailableChanged(bool canRun)
        {
            OnRunAvailableChanged?.Invoke(canRun);
        }

        /// <summary>
        /// Raises the card interacted event.
        /// </summary>
        /// <param name="card">The card that was interacted with.</param>
        public void RaiseCardInteracted(CardData card)
        {
            OnCardInteracted?.Invoke(card);
        }

        /// <summary>
        /// Raises the game state changed event.
        /// </summary>
        /// <param name="newState">The new game state.</param>
        public void RaiseGameStateChanged(GameState newState)
        {
            OnGameStateChanged?.Invoke(newState);
        }

        /// <summary>
        /// Raises the game ended event.
        /// </summary>
        /// <param name="result">The game result (Victory or Defeat).</param>
        public void RaiseGameEnded(GameResult result)
        {
            OnGameEnded?.Invoke(result);
        }

        /// <summary>
        /// Raises the room dealt event.
        /// </summary>
        /// <param name="cards">The cards dealt to the room.</param>
        public void RaiseRoomDealt(IReadOnlyList<CardData> cards)
        {
            OnRoomDealt?.Invoke(cards);
        }

        /// <summary>
        /// Raises the room cleared event.
        /// </summary>
        public void RaiseRoomCleared()
        {
            OnRoomCleared?.Invoke();
        }

        /// <summary>
        /// Raises the deck count changed event.
        /// </summary>
        /// <param name="remainingCards">The number of cards remaining in the deck.</param>
        public void RaiseDeckCountChanged(int remainingCards)
        {
            OnDeckCountChanged?.Invoke(remainingCards);
        }

        /// <summary>
        /// Raises the card removed from room event.
        /// </summary>
        /// <param name="card">The card that was removed.</param>
        public void RaiseCardRemovedFromRoom(CardData card)
        {
            OnCardRemovedFromRoom?.Invoke(card);
        }

        /// <summary>
        /// Clears all event subscriptions. Useful for cleanup.
        /// </summary>
        public void ClearAllSubscriptions()
        {
            OnHPChanged = null;
            OnShieldChanged = null;
            OnHeartLockChanged = null;
            OnRunAvailableChanged = null;
            OnCardInteracted = null;
            OnGameStateChanged = null;
            OnGameEnded = null;
            OnRoomDealt = null;
            OnRoomCleared = null;
            OnDeckCountChanged = null;
            OnCardRemovedFromRoom = null;
        }
    }
}
