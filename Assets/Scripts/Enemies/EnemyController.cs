using UnityEngine;

public class EnemyController : EnemyBase
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 2f;

    [Header("Jump Settings")]
    [SerializeField] private float _jumpForce = 8f;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask _groundLayer;

    [Header("Enemy Type")]
    [SerializeField] private EnemyType _enemyType = EnemyType.Melee;

    [Header("Ranged Settings")]
    [SerializeField] private float _preferredDistance = 5f;
    [SerializeField] private float _shootCooldown = 1.5f;
    [SerializeField] private GameObject _projectilePrefab;

    [Header("Heavy Settings")]
    [SerializeField] private float _chargeSpeed = 8f;
    [SerializeField] private float _chargeCooldown = 3f;

    [Header("References")]
    [SerializeField] private Transform _player;
    [SerializeField] private LayerMask _playerLayer;

    private bool _isGrounded;
    private float _shootTimer;
    private float _chargeTimer;
    private bool _isCharging;
    private float _noHitTimer;

    protected override void Awake()
    {
        base.Awake();

        if (_groundCheck == null)
        {
            GameObject go = new GameObject("GroundCheck");
            go.transform.SetParent(transform);
            go.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            _groundCheck = go.transform;
        }

        if (_isElite)
        {
            if (_spriteRenderer != null) _spriteRenderer.color = Color.yellow;
            transform.localScale *= 1.3f;

            switch (_enemyType)
            {
                case EnemyType.Ranged:
                    _shootCooldown *= 0.6f;
                    break;
                case EnemyType.Heavy:
                    _knockbackForce *= 1.5f;
                    break;
                case EnemyType.Fast:
                    _moveSpeed *= 1.3f;
                    break;
            }
        }
    }

    public void Initialize(Transform player)
    {
        _player = player;
    }

    private void Update()
    {
        if (_isDead) return;

        if (_attackCooldownTimer > 0) _attackCooldownTimer -= Time.deltaTime;
        if (_shootTimer > 0) _shootTimer -= Time.deltaTime;
        if (_chargeTimer > 0) _chargeTimer -= Time.deltaTime;

        _isGrounded = Physics2D.OverlapCircle(
            _groundCheck.position, _groundCheckRadius, _groundLayer);

        switch (_enemyType)
        {
            case EnemyType.Melee:
                ChasePlayer();
                break;
            case EnemyType.Fast:
                ChasePlayerAggressive();
                break;
            case EnemyType.Heavy:
                HeavyBehavior();
                break;
            case EnemyType.Ranged:
                RangedBehavior();
                break;
        }
    }

    private void ChasePlayer()
    {
        if (_player == null) return;

        float directionX = Mathf.Sign(_player.position.x - transform.position.x);
        _rigidbody.linearVelocity = new Vector2(
            directionX * _moveSpeed, _rigidbody.linearVelocity.y);

        if (_player.position.y > transform.position.y + 1f && _isGrounded)
        {
            _rigidbody.linearVelocity = new Vector2(
                _rigidbody.linearVelocity.x, _jumpForce);
        }
    }

    private void ChasePlayerAggressive()
    {
        if (_player == null) return;

        float directionX = Mathf.Sign(_player.position.x - transform.position.x);
        _rigidbody.linearVelocity = new Vector2(
            directionX * _moveSpeed, _rigidbody.linearVelocity.y);
    }

    private void HeavyBehavior()
    {
        if (_player == null) return;

        if (_isCharging) return;

        float dist = Vector2.Distance(transform.position, _player.position);

        if (dist < 3f && _chargeTimer <= 0)
        {
            StartCoroutine(ChargeRoutine());
            return;
        }

        float directionX = Mathf.Sign(_player.position.x - transform.position.x);
        _rigidbody.linearVelocity = new Vector2(
            directionX * _moveSpeed, _rigidbody.linearVelocity.y);
    }

    private System.Collections.IEnumerator ChargeRoutine()
    {
        _isCharging = true;
        _chargeTimer = _chargeCooldown;

        float dir = Mathf.Sign(_player.position.x - transform.position.x);
        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            _rigidbody.linearVelocity = new Vector2(
                dir * _chargeSpeed, _rigidbody.linearVelocity.y);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _isCharging = false;
    }

    private void RangedBehavior()
    {
        if (_player == null) return;

        float dist = Vector2.Distance(transform.position, _player.position);

        if (dist > _preferredDistance + 1f)
        {
            float dir = Mathf.Sign(_player.position.x - transform.position.x);
            _rigidbody.linearVelocity = new Vector2(
                dir * _moveSpeed, _rigidbody.linearVelocity.y);
        }
        else if (dist < _preferredDistance - 1f)
        {
            float dir = -Mathf.Sign(_player.position.x - transform.position.x);
            _rigidbody.linearVelocity = new Vector2(
                dir * _moveSpeed * 1.5f, _rigidbody.linearVelocity.y);
        }
        else
        {
            _rigidbody.linearVelocity = new Vector2(
                0f, _rigidbody.linearVelocity.y);

            if (_shootTimer <= 0)
            {
                ShootProjectile();
                _shootTimer = _shootCooldown;
            }
        }

        _noHitTimer += Time.deltaTime;
        if (_noHitTimer > 5f)
        {
            float dir = Mathf.Sign(_player.position.x - transform.position.x);
            _rigidbody.linearVelocity = new Vector2(
                dir * _moveSpeed, _rigidbody.linearVelocity.y);
        }
    }

    private void ShootProjectile()
    {
        if (_projectilePrefab == null || _player == null) return;

        Vector2 dir = (_player.position - transform.position).normalized;
        GameObject proj = Instantiate(
            _projectilePrefab, transform.position, Quaternion.identity);

        EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
        if (ep != null)
        {
            ep.Initialize(dir, _damageToPlayer, this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isDead) return;
        if (((1 << other.gameObject.layer) & _playerLayer) == 0) return;
        if (_attackCooldownTimer > 0) return;

        ApplyContactDamage(other);
    }

    public void OnProjectileHit()
    {
        _noHitTimer = 0f;
    }
}
