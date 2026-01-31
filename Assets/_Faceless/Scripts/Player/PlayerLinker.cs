using UnityEngine;

public class PlayerLinker : MonoBehaviour
{
    [SerializeField] private ManagerLinkerSO _managerLinker;

    private void OnEnable()
    {
        if (_managerLinker != null)
        {
            _managerLinker.GetPlayerTransform = () => transform;
        }
    }

    private void OnDisable()
    {
        if (_managerLinker != null && _managerLinker.GetPlayerTransform != null)
        {
            // Only clear if it's currently pointing to us, though with single delegate it's safer to just set null or handle carefully.
            // For simple usage:
            _managerLinker.GetPlayerTransform = null;
        }
    }
}
