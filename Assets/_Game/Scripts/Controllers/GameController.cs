using UnityEngine;
using LavaQuest.Core;
using LavaQuest.Data;

namespace LavaQuest.Controllers
{
    public sealed class GameController : MonoBehaviour
    {
        private void OnEnable()
        {
            GameEvents.OnGameInitializeRequested += HandleGameInitializeRequested;
        }
        
        private void HandleGameInitializeRequested()
        {
            InitializeGame();
        }
        
        private void InitializeGame()
        {
            LavaQuestConfig config = GameConfig.Config;
            
            if (!config)
            {
                return;
            }
            
            GameEvents.RaiseCameraSetStartPosition(config.gameStartCameraOffset);
            GameEvents.RaiseAvatarsSpawnRequested();
            GameEvents.RaiseCameraAnimateToStart(config.gameStartDelay, null);
        }
        
        private void OnDisable()
        {
            GameEvents.OnGameInitializeRequested -= HandleGameInitializeRequested;
        }
    }
}