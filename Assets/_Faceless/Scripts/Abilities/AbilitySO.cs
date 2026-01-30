using UnityEngine;

public abstract class AbilitySO : ScriptableObject
{
    [Header("Base Info")]
    public AbilityType Type;
    public string AbilityName;
    [TextArea] public string Description;
    public Sprite Icon;

    protected GameObject Owner;
    protected AbilityInputReader InputReader;
    protected AbilityManager Manager;

    public virtual void Initialize(GameObject owner, AbilityManager manager, AbilityInputReader inputReader)
    {
        Owner = owner;
        Manager = manager; // Reference back to manager if needed
        InputReader = inputReader;
    }

    /// <summary>
    /// Called every frame while this ability is ACTIVE (Equipped/Current).
    /// </summary>
    public virtual void OnUpdate() { }

    /// <summary>
    /// Called every fixed frame while this ability is ACTIVE.
    /// </summary>
    public virtual void OnFixedUpdate() { }

    /// <summary>
    /// Called when the ability becomes the active one.
    /// </summary>
    public virtual void OnEquip() { }

    /// <summary>
    /// Called when the ability is switched out or removed.
    /// </summary>
    public virtual void OnUnequip() { }
}
