using UnityEngine;

public class WayPoint : MonoBehaviour
{
    [SerializeField] private Transform _destination;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Player entered waypoint trigger.");

        if (other.TryGetComponent<PlayerUtilities>(out var player))
        {
            if (_destination != null)
            {
                player.SetTeleportDestination(_destination);
            }
            else
            {
                Debug.LogWarning($"Waypoint destination is missing on object {gameObject.name}!");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerUtilities>(out var player))
        {
            player.SetTeleportDestination(null);
        }
    }
}
