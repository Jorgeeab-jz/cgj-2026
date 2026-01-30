using UnityEngine;

public class IceOrb : WizardOrb
{
    protected override void HandleCollision(Collider2D hit)
    {
        // Try to find IFreezable component on the hit object or its parent
        IFreezable freezable = hit.GetComponent<IFreezable>();
        if (freezable == null)
        {
            freezable = hit.GetComponentInParent<IFreezable>();
        }

        if (freezable != null)
        {
            freezable.Freeze();
        }
    }
}
