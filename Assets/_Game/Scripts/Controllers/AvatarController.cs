using UnityEngine;
using System.Collections.Generic;
using LavaQuest.Core;
using LavaQuest.Data;
using LavaQuest.Gameplay;

namespace LavaQuest.Controllers
{
    public sealed class AvatarController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform _avatarContainer;
        [SerializeField] private PlayerAvatar _avatarPrefab;
        [SerializeField] private AvatarDatabase _avatarDatabase;
        [SerializeField] private List<PlatformAnchor> _platformAnchors = new();
        
        private readonly List<PlayerAvatar> _allAvatars = new();
        
        private PlayerAvatar _playerAvatar;
        private int _siblingIndexCounter;
        private int _fakePlayerCount;
        private int _initialFakePlayerCount;
        
        public PlayerAvatar PlayerAvatar => _playerAvatar;
        public int FakePlayerCount => _fakePlayerCount;
        public int InitialFakePlayerCount => _initialFakePlayerCount;
        public List<PlatformAnchor> PlatformAnchors => _platformAnchors;

        private void OnEnable()
        {
            GameEvents.OnAvatarsSpawnRequested += HandleSpawnRequested;
            GameEvents.OnAvatarsClearRequested += HandleClearRequested;
            GameEvents.OnGameResetRequested += HandleGameReset;
        }
        
        private void Start()
        {
            BroadcastAvatarContainer();
        }
        
        private void BroadcastAvatarContainer()
        {
            GameEvents.RaiseAvatarContainerReady(_avatarContainer);
        }
        
        private void HandleSpawnRequested()
        {
            SpawnAvatars();
        }
        
        private void HandleClearRequested()
        {
            ClearAllAvatars();
        }
        
        private void HandleGameReset()
        {
            ClearAllAvatars();
            _siblingIndexCounter = 0;
            _fakePlayerCount = 0;
            _initialFakePlayerCount = 0;
        }
        
        private void SpawnAvatars()
        {
            if (_platformAnchors.Count == 0)
            {
                return;
            }
            
            if (!_avatarDatabase)
            {
                return;
            }
            
            if (!_avatarPrefab)
            {
                return;
            }
            
            if (!_avatarContainer)
            {
                return;
            }
            
            LavaQuestConfig config = GameConfig.Config;
            if (!config)
            {
                return;
            }
            
            _avatarDatabase.ResetSelection();
            _siblingIndexCounter = 0;
            
            PlatformAnchor startAnchor = _platformAnchors[0];
            int totalPlayers = GameConfig.Config.displayAvatarCount;
            
            for (int playerIndex = 1; playerIndex < totalPlayers; playerIndex++)
            {
                PlayerAvatar avatar = Instantiate(_avatarPrefab, _avatarContainer);

                AvatarData avatarData = _avatarDatabase.GetNextOpponentAvatar();

                Sprite iconSprite = avatarData.iconSprite;
                Sprite frameSprite = avatarData.frameSprite;
                
                avatar.Initialize(0, false, iconSprite, frameSprite);
                
                Vector2 slotPosition = startAnchor.ReserveNextSlot();
                avatar.SetPosition(slotPosition);
                avatar.PlayIdleBounce();
                
                startAnchor.AddAvatar(avatar);
                _allAvatars.Add(avatar);
            }
            
            PlayerAvatar playerAvatar = Instantiate(_avatarPrefab, _avatarContainer);
            AvatarData playerAvatarData = _avatarDatabase.playerAvatar;

            Sprite playerIconSprite = playerAvatarData.iconSprite;
            Sprite playerFrameSprite = playerAvatarData.frameSprite;
            
            playerAvatar.Initialize(0, true, playerIconSprite, playerFrameSprite);
            
            Vector2 playerSlotPosition = startAnchor.ReserveNextSlot();
            playerAvatar.SetPosition(playerSlotPosition);
            playerAvatar.PlayIdleBounce();
            
            startAnchor.AddAvatar(playerAvatar);
            _allAvatars.Add(playerAvatar);
            _playerAvatar = playerAvatar;
            
            UpdateAvatarSortingOrder();

            _fakePlayerCount = config.matchmakingMaxPlayers;
            _initialFakePlayerCount = _fakePlayerCount;
            
            GameEvents.RaisePlayerCountChanged(_fakePlayerCount, _initialFakePlayerCount);
        }
        
        public int CalculateVisualEliminationCount(int fakeEliminationCount)
        {
            if (_allAvatars.Count <= 1)
            {
                return 0;
            }
            
            int nonPlayerAvatars = _allAvatars.Count - 1;
            int nonPlayerFakePlayers = _fakePlayerCount - 1;
            
            if (nonPlayerFakePlayers <= 0)
            {
                return 0;
            }
            
            float ratio = (float)fakeEliminationCount / nonPlayerFakePlayers;
            int visualEliminations = Mathf.RoundToInt(nonPlayerAvatars * ratio);
            
            return Mathf.Clamp(visualEliminations, 0, nonPlayerAvatars);
        }
        
        public void ReduceFakePlayerCount(int eliminatedCount)
        {
            _fakePlayerCount = Mathf.Max(1, _fakePlayerCount - eliminatedCount);
        }
        
        public void BringAvatarToFront(PlayerAvatar avatar)
        {
            if (!avatar.IsPlayer)
            {
                _siblingIndexCounter++;
                avatar.transform.SetSiblingIndex(_siblingIndexCounter + 1000);
            }
            
            if (_playerAvatar)
            {
                _playerAvatar.transform.SetAsLastSibling();
            }
        }
        
        public void UpdateAvatarSortingOrder()
        {
            if (_playerAvatar)
            {
                _playerAvatar.transform.SetAsLastSibling();
            }
        }
        
        public void RemoveAvatar(PlayerAvatar avatar)
        {
            _allAvatars.Remove(avatar);
            Destroy(avatar.gameObject);
        }
        
        private void ClearAllAvatars()
        {
            for (int avatarIndex = _allAvatars.Count - 1; avatarIndex >= 0; avatarIndex--)
            {
                if (_allAvatars[avatarIndex])
                {
                    Destroy(_allAvatars[avatarIndex].gameObject);
                }
            }
            
            _allAvatars.Clear();
            _playerAvatar = null;
            
            for (int anchorIndex = 0; anchorIndex < _platformAnchors.Count; anchorIndex++)
            {
                _platformAnchors[anchorIndex].ClearAvatars();
            }
        }
        
        public void BroadcastPlayerAvatarData()
        {
            if (!_playerAvatar || !_avatarDatabase)
            {
                return;
            }
            
            AvatarData playerAvatarData = _avatarDatabase.playerAvatar;
            
            if (playerAvatarData)
            {
                GameEvents.RaisePlayerAvatarDataReady(playerAvatarData.iconSprite, playerAvatarData.frameSprite);
            }
        }
        
        private void OnDisable()
        {
            GameEvents.OnAvatarsSpawnRequested -= HandleSpawnRequested;
            GameEvents.OnAvatarsClearRequested -= HandleClearRequested;
            GameEvents.OnGameResetRequested -= HandleGameReset;
        }
    }
}