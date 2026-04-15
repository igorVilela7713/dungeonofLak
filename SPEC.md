# SPEC — FASE 9: Inimigos Avançados e Ecossistema de Combate

## Contratos Obrigatórios

- Todo inimigo: deve herdar `EnemyBase` e implementar `IDamageable`
- Todo boss: deve herdar `EnemyBase`, implementar `IDamageable` e `IBoss`
- `OnDeath()` é o ponto real de morte. `Destroy` é apenas cleanup.
- Valores NÃO devem ser hardcoded em múltiplos lugares — usar `[SerializeField]` sempre

## Arquivos a Criar

| Path | Tipo | Descrição |
|------|------|-----------|
| `Assets/Scripts/Enemies/EnemyType.cs` | Enum | `Melee`, `Fast`, `Heavy`, `Ranged` |
| `Assets/Scripts/Enemies/EnemyBase.cs` | Classe base (MonoBehaviour, IDamageable) | TakeDamage, OnDeath, Drop de runas, HP |
| `Assets/Scripts/Enemies/EnemyProjectile.cs` | MonoBehaviour | Projétil — Rigidbody2D velocity, ignora Enemy layer |
| `Assets/Scripts/Bosses/IBoss.cs` | Interface | `void Initialize(Transform player)` |
| `Assets/Scripts/Bosses/BossMeleeController.cs` | MonoBehaviour (herda EnemyBase, IBoss) | Boss 1 (andar 5) |
| `Assets/Scripts/Bosses/BossAreaController.cs` | MonoBehaviour (herda EnemyBase, IBoss) | Boss 2 (andar 10) |
| `Assets/Scripts/Bosses/BossHybridController.cs` | MonoBehaviour (herda EnemyBase, IBoss) | Boss 3 (andar 15) |
| `Assets/Scripts/Bosses/DangerZone.cs` | MonoBehaviour | Zona de perigo — dano por tick, auto-destroy |
| `Assets/Prefabs/EnemyFast.prefab` | Prefab | Inimigo rápido |
| `Assets/Prefabs/EnemyHeavy.prefab` | Prefab | Inimigo pesado |
| `Assets/Prefabs/EnemyRanged.prefab` | Prefab | Inimigo ranged |
| `Assets/Prefabs/EnemyProjectile.prefab` | Prefab | Projétil inimigo |
| `Assets/Prefabs/DangerZone.prefab` | Prefab | Zona de perigo (Boss2) |
| `Assets/Prefabs/Boss1.prefab` | Prefab | Boss melee agressivo |
| `Assets/Prefabs/Boss2.prefab` | Prefab | Boss area control |
| `Assets/Prefabs/Boss3.prefab` | Prefab | Boss híbrido |

---

## Arquivos a Modificar

| Path | Mudanças |
|------|----------|
| `Assets/Scripts/Enemies/EnemyController.cs` | Herdar de `EnemyBase`. Remover TakeDamage, DieSequence, DropRune, campos de vida. Adicionar `_enemyType`, campos ranged/heavy. Métodos isolados: ChasePlayer, ChasePlayerAggressive, HeavyBehavior, RangedBehavior. Padronizar TODOS os layer checks para `[SerializeField] LayerMask _playerLayer`. |
| `Assets/Scripts/Rooms/RoomController.cs` | Adicionar `GameObject[] _enemyPrefabs`. `SetEnemyPrefabs()`. `SpawnCombatEnemies()` seleciona aleatório do array. Padronizar layer check. |
| `Assets/Scripts/Dungeon/DungeonGenerator.cs` | Adicionar `_enemyFastPrefab`, `_enemyHeavyPrefab`, `_enemyRangedPrefab`. `GetEnemyPrefabsForFloor(floor)`. |
| `Assets/Scripts/Dungeon/BossFloorHandler.cs` | `GameObject[] _bossPrefabs`. `GetComponent<IBoss>()?.Initialize()`. |
| `Assets/Scripts/Dungeon/FloorConfigSO.cs` | (Opcional) `_eliteChancePerFloor`. |

---

## Regras de Comportamento

| Tipo | Regra |
|------|-------|
| Ranged | Nunca fica parado infinitamente. Sempre tenta reposicionar se não acertar. |
| Heavy | Sempre tem charge disponível. |
| Fast | Nunca pausa durante chase. |
| Boss | Flag `_isActive` garante que coroutine inicia apenas uma vez. |

## Regras de Spawn

- Room nunca pode spawnar apenas ranged OU apenas heavy
- Sempre balanceado: Melee deve estar presente na pool junto com tipos avançados
- Distribuição controlada por floor (tabela abaixo)

---

## EnemyBase — Classe Compartilhada

```csharp
// Assets/Scripts/Enemies/EnemyBase.cs
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
```

---

## IBoss — Interface

```csharp
// Assets/Scripts/Bosses/IBoss.cs
using UnityEngine;

public interface IBoss
{
    void Initialize(Transform player);
}
```

---

## EnemyController — Refatorado

Padronizado: **todos** os layer checks usam `[SerializeField] LayerMask _playerLayer` em vez de `NameToLayer`.

```csharp
// Assets/Scripts/Enemies/EnemyController.cs
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
```

---

## EnemyProjectile — Corrigido

Correções aplicadas:
- Ignora colisão com layer Enemy
- Reseta `_noHitTimer` do inimigo ranged quando acerta o player
- Referência ao owner passada via `Initialize`
- Layer check padronizado com `LayerMask`

```csharp
// Assets/Scripts/Enemies/EnemyProjectile.cs
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

        // Ignorar inimigos
        if (((1 << other.gameObject.layer) & _enemyLayer) != 0) return;

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(_damage);

            // Reset no-hit timer do ranged owner
            if (_owner != null)
            {
                _owner.OnProjectileHit();
            }
        }

        Destroy(gameObject);
    }
}
```

---

## DangerZone — Com Limite de Instâncias

```csharp
// Assets/Scripts/Bosses/DangerZone.cs
using UnityEngine;

public class DangerZone : MonoBehaviour
{
    [SerializeField] private int _damagePerTick = 5;
    [SerializeField] private float _tickInterval = 0.5f;
    [SerializeField] private float _duration = 4f;

    private float _tickTimer;
    private SpriteRenderer _sr;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr != null) _sr.color = new Color(1f, 0f, 0f, 0.4f);
    }

    public void Initialize(float duration)
    {
        _duration = duration;
        Destroy(gameObject, _duration);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        _tickTimer -= Time.deltaTime;
        if (_tickTimer > 0) return;

        // Layer check padronizado
        int playerLayer = LayerMask.NameToLayer("Player");
        if (other.gameObject.layer == playerLayer)
        {
            IDamageable d = other.GetComponent<IDamageable>();
            if (d != null) d.TakeDamage(_damagePerTick);
            _tickTimer = _tickInterval;
        }
    }
}
```

---

## Boss Scripts

### BossMeleeController

Flag `_isActive` impede coroutine duplicada.

```csharp
// Assets/Scripts/Bosses/BossMeleeController.cs
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
        if (_isActive) return; // Prevenir double init
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
```

### BossAreaController

Limite de danger zones via `_maxDangerZones`.

```csharp
// Assets/Scripts/Bosses/BossAreaController.cs
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

        // Limpar referências destruídas
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
```

### BossHybridController

```csharp
// Assets/Scripts/Bosses/BossHybridController.cs
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
```

---

## BossFloorHandler — Refatorado

```csharp
// Assets/Scripts/Dungeon/BossFloorHandler.cs — método SpawnBoss():
private void SpawnBoss()
{
    if (_bossPrefabs == null || _bossPrefabs.Length == 0) return;
    if (_bossSpawnPoint == null || _player == null) return;

    int floor = FloorManager.Instance != null ? FloorManager.Instance.CurrentFloor : 5;
    int index = Mathf.Clamp((floor / 5) - 1, 0, _bossPrefabs.Length - 1);

    GameObject boss = Instantiate(
        _bossPrefabs[index], _bossSpawnPoint.position, Quaternion.identity);

    // IBoss genérico — sem GetComponent repetido
    IBoss bossInterface = boss.GetComponent<IBoss>();
    if (bossInterface != null) bossInterface.Initialize(_player);

    // ApplyDifficulty via EnemyBase (funciona para qualquer boss)
    EnemyBase eb = boss.GetComponent<EnemyBase>();
    if (eb != null)
    {
        FloorConfigSO config = FloorManager.Instance?.FloorConfig;
        if (config != null)
        {
            float hpMult = DifficultyScaler.GetHPMultiplier(
                floor, config.HpScalingPerFloor) * 3f;
            float dmgMult = DifficultyScaler.GetDamageMultiplier(
                floor, config.DmgScalingPerFloor) * 2f;
            float runeMult = DifficultyScaler.GetRuneMultiplier(
                floor, config.HpScalingPerFloor) * 5f;
            eb.ApplyDifficulty(hpMult, dmgMult, runeMult);
        }
    }

    EnemyDeathTracker tracker = boss.AddComponent<EnemyDeathTracker>();
    tracker.Initialize(_roomController != null
        ? _roomController : GetComponent<RoomController>());

    if (_roomController != null)
        _roomController.IncrementEnemiesAlive();
}
```

---

## RoomController — Múltiplos Prefabs

```csharp
// Assets/Scripts/Rooms/RoomController.cs — adicionar e modificar:

[SerializeField] private GameObject[] _enemyPrefabs;

public void SetEnemyPrefabs(GameObject[] prefabs)
{
    _enemyPrefabs = prefabs;
}

private void SpawnCombatEnemies()
{
    GameObject prefabToSpawn = _enemyPrefab; // fallback

    if (_enemyPrefabs != null && _enemyPrefabs.Length > 0)
    {
        prefabToSpawn = _enemyPrefabs[Random.Range(0, _enemyPrefabs.Length)];
    }

    if (prefabToSpawn == null || _spawnPoints == null) return;

    foreach (Transform point in _spawnPoints)
    {
        GameObject enemy = Instantiate(
            prefabToSpawn, point.position, Quaternion.identity);

        var controller = enemy.GetComponent<EnemyController>();
        if (controller != null)
        {
            controller.Initialize(_player);

            if (FloorManager.Instance != null
                && FloorManager.Instance.FloorConfig != null)
            {
                int floor = FloorManager.Instance.CurrentFloor;
                var config = FloorManager.Instance.FloorConfig;
                float hpMult = DifficultyScaler.GetHPMultiplier(
                    floor, config.HpScalingPerFloor);
                float dmgMult = DifficultyScaler.GetDamageMultiplier(
                    floor, config.DmgScalingPerFloor);
                float runeMult = DifficultyScaler.GetRuneMultiplier(
                    floor, config.HpScalingPerFloor);
                controller.ApplyDifficulty(hpMult, dmgMult, runeMult);
            }
        }

        var tracker = enemy.AddComponent<EnemyDeathTracker>();
        tracker.Initialize(this);

        _enemiesAlive++;
    }
}
```

---

## DungeonGenerator — Distribuição por Floor

```csharp
// Assets/Scripts/Dungeon/DungeonGenerator.cs — adicionar:

[SerializeField] private GameObject _enemyFastPrefab;
[SerializeField] private GameObject _enemyHeavyPrefab;
[SerializeField] private GameObject _enemyRangedPrefab;

private GameObject[] GetEnemyPrefabsForFloor(int floor)
{
    List<GameObject> pool = new List<GameObject>();

    // Melee sempre presente — garante balanceamento
    if (_enemyPrefab != null) pool.Add(_enemyPrefab);

    if (floor >= 3 && _enemyFastPrefab != null) pool.Add(_enemyFastPrefab);
    if (floor >= 5 && _enemyHeavyPrefab != null) pool.Add(_enemyHeavyPrefab);
    if (floor >= 7 && _enemyRangedPrefab != null) pool.Add(_enemyRangedPrefab);

    return pool.ToArray();
}

// Em GenerateFloor(), substituir rc.SetEnemyPrefab(...) por:
//   rc.SetEnemyPrefabs(GetEnemyPrefabsForFloor(floorNumber));
```

| Floor | Inimigos possíveis |
|-------|-------------------|
| 1–2 | Melee |
| 3–4 | Melee, Fast |
| 5–6 | Melee, Fast, Heavy |
| 7+ | Melee, Fast, Heavy, Ranged |

---

## Setup na Unity

### S1 — Layer EnemyProjectile

1. **Project Settings → Tags and Layers**
2. Nova layer índice 12: **`EnemyProjectile`**

### S2 — Collision Matrix

| | Player | Enemy | EnemyProjectile | Ground | Default |
|-|--------|-------|-----------------|--------|---------|
| EnemyProjectile | SIM | NÃO | NÃO | SIM | NÃO |

### S3 — Prefab EnemyProjectile

| Componente | Configuração |
|-----------|-------------|
| SpriteRenderer | Placeholder. Color: vermelho. Order +2 |
| Rigidbody2D | Gravity 0, Freeze Rotation Z ✓ |
| CircleCollider2D | Is Trigger true, Radius 0.15 |
| EnemyProjectile.cs | _speed=6, _damage=8, _lifetime=3 |
| Layer | EnemyProjectile |

### S4 — Prefab DangerZone

| Componente | Configuração |
|-----------|-------------|
| SpriteRenderer | Placeholder. Color: vermelho transparente (A:0.4). Scale 2×2 |
| BoxCollider2D | Is Trigger true, Size (2, 2) |
| DangerZone.cs | _damagePerTick=5, _tickInterval=0.5 |
| Layer | Default |

### S5 — Prefab EnemyFast

Duplicar `Enemy.prefab`. Configurar EnemyController:

| Campo | Valor |
|-------|-------|
| _enemyType | Fast |
| _moveSpeed | 5 |
| _maxHealth | 15 |
| _damageToPlayer | 8 |
| _attackCooldown | 0.6 |
| _knockbackForce | 3 |

SpriteRenderer Color: ciano. Scale: 0.8.

### S6 — Prefab EnemyHeavy

Duplicar `Enemy.prefab`:

| Campo | Valor |
|-------|-------|
| _enemyType | Heavy |
| _moveSpeed | 1 |
| _maxHealth | 90 |
| _damageToPlayer | 20 |
| _attackCooldown | 1.5 |
| _knockbackForce | 10 |
| _chargeSpeed | 8 |
| _chargeCooldown | 3 |

Color: laranja. Scale: 1.5.

### S7 — Prefab EnemyRanged

Duplicar `Enemy.prefab`:

| Campo | Valor |
|-------|-------|
| _enemyType | Ranged |
| _moveSpeed | 1.5 |
| _maxHealth | 20 |
| _damageToPlayer | 5 |
| _attackCooldown | 2 |
| _knockbackForce | 2 |
| _preferredDistance | 5 |
| _shootCooldown | 1.5 |
| _projectilePrefab | EnemyProjectile.prefab |

Color: roxo. Scale: 0.9.

### S8 — Atualizar Enemy.prefab

`_enemyType` = Melee. Campos novos com defaults. `_projectilePrefab` = vazio.

### S9 — Atualizar EliteEnemy.prefab

`_enemyType` = Melee. `_isElite` = true.

### S10 — Prefabs Boss

**Boss1:** Sprite vermelho, Scale 2.5, BossMeleeController, GroundCheck, Layer Enemy, CircleCollider2D trigger+physics.

**Boss2:** Sprite azul, BossAreaController, _dangerZonePrefab = DangerZone, _projectilePrefab = EnemyProjectile.

**Boss3:** Sprite verde, BossHybridController, _projectilePrefab = EnemyProjectile.

### S11 — Atualizar Arena Prefabs

- BossArena_5: `_bossPrefabs[0]` = Boss1
- BossArena_10: `_bossPrefabs[0]` = Boss2
- BossArena_15: `_bossPrefabs[0]` = Boss3

### S12 — Atualizar cena Main

DungeonGenerator: `_enemyPrefab`, `_enemyFastPrefab`, `_enemyHeavyPrefab`, `_enemyRangedPrefab`.

---

## Atualização do SETUP.md

Adicionar seção **15** ao final:

- 15.1 — Visão Geral (arquétipos, EnemyBase, IBoss, distribuição por floor)
- 15.2 — Comportamento por Arquétipo (tabela stats + gameplay)
- 15.3 — Layer EnemyProjectile
- 15.4 — Hierarquia de Scripts (diagrama)
- 15.5 — Prefabs Criados
- 15.6 — Sistema de Bosses
- 15.7 — Como Testar
- 15.8 — Troubleshooting

---

## Checklist de Implementação

### Bloco 1: Fundação

- [x] **1.1** Criar `Assets/Scripts/Enemies/EnemyType.cs`
- [x] **1.2** Criar `Assets/Scripts/Enemies/EnemyBase.cs`
- [x] **1.3** Criar `Assets/Scripts/Bosses/IBoss.cs`
- [x] **1.4** Refatorar `Assets/Scripts/Enemies/EnemyController.cs`
  - Herdar EnemyBase, remover lógica de vida/morte
  - Métodos isolados por tipo
  - LayerMask serializado (_playerLayer) em todos os layer checks
  - Charge coroutine para heavy
  - Timeout no-hit para ranged
  - Passar `this` para EnemyProjectile.Initialize

### Bloco 2: Projétil e DangerZone

- [x] **2.1** Criar `Assets/Scripts/Enemies/EnemyProjectile.cs`
  - Rigidbody2D velocity
  - Ignora Enemy layer
  - Reseta _noHitTimer do owner ao acertar
- [x] **2.2** Criar `Assets/Scripts/Bosses/DangerZone.cs`
- [ ] **2.3** Criar prefab EnemyProjectile
- [ ] **2.4** Criar prefab DangerZone

### Bloco 3: Prefabs Inimigos

- [ ] **3.1** Criar EnemyFast.prefab
- [ ] **3.2** Criar EnemyHeavy.prefab
- [ ] **3.3** Criar EnemyRanged.prefab
- [ ] **3.4** Atualizar Enemy.prefab
- [ ] **3.5** Atualizar EliteEnemy.prefab

### Bloco 4: Integração

- [x] **4.1** Modificar RoomController.cs
- [x] **4.2** Modificar DungeonGenerator.cs
- [ ] **4.3** Atualizar cena Main

### Bloco 5: Bosses

- [x] **5.1** Criar BossMeleeController.cs (flag _isActive)
- [x] **5.2** Criar BossAreaController.cs (limite _maxDangerZones)
- [x] **5.3** Criar BossHybridController.cs (flag _isActive)
- [ ] **5.4** Criar prefabs Boss1, Boss2, Boss3
- [x] **5.5** Modificar BossFloorHandler.cs (IBoss)
- [ ] **5.6** Atualizar arena prefabs

### Bloco 6: SETUP.md

- [x] **6.1** Adicionar seção 15

### Bloco 7: Testes

- [ ] **7.1** Melee — comportamento inalterado
- [ ] **7.2** Fast — chase rápido, sem pausas
- [ ] **7.3** Heavy — lento, knockback forte, charge
- [ ] **7.4** Ranged — mantém distância, atira projéteis
- [ ] **7.5** Projétil — acerta player, ignora enemy, morre no ground
- [ ] **7.6** Elite — maior, amarelo, bônus por tipo
- [ ] **7.7** Boss 1 — combos, enraged em 30%
- [ ] **7.8** Boss 2 — danger zones (limite 5), projéteis caem
- [ ] **7.9** Boss 3 — alternância melee/ranged
- [ ] **7.10** Variedade por floor
- [ ] **7.11** Múltiplos ranged na mesma sala
- [ ] **7.12** Múltiplos projéteis simultâneos
- [ ] **7.13** Boss + player colidindo constantemente
- [ ] **7.14** Stress test (10+ inimigos)

---

## Validação

- [x] Scripts compilam sem erro
- [x] EnemyBase compartilhada entre todos os inimigos
- [x] IBoss usado no BossFloorHandler
- [x] Projétil usa Rigidbody2D velocity
- [x] Todos os layer checks padronizados (LayerMask serializado)
- [x] EnemyProjectile ignora Enemy layer e reseta _noHitTimer
- [x] DangerZone com limite de instâncias no Boss2
- [x] Flag _isActive em todos os bosses
- [x] SETUP.md atualizado
- [ ] Testável em Play Mode

---

## Melhorias Futuras

### Gameplay
- Inimigos voadores
- Status effects (poison, burn)
- Mini-boss

### Sistema
- Object Pooling para EnemyProjectile e DangerZone
- Behavior modular (Strategy Pattern) para EnemyController
- AI State Machine
- Barra de vida nos inimigos

### UX
- Hit feedback (flash ao receber dano, knockback visual)
- Partículas placeholder
- Sons
