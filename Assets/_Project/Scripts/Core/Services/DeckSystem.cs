using System;
using System.Collections.Generic;
using Scoundrel.Core.Data;
using Scoundrel.Core.Factory;
using Scoundrel.Core.Interfaces;
using UnityEngine;

namespace Scoundrel.Core.Services
{
    /// <summary>
    /// Manages the deck of cards: shuffling, drawing, and returning cards.
    /// Uses Fisher-Yates shuffle algorithm for proper randomization.
    /// </summary>
    public sealed class DeckSystem : IDeckSystem
    {
        private readonly GameEvents _events;
        private readonly List<CardData> _cards;
        private readonly System.Random _random;

        public int RemainingCards => _cards.Count;
        public bool IsEmpty => _cards.Count == 0;

        public DeckSystem(GameEvents events)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _cards = new List<CardData>(DeckFactory.ExpectedDeckSize);
            _random = new System.Random();
        }

        /// <summary>
        /// Initializes the deck with the standard 44-card composition.
        /// </summary>
        public void Initialize()
        {
            _cards.Clear();
            _cards.AddRange(DeckFactory.CreateDeck());

            Debug.Log($"[DeckSystem] Initialized with {_cards.Count} cards");

            _events.RaiseDeckCountChanged(_cards.Count);
        }

        /// <summary>
        /// Shuffles the deck using Fisher-Yates algorithm.
        /// This provides a uniform random distribution.
        /// </summary>
        public void Shuffle()
        {
            int n = _cards.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                // Swap cards[i] and cards[j]
                (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
            }

            Debug.Log($"[DeckSystem] Shuffled {n} cards");
        }

        /// <summary>
        /// Draws the specified number of cards from the top of the deck.
        /// </summary>
        /// <param name="count">Number of cards to draw.</param>
        /// <returns>List of drawn cards. May be fewer than requested if deck is low.</returns>
        public List<CardData> Draw(int count)
        {
            if (count <= 0)
            {
                return new List<CardData>();
            }

            int actualCount = Mathf.Min(count, _cards.Count);
            var drawnCards = new List<CardData>(actualCount);

            // Draw from the "top" (end of list for efficiency)
            for (int i = 0; i < actualCount; i++)
            {
                int lastIndex = _cards.Count - 1;
                drawnCards.Add(_cards[lastIndex]);
                _cards.RemoveAt(lastIndex);
            }

            _events.RaiseDeckCountChanged(_cards.Count);

            Debug.Log($"[DeckSystem] Drew {drawnCards.Count} cards. Remaining: {_cards.Count}");

            return drawnCards;
        }

        /// <summary>
        /// Returns cards to the bottom of the deck (used by Run action).
        /// </summary>
        /// <param name="cards">Cards to return to the deck.</param>
        public void ReturnToBottom(IReadOnlyList<CardData> cards)
        {
            if (cards == null || cards.Count == 0) return;

            // Insert at index 0 (bottom of deck)
            // We insert in reverse order so the first card in the list
            // ends up at the bottom
            for (int i = cards.Count - 1; i >= 0; i--)
            {
                _cards.Insert(0, cards[i]);
            }

            _events.RaiseDeckCountChanged(_cards.Count);

            Debug.Log($"[DeckSystem] Returned {cards.Count} cards to bottom. Total: {_cards.Count}");
        }

        /// <summary>
        /// Resets the deck to its initial state for a new game.
        /// </summary>
        public void Reset()
        {
            Initialize();
            Shuffle();

            Debug.Log("[DeckSystem] Reset and shuffled");
        }

        /// <summary>
        /// Peeks at the top card without removing it (for debugging).
        /// </summary>
        public CardData? PeekTop()
        {
            if (_cards.Count == 0) return null;
            return _cards[^1];
        }
    }
}
