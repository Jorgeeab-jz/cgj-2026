using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private ManagerLinkerSO _managerLinker;
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private Rigidbody2D _rb;

    private void FixedUpdate()
    {
        MoveTowardsPlayer();
    }

    private void MoveTowardsPlayer()
    {
        if (_managerLinker == null || _managerLinker.GetPlayerTransform == null) return;

        Transform playerTransform = _managerLinker.GetPlayerTransform.Invoke();

        if (playerTransform != null)
        {
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            Vector2 targetPosition = _rb.position + direction * _moveSpeed * Time.fixedDeltaTime;
            _rb.MovePosition(targetPosition);
        }
    }
}
