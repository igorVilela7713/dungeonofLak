using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float _speed = 6f;
    [SerializeField] private int _damage = 8;
    [SerializeField] private float _lifetime = 3f;
    [SerializeField] private LayerMask _enemyLayer;

    private Rigidbody2D _rb;
    private bool _initialized;
    private EnemyController _owner;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (_rb != null) _rb.gravityScale = 0f;
    }

    public void Initialize(Vector2 direction, int damage, EnemyController owner = null)
    {
        _damage = damage;
        _owner = owner;
        _initialized = true;

        if (_rb != null)
        {
            _rb.linearVelocity = direction.normalized * _speed;
        }

        Destroy(gameObject, _lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_initialized) return;

        if (((1 << other.gameObject.layer) & _enemyLayer) != 0) return;

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(_damage);

            if (_owner != null)
            {
                _owner.OnProjectileHit();
            }
        }

        Destroy(gameObject);
    }
}
