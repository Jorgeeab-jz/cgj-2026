using UnityEngine;
using DG.Tweening;

public class Door : MonoBehaviour
{
    [SerializeField] private DoorLinker doorLinker;
    [SerializeField] private SpriteRenderer[] keyLights;
    [SerializeField] private SpriteRenderer _doorSprite;

    private void OnEnable()
    {
        if (doorLinker != null)
        {
            doorLinker.OnDoorOpened += OpenDoor;
            doorLinker.OnKeyCollected += OnKeyCollected;
        }
    }

    private void OnDisable()
    {
        if (doorLinker != null)
        {
            doorLinker.OnDoorOpened -= OpenDoor;
            doorLinker.OnKeyCollected -= OnKeyCollected;
        }
    }

    private void OnKeyCollected(int currentKeys, int requiredKeys)
    {
        int lightIndex = currentKeys - 1;

        if (lightIndex >= 0 && lightIndex < keyLights.Length)
        {
            SpriteRenderer light = keyLights[lightIndex];
            if (light != null)
            {
                
                // Using DOFloat on the material
                light.material.DOFloat(1f, "_Alpha", 0.2f)
                    .OnComplete(() => light.material.DOFloat(0.5f, "_Alpha", 0.5f));
            }
        }
    }

    private void OpenDoor()
    {
        Debug.Log("Door Opened!");
        
        _doorSprite.DOFade(0f, 0.5f);
    }
}
