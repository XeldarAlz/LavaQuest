using UnityEngine;

namespace LavaQuest.Data
{
    [CreateAssetMenu(fileName = "LavaQuestConfig", menuName = "LavaQuest/Config")]
    public sealed class LavaQuestConfig : ScriptableObject
    {
        [Header("Game Settings")]
        [Range(3, 15)] public int platformCount = 8;
        [Range(1f, 24f)] public float eventDurationHours = 8f;
        
        [Header("Matchmaking")]
        [Range(50, 200)] public int matchmakingMaxPlayers = 100;
        [Range(1f, 4f)] public float matchmakingDuration = 3f;
        [Range(0f, 2f)] public float matchmakingStartDelay = 0.5f;
        [Range(0.1f, 1f)] public float matchmakingDelayBeforeStart = 0.5f;
        [Range(5, 50)] public int matchmakingPopSoundInterval = 20;
        
        [Header("Victory")]
        [Range(100, 10000)] public int victoryRewardAmount = 1453;
        [Range(0.5f, 3f)] public float victoryRewardCountDuration = 1f;
        
        [Header("Tutorial")]
        [Range(0.5f, 5f)] public float tutorialTapDelay = 2f;
        [Range(0.5f, 3f)] public float gameStartDelay = 1f;
        [Range(-1000f, 1000f)] public float gameStartCameraOffset = 500f;
        
        [Header("Avatar")]
        [Range(0.2f, 0.5f)] public float eliminationRate = 0.3f;
        [Range(2, 20)] public int displayAvatarCount = 12;
        public AvatarAnimationConfig avatarAnimationConfig;
        
        [Header("Jump Animation")]
        [Range(0.05f, 0.3f)] public float avatarJumpDelay = 0.15f;
        [Range(0.2f, 1f)] public float jumpDuration = 0.5f;
        [Range(50f, 300f)] public float jumpHeight = 150f;
        
        [Header("Fall Animation")]
        [Range(0.5f, 2f)] public float fallDuration = 1.2f;
        [Range(250f, 2500f)] public float fallDistanceBelowScreen = 500f;
        
        [Header("Camera")]
        [Range(0.3f, 2f)] public float cameraFollowDuration = 0.6f;
        [Range(-500f, 500f)] public float cameraVerticalOffset = 200f;
    }
}