using Cysharp.Threading.Tasks;
using Scoundrel.Core.Data;

namespace Scoundrel.Core.Interfaces
{
    /// <summary>
    /// Interface for the dialog service that manages modal dialogs.
    /// Provides async methods to show dialogs and wait for user responses.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Whether a dialog is currently being displayed.
        /// </summary>
        bool IsDialogActive { get; }

        /// <summary>
        /// Shows a confirmation dialog for shield downgrade.
        /// </summary>
        /// <param name="currentShield">The player's current shield value.</param>
        /// <param name="newShield">The new shield value being equipped.</param>
        /// <returns>True if user confirms the downgrade, false if cancelled.</returns>
        UniTask<bool> ShowShieldDowngradeConfirmAsync(int currentShield, int newShield);

        /// <summary>
        /// Shows a generic confirmation dialog.
        /// </summary>
        /// <param name="title">The dialog title.</param>
        /// <param name="message">The dialog message.</param>
        /// <param name="confirmText">Text for the confirm button.</param>
        /// <param name="cancelText">Text for the cancel button.</param>
        /// <returns>True if user confirms, false if cancelled.</returns>
        UniTask<bool> ShowConfirmDialogAsync(string title, string message, string confirmText = "Confirm", string cancelText = "Cancel");

        /// <summary>
        /// Closes any active dialog immediately.
        /// </summary>
        void CloseActiveDialog();
    }
}
