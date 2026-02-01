using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Abilities/Telekinesis")]
public class TelekinesisAbilitySO : AbilitySO
{
    [Header("Telekinesis Settings")]
    public float GrabRange = 10f;
    public float ThrowForce = 15f;
    public LayerMask GrabbableLayers;
    public float SmoothTime = 10f; // Joint frequency/damping factor
    
    [Header("Drag Physics")]
    public float BaseDragSpeed = 20f; // Constant speed for all objects
    public float HeldObjectDrag = 50f; // High drag to stop momentum immediately

    private TargetJoint2D _joint;
    private Rigidbody2D _grabbedRB;
    private BreakableObject _grabbedBreakable;
    private Vector2 _currentJointTarget;
    private Vector2 _lastMouseWorldPos;
    
    // State backup
    private float _originalDrag;
    
    // Player Stats Backup
    private float _originalWalkSpeed;
    private float _originalRunSpeed;
    private float _originalAirAccel;
    private float _originalRunAirAccel;

    // We track ignored colliders to restore them later
    private System.Collections.Generic.List<Collider2D> _ignoredColliders = new System.Collections.Generic.List<Collider2D>();

    private Animator _animator;
    private Rigidbody2D _rb;

    public override void Initialize(GameObject owner, AbilityManager manager, AbilityInputReader inputReader)
    {
        base.Initialize(owner, manager, inputReader);
        if (owner != null)
        {
            _animator = owner.GetComponentInChildren<Animator>();
            _rb = owner.GetComponent<Rigidbody2D>();
        }
    }

    public override void OnEquip()
    {
        InputReader.OnPrimaryActionChanged += HandleGrabInput;
        InputReader.OnSecondaryActionChanged += HandleBreakInput;
    }

    public override void OnUnequip()
    {
        InputReader.OnPrimaryActionChanged -= HandleGrabInput;
        InputReader.OnSecondaryActionChanged -= HandleBreakInput;
        ReleaseObject();
    }

    public override void OnUpdate()
    {
        if (_joint != null && _grabbedRB != null)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(InputReader.MousePosition);
            _lastMouseWorldPos = mousePos;

            // Use constant speed regardless of mass
            float speed = BaseDragSpeed;
            _currentJointTarget = Vector2.MoveTowards(_currentJointTarget, mousePos, speed * Time.deltaTime);

            _joint.target = _currentJointTarget;
        }
    }

    private void HandleGrabInput(bool isPressed)
    {
        // Block input if clicking on UI (but allow releasing existing grab)
        if (isPressed && UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (isPressed)
        {
            TryGrab();
        }
        else
        {
            ReleaseObject();
        }
    }

    private void HandleBreakInput(bool isPressed)
    {
        if (isPressed && _grabbedBreakable != null)
        {
            _grabbedBreakable.Break();
            ReleaseObject();
        }
    }

    private void TryGrab()
    {
        if (_grabbedRB != null) return;

        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(InputReader.MousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, 0f, GrabbableLayers);

        if (hit.collider != null && hit.rigidbody != null)
        {
            _grabbedRB = hit.rigidbody;
            _grabbedBreakable = hit.collider.GetComponent<BreakableObject>();

            // Backup Object State
            _originalDrag = _grabbedRB.linearDamping;

            // Apply "Held" Physics
            _grabbedRB.linearDamping = HeldObjectDrag;
            
            // --- PLAYER MOVEMENT FREEZE ---
            Manager.EnablePlayerMovement(false);

            // --- COLLISION EXCLUSION LOGIC ---
            // Instead of excludeLayers (which ignores walls if on Default), we use IgnoreCollision.

            // 1. Ignore Player Colliders
            if (Owner != null)
            {
                Collider2D[] ownerColliders = Owner.GetComponentsInChildren<Collider2D>();
                foreach (var ownerCol in ownerColliders)
                {
                    Physics2D.IgnoreCollision(ownerCol, hit.collider, true);
                    _ignoredColliders.Add(ownerCol);
                }
            }

            _joint = _grabbedRB.gameObject.AddComponent<TargetJoint2D>();
            _joint.anchor = _grabbedRB.transform.InverseTransformPoint(hit.point);
            
            _currentJointTarget = hit.point;
            _lastMouseWorldPos = mouseWorldPos; // Initialize to prevent zero-delta on first frame
            
            _joint.target = _currentJointTarget;
            
            _joint.maxForce = 1000f * _grabbedRB.mass; 
            _joint.frequency = 10f;
            _joint.dampingRatio = 1f;
            
            Cursor.visible = false;

            if (_animator != null)
            {
                _animator.Play("CastTelekinesis");
            }
        }
    }

    private void ReleaseObject()
    {
        if (_joint != null)
        {
            Destroy(_joint);
            _joint = null;
        }
        
        // Restore Physics State
        if (_grabbedRB != null)
        {
            _grabbedRB.linearDamping = _originalDrag;
            
            // Restore Collisions
            Collider2D grabbedCol = _grabbedRB.GetComponent<Collider2D>();
            
            if (grabbedCol != null)
            {
                foreach (var ignoredCol in _ignoredColliders)
                {
                    if (ignoredCol != null) 
                        Physics2D.IgnoreCollision(ignoredCol, grabbedCol, false);
                }
            }
        }
        
        // Restore Player Movement
        Manager.EnablePlayerMovement(true);

        _ignoredColliders.Clear();
        _grabbedRB = null;
        _grabbedBreakable = null;
        Cursor.visible = true;

        if (Manager != null)
        {
            Manager.StartCoroutine(RestoreAnimationRoutine());
        }
    }

    private IEnumerator RestoreAnimationRoutine()
    {
        // Wait a frame for physics to update velocity if input is held
        yield return null;

        if (_rb != null && _rb.linearVelocity.magnitude > 0.1f)
        {
            if (_animator != null) _animator.Play("Run");
        }
        else
        {
            if (_animator != null) _animator.Play("Idle");
        }
    }
}
