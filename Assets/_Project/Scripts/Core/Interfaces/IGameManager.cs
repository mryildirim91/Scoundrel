using Cysharp.Threading.Tasks;
using Scoundrel.Core.Data;
using Scoundrel.Core.Enums;

namespace Scoundrel.Core.Interfaces
{
    /// <summary>
    /// Interface for the main game orchestrator.
    /// Manages the game state machine and coordinates all game systems.
    /// </summary>
    public interface IGameManager
    {
        /// <summary>
        /// Current state of the game flow.
        /// </summary>
        GameState CurrentState { get; }

        /// <summary>
        /// Result of the game (None while in progress).
        /// </summary>
        GameResult Result { get; }

        /// <summary>
        /// Whether the game is currently active (not GameOver).
        /// </summary>
        bool IsGameActive { get; }

        /// <summary>
        /// Whether player input is currently allowed.
        /// </summary>
        bool CanAcceptInput { get; }

        /// <summary>
        /// Starts a new game. Initializes deck, resets player state, deals first room.
        /// </summary>
        UniTask StartGameAsync();

        /// <summary>
        /// Handles player interaction with a card.
        /// </summary>
        /// <param name="card">The card the player selected.</param>
        UniTask HandleCardInteractionAsync(CardData card);

        /// <summary>
        /// Handles player using the Run action.
        /// </summary>
        UniTask HandleRunAsync();

        /// <summary>
        /// Restarts the game (convenience method that calls StartGameAsync).
        /// </summary>
        UniTask RestartGameAsync();
    }
}
