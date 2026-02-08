using Cysharp.Threading.Tasks;
using Scoundrel.Core.Data;

namespace Scoundrel.Core.Interfaces
{
    /// <summary>
    /// Interface for the command processor service.
    /// Handles validation and execution of player commands.
    /// </summary>
    public interface ICommandProcessor
    {
        /// <summary>
        /// Whether a command is currently being processed.
        /// </summary>
        bool IsProcessing { get; }

        /// <summary>
        /// Processes a card interaction (fight, equip, or drink).
        /// Automatically creates the appropriate command based on card type.
        /// </summary>
        /// <param name="card">The card the player interacted with.</param>
        /// <returns>True if command executed successfully.</returns>
        UniTask<bool> ProcessCardInteractionAsync(CardData card);

        /// <summary>
        /// Processes a run action.
        /// </summary>
        /// <returns>True if command executed successfully.</returns>
        UniTask<bool> ProcessRunAsync();

        /// <summary>
        /// Executes a command directly.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>True if executed successfully.</returns>
        UniTask<bool> ExecuteCommandAsync(ICommand command);

        /// <summary>
        /// Calculates the damage that would be dealt by a monster card.
        /// Used for damage preview UI.
        /// </summary>
        /// <param name="card">The monster card.</param>
        /// <returns>Damage amount (0 if not a monster).</returns>
        int CalculateDamagePreview(CardData card);

        /// <summary>
        /// Checks if equipping the given shield card would be a downgrade.
        /// Used for UI confirmation prompts.
        /// </summary>
        /// <param name="card">The shield card to check.</param>
        /// <returns>True if it would be a downgrade, false otherwise.</returns>
        bool IsShieldDowngrade(CardData card);
    }
}
