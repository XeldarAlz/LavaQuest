using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using LavaQuest.Core;

namespace LavaQuest.UI
{
    public sealed class VictoryPanel : GameStatePanel
    {
        [Header("Victory Panel")]
        [SerializeField] private TextMeshProUGUI _congratulationsText;
        [SerializeField] private TextMeshProUGUI _youWinText;
        [SerializeField] private TextMeshProUGUI _rewardAmountText;
        [SerializeField] private TextMeshProUGUI _tapToClaimText;
        [SerializeField] private Image _playerAvatarImage;
        [SerializeField] private Image _playerFrameImage;
        [SerializeField] private ParticleSystem _particleSystem1;
        [SerializeField] private ParticleSystem _particleSystem2;
        
        [Header("Animation Containers")]
        [SerializeField] private Transform _imageContainer;
        [SerializeField] private Transform _rewardContainer;
        [SerializeField] private Transform _playerAvatarContainer;

        [Header("Reward Animation")]
        [SerializeField] private float _stepAnimationDelay = 0.3f;
        [SerializeField] private float _imageScaleDuration = 0.4f;
        [SerializeField] private float _textAnimationDelay = 0.2f;
        [SerializeField] private float _delayAfterRewardCount = 0.5f;
        [SerializeField] private float _rewardScaleTarget = 1.2f;
        [SerializeField] private float _rewardScaleDuration = 0.1f;
        
        [Header("Tap To Claim Animation")]
        [SerializeField] private float _tapTextFadeDuration = 0.3f;
        [SerializeField] private float _tapTextFadeTarget = 0.5f;
        [SerializeField] private float _tapTextFadePulseDuration = 0.5f;
        
        [Header("Text Content")]
        [SerializeField] private string _congratulationsString = "Congratulations!";
        [SerializeField] private string _youWinString = "You win";
        
        private bool _canTapToClaim;
        private Tween _tapTextTween;
        private Sequence _rewardSequence;

        private void OnEnable()
        {
            GameEvents.OnPlayerAvatarDataReady += HandlePlayerAvatarDataReady;
        }

        private void HandlePlayerAvatarDataReady(Sprite iconSprite, Sprite frameSprite)
        {
            SetPlayerAvatar(iconSprite, frameSprite);
        }

        public override void Tick()
        {
            if (_canTapToClaim && Input.GetMouseButtonDown(0))
            {
                OnTapToClaim();
            }
        }

        public override void Enter()
        {
            _canTapToClaim = false;
            GameEvents.RaisePlayVictory();
            SetupDisplay();
            
            base.Enter();
            StartAnimations();
        }

        public override void Exit()
        {
            StopParticles();
            KillAllTweens();
            base.Exit();
        }

        private void PlayParticles()
        {
            if (_particleSystem1)
            {
                _particleSystem1.Play();
            }

            if (_particleSystem2)
            {
                _particleSystem2.Play();
            }
        }

        private void StopParticles()
        {
            if (_particleSystem1)
            {
                _particleSystem1.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            if (_particleSystem2)
            {
                _particleSystem2.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        private void OnTapToClaim()
        {
            if (!_canTapToClaim)
            {
                return;
            }

            _canTapToClaim = false;
            GameEvents.RaisePlayButtonClick();
            ChangeState(GameState.Intro);
        }

        private void SetupDisplay()
        {
            if (_congratulationsText)
            {
                _congratulationsText.text = _congratulationsString;
            }

            if (_youWinText)
            {
                _youWinText.text = _youWinString;
            }

            if (_rewardAmountText)
            {
                _rewardAmountText.text = "0";
            }

            if (_tapToClaimText)
            {
                _tapToClaimText.alpha = 0f;
            }
            
            if (_imageContainer)
            {
                _imageContainer.localScale = Vector3.zero;
            }
            
            if (_rewardContainer)
            {
                _rewardContainer.localScale = Vector3.zero;
            }
            
            if (_playerAvatarContainer)
            {
                _playerAvatarContainer.localScale = Vector3.zero;
            }
        }

        private void StartAnimations()
        {
            float currentDelay = 0f;

            if (_imageContainer)
            {
                _imageContainer.DOScale(1f, _imageScaleDuration).SetDelay(currentDelay).SetEase(Ease.OutBack);
            }

            currentDelay += _stepAnimationDelay;

            if (_rewardContainer)
            {
                _rewardContainer.DOScale(1f, _imageScaleDuration).SetDelay(currentDelay).SetEase(Ease.OutBack);
            }

            DOVirtual.DelayedCall(currentDelay + _imageScaleDuration, PlayParticles);

            currentDelay += _stepAnimationDelay;

            if (_playerAvatarContainer)
            {
                _playerAvatarContainer.DOScale(1f, _imageScaleDuration).SetDelay(currentDelay).SetEase(Ease.OutBack);
            }

            DOVirtual.DelayedCall(currentDelay + _textAnimationDelay, AnimateRewardCount);

            float totalDelay = currentDelay + _textAnimationDelay + GameConfig.Config.victoryRewardCountDuration + _delayAfterRewardCount;
            DOVirtual.DelayedCall(totalDelay, () =>
            {
                _canTapToClaim = true;
                ShowTapToClaim();
            });
        }

        private void AnimateRewardCount()
        {
            if (!_rewardAmountText)
            {
                return;
            }

            int currentValue = 0;

            _rewardSequence = DOTween.Sequence();
            _rewardSequence.Append(DOTween.To(() => currentValue, x =>
                {
                    currentValue = x;
                    _rewardAmountText.text = currentValue.ToString("N0");
                }, GameConfig.Config.victoryRewardAmount, GameConfig.Config.victoryRewardCountDuration)
                .SetEase(Ease.OutQuad));

            _rewardSequence.Join(_rewardAmountText.transform.DOScale(_rewardScaleTarget, _rewardScaleDuration)
                .SetLoops((int)(GameConfig.Config.victoryRewardCountDuration / _rewardScaleDuration), LoopType.Yoyo));
        }

        private void ShowTapToClaim()
        {
            if (!_tapToClaimText)
            {
                return;
            }

            _tapToClaimText.DOFade(1f, _tapTextFadeDuration);

            _tapTextTween = _tapToClaimText.DOFade(_tapTextFadeTarget, _tapTextFadePulseDuration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        }

        private void SetPlayerAvatar(Sprite iconSprite, Sprite frameSprite)
        {
            if (_playerAvatarImage && iconSprite)
            {
                _playerAvatarImage.sprite = iconSprite;
            }

            if (_playerFrameImage && frameSprite)
            {
                _playerFrameImage.sprite = frameSprite;
            }
        }

        private void KillAllTweens()
        {
            if (_rewardSequence != null && _rewardSequence.IsActive())
            {
                _rewardSequence.Kill();
                _rewardSequence = null;
            }

            if (_tapTextTween == null || !_tapTextTween.IsActive())
            {
                return;
            }
            
            _tapTextTween.Kill();
            _tapTextTween = null;
        }
        
        private void OnDisable()
        {
            GameEvents.OnPlayerAvatarDataReady -= HandlePlayerAvatarDataReady;
            KillAllTweens();
        }
    }
}