using UnityEngine;
using System.Collections.Generic;

namespace LavaQuest.Core
{
    public sealed class GameStateMachine : MonoBehaviour
    {
        [Header("State Panels")]
        [SerializeField] private GameStatePanel _introPanel;
        [SerializeField] private GameStatePanel _matchmakingPanel;
        [SerializeField] private GameStatePanel _tutorialPanel;
        [SerializeField] private GameStatePanel _roundCompletePanel;
        [SerializeField] private GameStatePanel _victoryPanel;
        [SerializeField] private GameStatePanel _eliminatedPanel;
        
        private readonly Dictionary<GameState, IGameState> _states = new();
        private IGameState _currentState;
        private GameState _currentStateType = GameState.Empty;

        private void Awake()
        {
            RegisterStates();
        }
        
        private void OnEnable()
        {
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
        }
        
        private void OnDisable()
        {
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
        }
        
        private void Start()
        {
            Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
            
            ChangeState(GameState.Intro);
        }
        
        private void HandleGameStateChanged(GameState newState)
        {
            ChangeState(newState);
        }
        
        private void Update()
        {
            _currentState?.Tick();
        }
        
        private void RegisterStates()
        {
            RegisterState(GameState.Intro, _introPanel);
            RegisterState(GameState.Matchmaking, _matchmakingPanel);
            RegisterState(GameState.Tutorial, _tutorialPanel);
            RegisterState(GameState.RoundComplete, _roundCompletePanel);
            RegisterState(GameState.Victory, _victoryPanel);
            RegisterState(GameState.Eliminated, _eliminatedPanel);
        }
        
        private void RegisterState(GameState stateType, IGameState state)
        {
            if (state == null)
            {
                return;
            }
            
            state.Initialize(this);
            _states[stateType] = state;
        }
        
        public void ChangeState(GameState newStateType)
        {
            if (_currentStateType == newStateType)
            {
                return;
            }
            
            _currentState?.Exit();
            _currentStateType = newStateType;
            _currentState = null;
            
            if (_states.TryGetValue(newStateType, out IGameState newState))
            {
                _currentState = newState;
                _currentState.Enter();
            }
            
            GameEvents.RaiseGameStateChanged(newStateType);
        }
    }
}