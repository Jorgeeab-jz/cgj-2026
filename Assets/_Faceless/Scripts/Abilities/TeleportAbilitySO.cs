using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Abilities/Teleport")]
public class TeleportAbilitySO : AbilitySO
{
    [Header("Teleport Settings")]
    [Tooltip("Maximum distance the player can teleport.")]
    public float MaxTeleportDistance = 10f;
    
    [Tooltip("Layers that block the teleport (walls, ground, etc).")]
    public LayerMask ObstacleLayers;

    [Tooltip("Offset applied when hitting a wall to prevent spawning inside it. Should roughly match player radius.")]
    public float CollisionOffset = 0.5f;

    [Tooltip("Cooldown in seconds between teleports.")]
    public float Cooldown = 0.5f;

    [Header("Animation Settings")]
    public float StartDelay = 0.1f;
    public float EndDelay = 0.1f;

    private float _lastTeleportTime;
    private Rigidbody2D _ownerRb;
    private Animator _animator;
    private bool _isTeleporting;
    private float _cachedRunSpeed;
    private Coroutine _teleportCoroutine;

    public override void Initialize(GameObject owner, AbilityManager manager, AbilityInputReader inputReader)
    {
        base.Initialize(owner, manager, inputReader);
        if (owner != null)
        {
            _animator = owner.GetComponentInChildren<Animator>();
        }
    }

    public override void OnEquip()
    {
        if (Owner != null)
        {
            _ownerRb = Owner.GetComponent<Rigidbody2D>();
        }
        InputReader.OnPrimaryActionChanged += HandleTeleportInput;
    }

    public override void OnUnequip()
    {
        InputReader.OnPrimaryActionChanged -= HandleTeleportInput;
        
        // Cleanup if ability is swapped mid-teleport
        if (_isTeleporting)
        {
             if (Manager != null && _teleportCoroutine != null) Manager.StopCoroutine(_teleportCoroutine);
             if (Manager != null && Manager.RuntimeStats != null) Manager.RuntimeStats.RunSpeed = _cachedRunSpeed;
             _isTeleporting = false;
        }

        _ownerRb = null;
    }

    private void HandleTeleportInput(bool isPressed)
    {
        if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (isPressed && Time.time >= _lastTeleportTime + Cooldown && !_isTeleporting)
        {
            if (Manager != null)
            {
               _teleportCoroutine = Manager.StartCoroutine(TeleportRoutine());
            }
        }
    }

    private IEnumerator TeleportRoutine()
    {
        _isTeleporting = true;
        
        // 1. Lock Movement
        Manager.EnablePlayerMovement(false);

        // 2. Start Anim
        if (_animator != null) _animator.Play("TeleportStart");

        // 3. Wait for Disappear
        yield return new WaitForSeconds(StartDelay);

        // 4. Move
        PerformTeleportStep();

        // 5. End Anim
        if (_animator != null) _animator.Play("TeleportEnd");

        // 6. Wait for Reappear
        yield return new WaitForSeconds(EndDelay);

        // 7. Restore & Safety Check
        Manager.EnablePlayerMovement(true);

        yield return null; // Wait for physics

        if (_ownerRb != null && _ownerRb.linearVelocity.magnitude > 0.1f)
        {
             if (_animator != null) _animator.Play("Run");
        }
        else
        {
             if (_animator != null) _animator.Play("Idle");
        }

        _isTeleporting = false;
        _teleportCoroutine = null;
    }

    private void PerformTeleportStep()
    {
        if (Owner == null) return;

        Vector2 mouseScreenPos = InputReader.MousePosition;
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        Vector2 playerPos = Owner.transform.position;

        Vector2 direction = (mouseWorldPos - playerPos).normalized;
        float distance = Vector2.Distance(playerPos, mouseWorldPos);

        // 1. Clamp Distance
        if (distance > MaxTeleportDistance)
        {
            distance = MaxTeleportDistance;
        }

        Vector2 targetPos = playerPos + (direction * distance);

        // 2. Check overlap at pending target
        // We do a small overlap circle to ensure we aren't teleporting INTO a wall.
        // Radius can be small, or match collision offset.
        bool isTargetClear = Physics2D.OverlapCircle(targetPos, CollisionOffset * 0.5f, ObstacleLayers) == null;

        if (!isTargetClear)
        {
            // If the explicit target is blocked, we fall back to the Raycast behavior
            // (Teleport to the first hit point minus offset)
             RaycastHit2D hit = Physics2D.Raycast(playerPos, direction, distance, ObstacleLayers);

            if (hit.collider != null)
            {
                targetPos = hit.point - (direction * CollisionOffset);
            }
        }

        // 3. Move Player
        if (_ownerRb != null)
        {
            _ownerRb.position = targetPos;
            _ownerRb.linearVelocity = Vector2.zero; 
        }
        else
        {
            Owner.transform.position = targetPos;
        }

        _lastTeleportTime = Time.time;
        
        Debug.Log($"Teleported to {targetPos}");
    }
}
