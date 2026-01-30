using UnityEngine;

public class FireOrb : WizardOrb
{
    protected override void HandleCollision(Collider2D hit)
    {
        // Try to find IBurnable component on the hit object or its parent
        IBurnable burnable = hit.GetComponent<IBurnable>();
        if (burnable == null)
        {
            burnable = hit.GetComponentInParent<IBurnable>();
        }

        if (burnable != null)
        {
            burnable.Burn();

            Debug.Log("[FireOrb] Burned an object with Fire Orb.");
        }
    }
}
