using UnityEngine;

public class AbilityPickup : MonoBehaviour
{
    [SerializeField] private AbilityItem _item;
    [SerializeField] private ParticleSystem _pickupEffect;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_item == null || _item.AbilityToUnlock == null) return;

        // Try to get AbilityManager from the player
        // Note: The player collider might be on a child or parent. 
        // We use GetComponentInParent or check root if simple GetComponent fails.
        // Assuming Standard structure where RB/Collider is on Root or near it.
        
        var abilityManager = other.GetComponent<AbilityManager>();
        if (abilityManager == null)
            abilityManager = other.GetComponentInParent<AbilityManager>();

        if (abilityManager != null)
        {
            abilityManager.UnlockAbility(_item.AbilityToUnlock);
            PickupFeedback();
            Destroy(gameObject);
        }
    }

    private void PickupFeedback()
    {
        if (_pickupEffect != null)
        {
            Instantiate(_pickupEffect, transform.position, Quaternion.identity);
        }
        // Add Sound
    }
}
