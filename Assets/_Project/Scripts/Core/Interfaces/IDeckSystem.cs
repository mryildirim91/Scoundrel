using System.Collections.Generic;
using Scoundrel.Core.Data;

namespace Scoundrel.Core.Interfaces
{
    /// <summary>
    /// Interface for deck management operations.
    /// Handles the 44-card deck: shuffling, drawing, and returning cards.
    /// </summary>
    public interface IDeckSystem
    {
        /// <summary>
        /// Number of cards remaining in the deck.
        /// </summary>
        int RemainingCards { get; }

        /// <summary>
        /// Whether the deck is empty.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Initializes the deck with the standard 44-card composition.
        /// Should be called at game start.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Shuffles the deck using Fisher-Yates algorithm.
        /// </summary>
        void Shuffle();

        /// <summary>
        /// Draws the specified number of cards from the top of the deck.
        /// Returns fewer cards if deck doesn't have enough.
        /// </summary>
        List<CardData> Draw(int count);

        /// <summary>
        /// Returns cards to the bottom of the deck (used by Run action).
        /// </summary>
        void ReturnToBottom(IReadOnlyList<CardData> cards);

        /// <summary>
        /// Resets the deck to its initial state for a new game.
        /// </summary>
        void Reset();
    }
}
