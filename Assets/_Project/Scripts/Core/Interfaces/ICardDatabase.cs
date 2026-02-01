using Scoundrel.Core.Data;
using Scoundrel.Core.Enums;
using UnityEngine;

namespace Scoundrel.Core.Interfaces
{
    /// <summary>
    /// Interface for accessing card visual assets.
    /// Implemented by CardDatabase ScriptableObject.
    /// </summary>
    public interface ICardDatabase
    {
        /// <summary>
        /// Sprite for face-down cards.
        /// </summary>
        Sprite CardBack { get; }

        /// <summary>
        /// Overlay sprite for locked cards (overdose mechanic).
        /// </summary>
        Sprite LockOverlay { get; }

        /// <summary>
        /// Gets the sprite for a specific card.
        /// </summary>
        Sprite GetCardSprite(CardData card);

        /// <summary>
        /// Gets the sprite for a card by suit and rank.
        /// </summary>
        Sprite GetCardSprite(CardSuit suit, CardRank rank);
    }
}
