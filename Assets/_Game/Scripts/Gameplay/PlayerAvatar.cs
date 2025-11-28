using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using LavaQuest.Core;
using LavaQuest.Data;

namespace LavaQuest.Gameplay
{
    public sealed class PlayerAvatar : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private Image _avatarImage;
        [SerializeField] private Image _frameImage;
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private RectTransform _visualContainer;

        private int _currentPlatformIndex;
        private bool _isEliminated;
        private bool _isPlayer;
        private Sequence _currentSequence;
        private Sequence _idleSequence;
        private AvatarAnimationConfig _animationConfig;

        public int CurrentPlatformIndex => _currentPlatformIndex;
        public bool IsEliminated => _isEliminated;
        public bool IsPlayer => _isPlayer;

        public void Initialize(int startPlatformIndex, bool isPlayer, Sprite avatarSprite, Sprite frameSprite)
        {
            _currentPlatformIndex = startPlatformIndex;
            _isPlayer = isPlayer;
            _isEliminated = false;
            _animationConfig = GameConfig.Config.avatarAnimationConfig;

            if (_avatarImage && avatarSprite)
            {
                _avatarImage.sprite = avatarSprite;
            }

            if (_frameImage && frameSprite)
            {
                _frameImage.sprite = frameSprite;
            }

            ResetVisuals();
        }

        private void ResetVisuals()
        {
            _rectTransform.localScale = Vector3.one;
            _rectTransform.localRotation = Quaternion.identity;

            if (_visualContainer)
            {
                _visualContainer.localScale = Vector3.one;
            }

            if (_avatarImage)
            {
                Color color = _avatarImage.color;
                color.a = 1f;
                _avatarImage.color = color;
            }

            if (_frameImage)
            {
                Color color = _frameImage.color;
                color.a = 1f;
                _frameImage.color = color;
            }
        }

        public void JumpToPlatform(Vector2 targetPosition, float jumpHeight, float duration, 
            System.Action onComplete = null)
        {
            KillCurrentAnimation();
            StopIdleBounce();
            
            GameEvents.RaisePlayJump();
            Vector2 startPosition = _rectTransform.anchoredPosition;
            RectTransform scaleTarget = GetScaleTarget();
            _currentSequence = DOTween.Sequence();
            
            AddAnticipationSquash(scaleTarget);
            AddJumpArc(startPosition, targetPosition, jumpHeight, duration, scaleTarget);
            AddLandingSquash(duration, scaleTarget);
            
            _currentSequence.OnComplete(() => OnJumpComplete(onComplete));
        }

        private void AddAnticipationSquash(RectTransform scaleTarget)
        {
            _currentSequence.Append(scaleTarget.DOScaleY(_animationConfig.squashAmount, _animationConfig.anticipationDuration)
                .SetEase(Ease.OutQuad));
            _currentSequence.Join(scaleTarget.DOScaleX(1f / _animationConfig.squashAmount, _animationConfig.anticipationDuration)
                .SetEase(Ease.OutQuad));
        }

        private void AddJumpArc(Vector2 startPosition, Vector2 targetPosition, float jumpHeight, float duration,
            RectTransform scaleTarget)
        {
            float peakY = Mathf.Max(startPosition.y, targetPosition.y) + jumpHeight;
            float jumpUpDuration = duration * _animationConfig.jumpUpRatio;
            float jumpDownDuration = duration * _animationConfig.jumpDownRatio;

            _currentSequence.Append(_rectTransform.DOAnchorPosY(peakY, jumpUpDuration).SetEase(Ease.OutQuad));
            _currentSequence.Join(_rectTransform.DOAnchorPosX(targetPosition.x, duration).SetEase(Ease.Linear));

            AddJumpStretch(scaleTarget, jumpUpDuration);

            _currentSequence.Append(
                _rectTransform.DOAnchorPosY(targetPosition.y, jumpDownDuration).SetEase(Ease.InQuad));
        }

        private void AddJumpStretch(RectTransform scaleTarget, float jumpUpDuration)
        {
            float stretchDuration = jumpUpDuration * _animationConfig.jumpStretchRatio;
            
            _currentSequence.Join(scaleTarget.DOScaleY(_animationConfig.stretchAmount, stretchDuration).SetEase(Ease.OutQuad));
            _currentSequence.Join(
                scaleTarget.DOScaleX(1f / _animationConfig.stretchAmount, stretchDuration).SetEase(Ease.OutQuad));
            _currentSequence.Insert(_animationConfig.anticipationDuration + stretchDuration,
                scaleTarget.DOScale(1f, stretchDuration).SetEase(Ease.InOutQuad));
        }

        private void AddLandingSquash(float duration, RectTransform scaleTarget)
        {
            float jumpUpDuration = duration * _animationConfig.jumpUpRatio;
            float jumpDownDuration = duration * _animationConfig.jumpDownRatio;
            float landStartTime = _animationConfig.anticipationDuration + jumpUpDuration +
                                  (jumpDownDuration * _animationConfig.jumpLandStartRatio);
            float landSquashDuration = jumpDownDuration * (1f - _animationConfig.jumpLandStartRatio);
            
            _currentSequence.Insert(landStartTime,
                scaleTarget.DOScaleY(_animationConfig.squashAmount, landSquashDuration).SetEase(Ease.InQuad));
            _currentSequence.Insert(landStartTime,
                scaleTarget.DOScaleX(1f / _animationConfig.squashAmount, landSquashDuration).SetEase(Ease.InQuad));
            _currentSequence.Append(scaleTarget.DOScale(1f, _animationConfig.landSquashDuration).SetEase(Ease.OutBack, 2f));
        }

        private void OnJumpComplete(System.Action onComplete)
        {
            _currentPlatformIndex++;
            PlayIdleBounce();
            onComplete?.Invoke();
        }

        public void FallDown(float fallDistance, float duration, System.Action onComplete = null)
        {
            if (_isEliminated)
            {
                return;
            }

            _isEliminated = true;
            
            KillCurrentAnimation();
            StopIdleBounce();
            
            Vector2 startPosition = _rectTransform.anchoredPosition;
            Vector2 fallTarget = new(startPosition.x, startPosition.y - fallDistance);
            RectTransform scaleTarget = GetScaleTarget();
            _currentSequence = DOTween.Sequence();
            
            AddFallShake(scaleTarget);
            AddFallPreparation(startPosition, scaleTarget);
            AddFallMotion(startPosition, fallTarget, duration, scaleTarget);
            AddFallFade(duration);
            
            _currentSequence.OnComplete(() => onComplete?.Invoke());
        }

        private void AddFallShake(RectTransform scaleTarget)
        {
            _currentSequence.Append(_rectTransform.DOShakeAnchorPos(_animationConfig.fallShakeDuration, 
                _animationConfig.fallShakeIntensity, _animationConfig.shakeVibrato, _animationConfig.shakeRandomness, 
                _animationConfig.shakeSnapping, _animationConfig.shakeFadeOut));
            _currentSequence.Join(scaleTarget.DOScaleY(_animationConfig.fallPrepSquash, 
                    _animationConfig.fallPrepDuration).SetEase(Ease.OutQuad));
        }

        private void AddFallPreparation(Vector2 startPosition, RectTransform scaleTarget)
        {
            _currentSequence.Append(_rectTransform.DOAnchorPosY(startPosition.y + _animationConfig.fallPrepHeight,
                    _animationConfig.fallPrepDuration)
                .SetEase(Ease.OutQuad));
            _currentSequence.Join(scaleTarget.DOScaleY(_animationConfig.stretchAmount, _animationConfig.fallPrepDuration)
                .SetEase(Ease.OutQuad));
        }

        private void AddFallMotion(Vector2 startPosition, Vector2 fallTarget, float duration, RectTransform scaleTarget)
        {
            float wobbleX = Random.Range(-_animationConfig.fallWobbleRange, _animationConfig.fallWobbleRange);
            
            _currentSequence.Append(_rectTransform.DOAnchorPos(fallTarget, duration).SetEase(Ease.InQuad));

            _currentSequence.Join(_rectTransform.DOAnchorPosX(startPosition.x + wobbleX, duration * 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(2, LoopType.Yoyo));

            _currentSequence.Join(_rectTransform.DORotate(
                    new Vector3(0, 0, Random.Range(-360f, 360f)), duration, RotateMode.FastBeyond360)
                .SetEase(Ease.InQuad));

            _currentSequence.Join(scaleTarget.DOScale(_animationConfig.fallScaleEnd, duration).SetEase(Ease.InQuad));
        }

        private void AddFallFade(float duration)
        {
            float fadeStartTime = _animationConfig.fallFadeDelay + (duration * 0.5f);
            float fadeDuration = duration * 0.5f;
            
            if (_avatarImage)
            {
                _currentSequence.Insert(fadeStartTime, _avatarImage.DOFade(0f, fadeDuration).SetEase(Ease.InQuad));
            }

            if (_frameImage)
            {
                _currentSequence.Insert(fadeStartTime, _frameImage.DOFade(0f, fadeDuration).SetEase(Ease.InQuad));
            }
        }

        public void PlayIdleBounce()
        {
            StopIdleBounce();
            
            RectTransform scaleTarget = GetScaleTarget();
            float bounceOffset = Random.Range(_animationConfig.idleBounceMin, _animationConfig.idleBounceMax);
            float bounceDuration = Random.Range(_animationConfig.idleDurationMin, _animationConfig.idleDurationMax);
            float currentY = _rectTransform.anchoredPosition.y;
            
            _idleSequence = DOTween.Sequence();

            _idleSequence.Append(_rectTransform.DOAnchorPosY(currentY + bounceOffset, bounceDuration)
                .SetEase(Ease.InOutSine));
            _idleSequence.Join(scaleTarget.DOScaleY(_animationConfig.idleScaleAmount, bounceDuration).SetEase(Ease.InOutSine));
            
            _idleSequence.Append(_rectTransform.DOAnchorPosY(currentY, bounceDuration).SetEase(Ease.InOutSine));
            _idleSequence.Join(scaleTarget.DOScaleY(1f, bounceDuration).SetEase(Ease.InOutSine));
            _idleSequence.SetLoops(-1);
        }


        public void SetPosition(Vector2 position)
        {
            _rectTransform.anchoredPosition = position;
        }

        private RectTransform GetScaleTarget()
        {
            return _visualContainer ? _visualContainer : _rectTransform;
        }

        private void StopIdleBounce()
        {
            if (_idleSequence == null || !_idleSequence.IsActive())
            {
                return;
            }

            _idleSequence.Kill();
            _idleSequence = null;
        }

        private void KillCurrentAnimation()
        {
            if (_currentSequence == null || !_currentSequence.IsActive())
            {
                return;
            }

            _currentSequence.Kill();
            _currentSequence = null;
        }

        private void OnDestroy()
        {
            KillCurrentAnimation();
            StopIdleBounce();
        }
    }
}