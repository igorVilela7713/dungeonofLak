using UnityEngine;

public class BossHybridController : EnemyBase, IBoss
{
    [Header("Boss Movement")]
    [SerializeField] private float _moveSpeed = 2.5f;

    [Header("Phase Settings")]
    [SerializeField] private float _phaseTwoHPPercent = 0.5f;

    [Header("Melee")]
    [SerializeField] private float _meleeAttackCooldown = 0.8f;
    [SerializeField] private float _meleeRange = 1.5f;

    [Header("Ranged")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private float _shootCooldown = 1.2f;
    [SerializeField] private float _preferredDistance = 6f;
    [SerializeField] private float _spreadAngle = 15f;

    [Header("Jump")]
    [SerializeField] private float _jumpForce = 8f;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask _groundLayer;

    [Header("References")]
    [SerializeField] private LayerMask _playerLayer;

    private Transform _player;
    private bool _isPhaseTwo;
    private bool _isGrounded;
    private float _shootTimer;
    private float _meleeTimer;
    private bool _isActive;

    public void Initialize(Transform player)
    {
        if (_isActive) return;
        _player = player;
        _isActive = true;
        StartCoroutine(BossBehaviorRoutine());
    }

    protected override void Awake()
    {
        base.Awake();
        if (_groundCheck == null)
        {
            GameObject go = new GameObject("GroundCheck");
            go.transform.SetParent(transform);
            go.transform.localPosition = new Vector3(0f, -0.8f, 0f);
            _groundCheck = go.transform;
        }
        if (_isElite)
        {
            if (_spriteRenderer != null) _spriteRenderer.color = Color.yellow;
            transform.localScale *= 1.3f;
        }
    }

    private void Update()
    {
        if (_isDead) return;
        if (_attackCooldownTimer > 0) _attackCooldownTimer -= Time.deltaTime;
        if (_shootTimer > 0) _shootTimer -= Time.deltaTime;
        if (_meleeTimer > 0) _meleeTimer -= Time.deltaTime;

        _isGrounded = Physics2D.OverlapCircle(
            _groundCheck.position, _groundCheckRadius, _groundLayer);

        if (!_isPhaseTwo && _currentHealth <= _maxHealth * _phaseTwoHPPercent)
        {
            _isPhaseTwo = true;
            _moveSpeed *= 0.7f;
            if (_spriteRenderer != null) _spriteRenderer.color = Color.red;
        }
    }

    private System.Collections.IEnumerator BossBehaviorRoutine()
    {
        while (!_isDead)
        {
            if (_player == null) { yield return null; continue; }

            if (!_isPhaseTwo)
                yield return MeleePhase();
            else
                yield return RangedPhase();
        }
    }

    private System.Collections.IEnumerator MeleePhase()
    {
        while (!_isDead && _player != null &&
               Vector2.Distance(transform.position, _player.position) > _meleeRange)
        {
            float dir = Mathf.Sign(_player.position.x - transform.position.x);
            _rigidbody.linearVelocity = new Vector2(
                dir * _moveSpeed, _rigidbody.linearVelocity.y);
            if (_player.position.y > transform.position.y + 1f && _isGrounded)
            {
                _rigidbody.linearVelocity = new Vector2(
                    _rigidbody.linearVelocity.x, _jumpForce);
            }
            if (_isPhaseTwo) yield break;
            yield return null;
        }

        _rigidbody.linearVelocity = new Vector2(0f, _rigidbody.linearVelocity.y);

        if (_meleeTimer <= 0 && _player != null)
        {
            Collider2D hit = Physics2D.OverlapCircle(
                transform.position, _meleeRange, _playerLayer);
            if (hit != null) ApplyContactDamage(hit);
            _meleeTimer = _meleeAttackCooldown;
        }

        yield return new WaitForSeconds(_meleeAttackCooldown);
    }

    private System.Collections.IEnumerator RangedPhase()
    {
        float elapsed = 0f;
        float phaseDuration = 4f;

        while (elapsed < phaseDuration && !_isDead)
        {
            if (_player == null) yield break;

            float dist = Vector2.Distance(
                transform.position, _player.position);

            if (dist < _preferredDistance - 1f)
            {
                float dir = -Mathf.Sign(
                    _player.position.x - transform.position.x);
                _rigidbody.linearVelocity = new Vector2(
                    dir * _moveSpeed, _rigidbody.linearVelocity.y);
            }
            else
            {
                _rigidbody.linearVelocity = new Vector2(
                    0f, _rigidbody.linearVelocity.y);
            }

            if (_shootTimer <= 0)
            {
                ShootSpread();
                _shootTimer = _shootCooldown;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void ShootSpread()
    {
        if (_projectilePrefab == null || _player == null) return;

        Vector2 baseDir = (_player.position - transform.position).normalized;
        float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;

        float[] angles = { 0f, _spreadAngle, -_spreadAngle };

        for (int i = 0; i < angles.Length; i++)
        {
            float angle = (baseAngle + angles[i]) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            GameObject proj = Instantiate(
                _projectilePrefab, transform.position, Quaternion.identity);
            EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
            if (ep != null) ep.Initialize(dir, _damageToPlayer);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isDead) return;
        if (((1 << other.gameObject.layer) & _playerLayer) == 0) return;
        ApplyContactDamage(other);
    }
}
