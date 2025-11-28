using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using LavaQuest.Core;
using LavaQuest.Data;
using LavaQuest.Gameplay;

namespace LavaQuest.Controllers
{
    public sealed class RoundController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AvatarController _avatarController;
        
        private int _currentRound;
        private bool _isAnimating;

        private void OnEnable()
        {
            GameEvents.OnWinRequested += HandleWinRequested;
            GameEvents.OnLoseRequested += HandleLoseRequested;
            GameEvents.OnResetRequested += HandleResetRequested;
            GameEvents.OnGameResetRequested += HandleGameReset;
            GameEvents.OnAnimationStarted += HandleAnimationStarted;
            GameEvents.OnAnimationEnded += HandleAnimationEnded;
        }
        
        private void HandleWinRequested()
        {
            Win();
        }
        
        private void HandleLoseRequested()
        {
            Lose();
        }
        
        private void HandleResetRequested()
        {
            GameEvents.RaiseGameStateChanged(GameState.Intro);
        }
        
        private void HandleGameReset()
        {
            _currentRound = 0;
            _isAnimating = false;
        }
        
        private void HandleAnimationStarted()
        {
            _isAnimating = true;
        }
        
        private void HandleAnimationEnded()
        {
            _isAnimating = false;
        }
        
        private void Win()
        {
            if (_isAnimating)
            {
                return;
            }
            
            PlayerAvatar playerAvatar = _avatarController.PlayerAvatar;
            
            if (!playerAvatar || playerAvatar.IsEliminated)
            {
                return;
            }
            
            int currentPlatformIndex = playerAvatar.CurrentPlatformIndex;
            List<PlatformAnchor> platformAnchors = _avatarController.PlatformAnchors;
            
            if (currentPlatformIndex >= platformAnchors.Count - 1)
            {
                return;
            }
            
            _isAnimating = true;
            _currentRound++;
            
            GameEvents.RaiseAnimationStarted();
            ProcessRoundWin(currentPlatformIndex);
        }
        
        private void Lose()
        {
            if (_isAnimating)
            {
                return;
            }
            
            PlayerAvatar playerAvatar = _avatarController.PlayerAvatar;
            
            if (!playerAvatar || playerAvatar.IsEliminated)
            {
                return;
            }
            
            _isAnimating = true;
            
            GameEvents.RaiseAnimationStarted();
            ProcessRoundLose();
        }
        
        private void ProcessRoundWin(int currentPlatformIndex)
        {
            List<PlatformAnchor> platformAnchors = _avatarController.PlatformAnchors;
            PlatformAnchor currentAnchor = platformAnchors[currentPlatformIndex];
            PlatformAnchor nextAnchor = platformAnchors[currentPlatformIndex + 1];
            
            List<PlayerAvatar> avatarsToMove = currentAnchor.GetAvatars();
            
            int fakeEliminationCount = CalculateFakeEliminationCount();
            int visualEliminationCount = _avatarController.CalculateVisualEliminationCount(fakeEliminationCount);
            
            _avatarController.ReduceFakePlayerCount(fakeEliminationCount);
            
            SeparateAvatarsForWin(avatarsToMove, visualEliminationCount, 
                out List<PlayerAvatar> avatarsToAdvance, out List<PlayerAvatar> avatarsToEliminate);
            
            currentAnchor.ClearAvatars();
            
            AnimateRoundWin(avatarsToAdvance, avatarsToEliminate, nextAnchor);
            
            GameEvents.RaiseCameraMoveRequested(nextAnchor.Position);
        }
        
        private void SeparateAvatarsForWin(List<PlayerAvatar> allAvatars, int visualEliminationCount,
            out List<PlayerAvatar> avatarsToAdvance, out List<PlayerAvatar> avatarsToEliminate)
        {
            avatarsToEliminate = new();
            avatarsToAdvance = new();
            
            PlayerAvatar playerAvatar = _avatarController.PlayerAvatar;
            avatarsToAdvance.Add(playerAvatar);
            
            for (int avatarIndex = 0; avatarIndex < allAvatars.Count; avatarIndex++)
            {
                PlayerAvatar avatar = allAvatars[avatarIndex];
                
                if (avatar.IsPlayer)
                {
                    continue;
                }
                
                if (avatarsToEliminate.Count < visualEliminationCount)
                {
                    bool shouldEliminate = Random.value > 0.6f;
                    
                    if (shouldEliminate)
                    {
                        avatarsToEliminate.Add(avatar);
                        continue;
                    }
                }
                
                avatarsToAdvance.Add(avatar);
            }
            
            FillRemainingEliminationSlots(avatarsToAdvance, avatarsToEliminate, visualEliminationCount);
        }
        
        private void FillRemainingEliminationSlots(List<PlayerAvatar> avatarsToAdvance, 
            List<PlayerAvatar> avatarsToEliminate, int targetCount)
        {
            while (avatarsToEliminate.Count < targetCount && avatarsToAdvance.Count > 1)
            {
                int randomIndex = Random.Range(1, avatarsToAdvance.Count);
                avatarsToEliminate.Add(avatarsToAdvance[randomIndex]);
                avatarsToAdvance.RemoveAt(randomIndex);
            }
        }
        
        private void AnimateRoundWin(List<PlayerAvatar> avatarsToAdvance, 
            List<PlayerAvatar> avatarsToEliminate, PlatformAnchor nextAnchor)
        {
            LavaQuestConfig config = GameConfig.Config;
            
            int completedAnimations = 0;
            int totalAnimations = avatarsToAdvance.Count + avatarsToEliminate.Count;
            
            System.Action onAnimationComplete = () =>
            {
                completedAnimations++;
                
                if (completedAnimations >= totalAnimations)
                {
                    OnRoundAnimationsComplete();
                }
            };
            
            List<PlayerAvatar> orderedAvatars = OrderAvatarsPlayerFirst(avatarsToAdvance);
            
            AnimateAvatarsJumping(orderedAvatars, nextAnchor, config, onAnimationComplete, delayOffset: 0);
            AnimateAvatarsFalling(avatarsToEliminate, avatarsToAdvance.Count, config, onAnimationComplete);
        }
        
        private List<PlayerAvatar> OrderAvatarsPlayerFirst(List<PlayerAvatar> avatars)
        {
            List<PlayerAvatar> playerFirst = new();
            List<PlayerAvatar> others = new();
            
            for (int index = 0; index < avatars.Count; index++)
            {
                if (avatars[index].IsPlayer)
                {
                    playerFirst.Add(avatars[index]);
                }
                else
                {
                    others.Add(avatars[index]);
                }
            }
            
            List<PlayerAvatar> orderedAvatars = new();
            orderedAvatars.AddRange(playerFirst);
            orderedAvatars.AddRange(others);
            
            return orderedAvatars;
        }
        
        private void AnimateAvatarsJumping(List<PlayerAvatar> avatars, PlatformAnchor targetAnchor, 
            LavaQuestConfig config, System.Action onComplete, int delayOffset)
        {
            for (int i = 0; i < avatars.Count; i++)
            {
                PlayerAvatar avatar = avatars[i];
                float delay = (i + delayOffset) * config.avatarJumpDelay;
                
                Vector2 targetPosition = targetAnchor.ReserveNextSlot();
                targetAnchor.AddAvatar(avatar);
                
                DOVirtual.DelayedCall(delay, () =>
                {
                    _avatarController.BringAvatarToFront(avatar);
                    
                    avatar.JumpToPlatform(
                        targetPosition,
                        config.jumpHeight,
                        config.jumpDuration,
                        onComplete
                    );
                });
            }
        }
        
        private void AnimateAvatarsFalling(List<PlayerAvatar> avatars, int delayOffset, 
            LavaQuestConfig config, System.Action onComplete)
        {
            for (int index = 0; index < avatars.Count; index++)
            {
                PlayerAvatar avatar = avatars[index];
                float delay = (delayOffset + index) * config.avatarJumpDelay;
                
                DOVirtual.DelayedCall(delay, () =>
                {
                    avatar.FallDown(
                        config.fallDistanceBelowScreen,
                        config.fallDuration,
                        () =>
                        {
                            onComplete();
                            _avatarController.RemoveAvatar(avatar);
                        }
                    );
                });
            }
        }
        
        private void ProcessRoundLose()
        {
            PlayerAvatar playerAvatar = _avatarController.PlayerAvatar;
            List<PlatformAnchor> platformAnchors = _avatarController.PlatformAnchors;
            
            int currentPlatformIndex = playerAvatar.CurrentPlatformIndex;
            PlatformAnchor currentAnchor = platformAnchors[currentPlatformIndex];
            
            List<PlayerAvatar> otherAvatars = GetOtherAvatarsOnPlatform(currentAnchor);
            
            currentAnchor.ClearAvatars();
            
            AnimateRoundLose(playerAvatar, otherAvatars, currentPlatformIndex, platformAnchors);
        }
        
        private List<PlayerAvatar> GetOtherAvatarsOnPlatform(PlatformAnchor anchor)
        {
            List<PlayerAvatar> avatarsOnPlatform = anchor.GetAvatars();
            List<PlayerAvatar> otherAvatars = new();
            
            for (int index = 0; index < avatarsOnPlatform.Count; index++)
            {
                if (!avatarsOnPlatform[index].IsPlayer)
                {
                    otherAvatars.Add(avatarsOnPlatform[index]);
                }
            }
            
            return otherAvatars;
        }
        
        private void AnimateRoundLose(PlayerAvatar playerAvatar, List<PlayerAvatar> otherAvatars, 
            int currentPlatformIndex, List<PlatformAnchor> platformAnchors)
        {
            LavaQuestConfig config = GameConfig.Config;
            
            int completedAnimations = 0;
            int totalAnimations = 1 + otherAvatars.Count;
            
            System.Action onAnimationComplete = () =>
            {
                completedAnimations++;

                if (completedAnimations < totalAnimations)
                {
                    return;
                }
                
                _isAnimating = false;
                GameEvents.RaiseAnimationEnded();
                OnPlayerEliminated();
            };
            
            AnimatePlayerFalling(playerAvatar, config, onAnimationComplete);
            AnimateOtherAvatarsAdvancing(otherAvatars, currentPlatformIndex, platformAnchors, config, onAnimationComplete);
        }
        
        private void AnimatePlayerFalling(PlayerAvatar playerAvatar, LavaQuestConfig config, System.Action onComplete)
        {
            playerAvatar.FallDown(config.fallDistanceBelowScreen, config.fallDuration, onComplete
            );
        }
        
        private void AnimateOtherAvatarsAdvancing(List<PlayerAvatar> otherAvatars, int currentPlatformIndex, 
            List<PlatformAnchor> platformAnchors, LavaQuestConfig config, System.Action onComplete)
        {
            if (currentPlatformIndex >= platformAnchors.Count - 1)
            {
                CompleteRemainingAnimations(otherAvatars.Count, onComplete);
                return;
            }
            
            PlatformAnchor nextAnchor = platformAnchors[currentPlatformIndex + 1];
            
            AnimateAvatarsJumping(otherAvatars, nextAnchor, config, onComplete, delayOffset: 1);
            
            GameEvents.RaiseCameraMoveRequested(nextAnchor.Position);
        }
        
        private void CompleteRemainingAnimations(int count, System.Action onComplete)
        {
            for (int i = 0; i < count; i++)
            {
                onComplete();
            }
        }
        
        private int CalculateFakeEliminationCount()
        {
            int currentFakePlayers = _avatarController.FakePlayerCount;
            
            if (currentFakePlayers <= 2)
            {
                return 0;
            }
            
            LavaQuestConfig config = GameConfig.Config;
            int nonPlayerCount = currentFakePlayers - 1;
            
            return Mathf.FloorToInt(nonPlayerCount * config.eliminationRate);
        }
        
        private void OnRoundAnimationsComplete()
        {
            _isAnimating = false;
            GameEvents.RaiseAnimationEnded();
            
            _avatarController.UpdateAvatarSortingOrder();
            
            PlayerAvatar playerAvatar = _avatarController.PlayerAvatar;
            List<PlatformAnchor> platformAnchors = _avatarController.PlatformAnchors;
            
            int currentPlatformIndex = playerAvatar.CurrentPlatformIndex;
            int survivingFakePlayers = _avatarController.FakePlayerCount;
            
            GameEvents.RaisePlayerCountChanged(survivingFakePlayers, _avatarController.InitialFakePlayerCount);
            GameEvents.RaiseRoundComplete(_currentRound, platformAnchors.Count - 1);
            
            if (currentPlatformIndex >= platformAnchors.Count - 1)
            {
                OnPlayerReachedTop();
            }
            else
            {
                GameEvents.RaiseGameStateChanged(GameState.RoundComplete);
            }
        }
        
        private void OnPlayerEliminated()
        {
            GameEvents.RaisePlayerEliminated();
            GameEvents.RaiseGameStateChanged(GameState.Eliminated);
        }
        
        private void OnPlayerReachedTop()
        {
            _avatarController.BroadcastPlayerAvatarData();
            GameEvents.RaisePlayerVictory();
            GameEvents.RaiseGameStateChanged(GameState.Victory);
        }
        
        private void OnDisable()
        {
            GameEvents.OnWinRequested -= HandleWinRequested;
            GameEvents.OnLoseRequested -= HandleLoseRequested;
            GameEvents.OnResetRequested -= HandleResetRequested;
            GameEvents.OnGameResetRequested -= HandleGameReset;
            GameEvents.OnAnimationStarted -= HandleAnimationStarted;
            GameEvents.OnAnimationEnded -= HandleAnimationEnded;
        }
    }
}