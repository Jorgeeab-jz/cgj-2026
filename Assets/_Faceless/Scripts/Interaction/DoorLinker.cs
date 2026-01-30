using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "DoorLinker", menuName = "Faceless/DoorLinker")]
public class DoorLinker : ScriptableObject
{
    [SerializeField] private int keysRequired = 3;
    [SerializeField] private int keysCollected = 0;

    public UnityAction<int, int> OnKeyCollected;
    public UnityAction OnDoorOpened;

    private void OnEnable()
    {
        keysCollected = 0;
    }

    public void AddKey()
    {
        keysCollected++;
        OnKeyCollected?.Invoke(keysCollected, keysRequired);

        if (keysCollected >= keysRequired)
        {
            OnDoorOpened?.Invoke();
        }
    }

    public void Reset()
    {
        keysCollected = 0;
    }
}
