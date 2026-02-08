using System;
using Cysharp.Threading.Tasks;
using PrimeTween;
using Scoundrel.Core;
using Scoundrel.Core.Enums;
using Scoundrel.Core.Interfaces;
using Sisus.Init;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scoundrel.UI.Animation
{
    /// <summary>
    /// Handles HUD animations including HP punch, shield flash, and value change feedback.
    /// Uses PrimeTween for smooth animations.
    /// </summary>
    public class HUDAnimator : MonoBehaviour<GameBootstrapper>
    {
        [Header("Health Animation References")]
        [SerializeField] private RectTransform _healthContainer;
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private Image _heartIcon;

        [Header("Shield Animation References")]
        [SerializeField] private RectTransform _shieldContainer;
        [SerializeField] private TextMeshProUGUI _shieldText;
        [SerializeField] private Image _shieldIcon;

        [Header("Deck Animation References")]
        [SerializeField] private RectTransform _deckContainer;

        [Header("Damage Animation")]
        [SerializeField] private float _damagePunchScale = 1.3f;
        [SerializeField] private float _damagePunchDuration = 0.25f;
        [SerializeField] private Color _damageFlashColor = Color.red;
        [SerializeField] private float _damageShakeFrequency = 15f;
        [SerializeField] private float _damageShakeIntensity = 10f;

        [Header("Heal Animation")]
        [SerializeField] private float _healPunchScale = 1.2f;
        [SerializeField] private float _healPunchDuration = 0.3f;
        [SerializeField] private Color _healFlashColor = Color.green;

        [Header("Shield Animation")]
        [SerializeField] private float _shieldPunchScale = 1.25f;
        [SerializeField] private float _shieldPunchDuration = 0.25f;
        [SerializeField] private Color _shieldFlashColor = Color.cyan;
        [SerializeField] private Color _shieldBreakColor = Color.gray;

        [Header("Low Health Warning")]
        [SerializeField] private float _lowHealthPulseSpeed = 1.5f;
        [SerializeField] private float _lowHealthPulseScale = 1.1f;
        [SerializeField] private float _lowHealthThreshold = 0.3f;

        [Header("Deck Animation")]
        [SerializeField] private float _deckDrawPunchScale = 0.9f;
        [SerializeField] private float _deckDrawDuration = 0.2f;

        [Header("Game Over Animation")]
        [SerializeField] private RectTransform _gameOverPanel;
        [SerializeField] private float _gameOverFadeDuration = 0.5f;
        [SerializeField] private float _gameOverScaleFrom = 0.8f;

        private IGameEvents _events;
        private IPlayerState _playerState;
        private bool _isLowHealthPulsing;
        private Color _originalHeartColor;
        private Color _originalHealthTextColor;
        private Color _originalShieldColor;
        private Color _originalShieldTextColor;

        protected override void Init(GameBootstrapper gameBootstrapper)
        {
            _events = gameBootstrapper.Events;
            _playerState = gameBootstrapper.PlayerState;

            // Store original colors
            if (_heartIcon != null) _originalHeartColor = _heartIcon.color;
            if (_healthText != null) _originalHealthTextColor = _healthText.color;
            if (_shieldIcon != null) _originalShieldColor = _shieldIcon.color;
            if (_shieldText != null) _originalShieldTextColor = _shieldText.color;

            Debug.Log("[HUDAnimator] Initialized via Init(args)");
        }

        private void Start()
        {
            if (_events != null)
            {
                _events.OnHPChanged += HandleHpChanged;
                _events.OnShieldChanged += HandleShieldChanged;
                _events.OnDeckCountChanged += HandleDeckCountChanged;
                _events.OnGameEnded += HandleGameEnded;
                _events.OnGameStateChanged += HandleGameStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (_events != null)
            {
                _events.OnHPChanged -= HandleHpChanged;
                _events.OnShieldChanged -= HandleShieldChanged;
                _events.OnDeckCountChanged -= HandleDeckCountChanged;
                _events.OnGameEnded -= HandleGameEnded;
                _events.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        private void HandleHpChanged(int newHP, int delta)
        {
            if (delta < 0)
            {
                AnimateDamageAsync(-delta).Forget();
            }
            else if (delta > 0)
            {
                AnimateHealAsync(delta).Forget();
            }

            // Check for low health warning
            if (_playerState != null)
            {
                float hpPercent = (float)newHP / _playerState.MaxHP;
                if (hpPercent <= _lowHealthThreshold && !_isLowHealthPulsing)
                {
                    StartLowHealthPulse();
                }
                else if (hpPercent > _lowHealthThreshold && _isLowHealthPulsing)
                {
                    StopLowHealthPulse();
                }
            }
        }

        private void HandleShieldChanged(int newShield)
        {
            AnimateShieldChangeAsync(newShield).Forget();
        }

        private void HandleDeckCountChanged(int remainingCards)
        {
            AnimateDeckDraw();
        }

        private void HandleGameEnded(GameResult result)
        {
            AnimateGameOverAsync(result).Forget();
        }

        private void HandleGameStateChanged(GameState newState)
        {
            if (newState == GameState.Initializing)
            {
                StopLowHealthPulse();
                ResetColors();
            }
        }

        /// <summary>
        /// Animates damage taken with punch and shake effects.
        /// </summary>
        private async UniTask AnimateDamageAsync(int damageAmount)
        {
            if (_healthContainer == null) return;

            // Punch scale
            _ = Tween.PunchScale(_healthContainer, Vector3.one * (_damagePunchScale - 1f), _damagePunchDuration);

            // Flash color on heart icon
            if (_heartIcon != null)
            {
                _ = Tween.Color(_heartIcon, _damageFlashColor, _damagePunchDuration * 0.3f);
                await UniTask.Delay(TimeSpan.FromSeconds(_damagePunchDuration * 0.3f));
                _ = Tween.Color(_heartIcon, _originalHeartColor, _damagePunchDuration * 0.7f);
            }

            // Flash color on health text
            if (_healthText != null)
            {
                Color originalColor = _healthText.color;
                _ = Tween.Color(_healthText, _damageFlashColor, _damagePunchDuration * 0.3f);
                await UniTask.Delay(TimeSpan.FromSeconds(_damagePunchDuration * 0.3f));
                _ = Tween.Color(_healthText, originalColor, _damagePunchDuration * 0.7f);
            }

            // Shake effect for significant damage
            if (damageAmount >= 3 && _healthContainer != null)
            {
                AnimateShake(_healthContainer, _damageShakeIntensity, _damageShakeFrequency);
            }
        }

        /// <summary>
        /// Animates healing received.
        /// </summary>
        private async UniTask AnimateHealAsync(int healAmount)
        {
            if (_healthContainer == null) return;

            // Punch scale
            _ = Tween.PunchScale(_healthContainer, Vector3.one * (_healPunchScale - 1f), _healPunchDuration);

            // Flash color on heart icon
            if (_heartIcon != null)
            {
                _ = Tween.Color(_heartIcon, _healFlashColor, _healPunchDuration * 0.3f);
                await UniTask.Delay(TimeSpan.FromSeconds(_healPunchDuration * 0.3f));
                _ = Tween.Color(_heartIcon, _originalHeartColor, _healPunchDuration * 0.7f);
            }

            // Flash color on health text
            if (_healthText != null)
            {
                _ = Tween.Color(_healthText, _healFlashColor, _healPunchDuration * 0.3f);
                await UniTask.Delay(TimeSpan.FromSeconds(_healPunchDuration * 0.3f));

                // Return to appropriate color (may be low health)
                float hpPercent = _playerState != null
                    ? (float)_playerState.CurrentHP / _playerState.MaxHP
                    : 1f;
                Color targetColor = hpPercent <= _lowHealthThreshold ? _damageFlashColor : _originalHealthTextColor;
                _ = Tween.Color(_healthText, targetColor, _healPunchDuration * 0.7f);
            }
        }

        /// <summary>
        /// Animates shield value change.
        /// </summary>
        private async UniTask AnimateShieldChangeAsync(int newShield)
        {
            if (_shieldContainer == null) return;

            // Punch scale
            _ = Tween.PunchScale(_shieldContainer, Vector3.one * (_shieldPunchScale - 1f), _shieldPunchDuration);

            Color flashColor = newShield > 0 ? _shieldFlashColor : _shieldBreakColor;

            // Flash color on shield icon
            if (_shieldIcon != null)
            {
                _ = Tween.Color(_shieldIcon, flashColor, _shieldPunchDuration * 0.3f);
                await UniTask.Delay(TimeSpan.FromSeconds(_shieldPunchDuration * 0.3f));

                Color targetColor = newShield > 0 ? _originalShieldColor : _shieldBreakColor;
                _ = Tween.Color(_shieldIcon, targetColor, _shieldPunchDuration * 0.7f);
            }

            // Flash color on shield text
            if (_shieldText != null)
            {
                _ = Tween.Color(_shieldText, flashColor, _shieldPunchDuration * 0.3f);
                await UniTask.Delay(TimeSpan.FromSeconds(_shieldPunchDuration * 0.3f));

                Color targetColor = newShield > 0 ? _originalShieldTextColor : _shieldBreakColor;
                _ = Tween.Color(_shieldText, targetColor, _shieldPunchDuration * 0.7f);
            }
        }

        /// <summary>
        /// Animates deck card draw.
        /// </summary>
        private void AnimateDeckDraw()
        {
            if (_deckContainer == null) return;

            _ = Tween.PunchScale(_deckContainer, Vector3.one * (_deckDrawPunchScale - 1f), _deckDrawDuration);
        }

        /// <summary>
        /// Starts the low health pulse warning animation.
        /// </summary>
        private void StartLowHealthPulse()
        {
            if (_heartIcon == null || _isLowHealthPulsing) return;

            _isLowHealthPulsing = true;
            AnimateLowHealthPulseLoop().Forget();
        }

        /// <summary>
        /// Stops the low health pulse warning animation.
        /// </summary>
        private void StopLowHealthPulse()
        {
            _isLowHealthPulsing = false;

            if (_healthContainer != null)
            {
                _ = Tween.Scale(_healthContainer, Vector3.one, 0.2f);
            }
        }

        private async UniTask AnimateLowHealthPulseLoop()
        {
            while (_isLowHealthPulsing && _healthContainer != null)
            {
                // Pulse out
                _ = Tween.Scale(_healthContainer, Vector3.one * _lowHealthPulseScale, 1f / _lowHealthPulseSpeed * 0.5f, Ease.InOutSine);
                await UniTask.Delay(TimeSpan.FromSeconds(1f / _lowHealthPulseSpeed * 0.5f));

                if (!_isLowHealthPulsing) break;

                // Pulse in
                _ = Tween.Scale(_healthContainer, Vector3.one, 1f / _lowHealthPulseSpeed * 0.5f, Ease.InOutSine);
                await UniTask.Delay(TimeSpan.FromSeconds(1f / _lowHealthPulseSpeed * 0.5f));
            }
        }

        /// <summary>
        /// Animates game over panel appearance.
        /// </summary>
        private async UniTask AnimateGameOverAsync(GameResult result)
        {
            if (_gameOverPanel == null) return;

            CanvasGroup canvasGroup = _gameOverPanel.GetComponent<CanvasGroup>();

            // Set initial state
            _gameOverPanel.localScale = Vector3.one * _gameOverScaleFrom;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            // Animate in
            _ = Tween.Scale(_gameOverPanel, Vector3.one, _gameOverFadeDuration, Ease.OutBack);

            if (canvasGroup != null)
            {
                _ = Tween.Alpha(canvasGroup, 1f, _gameOverFadeDuration);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(_gameOverFadeDuration));
        }

        /// <summary>
        /// Animates a shake effect on a transform.
        /// </summary>
        private void AnimateShake(RectTransform target, float intensity, float frequency)
        {
            if (target == null) return;

            _ = Tween.ShakeLocalPosition(target, Vector3.one * intensity, _damagePunchDuration, frequency);
        }

        /// <summary>
        /// Resets colors to original values.
        /// </summary>
        private void ResetColors()
        {
            if (_heartIcon != null) _heartIcon.color = _originalHeartColor;
            if (_healthText != null) _healthText.color = _originalHealthTextColor;
            if (_shieldIcon != null) _shieldIcon.color = _originalShieldColor;
            if (_shieldText != null) _shieldText.color = _originalShieldTextColor;
        }

        /// <summary>
        /// Shows damage preview animation on health display.
        /// </summary>
        public void ShowDamagePreviewAnimation()
        {
            if (_healthContainer == null) return;

            // Subtle pulse to indicate preview
            _ = Tween.Scale(_healthContainer, Vector3.one * 1.05f, 0.15f, Ease.OutQuad);
        }

        /// <summary>
        /// Clears damage preview animation.
        /// </summary>
        public void ClearDamagePreviewAnimation()
        {
            if (_healthContainer == null) return;

            _ = Tween.Scale(_healthContainer, Vector3.one, 0.15f, Ease.OutQuad);
        }

        /// <summary>
        /// Shows shield efficiency reduction animation.
        /// </summary>
        public void ShowReducedEfficiencyAnimation(bool show)
        {
            if (_shieldIcon == null) return;

            if (show)
            {
                // Pulse to indicate reduced efficiency
                _ = Tween.Alpha(_shieldIcon, 0.5f, 0.2f);
            }
            else
            {
                _ = Tween.Alpha(_shieldIcon, 1f, 0.2f);
            }
        }
    }
}
