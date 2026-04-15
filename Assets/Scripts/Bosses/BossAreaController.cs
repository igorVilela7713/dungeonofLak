using System.Collections.Generic;
using UnityEngine;

public class BossAreaController : EnemyBase, IBoss
{
    [Header("Boss Movement")]
    [SerializeField] private float _moveSpeed = 1.5f;

    [Header("Danger Zones")]
    [SerializeField] private GameObject _dangerZonePrefab;
    [SerializeField] private float _attackInterval = 2f;
    [SerializeField] private float _dangerZoneDuration = 4f;
    [SerializeField] private float _dangerZoneRadius = 3f;
    [SerializeField] private int _maxDangerZones = 5;

    [Header("Projectiles")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private float _fallingProjectileInterval = 3f;

    [Header("References")]
    [SerializeField] private LayerMask _playerLayer;

    private Transform _player;
    private bool _isPhaseTwo;
    private bool _isActive;
    private readonly List<GameObject> _activeDangerZones = new List<GameObject>();

    public void Initialize(Transform player)
    {
        if (_isActive) return;
        _player = player;
        _isActive = true;
        StartCoroutine(BossBehaviorRoutine());
        StartCoroutine(FallingProjectileRoutine());
    }

    protected override void Awake()
    {
        base.Awake();
        if (_isElite)
        {
            if (_spriteRenderer != null) _spriteRenderer.color = Color.yellow;
            transform.localScale *= 1.3f;
        }
    }

    private void Update()
    {
        if (_isDead) return;

        _activeDangerZones.RemoveAll(z => z == null);

        if (!_isPhaseTwo && _currentHealth <= _maxHealth * 0.5f)
        {
            _isPhaseTwo = true;
            if (_spriteRenderer != null) _spriteRenderer.color = Color.magenta;
        }
    }

    private System.Collections.IEnumerator BossBehaviorRoutine()
    {
        while (!_isDead)
        {
            if (_player != null)
            {
                float dist = Vector2.Distance(
                    transform.position, _player.position);
                if (dist > 3f)
                {
                    float dir = Mathf.Sign(
                        _player.position.x - transform.position.x);
                    _rigidbody.linearVelocity = new Vector2(
                        dir * _moveSpeed, _rigidbody.linearVelocity.y);
                }
                else
                {
                    _rigidbody.linearVelocity = new Vector2(
                        0f, _rigidbody.linearVelocity.y);
                }
            }

            yield return new WaitForSeconds(_attackInterval);

            if (_isDead || _player == null) continue;

            SpawnDangerZone(_player.position);

            if (_isPhaseTwo)
            {
                Vector2 offset = new Vector2(
                    Random.Range(-_dangerZoneRadius, _dangerZoneRadius), 0f);
                SpawnDangerZone((Vector2)_player.position + offset);
            }
        }
    }

    private System.Collections.IEnumerator FallingProjectileRoutine()
    {
        while (!_isDead)
        {
            yield return new WaitForSeconds(_fallingProjectileInterval);

            if (_isDead || _player == null || _projectilePrefab == null) continue;

            Vector3 spawnPos = _player.position + new Vector3(0f, 8f, 0f);
            Vector2 dir = (_player.position - spawnPos).normalized;

            GameObject proj = Instantiate(
                _projectilePrefab, spawnPos, Quaternion.identity);
            EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
            if (ep != null) ep.Initialize(dir, _damageToPlayer);
        }
    }

    private void SpawnDangerZone(Vector2 position)
    {
        if (_dangerZonePrefab == null) return;
        if (_activeDangerZones.Count >= _maxDangerZones) return;

        GameObject dz = Instantiate(
            _dangerZonePrefab, position, Quaternion.identity);
        _activeDangerZones.Add(dz);

        DangerZone dzScript = dz.GetComponent<DangerZone>();
        if (dzScript != null) dzScript.Initialize(_dangerZoneDuration);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isDead) return;
        if (((1 << other.gameObject.layer) & _playerLayer) == 0) return;
        ApplyContactDamage(other);
    }
}
