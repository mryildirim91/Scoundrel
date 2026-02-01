using System.Collections.Generic;
using Scoundrel.Core.Data;

namespace Scoundrel.Core.Interfaces
{
    /// <summary>
    /// Interface for managing the current room (4 cards displayed to player).
    /// </summary>
    public interface IRoomSystem
    {
        /// <summary>
        /// Cards currently in the room.
        /// </summary>
        IReadOnlyList<CardData> CurrentCards { get; }

        /// <summary>
        /// Number of cards in the room.
        /// </summary>
        int CardCount { get; }

        /// <summary>
        /// Whether the room is empty (all cards cleared).
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Sets the cards in the room (called when dealing).
        /// </summary>
        void SetCards(List<CardData> cards);

        /// <summary>
        /// Removes a specific card from the room (after player interacts with it).
        /// </summary>
        void RemoveCard(CardData card);

        /// <summary>
        /// Clears and returns all remaining cards (used by Run action).
        /// </summary>
        List<CardData> ClearRoom();

        /// <summary>
        /// Gets all potion cards currently in the room.
        /// Used for checking overdose lock state.
        /// </summary>
        IReadOnlyList<CardData> GetPotionsInRoom();

        /// <summary>
        /// Checks if a specific card exists in the room.
        /// </summary>
        bool ContainsCard(CardData card);
    }
}
