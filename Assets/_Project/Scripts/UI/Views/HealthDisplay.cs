using Scoundrel.Core;
using Scoundrel.Core.Interfaces;
using Sisus.Init;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scoundrel.UI.Views
{
    /// <summary>
    /// Displays player's current HP.
    /// Format: Heart icon + "15/20"
    /// </summary>
    public class HealthDisplay : MonoBehaviour<GameBootstrapper>
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private Image _heartIcon;

        [Header("Colors")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _lowHealthColor = Color.red;
        [SerializeField] private Color _healColor = Color.green;
        [SerializeField] private float _lowHealthThreshold = 0.3f;

        private IGameEvents _events;
        private IPlayerState _playerState;

        protected override void Init(GameBootstrapper gameBootstrapper)
        {
            _events = gameBootstrapper.Events;
            _playerState = gameBootstrapper.PlayerState;

            Debug.Log("[HealthDisplay] Initialized via Init(args)");
        }

        private void Start()
        {
            // Subscribe to HP changes
            if (_events != null)
            {
                _events.OnHPChanged += HandleHPChanged;
            }

            // Initial update
            if (_playerState != null)
            {
                UpdateDisplay(_playerState.CurrentHP, 0);
            }
        }

        private void OnDestroy()
        {
            if (_events != null)
            {
                _events.OnHPChanged -= HandleHPChanged;
            }
        }

        private void HandleHPChanged(int newHP, int delta)
        {
            UpdateDisplay(newHP, delta);
        }

        private void UpdateDisplay(int currentHP, int delta)
        {
            if (_playerState == null) return;

            int maxHP = _playerState.MaxHP;

            // Update text
            if (_healthText != null)
            {
                _healthText.text = $"{currentHP}/{maxHP}";

                // Change color based on HP percentage
                float hpPercent = (float)currentHP / maxHP;
                if (hpPercent <= _lowHealthThreshold)
                {
                    _healthText.color = _lowHealthColor;
                }
                else if (delta > 0)
                {
                    _healthText.color = _healColor;
                }
                else
                {
                    _healthText.color = _normalColor;
                }
            }

            // Update heart icon color
            if (_heartIcon != null)
            {
                float hpPercent = (float)currentHP / maxHP;
                _heartIcon.color = hpPercent <= _lowHealthThreshold ? _lowHealthColor : _normalColor;
            }
        }

        /// <summary>
        /// Shows damage preview (used when holding on a monster card).
        /// </summary>
        public void ShowDamagePreview(int damage)
        {
            if (_healthText != null && _playerState != null)
            {
                int previewHP = Mathf.Max(0, _playerState.CurrentHP - damage);
                _healthText.text = $"<color=red>{previewHP}</color>/{_playerState.MaxHP}";
            }
        }

        /// <summary>
        /// Clears any damage preview.
        /// </summary>
        public void ClearPreview()
        {
            if (_playerState != null)
            {
                UpdateDisplay(_playerState.CurrentHP, 0);
            }
        }
    }
}
