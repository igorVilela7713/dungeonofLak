# Setup Unity — Fase 2 + Fase 3

## 1. Layers

Project Settings → Tags and Layers → Criar:

| Layer | Índice | Uso |
|-------|--------|-----|
| Player | 8 | Player e hitbox do player |
| Enemy | 9 | Inimigos |
| PlayerAttack | 10 | Hitbox de ataque (opcional) |
| Ground | 11 | Chão e plataformas |

---

## 2. Physics 2D Collision Matrix

Project Settings → Physics 2D → Layer Collision Matrix:

| | Player | Enemy | PlayerAttack | Ground | Default |
|-|--------|-------|--------------|--------|---------|
| Player | NÃO | SIM | NÃO | SIM | SIM |
| Enemy | SIM | NÃO | SIM | SIM | SIM |
| PlayerAttack | NÃO | SIM | NÃO | NÃO | NÃO |
| Ground | SIM | SIM | NÃO | NÃO | SIM |
| Default | SIM | SIM | NÃO | SIM | SIM |

---

## 3. Prefab: Player

```
Criar GameObject "Player"
  → SpriteRenderer (sprite de placeholder, Layer = Player)
  → Rigidbody2D (Gravity Scale = 3, Freeze Rotation Z)
  → BoxCollider2D (isTrigger = false)
  → PlayerController.cs
  → PlayerHealth.cs (OnDeath = evento vazio por enquanto)
  → WeaponController.cs
    - _player: arrastar Player (this.transform)
    - _hitboxPrefab: arrastar SwordHitbox (próximo passo)
  → GroundCheck (empty child, posição nos pés do player)
```

**No Inspector:**
- Rigidbody2D → Constraints → Freeze Rotation Z ✓
- Layer: Player
- PlayerController:
  - _moveSpeed: 5
  - _jumpForce: 10
  - _groundCheck: arrastar GroundCheck (child)
  - _groundCheckRadius: 0.15
  - _groundLayer: marcar layer Ground

**GroundCheck:**
- Criar Empty GameObject como filho do Player
- Renomear para "GroundCheck"
- Posicionar na base do player (nos pés)
- Arrastar no campo _groundCheck do PlayerController

---

## 4. Prefab: SwordHitbox

```
Criar empty GameObject "SwordHitbox"
  → BoxCollider2D (isTrigger = true, tamanho pequeno, ex: 0.5x0.5)
  → SwordHitbox.cs
  → Converter em Prefab (Assets/Prefabs/)
  → Deletar da cena
```

---

## 5. Prefab: Enemy

```
Criar GameObject "Enemy"
  → SpriteRenderer (sprite de placeholder, Layer = Enemy)
  → Rigidbody2D (Gravity Scale = 0, Freeze Rotation Z)
  → CircleCollider2D (isTrigger = false)
  → EnemyController.cs
    - _player: deixar vazio (RoomController configura via Initialize)
```

**No Inspector:**
- Layer: Enemy
- CircleCollider2D: ajustar tamanho ao sprite

---

## 6. GameObject: GameManager (Fase 3)

```
Criar empty GameObject "GameManager"
  → GameManager.cs
    - _playerHealth: arrastar PlayerHealth do Player
    - _sceneName: "Main"
```

---

## 7. GameObject: Room (Fase 3)

```
**No Inspector do Room:**
- BoxCollider2D → **isTrigger = true** ✓ (obrigatório para detectar entrada do player)
- Ajustar tamanho do collider para cobrir toda a área jogável da sala
```

**Spawn Points:**
São Empty GameObjects que definem onde os inimigos aparecem.

```
Criar filhos do Room:
  - "SpawnPoint1" (Empty, posicionar onde o inimigo 1 deve aparecer)
  - "SpawnPoint2" (Empty, posicionar onde o inimigo 2 deve aparecer)
  - Adicionar mais se necessário (SpawnPoint3, etc.)
```

**No Inspector do RoomController:**
- _spawnPoints: clicar no "+" e arrastar cada SpawnPoint (1, 2, 3...)
- _enemyPrefab: arrastar o prefab Enemy
- _doors: arrastar Door1, Door2
- _player: arrastar o Player da cena
- _roomCenter: arrastar RoomCenter (Empty no centro da sala)

**RoomCenter:**
Empty GameObject filho do Room posicionado no centro da sala. As portas fecham quando o player chega a 1 unidade desse ponto.

**Hierarquia do Room:**
```
Room (RoomController.cs, BoxCollider2D isTrigger=true)
├── RoomCenter (empty, posição central da sala)
├── SpawnPoint1 (empty, posição de spawn inimigo 1)
├── SpawnPoint2 (empty, posição de spawn inimigo 2)
├── Door1 (Door prefab, posição na entrada)
└── Door2 (Door prefab, posição na entrada)
```

**Comportamento:**
1. Player entra no trigger do Room → inimigos spawnam
2. Player chega ao centro da sala → portas fecham
3. Player mata todos os inimigos → portas abrem

---

## 8. Prefab: Door (Fase 3)

```
Criar GameObject "Door"
  → BoxCollider2D (isTrigger = false, tamanho como porta)
  → SpriteRenderer (sprite de porta placeholder)
  → Door.cs
    - _collider: arrastar o BoxCollider2D deste objeto
  → Converter em Prefab
```

---

## 9. Hierarquia Final na Cena

```
Scene "Main"
├── Main Camera
├── Player (PlayerController, PlayerHealth, WeaponController)
├── GameManager (GameManager.cs)
├── Room (RoomController.cs, BoxCollider2D isTrigger=true)
│   ├── SpawnPoint1 (empty)
│   ├── SpawnPoint2 (empty)
│   ├── Door1 (Door.cs, BoxCollider2D)
│   └── Door2 (Door.cs, BoxCollider2D)
└── [Paredes/Walls como tilemap]
```

---

## 10. Como Testar

1. Abrir cena "Main"
2. Configurar layers (Player=8, Enemy=9)
3. Configurar Collision Matrix (Player↔Enemy = SIM)
4. Posicionar Player na sala
5. Posicionar Room com spawn points nas posições corretas
6. Posicionar Doors nas entradas da sala
7. Play Mode
8. Mover Player → atacar → verificar movimento e hitbox
9. Entrar na sala → portas fecham → inimigos spawneam
10. Matar inimigos → portas abrem
11. Deixar player morrer → cena reinicia

---

## 11. Troubleshooting

| Problema | Solução |
|----------|---------|
| Player não se move | Verificar Rigidbody2D, Layer Player |
| Player não pula | Verificar GroundCheck posição, _groundLayer marcado com Ground |
| Player sobe infinitamente | Verificar Gravity Scale = 3 no Rigidbody2D |
| Inimigo não segue | Verificar _player configurado no Initialize |
| Hitbox não detecta | Verificar isTrigger=true, Layer correto |
| Porta não bloqueia | Verificar _collider atribuído, isTrigger=false |
| Cena não reinicia | Verificar GameManager, PlayerHealth.OnDeath |
| Inimigo não morre | Verificar IDamageable implementado |
