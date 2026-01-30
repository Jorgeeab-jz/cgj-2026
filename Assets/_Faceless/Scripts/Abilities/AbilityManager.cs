using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerControllerStats _baseStats; // The original stats asset
    [SerializeField] private AbilityInputReader _inputReader;
    
    [Header("State")]
    [SerializeField] private PlayerControllerStats _runtimeStats; // The clone we modify
    [SerializeField] private List<AbilitySO> _unlockedAbilities = new List<AbilitySO>();
    [SerializeField] private AbilitySO _currentActiveAbility;

    public PlayerControllerStats RuntimeStats => _runtimeStats;
    public PlayerControllerStats BaseStats => _baseStats; 

    private void Awake()
    {

        ResetAbilities();
    }

    private void Start()
    {
        InjectRuntimeStats();
    }

    private void InjectRuntimeStats()
    {

    }

    private void Update()
    {
        if (_currentActiveAbility != null)
        {
            _currentActiveAbility.OnUpdate();
        }
    }

    private void FixedUpdate()
    {
        if (_currentActiveAbility != null)
        {
            _currentActiveAbility.OnFixedUpdate();
        }
    }

    public void UnlockAbility(AbilitySO abilityPrefab)
    {
        if (HasAbility(abilityPrefab.Type)) return;

        AbilitySO instance = Instantiate(abilityPrefab);
        instance.Initialize(gameObject, this, _inputReader);
        _unlockedAbilities.Add(instance);

        // If it's a passive stat modifier, apply it immediately
        if (instance is StatModifierAbilitySO statAbility)
        {
            statAbility.ApplyStats(_runtimeStats);
        }
        // If it's an active ability (Mask), equip it
        else
        {
            EquipAbility(instance);
        }
        
        Debug.Log($"Unlocked ability: {instance.AbilityName}");
    }

    public void EquipAbility(AbilitySO ability)
    {
        if (_currentActiveAbility != null)
        {
            _currentActiveAbility.OnUnequip();
        }

        _currentActiveAbility = ability;
        
        if (_currentActiveAbility != null)
        {
            _currentActiveAbility.OnEquip();
        }
    }

    public bool HasAbility(AbilityType type)
    {
        return _unlockedAbilities.Exists(a => a.Type == type);
    }

    private void ResetAbilities()
    {

    }
}
