using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using LavaQuest.Core;

namespace LavaQuest.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class PlayingPanel : MonoBehaviour
    {
        [Header("Buttons")] 
        [SerializeField] private Button _winButton;
        [SerializeField] private Button _loseButton;

        [Header("Animation")]
        [SerializeField] private float _showDuration = 0.3f;
        [SerializeField] private float _hideDuration = 0.2f;

        private CanvasGroup _canvasGroup;
        private bool _isAnimating;
        private bool _isVisible;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            HideImmediate();
            
            if (_winButton)
            {
                _winButton.onClick.AddListener(OnWinButtonClicked);
            }

            if (_loseButton)
            {
                _loseButton.onClick.AddListener(OnLoseButtonClicked);
            }
        }

        private void OnEnable()
        {
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
            GameEvents.OnAnimationStarted += HandleAnimationStarted;
            GameEvents.OnAnimationEnded += HandleAnimationEnded;
        }

        private void HideImmediate()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
            _isVisible = false;
        }

        private void HandleGameStateChanged(GameState newState)
        {
            bool shouldBeVisible = newState == GameState.Playing || newState == GameState.RoundComplete;
            
            if (shouldBeVisible && !_isVisible)
            {
                ShowPanel();
            }
            else if (!shouldBeVisible && _isVisible)
            {
                HidePanel();
            }
            
            UpdateButtonStates();
        }

        private void HandleAnimationStarted()
        {
            _isAnimating = true;
            UpdateButtonStates();
        }

        private void HandleAnimationEnded()
        {
            _isAnimating = false;
            UpdateButtonStates();
        }

        private void ShowPanel()
        {
            _isVisible = true;
            
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;
            _canvasGroup.DOFade(1f, _showDuration).SetEase(Ease.OutQuad);
        }

        private void HidePanel()
        {
            _isVisible = false;
            
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
            _canvasGroup.DOFade(0f, _hideDuration).SetEase(Ease.InQuad);
        }

        private void OnWinButtonClicked()
        {
            if (_isAnimating)
            {
                return;
            }

            GameEvents.RaisePlayButtonClick();
            GameEvents.RaiseWinRequested();
        }

        private void OnLoseButtonClicked()
        {
            if (_isAnimating)
            {
                return;
            }

            GameEvents.RaisePlayButtonClick();
            GameEvents.RaiseLoseRequested();
        }

        private void UpdateButtonStates()
        {
            bool canInteract = !_isAnimating && _isVisible;

            if (_winButton)
            {
                _winButton.interactable = canInteract;
            }

            if (_loseButton)
            {
                _loseButton.interactable = canInteract;
            }
        }
        
        private void OnDisable()
        {
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
            GameEvents.OnAnimationStarted -= HandleAnimationStarted;
            GameEvents.OnAnimationEnded -= HandleAnimationEnded;
        }

        private void OnDestroy()
        {
            if (_winButton)
            {
                _winButton.onClick.RemoveListener(OnWinButtonClicked);
            }

            if (_loseButton)
            {
                _loseButton.onClick.RemoveListener(OnLoseButtonClicked);
            }
        }
    }
}