using System;
using System.Collections.Generic;
using Scoundrel.Core.Data;
using Scoundrel.Core.Interfaces;
using Scoundrel.UI.Views;
using UnityEngine;

namespace Scoundrel.UI.Mobile
{
    /// <summary>
    /// Object pool for CardView instances to reduce GC allocations.
    /// Pre-allocates and reuses CardView objects instead of creating/destroying them.
    /// </summary>
    public class CardViewPool : MonoBehaviour
    {
        [Header("Pool Settings")]
        [SerializeField] private CardView _cardPrefab;
        [SerializeField] private Transform _poolContainer;
        [SerializeField] private int _initialPoolSize = 8;
        [SerializeField] private int _maxPoolSize = 16;

        private readonly Stack<CardView> _availableCards = new();
        private readonly HashSet<CardView> _activeCards = new();

        private ICardDatabase _cardDatabase;
        private ICommandProcessor _commandProcessor;
        private IGameManager _gameManager;
        private Action<int> _onDamagePreviewStart;
        private Action _onDamagePreviewEnd;
        private Action<bool> _onShieldEfficiencyPreview;
        private bool _isInitialized;

        /// <summary>
        /// Number of cards currently in the pool (inactive).
        /// </summary>
        public int AvailableCount => _availableCards.Count;

        /// <summary>
        /// Number of cards currently in use.
        /// </summary>
        public int ActiveCount => _activeCards.Count;

        /// <summary>
        /// Total number of cards managed by this pool.
        /// </summary>
        public int TotalCount => AvailableCount + ActiveCount;

        private void Awake()
        {
            // Create pool container if not assigned
            if (_poolContainer == null)
            {
                var containerObj = new GameObject("CardPool");
                containerObj.transform.SetParent(transform);
                _poolContainer = containerObj.transform;
            }
        }

        /// <summary>
        /// Initializes the pool with required dependencies.
        /// </summary>
        public void Initialize(
            ICardDatabase cardDatabase,
            ICommandProcessor commandProcessor,
            IGameManager gameManager,
            Action<int> onDamagePreviewStart,
            Action onDamagePreviewEnd,
            Action<bool> onShieldEfficiencyPreview)
        {
            _cardDatabase = cardDatabase;
            _commandProcessor = commandProcessor;
            _gameManager = gameManager;
            _onDamagePreviewStart = onDamagePreviewStart;
            _onDamagePreviewEnd = onDamagePreviewEnd;
            _onShieldEfficiencyPreview = onShieldEfficiencyPreview;
            _isInitialized = true;

            // Pre-warm the pool
            PreWarm(_initialPoolSize);

            Debug.Log($"[CardViewPool] Initialized with {_initialPoolSize} cards");
        }

        /// <summary>
        /// Pre-allocates CardView instances.
        /// </summary>
        public void PreWarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (TotalCount >= _maxPoolSize) break;

                CardView card = CreateNewCard();
                card.gameObject.SetActive(false);
                _availableCards.Push(card);
            }
        }

        /// <summary>
        /// Gets a CardView from the pool or creates a new one if necessary.
        /// </summary>
        public CardView Get(Transform parent = null)
        {
            CardView card;

            if (_availableCards.Count > 0)
            {
                card = _availableCards.Pop();
            }
            else if (TotalCount < _maxPoolSize)
            {
                card = CreateNewCard();
            }
            else
            {
                Debug.LogWarning("[CardViewPool] Pool exhausted, creating extra card");
                card = CreateNewCard();
            }

            _activeCards.Add(card);

            // Reparent if needed
            if (parent != null)
            {
                card.transform.SetParent(parent, false);
            }

            card.gameObject.SetActive(true);

            return card;
        }

        /// <summary>
        /// Returns a CardView to the pool for reuse.
        /// </summary>
        public void Return(CardView card)
        {
            if (card == null) return;

            if (!_activeCards.Remove(card))
            {
                Debug.LogWarning("[CardViewPool] Returning card that wasn't from this pool");
            }

            // Reset the card
            card.gameObject.SetActive(false);
            card.transform.SetParent(_poolContainer, false);
            card.Hide();

            _availableCards.Push(card);
        }

        /// <summary>
        /// Returns all active cards to the pool.
        /// </summary>
        public void ReturnAll()
        {
            // Copy to list to avoid modifying collection while iterating
            var activeList = new List<CardView>(_activeCards);
            foreach (var card in activeList)
            {
                Return(card);
            }
        }

        /// <summary>
        /// Clears the entire pool and destroys all cards.
        /// </summary>
        public void Clear()
        {
            foreach (var card in _activeCards)
            {
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }
            _activeCards.Clear();

            while (_availableCards.Count > 0)
            {
                var card = _availableCards.Pop();
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }
        }

        private CardView CreateNewCard()
        {
            if (_cardPrefab == null)
            {
                Debug.LogError("[CardViewPool] Card prefab not assigned!");
                return null;
            }

            CardView card = Instantiate(_cardPrefab, _poolContainer);

            // Initialize with dependencies if available
            if (_isInitialized)
            {
                card.Initialize(
                    _cardDatabase,
                    _commandProcessor,
                    _gameManager,
                    _onDamagePreviewStart,
                    _onDamagePreviewEnd,
                    _onShieldEfficiencyPreview);
            }

            return card;
        }

        private void OnDestroy()
        {
            Clear();
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showPoolStatus = false;

        private void OnGUI()
        {
            if (!_showPoolStatus) return;

            GUILayout.BeginArea(new Rect(Screen.width - 200, 10, 190, 80));
            GUILayout.BeginVertical("box");
            GUILayout.Label($"Card Pool Status");
            GUILayout.Label($"Available: {AvailableCount}");
            GUILayout.Label($"Active: {ActiveCount}");
            GUILayout.Label($"Total: {TotalCount}/{_maxPoolSize}");
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
#endif
    }
}
