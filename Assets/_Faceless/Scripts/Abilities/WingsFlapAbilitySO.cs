using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Abilities/WingsFlap")]
public class WingsFlapAbilitySO : AbilitySO
{
    [Header("Wings Flap Settings")]
    public int TotalJumps = 4; // 1 Ground + 3 Air
    
    [Tooltip("Percent of Max Jump Height for each consecutive air jump")]
    public float Flap1HeightRatio = 0.8f;
    public float Flap2HeightRatio = 0.6f;
    public float Flap3HeightRatio = 0.4f;

    [Header("Detection")]
    public LayerMask GroundLayer; // User must set this!
    public float GroundCheckDistance = 0.1f;
    public string JumpActionName = "Jump";

    // References
    private float _originalMaxJumpHeight;
    private int _originalMaxJumps;
    private int _airJumpsUsed = 0;
    
    // Runtime Components
    private InputAction _jumpAction;
    private Collider2D _collider;
    
    // Safety
    private bool _initialized;

    public override void Initialize(GameObject owner, AbilityManager manager, AbilityInputReader inputReader)
    {
        base.Initialize(owner, manager, inputReader);
        _collider = owner.GetComponent<Collider2D>();
        
        // Try to find PlayerInput to get the Jump action
        var playerInput = owner.GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            // Find action. Can be in ANY map, usually "Gameplay" or "Player"
            _jumpAction = playerInput.actions[JumpActionName];
            if (_jumpAction == null)
            {
                Debug.LogWarning($"WingsFlap: Action '{JumpActionName}' not found in PlayerInput.");
            }
        }
        else
        {
            Debug.LogWarning("WingsFlap: PlayerInput component not found on Owner. Jump detection will function via generic detection if possible, or fail.");
        }
        
        _initialized = true;
    }

    public override void OnEquip()
    {
        if (!_initialized) return;

        Debug.Log($"WingsFlap: OnEquip called.");

        if (Manager != null && Manager.RuntimeStats != null)
        {
            // Try to force inject stats into controller to ensure we aren't editing a detached clone
            ForceInjectStats();

            // Backup
            _originalMaxJumpHeight = Manager.RuntimeStats.MaxJumpHeight;
            _originalMaxJumps = (int)Manager.RuntimeStats.MaxNumberJumps;

            // Apply Ability Stats
            Manager.RuntimeStats.MaxNumberJumps = TotalJumps;
            Debug.Log($"WingsFlap: Applied Stats. MaxJumps set to {Manager.RuntimeStats.MaxNumberJumps} (Target: {TotalJumps})");
        }
        else
        {
             Debug.LogError("WingsFlap: Manager or RuntimeStats is null!");
        }

        if (_jumpAction != null)
        {
            _jumpAction.performed += OnJumpPerformed;
        }
    }

    private void ForceInjectStats()
    {
        if (Owner == null || Manager.RuntimeStats == null) return;

        var controller = Owner.GetComponent("PlayerController"); // Get by string incase type is hidden
        if (controller == null) return;

        var type = controller.GetType();
        var fields = type.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        bool injected = false;
        foreach (var field in fields)
        {
            // Check if field type matches our stats type
            if (field.FieldType.IsAssignableFrom(Manager.RuntimeStats.GetType()) || 
                field.FieldType.Name.Contains("PlayerControllerStats"))
            {
                try 
                {
                    field.SetValue(controller, Manager.RuntimeStats);
                    Debug.Log($"WingsFlap: Force Injected RuntimeStats into field '{field.Name}'");
                    injected = true;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"WingsFlap: Failed to inject into '{field.Name}': {e.Message}");
                }
            }
        }
        
        if (!injected) Debug.LogWarning("WingsFlap: Could not find any field to inject RuntimeStats into!");
    }

    public override void OnUnequip()
    {
        if (Manager != null && Manager.RuntimeStats != null)
        {
            // Restore
            Manager.RuntimeStats.MaxJumpHeight = _originalMaxJumpHeight;
            Manager.RuntimeStats.MaxNumberJumps = _originalMaxJumps;
            Debug.Log($"WingsFlap: Unequipped. Restored Jumps to {_originalMaxJumps}");
        }
        
        if (_jumpAction != null)
        {
            _jumpAction.performed -= OnJumpPerformed;
        }
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        if (Manager == null || Manager.RuntimeStats == null) return;

        // Perform Check
        bool grounded = CheckGrounded();
        Debug.Log($"WingsFlap: Jump Detected. Grounded: {grounded}. Current Air Jumps Used: {_airJumpsUsed}");

        if (grounded) 
        {
            // Ground jump happening now. 
            // Reset logic
            _airJumpsUsed = 0;
            Manager.RuntimeStats.MaxJumpHeight = _originalMaxJumpHeight;
        }
        else
        {
            // Air Jump happening
            _airJumpsUsed++;
            UpdateJumpHeight();
        }
    }

    public override void OnUpdate()
    {
        // Continuous Ground Check to reset state if we land without jumping (falling)
        if (CheckGrounded())
        {
             if (_airJumpsUsed > 0)
             {
                 _airJumpsUsed = 0;
                 if (Manager != null && Manager.RuntimeStats != null)
                     Manager.RuntimeStats.MaxJumpHeight = _originalMaxJumpHeight;
             }
        }
    }

    private void UpdateJumpHeight()
    {
         float ratio = 1.0f;

         // Logic:
         // Jump 1 (Ground) -> ratio 1.0
         // Jump 2 (Air 1)  -> ratio 0.8
         // Jump 3 (Air 2)  -> ratio 0.6
         // Jump 4 (Air 3)  -> ratio 0.4
         
         if (_airJumpsUsed == 1) ratio = Flap1HeightRatio;
         else if (_airJumpsUsed == 2) ratio = Flap2HeightRatio;
         else if (_airJumpsUsed >= 3) ratio = Flap3HeightRatio;
         
         Manager.RuntimeStats.MaxJumpHeight = _originalMaxJumpHeight * ratio;
         // Debug.Log($"WingsFlap: AirJump {_airJumpsUsed}, New Height: {Manager.RuntimeStats.MaxJumpHeight}");
    }

    private bool CheckGrounded()
    {
        if (_collider == null) return false;
        
        Bounds b = _collider.bounds;
        // BoxCast slightly downwards
        // Need to be careful not to detect self. 
        // BoxCastOrigin, BoxSize, Angle, Direction, Distance, Mask
        
        // Note: BoxCastAll might be better if we want to filter triggers? 
        // But Physics2D.BoxCast usually hits nearest.
        
        RaycastHit2D hit = Physics2D.BoxCast(b.center, b.size, 0f, Vector2.down, GroundCheckDistance, GroundLayer);
        return hit.collider != null;
    }
}
