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
    /// Supports three visual states based on room card count:
    /// - Tactical Retreat (4 cards): Red, costs 1 HP
    /// - Safe Exit (1 card): Green, free
    /// - Dead Zone (2-3 cards) / Disabled: White/Gray
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class RunButtonController : MonoBehaviour<GameBootstrapper>
    {
        [Header("UI References")]
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _buttonText;
        [SerializeField] private Image _buttonImage;

        [Header("Text Settings")]
        [SerializeField] private string _tacticalRetreatText = "RUN\n-1 HP";
        [SerializeField] private string _safeExitText = "RUN\nFREE";
        [SerializeField] private string _disabledText = "RUN\n---";

        [Header("Colors")]
        [SerializeField] private Color _tacticalRetreatColor = Color.red;
        [SerializeField] private Color _safeExitColor = Color.green;
        [SerializeField] private Color _disabledColor = Color.white;

        private GameBootstrapper _gameBootstrapper;
        private IGameEvents _events;
        private IPlayerState _playerState;
        private IRoomSystem _roomSystem;
        private IDeckSystem _deckSystem;
        private IGameManager _gameManager;

        protected override void Init(GameBootstrapper gameBootstrapper)
        {
            _gameBootstrapper = gameBootstrapper;
            _events = gameBootstrapper.Events;
            _playerState = gameBootstrapper.PlayerState;
            _roomSystem = gameBootstrapper.RoomSystem;
            _deckSystem = gameBootstrapper.DeckSystem;
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
                _events.OnCardsAddedToRoom += HandleCardsAdded;
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
                _events.OnCardsAddedToRoom -= HandleCardsAdded;
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

        private void HandleCardsAdded(System.Collections.Generic.IReadOnlyList<Core.Data.CardData> cards)
        {
            UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            if (_playerState == null || _roomSystem == null || _gameManager == null)
            {
                SetButtonState(RunType.None);
                return;
            }

            bool isPlayerTurn = _gameManager.CurrentState == GameState.PlayerTurn;
            if (!isPlayerTurn)
            {
                SetButtonState(RunType.None);
                return;
            }

            RunType runType = DetermineRunType();
            SetButtonState(runType);
        }

        /// <summary>
        /// Determines the current run type based on room state and player conditions.
        /// </summary>
        private RunType DetermineRunType()
        {
            int cardCount = _roomSystem.CardCount;

            if (cardCount == 4)
            {
                // Tactical Retreat: requires CanRun flag (not on cooldown) and enough HP
                bool canRetreat = _playerState.CanRun && _playerState.CurrentHP > 1;
                return canRetreat ? RunType.TacticalRetreat : RunType.None;
            }

            if (cardCount == 1)
            {
                // Safe Exit: requires deck to have cards (no HP cost, no cooldown)
                return _deckSystem != null && !_deckSystem.IsEmpty
                    ? RunType.SafeExit
                    : RunType.None;
            }

            // Dead zone: 0, 2, or 3 cards
            return RunType.None;
        }

        private void SetButtonState(RunType runType)
        {
            switch (runType)
            {
                case RunType.TacticalRetreat:
                    _button.interactable = true;
                    if (_buttonText != null)
                    {
                        _buttonText.text = _tacticalRetreatText;
                        _buttonText.color = _tacticalRetreatColor;
                    }
                    if (_buttonImage != null)
                    {
                        _buttonImage.color = _tacticalRetreatColor;
                    }
                    break;

                case RunType.SafeExit:
                    _button.interactable = true;
                    if (_buttonText != null)
                    {
                        _buttonText.text = _safeExitText;
                        _buttonText.color = _safeExitColor;
                    }
                    if (_buttonImage != null)
                    {
                        _buttonImage.color = _safeExitColor;
                    }
                    break;

                default: // None / Disabled
                    _button.interactable = false;
                    if (_buttonText != null)
                    {
                        _buttonText.text = _disabledText;
                        _buttonText.color = _disabledColor;
                    }
                    if (_buttonImage != null)
                    {
                        _buttonImage.color = _disabledColor;
                    }
                    break;
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
