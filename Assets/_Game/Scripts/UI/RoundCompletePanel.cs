using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;
using LavaQuest.Core;

namespace LavaQuest.UI
{
    public sealed class RoundCompletePanel : GameStatePanel
    {
        [Header("Round Complete Panel")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _congratsText;
        [SerializeField] private TextMeshProUGUI _levelsValueText;
        [SerializeField] private TextMeshProUGUI _playersValueText;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private float _numberCountDuration = 0.5f;

        [Header("Timer Settings")]
        [SerializeField] private float _timerUpdateInterval = 1f;
        
        [Header("Text Content")]
        [SerializeField] private string _titleString = "Lava Quest";
        [SerializeField] private string _congratsString = "Congratulations! You advanced to the next step!";
        [SerializeField] private string _valueFormat = "{0}/{1}";
        
        private float _remainingTimeSeconds;
        private bool _isTimerRunning;
        private Coroutine _timerCoroutine;
        private Tween _levelsTween;
        private Tween _playersTween;
        private int _currentLevel;
        private int _currentPlayers;

        protected override void Awake()
        {
            base.Awake();
            _remainingTimeSeconds = GameConfig.Config.eventDurationHours * 3600f;
        }

        private void OnEnable()
        {
            GameEvents.OnRoundComplete += HandleRoundComplete;
            GameEvents.OnPlayerCountChanged += HandlePlayerCountChanged;
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
        }

        private void HandleGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.Playing:
                    if (!_isTimerRunning)
                    {
                        StartTimer();
                    }
                    break;
                case GameState.Intro:
                case GameState.Eliminated:
                    StopTimer();
                    ResetValues();
                    break;
            }
        }

        private void HandleRoundComplete(int currentLevel, int totalLevels)
        {
            int previousLevel = _currentLevel;
            _currentLevel = currentLevel;

            AnimateLevelText(previousLevel, currentLevel, totalLevels);
        }

        private void HandlePlayerCountChanged(int currentPlayers, int totalPlayers)
        {
            int previousPlayers = _currentPlayers > 0 ? _currentPlayers : totalPlayers;
            _currentPlayers = currentPlayers;

            AnimatePlayersText(previousPlayers, currentPlayers, totalPlayers);
        }

        public override void Enter()
        {
            UpdateStaticTexts();
            UpdateTimerText();
            base.Enter();
        }

        private void ResetValues()
        {
            _remainingTimeSeconds = GameConfig.Config.eventDurationHours * 3600f;
            _currentLevel = 0;
            _currentPlayers = 0;
            
            if (_levelsValueText)
            {
                _levelsValueText.text = string.Format(_valueFormat, 0, 0);
            }
            
            if (_playersValueText)
            {
                _playersValueText.text = string.Format(_valueFormat, 0, 0);
            }
        }

        private void UpdateStaticTexts()
        {
            if (_titleText)
            {
                _titleText.text = _titleString;
            }

            if (_congratsText)
            {
                _congratsText.text = _congratsString;
            }
        }

        private void AnimateLevelText(int fromLevel, int toLevel, int totalLevels)
        {
            if (!_levelsValueText)
            {
                return;
            }

            KillLevelTween();
            
            _levelsValueText.text = string.Format(_valueFormat, fromLevel, totalLevels);
            
            _levelsTween = DOVirtual.Float(fromLevel, toLevel, _numberCountDuration, value =>
            {
                _levelsValueText.text = string.Format(_valueFormat, Mathf.RoundToInt(value), totalLevels);
            }).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                _levelsValueText.text = string.Format(_valueFormat, toLevel, totalLevels);
            });
        }

        private void AnimatePlayersText(int fromPlayers, int toPlayers, int totalPlayers)
        {
            if (!_playersValueText)
            {
                return;
            }

            KillPlayersTween();
            
            _playersValueText.text = string.Format(_valueFormat, fromPlayers, totalPlayers);
            
            _playersTween = DOVirtual.Float(fromPlayers, toPlayers, _numberCountDuration, value =>
            {
                _playersValueText.text = string.Format(_valueFormat, Mathf.RoundToInt(value), totalPlayers);
            }).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                _playersValueText.text = string.Format(_valueFormat, toPlayers, totalPlayers);
            });
        }

        private void KillTweens()
        {
            KillLevelTween();
            KillPlayersTween();
        }

        private void KillLevelTween()
        {
            if (_levelsTween == null || !_levelsTween.IsActive())
            {
                return;
            }
            
            _levelsTween.Kill();
            _levelsTween = null;
        }

        private void KillPlayersTween()
        {
            if (_playersTween == null || !_playersTween.IsActive())
            {
                return;
            }
            
            _playersTween.Kill();
            _playersTween = null;
        }

        private void StartTimer()
        {
            if (_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine);
            }

            _isTimerRunning = true;
            _timerCoroutine = StartCoroutine(TimerCoroutine());
        }

        private void StopTimer()
        {
            _isTimerRunning = false;
            
            if (_timerCoroutine == null)
            {
                return;
            }
            
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }

        private IEnumerator TimerCoroutine()
        {
            while (_remainingTimeSeconds > 0f)
            {
                yield return new WaitForSeconds(_timerUpdateInterval);
                _remainingTimeSeconds = Mathf.Max(0f, _remainingTimeSeconds - _timerUpdateInterval);
                UpdateTimerText();
            }

            _isTimerRunning = false;
        }

        private void UpdateTimerText()
        {
            if (!_timerText)
            {
                return;
            }

            int totalSeconds = Mathf.FloorToInt(_remainingTimeSeconds);
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;

            _timerText.text = $"{hours}:{minutes:00}:{seconds:00}";
        }
        
        private void OnDisable()
        {
            GameEvents.OnRoundComplete -= HandleRoundComplete;
            GameEvents.OnPlayerCountChanged -= HandlePlayerCountChanged;
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
            KillTweens();
            StopTimer();
        }
    }
}