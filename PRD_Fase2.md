# PRD — Fase 2: Player e Combate

## Objetivo
Implementar controle do player, sistema de vida e combate corpo-a-corpo com espada em um roguelike 2D.

---

## Componentes Necessários

### Scripts

| Script | Responsabilidade | Pasta |
|--------|-----------------|-------|
| `PlayerController` | Movimento 2D (WASD/Setas) | `Scripts/Movement/` |
| `PlayerHealth` | Gerenciar HP, receber dano, morte | `Scripts/Combat/` |
| `WeaponController` | Ataque com espada, hitbox, cooldown | `Scripts/Combat/` |
| `EnemyController` | IA simples: seguir, atacar, receber dano | `Scripts/Enemies/` |
| `IDamageable` | Interface para dano | `Scripts/Combat/` |

### Prefabs

| Prefab | Descrição |
|--------|-----------|
| `Player.prefab` | Player com Rigidbody2D, Collider2D, Sprite |
| `Enemy.prefab` | Inimigo básico com Rigidbody2D, Collider2D |
| `SwordHitbox.prefab` | Hitbox temporal para ataque |

### Componentes Unity

- `Rigidbody2D` — física no player e inimigo
- `Collider2D` (BoxCollider2D/CircleCollider2D) — colisão
- `SpriteRenderer` — renderização
- `Layer` "Enemy" e "Player" — para detecção de colisão
- `Physics2D` Layer Collision Matrix — configurar quem colide com quem

---

## Estrutura Sugerida

```
Assets/Scripts/
├── Movement/
│   └── PlayerController.cs
├── Combat/
│   ├── IDamageable.cs
│   ├── PlayerHealth.cs
│   └── WeaponController.cs
└── Enemies/
    └── EnemyController.cs

Assets/Prefabs/
├── Player.prefab
├── Enemy.prefab
└── SwordHitbox.prefab
```

---

## Fluxo Lógico

### Movimento (Tarefa 4)
1. Input captura WASD/Setas
2. Aplica velocidade no Rigidbody2D
3. Colisão com tilemap/walls impede passagem

### Vida (Tarefa 5)
1. `PlayerHealth` exposto como `int CurrentHealth`
2. `TakeDamage(int amount)` reduz HP
3. Se HP <= 0, chamar `Die()`

### Combate (Tarefa 6)
1. Input de ataque (e.g., J, Espaço, Clique)
2. `WeaponController.Attack()`:
   - Spawn hitbox temporária na frente do player
   - WaitForSeconds(damageCooldown)
   - Hitbox detecta `OnTriggerEnter2D` com inimigos
   - Aplica `TakeDamage()` via `IDamageable`

### Inimigo (Tarefa 7)
1. `EnemyController` tem `IDamageable`
2. `Update()`: mover em direção ao player (Vector2.MoveTowards)
3. `OnTriggerEnter2D`: se colidir com player, aplicar dano
4. `TakeDamage()`: reduzir HP, morrer se <= 0

---

## Riscos

| Risco | Mitigação |
|-------|-----------|
| Hitbox não detecta colisão | Usar `Collider2D.isTrigger = true`, verificar Layer |
| Ataque acerta múltiplas vezes | Usar cooldown e flag `isAttacking` |
| Inimigo atravessa player | Configurar Physics2D Layer Collision |
| Overengineering em IA | Seguir player simples (MoveTowards), sem pathfinding |
| Dano em cascata (bug de frame) | Armazenar `_isDead` flag antes de aplicar dano |

---

## Prioridade de Implementação

1. `IDamageable` (interface base)
2. `PlayerHealth`
3. `PlayerController`
4. `WeaponController`
5. `EnemyController`

Executar cada tarefa separadamente no Unity antes de avançar.
