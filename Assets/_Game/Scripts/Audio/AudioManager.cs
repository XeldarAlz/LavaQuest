using UnityEngine;
using UnityEngine.Audio;
using LavaQuest.Core;

namespace LavaQuest.Audio
{
    public sealed class AudioManager : MonoBehaviour
    {
        [Header("Audio Mixers")]
        [SerializeField] private AudioMixerGroup _musicMixerGroup;
        [SerializeField] private AudioMixerGroup _sfxMixerGroup;
        
        [Header("Audio Sources")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;
        
        [Header("Music Clips")]
        [SerializeField] private AudioClip _mainMusic;
        
        [Header("SFX Clips")]
        [SerializeField] private AudioClip _buttonClickSfx;
        [SerializeField] private AudioClip _popSfx;
        [SerializeField] private AudioClip _matchmakingCompleteSfx;
        [SerializeField] private AudioClip _gameStartSfx;
        [SerializeField] private AudioClip _jumpSfx;
        [SerializeField] private AudioClip _victorySfx;
        [SerializeField] private AudioClip _eliminatedSfx;
        
        private AudioMixer _audioMixer;
        
        private void Awake()
        {
            SetupAudioSources();
        }
        
        private void OnEnable()
        {
            GameEvents.OnPlayButtonClick += HandlePlayButtonClick;
            GameEvents.OnPlayJump += HandlePlayJump;
            GameEvents.OnPlayVictory += HandlePlayVictory;
            GameEvents.OnPlayEliminated += HandlePlayEliminated;
            GameEvents.OnPlayPop += HandlePlayPop;
            GameEvents.OnPlayMatchmakingComplete += HandlePlayMatchmakingComplete;
            GameEvents.OnPlayGameStart += HandlePlayGameStart;
        }
        
        private void Start()
        {
            PlayMainMusic();
        }
        
        private void HandlePlayButtonClick()
        {
            PlaySfx(_buttonClickSfx);
        }
        
        private void HandlePlayJump()
        {
            PlaySfx(_jumpSfx);
        }
        
        private void HandlePlayVictory()
        {
            PlaySfx(_victorySfx);
        }
        
        private void HandlePlayEliminated()
        {
            PlaySfx(_eliminatedSfx);
        }
        
        private void HandlePlayPop()
        {
            PlaySfx(_popSfx);
        }
        
        private void HandlePlayMatchmakingComplete()
        {
            PlaySfx(_matchmakingCompleteSfx);
        }
        
        private void HandlePlayGameStart()
        {
            PlaySfx(_gameStartSfx);
        }
        
        private void SetupAudioSources()
        {
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
            
            if (_musicMixerGroup)
            {
                _musicSource.outputAudioMixerGroup = _musicMixerGroup;
                _audioMixer = _musicMixerGroup.audioMixer;
            }
            
            _sfxSource.loop = false;
            _sfxSource.playOnAwake = false;

            if (!_sfxMixerGroup)
            {
                return;
            }
            
            _sfxSource.outputAudioMixerGroup = _sfxMixerGroup;
                
            if (!_audioMixer)
            {
                _audioMixer = _sfxMixerGroup.audioMixer;
            }
        }
        
        private void PlayMainMusic()
        {
            if (!_mainMusic || !_musicSource)
            {
                return;
            }
            
            _musicSource.clip = _mainMusic;
            _musicSource.Play();
        }
        
        private void PlaySfx(AudioClip clip)
        {
            if (clip && _sfxSource)
            {
                _sfxSource.PlayOneShot(clip);
            }
        }
        
        private void OnDisable()
        {
            GameEvents.OnPlayButtonClick -= HandlePlayButtonClick;
            GameEvents.OnPlayJump -= HandlePlayJump;
            GameEvents.OnPlayVictory -= HandlePlayVictory;
            GameEvents.OnPlayEliminated -= HandlePlayEliminated;
            GameEvents.OnPlayPop -= HandlePlayPop;
            GameEvents.OnPlayMatchmakingComplete -= HandlePlayMatchmakingComplete;
            GameEvents.OnPlayGameStart -= HandlePlayGameStart;
        }
    }
}