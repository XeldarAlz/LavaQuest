using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using LavaQuest.Core;

namespace LavaQuest.UI
{
    public sealed class MatchmakingPanel : GameStatePanel
    {
        [Header("Matchmaking Panel")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _findingPlayersText;
        [SerializeField] private TextMeshProUGUI _playerCountText;
        [SerializeField] private TextMeshProUGUI _tapToContinueText;
        [SerializeField] private RectTransform _avatarContainer;
        [SerializeField] private List<Image> _avatarSlots = new();
        [SerializeField] private ParticleSystem _particleSystem1;
        [SerializeField] private ParticleSystem _particleSystem2;

        [Header("Count Animation Timing")]
        [SerializeField] private float _part1DurationPercent = 0.3f;
        [SerializeField] private float _part2DurationPercent = 0.3f;
        [SerializeField] private float _part3DurationPercent = 0.2f;
        [SerializeField] private Vector2 _pause1DurationRange = new(0.08f, 0.15f);
        [SerializeField] private Vector2 _pause2DurationRange = new(0.08f, 0.15f);
        [SerializeField] private Vector2 _part1TargetRange = new(0.25f, 0.4f);
        [SerializeField] private Vector2 _part2TargetRange = new(0.6f, 0.85f);
        
        [Header("Avatar Animation")]
        [SerializeField] private float _avatarPart1Percent = 0.35f;
        [SerializeField] private float _avatarPart2Percent = 0.40f;
        [SerializeField] private float _avatarAppearDuration = 0.4f;
        [SerializeField] private Vector2 _avatarRotationRange = new(-12f, 12f);
        [SerializeField] private float _avatarOutBackOvershoot = 1.5f;
        [SerializeField] private Vector2 _avatarAdditionalRotationRange = new(-5f, 5f);
        [SerializeField] private float _avatarFadeDurationMultiplier = 0.5f;
        
        [Header("Avatar Idle Animation")]
        [SerializeField] private Vector2 _idleRotationAmountRange = new(4f, 8f);
        [SerializeField] private Vector2 _idleRotationDurationRange = new(1.2f, 2.0f);
        [SerializeField] private Vector2 _idleRotationDelayRange = new(0f, 0.5f);
        [SerializeField] private Vector2 _idleScaleAmountRange = new(1.03f, 1.07f);
        [SerializeField] private Vector2 _idleScaleDurationRange = new(0.8f, 1.4f);
        [SerializeField] private Vector2 _idleScaleDelayRange = new(0f, 0.3f);
        
        [Header("Tap To Continue Animation")]
        [SerializeField] private float _tapTextFadeDuration = 0.3f;
        [SerializeField] private float _tapTextScaleTarget = 1.1f;
        [SerializeField] private float _tapTextScaleDuration = 0.8f;
        
        [Header("Text Content")]
        [SerializeField] private string _playerCountFormat = "{0}/{1}";
        
        private float _part1Duration;
        private float _pause1Duration;
        private float _part2Duration;
        private float _pause2Duration;
        private float _part3Duration;
        
        private bool _isMatchmakingComplete;
        private bool _canTapToContinue;
        private Sequence _countSequence;
        private Sequence _avatarSequence;
        private Tween _tapTextTween;
        private int _lastPopSoundCount;
        private readonly List<Tween> _avatarIdleTweens = new();

        public override void Enter()
        {
            _isMatchmakingComplete = false;
            _canTapToContinue = false;
            ResetDisplay();
            PlayParticles();
            base.Enter();
            StartMatchmaking();
        }
        
        public override void Tick()
        {
            if (_canTapToContinue && Input.GetMouseButtonDown(0))
            {
                OnTapToContinue();
            }
        }

        public override void Exit()
        {
            KillAllTweens();
            StopParticles();
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

        private void ResetDisplay()
        {
            if (_playerCountText)
            {
                _playerCountText.text = string.Format(_playerCountFormat, 0, GameConfig.Config.matchmakingMaxPlayers);
            }

            if (_tapToContinueText)
            {
                _tapToContinueText.alpha = 0f;
                _tapToContinueText.gameObject.SetActive(false);
            }

            for (int slotIndex = 0; slotIndex < _avatarSlots.Count; slotIndex++)
            {
                if (!_avatarSlots[slotIndex])
                {
                    continue;
                }
                
                _avatarSlots[slotIndex].transform.localScale = Vector3.zero;
                _avatarSlots[slotIndex].transform.localRotation = Quaternion.identity;
                Color avatarColor = _avatarSlots[slotIndex].color;
                avatarColor.a = 0f;
                _avatarSlots[slotIndex].color = avatarColor;
            }
        }

        private void StartMatchmaking()
        {
            if (GameConfig.Config.matchmakingStartDelay > 0f)
            {
                DOVirtual.DelayedCall(GameConfig.Config.matchmakingStartDelay, () =>
                {
                    AnimatePlayerCount();
                    AnimateAvatars();
                });
            }
            else
            {
                AnimatePlayerCount();
                AnimateAvatars();
            }
        }

        private void AnimatePlayerCount()
        {
            _countSequence = DOTween.Sequence();

            int currentCount = 0;
            _lastPopSoundCount = 0;
            int popSoundInterval = Mathf.Max(1, GameConfig.Config.matchmakingMaxPlayers / GameConfig.Config.matchmakingPopSoundInterval);

            int part1Target = Random.Range(Mathf.FloorToInt(GameConfig.Config.matchmakingMaxPlayers * _part1TargetRange.x), 
                Mathf.FloorToInt(GameConfig.Config.matchmakingMaxPlayers * _part1TargetRange.y));
            int part2Target = Random.Range(Mathf.FloorToInt(GameConfig.Config.matchmakingMaxPlayers * _part2TargetRange.x), 
                Mathf.FloorToInt(GameConfig.Config.matchmakingMaxPlayers * _part2TargetRange.y));

            _part1Duration = GameConfig.Config.matchmakingDuration * _part1DurationPercent;
            _pause1Duration = GameConfig.Config.matchmakingDuration * Random.Range(_pause1DurationRange.x, _pause1DurationRange.y);
            _part2Duration = GameConfig.Config.matchmakingDuration * _part2DurationPercent;
            _pause2Duration = GameConfig.Config.matchmakingDuration * Random.Range(_pause2DurationRange.x, _pause2DurationRange.y);
            _part3Duration = GameConfig.Config.matchmakingDuration * _part3DurationPercent;

            _countSequence.Append(DOTween.To(() => currentCount, x =>
                {
                    currentCount = x;
                    UpdatePlayerCount(x, popSoundInterval);
                }, part1Target, _part1Duration)
                .SetEase(Ease.OutQuad));

            _countSequence.AppendInterval(_pause1Duration);

            _countSequence.Append(DOTween.To(() => currentCount, x =>
                {
                    currentCount = x;
                    UpdatePlayerCount(x, popSoundInterval);
                }, part2Target, _part2Duration)
                .SetEase(Ease.Linear));

            _countSequence.AppendInterval(_pause2Duration);

            _countSequence.Append(DOTween.To(() => currentCount, x =>
                {
                    currentCount = x;
                    UpdatePlayerCount(x, popSoundInterval);
                }, GameConfig.Config.matchmakingMaxPlayers, _part3Duration)
                .SetEase(Ease.OutSine));

            _countSequence.OnComplete(OnMatchmakingComplete);
        }

        private void UpdatePlayerCount(int count, int popSoundInterval)
        {
            if (_playerCountText)
            {
                _playerCountText.text = string.Format(_playerCountFormat, count, GameConfig.Config.matchmakingMaxPlayers);
            }

            if (count - _lastPopSoundCount < popSoundInterval)
            {
                return;
            }
            
            _lastPopSoundCount = count;
            GameEvents.RaisePlayPop();
        }

        private void AnimateAvatars()
        {
            _avatarSequence = DOTween.Sequence();

            int totalAvatars = 0;
            
            for (int slotIndex = 0; slotIndex < _avatarSlots.Count; slotIndex++)
            {
                if (_avatarSlots[slotIndex])
                {
                    totalAvatars++;
                }
            }

            if (totalAvatars == 0)
            {
                return;
            }

            int part1Avatars = Mathf.CeilToInt(totalAvatars * _avatarPart1Percent);
            int part2Avatars = Mathf.CeilToInt(totalAvatars * _avatarPart2Percent);

            float part1Interval = (_part1Duration - _avatarAppearDuration) / Mathf.Max(1, part1Avatars);
            float part2Interval = (_part2Duration - _avatarAppearDuration) / Mathf.Max(1, part2Avatars);
            float part3Interval = (_part3Duration - _avatarAppearDuration) / Mathf.Max(1, totalAvatars - part1Avatars - part2Avatars);

            float currentTime = 0f;
            int avatarIndex = 0;

            for (int partAvatarIndex = 0; partAvatarIndex < part1Avatars && avatarIndex < _avatarSlots.Count; partAvatarIndex++)
            {
                while (avatarIndex < _avatarSlots.Count && !_avatarSlots[avatarIndex])
                {
                    avatarIndex++;
                }

                if (avatarIndex >= _avatarSlots.Count)
                {
                    break;
                }

                AnimateSingleAvatar(_avatarSlots[avatarIndex], currentTime, _avatarAppearDuration);
                currentTime += part1Interval;
                avatarIndex++;
            }

            currentTime = _part1Duration + _pause1Duration;

            for (int partAvatarIndex = 0; partAvatarIndex < part2Avatars && avatarIndex < _avatarSlots.Count; partAvatarIndex++)
            {
                while (avatarIndex < _avatarSlots.Count && !_avatarSlots[avatarIndex])
                {
                    avatarIndex++;
                }

                if (avatarIndex >= _avatarSlots.Count)
                {
                    break;
                }

                AnimateSingleAvatar(_avatarSlots[avatarIndex], currentTime, _avatarAppearDuration);
                currentTime += part2Interval;
                avatarIndex++;
            }

            currentTime = _part1Duration + _pause1Duration + _part2Duration + _pause2Duration;

            while (avatarIndex < _avatarSlots.Count)
            {
                while (avatarIndex < _avatarSlots.Count && !_avatarSlots[avatarIndex])
                {
                    avatarIndex++;
                }

                if (avatarIndex >= _avatarSlots.Count)
                {
                    break;
                }

                AnimateSingleAvatar(_avatarSlots[avatarIndex], currentTime, _avatarAppearDuration);
                currentTime += part3Interval;
                avatarIndex++;
            }
        }

        private void AnimateSingleAvatar(Image avatarSlot, float delay, float duration)
        {
            float randomRotation = Random.Range(_avatarRotationRange.x, _avatarRotationRange.y);
            avatarSlot.transform.localRotation = Quaternion.Euler(0f, 0f, randomRotation);

            _avatarSequence.Insert(delay, avatarSlot.transform
                .DOScale(1f, duration)
                .SetEase(Ease.OutBack, _avatarOutBackOvershoot)
                .OnComplete(() => StartAvatarIdleAnimation(avatarSlot)));

            _avatarSequence.Insert(delay, avatarSlot.transform
                .DOLocalRotate(new Vector3(0f, 0f, randomRotation + Random.Range(_avatarAdditionalRotationRange.x, _avatarAdditionalRotationRange.y)), duration)
                .SetEase(Ease.OutQuad));

            _avatarSequence.Insert(delay, avatarSlot
                .DOFade(1f, duration * _avatarFadeDurationMultiplier)
                .From(0f)
                .SetEase(Ease.OutQuad));
        }

        private void StartAvatarIdleAnimation(Image avatarSlot)
        {
            if (!avatarSlot)
            {
                return;
            }

            float baseRotation = avatarSlot.transform.localEulerAngles.z;
            if (baseRotation > 180f)
            {
                baseRotation -= 360f;
            }

            float rotationAmount = Random.Range(_idleRotationAmountRange.x, _idleRotationAmountRange.y);
            float rotationDuration = Random.Range(_idleRotationDurationRange.x, _idleRotationDurationRange.y);
            float rotationDelay = Random.Range(_idleRotationDelayRange.x, _idleRotationDelayRange.y);

            Tween rotationTween = avatarSlot.transform
                .DOLocalRotate(new Vector3(0f, 0f, baseRotation + rotationAmount), rotationDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(rotationDelay);
            _avatarIdleTweens.Add(rotationTween);

            float scaleAmount = Random.Range(_idleScaleAmountRange.x, _idleScaleAmountRange.y);
            float scaleDuration = Random.Range(_idleScaleDurationRange.x, _idleScaleDurationRange.y);
            float scaleDelay = Random.Range(_idleScaleDelayRange.x, _idleScaleDelayRange.y);

            Tween scaleTween = avatarSlot.transform
                .DOScale(scaleAmount, scaleDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(scaleDelay);
            _avatarIdleTweens.Add(scaleTween);
        }

        private void OnMatchmakingComplete()
        {
            _isMatchmakingComplete = true;
            GameEvents.RaisePlayMatchmakingComplete();

            DOVirtual.DelayedCall(GameConfig.Config.matchmakingDelayBeforeStart, () =>
            {
                _canTapToContinue = true;
                ShowTapToContinue();
            });
        }

        private void ShowTapToContinue()
        {
            if (!_tapToContinueText)
            {
                return;
            }

            _tapToContinueText.gameObject.SetActive(true);
            _tapToContinueText.alpha = 0f;
            _tapToContinueText.DOFade(1f, _tapTextFadeDuration);
            _tapToContinueText.transform.localScale = Vector3.one;

            _tapTextTween = _tapToContinueText.transform.DOScale(_tapTextScaleTarget, _tapTextScaleDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void OnTapToContinue()
        {
            if (!_isMatchmakingComplete || !_canTapToContinue)
            {
                return;
            }

            _canTapToContinue = false;
            GameEvents.RaisePlayGameStart();
            ChangeState(GameState.Tutorial);
        }

        private void KillAllTweens()
        {
            if (_countSequence != null && _countSequence.IsActive())
            {
                _countSequence.Kill();
                _countSequence = null;
            }

            if (_avatarSequence != null && _avatarSequence.IsActive())
            {
                _avatarSequence.Kill();
                _avatarSequence = null;
            }

            if (_tapTextTween != null && _tapTextTween.IsActive())
            {
                _tapTextTween.Kill();
                _tapTextTween = null;
            }

            KillAvatarIdleTweens();

            if (_tapToContinueText)
            {
                _tapToContinueText.transform.localScale = Vector3.one;
            }
        }

        private void KillAvatarIdleTweens()
        {
            for (int tweenIndex = 0; tweenIndex < _avatarIdleTweens.Count; tweenIndex++)
            {
                if (_avatarIdleTweens[tweenIndex] != null && _avatarIdleTweens[tweenIndex].IsActive())
                {
                    _avatarIdleTweens[tweenIndex].Kill();
                }
            }
            
            _avatarIdleTweens.Clear();
        }

        private void OnDisable()
        {
            KillAllTweens();
        }
    }
}
