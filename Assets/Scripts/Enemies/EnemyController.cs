using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyController : MonoBehaviour, IDamageable
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 2f;
    
    [Header("Combat Settings")]
    [SerializeField] private int _maxHealth = 30;
    [SerializeField] private int _damageToPlayer = 10;
    [SerializeField] private float _attackCooldown = 1f;
    
    [Header("References")]
    [SerializeField] private Transform _player;
    
    private int _currentHealth;
    private bool _isDead;
    private float _attackCooldownTimer;
    
    private void Awake()
    {
        _currentHealth = _maxHealth;
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
        transform.position += (Vector3)direction * _moveSpeed * Time.deltaTime;
    }
    
    public void TakeDamage(int amount)
    {
        if (_isDead) return;
        
        _currentHealth -= amount;
        if (_currentHealth <= 0)
        {
            _isDead = true;
            Destroy(gameObject);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("player")) return;
        if (_attackCooldownTimer > 0) return;
        
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(_damageToPlayer);
            _attackCooldownTimer = _attackCooldown;
        }
    }
}
