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

    private void Awake()
    {
        // 1. Clone the stats so we don't modify the asset
        if (_baseStats != null)
        {
            _runtimeStats = Instantiate(_baseStats);
            // Assign the cloned stats to the specific components that need it?
            // HACK: The PlatformerController scripts might hold a ref to the original asset if they are already initialized.
            // Ideally, we'd inject this, but for now, we rely on this component initializing BEFORE PlayerController
            // OR we might need to manually swap the reference in PlayerController if possible.
            // Looking at PlayerController.cs, it takes stats in SerializeField. We might need to inject it.
            // For now, let's assume we can swap it or we just modify the one assigned if it's already an instance (unlikely for SO).
            
            // ACTUALLY: The PlayerController instantiates its logic with the Stats. 
            // We need to ensure PlayerController uses OUR RuntimeStats.
            // We can try to GetComponent<PlayerController>() and see if we can swap it, but fields are private.
            // PLAN B: We modify the Runtime Instance if PlayerController already cloned it? 
            // PlayerController DOES NOT clone it. It uses it directly.
            // DANGER: Modifying _baseStats directly modifies the Project Asset.
            // SOLUTION: We WILL inject _runtimeStats into PlayerController via Reflection or by ensuring PlayerController reads from us.
            // Since we can't easily change PlayerController code (Package), we will use Reflection to swap the field in Awake.
        }

        ResetAbilities();
    }

    private void Start()
    {
        InjectRuntimeStats();
    }

    private void InjectRuntimeStats()
    {
        var controller = GetComponent<PlayerController>();
        if (controller != null && _runtimeStats != null)
        {
            // 1. Inject into PlayerController
            var pcField = typeof(PlayerController).GetField("playerControllerStats", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (pcField != null)
            {
                pcField.SetValue(controller, _runtimeStats);
            }

            // 2. Inject into CollisionsChecker (Fixes 'Falling' if checks were using wrong/null stats)
            var collisionsChecker = GetComponent<CollisionsChecker>();
            if (collisionsChecker != null)
            {
                var ccField = typeof(CollisionsChecker).GetField("stats", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (ccField != null)
                {
                    ccField.SetValue(collisionsChecker, _runtimeStats);
                }
            }
            
            // 3. Cleanup rogue components from previous bugs (Telekinesis accidentally added Joint to Player)
            var rogueJoint = GetComponent<TargetJoint2D>();
            if (rogueJoint != null)
            {
                Debug.LogWarning("AbilityManager: Found and removing rogue TargetJoint2D on Player.");
                Destroy(rogueJoint);
            }
            
            // Note: We cannot easily inject into the private modules (MovementLogic, etc) inside PlayerController
            // without deep reflection. However, as long as CollisionsChecker and Controller have the new stats,
            // the main logic loop should be consistent enough for simple param tweaks.
            // If deeper issues persist, we might need a more invasive re-initialization.
        }
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
        // Reset stats to "Zero" ability state
        // For example, set Jumps to 1 (Player default is often 1, Double Jump makes it 2)
        // Set Dash to 0.
        // This depends on "Base" vs "Unlocked". 
        // Let's assume Base Stats have everything ENABLED (max power) or DISABLED (start power).
        // Ideally, Base Stats should contain "Max Potential" or "Starting State".
        // Let's assume Base Stats are "Starting State" (1 Jump, 0 Dash).
        // If Base Stats are "Max Potential" (2 Jumps, 1 Dash), then we need to NERF them here.
        
        // Let's assume we NERF them to start.
        if (_runtimeStats != null)
        {
            _runtimeStats.MaxNumberJumps = 1;
            _runtimeStats.MaxNumberDash = 0;
            // Disable Wall Jump?
            // _runtimeStats.WallJumpClimb = Vector2.zero; // Logic might be complex
        }
    }
}
