using UnityEngine;

[CreateAssetMenu(menuName = "Items/Ability Item")]
public class AbilityItem : ScriptableObject
{
    public string ItemName;
    [TextArea] public string Description;
    public Sprite Icon;
    public AbilitySO AbilityToUnlock;
}
