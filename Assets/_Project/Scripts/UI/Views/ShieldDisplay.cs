using Scoundrel.Core;
using Scoundrel.Core.Interfaces;
using Sisus.Init;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scoundrel.UI.Views
{
    /// <summary>
    /// Displays player's current shield value.
    /// Format: Diamond icon + "8"
    /// </summary>
    public class ShieldDisplay : MonoBehaviour<GameBootstrapper>
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _shieldText;
        [SerializeField] private Image _shieldIcon;

        [Header("Colors")]
        [SerializeField] private Color _normalColor = Color.cyan;
        [SerializeField] private Color _noShieldColor = Color.gray;
        [SerializeField] private Color _reducedEfficiencyColor = new Color(0.5f, 0.8f, 1f, 0.5f);

        private IGameEvents _events;
        private IPlayerState _playerState;
        private bool _showingReducedEfficiency;

        protected override void Init(GameBootstrapper gameBootstrapper)
        {
            _events = gameBootstrapper.Events;
            _playerState = gameBootstrapper.PlayerState;

            Debug.Log("[ShieldDisplay] Initialized via Init(args)");
        }

        private void Start()
        {
            // Subscribe to shield changes
            if (_events != null)
            {
                _events.OnShieldChanged += HandleShieldChanged;
            }

            // Initial update
            if (_playerState != null)
            {
                UpdateDisplay(_playerState.ShieldValue);
            }
        }

        private void OnDestroy()
        {
            if (_events != null)
            {
                _events.OnShieldChanged -= HandleShieldChanged;
            }
        }

        private void HandleShieldChanged(int newShield)
        {
            UpdateDisplay(newShield);
        }

        private void UpdateDisplay(int shieldValue)
        {
            if (_shieldText != null)
            {
                _shieldText.text = shieldValue.ToString();

                // Color based on shield state
                if (shieldValue == 0)
                {
                    _shieldText.color = _noShieldColor;
                }
                else if (_showingReducedEfficiency)
                {
                    _shieldText.color = _reducedEfficiencyColor;
                }
                else
                {
                    _shieldText.color = _normalColor;
                }
            }

            if (_shieldIcon != null)
            {
                _shieldIcon.color = shieldValue == 0 ? _noShieldColor : _normalColor;
            }
        }

        /// <summary>
        /// Shows that shield is at reduced efficiency (for Clubs monsters).
        /// </summary>
        public void ShowReducedEfficiency(bool show)
        {
            _showingReducedEfficiency = show;

            if (_playerState != null)
            {
                UpdateDisplay(_playerState.ShieldValue);
            }

            if (_shieldIcon != null)
            {
                _shieldIcon.color = show ? _reducedEfficiencyColor : _normalColor;
            }
        }

        /// <summary>
        /// Shows a preview of what the shield value would be after equipping a new shield.
        /// </summary>
        public void ShowShieldPreview(int newShieldValue)
        {
            if (_shieldText != null)
            {
                bool isDowngrade = newShieldValue < (_playerState?.ShieldValue ?? 0);
                string colorTag = isDowngrade ? "orange" : "lime";
                _shieldText.text = $"<color={colorTag}>{newShieldValue}</color>";
            }
        }

        /// <summary>
        /// Clears any preview.
        /// </summary>
        public void ClearPreview()
        {
            if (_playerState != null)
            {
                UpdateDisplay(_playerState.ShieldValue);
            }
        }
    }
}
