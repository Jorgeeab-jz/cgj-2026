using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Wizard")]
public class WizardAbilitySO : AbilitySO
{
    [Header("Wizard Settings")]
    public GameObject FireOrbPrefab;
    public GameObject IceOrbPrefab;
    [Tooltip("Speed of the orbs")]
    public float OrbSpeed = 10f;
    [Tooltip("Time between shots in seconds")]
    public float FireRate = 0.5f;

    private float _lastFireTime;
    private float _lastIceTime;

    public override void OnUpdate()
    {
        base.OnUpdate();

        // Check Primary (Fire)
        if (InputReader.IsPrimaryPressed && Time.time >= _lastFireTime + FireRate)
        {
            if (AttemptShoot(FireOrbPrefab, ref _lastFireTime))
            {
               // Success
            }
        }

        // Check Secondary (Ice)
        if (InputReader.IsSecondaryPressed && Time.time >= _lastIceTime + FireRate)
        {
            if (AttemptShoot(IceOrbPrefab, ref _lastIceTime))
            {
                // Success
            }
        }
    }

    private bool AttemptShoot(GameObject prefab, ref float lastTime)
    {
        if (prefab == null || Owner == null) return false;

        // Prevent shooting through UI
        if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return false;
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(InputReader.MousePosition);
        Vector2 spawnPos = Owner.transform.position;
        Vector2 direction = (mousePos - spawnPos).normalized;

        GameObject orb = Instantiate(prefab, spawnPos, Quaternion.identity);
        WizardOrb orbScript = orb.GetComponent<WizardOrb>();
        
        if (orbScript != null)
        {
            orbScript.Initialize(direction, OrbSpeed);
        }

        lastTime = Time.time;
        return true;
    }
}
