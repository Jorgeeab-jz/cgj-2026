using UnityEngine;

public class Key : MonoBehaviour
{
    [SerializeField] private DoorLinker doorLinker;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Assuming the player has a specific tag or component. 
        // For now, checking for "Player" tag is a common standard, 
        // but we can also check for a PlayerController component.
        if (other.CompareTag("Player"))
        {
            if (doorLinker != null)
            {
                doorLinker.AddKey();
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("DoorLinker not assigned on Key: " + gameObject.name);
            }
        }
    }
}
