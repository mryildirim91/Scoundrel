using Cysharp.Threading.Tasks;

namespace Scoundrel.Core.Interfaces
{
    /// <summary>
    /// Command pattern interface for player actions.
    /// All game actions (fight, equip shield, drink potion, run) implement this interface.
    /// Commands are validated before execution and can be async for animation support.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Checks if the command can be executed in current game state.
        /// </summary>
        /// <returns>True if the command is valid and can execute.</returns>
        bool CanExecute();

        /// <summary>
        /// Executes the command. May include animations via UniTask.
        /// </summary>
        UniTask ExecuteAsync();
    }
}
