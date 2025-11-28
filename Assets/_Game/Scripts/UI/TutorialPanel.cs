using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using LavaQuest.Core;

namespace LavaQuest.UI
{
    public sealed class TutorialPanel : GameStatePanel
    {
        [Header("Tutorial Panel")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Image _playersImage;
        [SerializeField] private TextMeshProUGUI _playersText;
        [SerializeField] private Image _levelsImage;
        [SerializeField] private TextMeshProUGUI _levelsText;
        [SerializeField] private Image _rewardImage;
        [SerializeField] private TextMeshProUGUI _rewardText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _tapToStartText;
        [SerializeField] private float _stepAnimationDelay = 0.3f;

        [Header("Animation Settings")]
        [SerializeField] private float _tapDelay = 2f;
        [SerializeField] private float _imageScaleDuration = 0.4f;
        [SerializeField] private float _textScaleDuration = 0.3f;
        [SerializeField] private float _textAnimationDelay = 0.2f;
        [SerializeField] private float _tapTextFadeDuration = 0.3f;
        [SerializeField] private float _tapTextScaleTarget = 1.1f;
        [SerializeField] private float _tapTextScaleDuration = 0.8f;
        
        [Header("Text Content")]
        [SerializeField] private string _titleString = "Lava Quest";
        [SerializeField] private string _playersFormat = "Start with {0} players!";
        [SerializeField] private string _levelsString = "Beat Levels!";
        [SerializeField] private string _rewardFormat = "Win your share of\n{0} coins!";
        [SerializeField] private string _descriptionFormat = "You will fail the challenge if you fail a level!\nBeat {0} levels to complete the challenge!";
        
        private bool _canTapToStart;
        private Tween _tapTextTween;

        public override void Enter()
        {
            _canTapToStart = false;
            SetupTexts();
            
            base.Enter();
            AnimateSteps();
        }

        public override void Tick()
        {
            if (_canTapToStart && Input.GetMouseButtonDown(0))
            {
                OnTapToStart();
            }
        }

        public override void Exit()
        {
            KillTweens();
            base.Exit();
        }

        private void SetupTexts()
        {
            if (_titleText)
            {
                _titleText.text = _titleString;
            }

            if (_playersText)
            {
                _playersText.text = string.Format(_playersFormat, GameConfig.Config.matchmakingMaxPlayers);
            }

            if (_levelsText)
            {
                _levelsText.text = _levelsString;
            }

            if (_rewardText)
            {
                _rewardText.text = string.Format(_rewardFormat, GameConfig.Config.victoryRewardAmount.ToString("N0"));
            }

            if (_descriptionText)
            {
                _descriptionText.text = string.Format(_descriptionFormat, GameConfig.Config.platformCount);
            }

            if (_tapToStartText)
            {
                _tapToStartText.alpha = 0f;
            }
        }

        private void AnimateSteps()
        {
            float currentDelay = 0f;

            if (_playersImage)
            {
                _playersImage.transform.localScale = Vector3.zero;
                _playersImage.transform.DOScale(1f, _imageScaleDuration).SetDelay(currentDelay).SetEase(Ease.OutBack);
            }

            if (_playersText)
            {
                _playersText.transform.localScale = Vector3.zero;
                _playersText.transform.DOScale(1f, _textScaleDuration).SetDelay(currentDelay + _textAnimationDelay).SetEase(Ease.OutBack);
            }

            currentDelay += _stepAnimationDelay;

            if (_levelsImage)
            {
                _levelsImage.transform.localScale = Vector3.zero;
                _levelsImage.transform.DOScale(1f, _imageScaleDuration).SetDelay(currentDelay).SetEase(Ease.OutBack);
            }

            if (_levelsText)
            {
                _levelsText.transform.localScale = Vector3.zero;
                _levelsText.transform.DOScale(1f, _textScaleDuration).SetDelay(currentDelay + _textAnimationDelay).SetEase(Ease.OutBack);
            }

            currentDelay += _stepAnimationDelay;

            if (_rewardImage)
            {
                _rewardImage.transform.localScale = Vector3.zero;
                _rewardImage.transform.DOScale(1f, _imageScaleDuration).SetDelay(currentDelay).SetEase(Ease.OutBack);
            }

            if (_rewardText)
            {
                _rewardText.transform.localScale = Vector3.zero;
                _rewardText.transform.DOScale(1f, _textScaleDuration).SetDelay(currentDelay + _textAnimationDelay).SetEase(Ease.OutBack);
            }

            DOVirtual.DelayedCall(_tapDelay, ShowTapToStart);
        }

        private void ShowTapToStart()
        {
            if (!_tapToStartText)
            {
                _canTapToStart = true;
                return;
            }

            _tapToStartText.alpha = 0f;
            _tapToStartText.DOFade(1f, _tapTextFadeDuration).OnComplete(() => { _canTapToStart = true; });

            _tapToStartText.transform.localScale = Vector3.one;

            _tapTextTween = _tapToStartText.transform.DOScale(_tapTextScaleTarget, _tapTextScaleDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void OnTapToStart()
        {
            if (!_canTapToStart)
            {
                return;
            }

            _canTapToStart = false;
            
            GameEvents.RaisePlayButtonClick();
            GameEvents.RaiseGameInitializeRequested();
            ChangeState(GameState.Playing);
        }

        private void KillTweens()
        {
            if (_tapTextTween != null && _tapTextTween.IsActive())
            {
                _tapTextTween.Kill();
                _tapTextTween = null;
            }

            if (_tapToStartText)
            {
                _tapToStartText.transform.localScale = Vector3.one;
            }
        }
        
        private void OnDisable()
        {
            KillTweens();
        }
    }
}