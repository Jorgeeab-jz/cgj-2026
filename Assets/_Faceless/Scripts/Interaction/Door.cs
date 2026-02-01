using UnityEngine;
using DG.Tweening;

public class Door : MonoBehaviour
{
    [SerializeField] private DoorLinker doorLinker;
    [SerializeField] private SpriteRenderer[] keyLights;
    [SerializeField] private SpriteRenderer _doorSprite;
    [SerializeField] private Collider2D _interactionCollider;
    [SerializeField] private GameObject _levelCompleteCanvas;
    [SerializeField] private GameObject _doorPrompt;

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

        if (_interactionCollider != null)
        {
             _interactionCollider.enabled = true;
        }

        _doorPrompt.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerUtilities>(out var player))
        {
            player.SetCurrentDoor(this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerUtilities>(out var player))
        {
            player.SetCurrentDoor(null);
        }
    }

    public void Interact()
    {
        if (_levelCompleteCanvas != null)
        {
            _levelCompleteCanvas.SetActive(true);
            Time.timeScale = 0;
        }
    }
}
