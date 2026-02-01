using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Abilities/Wizard")]
public class WizardAbilitySO : AbilitySO
{
    [Header("Wizard Settings")]
    public GameObject FireOrbPrefab;
    public GameObject IceOrbPrefab;
    [Tooltip("Speed of the orbs")]
    public float OrbSpeed = 10f;
    [Tooltip("Time between shots in seconds")]
    public float FireRate = 0.5f;

    [Tooltip("Time before the orb spawns")]
    public float CastDelay = 0.2f;
    [Tooltip("Time after spawn before player can move/shoot again")]
    public float RecoveryTime = 0.3f;

    private float _lastFireTime;
    private float _lastIceTime;
    private Animator _animator;
    private Rigidbody2D _rb;
    private bool _isCasting;
    private Coroutine _castCoroutine;
    private float _cachedRunSpeed; // To store speed during cast

    public override void Initialize(GameObject owner, AbilityManager manager, AbilityInputReader inputReader)
    {
        base.Initialize(owner, manager, inputReader);
        if (owner != null)
        {
            _animator = owner.GetComponentInChildren<Animator>();
            _rb = owner.GetComponent<Rigidbody2D>();
        }
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
        // Encapsulate cleanup to avoid stuck state
        if (_isCasting)
        {
            if (Manager != null && _castCoroutine != null) Manager.StopCoroutine(_castCoroutine);
            if (Manager != null && Manager.RuntimeStats != null) Manager.RuntimeStats.RunSpeed = _cachedRunSpeed;
            _isCasting = false;
        }
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (_isCasting) return;
        
        // Prevent casting while moving
        if (Manager != null && Manager.IsPlayerMoving()) return;

        // Check Primary (Fire)
        if (InputReader.IsPrimaryPressed && Time.time >= _lastFireTime + FireRate)
        {
            StartCast(true);
        }

        // Check Secondary (Ice)
        else if (InputReader.IsSecondaryPressed && Time.time >= _lastIceTime + FireRate)
        {
            StartCast(false);
        }
    }

    private void StartCast(bool isFire)
    {
        if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;
        if (Owner == null || Manager == null) return;

        _castCoroutine = Manager.StartCoroutine(CastRoutine(isFire));
    }

    private IEnumerator CastRoutine(bool isFire)
    {
        _isCasting = true;

        // 1. Lock Movement
        Manager.EnablePlayerMovement(false);

        // 2. Play Animation
        PlayAnimation(isFire ? "CastFire" : "CastIce");

        // 3. Wait for Cast Point
        yield return new WaitForSeconds(CastDelay);

        // 4. Spawn Orb
        GameObject prefab = isFire ? FireOrbPrefab : IceOrbPrefab;
        SpawnOrb(prefab);
        
        if (isFire) _lastFireTime = Time.time;
        else _lastIceTime = Time.time;

        // 5. Recovery Time
        yield return new WaitForSeconds(RecoveryTime);

        // 6. Return to Idle and Unlock
        Manager.EnablePlayerMovement(true);

        // Wait a frame for physics to update velocity if input is held
        yield return null;

        if (_rb != null && _rb.linearVelocity.magnitude > 0.1f)
        {
            PlayAnimation("Run");
        }
        else
        {
            PlayAnimation("Idle");
        }

        _isCasting = false;
        _castCoroutine = null;
    }

    private void SpawnOrb(GameObject prefab)
    {
        if (prefab == null) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(InputReader.MousePosition);
        Vector2 spawnPos = Owner.transform.position;
        Vector2 direction = (mousePos - spawnPos).normalized;

        GameObject orb = Instantiate(prefab, spawnPos, Quaternion.identity);
        WizardOrb orbScript = orb.GetComponent<WizardOrb>();
        
        if (orbScript != null)
        {
            orbScript.Initialize(direction, OrbSpeed);
        }
    }



    private void PlayAnimation(string animationName)
    {
        if (_animator != null)
        {
            _animator.Play(animationName);
        }
    }
}
