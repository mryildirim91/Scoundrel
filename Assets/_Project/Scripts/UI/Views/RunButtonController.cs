using Scoundrel.Core;
using Scoundrel.Core.Enums;
using Scoundrel.Core.Interfaces;
using Sisus.Init;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scoundrel.UI.Views
{
    /// <summary>
    /// Controls the Run button state and interaction.
    /// Displays "RUN -1 HP" and manages enabled/disabled state.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class RunButtonController : MonoBehaviour<GameBootstrapper>
    {
        [Header("UI References")]
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _buttonText;
        [SerializeField] private Image _buttonImage;

        [Header("Text Settings")]
        [SerializeField] private string _enabledText = "RUN\n-1 HP";
        [SerializeField] private string _disabledText = "RUN\n---";

        [Header("Colors")]
        [SerializeField] private Color _enabledColor = Color.white;
        [SerializeField] private Color _disabledColor = Color.gray;

        private GameBootstrapper _gameBootstrapper;
        private IGameEvents _events;
        private IPlayerState _playerState;
        private IRoomSystem _roomSystem;
        private IGameManager _gameManager;

        protected override void Init(GameBootstrapper gameBootstrapper)
        {
            _gameBootstrapper = gameBootstrapper;
            _events = gameBootstrapper.Events;
            _playerState = gameBootstrapper.PlayerState;
            _roomSystem = gameBootstrapper.RoomSystem;
            _gameManager = gameBootstrapper.GameManager;

            Debug.Log("[RunButtonController] Initialized via Init(args)");
        }
        
        protected override void OnAwake()
        {
            base.OnAwake();
            if (_button == null) _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleRunClicked);
        }

        private void Start()
        {
            // Subscribe to relevant events
            if (_events != null)
            {
                _events.OnRunAvailableChanged += HandleRunAvailableChanged;
                _events.OnGameStateChanged += HandleGameStateChanged;
                _events.OnRoomDealt += HandleRoomDealt;
                _events.OnCardRemovedFromRoom += HandleCardRemoved;
            }

            // Initial state
            UpdateButtonState();
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(HandleRunClicked);

            if (_events != null)
            {
                _events.OnRunAvailableChanged -= HandleRunAvailableChanged;
                _events.OnGameStateChanged -= HandleGameStateChanged;
                _events.OnRoomDealt -= HandleRoomDealt;
                _events.OnCardRemovedFromRoom -= HandleCardRemoved;
            }
        }

        private void HandleRunAvailableChanged(bool canRun)
        {
            UpdateButtonState();
        }

        private void HandleGameStateChanged(GameState newState)
        {
            UpdateButtonState();
        }

        private void HandleRoomDealt(System.Collections.Generic.IReadOnlyList<Core.Data.CardData> cards)
        {
            UpdateButtonState();
        }

        private void HandleCardRemoved(Core.Data.CardData card)
        {
            UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            if (_playerState == null || _roomSystem == null || _gameManager == null)
            {
                SetButtonEnabled(false);
                return;
            }

            bool isPlayerTurn = _gameManager.CurrentState == GameState.PlayerTurn;
            bool canRunFlag = _playerState.CanRun;
            int roomCardCount = _roomSystem.CardCount;
            bool hasEnoughCards = roomCardCount >= 3 && roomCardCount <= 4;
            bool hasEnoughHP = _playerState.CurrentHP > 1;

            bool canRun = isPlayerTurn && canRunFlag && hasEnoughCards && hasEnoughHP;

            SetButtonEnabled(canRun);
        }

        private void SetButtonEnabled(bool enabled)
        {
            _button.interactable = enabled;

            if (_buttonText != null)
            {
                _buttonText.text = enabled ? _enabledText : _disabledText;
                _buttonText.color = enabled ? _enabledColor : _disabledColor;
            }

            if (_buttonImage != null)
            {
                _buttonImage.color = enabled ? _enabledColor : _disabledColor;
            }
        }

        private void HandleRunClicked()
        {
            if (_gameBootstrapper != null && _gameManager != null && _gameManager.CanAcceptInput)
            {
                _gameBootstrapper.OnRunClicked();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_button == null) _button = GetComponent<Button>();
        }
#endif
    }
}
