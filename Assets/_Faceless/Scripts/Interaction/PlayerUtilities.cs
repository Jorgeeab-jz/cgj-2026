using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerUtilities : MonoBehaviour
{
    [SerializeField] private InputActionReference _wayPointaction;

    private Transform _currentTeleportDestination;

    private void OnEnable()
    {
        _wayPointaction.action.performed += OnWaypointPressed;

        _wayPointaction.action.Enable();
    }

    private void OnDisable()
    {
        _wayPointaction.action.performed -= OnWaypointPressed;

        _wayPointaction.action.Disable();
    }

    private void OnWaypointPressed (InputAction.CallbackContext ctx) 
    {
        if (_currentTeleportDestination != null)
        {
            transform.position = _currentTeleportDestination.position;
        }
    } 

    public void SetTeleportDestination(Transform destination)
    {
        _currentTeleportDestination = destination;
    }
}
