using LavaQuest.Data;
using UnityEngine;

namespace LavaQuest.Core
{
    public static class GameConfig
    {
        private const string CONFIG_PATH = "GameSettings";
        
        private static LavaQuestConfig _config;
        
        public static LavaQuestConfig Config
        {
            get
            {
                if (_config)
                {
                    return _config;
                }

                _config = Resources.Load<LavaQuestConfig>(CONFIG_PATH);
                return _config;
            }
        }
        
        public static AvatarAnimationConfig AvatarAnimation
        {
            get
            {
                return Config?.avatarAnimationConfig;
            }
        }
    }
}