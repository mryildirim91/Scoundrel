using System;
using Cysharp.Threading.Tasks;
using Scoundrel.Core.Interfaces;
using UnityEngine;

namespace Scoundrel.UI.Dialogs
{
    /// <summary>
    /// Service that manages modal dialogs throughout the game.
    /// Provides async methods to show dialogs and wait for user responses.
    /// Works with UI dialog components to display and handle user input.
    /// </summary>
    public sealed class DialogService : IDialogService
    {
        private ConfirmDialog _confirmDialog;
        private bool _isDialogActive;

        /// <summary>
        /// Whether a dialog is currently being displayed.
        /// </summary>
        public bool IsDialogActive => _isDialogActive;

        /// <summary>
        /// Registers the confirm dialog UI component with this service.
        /// Called by ConfirmDialog during initialization.
        /// </summary>
        public void RegisterConfirmDialog(ConfirmDialog dialog)
        {
            _confirmDialog = dialog;
            Debug.Log("[DialogService] ConfirmDialog registered");
        }

        /// <summary>
        /// Unregisters the confirm dialog when destroyed.
        /// </summary>
        public void UnregisterConfirmDialog(ConfirmDialog dialog)
        {
            if (_confirmDialog == dialog)
            {
                _confirmDialog = null;
                Debug.Log("[DialogService] ConfirmDialog unregistered");
            }
        }

        /// <summary>
        /// Shows a confirmation dialog for shield downgrade.
        /// </summary>
        public async UniTask<bool> ShowShieldDowngradeConfirmAsync(int currentShield, int newShield)
        {
            string title = "Downgrade Shield?";
            // Note: Message uses TMP rich text tags. Ensure Rich Text is enabled on the TextMeshProUGUI component.
            string message = $"Replace your shield?\n\n<color=#00FFFF>{currentShield}</color>  →  <color=#FFA500>{newShield}</color>";

            return await ShowConfirmDialogAsync(title, message, "Equip", "Cancel");
        }

        /// <summary>
        /// Shows a generic confirmation dialog.
        /// </summary>
        public async UniTask<bool> ShowConfirmDialogAsync(string title, string message, string confirmText = "Confirm", string cancelText = "Cancel")
        {
            if (_confirmDialog == null)
            {
                Debug.LogWarning("[DialogService] No ConfirmDialog registered. Auto-confirming.");
                return true;
            }

            if (_isDialogActive)
            {
                Debug.LogWarning("[DialogService] Dialog already active. Ignoring request.");
                return false;
            }

            _isDialogActive = true;

            try
            {
                bool result = await _confirmDialog.ShowAsync(title, message, confirmText, cancelText);
                return result;
            }
            finally
            {
                _isDialogActive = false;
            }
        }

        /// <summary>
        /// Closes any active dialog immediately.
        /// </summary>
        public void CloseActiveDialog()
        {
            if (_confirmDialog != null && _isDialogActive)
            {
                _confirmDialog.Hide();
                _isDialogActive = false;
            }
        }
    }
}
