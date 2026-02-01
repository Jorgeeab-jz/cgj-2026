using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private ManagerLinkerSO _managerLinker;
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _stunDuration = 1.0f;
    [SerializeField] private LayerMask _stunLayerMask;
    [SerializeField] private Rigidbody2D _rb;

    private float _stunTimer;
    private Vector2 _homePosition;
    private bool _isAggro = true;

    private void Start()
    {
        _homePosition = transform.position;
    }

    public void SetHomePosition(Vector2 position)
    {
        _homePosition = position;
    }

    public void SetAggro(bool aggro)
    {
        _isAggro = aggro;
    }

    private void FixedUpdate()
    {
        if (_stunTimer > 0)
        {
            _stunTimer -= Time.fixedDeltaTime;
            return;
        }

        if (_isAggro)
        {
            MoveTowardsPlayer();
        }
        else
        {
            MoveTowardsHome();
        }
    }

    private void MoveTowardsHome()
    {
        if (Vector2.Distance(_rb.position, _homePosition) > 0.1f)
        {
            Vector2 direction = (_homePosition - _rb.position).normalized;
            Vector2 targetPosition = _rb.position + direction * _moveSpeed * Time.fixedDeltaTime;
            _rb.MovePosition(targetPosition);
            UpdateFacingDirection(direction);
        }
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
            UpdateFacingDirection(direction);
        }
    }

    private void UpdateFacingDirection(Vector2 direction)
    {
        if (direction.x > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (direction.x < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
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
