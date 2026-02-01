using UnityEngine;

public class EnemyZone : MonoBehaviour
{
    [SerializeField] private EnemyController _enemyPrefab;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private AudioClip _zoneMusic;
    [SerializeField] private ManagerLinkerSO _managerLinker;

    [SerializeField] private Transform _playerZoneSpawnPoint;

    private EnemyController _currentEnemy;
    private Vector3 _originalPlayerSpawnPoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var playerUtilities = other.GetComponent<PlayerUtilities>();
            if (playerUtilities != null && _playerZoneSpawnPoint != null)
            {
                _originalPlayerSpawnPoint = playerUtilities.GetRespawnPoint();
                playerUtilities.SetRespawnPoint(_playerZoneSpawnPoint.position);
            }

            if (_currentEnemy == null)
            {
                if (_enemyPrefab != null && _spawnPoint != null)
                {
                    _currentEnemy = Instantiate(_enemyPrefab, _spawnPoint.position, Quaternion.identity);
                    _currentEnemy.SetHomePosition(_spawnPoint.position);
                }
            }
            
            if (_currentEnemy != null)
            {
                _currentEnemy.SetAggro(true);
            }

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
            var playerUtilities = other.GetComponent<PlayerUtilities>();
            if (playerUtilities != null)
            {
                playerUtilities.SetRespawnPoint(_originalPlayerSpawnPoint);
            }

            if (_currentEnemy != null)
            {
                _currentEnemy.SetAggro(false);
            }

            if (_managerLinker != null)
            {
                _managerLinker.RaiseZoneExitMusicRequest();
            }
        }
    }
}
