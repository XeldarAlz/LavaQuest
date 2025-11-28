using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using LavaQuest.Core;

namespace LavaQuest.UI
{
    public sealed class EliminatedPanel : GameStatePanel
    {
        [Header("Eliminated Panel")]
        [SerializeField] private Button _closeButton;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _messageText;

        [Header("Text Content")]
        [SerializeField] private string _titleString = "Lava Quest";
        [SerializeField] private string _messageString = "You failed the challenge.\nBetter luck next time!";
        
        [Header("Animation")]
        [SerializeField] private float _initialScale = 0.8f;

        private bool _canClose;

        protected override void Awake()
        {
            base.Awake();
            
            if (_closeButton)
            {
                _closeButton.onClick.AddListener(OnCloseButtonClicked);
            }
        }

        public override void Tick()
        {
            if (_canClose && Input.GetMouseButtonDown(0))
            {
                OnCloseButtonClicked();
            }
        }

        public override void Enter()
        {
            _canClose = false;
            GameEvents.RaisePlayEliminated();
            SetupDisplay();
            ShowPanelWithCallback();
        }

        public override void Exit()
        {
            _canClose = false;
            base.Exit();
        }

        private void OnCloseButtonClicked()
        {
            if (!_canClose)
            {
                return;
            }

            _canClose = false;
            GameEvents.RaisePlayButtonClick();
            ChangeState(GameState.Intro);
        }

        private void SetupDisplay()
        {
            if (_titleText)
            {
                _titleText.text = _titleString;
            }

            if (_messageText)
            {
                _messageText.text = _messageString;
            }
        }

        private void ShowPanelWithCallback()
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, showDuration).SetEase(Ease.OutQuad);

            if (panelRoot)
            {
                panelRoot.localScale = Vector3.one * _initialScale;
                panelRoot.DOScale(1f, showDuration).SetEase(Ease.OutBack).OnComplete(() => _canClose = true);
            }
            else
            {
                _canClose = true;
            }
        }
        
        private void OnDestroy()
        {
            if (_closeButton)
            {
                _closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            }
        }
    }
}