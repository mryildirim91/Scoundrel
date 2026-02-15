using System;
using System.Collections.Generic;
using Scoundrel.Core.Data;
using Scoundrel.Core.Enums;
using Scoundrel.Core.Interfaces;
using UnityEngine;
using ZLinq;

namespace Scoundrel.Core.Services
{
    /// <summary>
    /// Manages the current room (4 cards displayed to the player).
    /// Handles card placement, removal, and queries.
    /// </summary>
    public sealed class RoomSystem : IRoomSystem
    {
        private readonly GameEvents _events;
        private readonly List<CardData> _currentCards;

        public IReadOnlyList<CardData> CurrentCards => _currentCards;
        public int CardCount => _currentCards.Count;
        public bool IsEmpty => _currentCards.Count == 0;

        public RoomSystem(GameEvents events)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _currentCards = new List<CardData>(4);
        }

        /// <summary>
        /// Sets the cards in the room (called when dealing).
        /// </summary>
        public void SetCards(List<CardData> cards)
        {
            _currentCards.Clear();

            if (cards != null)
            {
                _currentCards.AddRange(cards);
            }

            _events.RaiseRoomDealt(_currentCards);

            Debug.Log($"[RoomSystem] Room set with {_currentCards.Count} cards");
        }

        /// <summary>
        /// Removes a specific card from the room (after player interacts with it).
        /// </summary>
        public void RemoveCard(CardData card)
        {
            bool removed = _currentCards.Remove(card);

            if (removed)
            {
                _events.RaiseCardRemovedFromRoom(card);
                Debug.Log($"[RoomSystem] Removed {card}. Remaining: {_currentCards.Count}");

                if (IsEmpty)
                {
                    _events.RaiseRoomCleared();
                    Debug.Log("[RoomSystem] Room cleared");
                }
            }
            else
            {
                Debug.LogWarning($"[RoomSystem] Card not found in room: {card}");
            }
        }

        /// <summary>
        /// Clears and returns all remaining cards (used by Run action).
        /// </summary>
        public List<CardData> ClearRoom()
        {
            var cards = new List<CardData>(_currentCards);
            _currentCards.Clear();

            _events.RaiseRoomCleared();

            Debug.Log($"[RoomSystem] Cleared room, returning {cards.Count} cards");

            return cards;
        }

        /// <summary>
        /// Adds cards to the existing room without clearing current cards.
        /// Used by Safe Exit to deal new cards while keeping the carried-over card.
        /// Fires OnCardsAddedToRoom (not OnRoomDealt) so UI only animates new cards.
        /// </summary>
        public void AddCards(List<CardData> cards)
        {
            if (cards == null || cards.Count == 0) return;

            _currentCards.AddRange(cards);

            // Fire the additive event (not RoomDealt which would reset the UI)
            _events.RaiseCardsAddedToRoom(cards);

            Debug.Log($"[RoomSystem] Added {cards.Count} cards to room. Total: {_currentCards.Count}");
        }

        /// <summary>
        /// Gets all potion cards currently in the room.
        /// Used for visual feedback on overdose lock state.
        /// </summary>
        public IReadOnlyList<CardData> GetPotionsInRoom()
        {
            return _currentCards.AsValueEnumerable()
                .Where(c => c.Type == CardType.Potion)
                .ToList();
        }

        /// <summary>
        /// Gets all monster cards currently in the room.
        /// </summary>
        public IReadOnlyList<CardData> GetMonstersInRoom()
        {
            return _currentCards.AsValueEnumerable()
                .Where(c => c.Type == CardType.Monster)
                .ToList();
        }

        /// <summary>
        /// Gets all shield cards currently in the room.
        /// </summary>
        public IReadOnlyList<CardData> GetShieldsInRoom()
        {
            return _currentCards.AsValueEnumerable()
                .Where(c => c.Type == CardType.Shield)
                .ToList();
        }

        /// <summary>
        /// Checks if a specific card exists in the room.
        /// </summary>
        public bool ContainsCard(CardData card)
        {
            return _currentCards.Contains(card);
        }

        /// <summary>
        /// Gets a card at the specified index.
        /// </summary>
        public CardData? GetCardAt(int index)
        {
            if (index < 0 || index >= _currentCards.Count)
            {
                return null;
            }
            return _currentCards[index];
        }

        /// <summary>
        /// Gets the index of a card in the room.
        /// </summary>
        public int GetCardIndex(CardData card)
        {
            return _currentCards.IndexOf(card);
        }
    }
}
