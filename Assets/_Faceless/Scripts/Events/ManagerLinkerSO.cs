using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ManagerLinker", menuName = "Events/ManagerLinker")]
public class ManagerLinkerSO : ScriptableObject
{
    public event Action<AbilityType> OnAbilityEquippedMusicRequest;
    public event Action<AudioClip> OnZoneEnterMusicRequest;
    public event Action OnZoneExitMusicRequest;
    public Func<Transform> GetPlayerTransform;

    public void RaiseAbilityEquippedMusicRequest(AbilityType abilityType)
    {
        OnAbilityEquippedMusicRequest?.Invoke(abilityType);
    }

    public void RaiseZoneEnterMusicRequest(AudioClip musicClip)
    {
        OnZoneEnterMusicRequest?.Invoke(musicClip);
    }

    public void RaiseZoneExitMusicRequest()
    {
        OnZoneExitMusicRequest?.Invoke();
    }
}
