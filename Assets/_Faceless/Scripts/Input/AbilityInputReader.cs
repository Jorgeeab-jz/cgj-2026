using UnityEngine;
using UnityEngine.InputSystem;
using System;

[CreateAssetMenu(fileName = "AbilityInputReader", menuName = "Input/AbilityInputReader")]
public class AbilityInputReader : ScriptableObject, AbilityInputActions.IAbilityActions
{
    private AbilityInputActions _inputActions;

    public event Action<bool> OnPrimaryActionChanged;
    public event Action<bool> OnSecondaryActionChanged;
    public event Action<Vector2> OnMousePositionChanged;

    public Vector2 MousePosition { get; private set; }
    public bool IsPrimaryPressed { get; private set; }
    public bool IsSecondaryPressed { get; private set; }

    private void OnEnable()
    {
        if (_inputActions == null)
        {
            _inputActions = new AbilityInputActions();
            _inputActions.Ability.SetCallbacks(this);
        }
        _inputActions.Enable();
    }

    private void OnDisable()
    {
        _inputActions?.Disable();
    }

    public void OnPrimaryAction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            IsPrimaryPressed = true;
            OnPrimaryActionChanged?.Invoke(true);
        }
        else if (context.canceled)
        {
            IsPrimaryPressed = false;
            OnPrimaryActionChanged?.Invoke(false);
        }
    }

    public void OnSecondaryAction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            IsSecondaryPressed = true;
            OnSecondaryActionChanged?.Invoke(true);
        }
        else if (context.canceled)
        {
            IsSecondaryPressed = false;
            OnSecondaryActionChanged?.Invoke(false);
        }
    }

    public void OnMousePosition(InputAction.CallbackContext context)
    {
        MousePosition = context.ReadValue<Vector2>();
        OnMousePositionChanged?.Invoke(MousePosition);
    }
}
