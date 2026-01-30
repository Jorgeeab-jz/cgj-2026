using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Abilities/WingsFlap")]
public class WingsFlapAbilitySO : AbilitySO
{
    [Header("Wings Flap Settings")]
    [SerializeField] private int TotalJumps = 4; 
    [SerializeField] private float _timeToJumpApex = 0.6f;

    [Tooltip("Percent of Max Jump Height for each consecutive air jump")]
    public float Flap1HeightRatio = 0.8f;
    public float Flap2HeightRatio = 0.6f;
    public float Flap3HeightRatio = 0.4f;

    [Header("Detection")]
    public LayerMask GroundLayer;
    public float GroundCheckDistance = 0.1f;
    public string JumpActionName = "Jump";

    // References
    private float _originalMaxJumpHeight;
    private int _originalMaxJumps;
    private int _airJumpsUsed = 0;
    private float _defaultTimeToJumpApex;

    // Runtime Components
    private InputAction _jumpAction;
    private Collider2D _collider;
    
    private bool _initialized;

    public override void Initialize(GameObject owner, AbilityManager manager, AbilityInputReader inputReader)
    {
        base.Initialize(owner, manager, inputReader);
        _collider = owner.GetComponent<Collider2D>();

        _defaultTimeToJumpApex = Manager.BaseStats.TimeTillJumpApex;
        _originalMaxJumpHeight = Manager.BaseStats.MaxJumpHeight;
        _originalMaxJumps = (int)Manager.BaseStats.MaxNumberJumps;

        var playerInput = owner.GetComponent<PlayerInput>();
        if (playerInput != null)
        {
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

    private void OnEnable()
    {
        OnUnequip();
    }

    private void OnDisable()
    {
        OnUnequip();
    }

    public override void OnEquip()
    {
        if (!_initialized) return;

        Manager.RuntimeStats.MaxNumberJumps = TotalJumps;
        Manager.RuntimeStats.TimeTillJumpApex = _timeToJumpApex;
    }


    public override void OnUnequip()
    {
        if (Manager != null && Manager.RuntimeStats != null)
        {
            // Restore
            Manager.RuntimeStats.MaxNumberJumps = _originalMaxJumps;
            Manager.RuntimeStats.TimeTillJumpApex = _defaultTimeToJumpApex;
        }
        
        if (_jumpAction != null)
        {
            _jumpAction.performed -= OnJumpPerformed;
        }
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        if (Manager == null || Manager.RuntimeStats == null) return;

        bool grounded = CheckGrounded();
        Debug.Log($"WingsFlap: Jump Detected. Grounded: {grounded}. Current Air Jumps Used: {_airJumpsUsed}");

        if (grounded) 
        {
            _airJumpsUsed = 0;
            Manager.RuntimeStats.MaxJumpHeight = _originalMaxJumpHeight;
        }
        else
        {
            _airJumpsUsed++;
            UpdateJumpHeight();
        }
    }

    public override void OnUpdate()
    {
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
         
         if (_airJumpsUsed == 1) ratio = Flap1HeightRatio;
         else if (_airJumpsUsed == 2) ratio = Flap2HeightRatio;
         else if (_airJumpsUsed >= 3) ratio = Flap3HeightRatio;
         
         Manager.RuntimeStats.MaxJumpHeight = _originalMaxJumpHeight * ratio;
    }

    private bool CheckGrounded()
    {
        if (_collider == null) return false;
        
        Bounds b = _collider.bounds;
        
        RaycastHit2D hit = Physics2D.BoxCast(b.center, b.size, 0f, Vector2.down, GroundCheckDistance, GroundLayer);
        return hit.collider != null;
    }
}
