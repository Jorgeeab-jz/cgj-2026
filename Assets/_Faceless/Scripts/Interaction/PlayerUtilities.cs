using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerUtilities : MonoBehaviour
{
    [SerializeField] private InputActionReference _wayPointaction;
    [SerializeField] private InputActionReference _interactAction;

    private Transform _currentTeleportDestination;
    private Vector3 _respawnPoint;
    private Door _currentDoor;

    private void Start()
    {
        _respawnPoint = transform.position;
    }

    private void OnEnable()
    {
        _wayPointaction.action.performed += OnWaypointPressed;
        _interactAction.action.performed += OnInteractPressed;

        _wayPointaction.action.Enable();
        _interactAction.action.Enable();
    }

    private void OnDisable()
    {
        _wayPointaction.action.performed -= OnWaypointPressed;
        _interactAction.action.performed -= OnInteractPressed;

        _wayPointaction.action.Disable();
        _interactAction.action.Disable();
    }

    private void OnWaypointPressed (InputAction.CallbackContext ctx) 
    {
        if (_currentTeleportDestination != null)
        {
            transform.position = _currentTeleportDestination.position;
        }
    } 

    private void OnInteractPressed(InputAction.CallbackContext ctx)
    {
        if (_currentDoor != null)
        {
            _currentDoor.Interact();
        }
    }

    public void SetCurrentDoor(Door door)
    {
        _currentDoor = door;
    }

    public void SetTeleportDestination(Transform destination)
    {
        _currentTeleportDestination = destination;
    }

    public void SetRespawnPoint(Vector3 spawnPoint)
    {
        _respawnPoint = spawnPoint;
    }

    public Vector3 GetRespawnPoint()
    {
        return _respawnPoint;
    }

    public void OnPlayerDeath()
    {
        transform.position = _respawnPoint;

        Debug.Log("Player died");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<EnemyController>() != null)
        {
            OnPlayerDeath();
        }
    }
}
