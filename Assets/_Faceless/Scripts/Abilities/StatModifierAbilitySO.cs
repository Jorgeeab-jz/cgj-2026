using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Stat Modifier")]
public class StatModifierAbilitySO : AbilitySO
{
    [Header("Stat Modifications")]
    public int ExtraJumps = 0; // If 1, MaxJumps += 1
    public int ExtraDashes = 0; // If 1, MaxDashes += 1
    
    // Add other modifications as needed

    public void ApplyStats(PlayerControllerStats stats)
    {
        stats.MaxNumberJumps += ExtraJumps;
        stats.MaxNumberDash += ExtraDashes;
    }

    public override void OnEquip()
    {
        // Passive abilities don't need active logic usually, 
        // but if we want to toggle them, we could do it here.
        // For now, they are "Applied on Unlock".
    }
}
