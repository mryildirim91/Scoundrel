using Scoundrel.Core;
using Scoundrel.Core.Interfaces;
using Sisus.Init;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scoundrel.UI.Views
{
    /// <summary>
    /// Displays the remaining card count in the deck.
    /// Shows as a deck stack with a number overlay.
    /// </summary>
    public class DeckView : MonoBehaviour<GameBootstrapper>
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _countText;
        [SerializeField] private Image _deckImage;

        [Header("Visual Settings")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _lowDeckColor = Color.yellow;
        [SerializeField] private int _lowDeckThreshold = 8;

        private IGameEvents _events;
        private IDeckSystem _deckSystem;

        protected override void Init(GameBootstrapper gameBootstrapper)
        {
            _events = gameBootstrapper.Events;
            _deckSystem = gameBootstrapper.DeckSystem;

            Debug.Log("[DeckView] Initialized via Init(args)");
        }

        private void Start()
        {
            // Subscribe to deck count changes
            if (_events != null)
            {
                _events.OnDeckCountChanged += HandleDeckCountChanged;
            }

            // Initial update
            if (_deckSystem != null)
            {
                UpdateDisplay(_deckSystem.RemainingCards);
            }
        }

        private void OnDestroy()
        {
            if (_events != null)
            {
                _events.OnDeckCountChanged -= HandleDeckCountChanged;
            }
        }

        private void HandleDeckCountChanged(int remainingCards)
        {
            UpdateDisplay(remainingCards);
        }

        private void UpdateDisplay(int remainingCards)
        {
            if (_countText != null)
            {
                _countText.text = remainingCards.ToString();

                // Change color when deck is low
                _countText.color = remainingCards <= _lowDeckThreshold ? _lowDeckColor : _normalColor;
            }

            if (_deckImage != null)
            {
                // Adjust alpha or show empty state when deck is empty
                _deckImage.color = remainingCards > 0 ? _normalColor : new Color(1, 1, 1, 0.3f);
            }
        }
    }
}
