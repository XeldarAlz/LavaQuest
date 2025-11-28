using System;
using UnityEngine;

namespace LavaQuest.Core
{
    public static class GameEvents
    {
        public static event Action<GameState> OnGameStateChanged;
        
        public static event Action<int, int> OnRoundComplete;
        public static event Action<int, int> OnPlayerCountChanged;
        public static event Action OnPlayerEliminated;
        public static event Action OnPlayerVictory;
        
        public static event Action OnWinRequested;
        public static event Action OnLoseRequested;
        public static event Action OnResetRequested;
        
        public static event Action<Sprite, Sprite> OnPlayerAvatarDataReady;
        public static event Action<RectTransform> OnAvatarContainerReady;
        
        public static event Action OnPlayButtonClick;
        public static event Action OnPlayJump;
        public static event Action OnPlayVictory;
        public static event Action OnPlayEliminated;
        public static event Action OnPlayPop;
        public static event Action OnPlayMatchmakingComplete;
        public static event Action OnPlayGameStart;
        
        public static event Action OnGameInitializeRequested;
        public static event Action OnGameResetRequested;
        public static event Action<Vector2> OnCameraMoveRequested;
        public static event Action OnCameraResetRequested;
        public static event Action<float> OnCameraSetStartPosition;
        public static event Action<float, Action> OnCameraAnimateToStart;
        public static event Action OnAvatarsSpawnRequested;
        public static event Action OnAvatarsClearRequested;
        public static event Action OnAnimationStarted;
        public static event Action OnAnimationEnded;
        
        public static void RaiseGameStateChanged(GameState newState)
        {
            OnGameStateChanged?.Invoke(newState);
        }
        
        public static void RaiseRoundComplete(int currentLevel, int totalLevels)
        {
            OnRoundComplete?.Invoke(currentLevel, totalLevels);
        }
        
        public static void RaisePlayerCountChanged(int currentPlayers, int totalPlayers)
        {
            OnPlayerCountChanged?.Invoke(currentPlayers, totalPlayers);
        }
        
        public static void RaisePlayerEliminated()
        {
            OnPlayerEliminated?.Invoke();
        }
        
        public static void RaisePlayerVictory()
        {
            OnPlayerVictory?.Invoke();
        }
        
        public static void RaiseWinRequested()
        {
            OnWinRequested?.Invoke();
        }
        
        public static void RaiseLoseRequested()
        {
            OnLoseRequested?.Invoke();
        }
        
        public static void RaiseResetRequested()
        {
            OnResetRequested?.Invoke();
        }
        
        public static void RaisePlayerAvatarDataReady(Sprite iconSprite, Sprite frameSprite)
        {
            OnPlayerAvatarDataReady?.Invoke(iconSprite, frameSprite);
        }
        
        public static void RaiseAvatarContainerReady(RectTransform container)
        {
            OnAvatarContainerReady?.Invoke(container);
        }
        
        
        public static void RaisePlayButtonClick()
        {
            OnPlayButtonClick?.Invoke();
        }
        
        public static void RaisePlayJump()
        {
            OnPlayJump?.Invoke();
        }
        
        public static void RaisePlayVictory()
        {
            OnPlayVictory?.Invoke();
        }
        
        public static void RaisePlayEliminated()
        {
            OnPlayEliminated?.Invoke();
        }
        
        public static void RaisePlayPop()
        {
            OnPlayPop?.Invoke();
        }
        
        public static void RaisePlayMatchmakingComplete()
        {
            OnPlayMatchmakingComplete?.Invoke();
        }
        
        public static void RaisePlayGameStart()
        {
            OnPlayGameStart?.Invoke();
        }
        
        public static void RaiseGameInitializeRequested()
        {
            OnGameInitializeRequested?.Invoke();
        }
        
        public static void RaiseGameResetRequested()
        {
            OnGameResetRequested?.Invoke();
        }
        
        public static void RaiseCameraMoveRequested(Vector2 targetPosition)
        {
            OnCameraMoveRequested?.Invoke(targetPosition);
        }
        
        public static void RaiseCameraResetRequested()
        {
            OnCameraResetRequested?.Invoke();
        }
        
        public static void RaiseCameraSetStartPosition(float offset)
        {
            OnCameraSetStartPosition?.Invoke(offset);
        }
        
        public static void RaiseCameraAnimateToStart(float duration, Action onComplete)
        {
            OnCameraAnimateToStart?.Invoke(duration, onComplete);
        }
        
        public static void RaiseAvatarsSpawnRequested()
        {
            OnAvatarsSpawnRequested?.Invoke();
        }
        
        public static void RaiseAvatarsClearRequested()
        {
            OnAvatarsClearRequested?.Invoke();
        }
        
        public static void RaiseAnimationStarted()
        {
            OnAnimationStarted?.Invoke();
        }
        
        public static void RaiseAnimationEnded()
        {
            OnAnimationEnded?.Invoke();
        }
    }
}
