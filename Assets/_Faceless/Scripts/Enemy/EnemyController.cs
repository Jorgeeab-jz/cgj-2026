using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private ManagerLinkerSO _managerLinker;
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _stunDuration = 1.0f;
    [SerializeField] private LayerMask _stunLayerMask;
    [SerializeField] private Rigidbody2D _rb;

    private float _stunTimer;

    private void FixedUpdate()
    {
        if (_stunTimer > 0)
        {
            _stunTimer -= Time.fixedDeltaTime;
            return;
        }

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & _stunLayerMask) != 0)
        {
            _stunTimer = _stunDuration;
            Destroy(collision.gameObject);
        }
    }
}
