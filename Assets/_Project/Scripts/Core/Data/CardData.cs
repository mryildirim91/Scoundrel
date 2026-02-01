using System;
using Scoundrel.Core.Enums;

namespace Scoundrel.Core.Data
{
    /// <summary>
    /// Immutable data structure representing a playing card.
    /// Value and Type are computed from Suit and Rank.
    /// </summary>
    [Serializable]
    public readonly struct CardData : IEquatable<CardData>
    {
        public CardSuit Suit { get; }
        public CardRank Rank { get; }

        /// <summary>
        /// Numeric value of the card (2-14).
        /// </summary>
        public int Value => (int)Rank;

        /// <summary>
        /// Gameplay type derived from suit.
        /// </summary>
        public CardType Type => Suit switch
        {
            CardSuit.Spades => CardType.Monster,
            CardSuit.Clubs => CardType.Monster,
            CardSuit.Diamonds => CardType.Shield,
            CardSuit.Hearts => CardType.Potion,
            _ => throw new ArgumentOutOfRangeException(nameof(Suit), Suit, "Unknown card suit")
        };

        /// <summary>
        /// Whether this card is a monster (Spades or Clubs).
        /// </summary>
        public bool IsMonster => Type == CardType.Monster;

        /// <summary>
        /// Whether this card is a Spade (100% shield efficiency).
        /// </summary>
        public bool IsSpade => Suit == CardSuit.Spades;

        /// <summary>
        /// Whether this card is a Club (50% shield efficiency).
        /// </summary>
        public bool IsClub => Suit == CardSuit.Clubs;

        /// <summary>
        /// Whether this card is a Shield (Diamond).
        /// </summary>
        public bool IsShield => Type == CardType.Shield;

        /// <summary>
        /// Whether this card is a Potion (Heart).
        /// </summary>
        public bool IsPotion => Type == CardType.Potion;

        public CardData(CardSuit suit, CardRank rank)
        {
            Suit = suit;
            Rank = rank;
        }

        public bool Equals(CardData other)
        {
            return Suit == other.Suit && Rank == other.Rank;
        }

        public override bool Equals(object obj)
        {
            return obj is CardData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Suit, Rank);
        }

        public static bool operator ==(CardData left, CardData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CardData left, CardData right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"{Rank} of {Suit}";
        }
    }
}
