using UnityEngine;
using System.Collections.Generic;

namespace LavaQuest.Data
{
    [CreateAssetMenu(fileName = "AvatarDatabase", menuName = "LavaQuest/Avatar Database")]
    public sealed class AvatarDatabase : ScriptableObject
    {
        [Header("Player Avatar")] 
        public AvatarData playerAvatar;
        
        [Header("Opponent Avatars")]
        public List<AvatarData> opponentAvatars = new();
        
        private readonly List<int> _availableIndices = new();
        
        public void ResetSelection()
        {
            _availableIndices.Clear();
            
            for (int index = 0; index < opponentAvatars.Count; index++)
            {
                _availableIndices.Add(index);
            }
            
            ShuffleAvailableIndices();
        }
        
        public AvatarData GetNextOpponentAvatar()
        {
            if (opponentAvatars.Count == 0)
            {
                return null;
            }
            
            if (_availableIndices.Count == 0)
            {
                ResetSelection();
            }
            
            int randomIndex = Random.Range(0, _availableIndices.Count);
            int avatarIndex = _availableIndices[randomIndex];
            
            _availableIndices.RemoveAt(randomIndex);
            
            return opponentAvatars[avatarIndex];
        }

        private void ShuffleAvailableIndices()
        {
            for (int index = _availableIndices.Count - 1; index > 0; index--)
            {
                int randomIndex = Random.Range(0, index + 1);
                (_availableIndices[index], _availableIndices[randomIndex]) = (_availableIndices[randomIndex], _availableIndices[index]);
            }
        }
        
        private void OnEnable()
        {
            ResetSelection();
        }
    }
}