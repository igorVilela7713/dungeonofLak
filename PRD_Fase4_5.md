# PRD — Fase 4 + Fase 5: Enemy Base + Loop de Sala

## Análise do Estado Atual

### ✅ O que já existe

| Sistema | Script | Status |
|---------|--------|--------|
| Enemy segue player | `EnemyController.ChasePlayer()` | ✅ Pronto |
| Enemy recebe dano | `EnemyController.TakeDamage()` + `IDamageable` | ✅ Pronto |
| Enemy dá dano ao player | `EnemyController.OnTriggerEnter2D()` | ✅ Pronto |
| Sala spawna inimigos | `RoomController.SpawnEnemies()` | ✅ Pronto |
| Sala detecta morte | `EnemyDeathTracker.OnDestroy()` → `OnEnemyKilled()` | ✅ Pronto |
| Porta abre/fecha | `Door.Open()` / `Door.Close()` | ✅ Pronto |
| Restart na morte | `GameManager` → `SceneManager.LoadScene()` | ✅ Pronto |

### ❌ O que falta (gaps de gameplay feel)

| Gap | Impacto | Prioridade |
|-----|---------|------------|
| Knockback no player ao tomar dano | Sem feedback de impacto | P0 |
| I-frames (invencibilidade pós-dano) | Player leva dano múltiplo por segundo | P0 |
| Morte do inimigo instantânea (sem visual) | Inimigo simplesmente desaparece | P1 |
| Layer Collision Matrix não configurada | Todas layers colidem com tudo (default) | P1 |
| Inimigo usa `transform.position` ao invés de Rigidbody2D | Física inconsistente | P2 |

---

## Objetivo

Polir o loop de combate Enemy + Sala para que o jogo tenha feedback adequado ao player tomar dano, e o inimigo tenha uma sequência de morte visível.

---

## Componentes Necessários

### Scripts a criar/modificar

| Script | Ação | Pasta |
|--------|------|-------|
| `PlayerController` | Modificar — adicionar knockback e I-frames | `Scripts/Movement/` |
| `EnemyController` | Modificar — knockback no player, usar Rigidbody2D | `Scripts/Enemies/` |

**Nenhum script novo necessário.** A abordagem é modificar os existentes.

### Componentes Unity necessários

- `Rigidbody2D` no Player (já existe) — para receber forças de knockback
- `Rigidbody2D` no Enemy (verificar se existe) — para AddForce funcionar
- Layer Collision Matrix — configurar no `Project Settings > Physics 2D`

---

## Estrutura Sugerida (pós-modificação)

```
Assets/Scripts/
├── Movement/
│   └── PlayerController.cs    # + knockback, i-frames, flash
├── Enemies/
│   └── EnemyController.cs     # + knockback no player, Rigidbody2D
├── Combat/
│   ├── IDamageable.cs         # sem alteração
│   ├── PlayerHealth.cs        # sem alteração
│   └── WeaponController.cs    # sem alteração
└── Rooms/
    ├── RoomController.cs      # sem alteração
    ├── Door.cs                # sem alteração
    └── EnemyDeathTracker.cs   # sem alteração
```

---

## Fluxo de Funcionamento

### Fluxo atual (sem knockback)
```
Enemy colide com Player (OnTriggerEnter2D)
    ↓
Enemy dá dano → PlayerHealth.TakeDamage(10)
    ↓
Player continua andando normalmente ❌
```

### Fluxo proposto (com knockback + i-frames)
```
Enemy colide com Player (OnTriggerEnter2D)
    ↓
Enemy dá dano → PlayerHealth.TakeDamage(10)
    ↓
PlayerController.ApplyKnockback(direction, force)
    → Rigidbody2D.AddForce(-direction * knockbackForce, ForceMode2D.Impulse)
    → _isInvincible = true
    → SpriteRenderer.flash = coroutine (feedback visual)
    ↓
Wait(0.5s) → _isInvincible = false
    ↓
Durante i-frames, Player ignora dano ✅
```

### Fluxo de morte do inimigo (proposto)
```
EnemyController.TakeDamage(amount)
    ↓
_currentHealth <= 0
    ↓
_isDead = true
    ↓
Collider2D.enabled = false          # para de interagir
SpriteRenderer.color = vermelho     # flash de dano
    ↓
Wait(0.3s)                          # breve delay para "morte"
    ↓
Destroy(gameObject)                 # EnemyDeathTracker.OnDestroy → OnEnemyKilled()
```

---

## Especificação: Modificações no EnemyController

### Mudança 1: Rigidbody2D no movimento

O enemy atualmente move via `transform.position`. Para knockback funcionar, precisa usar Rigidbody2D.

```csharp
// Antes (atual)
private void ChasePlayer()
{
    Vector2 direction = (_player.position - transform.position).normalized;
    transform.position += (Vector3)direction * _moveSpeed * Time.deltaTime;
}

// Depois (proposto)
private Rigidbody2D _rigidbody;

private void Awake()
{
    _rigidbody = GetComponent<Rigidbody2D>();
    _rigidbody.gravityScale = 0;
    _rigidbody.freezeRotation = true;
    _currentHealth = _maxHealth;
}

private void ChasePlayer()
{
    if (_player == null) return;

    Vector2 direction = (_player.position - transform.position).normalized;
    _rigidbody.linearVelocity = direction * _moveSpeed;
}
```

### Mudança 2: Knockback no player ao colidir

```csharp
private void OnTriggerEnter2D(Collider2D other)
{
    if (other.gameObject.layer != LayerMask.NameToLayer("player")) return;
    if (_attackCooldownTimer > 0) return;

    IDamageable damageable = other.GetComponent<IDamageable>();
    if (damageable != null)
    {
        damageable.TakeDamage(_damageToPlayer);
        _attackCooldownTimer = _attackCooldown;

        // Knockback no player
        PlayerController playerController = other.GetComponent<PlayerController>();
        if (playerController != null)
        {
            Vector2 knockDir = (other.transform.position - transform.position).normalized;
            playerController.ApplyKnockback(knockDir, _knockbackForce);
        }
    }
}
```

Novos campos:
```csharp
[SerializeField] private float _knockbackForce = 5f;
```

### Mudança 3: Sequência de morte com delay

```csharp
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

private IEnumerator DieSequence()
{
    GetComponent<Collider2D>().enabled = false;
    SpriteRenderer sr = GetComponent<SpriteRenderer>();
    if (sr != null) sr.color = Color.red;
    yield return new WaitForSeconds(0.3f);
    Destroy(gameObject);
}
```

---

## Especificação: Modificações no PlayerController

### Mudança 1: Knockback + I-frames

```csharp
[Header("Combat")]
[SerializeField] private float _knockbackForce = 5f;
[SerializeField] private float _invincibilityDuration = 0.5f;

private bool _isInvincible;
```

Novo método público:
```csharp
public void ApplyKnockback(Vector2 direction, float force)
{
    _rigidbody.AddForce(direction * force, ForceMode2D.Impulse);
    StartCoroutine(InvincibilityRoutine());
}

private IEnumerator InvincibilityRoutine()
{
    _isInvincible = true;
    SpriteRenderer sr = GetComponent<SpriteRenderer>();
    if (sr != null)
    {
        // Flash rápido
        for (int i = 0; i < 4; i++)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(0.08f);
            sr.color = Color.white;
            yield return new WaitForSeconds(0.08f);
        }
    }
    _isInvincible = false;
}
```

### Mudança 2: I-frames no PlayerHealth

O `PlayerHealth.TakeDamage` precisa verificar i-frames. Duas opções:

**Opção A (recomendada):** Check no `EnemyController` antes de chamar `TakeDamage`

```csharp
// No EnemyController.OnTriggerEnter2D:
PlayerController pc = other.GetComponent<PlayerController>();
if (pc != null && pc.IsInvincible) return;
```

**Opção B:** Check no próprio `PlayerHealth`

```csharp
public void TakeDamage(int amount)
{
    PlayerController pc = GetComponent<PlayerController>();
    if (pc != null && pc.IsInvincible) return;

    _currentHealth -= amount;
    // ...
}
```

**Recomendo Opção A** — não modifica o contrato de `IDamageable` e mantém `PlayerHealth` simples.

Nova propriedade em `PlayerController`:
```csharp
public bool IsInvincible => _isInvincible;
```

---

## Configuração: Layer Collision Matrix

**Problemática:** O `DynamicsManager.asset` atual tem a matrix como `ffffffff...` (tudo colide com tudo). Isso pode causar colisões indesejadas.

Configurar em `Project Settings > Physics 2D > Layer Collision Matrix`:

| | player | enemy | Ground | Default |
|-|--------|-------|--------|---------|
| player | NÃO | SIM | SIM | SIM |
| enemy | SIM | NÃO | SIM | SIM |
| Ground | SIM | SIM | NÃO | SIM |
| Default | SIM | SIM | SIM | SIM |

> NÃO marcar `player ↔ player` nem `enemy ↔ enemy` para evitar auto-colisão.

---

## Ordem de Implementação

1. **EnemyController** — adicionar `Rigidbody2D` e usar `linearVelocity` em vez de `transform.position`
2. **PlayerController** — adicionar `ApplyKnockback()` + `IsInvincible` + coroutine de flash
3. **EnemyController** — adicionar chamada de `ApplyKnockback` no `OnTriggerEnter2D`
4. **EnemyController** — adicionar `DieSequence()` com delay
5. **Layer Collision Matrix** — configurar no Unity Editor
6. **Testar** — Play Mode, verificar knockback, i-frames, morte do inimigo

---

## Riscos e Pontos de Atenção

| Risco | Mitigação |
|-------|-----------|
| Knockback empurra player para fora da sala | Usar force moderado (_knockbackForce = 5). Player já tem Rigidbody2D com collider. |
| I-frames muito curtos = player leva dano em cascata | 0.5s é padrão para roguelikes. Ajustar se necessário. |
| Rigidbody2D no enemy altera comportamento de chase | Usar `linearVelocity` em vez de `AddForce`. Freeze Rotation Z. Gravity Scale = 0. |
| Morte com delay causa bug de `OnEnemyKilled()` ser chamado após `Destroy` | `EnemyDeathTracker.OnDestroy()` é chamado no `Destroy(gameObject)` — delay é antes do Destroy, não causa conflito. |
| Multiple enemies hitting player simultaneously | I-frames previnem dano múltiplo. Cada enemy tem cooldown próprio (_attackCooldown = 1s). |

---

## Dependências com Futuro

| Sistema Futuro | Dependência |
|----------------|-------------|
| Fase 8 (Armas) | `WeaponController` não precisa mudar — usa `IDamageable.TakeDamage()` |
| Fase 9 (Inimigos Avançados) | Knockback force pode ser configurável por tipo de inimigo via ScriptableObject |
| Fase 14 (Polimento) | Camera shake pode ser adicionado ao `ApplyKnockback()` como extensão |
| Fase 5 (Sala) | `EnemyDeathTracker` continua funcionando — delay é antes do Destroy |

---

## Critérios de Validação

| Teste | Resultado Esperado |
|-------|-------------------|
| Player encosta em enemy | Player é empurrado para trás, pisca vermelho |
| Player encosta em enemy novamente durante flash | NÃO toma dano (i-frames ativos) |
| Player mata enemy | Enemy pisca vermelho, some após 0.3s |
| Enemy morre na sala | `_enemiesAlive` decrementa corretamente, portas abrem |
| Player morre | Cena reinicia (comportamento atual mantido) |
| Player não atravessa paredes | Knockback não ultrapassa colliders |

---

## Referências

- `Rigidbody2D.AddForce` com `ForceMode2D.Impulse`: https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Rigidbody2D.AddForce.html
- `ForceMode2D.Impulse`: força instantânea considerando massa — ideal para knockback
- Padrão roguelike: i-frames de 0.3-0.5s com flash visual é o padrão da indústria
