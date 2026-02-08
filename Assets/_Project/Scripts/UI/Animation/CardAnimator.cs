using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PrimeTween;
using Scoundrel.Core;
using Scoundrel.Core.Data;
using Scoundrel.Core.Enums;
using Scoundrel.Core.Interfaces;
using Scoundrel.UI.Views;
using Sisus.Init;
using UnityEngine;

namespace Scoundrel.UI.Animation
{
    /// <summary>
    /// Handles card animations including deal, flip, discard, and interaction feedback.
    /// Uses PrimeTween for smooth animations.
    /// Works with GridLayoutGroup by only animating scale/alpha, not position.
    /// </summary>
    public class CardAnimator : MonoBehaviour<GameBootstrapper>
    {
        [Header("References")]
        [SerializeField] private RoomView _roomView;

        [Header("Deal Animation")]
        [SerializeField] private float _dealDuration = 0.3f;
        [SerializeField] private float _dealStaggerDelay = 0.1f;
        [SerializeField] private float _dealStartScale = 0.3f;
        [SerializeField] private Ease _dealEase = Ease.OutBack;

        [Header("Discard Animation")]
        [SerializeField] private float _discardDuration = 0.2f;
        [SerializeField] private float _discardEndScale = 0.3f;
        [SerializeField] private Ease _discardEase = Ease.InBack;

        [Header("Interaction Feedback")]
        [SerializeField] private float _tapPunchStrength = 0.15f;
        [SerializeField] private float _tapPunchDuration = 0.2f;

        [Header("Lock Animation")]
        [SerializeField] private float _lockPulseDuration = 0.4f;
        [SerializeField] private float _lockPulseStrength = 0.08f;

        private IGameEvents _events;
        private Dictionary<CardData, CardView> _cardViewCache = new Dictionary<CardData, CardView>();
        private bool _isAnimating;

        protected override void Init(GameBootstrapper gameBootstrapper)
        {
            _events = gameBootstrapper.Events;
            Debug.Log("[CardAnimator] Initialized via Init(args)");
        }

        private void Start()
        {
            if (_events != null)
            {
                _events.OnRoomDealt += HandleRoomDealt;
                _events.OnCardRemovedFromRoom += HandleCardRemoved;
                _events.OnCardInteracted += HandleCardInteracted;
                _events.OnHeartLockChanged += HandleHeartLockChanged;
                _events.OnGameStateChanged += HandleGameStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (_events != null)
            {
                _events.OnRoomDealt -= HandleRoomDealt;
                _events.OnCardRemovedFromRoom -= HandleCardRemoved;
                _events.OnCardInteracted -= HandleCardInteracted;
                _events.OnHeartLockChanged -= HandleHeartLockChanged;
                _events.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        private void HandleGameStateChanged(GameState newState)
        {
            if (newState == GameState.Initializing)
            {
                _cardViewCache.Clear();
                ResetAllCardTransforms();
            }
        }

        private void HandleRoomDealt(IReadOnlyList<CardData> cards)
        {
            AnimateDealCardsAsync(cards).Forget();
        }

        private void HandleCardRemoved(CardData card)
        {
            AnimateCardDiscardAsync(card).Forget();
        }

        private void HandleCardInteracted(CardData card)
        {
            AnimateCardTapAsync(card).Forget();
        }

        private void HandleHeartLockChanged(bool isLocked)
        {
            if (isLocked)
            {
                AnimatePotionLock();
            }
        }

        /// <summary>
        /// Resets all card transforms to default state.
        /// </summary>
        private void ResetAllCardTransforms()
        {
            if (_roomView == null) return;

            for (int i = 0; i < 4; i++)
            {
                CardView cardView = _roomView.GetCardView(i);
                if (cardView == null) continue;

                RectTransform cardRect = cardView.GetComponent<RectTransform>();
                CanvasGroup canvasGroup = cardView.GetComponent<CanvasGroup>();

                if (cardRect != null)
                {
                    cardRect.localScale = Vector3.one;
                    cardRect.localRotation = Quaternion.identity;
                }

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                }
            }
        }

        /// <summary>
        /// Animates dealing cards to the room with staggered timing.
        /// Only animates scale and alpha - lets GridLayoutGroup handle positioning.
        /// </summary>
        private async UniTask AnimateDealCardsAsync(IReadOnlyList<CardData> cards)
        {
            if (_roomView == null) return;

            _isAnimating = true;
            _cardViewCache.Clear();

            for (int i = 0; i < cards.Count && i < 4; i++)
            {
                CardView cardView = _roomView.GetCardView(i);
                if (cardView == null) continue;

                _cardViewCache[cards[i]] = cardView;

                RectTransform cardRect = cardView.GetComponent<RectTransform>();
                CanvasGroup canvasGroup = cardView.GetComponent<CanvasGroup>();

                if (cardRect == null) continue;

                // Set starting state (small and transparent)
                cardRect.localScale = Vector3.one * _dealStartScale;
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 0f;
                }

                // Animate deal with stagger
                float delay = i * _dealStaggerDelay;

                // Scale up to normal size
                _ = Tween.Scale(cardRect, Vector3.one, _dealDuration, _dealEase, startDelay: delay);

                // Fade in
                if (canvasGroup != null)
                {
                    _ = Tween.Alpha(canvasGroup, 1f, _dealDuration * 0.6f, startDelay: delay);
                }
            }

            // Wait for all animations to complete
            await UniTask.Delay(TimeSpan.FromSeconds(_dealDuration + (_dealStaggerDelay * cards.Count)));
            _isAnimating = false;
        }

        /// <summary>
        /// Animates a card being discarded/removed from the room.
        /// Only animates scale and alpha - no position changes.
        /// </summary>
        private async UniTask AnimateCardDiscardAsync(CardData card)
        {
            if (!_cardViewCache.TryGetValue(card, out CardView cardView)) return;
            if (cardView == null || !cardView.gameObject.activeSelf) return;

            RectTransform cardRect = cardView.GetComponent<RectTransform>();
            CanvasGroup canvasGroup = cardView.GetComponent<CanvasGroup>();

            if (cardRect == null) return;

            _isAnimating = true;

            // Animate out - shrink and fade
            _ = Tween.Scale(cardRect, Vector3.one * _discardEndScale, _discardDuration, _discardEase);

            if (canvasGroup != null)
            {
                _ = Tween.Alpha(canvasGroup, 0f, _discardDuration);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(_discardDuration));

            // Reset transform for reuse (important!)
            cardRect.localScale = Vector3.one;
            cardRect.localRotation = Quaternion.identity;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }

            _cardViewCache.Remove(card);
            _isAnimating = false;
        }

        /// <summary>
        /// Animates tap/click feedback on a card using punch scale.
        /// </summary>
        private async UniTask AnimateCardTapAsync(CardData card)
        {
            if (!_cardViewCache.TryGetValue(card, out CardView cardView)) return;
            if (cardView == null) return;

            RectTransform cardRect = cardView.GetComponent<RectTransform>();
            if (cardRect == null) return;

            // Use PrimeTween's PunchScale for a bounce effect
            _ = Tween.PunchScale(cardRect, Vector3.one * _tapPunchStrength, _tapPunchDuration);

            await UniTask.Delay(TimeSpan.FromSeconds(_tapPunchDuration));
        }

        /// <summary>
        /// Animates potion cards becoming locked due to overdose mechanic.
        /// </summary>
        private void AnimatePotionLock()
        {
            if (_roomView == null) return;

            for (int i = 0; i < 4; i++)
            {
                CardView cardView = _roomView.GetCardView(i);
                if (cardView == null || !cardView.gameObject.activeSelf) continue;
                if (!cardView.CardData.IsPotion) continue;

                RectTransform cardRect = cardView.GetComponent<RectTransform>();
                if (cardRect == null) continue;

                // Pulse animation to indicate lock
                _ = Tween.PunchScale(cardRect, Vector3.one * -_lockPulseStrength, _lockPulseDuration);
            }
        }

        /// <summary>
        /// Whether card animations are currently playing.
        /// </summary>
        public bool IsAnimating => _isAnimating;
    }
}
