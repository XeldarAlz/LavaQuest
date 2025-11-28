using UnityEngine;

namespace LavaQuest.Data
{
    [CreateAssetMenu(fileName = "AvatarAnimationConfig", menuName = "LavaQuest/Avatar Animation Config")]
    public sealed class AvatarAnimationConfig : ScriptableObject
    {
        [Header("Squash and Stretch")]
        [Range(0.5f, 1.0f)] public float squashAmount = 0.8f;
        [Range(1.0f, 1.5f)] public float stretchAmount = 1.2f;
        [Range(0.05f, 0.3f)] public float anticipationDuration = 0.1f;
        [Range(0.05f, 0.3f)] public float landSquashDuration = 0.15f;
        
        [Header("Jump Animation Timing")]
        [Range(0.3f, 0.6f)] public float jumpUpRatio = 0.45f;
        [Range(0.4f, 0.7f)] public float jumpDownRatio = 0.55f;
        [Range(0.3f, 0.7f)] public float jumpStretchRatio = 0.5f;
        [Range(0.5f, 0.9f)] public float jumpLandStartRatio = 0.7f;
        
        [Header("Fall Animation")]
        [Range(0.1f, 0.5f)] public float fallShakeDuration = 0.2f;
        [Range(10f, 50f)] public float fallShakeIntensity = 20f;
        [Range(0.05f, 0.3f)] public float fallPrepDuration = 0.15f;
        [Range(20f, 100f)] public float fallPrepHeight = 50f;
        [Range(0.5f, 1.0f)] public float fallPrepSquash = 0.7f;
        [Range(50f, 200f)] public float fallWobbleRange = 100f;
        [Range(0.1f, 0.5f)] public float fallScaleEnd = 0.3f;
        [Range(0.2f, 0.6f)] public float fallFadeDelay = 0.35f;
        
        [Header("Idle Bounce Animation")]
        [Range(5f, 20f)] public float idleBounceMin = 8f;
        [Range(10f, 30f)] public float idleBounceMax = 15f;
        [Range(0.3f, 1.0f)] public float idleDurationMin = 0.6f;
        [Range(0.5f, 1.5f)] public float idleDurationMax = 0.9f;
        [Range(1.0f, 1.2f)] public float idleScaleAmount = 1.05f;
        
        [Header("Shake Settings")]
        [Range(10, 40)] public int shakeVibrato = 20;
        [Range(0f, 180f)] public float shakeRandomness = 90f;
        
        public bool shakeSnapping = false;
        public bool shakeFadeOut = true;
    }
}