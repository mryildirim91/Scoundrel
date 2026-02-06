using Scoundrel.Core;
using Scoundrel.Core.Enums;
using Scoundrel.Core.Interfaces;
using Sisus.Init;
using UnityEngine;
using UnityEngine.UI;

namespace Scoundrel.UI.Views
{
    /// <summary>
    /// Orchestrates all HUD elements and manages game over display.
    /// </summary>
    public class HUDController : MonoBehaviour<GameBootstrapper>
    {
        [Header("HUD Components")]
        [SerializeField] private HealthDisplay _healthDisplay;
        [SerializeField] private ShieldDisplay _shieldDisplay;
        [SerializeField] private DeckView _deckView;
        [SerializeField] private RunButtonController _runButton;
        [SerializeField] private RoomView _roomView;

        [Header("Game Over Panel")]
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TMPro.TextMeshProUGUI _gameOverText;
        [SerializeField] private Button _restartButton;

        private GameBootstrapper _gameBootstrapper;
        private IGameEvents _events;

        protected override void Init(GameBootstrapper gameBootstrapper)
        {
            _gameBootstrapper = gameBootstrapper;
            _events = gameBootstrapper.Events;

            Debug.Log("[HUDController] Initialized via Init(args)");
        }

        private void Start()
        {
            // Subscribe to game events
            if (_events != null)
            {
                _events.OnGameEnded += HandleGameEnded;
                _events.OnGameStateChanged += HandleGameStateChanged;
            }

            // Setup restart button
            if (_restartButton != null)
            {
                _restartButton.onClick.AddListener(HandleRestartClicked);
            }

            // Hide game over panel initially
            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (_events != null)
            {
                _events.OnGameEnded -= HandleGameEnded;
                _events.OnGameStateChanged -= HandleGameStateChanged;
            }

            if (_restartButton != null)
            {
                _restartButton.onClick.RemoveListener(HandleRestartClicked);
            }
        }

        private void HandleGameStateChanged(GameState newState)
        {
            // Hide game over panel when game restarts
            if (newState == GameState.Initializing && _gameOverPanel != null)
            {
                _gameOverPanel.SetActive(false);
            }
        }

        private void HandleGameEnded(GameResult result)
        {
            Debug.Log($"[HUDController] Game ended: {result}");

            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(true);
            }

            if (_gameOverText != null)
            {
                switch (result)
                {
                    case GameResult.Victory:
                        _gameOverText.text = "VICTORY!\nYou cleared the dungeon!";
                        _gameOverText.color = Color.green;
                        break;
                    case GameResult.Defeat:
                        _gameOverText.text = "DEFEAT\nYou were slain...";
                        _gameOverText.color = Color.red;
                        break;
                    default:
                        _gameOverText.text = "Game Over";
                        _gameOverText.color = Color.white;
                        break;
                }
            }
        }

        private void HandleRestartClicked()
        {
            if (_gameBootstrapper != null)
            {
                _gameBootstrapper.RestartGame();
            }
        }

        /// <summary>
        /// Shows damage preview on the health display.
        /// </summary>
        public void ShowDamagePreview(int damage)
        {
            if (_healthDisplay != null)
            {
                _healthDisplay.ShowDamagePreview(damage);
            }
        }

        /// <summary>
        /// Shows shield efficiency reduction indicator.
        /// </summary>
        public void ShowReducedShieldEfficiency(bool show)
        {
            if (_shieldDisplay != null)
            {
                _shieldDisplay.ShowReducedEfficiency(show);
            }
        }

        /// <summary>
        /// Clears all previews.
        /// </summary>
        public void ClearPreviews()
        {
            if (_healthDisplay != null)
            {
                _healthDisplay.ClearPreview();
            }

            if (_shieldDisplay != null)
            {
                _shieldDisplay.ClearPreview();
            }
        }
    }
}
