using System;
using Cysharp.Threading.Tasks;
using PrimeTween;
using Scoundrel.Core;
using Scoundrel.Core.Enums;
using Scoundrel.Core.Interfaces;
using Sisus.Init;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scoundrel.UI.Dialogs
{
    /// <summary>
    /// UI component for displaying confirmation dialogs.
    /// Used for shield downgrade prompts and other confirmations.
    /// </summary>
    public class ConfirmDialog : MonoBehaviour<GameBootstrapper>
    {
        [Header("Dialog Components")]
        [SerializeField] private GameObject _dialogContainer;
        [SerializeField] private RectTransform _dialogPanel;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _backdrop;

        [Header("Content")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _messageText;

        [Header("Buttons")]
        [SerializeField] private Button _confirmButton;
        [SerializeField] private TextMeshProUGUI _confirmButtonText;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private TextMeshProUGUI _cancelButtonText;

        [Header("Animation Settings")]
        [SerializeField] private float _showDuration = 0.3f;
        [SerializeField] private float _hideDuration = 0.2f;
        [SerializeField] private float _scaleFrom = 0.8f;
        [SerializeField] private Ease _showEase = Ease.OutBack;
        [SerializeField] private Ease _hideEase = Ease.InQuad;
        [SerializeField] private float _backdropAlpha = 0.6f;

        private DialogService _dialogService;
        private IGameEvents _events;
        private UniTaskCompletionSource<bool> _completionSource;
        private bool _isVisible;

        protected override void Init(GameBootstrapper gameBootstrapper)
        {
            _events = gameBootstrapper.Events;

            // Get DialogService from bootstrapper - cast from IDialogService to concrete DialogService
            _dialogService = gameBootstrapper.DialogService as DialogService;

            if (_dialogService == null)
            {
                Debug.LogWarning("[ConfirmDialog] DialogService not found on GameBootstrapper.");
            }

            Debug.Log("[ConfirmDialog] Initialized via Init(args)");
        }

        private void Start()
        {
            // Register with DialogService
            if (_dialogService != null)
            {
                _dialogService.RegisterConfirmDialog(this);
            }

            // Setup button listeners
            if (_confirmButton != null)
            {
                _confirmButton.onClick.AddListener(OnConfirmClicked);
            }

            if (_cancelButton != null)
            {
                _cancelButton.onClick.AddListener(OnCancelClicked);
            }

            // Subscribe to game events to auto-close on game restart
            if (_events != null)
            {
                _events.OnGameStateChanged += HandleGameStateChanged;
            }

            // Hide initially
            HideImmediate();
        }

        private void OnDestroy()
        {
            // Unregister from DialogService
            if (_dialogService != null)
            {
                _dialogService.UnregisterConfirmDialog(this);
            }

            // Cleanup button listeners
            if (_confirmButton != null)
            {
                _confirmButton.onClick.RemoveListener(OnConfirmClicked);
            }

            if (_cancelButton != null)
            {
                _cancelButton.onClick.RemoveListener(OnCancelClicked);
            }

            // Unsubscribe from events
            if (_events != null)
            {
                _events.OnGameStateChanged -= HandleGameStateChanged;
            }

            // Complete any pending task
            _completionSource?.TrySetResult(false);
        }

        private void HandleGameStateChanged(GameState newState)
        {
            // Auto-close dialog on game restart
            if (newState == GameState.Initializing && _isVisible)
            {
                Hide();
            }
        }

        /// <summary>
        /// Sets the DialogService reference. Called after bootstrapper creates the service.
        /// </summary>
        public void SetDialogService(DialogService dialogService)
        {
            _dialogService = dialogService;
            _dialogService?.RegisterConfirmDialog(this);
        }

        /// <summary>
        /// Shows the dialog with the specified content and waits for user response.
        /// </summary>
        public async UniTask<bool> ShowAsync(string title, string message, string confirmText, string cancelText)
        {
            if (_isVisible)
            {
                Debug.LogWarning("[ConfirmDialog] Dialog already visible");
                return false;
            }

            // Setup content
            if (_titleText != null) _titleText.text = title;
            if (_messageText != null) _messageText.text = message;
            if (_confirmButtonText != null) _confirmButtonText.text = confirmText;
            if (_cancelButtonText != null) _cancelButtonText.text = cancelText;

            // Create completion source for async result
            _completionSource = new UniTaskCompletionSource<bool>();

            // Show with animation
            await ShowAnimatedAsync();

            // Wait for user response
            bool result = await _completionSource.Task;

            return result;
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        public void Hide()
        {
            if (!_isVisible) return;

            HideAnimatedAsync().Forget();
        }

        /// <summary>
        /// Hides the dialog immediately without animation.
        /// </summary>
        private void HideImmediate()
        {
            _isVisible = false;

            if (_dialogContainer != null)
            {
                _dialogContainer.SetActive(false);
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            if (_dialogPanel != null)
            {
                _dialogPanel.localScale = Vector3.one * _scaleFrom;
            }

            if (_backdrop != null)
            {
                var color = _backdrop.color;
                color.a = 0f;
                _backdrop.color = color;
            }
        }

        private async UniTask ShowAnimatedAsync()
        {
            _isVisible = true;

            // Activate container
            if (_dialogContainer != null)
            {
                _dialogContainer.SetActive(true);
            }

            // Set initial state
            if (_dialogPanel != null)
            {
                _dialogPanel.localScale = Vector3.one * _scaleFrom;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = true;
            }

            if (_backdrop != null)
            {
                var color = _backdrop.color;
                color.a = 0f;
                _backdrop.color = color;
            }

            // Animate in
            if (_dialogPanel != null)
            {
                _ = Tween.Scale(_dialogPanel, Vector3.one, _showDuration, _showEase);
            }

            if (_canvasGroup != null)
            {
                _ = Tween.Alpha(_canvasGroup, 1f, _showDuration);
            }

            if (_backdrop != null)
            {
                _ = Tween.Alpha(_backdrop, _backdropAlpha, _showDuration);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(_showDuration));

            // Enable interaction after animation
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = true;
            }
        }

        private async UniTask HideAnimatedAsync()
        {
            if (!_isVisible) return;

            // Disable interaction immediately
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
            }

            // Animate out
            if (_dialogPanel != null)
            {
                _ = Tween.Scale(_dialogPanel, Vector3.one * _scaleFrom, _hideDuration, _hideEase);
            }

            if (_canvasGroup != null)
            {
                _ = Tween.Alpha(_canvasGroup, 0f, _hideDuration);
            }

            if (_backdrop != null)
            {
                _ = Tween.Alpha(_backdrop, 0f, _hideDuration);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(_hideDuration));

            _isVisible = false;

            // Deactivate container
            if (_dialogContainer != null)
            {
                _dialogContainer.SetActive(false);
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = false;
            }
        }

        private void OnConfirmClicked()
        {
            if (!_isVisible) return;

            Debug.Log("[ConfirmDialog] Confirm clicked");

            // Complete the task with true (confirmed)
            _completionSource?.TrySetResult(true);

            // Hide the dialog
            HideAnimatedAsync().Forget();
        }

        private void OnCancelClicked()
        {
            if (!_isVisible) return;

            Debug.Log("[ConfirmDialog] Cancel clicked");

            // Complete the task with false (cancelled)
            _completionSource?.TrySetResult(false);

            // Hide the dialog
            HideAnimatedAsync().Forget();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-find components if not assigned
            if (_dialogContainer == null)
            {
                _dialogContainer = gameObject;
            }

            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }
        }
#endif
    }
}
