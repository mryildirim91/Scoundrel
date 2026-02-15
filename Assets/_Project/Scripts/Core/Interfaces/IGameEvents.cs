using System;
using System.Collections.Generic;
using Scoundrel.Core.Data;
using Scoundrel.Core.Enums;

namespace Scoundrel.Core.Interfaces
{
    /// <summary>
    /// Event bus interface for decoupled communication between game systems.
    /// UI components subscribe to these events to update visuals without direct coupling.
    /// </summary>
    public interface IGameEvents
    {
        /// <summary>
        /// Fired when player's HP changes. Parameters: (newHP, delta)
        /// </summary>
        event Action<int, int> OnHPChanged;

        /// <summary>
        /// Fired when player's shield value changes. Parameter: newShieldValue
        /// </summary>
        event Action<int> OnShieldChanged;

        /// <summary>
        /// Fired when run availability changes. Parameter: canRun
        /// </summary>
        event Action<bool> OnRunAvailableChanged;

        /// <summary>
        /// Fired when a card is interacted with. Parameter: card data
        /// </summary>
        event Action<CardData> OnCardInteracted;

        /// <summary>
        /// Fired when game state transitions. Parameter: new state
        /// </summary>
        event Action<GameState> OnGameStateChanged;

        /// <summary>
        /// Fired when the game ends. Parameter: result (Victory/Defeat)
        /// </summary>
        event Action<GameResult> OnGameEnded;

        /// <summary>
        /// Fired when new cards are dealt to the room. Parameter: list of cards
        /// </summary>
        event Action<IReadOnlyList<CardData>> OnRoomDealt;

        /// <summary>
        /// Fired when the room is cleared (all cards removed).
        /// </summary>
        event Action OnRoomCleared;

        /// <summary>
        /// Fired when deck count changes. Parameter: remaining cards
        /// </summary>
        event Action<int> OnDeckCountChanged;

        /// <summary>
        /// Fired when a card is removed from the room. Parameter: card data
        /// </summary>
        event Action<CardData> OnCardRemovedFromRoom;

        /// <summary>
        /// Fired when cards are added to an existing room (Safe Exit).
        /// Unlike OnRoomDealt, this does not replace existing cards.
        /// Parameter: the newly added cards only.
        /// </summary>
        event Action<IReadOnlyList<CardData>> OnCardsAddedToRoom;
    }
}
