using UnityEngine;

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

    private float _lastTeleportTime;
    private Rigidbody2D _ownerRb;

    public override void Initialize(GameObject owner, AbilityManager manager, AbilityInputReader inputReader)
    {
        base.Initialize(owner, manager, inputReader);
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
        _ownerRb = null;
    }

    private void HandleTeleportInput(bool isPressed)
    {
        if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (isPressed && Time.time >= _lastTeleportTime + Cooldown)
        {
            PerformTeleport();
        }
    }

    private void PerformTeleport()
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
