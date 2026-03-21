# SPEC — Fase 4 + 5: Knockback, I-frames e Morte do Inimigo

## 1. Arquivos a Modificar

Nenhum arquivo novo. Modificar 2 scripts existentes:

```
Assets/Scripts/
├── Enemies/
│   └── EnemyController.cs      # MODIFICAR — knockback no player, Rigidbody2D, die sequence
└── Movement/
    └── PlayerController.cs     # MODIFICAR — knockback, i-frames, flash
```

---

## 2. Estrutura de Classes

### EnemyController (modificar)

Responsabilidade: perseguir player, aplicar dano + knockback ao colidir, morrer com sequência visual.

```
Campos a adicionar:
- _rigidbody (Rigidbody2D)       — cache em Awake, substitui transform.position
- _knockbackForce (float)        — força de knockback aplicada no player, default 5
- _spriteRenderer (SpriteRenderer) — cache em Awake para flash na morte

Campos existentes (sem alteração):
- _moveSpeed, _maxHealth, _damageToPlayer, _attackCooldown, _player
- _currentHealth, _isDead, _attackCooldownTimer

Métodos a modificar:
- Awake()                          — cache Rigidbody2D, SpriteRenderer
- ChasePlayer()                    — usar _rigidbody.linearVelocity em vez de transform.position
- OnTriggerEnter2D()               — adicionar check de IsInvincible + chamada de ApplyKnockback
- TakeDamage()                     — chamar DieSequence() em vez de Destroy direto

Métodos a adicionar:
- DieSequence() : IEnumerator      — desativa collider, flash vermelho, espera 0.3s, Destroy
```

### PlayerController (modificar)

Responsabilidade: receber knockback, ter período de invencibilidade com flash visual.

```
Campos a adicionar:
- _knockbackForce (float)          — força default do knockback, default 5
- _invincibilityDuration (float)   — duração dos i-frames, default 0.5
- _isInvincible (bool)             — flag de invencibilidade

Campos existentes (sem alteração):
- _moveSpeed, _jumpForce, _groundCheck, _groundCheckRadius, _groundLayer
- _rigidbody, _facingDirection, _horizontalInput, _isGrounded
- _moveAction, _jumpAction

Propriedades a adicionar:
- IsInvincible → bool              — expõe _isInvincible para EnemyController verificar

Métodos a adicionar:
- ApplyKnockback(Vector2, float)   — aplica force no rigidbody, inicia InvincibilityRoutine
- InvincibilityRoutine() : IEnumerator — seta _isInvincible, flash sprite, espera, reseta
```

---

## 3. Fluxo Lógico

### Fluxo 1: Enemy dá dano no player (com knockback)

```
1. EnemyController.Update()
       ↓
2. ChasePlayer() → _rigidbody.linearVelocity = direction * speed
       ↓
3. Enemy colide com Player → OnTriggerEnter2D(other)
       ↓
4. Verifica: other.layer == "player"?
       ↓ NÃO → ignora
5. Verifica: _attackCooldownTimer <= 0?
       ↓ NÃO → ignora
6. Verifica: PlayerController.IsInvincible?
       ↓ SIM → ignora (não aplica dano)
7. damageable.TakeDamage(_damageToPlayer)
       ↓
8. _attackCooldownTimer = _attackCooldown
       ↓
9. PlayerController.ApplyKnockback(knockDir, _knockbackForce)
       ↓
10. Rigidbody2D.AddForce(direction * force, Impulse)  → player é empurrado
        ↓
11. InvincibilityRoutine() inicia
        ↓
12. _isInvincible = true
        ↓
13. Flash vermelho/branco 4x (0.08s cada)
        ↓
14. _isInvincible = false
```

### Fluxo 2: Player mata enemy

```
1. WeaponController.OnHitboxTrigger(other)
       ↓
2. IDamageable.TakeDamage(damage)
       ↓
3. EnemyController.TakeDamage(amount)
       ↓
4. _currentHealth -= amount
       ↓
5. _currentHealth > 0? → retorna (inimigo vivo)
   _currentHealth <= 0?
       ↓
6. _isDead = true
       ↓
7. StartCoroutine(DieSequence())
       ↓
8. Collider2D.enabled = false        → para de receber hits e causar dano
       ↓
9. SpriteRenderer.color = Color.red  → flash vermelho
       ↓
10. WaitForSeconds(0.3s)
        ↓
11. Destroy(gameObject)
        ↓
12. EnemyDeathTracker.OnDestroy() → RoomController.OnEnemyKilled()
```

### Fluxo 3: Enemy com Rigidbody2D (movimento)

```
1. EnemyController.Awake()
       ↓
2. _rigidbody = GetComponent<Rigidbody2D>()
   _rigidbody.gravityScale = 0       → sem gravidade (top-down/2D sem queda)
   _rigidbody.freezeRotation = true
       ↓
3. Update() → ChasePlayer()
       ↓
4. direction = (_player.position - transform.position).normalized
   _rigidbody.linearVelocity = direction * _moveSpeed
       ↓
5. Quando ApplyKnockback é chamado no player:
   Rigidbody2D.AddForce() sobrescreve temporariamente o velocity
   PlayerController.Update() restaura velocity no próximo frame
```

---

## 4. Pseudocódigo Detalhado

### EnemyController.cs (arquivo completo)

```csharp
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
        // Para de interagir
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Para de se mover
        _rigidbody.linearVelocity = Vector2.zero;

        // Flash vermelho
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

        // Verificar i-frames do player
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc != null && pc.IsInvincible) return;

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(_damageToPlayer);
            _attackCooldownTimer = _attackCooldown;

            // Knockback no player
            if (pc != null)
            {
                Vector2 knockDir = (other.transform.position - transform.position).normalized;
                pc.ApplyKnockback(knockDir, _knockbackForce);
            }
        }
    }
}
```

### PlayerController.cs (arquivo completo)

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;

    [Header("Jump Settings")]
    [SerializeField] private float _jumpForce = 10f;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask _groundLayer;

    [Header("Combat")]
    [SerializeField] private float _knockbackForce = 5f;
    [SerializeField] private float _invincibilityDuration = 0.5f;

    private Rigidbody2D _rigidbody;
    private Vector2 _facingDirection = Vector2.right;
    private float _horizontalInput;
    private bool _isGrounded;
    private bool _isInvincible;
    private InputAction _moveAction;
    private InputAction _jumpAction;

    public Vector2 FacingDirection => _facingDirection;
    public bool IsInvincible => _isInvincible;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.gravityScale = 3f;
        _rigidbody.freezeRotation = true;

        _moveAction = new InputAction("Move", InputActionType.Value);
        _moveAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/a")
            .With("Negative", "<Keyboard>/leftArrow")
            .With("Positive", "<Keyboard>/d")
            .With("Positive", "<Keyboard>/rightArrow");
        _moveAction.AddBinding("<Gamepad>/leftStick/x");

        _jumpAction = new InputAction("Jump", InputActionType.Button);
        _jumpAction.AddBinding("<Keyboard>/w");
        _jumpAction.AddBinding("<Keyboard>/upArrow");
        _jumpAction.AddBinding("<Keyboard>/space");
        _jumpAction.AddBinding("<Gamepad>/buttonSouth");
    }

    private void OnEnable()
    {
        _moveAction.Enable();
        _jumpAction.Enable();
        _jumpAction.performed += OnJumpPerformed;
    }

    private void OnDisable()
    {
        _jumpAction.performed -= OnJumpPerformed;
        _moveAction.Disable();
        _jumpAction.Disable();
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (_isGrounded)
        {
            _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, _jumpForce);
        }
    }

    private void FixedUpdate()
    {
        _isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);
    }

    private void Update()
    {
        _horizontalInput = _moveAction.ReadValue<float>();
        _rigidbody.linearVelocity = new Vector2(_horizontalInput * _moveSpeed, _rigidbody.linearVelocity.y);

        if (_horizontalInput != 0)
        {
            _facingDirection = new Vector2(_horizontalInput > 0 ? 1 : -1, 0);
        }
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        _rigidbody.AddForce(direction * force, ForceMode2D.Impulse);
        StartCoroutine(InvincibilityRoutine());
    }

    private System.Collections.IEnumerator InvincibilityRoutine()
    {
        _isInvincible = true;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            for (int i = 0; i < 4; i++)
            {
                sr.color = Color.red;
                yield return new WaitForSeconds(0.08f);
                sr.color = Color.white;
                yield return new WaitForSeconds(0.08f);
            }
        }
        else
        {
            yield return new WaitForSeconds(_invincibilityDuration);
        }

        _isInvincible = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (_groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
        }
    }
}
```

---

## 5. Integração na Unity

### Enemy.prefab — verificação no Inspector

```
Enemy (GameObject)
├── Rigidbody2D        ← VERIFICAR se existe. Se não, adicionar.
│   ├── Gravity Scale: 0
│   ├── Freeze Rotation Z: ✓
│   └── Collision Detection: Continuous (opcional, evita tunelamento)
├── CircleCollider2D   ← já existe (isTrigger = false)
├── EnemyController.cs ← já existe
│   ├── _moveSpeed: 2
│   ├── _maxHealth: 30
│   ├── _damageToPlayer: 10
│   ├── _attackCooldown: 1
│   ├── _knockbackForce: 5        ← NOVO
│   └── _player: vazio (RoomController configura via Initialize)
└── SpriteRenderer     ← já existe
```

**Atenção:** Enemy precisa de `Rigidbody2D` para:
1. `OnTriggerEnter2D` ser chamado (Unity requer Rigidbody2D em pelo menos um dos objetos)
2. `AddForce` funcionar no futuro (se quiser knockback no enemy também)
3. Movimento via `linearVelocity` funcionar

### Player.prefab — verificação no Inspector

```
Player (GameObject)
├── Rigidbody2D        ← já existe
├── BoxCollider2D      ← já existe
├── PlayerController.cs ← já existe
│   ├── _moveSpeed: 5
│   ├── _jumpForce: 10
│   ├── _groundCheck: GroundCheck (child)
│   ├── _groundCheckRadius: 0.15
│   ├── _groundLayer: Ground
│   ├── _knockbackForce: 5        ← NOVO (default)
│   ├── _invincibilityDuration: 0.5 ← NOVO (default)
│   └── IsInvincible: (readonly)   ← NOVO, visível no debug
├── PlayerHealth.cs    ← sem alteração
├── WeaponController.cs ← sem alteração
└── SpriteRenderer     ← já existe
```

### Layer Collision Matrix

`Project Settings > Physics 2D > Layer Collision Matrix`:

Configurar (a matrix atual está toda como `true`):

```
Desmarcar:
  - player ↔ player    (evita auto-colisão do player)
  - enemy ↔ enemy      (evita colisão entre inimigos)

Manter marcado:
  - player ↔ enemy     (dano e knockback)
  - player ↔ Ground    (chão)
  - enemy ↔ Ground     (inimigos andam no chão)
  - player ↔ Default
  - enemy ↔ Default
  - Ground ↔ Default
```

---

## 6. Ordem de Execução (passo a passo para implementação)

```
Passo 1: EnemyController.cs
  → Adicionar _rigidbody, _spriteRenderer no topo
  → Adicionar cache no Awake()
  → Mudar ChasePlayer() de transform.position para _rigidbody.linearVelocity
  → TESTAR: enemy ainda persegue player normalmente

Passo 2: EnemyController.cs
  → Adicionar DieSequence() IEnumerator
  → Mudar TakeDamage() para chamar DieSequence() em vez de Destroy direto
  → TESTAR: enemy morre com flash vermelho após 0.3s

Passo 3: PlayerController.cs
  → Adicionar _knockbackForce, _invincibilityDuration, _isInvincible
  → Adicionar propriedade IsInvincible
  → Adicionar ApplyKnockback() e InvincibilityRoutine()
  → TESTAR: chamar ApplyKnockback manualmente via botão debug

Passo 4: EnemyController.cs
  → Adicionar _knockbackForce no topo
  → Modificar OnTriggerEnter2D: check IsInvincible + chamada ApplyKnockback
  → TESTAR: player é empurrado ao encostar em enemy, pisca vermelho

Passo 5: Unity Editor
  → Verificar se Enemy.prefab tem Rigidbody2D (Gravity Scale = 0)
  → Configurar Layer Collision Matrix
  → TESTAR: player não leva dano durante flash vermelho

Passo 6: Teste completo
  → Player encosta em enemy → knockback + flash + dano
  → Durante flash, encostar novamente → SEM dano
  → Player mata enemy → flash vermelho, morre após 0.3s
  → Enemy morre na sala → _enemiesAlive decrementa, portas abrem
```

---

## 7. Layers e Colisão

| Layer | Índice | Colide com |
|-------|--------|------------|
| player | 7 | enemy, Ground, Default |
| enemy | 3 | player, Ground, Default |
| Ground | 8 | player, enemy, Default |

**Collision Matrix (marcados):**
- player ↔ enemy = SIM
- player ↔ Ground = SIM
- enemy ↔ Ground = SIM
- player ↔ player = NÃO
- enemy ↔ enemy = NÃO

---

## 8. Critérios de Validação

| # | Teste | Passo | Resultado Esperado |
|---|-------|-------|-------------------|
| 1 | Knockback funciona | Player encosta em enemy | Player é empurrado na direção oposta ao enemy |
| 2 | I-frames ativos | Durante flash vermelho, encostar no enemy novamente | NÃO toma dano, NÃO é empurrado novamente |
| 3 | I-frames expiram | Após ~0.5s (fim do flash), encostar no enemy | Toma dano normalmente, knockback funciona |
| 4 | Morte do enemy | Atacar enemy até HP <= 0 | Enemy fica vermelho, some após 0.3s |
| 5 | Sala funciona | Matar todos os enemies da sala | `_enemiesAlive` chega a 0, portas abrem |
| 6 | Morte do player | Deixar player morrer para enemies | Cena reinicia (comportamento atual) |
| 7 | Movimento do enemy | Observar enemy perseguindo player | Enemy usa Rigidbody2D, movimento suave |
| 8 | Não atravessa parede | Dar knockback no player em direção a parede | Player é empurrado mas para na parede |
