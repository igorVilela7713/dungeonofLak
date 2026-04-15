using UnityEngine;

public class BossMeleeController : EnemyBase, IBoss
{
    [Header("Boss Movement")]
    [SerializeField] private float _moveSpeed = 3f;

    [Header("Attack Pattern")]
    [SerializeField] private int _attackComboCount = 3;
    [SerializeField] private float _attackInterval = 0.4f;
    [SerializeField] private float _comboCooldown = 1.5f;
    [SerializeField] private float _attackRange = 1.5f;

    [Header("Jump")]
    [SerializeField] private float _jumpForce = 8f;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask _groundLayer;

    [Header("References")]
    [SerializeField] private LayerMask _playerLayer;

    private Transform _player;
    private bool _isGrounded;
    private bool _isEnraged;
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

        _isGrounded = Physics2D.OverlapCircle(
            _groundCheck.position, _groundCheckRadius, _groundLayer);

        if (!_isEnraged && _currentHealth <= _maxHealth * 0.3f)
        {
            _isEnraged = true;
            _moveSpeed *= 1.5f;
            _comboCooldown *= 0.5f;
            if (_spriteRenderer != null) _spriteRenderer.color = Color.red;
        }
    }

    private System.Collections.IEnumerator BossBehaviorRoutine()
    {
        while (!_isDead)
        {
            if (_player == null) { yield return null; continue; }

            while (!_isDead && Vector2.Distance(
                transform.position, _player.position) > _attackRange)
            {
                if (_player == null) break;
                float dir = Mathf.Sign(_player.position.x - transform.position.x);
                _rigidbody.linearVelocity = new Vector2(
                    dir * _moveSpeed, _rigidbody.linearVelocity.y);
                if (_player.position.y > transform.position.y + 1f && _isGrounded)
                {
                    _rigidbody.linearVelocity = new Vector2(
                        _rigidbody.linearVelocity.x, _jumpForce);
                }
                yield return null;
            }

            _rigidbody.linearVelocity = new Vector2(0f, _rigidbody.linearVelocity.y);

            for (int i = 0; i < _attackComboCount && !_isDead; i++)
            {
                AttackHit();
                yield return new WaitForSeconds(_attackInterval);
            }

            yield return new WaitForSeconds(_comboCooldown);
        }
    }

    private void AttackHit()
    {
        Collider2D hit = Physics2D.OverlapCircle(
            transform.position, _attackRange, _playerLayer);
        if (hit != null) ApplyContactDamage(hit);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isDead) return;
        if (((1 << other.gameObject.layer) & _playerLayer) == 0) return;
        ApplyContactDamage(other);
    }
}
