using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using LavaQuest.Core;

namespace LavaQuest.UI
{
    public sealed class IntroPanel : GameStatePanel
    {
        [Header("Intro Panel")]
        [SerializeField] private Button _startButton;
        [SerializeField] private TextMeshProUGUI _challengeText;
        
        [Header("Animation")]
        [SerializeField] private float _buttonScaleTarget = 1.1f;
        [SerializeField] private float _buttonScaleDuration = 0.8f;
        
        [Header("Text Content")]
        [SerializeField] private string _challengeFormat = "Beat {0} levels to complete the challenge!";
        
        private Tween _buttonScaleTween;
        
        protected override void Awake()
        {
            base.Awake();
            
            if (_startButton)
            {
                _startButton.onClick.AddListener(OnStartButtonClicked);
            }
        }
        
        private void OnDestroy()
        {
            if (_startButton)
            {
                _startButton.onClick.RemoveListener(OnStartButtonClicked);
            }
        }
        
        private void UpdateChallengeText()
        {
            if (_challengeText)
            {
                _challengeText.text = string.Format(_challengeFormat, GameConfig.Config.platformCount);
            }
        }
        
        public override void Enter()
        {
            GameEvents.RaiseGameResetRequested();
            
            UpdateChallengeText();
            
            base.Enter();
            
            StartButtonIdleAnimation();
        }
        
        public override void Exit()
        {
            StopButtonIdleAnimation();
            base.Exit();
        }
        
        private void OnStartButtonClicked()
        {
            GameEvents.RaisePlayButtonClick();
            ChangeState(GameState.Matchmaking);
        }
        
        private void StartButtonIdleAnimation()
        {
            if (!_startButton)
            {
                return;
            }
            
            StopButtonIdleAnimation();
            
            _startButton.transform.localScale = Vector3.one;
            _buttonScaleTween = _startButton.transform.DOScale(_buttonScaleTarget, _buttonScaleDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
        
        private void StopButtonIdleAnimation()
        {
            if (_buttonScaleTween != null && _buttonScaleTween.IsActive())
            {
                _buttonScaleTween.Kill();
            }
            
            if (_startButton)
            {
                _startButton.transform.localScale = Vector3.one;
            }
        }
    }
}