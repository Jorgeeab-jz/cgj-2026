using UnityEngine;

public class MusicZone : MonoBehaviour
{
    [SerializeField] private AudioClip _zoneMusic;
    [SerializeField] private ManagerLinkerSO _managerLinker;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (_managerLinker != null && _zoneMusic != null)
            {
                _managerLinker.RaiseZoneEnterMusicRequest(_zoneMusic);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (_managerLinker != null && _zoneMusic != null)
            {
                _managerLinker.RaiseZoneExitMusicRequest(_zoneMusic);
            }
        }
    }
}
