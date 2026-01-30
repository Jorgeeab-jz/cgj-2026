using UnityEngine;

public abstract class WizardOrb : MonoBehaviour
{
    [SerializeField] protected float Speed = 10f;
    [SerializeField] protected float LifeTime = 5f;
    [SerializeField] protected LayerMask CollisionLayers;
    [SerializeField] protected GameObject HitEffectPrefab;

    protected Rigidbody2D Rb;

    public virtual void Initialize(Vector2 direction, float speed)
    {
        Speed = speed;
        Rb = GetComponent<Rigidbody2D>();
        if (Rb != null)
        {
            Rb.linearVelocity = direction * Speed;
        }
        
        Destroy(gameObject, LifeTime);
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore if layer is not in CollisionLayers (optional check if using collision matrix)
        if (((1 << other.gameObject.layer) & CollisionLayers) == 0) 
        {
            Debug.Log($"[WizardOrb] Ignored object {other.gameObject.name} on layer {LayerMask.LayerToName(other.gameObject.layer)} due to CollisionLayers mask.");
            return;
        }

        HandleCollision(other);
        
        if (HitEffectPrefab != null)
        {
            Instantiate(HitEffectPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & CollisionLayers) == 0) return;
        
        HandleCollision(collision.collider);

        if (HitEffectPrefab != null)
        {
            Instantiate(HitEffectPrefab, collision.contacts[0].point, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    protected abstract void HandleCollision(Collider2D hit);
}
