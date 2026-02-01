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
        /// Editor utility to auto-populate card sprites from a folder.
        /// </summary>
        [ContextMenu("Auto-Populate Sprites")]
        private void AutoPopulateSprites()
        {
            Debug.Log("Use the CardDatabaseEditor to auto-populate sprites.");
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
