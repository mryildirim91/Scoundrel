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
    /// Requires GridLayoutGroup to be DISABLED - card slots should be manually positioned.
    /// </summary>
    public class CardAnimator : MonoBehaviour<GameBootstrapper>
    {
        [Header("References")]
        [SerializeField] private RoomView _roomView;
        [SerializeField] private RectTransform _deckPosition;

        [Header("Deal Animation")]
        [SerializeField] private float _dealDuration = 0.35f;
        [SerializeField] private float _dealStaggerDelay = 0.08f;
        [SerializeField] private float _dealStartScale = 0.4f;
        [SerializeField] private Ease _dealEase = Ease.OutBack;

        [Header("Discard Animation")]
        [SerializeField] private float _discardDuration = 0.25f;
        [SerializeField] private float _discardEndScale = 0.2f;
        [SerializeField] private Ease _discardEase = Ease.InBack;

        [Header("Interaction Feedback")]
        [SerializeField] private float _tapPunchStrength = 0.12f;
        [SerializeField] private float _tapPunchDuration = 0.2f;

        private IGameEvents _events;
        private Dictionary<CardData, CardView> _cardViewCache = new Dictionary<CardData, CardView>();
        private Dictionary<int, Vector3> _slotTargetPositions = new Dictionary<int, Vector3>();
        private bool _isAnimating;
        private bool _initialized;

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
                _events.OnGameStateChanged += HandleGameStateChanged;
            }

            // Cache the target positions of card slots (their initial positions in the scene)
            CacheSlotPositions();
        }

        private void OnDestroy()
        {
            if (_events != null)
            {
                _events.OnRoomDealt -= HandleRoomDealt;
                _events.OnCardRemovedFromRoom -= HandleCardRemoved;
                _events.OnCardInteracted -= HandleCardInteracted;
                _events.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        /// <summary>
        /// Caches the original positions of card slots as their target positions.
        /// Call this once at start before any animations.
        /// </summary>
        private void CacheSlotPositions()
        {
            if (_roomView == null || _initialized) return;

            for (int i = 0; i < 4; i++)
            {
                CardView cardView = _roomView.GetCardView(i);
                if (cardView == null) continue;

                RectTransform cardRect = cardView.GetComponent<RectTransform>();
                if (cardRect != null)
                {
                    _slotTargetPositions[i] = cardRect.anchoredPosition;
                }
            }

            _initialized = true;
            Debug.Log($"[CardAnimator] Cached {_slotTargetPositions.Count} slot positions");
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

        /// <summary>
        /// Resets all card transforms to their default state and positions.
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

                    // Reset to cached target position
                    if (_slotTargetPositions.TryGetValue(i, out Vector3 targetPos))
                    {
                        cardRect.anchoredPosition = targetPos;
                    }
                }

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                }
            }
        }

        /// <summary>
        /// Gets the deck start position for deal animations.
        /// </summary>
        private Vector2 GetDeckStartPosition(RectTransform cardRect)
        {
            if (_deckPosition != null && cardRect != null)
            {
                // Get the card's parent (Room_View)
                RectTransform cardParent = cardRect.parent as RectTransform;
                if (cardParent != null)
                {
                    // Use InverseTransformPoint to convert deck world position to parent's local space
                    // This works correctly for RectTransforms within the same canvas
                    Vector3 deckWorldPos = _deckPosition.position;
                    Vector3 localPos = cardParent.InverseTransformPoint(deckWorldPos);
                    return new Vector2(localPos.x, localPos.y);
                }
            }

            // Fallback: start from bottom-left corner (deck is on left side of screen)
            return new Vector2(-200f, -300f);
        }

        /// <summary>
        /// Animates dealing cards from the deck to their slots with staggered timing.
        /// </summary>
        private async UniTask AnimateDealCardsAsync(IReadOnlyList<CardData> cards)
        {
            if (_roomView == null) return;

            _isAnimating = true;
            _cardViewCache.Clear();

            // Ensure positions are cached
            if (!_initialized)
            {
                CacheSlotPositions();
            }

            for (int i = 0; i < cards.Count && i < 4; i++)
            {
                CardView cardView = _roomView.GetCardView(i);
                if (cardView == null) continue;

                _cardViewCache[cards[i]] = cardView;

                RectTransform cardRect = cardView.GetComponent<RectTransform>();
                CanvasGroup canvasGroup = cardView.GetComponent<CanvasGroup>();

                if (cardRect == null) continue;

                // Get target position (cached slot position)
                Vector2 targetPosition = _slotTargetPositions.TryGetValue(i, out Vector3 cached)
                    ? (Vector2)cached
                    : cardRect.anchoredPosition;

                // Get start position (from deck)
                Vector2 startPosition = GetDeckStartPosition(cardRect);

                // Set starting state
                cardRect.anchoredPosition = startPosition;
                cardRect.localScale = Vector3.one * _dealStartScale;
                cardRect.localRotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(-15f, 15f));

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 0f;
                }

                // Animate with stagger
                float delay = i * _dealStaggerDelay;

                // Position animation (fly from deck to slot)
                _ = Tween.UIAnchoredPosition(cardRect, targetPosition, _dealDuration, _dealEase, startDelay: delay);

                // Scale up
                _ = Tween.Scale(cardRect, Vector3.one, _dealDuration, _dealEase, startDelay: delay);

                // Rotation to upright
                _ = Tween.LocalRotation(cardRect, Quaternion.identity, _dealDuration * 0.8f, Ease.OutQuad, startDelay: delay);

                // Fade in (quick)
                if (canvasGroup != null)
                {
                    _ = Tween.Alpha(canvasGroup, 1f, _dealDuration * 0.4f, startDelay: delay);
                }
            }

            // Wait for all animations to complete
            float totalDuration = _dealDuration + (_dealStaggerDelay * Math.Max(0, cards.Count - 1));
            await UniTask.Delay(TimeSpan.FromSeconds(totalDuration + 0.05f));
            _isAnimating = false;
        }

        /// <summary>
        /// Animates a card being discarded - flies back toward deck.
        /// </summary>
        private async UniTask AnimateCardDiscardAsync(CardData card)
        {
            if (!_cardViewCache.TryGetValue(card, out CardView cardView)) return;
            if (cardView == null || !cardView.gameObject.activeSelf) return;

            RectTransform cardRect = cardView.GetComponent<RectTransform>();
            CanvasGroup canvasGroup = cardView.GetComponent<CanvasGroup>();

            if (cardRect == null) return;

            _isAnimating = true;

            // Get the slot index to restore position later
            int slotIndex = -1;
            for (int i = 0; i < 4; i++)
            {
                if (_roomView.GetCardView(i) == cardView)
                {
                    slotIndex = i;
                    break;
                }
            }

            // Determine discard direction based on card type
            Vector2 discardOffset;
            float discardRotation;

            if (card.IsMonster)
            {
                // Monsters fly toward player (down)
                discardOffset = new Vector2(0, -300f);
                discardRotation = UnityEngine.Random.Range(10f, 20f);
            }
            else if (card.IsShield)
            {
                // Shields fly to the side
                discardOffset = new Vector2(-250f, -100f);
                discardRotation = -25f;
            }
            else if (card.IsPotion)
            {
                // Potions float up and away
                discardOffset = new Vector2(50f, 200f);
                discardRotation = 15f;
            }
            else
            {
                discardOffset = new Vector2(0, -200f);
                discardRotation = 0f;
            }

            Vector2 targetPosition = cardRect.anchoredPosition + discardOffset;

            // Animate out
            _ = Tween.UIAnchoredPosition(cardRect, targetPosition, _discardDuration, _discardEase);
            _ = Tween.Scale(cardRect, Vector3.one * _discardEndScale, _discardDuration, _discardEase);
            _ = Tween.LocalRotation(cardRect, Quaternion.Euler(0, 0, discardRotation), _discardDuration, _discardEase);

            if (canvasGroup != null)
            {
                _ = Tween.Alpha(canvasGroup, 0f, _discardDuration * 0.8f);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(_discardDuration));

            // Reset transform for reuse
            cardRect.localScale = Vector3.one;
            cardRect.localRotation = Quaternion.identity;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }

            // Restore to original slot position
            if (slotIndex >= 0 && _slotTargetPositions.TryGetValue(slotIndex, out Vector3 originalPos))
            {
                cardRect.anchoredPosition = originalPos;
            }

            _cardViewCache.Remove(card);
            _isAnimating = false;
        }

        /// <summary>
        /// Animates tap/click feedback on a card.
        /// </summary>
        private async UniTask AnimateCardTapAsync(CardData card)
        {
            if (!_cardViewCache.TryGetValue(card, out CardView cardView)) return;
            if (cardView == null) return;

            RectTransform cardRect = cardView.GetComponent<RectTransform>();
            if (cardRect == null) return;

            // Punch scale for bounce effect
            _ = Tween.PunchScale(cardRect, Vector3.one * _tapPunchStrength, _tapPunchDuration);

            await UniTask.Delay(TimeSpan.FromSeconds(_tapPunchDuration));
        }

        /// <summary>
        /// Whether card animations are currently playing.
        /// </summary>
        public bool IsAnimating => _isAnimating;
    }
}
