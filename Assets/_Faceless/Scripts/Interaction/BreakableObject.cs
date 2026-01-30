using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    [SerializeField] private ParticleSystem _breakEffect;
    
    public void Break()
    {
        if (_breakEffect != null)
        {
            Instantiate(_breakEffect, transform.position, Quaternion.identity);
        }

        // Add sound or other logic here
        
        Destroy(gameObject);
    }
}
