# Setup Unity — Fase 2 + Fase 3

## 1. Layers

Project Settings → Tags and Layers → Criar:

| Layer | Índice | Uso |
|-------|--------|-----|
| Player | 8 | Player e hitbox do player |
| Enemy | 9 | Inimigos |
| PlayerAttack | 10 | Hitbox de ataque (opcional) |

---

## 2. Physics 2D Collision Matrix

Project Settings → Physics 2D → Layer Collision Matrix:

| | Player | Enemy | PlayerAttack | Default |
|-|--------|-------|--------------|---------|
| Player | NÃO | SIM | NÃO | SIM |
| Enemy | SIM | NÃO | SIM | SIM |
| PlayerAttack | NÃO | SIM | NÃO | NÃO |
| Default | SIM | SIM | NÃO | SIM |

---

## 3. Prefab: Player

```
Criar GameObject "Player"
  → SpriteRenderer (sprite de placeholder, Layer = Player)
  → Rigidbody2D (Gravity Scale = 0, Freeze Rotation Z)
  → BoxCollider2D (isTrigger = false)
  → PlayerController.cs
  → PlayerHealth.cs (OnDeath = evento vazio por enquanto)
  → WeaponController.cs
    - _player: arrastar Player (this.transform)
    - _hitboxPrefab: arrastar SwordHitbox (próximo passo)
```

**No Inspector:**
- Rigidbody2D → Constraints → Freeze Rotation Z ✓
- Layer: Player
- Input: legacy Input Manager (Fire1 = mouse button 0 ou J)

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
Criar empty GameObject "Room"
  → BoxCollider2D (isTrigger = true, tamanho cobrindo sala inteira)
  → RoomController.cs
    - _spawnPoints: arrastar SpawnPoint1, SpawnPoint2
    - _enemyPrefab: arrastar Enemy.prefab
    - _doors: arrastar Door1, Door2
    - _player: arrastar Player

Filhos:
  - "SpawnPoint1" (empty, posição de spawn inimigo 1)
  - "SpawnPoint2" (empty, posição de spawn inimigo 2)
  - "Door1" (Door prefab, posição na entrada)
  - "Door2" (Door prefab, posição na entrada)
```

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
| Inimigo não segue | Verificar _player configurado no Initialize |
| Hitbox não detecta | Verificar isTrigger=true, Layer correto |
| Porta não bloqueia | Verificar _collider atribuído, isTrigger=false |
| Cena não reinicia | Verificar GameManager, PlayerHealth.OnDeath |
| Inimigo não morre | Verificar IDamageable implementado |
