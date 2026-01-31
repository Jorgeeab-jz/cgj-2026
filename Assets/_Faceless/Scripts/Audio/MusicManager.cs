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

    private void Start()
    {
        InitializeAudioSources();
        if (_managerLinker != null)
        {
            _managerLinker.OnAbilityEquippedMusicRequest += HandleMusicChange;
        }
    }

    private void OnDestroy()
    {
        if (_managerLinker != null)
        {
            _managerLinker.OnAbilityEquippedMusicRequest -= HandleMusicChange;
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

    private void HandleMusicChange(AbilityType type)
    {
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
}
