using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ManagerLinker", menuName = "Events/ManagerLinker")]
public class ManagerLinkerSO : ScriptableObject
{
    public event Action<AbilityType> OnAbilityEquippedMusicRequest;

    public void RaiseAbilityEquippedMusicRequest(AbilityType abilityType)
    {
        OnAbilityEquippedMusicRequest?.Invoke(abilityType);
    }
}
