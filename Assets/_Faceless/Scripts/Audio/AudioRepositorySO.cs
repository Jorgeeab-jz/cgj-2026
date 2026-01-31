using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioRepository", menuName = "Audio/Repository")]
public class AudioRepositorySO : ScriptableObject
{
    [System.Serializable]
    public struct AbilityAudio
    {
        public AbilityType Type;
        public AudioClip Clip;
    }

    [SerializeField] private List<AbilityAudio> _abilityMusicTracks;

    public AudioClip GetClip(AbilityType type)
    {
        foreach (var track in _abilityMusicTracks)
        {
            if (track.Type == type)
            {
                return track.Clip;
            }
        }
        return null;
    }

    public List<AbilityAudio> GetAllTracks() => _abilityMusicTracks;
}
