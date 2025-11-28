using UnityEngine;
using DG.Tweening;
using System;
using LavaQuest.Core;
using LavaQuest.Data;

namespace LavaQuest.Controllers
{
    public sealed class CameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform _gameContainer;
        
        private Tween _cameraTween;
        
        private void OnEnable()
        {
            GameEvents.OnCameraMoveRequested += HandleCameraMoveRequested;
            GameEvents.OnCameraResetRequested += HandleCameraReset;
            GameEvents.OnCameraSetStartPosition += HandleSetStartPosition;
            GameEvents.OnCameraAnimateToStart += HandleAnimateToStart;
            GameEvents.OnGameResetRequested += HandleGameReset;
        }
        
        private void HandleCameraMoveRequested(Vector2 targetPosition)
        {
            MoveCameraToFollow(targetPosition);
        }
        
        private void HandleCameraReset()
        {
            ResetCameraPosition();
        }
        
        private void HandleSetStartPosition(float offset)
        {
            _gameContainer.anchoredPosition = new Vector2(0f, offset);
        }
        
        private void HandleAnimateToStart(float duration, Action onComplete)
        {
            KillCurrentTween();
            
            _cameraTween = _gameContainer.DOAnchorPosY(0f, duration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => onComplete?.Invoke());
        }
        
        private void HandleGameReset()
        {
            KillCurrentTween();
            ResetCameraPosition();
        }
        
        private void MoveCameraToFollow(Vector2 targetPosition)
        {
            KillCurrentTween();
            
            LavaQuestConfig config = GameConfig.Config;
            
            float targetY = -(targetPosition.y - config.cameraVerticalOffset);
            
            _cameraTween = _gameContainer.DOAnchorPosY(targetY, config.cameraFollowDuration)
                .SetEase(Ease.OutQuad);
        }
        
        private void ResetCameraPosition()
        {
            _gameContainer.anchoredPosition = Vector2.zero;
        }
        
        private void KillCurrentTween()
        {
            if (_cameraTween != null && _cameraTween.IsActive())
            {
                _cameraTween.Kill();
            }
        }
        
        private void OnDisable()
        {
            GameEvents.OnCameraMoveRequested -= HandleCameraMoveRequested;
            GameEvents.OnCameraResetRequested -= HandleCameraReset;
            GameEvents.OnCameraSetStartPosition -= HandleSetStartPosition;
            GameEvents.OnCameraAnimateToStart -= HandleAnimateToStart;
            GameEvents.OnGameResetRequested -= HandleGameReset;
        }
        
        private void OnDestroy()
        {
            KillCurrentTween();
        }
    }
}