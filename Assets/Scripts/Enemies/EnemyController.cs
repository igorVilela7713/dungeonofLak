using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyController : MonoBehaviour, IDamageable
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 2f;

    [Header("Jump Settings")]
    [SerializeField] private float _jumpForce = 8f;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask _groundLayer;

    [Header("Combat Settings")]
    [SerializeField] private int _maxHealth = 30;
    [SerializeField] private int _damageToPlayer = 10;
    [SerializeField] private float _attackCooldown = 1f;
    [SerializeField] private float _knockbackForce = 5f;

    [Header("References")]
    [SerializeField] private Transform _player;

    private int _currentHealth;
    private bool _isDead;
    private bool _isGrounded;
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
            _rigidbody.gravityScale = 3f;
            _rigidbody.freezeRotation = true;
        }

        if (_groundCheck == null)
        {
            GameObject go = new GameObject("GroundCheck");
            go.transform.SetParent(transform);
            go.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            _groundCheck = go.transform;
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

        _isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);

        ChasePlayer();
    }

    private void ChasePlayer()
    {
        if (_player == null) return;

        float directionX = Mathf.Sign(_player.position.x - transform.position.x);
        _rigidbody.linearVelocity = new Vector2(directionX * _moveSpeed, _rigidbody.linearVelocity.y);

        if (_player.position.y > transform.position.y + 1f && _isGrounded)
        {
            _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, _jumpForce);
        }
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
