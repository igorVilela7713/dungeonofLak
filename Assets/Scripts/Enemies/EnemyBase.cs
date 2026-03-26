using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] protected int _maxHealth = 30;

    [Header("Combat")]
    [SerializeField] protected int _damageToPlayer = 10;
    [SerializeField] protected float _attackCooldown = 1f;
    [SerializeField] protected float _knockbackForce = 5f;

    [Header("Elite")]
    [SerializeField] protected bool _isElite = false;

    [Header("Rune Drop")]
    [SerializeField] protected int _runeValue = 1;

    protected int _currentHealth;
    protected bool _isDead;
    protected float _attackCooldownTimer;
    protected Rigidbody2D _rigidbody;
    protected SpriteRenderer _spriteRenderer;
    protected float _healthMultiplier = 1f;
    protected float _damageMultiplier = 1f;

    public int RuneValue => _isElite ? _runeValue * 3 : _runeValue;
    public bool IsDead => _isDead;

    protected virtual void Awake()
    {
        _currentHealth = _maxHealth;
        _rigidbody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_rigidbody != null)
        {
            _rigidbody.gravityScale = 3f;
            _rigidbody.freezeRotation = true;
        }
    }

    public void ApplyDifficulty(float hpMult, float dmgMult, float runeMult = 1f)
    {
        _healthMultiplier = hpMult;
        _damageMultiplier = dmgMult;
        _maxHealth = Mathf.RoundToInt(_maxHealth * _healthMultiplier);
        _currentHealth = _maxHealth;
        _damageToPlayer = Mathf.RoundToInt(_damageToPlayer * _damageMultiplier);
        _runeValue = Mathf.Max(1, Mathf.RoundToInt(_runeValue * runeMult));
    }

    public virtual void TakeDamage(int amount)
    {
        if (_isDead) return;

        _currentHealth -= amount;
        if (_currentHealth <= 0)
        {
            _isDead = true;
            OnDeath();
        }
    }

    protected virtual void OnDeath()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        _rigidbody.linearVelocity = Vector2.zero;

        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = Color.red;
        }

        DropRune();

        Destroy(gameObject, 0.3f);
    }

    protected virtual void DropRune()
    {
        DungeonGenerator dg = FindFirstObjectByType<DungeonGenerator>();
        if (dg == null || dg.RunePickupPrefab == null) return;

        GameObject rune = Instantiate(dg.RunePickupPrefab, transform.position, Quaternion.identity);
        RunePickup pickup = rune.GetComponent<RunePickup>();
        if (pickup != null)
        {
            pickup.Initialize(RuneValue);
        }
    }

    protected virtual void ApplyContactDamage(Collider2D other)
    {
        if (_isDead) return;
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
