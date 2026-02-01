using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerUtilities : MonoBehaviour
{
    [SerializeField] private InputActionReference _wayPointaction;

    private Transform _currentTeleportDestination;
    private Vector3 _respawnPoint;

    private void Start()
    {
        _respawnPoint = transform.position;
    }

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

    public void SetRespawnPoint(Vector3 spawnPoint)
    {
        _respawnPoint = spawnPoint;
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
