# Setup Unity — Fase 2 + Fase 3 + Fase 4 + Fase 5

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
  - _knockbackForce: 5 (padrão)
  - _invincibilityDuration: 0.5 (padrão)

**GroundCheck:**
- Criar Empty GameObject como filho do Player
- Renomear para "GroundCheck"
- Posicionar na base do player (nos pés)
- Arrastar no campo _groundCheck do PlayerController

---

## 4. Prefab: SwordHitbox

### Passo 4.1: Criar o objeto na cena

1. Clique direito na hierarquia → **Create Empty**
2. Renomeie para `SwordHitbox`
3. Adicione o componente **BoxCollider2D**
   - Marque **isTrigger = true** (obrigatório)
   - Ajuste o tamanho: Size `X = 0.5`, `Y = 0.5`
4. Adicione o script **SwordHitbox.cs** no mesmo objeto

### Passo 4.2: Transformar em Prefab

1. Vá em `Assets/Prefabs/` no Project window (crie a pasta se não existir)
2. **Arraste o SwordHitbox da hierarquia** pra dentro da pasta `Assets/Prefabs/`
3. O prefab foi criado. **Delete o SwordHitbox da cena** — ele só existe como prefab agora

### Passo 4.3: Conectar no Player

No **Player** (que já tem `PlayerController`, `PlayerHealth`, etc.):

1. Se ainda não tem, adicione o componente **WeaponController.cs** ao Player
2. No Inspector do **WeaponController**, arraste:
   - `_player`: arraste o próprio **Player** (o transform do objeto)
   - `_hitboxPrefab`: arraste o **prefab SwordHitbox** da pasta `Assets/Prefabs/`

### Passo 4.4: Testar

1. Dê Play na cena
2. Clique com o mouse esquerdo (ou Enter, ou botão A do gamepad)
3. O `WeaponController` cria uma instância do SwordHitbox na frente do player (0.8 unidades na direção que ele olha)
4. A hitbox existe por **0.1 segundo** (configurável via `_attackDuration`)
5. Se colidir com algo que tem `IDamageable` (como o Enemy), chama `TakeDamage()`

### Fluxo de código:

```
PlayerController detecta input de ataque
        ↓
WeaponController.Attack() chama SpawnHitbox()
        ↓
Instantiate(_hitboxPrefab) cria SwordHitbox na cena
SwordHitbox.Initialize(this) conecta ao controller
StartCoroutine(AttackRoutine) espera 0.1s e destroi
        ↓
Se SwordHitbox colidir com Enemy:
  OnTriggerEnter2D → _controller.OnHitboxTrigger(other)
  other.GetComponent<IDamageable>() → TakeDamage(_damage)
```

**Hierarquia do Player com WeaponController:**

```
Player (Layer = Player)
├── Rigidbody2D
├── BoxCollider2D
├── PlayerController.cs
├── PlayerHealth.cs
├── WeaponController.cs
│   ├── _player = Player (this)
│   └── _hitboxPrefab = SwordHitbox prefab
└── GroundCheck (empty child)
```

---

## 5. Prefab: Enemy

```
Criar GameObject "Enemy"
  → SpriteRenderer (sprite de placeholder, Layer = Enemy)
  → Rigidbody2D (Gravity Scale = 0, Freeze Rotation Z)
  → CircleCollider2D (isTrigger = true)
  → EnemyController.cs
    - _player: deixar vazio (RoomController configura via Initialize)
```

**No Inspector:**
- Layer: Enemy
- CircleCollider2D:
  - **isTrigger = true** (obrigatório para detectar contato com player)
  - Ajustar tamanho ao sprite
- Rigidbody2D:
  - Gravity Scale: 0
  - Freeze Rotation Z: ✓
  - Collision Detection: Continuous (opcional, evita tunelamento)
- EnemyController:
  - _moveSpeed: 2
  - _maxHealth: 30
  - _damageToPlayer: 10
  - _attackCooldown: 1
  - _knockbackForce: 5 (NOVO)

**Atenção:** Enemy precisa de Rigidbody2D para:
1. `OnTriggerEnter2D` ser chamado (Unity requer Rigidbody2D em pelo menos um dos objetos)
2. Movimento via `_rigidbody.linearVelocity` funcionar
3. Knockback poder ser aplicado no futuro

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
3. Configurar Collision Matrix (Player↔Enemy = SIM, Player↔Player = NÃO, Enemy↔Enemy = NÃO)
4. Posicionar Player na sala
5. Posicionar Room com spawn points nas posições corretas
6. Posicionar Doors nas entradas da sala
7. Play Mode
8. Mover Player → atacar → verificar movimento e hitbox
9. Entrar na sala → portas fecham → inimigos spawneam
10. Matar inimigos → enemy fica vermelho, morre após 0.3s, portas abrem
11. Deixar inimigo encostar no player → player é empurrado, pisca vermelho/branco
12. Durante o flash vermelho, encostar no inimigo novamente → NÃO toma dano (i-frames)
13. Após ~0.5s, encostar no inimigo → toma dano normalmente
14. Deixar player morrer → cena reinicia

---

## 11. Troubleshooting

| Problema | Solução |
|----------|---------|
| Player não se move | Verificar Rigidbody2D, Layer Player |
| Player não pula | Verificar GroundCheck posição, _groundLayer marcado com Ground |
| Player sobe infinitamente | Verificar Gravity Scale = 3 no Rigidbody2D |
| Inimigo não segue | Verificar _player configurado no Initialize |
| Inimigo não se move | Verificar Rigidbody2D no Enemy prefab (Gravity Scale = 0) |
| Hitbox não detecta | Verificar isTrigger=true, Layer correto |
| Porta não bloqueia | Verificar _collider atribuído, isTrigger=false |
| Cena não reinicia | Verificar GameManager, PlayerHealth.OnDeath |
| Inimigo não morre | Verificar IDamageable implementado |
| Player não leva knockback | Verificar Rigidbody2D no Enemy, _knockbackForce configurado |
| Player leva dano múltiplo | Verificar i-frames: check IsInvincible no EnemyController |
| Enemy não para ao morrer | Verificar _rigidbody.linearVelocity = Vector2.zero no DieSequence |
| Inimigos se empurram | Verificar Collision Matrix: enemy↔enemy = NÃO |
