using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

public class MusicManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ManagerLinkerSO _managerLinker;
    [SerializeField] private AudioRepositorySO _audioRepository;

    [Header("Settings")]
    [SerializeField] private float _crossFadeDuration = 0.5f;
    [SerializeField] private float _maxVolume = 1f;

    private Dictionary<AbilityType, AudioSource> _audioSources = new Dictionary<AbilityType, AudioSource>();
    private AudioSource _zoneAudioSource;
    private AbilityType _currentAbilityType;
    private List<AudioClip> _activeZoneClips = new List<AudioClip>();
    private bool _isZoneActive => _activeZoneClips.Count > 0;

    private void Start()
    {
        InitializeAudioSources();
        CreateZoneAudioSource();

        if (_managerLinker != null)
        {
            _managerLinker.OnAbilityEquippedMusicRequest += HandleMusicChange;
            _managerLinker.OnZoneEnterMusicRequest += HandleZoneEnter;
            _managerLinker.OnZoneExitMusicRequest += HandleZoneExit;
        }
    }

    private void OnDestroy()
    {
        if (_managerLinker != null)
        {
            _managerLinker.OnAbilityEquippedMusicRequest -= HandleMusicChange;
            _managerLinker.OnZoneEnterMusicRequest -= HandleZoneEnter;
            _managerLinker.OnZoneExitMusicRequest -= HandleZoneExit;
        }
    }

    private void InitializeAudioSources()
    {
        if (_audioRepository == null) return;

        foreach (var track in _audioRepository.GetAllTracks())
        {
            if (track.Clip == null) continue;

            GameObject sourceObj = new GameObject($"MusicSource_{track.Type}");
            sourceObj.transform.SetParent(transform);
            
            AudioSource source = sourceObj.AddComponent<AudioSource>();
            source.clip = track.Clip;
            source.loop = true;
            source.volume = 0f;
            source.playOnAwake = false;
            
            _audioSources[track.Type] = source;
            source.Play();
        }
    }

    private void CreateZoneAudioSource()
    {
        GameObject sourceObj = new GameObject("MusicSource_Zone");
        sourceObj.transform.SetParent(transform);
        
        _zoneAudioSource = sourceObj.AddComponent<AudioSource>();
        _zoneAudioSource.loop = true;
        _zoneAudioSource.volume = 0f;
        _zoneAudioSource.playOnAwake = false;
    }

    private void HandleMusicChange(AbilityType type)
    {
        _currentAbilityType = type;

        if (_isZoneActive) return;

        // Fade out all sources except the target one
        foreach (var kvp in _audioSources)
        {
            var sourceType = kvp.Key;
            var source = kvp.Value;
            
            if (sourceType == type)
            {
                source.DOFade(_maxVolume, _crossFadeDuration);
            }
            else
            {
                source.DOFade(0f, _crossFadeDuration);
            }
        }
    }

    private void HandleZoneEnter(AudioClip clip)
    {
        if (clip == null) return;
        
        if (!_activeZoneClips.Contains(clip))
        {
            _activeZoneClips.Add(clip);
        }

        foreach (var source in _audioSources.Values)
        {
            source.DOFade(0f, _crossFadeDuration);
        }

        PlayZoneMusic(clip);
    }

    private void HandleZoneExit(AudioClip clip)
    {
        if (clip != null && _activeZoneClips.Contains(clip))
        {
            _activeZoneClips.Remove(clip);
        }

        if (_isZoneActive)
        {
            // Play the last added clip (nested zone behavior)
            PlayZoneMusic(_activeZoneClips.Last());
        }
        else
        {
            _zoneAudioSource.DOFade(0f, _crossFadeDuration);
            HandleMusicChange(_currentAbilityType);
        }
    }

    private void PlayZoneMusic(AudioClip clip)
    {
        if (_zoneAudioSource.clip == clip && _zoneAudioSource.isPlaying) return;

        _zoneAudioSource.clip = clip;
        _zoneAudioSource.Play();
        _zoneAudioSource.DOFade(_maxVolume, _crossFadeDuration);
    }
}
