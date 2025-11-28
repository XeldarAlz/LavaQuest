using UnityEngine;

namespace LavaQuest.Data
{
    [CreateAssetMenu(fileName = "AvatarData", menuName = "LavaQuest/Avatar Data")]
    public sealed class AvatarData : ScriptableObject
    {
        public Sprite iconSprite;
        public Sprite frameSprite;
    }
}