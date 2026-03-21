using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyController : MonoBehaviour, IDamageable
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 2f;

    [Header("Combat Settings")]
    [SerializeField] private int _maxHealth = 30;
    [SerializeField] private int _damageToPlayer = 10;
    [SerializeField] private float _attackCooldown = 1f;
    [SerializeField] private float _knockbackForce = 5f;

    [Header("References")]
    [SerializeField] private Transform _player;

    private int _currentHealth;
    private bool _isDead;
    private float _attackCooldownTimer;
    private Rigidbody2D _rigidbody;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _currentHealth = _maxHealth;
        _rigidbody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_rigidbody != null)
        {
            _rigidbody.gravityScale = 0;
            _rigidbody.freezeRotation = true;
        }
    }

    public void Initialize(Transform player)
    {
        _player = player;
    }

    private void Update()
    {
        if (_isDead) return;

        if (_attackCooldownTimer > 0)
        {
            _attackCooldownTimer -= Time.deltaTime;
        }

        ChasePlayer();
    }

    private void ChasePlayer()
    {
        if (_player == null) return;

        Vector2 direction = (_player.position - transform.position).normalized;
        _rigidbody.linearVelocity = direction * _moveSpeed;
    }

    public void TakeDamage(int amount)
    {
        if (_isDead) return;

        _currentHealth -= amount;
        if (_currentHealth <= 0)
        {
            _isDead = true;
            StartCoroutine(DieSequence());
        }
    }

    private System.Collections.IEnumerator DieSequence()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        _rigidbody.linearVelocity = Vector2.zero;

        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = Color.red;
        }

        yield return new WaitForSeconds(0.3f);

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isDead) return;
        if (other.gameObject.layer != LayerMask.NameToLayer("player")) return;
        if (_attackCooldownTimer > 0) return;

        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc != null && pc.IsInvincible) return;

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(_damageToPlayer);
            _attackCooldownTimer = _attackCooldown;

            if (pc != null)
            {
                Vector2 knockDir = (other.transform.position - transform.position).normalized;
                pc.ApplyKnockback(knockDir, _knockbackForce);
            }
        }
    }
}
