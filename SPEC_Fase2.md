# SPEC — Fase 2: Player e Combate

## 1. Arquivos a Criar

```
Assets/Scripts/
├── Combat/
│   ├── IDamageable.cs       # Interface para dano
│   ├── PlayerHealth.cs      # Sistema de HP
│   └── WeaponController.cs  # Ataque com espada
├── Movement/
│   └── PlayerController.cs  # Movimento 2D
└── Enemies/
    └── EnemyController.cs   # IA do inimigo

Assets/Prefabs/
└── SwordHitbox.prefab       # Hitbox temporária de ataque
```

---

## 2. Estrutura de Classes

### IDamageable (Interface)
```csharp
public interface IDamageable
{
    void TakeDamage(int amount);
}
```

### PlayerHealth
- **Campos:** `_maxHealth` (int), `_currentHealth` (int), `OnDeath` (event)
- **Propriedades:** `MaxHealth`, `CurrentHealth`
- **Métodos:** `TakeDamage(int)`, `Die()`

### PlayerController
- **Campos:** `_moveSpeed` (float), `_rigidbody` (Rigidbody2D)
- **Propriedades:** `FacingDirection` (Vector2)
- **Métodos:** `Update()`, `Move(Vector2)`

### WeaponController
- **Campos:** `_damage` (int), `_attackCooldown` (float), `_hitboxPrefab` (GameObject), `_player` (Transform)
- **Propriedades:** `IsAttacking` (bool)
- **Métodos:** `Attack()`, `SpawnHitbox()`, `OnHitboxTrigger(Collider2D)`

### EnemyController : MonoBehaviour, IDamageable
- **Campos:** `_moveSpeed` (float), `_maxHealth` (int), `_damageToPlayer` (int), `_player` (Transform), `_currentHealth` (int), `_isDead` (bool)
- **Métodos:** `Update()`, `ChasePlayer()`, `TakeDamage(int)`, `OnTriggerEnter2D(Collider2D)`

### SwordHitbox : MonoBehaviour
- **Campos:** `_controller` (WeaponController)
- **Métodos:** `Initialize(WeaponController)`, `OnTriggerEnter2D(Collider2D)`

---

## 3. Responsabilidades

| Script | Responsabilidade |
|--------|-----------------|
| `IDamageable` | Contrato: qualquer objeto que pode receber dano implementa `TakeDamage()` |
| `PlayerHealth` | Controla HP do player, dispara evento de morte |
| `PlayerController` | Lê input, move o Rigidbody2D |
| `WeaponController` | Spawna hitbox de ataque, aplica dano a inimigos |
| `EnemyController` | Persegue player, causa dano ao colidir, morre |
| `SwordHitbox` | Detecta colisão com inimigos, notifica WeaponController |

---

## 4. Fluxo Lógico

### Movimento
```
Unity Input (WASD)
    ↓
PlayerController.Update()
    ↓
Lê Input.GetAxis() → Vector2
    ↓
Move(direction)
    ↓
Rigidbody2D.velocity = direction * speed
```

### Ataque
```
Unity Input (J / Espaço)
    ↓
WeaponController.Attack()
    ↓
Verifica cooldown + flag IsAttacking
    ↓
Spawn SwordHitbox.prefab na frente do player
    ↓
Wait(attackDuration) → Destroy hitbox
    ↓
Wait(attackCooldown) → libera próximo ataque
```

### Dano
```
SwordHitbox.OnTriggerEnter2D(other)
    ↓
Verifica se other.gameObject tem IDamageable
    ↓
IDamageable.TakeDamage(damage)
    ↓
Inimigo: reduz HP, destrói se HP <= 0
Player: reduz HP, dispara OnDeath se HP <= 0
```

### Inimigo
```
EnemyController.Update()
    ↓
Se !_isDead → ChasePlayer()
    ↓
MoveTowards(_player.position, speed)
    ↓
OnTriggerEnter2D(other) com Player
    ↓
Player.TakeDamage(_damageToPlayer)
```

---

## 5. Pseudocódigo

### PlayerHealth.TakeDamage()
```
TakeDamage(int amount):
    _currentHealth -= amount
    if _currentHealth <= 0:
        _currentHealth = 0
        Die()
```

### PlayerController.Update()
```
Update():
    horizontal = Input.GetAxisRaw("Horizontal")
    vertical = Input.GetAxisRaw("Vertical")
    direction = Vector2(horizontal, vertical)
    Move(direction)
    if direction.x != 0:
        FacingDirection = new Vector2(direction.x, 0)
```

### WeaponController.Attack()
```
Attack():
    if IsAttacking: return
    if cooldown > 0: return
    
    IsAttacking = true
    SpawnHitbox()
    
    StartCoroutine(WaitAndDestroy(attackDuration, hitbox))
    StartCoroutine(Cooldown(attackCooldown))
```

### WeaponController.OnHitboxTrigger()
```
OnHitboxTrigger(Collider2D other):
    if other.gameObject.layer != Enemy: return
    damageable = other.GetComponent<IDamageable>()
    if damageable != null:
        damageable.TakeDamage(damage)
```

### EnemyController.ChasePlayer()
```
ChasePlayer():
    direction = (_player.position - transform.position).normalized
    transform.position += direction * moveSpeed * Time.deltaTime
```

### EnemyController.TakeDamage()
```
TakeDamage(int amount):
    if _isDead: return
    _currentHealth -= amount
    if _currentHealth <= 0:
        _isDead = true
        Destroy(gameObject)
```

---

## Layers e Colisão

| Layer | Índice | Colide com |
|-------|--------|------------|
| Player | 8 | Enemy |
| Enemy | 9 | Player, PlayerAttack |
| PlayerAttack | 10 | Enemy |

**Collision Matrix:** PlayerAttack ↔ Player = NO
