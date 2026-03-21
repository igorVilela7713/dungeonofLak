using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private int _damage = 10;
    [SerializeField] private float _attackCooldown = 0.3f;
    [SerializeField] private float _attackDuration = 0.1f;
    [SerializeField] private GameObject _hitboxPrefab;
    [SerializeField] private Transform _player;
    
    private bool _isAttacking;
    private float _cooldownTimer;
    
    public bool IsAttacking => _isAttacking;
    
    private void Update()
    {
        if (_cooldownTimer > 0)
        {
            _cooldownTimer -= Time.deltaTime;
        }
        
        if (Input.GetButtonDown("Fire1"))
        {
            Attack();
        }
    }
    
    public void Attack()
    {
        if (_isAttacking) return;
        if (_cooldownTimer > 0) return;
        
        _isAttacking = true;
        SpawnHitbox();
        
        StartCoroutine(AttackRoutine());
    }
    
    private System.Collections.IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(_attackDuration);
        _isAttacking = false;
        _cooldownTimer = _attackCooldown;
    }
    
    private void SpawnHitbox()
    {
        Vector3 spawnPos = _player.position + _player.right * 0.8f;
        GameObject hitbox = Instantiate(_hitboxPrefab, spawnPos, Quaternion.identity, _player);
        SwordHitbox swordHitbox = hitbox.GetComponent<SwordHitbox>();
        swordHitbox.Initialize(this);
    }
    
    public void OnHitboxTrigger(Collider2D other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(_damage);
        }
    }
}
