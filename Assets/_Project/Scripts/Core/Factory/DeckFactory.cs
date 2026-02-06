using System.Collections.Generic;
using Scoundrel.Core.Data;
using Scoundrel.Core.Enums;

namespace Scoundrel.Core.Factory
{
    /// <summary>
    /// Factory for creating the 44-card Scoundrel deck.
    ///
    /// Deck composition:
    /// - Spades: All 13 cards (2-10, J, Q, K, A) - Monsters with 100% shield efficiency
    /// - Clubs: All 13 cards (2-10, J, Q, K, A) - Monsters with 50% shield efficiency
    /// - Diamonds: 9 cards (2-10 only, no face cards) - Shields
    /// - Hearts: 9 cards (2-10 only, no face cards) - Potions
    ///
    /// Total: 26 monsters + 9 shields + 9 potions = 44 cards
    /// </summary>
    public static class DeckFactory
    {
        /// <summary>
        /// Creates the standard 44-card Scoundrel deck.
        /// </summary>
        /// <returns>A list containing all 44 cards.</returns>
        public static List<CardData> CreateDeck()
        {
            var deck = new List<CardData>(44);

            // Add all Spades (monsters - 100% block efficiency)
            // All 13 cards: 2-10, J, Q, K, A
            AddAllRanks(deck, CardSuit.Spades);

            // Add all Clubs (monsters - 50% block efficiency)
            // All 13 cards: 2-10, J, Q, K, A
            AddAllRanks(deck, CardSuit.Clubs);

            // Add Diamonds (shields) - only 2-10, no face cards or ace
            AddNumberCardsOnly(deck, CardSuit.Diamonds);

            // Add Hearts (potions) - only 2-10, no face cards or ace
            AddNumberCardsOnly(deck, CardSuit.Hearts);

            return deck;
        }

        /// <summary>
        /// Adds all 13 ranks of a suit to the deck.
        /// Used for Spades and Clubs (monsters).
        /// </summary>
        private static void AddAllRanks(List<CardData> deck, CardSuit suit)
        {
            // Number cards: 2-10
            for (int value = 2; value <= 10; value++)
            {
                deck.Add(new CardData(suit, (CardRank)value));
            }

            // Face cards and Ace
            deck.Add(new CardData(suit, CardRank.Jack));
            deck.Add(new CardData(suit, CardRank.Queen));
            deck.Add(new CardData(suit, CardRank.King));
            deck.Add(new CardData(suit, CardRank.Ace));
        }

        /// <summary>
        /// Adds only number cards (2-10) of a suit to the deck.
        /// Used for Diamonds (shields) and Hearts (potions).
        /// Face cards and Aces are excluded for balance.
        /// </summary>
        private static void AddNumberCardsOnly(List<CardData> deck, CardSuit suit)
        {
            for (int value = 2; value <= 10; value++)
            {
                deck.Add(new CardData(suit, (CardRank)value));
            }
        }

        /// <summary>
        /// Gets the expected card count for the Scoundrel deck.
        /// </summary>
        public const int ExpectedDeckSize = 44;

        /// <summary>
        /// Gets the number of monster cards in the deck.
        /// </summary>
        public const int MonsterCount = 26; // 13 Spades + 13 Clubs

        /// <summary>
        /// Gets the number of shield cards in the deck.
        /// </summary>
        public const int ShieldCount = 9; // Diamonds 2-10

        /// <summary>
        /// Gets the number of potion cards in the deck.
        /// </summary>
        public const int PotionCount = 9; // Hearts 2-10
    }
}
