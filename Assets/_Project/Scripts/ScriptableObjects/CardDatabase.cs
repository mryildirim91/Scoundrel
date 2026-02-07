using System;
using Scoundrel.Core.Data;
using Scoundrel.Core.Enums;
using Scoundrel.Core.Interfaces;
using UnityEngine;
using ZLinq;

namespace Scoundrel.ScriptableObjects
{
    /// <summary>
    /// Database mapping card data to sprites.
    /// Registered as a service via Init(args) for dependency injection.
    /// Create instance via: Assets > Create > Scoundrel > Card Database
    /// </summary>
    [CreateAssetMenu(fileName = "CardDatabase", menuName = "Scoundrel/Card Database")]
    public sealed class CardDatabase : ScriptableObject, ICardDatabase
    {
        [Header("Card Sprites")]
        [SerializeField, Tooltip("Array of all card sprite mappings")]
        private CardSpriteEntry[] cardSprites;

        [Header("Card Back")]
        [SerializeField, Tooltip("Sprite for face-down cards")]
        private Sprite cardBack;

        [Header("Lock Overlay")]
        [SerializeField, Tooltip("Overlay sprite for locked cards (overdose)")]
        private Sprite lockOverlay;

        public Sprite CardBack => cardBack;
        public Sprite LockOverlay => lockOverlay;

        /// <summary>
        /// Gets the sprite for a specific card.
        /// </summary>
        public Sprite GetCardSprite(CardData card)
        {
            var entry = cardSprites.AsValueEnumerable()
                .Where(e => e.Suit == card.Suit && e.Rank == card.Rank)
                .FirstOrDefault();

            if (entry == null || entry.Sprite == null)
            {
                Debug.LogWarning($"No sprite found for {card}");
                return null;
            }

            return entry.Sprite;
        }

        /// <summary>
        /// Gets the sprite for a card by suit and rank.
        /// </summary>
        public Sprite GetCardSprite(CardSuit suit, CardRank rank)
        {
            return GetCardSprite(new CardData(suit, rank));
        }

#if UNITY_EDITOR
        /// <summary>
        /// Called when the ScriptableObject is first created.
        /// Auto-populates the card entries array.
        /// </summary>
        private void Reset()
        {
            InitializeCardEntries();
        }

        /// <summary>
        /// Context menu to regenerate all 44 card entries.
        /// Preserves existing sprite assignments where possible.
        /// </summary>
        [ContextMenu("Initialize Card Entries (44 cards)")]
        private void InitializeCardEntries()
        {
            // Store existing sprite mappings to preserve them
            var existingSprites = new System.Collections.Generic.Dictionary<(CardSuit, CardRank), Sprite>();
            if (cardSprites != null)
            {
                foreach (var entry in cardSprites)
                {
                    if (entry != null && entry.Sprite != null)
                    {
                        existingSprites[(entry.Suit, entry.Rank)] = entry.Sprite;
                    }
                }
            }

            var entries = new System.Collections.Generic.List<CardSpriteEntry>();

            // SPADES: 2-10, J, Q, K, A (13 cards - Monsters)
            entries.Add(CreateEntry(CardSuit.Spades, CardRank.Two, existingSprites));
            entries.Add(CreateEntry(CardSuit.Spades, CardRank.Three, existingSprites));
            entries.Add(CreateEntry(CardSuit.Spades, CardRank.Four, existingSprites));
            entries.Add(CreateEntry(CardSuit.Spades, CardRank.Five, existingSprites));
            entries.Add(CreateEntry(CardSuit.Spades, CardRank.Six, existingSprites));
            entries.Add(CreateEntry(CardSuit.Spades, CardRank.Seven, existingSprites));
            entries.Add(CreateEntry(CardSuit.Spades, CardRank.Eight, existingSprites));
            entries.Add(CreateEntry(CardSuit.Spades, CardRank.Nine, existingSprites));
            entries.Add(CreateEntry(CardSuit.Spades, CardRank.Ten, existingSprites));
            entries.Add(CreateEntry(CardSuit.Spades, CardRank.Jack, existingSprites));
            entries.Add(CreateEntry(CardSuit.Spades, CardRank.Queen, existingSprites));
            entries.Add(CreateEntry(CardSuit.Spades, CardRank.King, existingSprites));
            entries.Add(CreateEntry(CardSuit.Spades, CardRank.Ace, existingSprites));

            // CLUBS: 2-10, J, Q, K, A (13 cards - Monsters)
            entries.Add(CreateEntry(CardSuit.Clubs, CardRank.Two, existingSprites));
            entries.Add(CreateEntry(CardSuit.Clubs, CardRank.Three, existingSprites));
            entries.Add(CreateEntry(CardSuit.Clubs, CardRank.Four, existingSprites));
            entries.Add(CreateEntry(CardSuit.Clubs, CardRank.Five, existingSprites));
            entries.Add(CreateEntry(CardSuit.Clubs, CardRank.Six, existingSprites));
            entries.Add(CreateEntry(CardSuit.Clubs, CardRank.Seven, existingSprites));
            entries.Add(CreateEntry(CardSuit.Clubs, CardRank.Eight, existingSprites));
            entries.Add(CreateEntry(CardSuit.Clubs, CardRank.Nine, existingSprites));
            entries.Add(CreateEntry(CardSuit.Clubs, CardRank.Ten, existingSprites));
            entries.Add(CreateEntry(CardSuit.Clubs, CardRank.Jack, existingSprites));
            entries.Add(CreateEntry(CardSuit.Clubs, CardRank.Queen, existingSprites));
            entries.Add(CreateEntry(CardSuit.Clubs, CardRank.King, existingSprites));
            entries.Add(CreateEntry(CardSuit.Clubs, CardRank.Ace, existingSprites));

            // DIAMONDS: 2-10 only (9 cards - Shields, NO face cards, NO ace)
            entries.Add(CreateEntry(CardSuit.Diamonds, CardRank.Two, existingSprites));
            entries.Add(CreateEntry(CardSuit.Diamonds, CardRank.Three, existingSprites));
            entries.Add(CreateEntry(CardSuit.Diamonds, CardRank.Four, existingSprites));
            entries.Add(CreateEntry(CardSuit.Diamonds, CardRank.Five, existingSprites));
            entries.Add(CreateEntry(CardSuit.Diamonds, CardRank.Six, existingSprites));
            entries.Add(CreateEntry(CardSuit.Diamonds, CardRank.Seven, existingSprites));
            entries.Add(CreateEntry(CardSuit.Diamonds, CardRank.Eight, existingSprites));
            entries.Add(CreateEntry(CardSuit.Diamonds, CardRank.Nine, existingSprites));
            entries.Add(CreateEntry(CardSuit.Diamonds, CardRank.Ten, existingSprites));

            // HEARTS: 2-10 only (9 cards - Potions, NO face cards, NO ace)
            entries.Add(CreateEntry(CardSuit.Hearts, CardRank.Two, existingSprites));
            entries.Add(CreateEntry(CardSuit.Hearts, CardRank.Three, existingSprites));
            entries.Add(CreateEntry(CardSuit.Hearts, CardRank.Four, existingSprites));
            entries.Add(CreateEntry(CardSuit.Hearts, CardRank.Five, existingSprites));
            entries.Add(CreateEntry(CardSuit.Hearts, CardRank.Six, existingSprites));
            entries.Add(CreateEntry(CardSuit.Hearts, CardRank.Seven, existingSprites));
            entries.Add(CreateEntry(CardSuit.Hearts, CardRank.Eight, existingSprites));
            entries.Add(CreateEntry(CardSuit.Hearts, CardRank.Nine, existingSprites));
            entries.Add(CreateEntry(CardSuit.Hearts, CardRank.Ten, existingSprites));

            cardSprites = entries.ToArray();

            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[CardDatabase] Initialized {cardSprites.Length} card entries (44 total). Assign sprites in Inspector.");
        }

        private CardSpriteEntry CreateEntry(
            CardSuit suit,
            CardRank rank,
            System.Collections.Generic.Dictionary<(CardSuit, CardRank), Sprite> existingSprites)
        {
            existingSprites.TryGetValue((suit, rank), out var sprite);
            return new CardSpriteEntry(suit, rank, sprite);
        }
#endif
    }

    /// <summary>
    /// Serializable entry mapping a card to its sprite.
    /// </summary>
    [Serializable]
    public class CardSpriteEntry
    {
        [SerializeField] private CardSuit suit;
        [SerializeField] private CardRank rank;
        [SerializeField] private Sprite sprite;

        public CardSuit Suit => suit;
        public CardRank Rank => rank;
        public Sprite Sprite => sprite;

        public CardSpriteEntry() { }

        public CardSpriteEntry(CardSuit suit, CardRank rank, Sprite sprite)
        {
            this.suit = suit;
            this.rank = rank;
            this.sprite = sprite;
        }
    }

}
