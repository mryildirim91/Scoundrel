using Cysharp.Threading.Tasks;
using Scoundrel.Core.Interfaces;
using Scoundrel.Core.Services;
using Scoundrel.ScriptableObjects;
using Sisus.Init;
using UnityEngine;

namespace Scoundrel.Core
{
    /// <summary>
    /// Bootstrapper component that initializes all game services and starts the game.
    /// Uses Init(args) for dependency injection - receives GameSettings and CardDatabase from Services.
    /// Add this component manually to a GameObject in your scene.
    /// </summary>
    public class GameBootstrapper : MonoBehaviour<GameSettings, CardDatabase>
    {
        // Injected dependencies
        private GameSettings _gameSettings;
        private CardDatabase _cardDatabase;

        // Services (created and registered during init)
        private GameEvents _events;
        private PlayerState _playerState;
        private DeckSystem _deckSystem;
        private RoomSystem _roomSystem;
        private CommandProcessor _commandProcessor;
        private GameManager _gameManager;

        // Public accessors for other components (via DI)
        public IGameEvents Events => _events;
        public IPlayerState PlayerState => _playerState;
        public IDeckSystem DeckSystem => _deckSystem;
        public IRoomSystem RoomSystem => _roomSystem;
        public ICommandProcessor CommandProcessor => _commandProcessor;
        public IGameManager GameManager => _gameManager;
        public IGameSettings Settings => _gameSettings;
        public ICardDatabase CardDatabase => _cardDatabase;

        protected override void Init(GameSettings gameSettings, CardDatabase cardDatabase)
        {
            _gameSettings = gameSettings;
            _cardDatabase = cardDatabase;

            Debug.Log("[GameBootstrapper] Init called with dependencies");

            // Initialize all services
            InitializeServices();
        }

        private void Start()
        {
            // Start the game
            StartGameAsync().Forget();
        }

        private void OnDestroy()
        {
            // Cleanup
            _gameManager?.Dispose();
            _events?.ClearAllSubscriptions();
        }

        /// <summary>
        /// Initializes all game services with proper dependency injection.
        /// </summary>
        private void InitializeServices()
        {
            // Create services in dependency order
            _events = new GameEvents();
            _playerState = new PlayerState(_gameSettings, _events);
            _deckSystem = new DeckSystem(_events);
            _roomSystem = new RoomSystem(_events);
            _commandProcessor = new CommandProcessor(_playerState, _roomSystem, _deckSystem);
            _gameManager = new GameManager(
                _gameSettings,
                _events,
                _playerState,
                _deckSystem,
                _roomSystem,
                _commandProcessor);

            Debug.Log("[GameBootstrapper] All services created");
        }

        /// <summary>
        /// Starts the game asynchronously.
        /// </summary>
        private async UniTask StartGameAsync()
        {
            Debug.Log("[GameBootstrapper] Starting game...");
            await _gameManager.StartGameAsync();
        }

        /// <summary>
        /// Restarts the game. Can be called from UI buttons.
        /// </summary>
        public void RestartGame()
        {
            _gameManager.RestartGameAsync().Forget();
        }

        /// <summary>
        /// Handles card interaction from UI. Call this when player taps a card.
        /// </summary>
        public void OnCardClicked(int cardIndex)
        {
            var cards = _roomSystem.CurrentCards;
            if (cardIndex < 0 || cardIndex >= cards.Count)
            {
                Debug.LogWarning($"[GameBootstrapper] Invalid card index: {cardIndex}");
                return;
            }

            _gameManager.HandleCardInteractionAsync(cards[cardIndex]).Forget();
        }

        /// <summary>
        /// Handles run button click from UI.
        /// </summary>
        public void OnRunClicked()
        {
            _gameManager.HandleRunAsync().Forget();
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo = false;
        private Vector2 _debugScrollPosition;
        private GUIStyle _labelStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _buttonStyle;

        private void InitDebugStyles()
        {
            if (_labelStyle != null) return;

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                richText = true
            };

            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                richText = true
            };

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 18,
                fixedHeight = 45
            };
        }

        private void OnGUI()
        {
            if (!_showDebugInfo) return;

            InitDebugStyles();

            GUILayout.BeginArea(new Rect(10, 10, 450, Screen.height - 20));
            _debugScrollPosition = GUILayout.BeginScrollView(_debugScrollPosition, "box");

            GUILayout.Label("Scoundrel Debug", _headerStyle);
            GUILayout.Space(10);

            // Check if services are initialized
            if (_gameManager == null)
            {
                GUILayout.Label("<color=red>Services not initialized!</color>", _labelStyle);
                GUILayout.Label(_gameSettings == null
                    ? "GameSettings is NULL - check Services component!"
                    : "GameSettings assigned but init failed", _labelStyle);
                GUILayout.EndScrollView();
                GUILayout.EndArea();
                return;
            }

            // Game state info
            GUILayout.Label($"State: <b>{_gameManager.CurrentState}</b>", _labelStyle);
            GUILayout.Label($"Result: {_gameManager.Result}", _labelStyle);
            GUILayout.Space(10);

            // Player state
            GUILayout.Label($"HP: <color=red>{_playerState.CurrentHP}</color> / {_playerState.MaxHP}", _labelStyle);
            GUILayout.Label($"Shield: <color=cyan>{_playerState.ShieldValue}</color>", _labelStyle);
            GUILayout.Label($"Can Run: {(_playerState.CanRun ? "Yes" : "<color=gray>No</color>")}", _labelStyle);
            GUILayout.Space(10);

            // Deck/Room info
            GUILayout.Label($"Deck: {_deckSystem.RemainingCards} cards", _labelStyle);
            GUILayout.Label($"Room: {_roomSystem.CardCount} cards", _labelStyle);
            GUILayout.Space(10);

            // Display room cards
            GUILayout.Label("Room Cards:", _labelStyle);
            if (_roomSystem.CardCount == 0)
            {
                GUILayout.Label("  (empty)", _labelStyle);
            }
            else
            {
                foreach (var card in _roomSystem.CurrentCards)
                {
                    string cardColor = card.IsMonster ? "red" : card.IsShield ? "cyan" : "lime";
                    GUILayout.Label($"  - <color={cardColor}>{card}</color>", _labelStyle);
                }
            }

            GUILayout.Space(15);

            // Always show Restart button
            if (GUILayout.Button("Restart Game", _buttonStyle))
            {
                RestartGame();
            }

            GUILayout.Space(10);

            // Show card buttons (with disabled state indication)
            bool canInteract = _gameManager.CanAcceptInput;

            if (_roomSystem.CardCount > 0)
            {
                GUILayout.Label(canInteract ? "Click Card:" : "Click Card: <color=yellow>(waiting...)</color>", _labelStyle);
                GUILayout.Space(5);

                GUI.enabled = canInteract;
                for (int i = 0; i < _roomSystem.CardCount; i++)
                {
                    var card = _roomSystem.CurrentCards[i];
                    string buttonText = $"{card}";

                    GUI.enabled = canInteract;
                    if (GUILayout.Button(buttonText, _buttonStyle))
                    {
                        OnCardClicked(i);
                    }
                }
                GUI.enabled = true;

                // Run button
                GUILayout.Space(10);
                bool canRun = canInteract && _playerState.CanRun && _roomSystem.CardCount >= 3;
                GUI.enabled = canRun;
                string runText = !_playerState.CanRun
                    ? "RUN (used last turn)"
                    : _roomSystem.CardCount < 3
                        ? $"RUN (need 3+ cards)"
                        : "RUN (-1 HP)";
                if (GUILayout.Button(runText, _buttonStyle))
                {
                    OnRunClicked();
                }
                GUI.enabled = true;
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
#endif
    }
}
