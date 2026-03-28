# Setup Unity — Fase 2 + Fase 3 + Fase 4 + Fase 5 + Fase 6

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

### Fluxo de código

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
  → Rigidbody2D (Gravity Scale = 3, Freeze Rotation Z)
  → CircleCollider2D (isTrigger = true) — para detectar contato com player
  → CircleCollider2D (isTrigger = false) — para colidir com o chão
  → EnemyController.cs
    - _player: deixar vazio (RoomController configura via Initialize)
  → GroundCheck (empty child, posição nos pés do enemy)
```

**No Inspector:**

- Layer: Enemy
- CircleCollider2D (trigger):
  - **isTrigger = true** (obrigatório para detectar contato com player)
  - Ajustar tamanho ao sprite
- CircleCollider2D (física):
  - **isTrigger = false** (obrigatório para colidir com o chão)
  - Ajustar tamanho ao sprite
- Rigidbody2D:
  - Gravity Scale: 3
  - Freeze Rotation Z: ✓
  - Collision Detection: Continuous (opcional, evita tunelamento)
- EnemyController:
  - _moveSpeed: 2
  - _jumpForce: 8
  - _groundCheck: arrastar GroundCheck (child)
  - _groundCheckRadius: 0.15
  - _groundLayer: marcar layer Ground
  - _maxHealth: 30
  - _damageToPlayer: 10
  - _attackCooldown: 1
  - _knockbackForce: 5

**GroundCheck (Enemy):**

- Criar Empty GameObject como filho do Enemy
- Renomear para "GroundCheck"
- Posicionar na base do enemy (nos pés)
- Arrastar no campo _groundCheck do EnemyController

**Atenção:** Enemy precisa de Rigidbody2D para:

1. `OnTriggerEnter2D` ser chamado (Unity requer Rigidbody2D em pelo menos um dos objetos)
2. Movimento via `_rigidbody.linearVelocity` funcionar
3. Knockback poder ser aplicado no futuro
4. Gravidade puxar o enemy para o chão

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
| Inimigo flutua | Verificar Gravity Scale = 3 no Rigidbody2D, _groundLayer marcado com Ground |
| Inimigo cai pelo chão | Verificar se existe CircleCollider2D com isTrigger=false para física |
| Inimigo não pula | Verificar GroundCheck posição, _groundLayer marcado com Ground,_jumpForce configurado |
| Hitbox não detecta | Verificar isTrigger=true, Layer correto |
| Porta não bloqueia | Verificar _collider atribuído, isTrigger=false |
| Cena não reinicia | Verificar GameManager, PlayerHealth.OnDeath |
| Inimigo não morre | Verificar IDamageable implementado |
| Player não leva knockback | Verificar Rigidbody2D no Enemy, _knockbackForce configurado |
| Player leva dano múltiplo | Verificar i-frames: check IsInvincible no EnemyController |
| Enemy não para ao morrer | Verificar _rigidbody.linearVelocity = Vector2.zero no DieSequence |
| Inimigos se empurram | Verificar Collision Matrix: enemy↔enemy = NÃO |

---

## 12. Fase 6 — Hub

O Hub é a "base" do jogador. Dali o player pode falar com NPCs, salvar o progresso e entrar na dungeon.

---

### 12.1 Criar a Cena Hub

1. No Unity: **File → New Scene → Basic (Built-in)**
2. A cena abre vazia (só com Main Camera e Directional Light)
3. **File → Save As** → navegar até `Assets/Scenes/`
4. Salvar como `Hub.unity`
5. **File → Build Settings → Add Open Scenes** — a cena Hub aparece na lista
6. Garantir que `Main.unity` também está na lista
7. Reordenar: **Hub no Index 0**, Main no Index 1

> O jogo sempre abre a cena do Index 0. Agora vai abrir o Hub.

---

### 12.2 Montar o Chão e Paredes

No Unity, abra a cena `Hub.unity`.

**Opção A — Tilemap (recomendado):**

1. Clique direito na Hierarchy → **2D Object → Tilemap → Rectangular**
2. Abra a janela **Window → 2D → Tile Palette**
3. Arraste os tiles do SunnyLand para criar o chão
4. Crie uma segunda Tilemap para as paredes/bordas (para o player não sair da área)
5. No Inspector de cada Tilemap: **Layer = Ground** (se precisar de colisão)
6. Adicione um **TilemapCollider2D** no Tilemap das paredes

**Opção B — Sprites simples:**

1. Crie um Sprite com **SpriteRenderer** como chão (Layer = Ground)
2. Adicione **BoxCollider2D** nas bordas como paredes
3. Ajuste o tamanho no Inspector

**Paredes invisíveis (alternativa):**

1. Crie Empty GameObjects nas bordas
2. Adicione **BoxCollider2D** em cada um (isTrigger = false)
3. Estique os colliders para cobrir cada lado da área jogável

---

### 12.3 Instanciar o Player

1. No Project window, encontre o **Player.prefab** em `Assets/Prefabs/`
2. **Arraste o prefab para a cena Hub**
3. Posicione onde o player deve aparecer (ex: centro da cena)
4. Crie um Empty GameObject chamado **"PlayerSpawn"** na mesma posição — serve como referência de onde o player aparece

> O Player já vem com PlayerController, PlayerHealth e WeaponController. No Hub, o ataque não atrapalha.

---

### 12.4 Criar a UI de Diálogo

Esta é a interface que mostra o texto dos NPCs.

**Passo 1 — Canvas:**

1. Clique direito na Hierarchy → **UI → Canvas**
2. No Inspector do Canvas:
   - Canvas Scaler → UI Scale Mode: **Scale With Screen Size**
   - Reference Resolution: **1920 × 1080**
   - Match Width Or Height: **0.5**
   - Render Mode: **Screen Space - Overlay** (padrão)

**Passo 2 — DialoguePanel:**

1. Clique direito no Canvas → **UI → Panel**
2. Renomear para **"DialoguePanel"**
3. No Inspector:
   - Rect Transform: Anchor **bottom center** (arrastar no ícone de ancoragem)
   - Width: **800**, Height: **200**
   - Pos X: 0, Pos Y: 200 (aproximado)
   - Cor de fundo (Image → Color): preto com **Alpha ~0.8** (preto semi-transparente)
4. **Desativar o DialoguePanel** (desmarcar a caixinha ao lado do nome no Inspector, ou clicar no checkmark no topo do objeto)

**Passo 3 — DialogueText:**

1. Clique direito no DialoguePanel → **UI → Text - TextMeshPro**
2. Renomear para **"DialogueText"**
3. No Inspector do TextMeshPro:
   - Font Size: **24**
   - Color: **branco**
   - Alignment: centro horizontal, centro vertical
   - Rect Transform: clicar no ícone de anchor (quadrado) → segurar Alt e clicar em **Stretch** (última opção, canto inferior direito) para preencher o painel
   - Margem: ajustar os offsets para dar padding (ex: Left: 20, Top: 20, Right: 20, Bottom: 20)

**Passo 4 — DialogueUI script:**

1. Clique direito na Hierarchy → **Create Empty**
2. Renomear para **"DialogueUI"**
3. No Inspector: **Add Component → DialogueUI**
4. Arrastar no campo `_dialoguePanel`: o **DialoguePanel** (o GameObject do painel)
5. Arrastar no campo `_dialogueText`: o **DialogueText** (o componente TextMeshProUGUI)

---

### 12.5 Criar o NPC

Este NPC só fala com o player — não salva nada.

1. Clique direito na Hierarchy → **Create Empty**
2. Renomear para **"NPC"**
3. No Inspector:
   - Adicionar **SpriteRenderer** → arrastar um sprite placeholder do SunnyLand (ex: personagem NPC)
   - Adicionar **BoxCollider2D**:
     - **isTrigger = true** (obrigatório — sem isso o player não detecta)
     - Ajustar Size para cobrir a área de interação (ex: X = 1, Y = 2)
   - Adicionar **NPCInteractable**:
     - `_dialogueText`: digitar o texto do NPC, ex: **"Bem-vindo a Mixhull! A dungeon fica ao leste."**
4. Posicionar na área do Hub (ex: perto do centro)

---

### 12.6 Criar o NPC de Save

Este NPC salva o progresso do player quando interage.

1. Clique direito na Hierarchy → **Create Empty**
2. Renomear para **"SaveNPC"**
3. No Inspector:
   - Adicionar **SpriteRenderer** → arrastar um sprite placeholder
   - Adicionar **BoxCollider2D**:
     - **isTrigger = true**
     - Ajustar Size (ex: X = 1, Y = 2)
   - Adicionar **SaveInteractable**:
     - `_playerHealth`: arrastar o **Player** da cena → selecionar o componente **PlayerHealth** (expandir o Player na Hierarchy se necessário, arrastar o componente, não o GameObject)
     - `_dialogueText`: digitar **"Progresso salvo!"** (ou o texto que quiser)
4. Posicionar em outro ponto do Hub

> O SaveInteractable salva `currentHealth` e `maxHealth` do PlayerHealth em `Application.persistentDataPath/save.json`.

---

### 12.7 Criar o Portal para a Dungeon

1. Clique direito na Hierarchy → **Create Empty**
2. Renomear para **"DungeonPortal"**
3. No Inspector:
   - Adicionar **SpriteRenderer** → arrastar um sprite placeholder de porta/portal
   - Adicionar **BoxCollider2D**:
     - **isTrigger = true**
     - Ajustar Size (ex: X = 1.5, Y = 2)
   - Adicionar **SceneTransition**:
     - `_targetScene`: digitar **"Main"** (exatamente assim, com M maiúsculo, sem .unity)
4. Posicionar em um canto da cena (ex: lado direito)
5. **Verificar:** a cena "Main" precisa estar no Build Settings para a transição funcionar

---

### 12.8 Configurar a Câmera no Hub

1. Selecionar a **Main Camera** na cena Hub
2. No Inspector: **Add Component → CameraFollow**
3. Arrastar no campo `_target`: o **Player** da cena
4. `_smoothTime`: deixar **0.15** (padrão) ou ajustar a gosto
5. Se quiser câmera seguindo na dungeon também: repetir os passos na cena Main

---

### 12.9 Criar o HubManager

1. Clique direito na Hierarchy → **Create Empty**
2. Renomear para **"HubManager"**
3. No Inspector: **Add Component → HubManager**
4. Arrastar no campo `_playerHealth`: o componente **PlayerHealth** do Player
5. O HubManager carrega o save (se existir) quando a cena inicia

---

### 12.10 Configurar o GameManager na Dungeon

1. Abrir a cena **Main** (dungeon)
2. Selecionar o objeto **GameManager** na Hierarchy
3. No Inspector:
   - `_sceneName`: mudar de `"Main"` para **"Hub"**
4. Agora quando o player morre na dungeon, volta para o Hub em vez de reiniciar a dungeon

---

### 12.11 Build Settings Final

1. **File → Build Settings**
2. Verificar que as cenas estão listadas na ordem:
   - **Index 0:** `Assets/Scenes/Hub.unity` ← jogo começa aqui
   - **Index 1:** `Assets/Scenes/Main.unity` ← dungeon
3. Se Hub não estiver na lista: abrir a cena Hub → **Add Open Scenes**
4. Se Main não estiver na lista: abrir a cena Main → **Add Open Scenes**

---

### 12.12 Como Testar em Play Mode

Siga esta sequência para validar que tudo funciona:

1. Abra a cena **Hub**
2. Clique em **Play** no Unity
3. O jogo abre no Hub — verify que o player aparece e se move
4. Caminhe até o NPC → o painel de diálogo aparece com o texto
5. Pressione **E** → o painel de diálogo some
6. Caminhe até o DungeonPortal → a cena muda para a dungeon
7. Gameplay normal na dungeon (movimentação, ataque, inimigos)
8. Deixe o player morrer → a cena volta para o Hub
9. Caminhe até o SaveNPC → pressione E → o texto "Progresso salvo!" aparece
10. Verifique o save: `Application.persistentDataPath` → existe um arquivo `save.json` com os dados

**Fluxo completo:** Hub → portal → dungeon → morrer → voltar ao Hub → salvar → reiniciar

---

### 12.13 Hierarquia da Cena Hub (referência)

```
Scene "Hub"
├── Main Camera
│   └── CameraFollow.cs (_target = Player)
├── Player (Player.prefab)
│   ├── PlayerController.cs
│   ├── PlayerHealth.cs
│   └── WeaponController.cs
├── HubManager
│   └── HubManager.cs (_playerHealth = Player)
├── DialogueUI
│   └── DialogueUI.cs
│       └── Canvas
│           └── DialoguePanel (desativado)
│               └── DialogueText (TextMeshProUGUI)
├── NPC
│   ├── SpriteRenderer
│   ├── BoxCollider2D (isTrigger = true)
│   └── NPCInteractable.cs (_dialogueText = "...")
├── SaveNPC
│   ├── SpriteRenderer
│   ├── BoxCollider2D (isTrigger = true)
│   └── SaveInteractable.cs (_playerHealth = Player)
├── DungeonPortal
│   ├── SpriteRenderer
│   ├── BoxCollider2D (isTrigger = true)
│   └── SceneTransition.cs (_targetScene = "Main")
├── PlayerSpawn (Empty, posição de spawn)
└── Tilemap / Paredes
```

---

### 12.14 Troubleshooting

| Problema | Solução |
|----------|---------|
| Cena Hub não abre | Verificar Build Settings: Hub deve estar no Index 0 |
| Transição não funciona | Verificar `_targetScene` = `"Main"` no SceneTransition, cena Main no Build Settings |
| Diálogo não aparece | Verificar: DialogueUI.Instance existe na cena, DialoguePanel está desativado por default |
| NPC não detecta player | Verificar: BoxCollider2D tem `isTrigger = true`, player está na Layer "player" (minúsculo) |
| Save não persiste | Verificar: permissões de escrita em `Application.persistentDataPath` |
| Câmera treme | Usar `LateUpdate`, `SmoothDamp`, verificar se tem Pixel Perfect Camera conflitando |
| Player não spawna no Hub | Verificar: Player.prefab está na cena Hub, posição correta |
| SaveNPC não salva | Verificar: `_playerHealth` está conectado ao PlayerHealth do Player no Inspector |
| Erro NullReference no DialogueUI | Verificar: GameObject DialogueUI existe na cena com o script DialogueUI.cs |
| Erro "Scene not found" | Verificar: nome da cena exato no `_targetScene`, cena listada no Build Settings |

---

## 13. Fase 7 — Dungeon Procedural

### 13.1 Arquivos Criados

| Arquivo | Função |
|---------|--------|
| `Assets/Scripts/Dungeon/RoomType.cs` | Enums `RoomType` e `FloorType` |
| `Assets/Scripts/Dungeon/FloorConfigSO.cs` | ScriptableObject de configuração de andares |
| `Assets/Scripts/Dungeon/FloorManager.cs` | Controla andar atual, progressão, tipo de andar |
| `Assets/Scripts/Dungeon/DungeonGenerator.cs` | Gera salas via Random Walk com seed |
| `Assets/Scripts/Dungeon/FloorTransition.cs` | Trigger para avançar andar |
| `Assets/Scripts/Dungeon/DifficultyScaler.cs` | Multiplicadores de dificuldade por andar |
| `Assets/Scripts/Dungeon/BossFloorHandler.cs` | Fluxo de andar de boss |
| `Assets/Scripts/Dungeon/RewardArea.cs` | Sala de recompensa pós-boss |
| `Assets/Scripts/Dungeon/TrapBase.cs` | Trap que causa dano no player e inimigos |
| `Assets/Scripts/Dungeon/RunCurrency.cs` | Economia de runas da run atual |
| `Assets/Scripts/Dungeon/RunePickup.cs` | Pickup de runa dropado por inimigos |
| `Assets/Scripts/Dungeon/RunUpgradeSO.cs` | ScriptableObject de dados de upgrade |
| `Assets/Scripts/Dungeon/RunUpgradeManager.cs` | Gerencia upgrades temporários da run |
| `Assets/Scripts/Dungeon/HealingShrine.cs` | Altar de cura |
| `Assets/Scripts/Dungeon/RiskRewardAltar.cs` | Altar de risco/recompensa |
| `Assets/Scripts/UI/FloorUI.cs` | Texto na tela com andar atual e runas |

### 13.2 Arquivos Modificados

| Arquivo | O que mudou |
|---------|-------------|
| `Assets/Scripts/Rooms/RoomController.cs` | Novos métodos: `InitializeReferences`, `InitializeDoors`, `SetActiveDoors`, `SetRoomType`, `SetEliteMode`, `IsCleared`, `GetDoors`, `GetEnemyPrefab`, `IncrementEnemiesAlive`. Spawn lógico por `RoomType`. Difficulty scaling aplicado nos inimigos. |
| `Assets/Scripts/Enemies/EnemyController.cs` | Novos: `_isElite` (cor amarela, escala 1.3x), `ApplyDifficulty(hpMult, dmgMult)`, `RuneValue`, `DropRune()` no `DieSequence`. |
| `Assets/Scripts/Core/GameManager.cs` | `OnPlayerDeath()` agora destrói `FloorManager`, `RunCurrency`, `RunUpgradeManager` antes de carregar cena. |
| `Assets/Scripts/Core/SaveData.cs` | Novo campo: `maxFloorReached`. |
| `Assets/Scripts/Combat/PlayerHealth.cs` | Novos métodos: `HealPercent(float)`, `SetMaxHealth(int)`. |
| `Assets/Scripts/Movement/PlayerController.cs` | Nova propriedade: `MoveSpeed`. Novo método: `SetMoveSpeed(float)`. |

### 13.3 Prefabs a Criar na Unity

#### 13.3.1 TrapSpike.prefab

1. Na pasta `Assets/Prefabs/`: **Create Empty** → renomear "TrapSpike"
2. Adicionar componentes:
   - **SpriteRenderer** → atribuir sprite de espinho (qualquer sprite placeholder do SunnyLand, ex: um tile de chão)
   - **BoxCollider2D** → marcar `Is Trigger = true`, Size: (1.5, 0.5), Offset Y: 0.25
   - **TrapBase.cs** → Inspector: `_damage`: 5, `_damageInterval`: 0.5, `_trapType`: "Spike"
3. Arrastar para `Assets/Prefabs/` para criar o prefab

#### 13.3.2 RunePickup.prefab

1. Na pasta `Assets/Prefabs/`: **Create Empty** → renomear "RunePickup"
2. Adicionar componentes:
   - **SpriteRenderer** → atribuir sprite de gem/item do SunnyLand
   - **CircleCollider2D** → marcar `Is Trigger = true`, Radius: 0.3
   - **RunePickup.cs** → Inspector: `_runeValue`: 1
3. Arrastar para `Assets/Prefabs/`

#### 13.3.3 HealingShrine.prefab

1. Na pasta `Assets/Prefabs/`: **Create Empty** → renomear "HealingShrine"
2. Adicionar componentes:
   - **SpriteRenderer** → atribuir sprite de altar/objeto decorativo
   - **BoxCollider2D** → marcar `Is Trigger = true`, Size: (1.5, 2), Offset Y: 0.5
   - **HealingShrine.cs** → Inspector: `_healPercent`: 0.25, `_dialogueText`: "Você sente sua força retornar..."
3. Arrastar para `Assets/Prefabs/`

#### 13.3.4 RiskRewardAltar.prefab

1. Na pasta `Assets/Prefabs/`: **Create Empty** → renomear "RiskRewardAltar"
2. Adicionar componentes:
   - **SpriteRenderer** → atribuir sprite de altar/objeto decorativo (pode ser diferente do HealingShrine)
   - **BoxCollider2D** → marcar `Is Trigger = true`, Size: (1.5, 2), Offset Y: 0.5
   - **RiskRewardAltar.cs** → Inspector: `_damageCost`: 25, `_runeReward`: 20, `_dialogueText`: "Aceitar dano em troca de runas? Pressione E"
3. Arrastar para `Assets/Prefabs/`

#### 13.3.5 Door.prefab (já existe — verificar)

O prefab `Assets/Prefabs/Door.prefab` já existe. Verificar se tem:

- **Door.cs** → campo `_collider` conectado ao próprio Collider2D
- **BoxCollider2D** → `Is Trigger = false`, Size cobrindo a área da porta
- **SpriteRenderer** com sprite de porta

Se estiver OK, pular. Se não existir ou estiver corrompido, criar:

1. **Create Empty** → renomear "Door"
2. Adicionar: SpriteRenderer, BoxCollider2D (isTrigger=false), Door.cs
3. No Door.cs → arrastar o BoxCollider2D para o campo `_collider`

#### 13.3.6 DungeonRoom.prefab

Este é o prefab principal. Será a sala padrão usada pelo DungeonGenerator.

**Passo 1 — Criar a raiz:**

1. Na cena (não no prefab ainda): **Create Empty** → renomear "DungeonRoom"
2. Adicionar componentes na raiz:
   - **RoomController.cs** → NÃO configurar campos agora (DungeonGenerator seta via code)
   - **BoxCollider2D** → marcar `Is Trigger = true`, Size: (18, 10) — esta é a área de detecção do player para ativar a sala
3. Na Layer da raiz: deixar **Default**

**Passo 2 — Criar filhos (Empty GameObjects):**

Criar estes filhos dentro de DungeonRoom (Create Empty para cada um):

| Filho | Posição (Local) | Componentes | Notas |
|-------|-----------------|-------------|-------|
| `RoomCenter` | (0, 0, 0) | Nenhum | Ponto central da sala para lógica de fechar portas |
| `SpawnPoint1` | (-4, -2, 0) | Nenhum | Posição de spawn de inimigo |
| `SpawnPoint2` | (4, -2, 0) | Nenhum | Posição de spawn de inimigo |
| `EliteSpawnPoint` | (0, -2, 0) | Nenhum | Posição de spawn de inimigo elite |
| `DoorLeft` | (-9, -3, 0) | Arrastar `Door.prefab` existente | Porta esquerda — ver detalhes abaixo |
| `DoorRight` | (9, -3, 0) | Arrastar `Door.prefab` existente | Porta direita — ver detalhes abaixo |
| `Ground` | (0, -5, 0) | SpriteRenderer | Sprite de chão — usar tile/ground do SunnyLand. Scale X: 20, Scale Y: 1 |
| `Walls` | Ver abaixo | Ver abaixo | Ver detalhes abaixo |

**Passo 3 — Configurar o Ground:**

O Ground é o chão visual da sala. Ele não precisa de collider — a colisão é feita pelo WallBottom.

1. Dentro de DungeonRoom, clique direito → **Create Empty** → renomear para "Ground"
2. No Inspector:
   - Adicionar **SpriteRenderer**
   - No campo **Sprite**: arrastar um sprite de chão do SunnyLand (procurar por "ground", "tile" ou "floor" nos sprites importados)
   - **Posição local:** X = 0, Y = -5, Z = 0
   - **Scale:** X = 20, Y = 1
3. **NÃO adicionar BoxCollider2D** — o chão é puramente visual. A colisão é do WallBottom.

> O Scale X: 20 estica o sprite para cobrir toda a largura da sala. Scale Y: 1 mantém a altura normal.

Visualização:

```
┌────────────────────┐  ← WallTop (Y=5)
│                    │
│   Sala jogável     │
│                    │
└════════════════════┘  ← Ground (Y=-5, visual) / WallBottom (Y=-5.5, colisão)
```

**Passo 4 — Configurar DoorLeft e DoorRight:**

Usar o prefab existente `Assets/Prefabs/Door.prefab` como prefab aninhado:

1. No Project, selecione `Assets/Prefabs/Door.prefab`
2. Arraste-o para **dentro** do GameObject DungeonRoom na Hierarchy — isso cria uma instância aninhada
3. Renomeie para "DoorLeft"
4. Posição local: **(-9, -3, 0)** — borda esquerda, altura do chão
5. **Duplique** DoorLeft (Ctrl+D) — renomeie a cópia para "DoorRight"
6. Posição local de DoorRight: **(9, -3, 0)** — borda direita
7. Para AMBAS as portas: **desmarcar o checkbox ativo** no Inspector (☐ no topo do GameObject). O DungeonGenerator ativa quando necessário.

> Door.prefab já tem Door.cs, BoxCollider2D e SpriteRenderer configurados. Não precisa adicionar nada manualmente.

**Passo 5 — Configurar Walls:**

Criar filhos dentro de um Empty chamado "Walls" (posição local do Walls: 0, 0, 0):

| Wall | Posição Local | BoxCollider2D Size | Layer |
|------|--------------|-------------------|-------|
| `WallTop` | (0, 5, 0) | (20, 0.5) | Ground (11) |
| `WallBottom` | (0, -5.5, 0) | (20, 0.5) | Ground (11) |
| `WallLeft` | (-10, 0, 0) | (0.5, 11) | Ground (11) |

> WallRight NÃO é criado — é a abertura para a próxima sala. Se a sala for isolada (última), o DungeonGenerator ativa a porta direita.

Alternativa simplificada: usar 1 Empty "Walls" com 1 BoxCollider2D (Size 20, 11, Is Trigger=false, Layer Ground). Mas o player não conseguiria sair pela direita.

**Recomendado para início:** Criar apenas WallTop e WallBottom com BoxCollider2D. As portas servem como barreiras laterais.

**Passo 6 — Conectar no Inspector do RoomController (raiz):**

No componente RoomController.cs da raiz DungeonRoom:

- `_spawnPoints`: expandir array para 2, arrastar SpawnPoint1 e SpawnPoint2
- `_eliteSpawnPoint`: arrastar EliteSpawnPoint
- `_eliteEnemyPrefab`: **deixar vazio** (DungeonGenerator seta)
- `_enemyPrefab`: **deixar vazio** (DungeonGenerator seta)
- `_doors`: expandir array para 2, arrastar DoorLeft e DoorRight
- `_player`: **deixar vazio** (DungeonGenerator seta)
- `_roomCenter`: arrastar RoomCenter
- `_roomType`: Combat (default)

**Passo 7 — Converter em Prefab:**

1. Arrastar o GameObject "DungeonRoom" da Hierarchy para `Assets/Prefabs/`
2. Deletar o DungeonRoom da cena

**Estrutura final do prefab:**

```
DungeonRoom (RoomController.cs, BoxCollider2D isTrigger)
├── RoomCenter
├── SpawnPoint1
├── SpawnPoint2
├── EliteSpawnPoint
├── DoorLeft (Door.prefab aninhado) [DESATIVADO]
├── DoorRight (Door.prefab aninhado) [DESATIVADO]
├── Ground (SpriteRenderer, Scale 20,1,1)
└── Walls
    ├── WallTop (BoxCollider2D, Layer Ground)
    ├── WallBottom (BoxCollider2D, Layer Ground)
    └── WallLeft (BoxCollider2D, Layer Ground)
```

#### 13.3.7 ExitRoom.prefab

1. **Duplicar** o prefab DungeonRoom (Ctrl+D no Project)
2. Renomear a cópia para "ExitRoom"
3. Abrir o prefab (duplo clique)
4. Criar filho **ExitTrigger**:
   - Posição local: (8, -2, 0) — lado direito da sala, perto do chão
   - Adicionar: **BoxCollider2D** → `Is Trigger = true`, Size: (1.5, 2.5)
   - Adicionar: **FloorTransition.cs**
5. Salvar e fechar o prefab

**Estrutura final:**

```
ExitRoom (RoomController.cs, BoxCollider2D isTrigger)
├── RoomCenter
├── SpawnPoint1
├── SpawnPoint2
├── EliteSpawnPoint
├── DoorLeft (Door.prefab aninhado) [DESATIVADO]
├── DoorRight (Door.prefab aninhado) [DESATIVADO]
├── ExitTrigger (BoxCollider2D isTrigger, FloorTransition.cs)
├── Ground (SpriteRenderer)
└── Walls
    ├── WallTop (BoxCollider2D, Layer Ground)
    ├── WallBottom (BoxCollider2D, Layer Ground)
    └── WallLeft (BoxCollider2D, Layer Ground)
```

#### 13.3.8 BossArena_5.prefab

1. Na cena: **Create Empty** → renomear "BossArena_5"
2. Adicionar componentes na raiz:
   - **RoomController.cs**
   - **BossFloorHandler.cs**
   - **BoxCollider2D** → `Is Trigger = true`, Size: (24, 12) — arena maior que sala normal

3. Criar filhos:

| Filho | Posição Local | Componentes | Notas |
|-------|--------------|-------------|-------|
| `RoomCenter` | (0, 0, 0) | Nenhum | |
| `BossSpawnPoint` | (0, 0, 0) | Nenhum | Boss spawna no centro |
| `PlayerSpawnPoint` | (-10, -3, 0) | Nenhum | Player entra pela esquerda |
| `DoorLeft` | (-12, -3, 0) | Arrastar `Door.prefab` existente | Trava player na arena |
| `DoorRight` | (12, -3, 0) | Arrastar `Door.prefab` existente | Trava player na arena |
| `Ground` | (0, -6, 0) | SpriteRenderer | Chão largo |
| `Walls` | (0, 0, 0) | Ver abaixo | |

1. Configurar Walls (mesmo padrão do DungeonRoom):
   - `WallTop` (0, 6, 0) — BoxCollider2D Size (24, 0.5), Layer Ground
   - `WallBottom` (0, -6.5, 0) — BoxCollider2D Size (24, 0.5), Layer Ground

2. Conectar no Inspector:
   - **RoomController.cs**: `_doors`: [DoorLeft, DoorRight], `_roomCenter`: RoomCenter, `_roomType`: Boss
   - **BossFloorHandler.cs**: `_bossPrefab`: Enemy.prefab (por enquanto), `_bossSpawnPoint`: BossSpawnPoint, `_roomController`: self (arrastar BossArena_5)

3. Arrastar para `Assets/Prefabs/`, deletar da cena

**Estrutura final:**

```
BossArena_5 (RoomController.cs, BossFloorHandler.cs, BoxCollider2D isTrigger)
├── RoomCenter
├── BossSpawnPoint
├── PlayerSpawnPoint
├── DoorLeft (Door.prefab aninhado) [DESATIVADO]
├── DoorRight (Door.prefab aninhado) [DESATIVADO]
├── Ground (SpriteRenderer)
└── Walls
    ├── WallTop (BoxCollider2D, Layer Ground)
    └── WallBottom (BoxCollider2D, Layer Ground)
```

#### 13.3.9 BossArena_10.prefab e BossArena_15.prefab

1. **Duplicar** BossArena_5 duas vezes (Ctrl+D, Ctrl+D)
2. Renomear para "BossArena_10" e "BossArena_15"
3. (Opcional) Ajustar posição dos filhos para layouts diferentes
4. Por enquanto podem ser idênticos ao BossArena_5

> Na fase futura, cada arena terá layout visual diferente e boss com moveset próprio.

#### 13.3.10 RewardRoom.prefab

1. Na cena: **Create Empty** → renomear "RewardRoom"
2. Adicionar componentes na raiz:
   - **RewardArea.cs**
   - **BoxCollider2D** → `Is Trigger = true`, Size: (16, 10)

3. Criar filhos:

| Filho | Posição Local | Componentes | Notas |
|-------|--------------|-------------|-------|
| `PlayerSpawnPoint` | (-7, -3, 0) | Nenhum | Player entra pela esquerda |
| `ExitTrigger` | (6, -2, 0) | BoxCollider2D (isTrigger, 1.5x2.5), FloorTransition.cs | Saída para próximo andar |
| `Ground` | (0, -5, 0) | SpriteRenderer | Chão |
| `Walls` → `WallTop` | (0, 5, 0) | BoxCollider2D (16, 0.5), Layer Ground | |
| `Walls` → `WallBottom` | (0, -5.5, 0) | BoxCollider2D (16, 0.5), Layer Ground | |

1. Conectar no Inspector do RewardArea.cs: `_exitTrigger`: arrastar ExitTrigger
2. Arrastar para `Assets/Prefabs/`, deletar da cena

**Estrutura final:**

```
RewardRoom (RewardArea.cs, BoxCollider2D isTrigger)
├── PlayerSpawnPoint
├── ExitTrigger (BoxCollider2D isTrigger, FloorTransition.cs)
├── Ground (SpriteRenderer)
└── Walls
    ├── WallTop (BoxCollider2D, Layer Ground)
    └── WallBottom (BoxCollider2D, Layer Ground)
```

#### Checklist de Prefabs

Depois de criar todos, verifique em `Assets/Prefabs/`:

- [ ] `TrapSpike.prefab` — tem TrapBase.cs + BoxCollider2D isTrigger
- [ ] `RunePickup.prefab` — tem RunePickup.cs + CircleCollider2D isTrigger
- [ ] `HealingShrine.prefab` — tem HealingShrine.cs + BoxCollider2D isTrigger
- [ ] `RiskRewardAltar.prefab` — tem RiskRewardAltar.cs + BoxCollider2D isTrigger
- [ ] `DungeonRoom.prefab` — tem RoomController.cs, RoomTrigger isTrigger, 2 SpawnPoints, 2 Doors (desativados), Ground, Walls
- [ ] `ExitRoom.prefab` — igual DungeonRoom + ExitTrigger com FloorTransition.cs
- [ ] `BossArena_5.prefab` — tem RoomController.cs, BossFloorHandler.cs, BossSpawnPoint, PlayerSpawnPoint, 2 Doors
- [ ] `BossArena_10.prefab` — cópia do BossArena_5
- [ ] `BossArena_15.prefab` — cópia do BossArena_5
- [ ] `RewardRoom.prefab` — tem RewardArea.cs, PlayerSpawnPoint, ExitTrigger com FloorTransition.cs

### 13.4 ScriptableObjects a Criar na Unity

1. **Criar FloorConfig:**
   - `Assets → Create → Dungeon → Floor Config`
   - Salvar como `Assets/Data/DefaultFloorConfig.asset`
   - Inspector: `_baseRoomCount`: 4, `_maxRoomCount`: 8, `_baseRoomWidth`: 20, `_roomWidthScalingPerFloor`: 0.05, `_bossFloors`: [5, 10, 15], `_hpScalingPerFloor`: 0.1, `_dmgScalingPerFloor`: 0.08, `_trapChance`: 0.3

2. **Criar Upgrades (em `Assets/Data/Upgrades/`):**
   - `Assets → Create → Dungeon → Run Upgrade`
   - `UpgradeDamage.asset`: name="Dano+", cost=15, type=Damage, value=1.2
   - `UpgradeSpeed.asset`: name="Velocidade+", cost=10, type=Speed, value=1.15
   - `UpgradeHealth.asset`: name="Vida+", cost=20, type=MaxHealth, value=1.25
   - `UpgradeBerserker.asset`: name="Berserker", cost=10, type=Damage, value=1.5, hasTradeOff=true, tradeOffType=MaxHealth, tradeOffValue=0.75

### 13.5 Configuração na Cena Main

1. Abrir cena `Main.unity`
2. **Remover a Room estática** da cena (se existir)
3. Criar GameObjects na Hierarchy:

**FloorManager:**

- Create Empty → renomear "FloorManager"
- Adicionar `FloorManager.cs`
- Inspector: `_dungeonGenerator` = arrastar DungeonGenerator, `_floorUI` = arrastar FloorUI, `_player` = arrastar Player, `_floorConfig` = arrastar DefaultFloorConfig.asset

**DungeonGenerator:**

- Create Empty → renomear "DungeonGenerator"
- Adicionar `DungeonGenerator.cs`
- Inspector: `_roomPrefab` = DungeonRoom.prefab, `_exitRoomPrefab` = ExitRoom.prefab, `_bossArenaPrefabs` = [BossArena_5, BossArena_10, BossArena_15], `_rewardRoomPrefab` = RewardRoom.prefab, `_eliteEnemyPrefab` = Enemy.prefab (com_isElite=true), `_trapPrefabs` = [TrapSpike.prefab], `_runePickupPrefab` = RunePickup.prefab, `_healingShrinePrefab` = HealingShrine.prefab, `_riskRewardAltarPrefab` = RiskRewardAltar.prefab, `_player` = Player, `_seed` = 0

**RunCurrency:**

- Create Empty → renomear "RunCurrency"
- Adicionar `RunCurrency.cs`
- Inspector: `_floorUI` = arrastar FloorUI

**RunUpgradeManager:**

- Create Empty → renomear "RunUpgradeManager"
- Adicionar `RunUpgradeManager.cs`
- Inspector: `_playerHealth` = PlayerHealth do Player, `_playerController` = PlayerController do Player

**FloorUI (Canvas):**

- UI → Canvas (Screen Space - Overlay, Canvas Scaler: Scale With Screen Size 1920x1080)
- Dentro do Canvas: UI → Text - TextMeshPro → "FloorText" (top center, size 32, branco)
- Dentro do Canvas: UI → Text - TextMeshPro → "RuneText" (top right, size 28, amarelo, texto "Runas: 0")
- Create Empty → "FloorUI" (filho do Canvas)
- Adicionar `FloorUI.cs`
- Inspector: `_floorText` = FloorText TMP, `_runeText` = RuneText TMP

1. **Verificar GameManager:**
   - Selecionar GameManager na cena
   - `_sceneName` deve ser "Hub"
   - `_playerHealth` deve estar conectado ao PlayerHealth do Player

### 13.6 Hierarquia Final da Cena Main

```
Scene "Main"
├── Main Camera
│   └── CameraFollow.cs (_target = Player)
├── Player (Player.prefab)
│   ├── PlayerController.cs
│   ├── PlayerHealth.cs
│   └── WeaponController.cs
├── GameManager (GameManager.cs)
├── FloorManager (FloorManager.cs)
├── DungeonGenerator (DungeonGenerator.cs)
├── RunCurrency (RunCurrency.cs)
├── RunUpgradeManager (RunUpgradeManager.cs)
└── Canvas
    ├── FloorText (TextMeshProUGUI)
    ├── RuneText (TextMeshProUGUI)
    └── FloorUI (FloorUI.cs)
```

> Rooms são gerados dinamicamente pelo DungeonGenerator. Não deve haver Room estático na cena.

### 13.7 Como Testar

1. Abrir cena **Main**
2. Clicar **Play**
3. **Verificações iniciais:**
   - FloorUI mostra "Floor 1" no topo da tela
   - Runas mostra "Runas: 0" no canto superior direito
   - Salas são geradas — player spawna na sala "start"
4. **Testar combate:**
   - Andar para a direita/esquerda → entrar em sala de combate
   - Inimigos spawneam automaticamente
   - Matar inimigos → runas dropam no chão
   - Andar sobre runas → "Runas: X" atualiza na UI
   - Matar todos → portas abrem
5. **Testar progressão:**
   - Chegar na última sala (ExitRoom) → andar avança para 2
   - Layout diferente — novas salas geradas
6. **Testar morte:**
   - Deixar inimigos matar o player → cena volta para Hub
   - Voltar para Main (via portal) → andar 1, runas 0
7. **Testar boss (avançar até andar 5):**
   - Andar 5 carrega arena de boss
   - Boss spawna com stats escalados
   - Matar boss → player recupera vida → transição para RewardRoom
8. **Testar traps:**
   - Trap causa dano ao player ao contato
   - Levar inimigo para cima da trap → inimigo toma dano

### 13.8 Troubleshooting

| Problema | Solução |
|----------|---------|
| Salas não aparecem | Verificar _roomPrefab no DungeonGenerator. RoomController deve estar no prefab. |
| Player não spawna | Verificar _player no FloorManager/DungeonGenerator |
| Salas sobrepostas | Verificar _roomWidth/_roomHeight corresponde ao tamanho real do prefab |
| Portas não funcionam | Verificar Door prefabs nos filhos da sala, _doors no RoomController |
| Boss não spawna | Verificar _bossPrefab e_bossSpawnPoint no BossFloorHandler |
| Andar não avança | Verificar FloorTransition no ExitTrigger, isTrigger=true no BoxCollider2D |
| Inimigos fracos | Verificar _hpScalingPerFloor e_dmgScalingPerFloor no FloorConfigSO |
| NullReference na geração | Verificar todos os prefabs atribuídos no Inspector do DungeonGenerator |
| FloorUI não aparece | Verificar FloorUI.cs conectado, TextMeshProUGUI arrastados |
| Trap não causa dano | Verificar isTrigger=true, layer "player" ou "Enemy" |
| Runas não dropam | Verificar _runePickupPrefab no DungeonGenerator |
| Runas não resetam | Verificar GameManager chama RunCurrency.ResetRunes() |
| Upgrade não aplica | Verificar RunUpgradeManager refs: _playerHealth,_playerController |
| HealingShrine não cura | Verificar isTrigger=true, PlayerHealth tem HealPercent() |
| RiskRewardAltar não responde | Verificar InputAction E habilitado, _playerInRange |
| Erro "Instance is null" | Verificar singleton GameObjects existem na cena antes de Play |

---

## 14. Fase 8 — Sistema de Armas

Esta fase substitui os campos hardcoded do `WeaponController` por um sistema baseado em ScriptableObject. Cada arma tem stats próprios (dano, cooldown, alcance, knockback, padrão de ataque) e um visual placeholder distinto. O player pode trocar de arma pressionando Q ou coletando pickups no chão.

---

### 14.1 Visão Geral do Sistema

**Antes (Fase 4):**

- `WeaponController` tinha `_damage`, `_attackCooldown`, `_attackDuration`, `_knockbackForce` como campos fixos no Inspector
- Só existia a Sword — não havia como trocar de arma
- Spawn usava `_player.right * 0.8f` — direção fixa, não usava `FacingDirection`
- Dano era direto, sem multiplicador de run

**Depois (Fase 8):**

- Stats vêm de `WeaponDataSO` — um ScriptableObject por arma
- Player carrega uma lista de armas disponíveis (`_availableWeapons`)
- Q cicla entre armas; pickups adicionam novas armas à lista
- Spawn usa `PlayerController.FacingDirection` — ataque na direção certa (esquerda/direita)
- Dano final = `weapon.damage × RunUpgradeManager.DamageMultiplier`
- Cada arma tem visual placeholder distinto (child com cor única)

**Fluxo de código — Ataque:**

```
Player pressiona Mouse/Enter/Gamepad
        ↓
WeaponController.Attack()
  → verifica _isAttacking, _cooldownTimer, _equippedWeapon
  → _isAttacking = true
  → SpawnHitbox()
      → pega FacingDirection do PlayerController
      → calcula offset via GetAttackOffset(facingDir)
      → Instantiate(_hitboxPrefab) na posição correta
      → ajusta localScale: xScale = dir.x < 0 ? -hitboxScale : hitboxScale
      → SwordHitbox.Initialize(this) — limpa HashSet de hits
  → StartCoroutine(AttackRoutine())
      → espera attackDuration
      → destroi hitbox
      → _isAttacking = false
      → _cooldownTimer = attackCooldown
        ↓
SwordHitbox.OnTriggerEnter2D(colisor)
  → verifica HashSet (já acertou? ignora)
  → adiciona ao HashSet
  → chama WeaponController.OnHitboxTrigger(colisor)
      → pega IDamageable do alvo
      → calcula finalDamage = weapon.damage
      → se RunUpgradeManager existe: finalDamage *= DamageMultiplier
      → damageable.TakeDamage(finalDamage)
      → aplica knockback no Rigidbody2D
```

**Fluxo de código — Troca de arma (pickup):**

```
Player encosta em WeaponPickup
        ↓
WeaponPickup.OnTriggerEnter2D(colisor)
  → verifica layer = "Player"
  → pega WeaponController via GetComponentInParent
  → wc.EquipWeapon(_weaponData)
  → Destroy(gameObject) — pickup consumido
        ↓
WeaponController.EquipWeapon(weapon)
  → adiciona na _availableWeapons se não existe
  → se _isAttacking: return (não equipa agora, mas arma já está na lista)
  → _equippedWeapon = weapon
  → _currentWeaponIndex = index na lista
  → _visualController.EquipVisual(weapon.weaponType)
```

**Fluxo de código — Troca de arma (Q):**

```
Player pressiona Q
        ↓
WeaponController.CycleWeapon()
  → se _availableWeapons.Count <= 1: return (não faz nada)
  → se _isAttacking: return
  → _currentWeaponIndex = (index + 1) % count
  → _equippedWeapon = _availableWeapons[index]
  → _visualController.EquipVisual(weapon.weaponType)
```

---

### 14.2 Arquivos Criados

| Arquivo | Função |
|---------|--------|
| `Assets/Scripts/Combat/WeaponType.cs` | Enum: `Sword`, `Spear`, `Axe`, `Dagger` |
| `Assets/Scripts/Combat/AttackPattern.cs` | Enum: `HorizontalSwing`, `ForwardThrust`, `OverheadSmash`, `QuickStab` |
| `Assets/Scripts/Combat/WeaponDataSO.cs` | ScriptableObject com todos os dados de uma arma |
| `Assets/Scripts/Combat/WeaponVisualController.cs` | Ativa/desativa child visual conforme arma equipada. Espelha sprite (flipX) e posição X da arma conforme FacingDirection do player. |
| `Assets/Scripts/Combat/WeaponPickup.cs` | Trigger no chão que equipa arma ao player. Espelha sprite conforme FacingDirection do player. |

---

### 14.3 Arquivos Modificados

| Arquivo | O que mudou |
|---------|-------------|
| `Assets/Scripts/Combat/WeaponController.cs` | Refatorado: lê stats de `WeaponDataSO`, usa `FacingDirection` do `PlayerController`, adiciona `EquipWeapon()`, lista `_availableWeapons` com swap por Q (tecla), `GetAttackOffset()` por `AttackPattern`, `BaseHitboxSize` constante, integração com `RunUpgradeManager.GetDamageMultiplier()`. `EquipWeapon()` sempre adiciona à lista; durante ataque não equipa mas arma entra no ciclo de Q. `Awake()` sincroniza visual com arma default. |
| `Assets/Scripts/Combat/SwordHitbox.cs` | Adicionado `HashSet<Collider2D> _hitTargets` para prevenir multi-hit. `Initialize()` limpa o set a cada ataque. |

---

### 14.4 ScriptableObjects — Criar as 4 Armas

#### 14.4.1 Criar a pasta

1. No **Project window**, navegar até `Assets/Data/`
2. Clique direito → **Create → Folder**
3. Renomear para **"Weapons"**
4. Caminho final: `Assets/Data/Weapons/`

#### 14.4.2 Criar WeaponSword.asset

1. Clique direito na pasta `Assets/Data/Weapons/`
2. **Create → Combat → Weapon Data**
3. Um asset aparece chamado "New Weapon Data"
4. Renomear para **"WeaponSword"**
5. Selecionar o asset — no Inspector, preencher:

**Seção Identity:**

| Campo | Valor |
|-------|-------|
| weaponType | Sword (selecionar no dropdown) |
| weaponName | `Sword` |

**Seção Stats:**

| Campo | Valor | Explicação |
|-------|-------|------------|
| damage | 10 | Dano base por acerto |
| attackCooldown | 0.3 | Segundos entre ataques |
| attackDuration | 0.15 | Segundos que a hitbox fica ativa |
| attackRange | 1.1 | Alcance em unidades. BaseHitboxSize = 0.8, então scale = 1.375 |
| knockbackForce | 2.0 | Força de empurrão no alvo |

**Seção Attack Pattern:**

| Campo | Valor |
|-------|-------|
| attackPattern | HorizontalSwing |

**Seção Visual:**

| Campo | Valor | Como escolher |
|-------|-------|---------------|
| placeholderColor | R: 0.85, G: 0.85, B: 0.9, A: 1 | Cinza claro — usar o color picker ou digitar os valores |

#### 14.4.3 Criar WeaponSpear.asset

1. Clique direito na pasta `Assets/Data/Weapons/`
2. **Create → Combat → Weapon Data**
3. Renomear para **"WeaponSpear"**
4. Preencher no Inspector:

**Seção Identity:**

| Campo | Valor |
|-------|-------|
| weaponType | Spear |
| weaponName | `Spear` |

**Seção Stats:**

| Campo | Valor | Explicação |
|-------|-------|------------|
| damage | 8 | Menor dano que sword, mas mais alcance |
| attackCooldown | 0.4 | Mais lenta que sword |
| attackDuration | 0.20 | Hitbox fica ativa um pouco mais |
| attackRange | 1.6 | Maior alcance — scale = 1.6/0.8 = 2.0 |
| knockbackForce | 1.5 | Menor knockback |

**Seção Attack Pattern:**

| Campo | Valor |
|-------|-------|
| attackPattern | ForwardThrust |

**Seção Visual:**

| Campo | Valor |
|-------|-------|
| placeholderColor | R: 0.3, G: 0.5, B: 0.9, A: 1 | Azul |

#### 14.4.4 Criar WeaponAxe.asset

1. Clique direito na pasta `Assets/Data/Weapons/`
2. **Create → Combat → Weapon Data**
3. Renomear para **"WeaponAxe"**
4. Preencher no Inspector:

**Seção Identity:**

| Campo | Valor |
|-------|-------|
| weaponType | Axe |
| weaponName | `Axe` |

**Seção Stats:**

| Campo | Valor | Explicação |
|-------|-------|------------|
| damage | 18 | Maior dano — arma lenta e pesada |
| attackCooldown | 0.6 | Mais lenta |
| attackDuration | 0.25 | Hitbox fica ativa mais tempo |
| attackRange | 1.2 | Alcance — scale = 1.2/0.8 = 1.5 |
| knockbackForce | 4.0 | Knockback alto — empurra muito |

**Seção Attack Pattern:**

| Campo | Valor |
|-------|-------|
| attackPattern | OverheadSmash |

**Seção Visual:**

| Campo | Valor |
|-------|-------|
| placeholderColor | R: 0.9, G: 0.3, B: 0.3, A: 1 | Vermelho |

#### 14.4.5 Criar WeaponDagger.asset

1. Clique direito na pasta `Assets/Data/Weapons/`
2. **Create → Combat → Weapon Data**
3. Renomear para **"WeaponDagger"**
4. Preencher no Inspector:

**Seção Identity:**

| Campo | Valor |
|-------|-------|
| weaponType | Dagger |
| weaponName | `Dagger` |

**Seção Stats:**

| Campo | Valor | Explicação |
|-------|-------|------------|
| damage | 5 | Dano baixo — compensado pela velocidade |
| attackCooldown | 0.15 | Muito rápida — quase 2x mais rápido que sword |
| attackDuration | 0.18 | Hitbox fica ativa tempo suficiente para acertar |
| attackRange | 0.9 | Alcance — scale = 0.9/0.8 = 1.125 |
| knockbackForce | 1.0 | Knockback mínimo |

**Seção Attack Pattern:**

| Campo | Valor |
|-------|-------|
| attackPattern | QuickStab |

**Seção Visual:**

| Campo | Valor |
|-------|-------|
| placeholderColor | R: 0.3, G: 0.8, B: 0.3, A: 1 | Verde |

#### 14.4.6 Verificação

Depois de criar os 4 assets, verificar em `Assets/Data/Weapons/`:

```
Assets/Data/Weapons/
├── WeaponSword.asset    ← weaponType=Sword, damage=10, cor=cinza
├── WeaponSpear.asset    ← weaponType=Spear, damage=8, cor=azul
├── WeaponAxe.asset      ← weaponType=Axe, damage=18, cor=vermelho
└── WeaponDagger.asset   ← weaponType=Dagger, damage=5, cor=verde
```

> Cada arma base tem um único `WeaponDataSO`. Não criar dois assets para o mesmo `WeaponType`. A lista `_availableWeapons` compara por referência do SO.

---

### 14.5 Ajustar o Prefab SwordHitbox

Este passo ajusta o tamanho base do collider da hitbox para corresponder à constante `BaseHitboxSize = 0.8f` no `WeaponController`.

1. No **Project window**, navegar até `Assets/Prefabs/`
2. **Duplo clique** no prefab `SwordHitbox` para abrir no Prefab Editor
3. Na Hierarchy do prefab, selecionar o objeto **SwordHitbox**
4. No Inspector, procurar o componente **BoxCollider2D**
5. Alterar o campo **Size**:
   - **X: 0.8** (era 0.5)
   - **Y: 0.8** (era 0.5)
6. Verificar que **Is Trigger** continua marcado como `true`
7. **Salvar** o prefab (Ctrl+S ou botão Save no topo do Prefab Editor)
8. Fechar o Prefab Editor (seta "Back" no canto superior esquerdo da Hierarchy)

**Por que 0.8?** O `WeaponController` usa a fórmula `attackRange / BaseHitboxSize` para calcular o `localScale` da hitbox. Com `BaseHitboxSize = 0.8`:

- Sword (range 1.1): scale = 1.375 → hitbox com 1.1×1.1 unidades
- Spear (range 1.6): scale = 2.0 → hitbox com 1.6×1.6 unidades
- Axe (range 1.2): scale = 1.5 → hitbox com 1.2×1.2 unidades
- Dagger (range 0.9): scale = 1.125 → hitbox com 0.9×0.9 unidades

---

### 14.6 Atualizar o Prefab Player — WeaponController

Abrir o prefab do Player para editar. Todos os passos abaixo são feitos **dentro do Prefab Editor**.

#### 14.6.1 Abrir o Prefab

1. No **Project window**, navegar até `Assets/Prefabs/`
2. **Duplo clique** no `Player.prefab` — abre o Prefab Editor
3. Na Hierarchy, você verá o Player e seus filhos (GroundCheck, etc.)

#### 14.6.2 Reconfigurar o WeaponController

O componente `WeaponController` foi refatorado. Os campos antigos (`_damage`, `_attackCooldown`, `_attackDuration`, `_knockbackForce`) foram **removidos** do script. Os novos campos aparecerão no Inspector automaticamente após o Unity recompilar.

1. Na Hierarchy do prefab, selecionar o **Player** (raiz)
2. No Inspector, procurar o componente **WeaponController**
3. Você verá a seção **"Setup"** com 4 campos novos:

**Preencher cada campo:**

| Campo | O que fazer | Onde encontrar |
|-------|-------------|----------------|
| `_defaultWeapon` | Arrastar o asset **WeaponSword** | `Assets/Data/Weapons/WeaponSword.asset` |
| `_hitboxPrefab` | Arrastar o prefab **SwordHitbox** | `Assets/Prefabs/SwordHitbox.prefab` |
| `_player` | Arrastar o **Player** (o próprio objeto raiz do prefab) | No próprio prefab — arrastar da Hierarchy do Prefab Editor |
| `_visualController` | Arrastar o componente **WeaponVisualController** | Será adicionado no próximo passo — por enquanto pode deixar vazio e voltar depois |

> **Atenção:** O campo `_player` deve ser o Transform do próprio Player. No Prefab Editor, arraste o objeto raiz "Player" da Hierarchy para este campo. Isso garante que a hitbox seja instanciada como filho do player e que a posição de spawn funcione.

> **Dica:** Se o campo `_visualController` ainda não existe o componente, deixe-o vazio por agora. Você vai voltar a ele no Passo 14.7 depois de criar o WeaponVisualController.

---

### 14.7 Atualizar o Prefab Player — Visuais de Arma

Ainda dentro do Prefab Editor do Player, criar os 4 visuais placeholder e o componente WeaponVisualController.

> O `WeaponVisualController` agora espelha o sprite (flipX) e a posição X da arma automaticamente conforme o `FacingDirection` do player. Quando o player vira para a esquerda, o sprite da arma vira e a posição X é invertida para o outro lado do player.

#### 14.7.1 Criar Visual_Sword

1. Na Hierarchy do Prefab Editor, clicar direito no **Player** (raiz) → **Create Empty**
2. Renomear para **"Visual_Sword"**
3. No Inspector, verificar a posição local: **X: 0.3, Y: 0, Z: 0** (offset na frente/mão do personagem — ajustar conforme o sprite)
4. Adicionar componente **SpriteRenderer**
5. No SpriteRenderer, configurar:

| Campo | Valor |
|-------|-------|
| Sprite | Qualquer sprite como placeholder (pode usar o mesmo sprite de idle do player, ou um quadrado simples) |
| Color | R: 0.85, G: 0.85, B: 0.9, A: 1 (cinza claro) |
| Sorting Layer | Mesmo layer do player (ex: "Default" ou o layer que o player usa) |
| Order in Layer | Ordem do player + 1 (ex: se player = 0, usar 1). Isso garante que o visual apareça **na frente** do sprite do player, não atrás |

1. O GameObject Visual_Sword deve estar **ATIVO** (checkbox marcado no topo do Inspector) — é a arma default

#### 14.7.2 Criar Visual_Spear

1. Na Hierarchy do Prefab Editor, clicar direito no **Player** → **Create Empty**
2. Renomear para **"Visual_Spear"**
3. Posição local: **mesma posição do Visual_Sword** (ex: X: 0.3, Y: 0, Z: 0) — todos os visuais ficam no mesmo ponto
4. Adicionar componente **SpriteRenderer**
5. Configurar:

| Campo | Valor |
|-------|-------|
| Sprite | Mesmo sprite placeholder usado no Visual_Sword |
| Color | R: 0.3, G: 0.5, B: 0.9, A: 1 (azul) |
| Sorting Layer | Mesmo do Visual_Sword |
| Order in Layer | Mesmo do Visual_Sword (ex: 1) |

1. **DESATIVAR** este GameObject — desmarcar o checkbox ativo no topo do Inspector (☐). Será ativado pelo `WeaponVisualController` quando o player pegar a spear

#### 14.7.3 Criar Visual_Axe

1. Clicar direito no **Player** → **Create Empty**
2. Renomear para **"Visual_Axe"**
3. Posição local: **mesma posição** (X: 0.3, Y: 0, Z: 0)
4. Adicionar **SpriteRenderer**:

| Campo | Valor |
|-------|-------|
| Sprite | Mesmo sprite placeholder |
| Color | R: 0.9, G: 0.3, B: 0.3, A: 1 (vermelho) |
| Sorting Layer | Mesmo |
| Order in Layer | Mesmo (ex: 1) |

1. **DESATIVAR** — desmarcar checkbox ativo

#### 14.7.4 Criar Visual_Dagger

1. Clicar direito no **Player** → **Create Empty**
2. Renomear para **"Visual_Dagger"**
3. Posição local: **mesma posição** (X: 0.3, Y: 0, Z: 0)
4. Adicionar **SpriteRenderer**:

| Campo | Valor |
|-------|-------|
| Sprite | Mesmo sprite placeholder |
| Color | R: 0.3, G: 0.8, B: 0.3, A: 1 (verde) |
| Sorting Layer | Mesmo |
| Order in Layer | Mesmo (ex: 1) |

1. **DESATIVAR** — desmarcar checkbox ativo

#### 14.7.5 Adicionar WeaponVisualController

1. Selecionar o **Player** (raiz) na Hierarchy do Prefab Editor
2. No Inspector, clicar **Add Component**
3. Digitar "WeaponVisualController" e selecionar o script
4. O componente aparece com 4 campos vazios na seção **"Visual References"**

**Arrastar cada referência:**

| Campo no Inspector | Arrastar da Hierarchy |
|--------------------|-----------------------|
| `_visualSword` | Visual_Sword |
| `_visualSpear` | Visual_Spear |
| `_visualAxe` | Visual_Axe |
| `_visualDagger` | Visual_Dagger |

#### 14.7.6 Conectar _visualController no WeaponController

Agora volte ao componente **WeaponController** no Player:

1. Selecionar o **Player** (raiz)
2. Expandir o componente **WeaponController**
3. No campo `_visualController`, arrastar o componente **WeaponVisualController** do mesmo Player

> **Como arrastar componente:** No Inspector, clique no pequeno círculo ao lado do campo `_visualController` — uma janela aparece listando os componentes do Player. Selecione "WeaponVisualController". Alternativamente, arraste o Player da Hierarchy para o campo e depois expandir o dropdown para selecionar o componente específico.

#### 14.7.7 Verificar estado dos visuais

Antes de salvar, verifique na Hierarchy do Prefab Editor:

| GameObject | Estado (checkbox no topo) | Cor no SpriteRenderer |
|------------|--------------------------|----------------------|
| Visual_Sword | **ATIVO** ✓ | R: 0.85, G: 0.85, B: 0.9 |
| Visual_Spear | **DESATIVADO** ☐ | R: 0.3, G: 0.5, B: 0.9 |
| Visual_Axe | **DESATIVADO** ☐ | R: 0.9, G: 0.3, B: 0.3 |
| Visual_Dagger | **DESATIVADO** ☐ | R: 0.3, G: 0.8, B: 0.3 |

#### 14.7.8 Salvar e sair

1. **Ctrl+S** para salvar o prefab
2. Clicar a seta **"Back"** no canto superior esquerdo da Hierarchy (ou "←" ao lado do nome do prefab) para sair do Prefab Editor

#### 14.7.9 Hierarquia final do Player

```
Player
├── Rigidbody2D
├── BoxCollider2D
├── PlayerController.cs
├── PlayerHealth.cs
├── WeaponController.cs
│   ├── _defaultWeapon = WeaponSword.asset
│   ├── _hitboxPrefab = SwordHitbox.prefab
│   ├── _player = Player (this.transform)
│   └── _visualController = WeaponVisualController (componente do mesmo Player)
├── WeaponVisualController.cs
│   ├── _visualSword = Visual_Sword
│   ├── _visualSpear = Visual_Spear
│   ├── _visualAxe = Visual_Axe
│   └── _visualDagger = Visual_Dagger
├── Visual_Sword (Empty, SpriteRenderer, color=cinza, ATIVO ✓)
│   └── SpriteRenderer (sprite placeholder, sorting acima do player)
├── Visual_Spear (Empty, SpriteRenderer, color=azul, DESATIVADO ☐)
│   └── SpriteRenderer
├── Visual_Axe (Empty, SpriteRenderer, color=vermelho, DESATIVADO ☐)
│   └── SpriteRenderer
├── Visual_Dagger (Empty, SpriteRenderer, color=verde, DESATIVADO ☐)
│   └── SpriteRenderer
└── GroundCheck (Empty, existente)
```

---

### 14.8 Criar o Prefab WeaponPickup

Este é o item que aparece no chão e que o player coleta para obter uma nova arma.

#### 14.8.1 Criar o GameObject

1. Na **Hierarchy** de qualquer cena aberta (pode ser a cena Main ou uma vazia)
2. Clique direito → **Create Empty**
3. Renomear para **"WeaponPickup"**

#### 14.8.2 Adicionar SpriteRenderer

1. Selecionar o WeaponPickup na Hierarchy
2. No Inspector: **Add Component → SpriteRenderer**
3. Configurar:

| Campo | Valor | Nota |
|-------|-------|------|
| Sprite | Gem/item do SunnyLand (qualquer sprite de item coletável) | Placeholder — será substituído por sprite final depois |
| Color | Branco (padrão) | A cor visual vem do WeaponDataSO, não do SpriteRenderer do pickup |
| Sorting Layer | Default (ou o layer dos itens no chão) |
| Order in Layer | 0 (ou acima do chão) |

#### 14.8.3 Adicionar CircleCollider2D

1. **Add Component → CircleCollider2D**
2. Configurar:

| Campo | Valor |
|-------|-------|
| Is Trigger | **true** (marcar — obrigatório para detecção) |
| Radius | 0.3 |

> O `Is Trigger = true` é essencial. Sem isso, o player colide fisicamente com o pickup e não detecta a coleta via `OnTriggerEnter2D`.

#### 14.8.4 Adicionar WeaponPickup.cs

1. **Add Component → WeaponPickup**
2. No campo `_weaponData`: **deixar vazio**

> Cada instância do pickup na cena/configura o seu `_weaponData` individualmente. O prefab fica com o campo vazio. Quando você arrastar o prefab para a cena e quiser que ele seja uma Spear, você arrasta `WeaponSpear.asset` para o `_weaponData` daquela instância.

#### 14.8.5 Converter em Prefab

1. No **Project window**, navegar até `Assets/Prefabs/`
2. **Arrastar** o GameObject "WeaponPickup" da Hierarchy para a pasta `Assets/Prefabs/`
3. O Unity pergunta se quer criar um Prefab — confirmar
4. O prefab `WeaponPickup.prefab` aparece na pasta
5. **Deletar** o WeaponPickup da Hierarchy — ele só existe como prefab agora

#### 14.8.6 Hierarquia do Prefab

```
WeaponPickup (prefab)
├── SpriteRenderer (gem/item placeholder, color=white)
├── CircleCollider2D (Is Trigger = true, Radius = 0.3)
└── WeaponPickup.cs
    └── _weaponData: vazio (configurar em cada instância)
```

---

### 14.9 Criar a Cena de Teste (WeaponTestScene)

Esta cena isolada permite testar o sistema de armas sem depender da geração de dungeon.

#### 14.9.1 Criar a cena

1. **File → New Scene → Basic (Built-in)**
2. Uma cena vazia abre (Main Camera + Directional Light)
3. **File → Save As**
4. Navegar até `Assets/Scenes/`
5. Salvar como **"WeaponTestScene.unity"**

> Não precisa adicionar ao Build Settings — esta cena é só para desenvolvimento.

#### 14.9.2 Adicionar o Player

1. No **Project window**, navegar até `Assets/Prefabs/`
2. **Arrastar** `Player.prefab` para a Hierarchy da cena
3. Posicionar o Player: **X: 0, Y: 0, Z: 0** (centro da cena)
4. O Player já vem com `PlayerController`, `PlayerHealth`, `WeaponController`, `WeaponVisualController` e os 4 visuais configurados

#### 14.9.3 Configurar a Câmera

1. Selecionar a **Main Camera** na Hierarchy
2. Adicionar componente **CameraFollow** (se não existir)
3. No campo `_target`: arrastar o **Player** da cena
4. `_smoothTime`: 0.15 (padrão)
5. Ajustar a posição Z da câmera para o que funcionar (ex: Z: -10) para ver o player

#### 14.9.4 Criar o Chão

1. Na Hierarchy, clicar direito → **Create Empty**
2. Renomear para **"Chao"**
3. Adicionar **SpriteRenderer**:
   - Sprite: qualquer sprite de chão/grande do SunnyLand (ou usar um quadrado simples)
   - Color: marrom/cinza (ex: R: 0.4, G: 0.3, B: 0.2)
   - Scale: **X: 30, Y: 1, Z: 1** (largo o suficiente para andar)
   - Posição: **X: 0, Y: -3, Z: 0** (abaixo do player)
4. Adicionar **BoxCollider2D**:
   - Is Trigger: **false** (colisão física, não trigger)
   - Size: ajustar para cobrir o sprite (ex: X: 30, Y: 1)
5. Definir a **Layer** como **Ground** (se a layer Ground existe no projeto)

#### 14.9.5 Criar os Pickups de Arma

Para cada arma que não é a default (Spear, Axe, Dagger), criar uma instância do prefab WeaponPickup.

**Pickup_Spear:**

1. No Project, selecionar `Assets/Prefabs/WeaponPickup.prefab`
2. **Arrastar** para a Hierarchy
3. Renomear a instância para **"Pickup_Spear"**
4. Posicionar: **X: 3, Y: -2, Z: 0** (à direita do player, perto do chão)
5. No Inspector da instância, expandir o componente **WeaponPickup**
6. No campo `_weaponData`: arrastar **WeaponSpear.asset** de `Assets/Data/Weapons/`

**Pickup_Axe:**

1. Arrastar `WeaponPickup.prefab` para a Hierarchy novamente
2. Renomear para **"Pickup_Axe"**
3. Posicionar: **X: 6, Y: -2, Z: 0**
4. No `_weaponData`: arrastar **WeaponAxe.asset**

**Pickup_Dagger:**

1. Arrastar `WeaponPickup.prefab` para a Hierarchy
2. Renomear para **"Pickup_Dagger"**
3. Posicionar: **X: 9, Y: -2, Z: 0**
4. No `_weaponData`: arrastar **WeaponDagger.asset**

> Os pickups ficam em linha reta à direita do player, cada um a 3 unidades de distância. Assim é fácil testar coletando um por vez.

#### 14.9.6 (Opcional) Adicionar Dummy Enemy

1. No Project, selecionar `Assets/Prefabs/Enemy.prefab`
2. Arrastar para a Hierarchy
3. Renomear para **"DummyEnemy"**
4. Posicionar: **X: -3, Y: -2, Z: 0** (à esquerda do player)
5. No componente EnemyController: `_player` pode ser deixado vazio ou arrastar o Player

> Se o EnemyController precisa de `_player` setado via `Initialize()`, pode ser necessário criar um dummy setup. Alternativamente, pular este passo e testar dano apenas na cena Main.

#### 14.9.7 Hierarquia final da cena

```
WeaponTestScene
├── Main Camera
│   └── CameraFollow.cs (_target = Player, _smoothTime = 0.15)
├── Directional Light (padrão da cena)
├── Player (Player.prefab)
│   ├── PlayerController.cs
│   ├── PlayerHealth.cs
│   ├── WeaponController.cs (_defaultWeapon = Sword, _hitboxPrefab = SwordHitbox)
│   ├── WeaponVisualController.cs (refs conectadas)
│   ├── Visual_Sword (ATIVO, cinza)
│   ├── Visual_Spear (DESATIVO, azul)
│   ├── Visual_Axe (DESATIVO, vermelho)
│   └── Visual_Dagger (DESATIVO, verde)
├── Chao (SpriteRenderer + BoxCollider2D, Layer = Ground)
├── Pickup_Spear (WeaponPickup, _weaponData = WeaponSpear.asset)
├── Pickup_Axe (WeaponPickup, _weaponData = WeaponAxe.asset)
├── Pickup_Dagger (WeaponPickup, _weaponData = WeaponDagger.asset)
└── DummyEnemy (Enemy.prefab, opcional)
```

#### 14.9.8 O que NÃO adicionar nesta cena

- NÃO adicionar `FloorManager` — não é necessário para teste de armas
- NÃO adicionar `DungeonGenerator` — não gerar salas
- NÃO adicionar `RunCurrency` — não testar economia aqui
- `RunUpgradeManager` é **opcional** — se não estiver na cena, o dano usa o valor base do `WeaponDataSO` sem multiplicador (comportamento correto, sem erro)
- NÃO adicionar `GameManager` — não testar morte/restart aqui
- NÃO adicionar `Canvas`/`FloorUI` — não testar UI aqui

> Esta cena é puramente para testar: atacar, trocar arma, visual, hitbox, dano, knockback.

---

### 14.10 Fluxo de Dano Explicado

Entender como o dano é calculado ajuda a debugar se algo parece errado.

#### Cálculo do dano

```
Dano Final = WeaponDataSO.damage × RunUpgradeManager.DamageMultiplier
```

- `WeaponDataSO.damage` — valor fixo por arma (Sword=10, Spear=8, Axe=18, Dagger=5)
- `RunUpgradeManager.DamageMultiplier` — começa em 1.0, aumenta com upgrades de dano
- Se `RunUpgradeManager.Instance` não existe na cena → multiplicador implícito = 1.0 → dano = valor do SO puro

#### Exemplos

| Arma | damage | DamageMultiplier | Dano Final |
|------|--------|-----------------|------------|
| Sword | 10 | 1.0 (sem upgrades) | 10 |
| Sword | 10 | 1.2 (upgrade Dano+) | 12 |
| Axe | 18 | 1.0 | 18 |
| Axe | 18 | 1.5 (Berserker) | 27 |
| Dagger | 5 | 1.0 | 5 |

#### Hitbox Scale

O tamanho da hitbox é calculado assim:

```
localScale = attackRange / BaseHitboxSize
```

`BaseHitboxSize = 0.8f` (constante no WeaponController). O BoxCollider2D do prefab SwordHitbox tem Size = 0.8.

| Arma | attackRange | Scale | Tamanho real da hitbox |
|------|-------------|-------|----------------------|
| Sword | 1.1 | 1.375 | 1.1 × 1.1 unidades |
| Spear | 1.6 | 2.0 | 1.6 × 1.6 unidades |
| Axe | 1.2 | 1.5 | 1.2 × 1.2 unidades |
| Dagger | 0.9 | 1.125 | 0.9 × 0.9 unidades |

#### Attack Pattern (Offset de Ataque)

Cada `AttackPattern` define onde a hitbox aparece relativa ao player:

| Pattern | Offset X | Offset Y | Sensação |
|---------|----------|----------|----------|
| HorizontalSwing | facingDir.x × range | +0.2 | Lateral, levemente acima |
| ForwardThrust | facingDir.x × (range + 0.2) | 0 | Reta para frente, mais longe |
| OverheadSmash | facingDir.x × 0.3 | range × 0.5 | Curto à frente, alto |
| QuickStab | facingDir.x × range | 0 | Reta para frente, curto |

> `facingDir` é o `PlayerController.FacingDirection` — sempre horizontal (esquerda = -1, direita = +1). Suporte vertical fica para fase futura.

---

### 14.11 Como Testar em Play Mode

#### Teste 1 — Arma Default (Sword)

1. Abrir a cena **WeaponTestScene**
2. Clicar **Play** no Unity
3. Verificações:
   - O player aparece no centro da cena
   - O **Visual_Sword** (cinza) está visível ao lado/frente do player
   - Visual_Spear, Visual_Axe, Visual_Dagger estão invisíveis
4. Pressionar **mouse esquerdo** (ou Enter, ou botão A do gamepad)
5. Verificações:
   - Uma hitbox aparece brevemente na frente do player (na direção que ele olha)
    - A hitbox desaparece após ~0.15 segundos
   - Se houver um DummyEnemy na frente, ele toma dano (fica vermelho)
6. Andar para a esquerda (tecla A) e atacar novamente:
   - A hitbox aparece na **esquerda** (não na direita)
   - O visual da arma também vira para a esquerda (flipX + posição X invertida)
   - Confirma que `FacingDirection` está funcionando

#### Teste 2 — Troca de Arma via Pickup

1. Com a Sword equipada (default), andar para a direita até o **Pickup_Axe** (X: 6)
2. Verificações:
   - O pickup desaparece ao contato
   - O **Visual_Axe** (vermelho) fica visível
   - Visual_Sword é desativado
3. Atacar com a Axe:
   - O ataque é mais lento (cooldown 0.6s vs 0.3s)
   - O knockback é maior (4.0 vs 2.0)
   - O offset é diferente — a hitbox aparece mais acima (OverheadSmash)
4. Andar sobre **Pickup_Spear**:
   - Visual muda para azul
   - Atacar → hitbox maior (attackRange 1.3)
5. Andar sobre **Pickup_Dagger**:
   - Visual muda para verde
   - Atacar → hitbox menor, ataque muito rápido

#### Teste 3 — Swap com Q

1. Iniciar o teste com apenas a Sword (default)
2. Pressionar **Q**:
   - Nada acontece — só tem 1 arma na lista
3. Coletar o Pickup_Axe → agora tem 2 armas
4. Pressionar **Q**:
   - A arma muda! Visual troca de vermelho (axe) para cinza (sword)
   - Atacar → usa a Sword (ataque mais rápido)
5. Pressionar **Q** novamente:
   - Volta para Axe
6. Coletar Pickup_Spear → 3 armas na lista
7. Pressionar **Q** várias vezes:
   - Cicla: Sword → Axe → Spear → Sword → Axe → ...

#### Teste 4 — Bloqueio de Troca Durante Ataque

1. Equipar a Axe
2. Atacar → durante os 0.25s de duração da hitbox, andar sobre o Pickup_Spear
3. Verificações:
   - O pickup é **destruído** (consumido)
   - A Spear é **adicionada à lista** `_availableWeapons`
   - Mas a arma equipada continua sendo a **Axe** (não trocou)
   - O visual continua vermelho (axe)
4. Após o ataque terminar:
   - Pressionar **Q** → a Spear agora aparece no ciclo
   - A arma equipada agora pode ser trocada normalmente

#### Teste 5 — Hitbox Não Acerta Múltiplas Vezes

1. Posicionar o player perto de um DummyEnemy
2. Atacar → o enemy deve tomar dano **uma vez** apenas
3. Verificar no Console que `TakeDamage` é chamado uma vez por ataque

---

### 14.12 Checklist de Validação

Depois de configurar tudo, passe por esta lista:

- [x] Scripts compilam sem erro (ver Console do Unity)
- [x] 4 ScriptableObject assets existem em `Assets/Data/Weapons/`
- [x] Cada SO tem `weaponType`, `damage`, `attackRange`, `attackPattern`, `placeholderColor` preenchidos
- [x] Player prefab tem `WeaponVisualController` com 4 referências conectadas
- [x] Player prefab tem `WeaponController` com `_defaultWeapon` = WeaponSword.asset
- [x] Player prefab tem 4 visuais (Visual_Sword/Spear/Axe/Dagger)
- [x] Visual_Sword está ATIVO, os outros 3 estão DESATIVADOS
- [x] SwordHitbox prefab tem BoxCollider2D Size = 0.8
- [x] WeaponPickup prefab tem CircleCollider2D com Is Trigger = true
- [x] WeaponTestScene existe em `Assets/Scenes/`
- [x] Na cena de teste: Player, Chão, 3 pickups com `_weaponData` configurado
- [x] Atacar com sword → hitbox aparece na direção correta (esquerda/direita)
- [x] Coletar pickup → visual muda
- [x] Pressionar Q → ciclo entre armas
- [x] Coletar pickup durante ataque → arma entra na lista, não equipa
- [x] Inimigo toma dano uma vez por ataque (sem multi-hit)

---

### 14.13 Troubleshooting

| Problema | Causa provável | Solução |
|----------|---------------|---------|
| Hitbox não aparece | `_defaultWeapon` não conectado | Arrastar WeaponSword.asset para o campo `_defaultWeapon` do WeaponController |
| Hitbox não aparece | `_hitboxPrefab` não conectado | Arrastar SwordHitbox.prefab para o campo `_hitboxPrefab` |
| Hitbox não aparece | SwordHitbox não tem `SwordHitbox.cs` | Abrir prefab, verificar componente SwordHitbox.cs existe |
| Erro NullReference no SpawnHitbox | `_player` não conectado | Arrastar o Player (this.transform) para o campo `_player` |
| Visual não muda | `_visualController` não conectado | Arrastar o componente WeaponVisualController para o campo `_visualController` |
| Visual não muda | WeaponVisualController refs vazias | Verificar os 4 campos (_visualSword, etc.) estão conectados |
| Visual não muda | Visual_* desativado incorretamente | Visual_Sword deve estar ATIVO; os outros DESATIVADOS |
| Todos os visuais aparecem juntos | Mais de um visual está ativo | Desativar Visual_Spear, Visual_Axe, Visual_Dagger |
| Visual some atrás do player | Order in Layer errado | Aumentar Order in Layer dos visuais (ex: +1 acima do player) |
| Swap Q não funciona | Só tem 1 arma na lista | Coletar um pickup primeiro — Q precisa de pelo menos 2 armas |
| Swap Q não funciona | `_availableWeapons` está vazio | Verificar `_defaultWeapon` está conectado — Awake adiciona na lista |
| Dano errado | Valores errados no SO | Abrir o WeaponDataSO asset e verificar `damage` |
| Dano errado | RunUpgradeManager não existe na cena | Comportamento esperado: dano usa valor puro do SO (multiplicador = 1.0) |
| Pickup não funciona | `Is Trigger` não marcado no CircleCollider2D | Marcar Is Trigger = true |
| Pickup não funciona | `_weaponData` vazio na instância | Arrastar o WeaponDataSO.asset correto para o campo `_weaponData` |
| Pickup não funciona | Player não está na layer "Player" | Verificar Layer do Player no prefab |
| Pickup não é consumido | Script WeaponPickup não está no prefab | Abrir prefab, adicionar componente WeaponPickup.cs |
| Troca durante ataque equipa errado | — | Comportamento esperado: `EquipWeapon()` adiciona à lista mas não equipa se `_isAttacking`. Pickup não é perdido. |
| Hitbox acerta múltiplas vezes | HashSet não está funcionando | Verificar que `SwordHitbox.cs` foi atualizado (tem `HashSet<Collider2D> _hitTargets`) |
| Hitbox tamanho errado | BoxCollider2D Size não é 0.8 | Abrir SwordHitbox.prefab, ajustar Size para X=0.8 Y=0.8 |
| Hitbox tamanho errado | attackRange no SO errado | Verificar valor no asset correspondente |
| Ataque não respeita direção | FacingDirection não funciona | Verificar PlayerController tem a propriedade pública `FacingDirection` |
| Erro "_defaultWeapon não atribuído" | Campo vazio no Inspector | Arrastar WeaponSword.asset para `_defaultWeapon` |
| Visual_Sword não aparece | Sprite não atribuído | Adicionar qualquer sprite no SpriteRenderer do Visual_Sword |

---

## 15. Fase 9 — Inimigos Avançados e Ecossistema de Combate

Esta fase adiciona tipos de inimigos avançados (Fast, Heavy, Ranged), uma classe base compartilhada (`EnemyBase`), bosses com padrões de ataque únicos, e distribuição de inimigos por andar.

---

### 15.1 Visão Geral do Sistema

**Antes (Fase 8):**

- Todos os inimigos usavam `EnemyController` com comportamento de Melee puro
- Só existia 1 tipo de inimigo — perseguia e batia no player
- Bosses não existiam — boss floor spawnavam Enemy normal
- Sem projéteis inimigos
- Layer check usava `NameToLayer("Player")` hardcoded

**Depois (Fase 9):**

- `EnemyBase` como classe compartilhada para todos os inimigos e bosses
- 4 arquétipos: Melee, Fast, Heavy, Ranged — cada um com comportamento distinto
- `IBoss` interface — bosses com padrões únicos (combo, area, híbrido)
- `EnemyProjectile` — projétil com Rigidbody2D velocity, ignora layer Enemy
- `DangerZone` — zona de dano por tick no chão
- Distribuição de inimigos por andar (Melee no andar 1, Ranged só a partir do 7)
- Layer check padronizado com `[SerializeField] LayerMask _playerLayer`

**Hierarquia de Scripts:**

```
EnemyBase (MonoBehaviour, IDamageable)
├── EnemyController (Melee, Fast, Heavy, Ranged)
├── BossMeleeController (Boss 1 — andar 5)
├── BossAreaController (Boss 2 — andar 10)
└── BossHybridController (Boss 3 — andar 15)

IBoss (interface)
├── BossMeleeController
├── BossAreaController
└── BossHybridController
```

**Distribuição por Floor:**

| Floor | Inimigos possíveis |
|-------|-------------------|
| 1–2 | Melee |
| 3–4 | Melee, Fast |
| 5–6 | Melee, Fast, Heavy |
| 7+ | Melee, Fast, Heavy, Ranged |

> Melee sempre está na pool — uma sala nunca spawna apenas ranged ou apenas heavy.

**Fluxo de código — Spawn de inimigos:**

```
DungeonGenerator.GenerateFloor(floor)
        ↓
GetEnemyPrefabsForFloor(floor)
  → Melee sempre na pool
  → floor >= 3: + Fast
  → floor >= 5: + Heavy
  → floor >= 7: + Ranged
        ↓
rc.SetEnemyPrefabs(pool)
        ↓
RoomController.OnTriggerEnter2D(player)
  → SpawnCombatEnemies()
      → prefabToSpawn = _enemyPrefabs[Random.Range(0, count)]
      → Instantiate(prefab) em cada spawn point
      → controller.Initialize(_player)
      → controller.ApplyDifficulty(hpMult, dmgMult, runeMult)
      → AddComponent<EnemyDeathTracker>()
```

**Fluxo de código — Comportamento por tipo:**

```
EnemyController.Update()
  → switch(_enemyType):
      Melee:   ChasePlayer() — persegue, pula obstáculos
      Fast:    ChasePlayerAggressive() — persegue sem pausas
      Heavy:   HeavyBehavior() — charge quando perto (< 3 unidades)
      Ranged:  RangedBehavior() — mantém distância, atira projéteis
               → Se não acerta por 5s: reposiciona (_noHitTimer)
               → ShootProjectile() → Instantiate EnemyProjectile
```

**Fluxo de código — Projétil:**

```
EnemyController.ShootProjectile()
  → dir = (player.position - transform.position).normalized
  → Instantiate(_projectilePrefab)
  → ep.Initialize(dir, _damageToPlayer, this)
        ↓
EnemyProjectile.Initialize()
  → _rb.linearVelocity = dir * _speed
  → Destroy(gameObject, _lifetime) — auto-destroy após 3s
        ↓
EnemyProjectile.OnTriggerEnter2D(other)
  → Ignora layer Enemy (bitwise check)
  → IDamageable.TakeDamage(_damage)
  → _owner.OnProjectileHit() — reseta _noHitTimer
  → Destroy(gameObject)
```

---

### 15.2 Arquivos Criados

| Arquivo | Tipo | Função |
|---------|------|--------|
| `Assets/Scripts/Enemies/EnemyType.cs` | Enum | `Melee`, `Fast`, `Heavy`, `Ranged` |
| `Assets/Scripts/Enemies/EnemyBase.cs` | Classe base | TakeDamage, OnDeath, Drop de runas, HP, ApplyDifficulty |
| `Assets/Scripts/Enemies/EnemyProjectile.cs` | MonoBehaviour | Projétil com Rigidbody2D velocity, ignora Enemy layer |
| `Assets/Scripts/Bosses/IBoss.cs` | Interface | `void Initialize(Transform player)` |
| `Assets/Scripts/Bosses/BossMeleeController.cs` | MonoBehaviour | Boss 1 — combos melee, enrage em 30% HP |
| `Assets/Scripts/Bosses/BossAreaController.cs` | MonoBehaviour | Boss 2 — danger zones, projéteis cadentes |
| `Assets/Scripts/Bosses/BossHybridController.cs` | MonoBehaviour | Boss 3 — alternância melee/ranged |
| `Assets/Scripts/Bosses/DangerZone.cs` | MonoBehaviour | Zona de dano por tick, auto-destroy |

---

### 15.3 Arquivos Modificados

| Arquivo | Mudanças |
|---------|----------|
| `Assets/Scripts/Enemies/EnemyController.cs` | Herda `EnemyBase`. Métodos isolados por tipo. LayerMask serializado `_playerLayer`. |
| `Assets/Scripts/Rooms/RoomController.cs` | Novo: `_enemyPrefabs[]`, `SetEnemyPrefabs()`, layer check padronizado. |
| `Assets/Scripts/Dungeon/DungeonGenerator.cs` | Novo: `_enemyFastPrefab`, `_enemyHeavyPrefab`, `_enemyRangedPrefab`, `GetEnemyPrefabsForFloor()`. |
| `Assets/Scripts/Dungeon/BossFloorHandler.cs` | Novo: `_bossPrefabs[]`, usa `IBoss.Initialize()`, suporte a array de bosses. |

---

### 15.4 Layer EnemyProjectile

#### 15.4.1 Criar a layer

1. No Unity: **Edit → Project Settings**
2. Na janela, clicar na aba **Tags and Layers**
3. Expandir **Layers** (não Tags)
4. Procurar o primeiro campo vazio após as layers existentes (provavelmente no índice 12)
5. Digitar: **`EnemyProjectile`**
6. Pressionar Enter para confirmar

> A layer EnemyProjectile é usada para que o projétil não colida com outros inimigos.

#### 15.4.2 Configurar Collision Matrix

1. No Unity: **Edit → Project Settings**
2. Na janela, clicar na aba **Physics 2D**
3. Na parte inferior, existe a **Layer Collision Matrix** — uma tabela de checkboxes
4. Encontrar a linha/coluna **EnemyProjectile** (deve aparecer após criar a layer)
5. Configurar os checkboxes:

| | Player | Enemy | PlayerAttack | Ground | EnemyProjectile | Default |
|-|--------|-------|--------------|--------|-----------------|---------|
| EnemyProjectile | ✓ | ✗ | ✗ | ✓ | ✗ | ✗ |

6. Marcar **apenas** Player e Ground — desmarcar todos os outros
7. Isso garante que o projétil:
   - Colide com o Player (causa dano)
   - Colide com o Ground (é destruído)
   - Não colide com outros inimigos
   - Não colide com outros projéteis

---

### 15.5 Prefab: EnemyProjectile

Este é o projétil que inimigos Ranged e bosses atiram.

#### 15.5.1 Criar o GameObject

1. Na **Hierarchy** de qualquer cena aberta (pode ser a cena Main)
2. Clique direito → **Create Empty**
3. Renomear para **"EnemyProjectile"**

#### 15.5.2 Adicionar SpriteRenderer

1. Selecionar o EnemyProjectile na Hierarchy
2. No Inspector: **Add Component → SpriteRenderer**
3. Configurar:

| Campo | Valor | Nota |
|-------|-------|------|
| Sprite | Qualquer sprite pequeno (um quadrado, ou usar sprite de partícula do SunnyLand) | Placeholder — pode ser um círculo simples |
| Color | **R: 1, G: 0, B: 0, A: 1** (vermelho puro) | Cor vermelha para diferenciar do player |
| Sorting Layer | Default | |
| Order in Layer | **2** | Aparece acima dos inimigos e do player |

> O sprite pode ser qualquer placeholder. O importante é a cor vermelha para o player identificar o projétil.

#### 15.5.3 Adicionar Rigidbody2D

1. **Add Component → Rigidbody2D**
2. Configurar:

| Campo | Valor | Por quê |
|-------|-------|---------|
| Body Type | Dynamic | Necessário para OnTriggerEnter2D funcionar |
| Gravity Scale | **0** | Projétil voa em linha reta, sem cair |
| Freeze Rotation Z | **✓** | Evita que o projétil gire |
| Collision Detection | Continuous | Evita tunelamento em paredes finas |

> **Gravity Scale = 0 é essencial.** Sem isso, o projétil cai em arco no chão em vez de ir em linha reta.

#### 15.5.4 Adicionar CircleCollider2D

1. **Add Component → CircleCollider2D**
2. Configurar:

| Campo | Valor |
|-------|-------|
| Is Trigger | **true** (marcar) |
| Radius | **0.15** |

> `Is Trigger = true` é obrigatório. Sem isso, o projétil colide fisicamente com tudo em vez de detectar via `OnTriggerEnter2D`.

#### 15.5.5 Adicionar EnemyProjectile.cs

1. **Add Component → EnemyProjectile**
2. O componente aparece com 4 campos:

| Campo | Valor | Explicação |
|-------|-------|------------|
| _speed | **6** | Velocidade do projétil em unidades/segundo |
| _damage | **8** | Dano causado ao player ao contato |
| _lifetime | **3** | Segundos até auto-destruir (evita projéteis infinitos) |
| _enemyLayer | **Enemy** | Layer para ignorar — projétil não acerta outros inimigos |

> `_enemyLayer` deve ser setado como a layer **Enemy**. Clique no dropdown e selecione "Enemy". Isso faz o bitwise check no `OnTriggerEnter2D` ignorar colisões com inimigos.

#### 15.5.6 Configurar a Layer

1. Na parte superior do Inspector (ao lado do nome do GameObject)
2. No dropdown **Layer**, selecionar: **EnemyProjectile** (a layer que criamos no passo 15.4)
3. Quando perguntar "Do you want to set the layer for all child objects?", clicar em **"This object only"**

#### 15.5.7 Converter em Prefab

1. No **Project window**, navegar até `Assets/Prefabs/`
2. **Arrastar** o GameObject "EnemyProjectile" da Hierarchy para a pasta `Assets/Prefabs/`
3. O Unity pergunta se quer criar um Prefab — confirmar
4. O prefab `EnemyProjectile.prefab` aparece na pasta
5. **Deletar** o EnemyProjectile da Hierarchy — ele só existe como prefab agora

#### 15.5.8 Hierarquia do Prefab

```
EnemyProjectile (prefab)
├── SpriteRenderer (sprite placeholder, vermelho, Order +2)
├── Rigidbody2D (Gravity 0, Freeze Rotation Z ✓)
├── CircleCollider2D (Is Trigger = true, Radius 0.15)
└── EnemyProjectile.cs (_speed=6, _damage=8, _lifetime=3, _enemyLayer=Enemy)
```

**Layer: EnemyProjectile**

---

### 15.6 Prefab: DangerZone

Esta é a zona de perigo que o Boss 2 cria no chão. Ela causa dano por tick ao player que fica em cima.

#### 15.6.1 Criar o GameObject

1. Na **Hierarchy**, clicar direito → **Create Empty**
2. Renomear para **"DangerZone"**

#### 15.6.2 Adicionar SpriteRenderer

1. **Add Component → SpriteRenderer**
2. Configurar:

| Campo | Valor | Nota |
|-------|-------|------|
| Sprite | Quadrado simples ou sprite de tile do SunnyLand | Placeholder — será o visual da zona |
| Color | **R: 1, G: 0, B: 0, A: 0.4** | Vermelho semi-transparente — indica área de perigo |
| Sorting Layer | Default | |
| Order in Layer | **-1** | Atrás do chão ou na mesma camada — não deve cobrir inimigos |

> O alpha 0.4 deixa o sprite semi-transparente. O player consegue ver o chão através da zona.

#### 15.6.3 Ajustar o Transform

1. No Inspector, na seção **Transform**:
2. Alterar **Scale** para:

| Campo | Valor |
|-------|-------|
| Scale X | **2** |
| Scale Y | **2** |

> Isso faz o sprite cobrir uma área de 2×2 unidades — bom para uma danger zone.

#### 15.6.4 Adicionar BoxCollider2D

1. **Add Component → BoxCollider2D**
2. Configurar:

| Campo | Valor |
|-------|-------|
| Is Trigger | **true** (marcar) |
| Size X | **2** |
| Size Y | **2** |

> O collider deve ter o mesmo tamanho do sprite. `Is Trigger = true` é obrigatório para detectar o player via `OnTriggerEnter2D`/`OnTriggerStay2D`.

#### 15.6.5 Adicionar DangerZone.cs

1. **Add Component → DangerZone**
2. O componente aparece com 3 campos:

| Campo | Valor | Explicação |
|-------|-------|------------|
| _damagePerTick | **5** | Dano causado a cada tick quando o player está na zona |
| _tickInterval | **0.5** | Segundos entre cada tick de dano |
| _duration | **4** | Segundos até a zona se auto-destruir (será sobrescrito pelo Initialize) |

> O campo `_duration` no Inspector é o default. O `BossAreaController` chama `Initialize(duration)` que sobrescreve esse valor.

#### 15.6.6 Converter em Prefab

1. Arrastar da Hierarchy para `Assets/Prefabs/`
2. Deletar da cena

#### 15.6.7 Hierarquia do Prefab

```
DangerZone (prefab)
├── SpriteRenderer (quadrado, vermelho transparente A:0.4, Scale 2x2)
├── BoxCollider2D (Is Trigger = true, Size 2x2)
└── DangerZone.cs (_damagePerTick=5, _tickInterval=0.5, _duration=4)
```

**Layer: Default**

---

### 15.7 Prefab: EnemyFast

O inimigo rápido persegue o player sem pausas. É fraco mas veloz.

#### 15.7.1 Duplicar o Enemy.prefab

1. No **Project window**, navegar até `Assets/Prefabs/`
2. Clicar no **Enemy.prefab**
3. **Ctrl+D** para duplicar
4. Renomear a cópia para **"EnemyFast"**

#### 15.7.2 Configurar EnemyController

1. **Duplo clique** no EnemyFast.prefab para abrir no Prefab Editor
2. Selecionar o objeto raiz "EnemyFast"
3. No componente **EnemyController**, preencher:

**Seção Movement Settings:**

| Campo | Valor | Explicação |
|-------|-------|------------|
| _moveSpeed | **5** | 2.5x mais rápido que Melee (que é 2) |

**Seção Enemy Type:**

| Campo | Valor |
|-------|-------|
| _enemyType | **Fast** (selecionar no dropdown) |

**Seção Combat Settings (herdados de EnemyBase):**

| Campo | Valor | Explicação |
|-------|-------|------------|
| _maxHealth | **15** | Metade do Melee (30) — frágil |
| _damageToPlayer | **8** | Um pouco menos que Melee (10) |
| _attackCooldown | **0.6** | Ataca mais rápido que Melee (1) |
| _knockbackForce | **3** | Menor knockback que Melee (5) |

**Seção Jump Settings:**

| Campo | Valor | Nota |
|-------|-------|------|
| _jumpForce | **8** | Manter padrão — Fast não usa muito |
| _groundCheck | arrastar GroundCheck | Já vem do prefab original |
| _groundCheckRadius | **0.15** | Manter padrão |
| _groundLayer | **Ground** | Já vem do prefab original |

**Seção References:**

| Campo | Valor | Nota |
|-------|-------|------|
| _player | deixar vazio | RoomController seta via Initialize |
| _playerLayer | **Player** | Clique no dropdown, marcar "Player" |

> `_playerLayer` é o campo novo substituindo o `NameToLayer`. Clique no dropdown e marque a checkbox da layer **Player**.

#### 15.7.3 Aparência Visual

1. Na Hierarchy do Prefab Editor, selecionar o objeto raiz
2. No componente **SpriteRenderer**:

| Campo | Valor |
|-------|-------|
| Color | **R: 0, G: 1, B: 1, A: 1** (ciano/azul claro) |

3. No **Transform**:

| Campo | Valor |
|-------|-------|
| Scale X | **0.8** |
| Scale Y | **0.8** |
| Scale Z | **1** |

> Menor que o Melee (que usa Scale 1). Cor ciano para diferenciar.

#### 15.7.4 Salvar e Sair

1. **Ctrl+S** para salvar o prefab
2. Clicar a seta **"Back"** no canto superior esquerdo da Hierarchy para sair do Prefab Editor

---

### 15.8 Prefab: EnemyHeavy

O inimigo pesado é lento, forte e faz charge.

#### 15.8.1 Duplicar o Enemy.prefab

1. Em `Assets/Prefabs/`: selecionar **Enemy.prefab**
2. **Ctrl+D** → renomear para **"EnemyHeavy"**

#### 15.8.2 Configurar EnemyController

1. Abrir o prefab no Prefab Editor
2. No componente **EnemyController**:

**Seção Movement Settings:**

| Campo | Valor | Explicação |
|-------|-------|------------|
| _moveSpeed | **1** | 2x mais lento que Melee |

**Seção Enemy Type:**

| Campo | Valor |
|-------|-------|
| _enemyType | **Heavy** |

**Seção Heavy Settings** (nova seção visível quando o script recompilou):

| Campo | Valor | Explicação |
|-------|-------|------------|
| _chargeSpeed | **8** | Velocidade da investida — bem rápida |
| _chargeCooldown | **3** | Segundos entre charges |

**Seção Combat Settings (EnemyBase):**

| Campo | Valor | Explicação |
|-------|-------|------------|
| _maxHealth | **90** | 3x o Melee — tanque |
| _damageToPlayer | **20** | 2x o Melee — dói muito |
| _attackCooldown | **1.5** | Mais lento para atacar |
| _knockbackForce | **10** | 2x o Melee — empurra forte |

**Seção References:**

| Campo | Valor |
|-------|-------|
| _player | deixar vazio |
| _playerLayer | **Player** |

#### 15.8.3 Aparência Visual

| Campo | Valor |
|-------|-------|
| SpriteRenderer Color | **R: 1, G: 0.5, B: 0, A: 1** (laranja) |
| Scale X | **1.5** |
| Scale Y | **1.5** |
| Scale Z | **1** |

> Maior que o Melee. Cor laranja para indicar perigo.

#### 15.8.4 Salvar e Sair

Ctrl+S → Back.

---

### 15.9 Prefab: EnemyRanged

O inimigo ranged mantém distância e atira projéteis.

#### 15.9.1 Duplicar o Enemy.prefab

1. Em `Assets/Prefabs/`: selecionar **Enemy.prefab**
2. **Ctrl+D** → renomear para **"EnemyRanged"**

#### 15.9.2 Configurar EnemyController

1. Abrir o prefab no Prefab Editor
2. No componente **EnemyController**:

**Seção Movement Settings:**

| Campo | Valor | Explicação |
|-------|-------|------------|
| _moveSpeed | **1.5** | Lento — não precisa correr |

**Seção Enemy Type:**

| Campo | Valor |
|-------|-------|
| _enemyType | **Ranged** |

**Seção Ranged Settings** (nova seção):

| Campo | Valor | Explicação |
|-------|-------|------------|
| _preferredDistance | **5** | Distância ideal do player em unidades |
| _shootCooldown | **1.5** | Segundos entre tiros |
| _projectilePrefab | **EnemyProjectile.prefab** | Arrastar de Assets/Prefabs/ |

> **`_projectilePrefab` é o campo mais importante.** Sem essa conexão, o ranged não atira. Arraste o prefab EnemyProjectile que criamos no passo 15.5.

**Seção Combat Settings (EnemyBase):**

| Campo | Valor | Explicação |
|-------|-------|------------|
| _maxHealth | **20** | Frágil — prioridade para o player matar |
| _damageToPlayer | **5** | Dano baixo de contato |
| _attackCooldown | **2** | Contato é pouco frequente |
| _knockbackForce | **2** | Knockback mínimo |

**Seção References:**

| Campo | Valor |
|-------|-------|
| _player | deixar vazio |
| _playerLayer | **Player** |

#### 15.9.3 Aparência Visual

| Campo | Valor |
|-------|-------|
| SpriteRenderer Color | **R: 0.5, G: 0, B: 0.8, A: 1** (roxo) |
| Scale X | **0.9** |
| Scale Y | **0.9** |
| Scale Z | **1** |

> Um pouco menor que o Melee. Cor roxo para diferenciar.

#### 15.9.4 Salvar e Sair

Ctrl+S → Back.

#### 15.9.5 Hierarquia do Prefab

```
EnemyRanged (prefab)
├── SpriteRenderer (sprite, roxo, Scale 0.9)
├── Rigidbody2D (Gravity 3, Freeze Rotation Z)
├── CircleCollider2D (trigger, Is Trigger = true)
├── CircleCollider2D (physics, Is Trigger = false)
├── EnemyController.cs
│   ├── _enemyType = Ranged
│   ├── _moveSpeed = 1.5
│   ├── _preferredDistance = 5
│   ├── _shootCooldown = 1.5
│   ├── _projectilePrefab = EnemyProjectile.prefab  ← IMPORTANTE
│   ├── _maxHealth = 20
│   └── _playerLayer = Player
└── GroundCheck (Empty child, nos pés)
```

---

### 15.10 Atualizar Enemy.prefab Existente

O prefab `Enemy.prefab` já existe e precisa ser atualizado com os novos campos que foram adicionados ao script `EnemyController` (que agora herda de `EnemyBase`).

#### 15.10.1 Abrir o Prefab

1. No **Project window**, navegar até `Assets/Prefabs/`
2. **Duplo clique** no `Enemy.prefab` — abre o Prefab Editor
3. Selecionar o objeto raiz "Enemy"

#### 15.10.2 Verificar os Novos Campos

O Unity já preenche os campos novos com os valores padrão do script. Verificar:

**Seção Enemy Type** (nova):

| Campo | Valor esperado |
|-------|---------------|
| _enemyType | Melee (default) |

**Seção Ranged Settings** (nova):

| Campo | Valor esperado |
|-------|---------------|
| _preferredDistance | 5 |
| _shootCooldown | 1.5 |
| _projectilePrefab | vazio (null) |

**Seção Heavy Settings** (nova):

| Campo | Valor esperado |
|-------|---------------|
| _chargeSpeed | 8 |
| _chargeCooldown | 3 |

**Seção References:**

| Campo | O que fazer |
|-------|------------|
| _player | Deixar vazio (RoomController seta) |
| _playerLayer | **Marcar como "Player"** — clicar no dropdown e marcar a checkbox |

> **`_playerLayer` é essencial.** Sem essa configuração, o layer check em `OnTriggerEnter2D` não detecta o player e o inimigo não causa dano por contato.

#### 15.10.3 Verificar EnemyBase Fields

Os campos de EnemyBase (que eram do EnemyController antigo) devem estar com os mesmos valores de antes:

| Campo | Valor |
|-------|-------|
| _maxHealth | 30 |
| _damageToPlayer | 10 |
| _attackCooldown | 1 |
| _knockbackForce | 5 |
| _isElite | false |
| _runeValue | 1 |

#### 15.10.4 Salvar e Sair

Ctrl+S → Back.

---

### 15.11 Atualizar EliteEnemy.prefab

Se existe um prefab `EliteEnemy.prefab`, ele precisa ser atualizado.

#### 15.11.1 Abrir o Prefab

1. Em `Assets/Prefabs/`: **duplo clique** no EliteEnemy.prefab

#### 15.11.2 Configurar

| Campo | O que fazer |
|-------|------------|
| _enemyType | **Melee** |
| _isElite | **true** (marcar) |
| _playerLayer | **Player** |

#### 15.11.3 Salvar e Sair

Ctrl+S → Back.

---

### 15.12 Prefab: Boss1 (Boss Melee)

Este é o boss do andar 5. Ele ataca em combos de 3 hits e fica enraged quando o HP cai abaixo de 30%.

#### 15.12.1 Criar o GameObject

1. Na **Hierarchy**, clicar direito → **Create Empty**
2. Renomear para **"Boss1"**

#### 15.12.2 Adicionar SpriteRenderer

1. **Add Component → SpriteRenderer**
2. Configurar:

| Campo | Valor |
|-------|-------|
| Sprite | Placeholder (sprite de inimigo grande, ou o mesmo do Enemy) |
| Color | **R: 0.8, G: 0.2, B: 0.2, A: 1** (vermelho escuro) |
| Sorting Layer | Default |
| Order in Layer | 0 |

#### 15.12.3 Ajustar o Transform

| Campo | Valor |
|-------|-------|
| Scale X | **2.5** |
| Scale Y | **2.5** |
| Scale Z | **1** |

> Boss é 2.5x maior que um inimigo normal.

#### 15.12.4 Adicionar Rigidbody2D

1. **Add Component → Rigidbody2D**
2. Configurar:

| Campo | Valor |
|-------|-------|
| Body Type | Dynamic |
| Gravity Scale | **3** |
| Freeze Rotation Z | **✓** |

#### 15.12.5 Adicionar Colliders

O boss precisa de **dois** CircleCollider2D:

**Collider 1 — Trigger (para detectar contato com player):**

1. **Add Component → CircleCollider2D**
2. Configurar:

| Campo | Valor |
|-------|-------|
| Is Trigger | **true** |
| Radius | **1.2** |

> Ajustar o radius para cobrir o sprite do boss.

**Collider 2 — Physics (para colidir com o chão):**

1. **Add Component → CircleCollider2D** (sim, adicionar um segundo)
2. Configurar:

| Campo | Valor |
|-------|-------|
| Is Trigger | **false** |
| Radius | **1.2** |

> Sem o collider físico, o boss cai através do chão. Sem o collider trigger, o boss não detecta contato com o player.

#### 15.12.6 Adicionar BossMeleeController.cs

1. **Add Component → BossMeleeController**
2. Preencher os campos:

**Seção Health (EnemyBase):**

| Campo | Valor |
|-------|-------|
| _maxHealth | **150** |
| _runeValue | **5** |

**Seção Combat (EnemyBase):**

| Campo | Valor |
|-------|-------|
| _damageToPlayer | **15** |
| _attackCooldown | **1** |
| _knockbackForce | **8** |

**Seção Boss Movement:**

| Campo | Valor |
|-------|-------|
| _moveSpeed | **3** |

**Seção Attack Pattern:**

| Campo | Valor | Explicação |
|-------|-------|------------|
| _attackComboCount | **3** | 3 hits por combo |
| _attackInterval | **0.4** | 0.4s entre cada hit do combo |
| _comboCooldown | **1.5** | 1.5s de espera entre combos |
| _attackRange | **1.5** | Alcance do ataque em unidades |

**Seção Jump:**

| Campo | Valor | Nota |
|-------|-------|------|
| _jumpForce | **8** | |
| _groundCheck | arrastar o filho GroundCheck | Criar no próximo passo |
| _groundCheckRadius | **0.15** | |
| _groundLayer | **Ground** | Marcar no dropdown |

**Seção References:**

| Campo | Valor |
|-------|-------|
| _playerLayer | **Player** |

#### 15.12.7 Criar GroundCheck

1. Na Hierarchy do Prefab Editor (ou na cena), clicar direito no **Boss1** → **Create Empty**
2. Renomear para **"GroundCheck"**
3. No **Transform**, ajustar a posição local:

| Campo | Valor |
|-------|-------|
| Pos X | **0** |
| Pos Y | **-0.8** |
| Pos Z | **0** |

> Pos Y = -0.8 posiciona o ground check nos pés do boss (que tem scale 2.5).

4. Voltar ao objeto raiz **Boss1**
5. No componente **BossMeleeController**, arrastar o **GroundCheck** para o campo `_groundCheck`

#### 15.12.8 Configurar a Layer

1. Selecionar o objeto raiz **Boss1**
2. No dropdown **Layer** (topo do Inspector): selecionar **Enemy**
3. Quando perguntar, clicar **"This object only"**

#### 15.12.9 Converter em Prefab

1. Arrastar da Hierarchy para `Assets/Prefabs/`
2. Deletar da cena

#### 15.12.10 Hierarquia do Prefab

```
Boss1 (prefab, Layer = Enemy)
├── SpriteRenderer (vermelho escuro, Scale 2.5)
├── Rigidbody2D (Gravity 3, Freeze Rotation Z)
├── CircleCollider2D (trigger, Is Trigger = true, Radius 1.2)
├── CircleCollider2D (physics, Is Trigger = false, Radius 1.2)
├── BossMeleeController.cs
│   ├── _maxHealth = 150
│   ├── _damageToPlayer = 15
│   ├── _moveSpeed = 3
│   ├── _attackComboCount = 3
│   ├── _attackRange = 1.5
│   ├── _groundCheck = GroundCheck
│   ├── _groundLayer = Ground
│   └── _playerLayer = Player
└── GroundCheck (Empty, pos Y = -0.8)
```

---

### 15.13 Prefab: Boss2 (Boss Area) 

Este é o boss do andar 10. Ele cria danger zones no chão e projéteis caem do céu.

#### 15.13.1 Criar o GameObject

1. **Create Empty** → renomear **"Boss2"**

#### 15.13.2 Adicionar Componentes Base

Mesmo padrão do Boss1:

| Componente | Configuração |
|-----------|-------------|
| SpriteRenderer | Placeholder **azul** (R:0.2, G:0.3, B:0.8), Scale **2.5** |
| Rigidbody2D | Gravity 3, Freeze Rotation Z ✓ |
| CircleCollider2D (trigger) | Is Trigger = true, Radius 1.2 |
| CircleCollider2D (physics) | Is Trigger = false, Radius 1.2 |
| Layer | **Enemy** |

#### 15.13.3 Adicionar BossAreaController.cs

1. **Add Component → BossAreaController**
2. Preencher:

**Seção Health (EnemyBase):**

| Campo | Valor |
|-------|-------|
| _maxHealth | **200** |
| _runeValue | **8** |

**Seção Combat (EnemyBase):**

| Campo | Valor |
|-------|-------|
| _damageToPlayer | **10** |
| _attackCooldown | **1** |
| _knockbackForce | **5** |

**Seção Boss Movement:**

| Campo | Valor |
|-------|-------|
| _moveSpeed | **1.5** |

**Seção Danger Zones:**

| Campo | Valor | Explicação |
|-------|-------|------------|
| _dangerZonePrefab | **DangerZone.prefab** | Arrastar de Assets/Prefabs/ |
| _attackInterval | **2** | Segundos entre cada danger zone |
| _dangerZoneDuration | **4** | Segundos que cada zona dura |
| _dangerZoneRadius | **3** | Raio do offset aleatório na fase 2 |
| _maxDangerZones | **5** | Máximo de zonas simultâneas |

> **`_dangerZonePrefab` é essencial.** Sem essa conexão, o boss não spawna zonas.

**Seção Projectiles:**

| Campo | Valor | Explicação |
|-------|-------|------------|
| _projectilePrefab | **EnemyProjectile.prefab** | Arrastar de Assets/Prefabs/ |
| _fallingProjectileInterval | **3** | Segundos entre projéteis cadentes |

> Os projéteis caem de cima do player (posição Y + 8). O `_projectilePrefab` é o mesmo usado pelo EnemyRanged.

**Seção References:**

| Campo | Valor |
|-------|-------|
| _playerLayer | **Player** |

> Boss2 NÃO precisa de GroundCheck — não pula nem verifica chão.

#### 15.13.4 Converter em Prefab

Arrastar para `Assets/Prefabs/`, deletar da cena.

#### 15.13.5 Hierarquia do Prefab

```
Boss2 (prefab, Layer = Enemy)
├── SpriteRenderer (azul, Scale 2.5)
├── Rigidbody2D (Gravity 3, Freeze Rotation Z)
├── CircleCollider2D (trigger, Is Trigger = true)
├── CircleCollider2D (physics, Is Trigger = false)
└── BossAreaController.cs
    ├── _maxHealth = 200
    ├── _dangerZonePrefab = DangerZone.prefab
    ├── _maxDangerZones = 5
    ├── _projectilePrefab = EnemyProjectile.prefab
    └── _playerLayer = Player
```

---

### 15.14 Prefab: Boss3 (Boss Híbrido)

Este é o boss do andar 15. Fase 1: melee. Fase 2 (50% HP): ranged com spread de 3 projéteis.

#### 15.14.1 Criar o GameObject

1. **Create Empty** → renomear **"Boss3"**

#### 15.14.2 Adicionar Componentes Base

| Componente | Configuração |
|-----------|-------------|
| SpriteRenderer | Placeholder **verde** (R:0.2, G:0.7, B:0.3), Scale **2.5** |
| Rigidbody2D | Gravity 3, Freeze Rotation Z ✓ |
| CircleCollider2D (trigger) | Is Trigger = true, Radius 1.2 |
| CircleCollider2D (physics) | Is Trigger = false, Radius 1.2 |
| Layer | **Enemy** |

#### 15.14.3 Adicionar BossHybridController.cs

1. **Add Component → BossHybridController**
2. Preencher:

**Seção Health (EnemyBase):**

| Campo | Valor |
|-------|-------|
| _maxHealth | **180** |
| _runeValue | **7** |

**Seção Combat (EnemyBase):**

| Campo | Valor |
|-------|-------|
| _damageToPlayer | **12** |
| _attackCooldown | **1** |
| _knockbackForce | **6** |

**Seção Boss Movement:**

| Campo | Valor |
|-------|-------|
| _moveSpeed | **2.5** |

**Seção Phase Settings:**

| Campo | Valor | Explicação |
|-------|-------|------------|
| _phaseTwoHPPercent | **0.5** | Muda para ranged quando HP < 50% |

**Seção Melee:**

| Campo | Valor |
|-------|-------|
| _meleeAttackCooldown | **0.8** |
| _meleeRange | **1.5** |

**Seção Ranged:**

| Campo | Valor | Explicação |
|-------|-------|------------|
| _projectilePrefab | **EnemyProjectile.prefab** | Arrastar de Assets/Prefabs/ |
| _shootCooldown | **1.2** |
| _preferredDistance | **6** |
| _spreadAngle | **15** | Ângulo entre os 3 projéteis em graus |

> Na fase 2, o boss atira 3 projéteis: um reto e dois nos ângulos ±15°.

**Seção Jump:**

| Campo | Valor |
|-------|-------|
| _jumpForce | **8** |
| _groundCheck | arrastar GroundCheck |
| _groundCheckRadius | **0.15** |
| _groundLayer | **Ground** |

**Seção References:**

| Campo | Valor |
|-------|-------|
| _playerLayer | **Player** |

#### 15.14.4 Criar GroundCheck

Mesmo que Boss1: criar Empty "GroundCheck" como filho, pos Y = -0.8, arrastar no `_groundCheck`.

#### 15.14.5 Converter em Prefab

Arrastar para `Assets/Prefabs/`, deletar da cena.

#### 15.14.6 Hierarquia do Prefab

```
Boss3 (prefab, Layer = Enemy)
├── SpriteRenderer (verde, Scale 2.5)
├── Rigidbody2D (Gravity 3, Freeze Rotation Z)
├── CircleCollider2D (trigger, Is Trigger = true)
├── CircleCollider2D (physics, Is Trigger = false)
├── BossHybridController.cs
│   ├── _maxHealth = 180
│   ├── _phaseTwoHPPercent = 0.5
│   ├── _meleeRange = 1.5
│   ├── _projectilePrefab = EnemyProjectile.prefab
│   ├── _preferredDistance = 6
│   ├── _spreadAngle = 15
│   ├── _groundCheck = GroundCheck
│   ├── _groundLayer = Ground
│   └── _playerLayer = Player
└── GroundCheck (Empty, pos Y = -0.8)
```

### 15.15 Atualizar BossArena Prefabs

Os prefabs de arena de boss (`BossArena_5`, `BossArena_10`, `BossArena_15`) criados na Fase 7 precisam ser atualizados para usar os novos bosses.

#### 15.15.1 Atualizar BossArena_5 -- parei aqui

1. No **Project window**, navegar até `Assets/Prefabs/`
2. **Duplo clique** no prefab **BossArena_5** para abrir no Prefab Editor
3. Selecionar o objeto raiz "BossArena_5"
4. Procurar o componente **BossFloorHandler** no Inspector
5. Expandir a seção **Boss Settings**

**Configurar _bossPrefabs:**

1. No campo `_bossPrefabs`, clicar na seta para expandir
2. Alterar **Size** para **1**
3. No **Element 0**, arrastar o prefab **Boss1.prefab** de `Assets/Prefabs/`

**Verificar _bossSpawnPoint:**

1. Deve estar conectado ao filho **BossSpawnPoint** da arena
2. Se estiver vazio, arrastar o filho BossSpawnPoint da Hierarchy do Prefab Editor

**Verificar _roomController:**

1. Deve estar conectado ao componente RoomController do próprio BossArena_5
2. Se estiver vazio: clicar no círculo do campo → selecionar "BossArena_5" → selecionar o componente "RoomController"

> O campo `_bossPrefab` (singular) pode ser deixado vazio. O sistema agora usa `_bossPrefabs[]` para selecionar o boss correto por andar.

6. **Ctrl+S** → salvar
7. **Back** → sair do Prefab Editor

#### 15.15.2 Atualizar BossArena_10

1. **Duplo clique** no prefab **BossArena_10**
2. Selecionar raiz → componente **BossFloorHandler**
3. `_bossPrefabs`: Size = 1, Element 0 = **Boss2.prefab**
4. `_bossSpawnPoint`: verificar conexão
5. Salvar e sair

#### 15.15.3 Atualizar BossArena_15

1. **Duplo clique** no prefab **BossArena_15**
2. Selecionar raiz → componente **BossFloorHandler**
3. `_bossPrefabs`: Size = 1, Element 0 = **Boss3.prefab**
4. `_bossSpawnPoint`: verificar conexão
5. Salvar e sair

**Resumo:**

| Arena | _bossPrefabs[0] | Andar |
|-------|-----------------|-------|
| BossArena_5 | Boss1.prefab | 5 |
| BossArena_10 | Boss2.prefab | 10 |
| BossArena_15 | Boss3.prefab | 15 |

---

### 15.16 Atualizar Cena Main — DungeonGenerator

O `DungeonGenerator` na cena Main precisa dos novos prefabs de inimigo para funcionar a distribuição por andar.

#### 15.16.1 Abrir a Cena

1. No **Project window**, navegar até `Assets/Scenes/`
2. **Duplo clique** em **Main.unity** para abrir

#### 15.16.2 Selecionar o DungeonGenerator

1. Na **Hierarchy**, procurar o objeto **DungeonGenerator**
2. Clicar nele para selecionar

#### 15.16.3 Configurar os Prefabs de Inimigo

No Inspector, na seção **Enemy Prefabs** (se não existir, o Unity criou automaticamente com o novo header do script):

| Campo | O que fazer | Onde encontrar |
|-------|-------------|----------------|
| _enemyPrefab | Já deve estar conectado ao **Enemy.prefab** | Verificar — não mexer |
| _enemyFastPrefab | Arrastar **EnemyFast.prefab** | `Assets/Prefabs/EnemyFast.prefab` |
| _enemyHeavyPrefab | Arrastar **EnemyHeavy.prefab** | `Assets/Prefabs/EnemyHeavy.prefab` |
| _enemyRangedPrefab | Arrastar **EnemyRanged.prefab** | `Assets/Prefabs/EnemyRanged.prefab` |

> `_enemyPrefab` (sem sufixo) é o Melee — provavelmente já está preenchido. Os 3 campos novos (`_enemyFastPrefab`, `_enemyHeavyPrefab`, `_enemyRangedPrefab`) precisam ser preenchidos agora.

#### 15.16.4 Verificar

1. Expandir a seção **Enemy Prefabs** no Inspector
2. Todos os 4 campos devem ter um prefab conectado (não vazio)
3. Cada campo mostra o nome do prefab ao lado

#### 15.16.5 Salvar

**Ctrl+S** para salvar a cena.

---

### 15.17 Checklist de Prefabs

Depois de criar tudo, verificar em `Assets/Prefabs/`:

- [ ] `EnemyProjectile.prefab` — Rigidbody2D, CircleCollider2D trigger, EnemyProjectile.cs, Layer EnemyProjectile
- [ ] `DangerZone.prefab` — BoxCollider2D trigger, DangerZone.cs, sprite vermelho transparente
- [ ] `EnemyFast.prefab` — EnemyController com _enemyType=Fast, speed 5, HP 15, color ciano
- [ ] `EnemyHeavy.prefab` — EnemyController com _enemyType=Heavy, charge configurado, color laranja
- [ ] `EnemyRanged.prefab` — EnemyController com _enemyType=Ranged, _projectilePrefab conectado, color roxo
- [ ] `Enemy.prefab` atualizado — _enemyType=Melee, _playerLayer configurado como Player
- [ ] `EliteEnemy.prefab` atualizado — _isElite=true, _playerLayer configurado
- [ ] `Boss1.prefab` — BossMeleeController, 2 colliders, GroundCheck, Layer Enemy
- [ ] `Boss2.prefab` — BossAreaController, _dangerZonePrefab e _projectilePrefab conectados, Layer Enemy
- [ ] `Boss3.prefab` — BossHybridController, _projectilePrefab conectado, GroundCheck, Layer Enemy
- [ ] `BossArena_5` atualizado — _bossPrefabs[0] = Boss1
- [ ] `BossArena_10` atualizado — _bossPrefabs[0] = Boss2
- [ ] `BossArena_15` atualizado — _bossPrefabs[0] = Boss3
- [ ] Cena Main — DungeonGenerator com _enemyFastPrefab, _enemyHeavyPrefab, _enemyRangedPrefab preenchidos

---

### 15.18 Resumo dos Valores por Tipo

| Campo | Melee | Fast | Heavy | Ranged |
|-------|-------|------|-------|--------|
| _maxHealth | 30 | 15 | 90 | 20 |
| _damageToPlayer | 10 | 8 | 20 | 5 |
| _moveSpeed | 2 | 5 | 1 | 1.5 |
| _attackCooldown | 1 | 0.6 | 1.5 | 2 |
| _knockbackForce | 5 | 3 | 10 | 2 |
| _projectilePrefab | — | — | — | EnemyProjectile |
| _chargeSpeed | — | — | 8 | — |
| Visual (cor) | padrão | ciano | laranja | roxo |

| Boss | HP | Dano | Andar | Padrão de Ataque |
|------|-----|------|-------|-----------------|
| Boss 1 (Melee) | 150 | 15 | 5 | Combos 3 hits, enrage 30% HP (fica vermelho, +50% speed, -50% cooldown) |
| Boss 2 (Area) | 200 | 10 | 10 | Danger zones (max 5), projéteis cadentes, fase 2 (50% HP): zonas extras |
| Boss 3 (Híbrido) | 180 | 12 | 15 | Fase 1: melee persegue. Fase 2 (50% HP): ranged com spread 3 projéteis ±15° |

---

### 15.19 Como Testar em Play Mode

#### Teste 1 — Inimigo Melee (Comportamento Inalterado)

1. Abrir cena **Main**
2. Clicar **Play**
3. Avançar para uma sala de combate no andar 1 ou 2
4. Verificações:
   - Inimigos spawneam com o comportamento original
   - Perseguem o player
   - Pulam obstáculos quando player está acima
   - Causam dano por contato
   - Ao morrer, ficam vermelhos por 0.3s, dropam runa, são destruídos
   - Player leva knockback ao ser atingido

#### Teste 2 — Inimigo Fast

1. Avançar até andar 3+
2. Verificações:
   - Inimigos Fast aparecem (sprite ciano, menor)
   - São perceptivelmente mais rápidos que Melee
   - Nunca pausam durante a perseguição
   - São mais frágeis — morrem mais rápido
   - Knockback menor que Melee

#### Teste 3 — Inimigo Heavy

1. Avançar até andar 5+
2. Verificações:
   - Inimigos Heavy aparecem (sprite laranja, maior)
   - São lentos no movimento
   - Quando perto (< 3 unidades), fazem uma charge rápida
   - Dano por contato é alto (20)
   - Knockback forte (empurra o player longe)
   - Têm mais HP (90) — demoram mais para morrer

#### Teste 4 — Inimigo Ranged

1. Avançar até andar 7+
2. Verificações:
   - Inimigos Ranged aparecem (sprite roxo, menor)
   - Mantêm distância do player (~5 unidades)
   - Param e atiram projéteis vermelhos
   - Se o player chega perto, recuam
   - Se erram por 5s+, se reposicionam para atirar de novo

#### Teste 5 — Projétil

1. Colocar um inimigo Ranged na mesma sala que o player
2. Verificações:
   - Projétil se move em linha reta em direção ao player
   - Projétil ignora outros inimigos (passa por eles)
   - Projétil causa dano ao player ao contato
   - Projétil é destruído ao encostar no chão/paredes
   - Projétil é auto-destruído após 3 segundos
   - Múltiplos projéteis podem existir simultaneamente

#### Teste 6 — Projétil Não Acerta Inimigos

1. Colocar 2 inimigos Ranged + 1 Melee na mesma sala
2. Ficar parado para tomar projéteis
3. Verificações:
   - Projéteis passam pelos outros inimigos sem causar dano
   - Projéteis só causam dano no player

#### Teste 7 — Elite (Qualquer Tipo)

1. Encontrar uma sala de Elite
2. Verificações:
   - Inimigo Elite é maior (Scale 1.3x)
   - Sprite amarelo
   - Ranged Elite: atira mais rápido (_shootCooldown × 0.6)
   - Heavy Elite: knockback maior (_knockbackForce × 1.5)
   - Fast Elite: mais rápido (_moveSpeed × 1.3)
   - Dropa 3× runas ao morrer

#### Teste 8 — Boss 1 (Andar 5)

1. Avançar até andar 5 (boss floor)
2. A arena de boss carrega
3. Verificações:
   - Boss spawna no centro da arena
   - Boss é grande (Scale 2.5), vermelho
   - Boss persegue o player
   - Quando perto, ataca em combos de 3 hits com intervalo de 0.4s
   - Após combo, espera 1.5s antes do próximo
   - Quando HP cai abaixo de 30%:
     - Boss fica vermelho vivo
     - Velocidade aumenta 50%
     - Cooldown do combo reduz pela metade
   - Ao matar o boss:
     - Player recupera vida total
     - Andar avança para 6
     - Boss dropa runas (5× valor base × multiplicadores)

#### Teste 9 — Boss 2 (Andar 10)

1. Avançar até andar 10
2. Verificações:
   - Boss spawna na arena, sprite azul
   - Boss cria danger zones vermelhas no chão onde o player está
   - Danger zones causam dano por tick (5 dano a cada 0.5s)
   - Danger zones desaparecem após 4 segundos
   - Máximo 5 danger zones simultâneas (não cria mais que 5)
   - Projéteis caem do céu periodicamente (a cada 3s)
   - Quando HP cai abaixo de 50% (fase 2):
     - Boss fica magenta
     - Cria danger zone extra com offset aleatório

#### Teste 10 — Boss 3 (Andar 15)

1. Avançar até andar 15
2. Verificações:
   - Fase 1 (HP > 50%):
     - Boss persegue e ataca melee
     - Usa OverlapCircle para acertar o player
     - Pula quando player está acima
   - Fase 2 (HP < 50%):
     - Boss fica vermelho
     - Velocidade reduz 30%
     - Muda para ranged: atira 3 projéteis em spread (±15°)
     - Mantém distância preferencial de 6 unidades
     - Se player chega perto, recua

#### Teste 11 — Variedade por Floor

1. Jogar do andar 1 ao 7+ prestando atenção nos tipos de inimigo
2. Verificações:
   - Andar 1–2: apenas Melee spawnam
   - Andar 3–4: Melee + Fast nas salas
   - Andar 5–6: Melee + Fast + Heavy
   - Andar 7+: todos os 4 tipos
   - Melee sempre presente na pool (nunca sala só com ranged)

#### Teste 12 — Múltiplos Ranged na Mesma Sala

1. Encontrar ou forçar uma sala com 3+ inimigos Ranged
2. Verificações:
   - Cada ranged atira independentemente
   - Múltiplos projéteis na tela ao mesmo tempo
   - Sem lag significativo
   - Projéteis não colidem entre si (EnemyProjectile layer ignora EnemyProjectile)

#### Teste 13 — Stress Test

1. Usar o DungeonGenerator para gerar uma sala com 10+ inimigos (ajustar _spawnPoints ou forçar)
2. Verificações:
   - Sem travamento
   - Inimigos não ficam presos uns nos outros (Collision Matrix: Enemy↔Enemy = NÃO)
   - Todos os tipos de inimigo funcionam simultaneamente
   - Projéteis de ranged funcionam normalmente

---

### 15.20 Troubleshooting

| Problema | Causa provável | Solução |
|----------|---------------|---------|
| Novo tipo de inimigo não aparece | Prefabs não conectados no DungeonGenerator | Verificar _enemyFastPrefab, _enemyHeavyPrefab, _enemyRangedPrefab no Inspector do DungeonGenerator |
| Projétil não se move | Rigidbody2D não existe ou Gravity não é 0 | Verificar prefab EnemyProjectile: Rigidbody2D com Gravity Scale = 0 |
| Projétil cai no chão | Gravity Scale != 0 | Alterar para 0 no Rigidbody2D do prefab |
| Projétil acerta inimigos | _enemyLayer não configurado | No EnemyProjectile.cs: _enemyLayer deve ser "Enemy" (clicar no dropdown, marcar Enemy) |
| Projétil não causa dano | Player não tem IDamageable ou layer errada | Verificar: Player tem PlayerHealth (que implementa IDamageable), Player está na layer Player |
| Projétil não é destruído no chão | Layer check errado ou Ground não existe | Verificar: Collision Matrix permite EnemyProjectile↔Ground, chão está na layer Ground |
| Ranged não atira | _projectilePrefab vazio | No EnemyController do EnemyRanged: arrastar EnemyProjectile.prefab para _projectilePrefab |
| Ranged fica parado infinitamente | _noHitTimer não reseta | Comportamento esperado: após 5s sem acertar, ranged se reposiciona. Verificar EnemyProjectile chama _owner.OnProjectileHit() |
| Heavy não faz charge | _chargeSpeed ou _chargeCooldown errados | Verificar valores: _chargeSpeed=8, _chargeCooldown=3, perto do player (< 3 unidades) |
| Heavy nunca chega perto | _moveSpeed muito baixo | Heavy é lento (speed=1) — é comportamento esperado |
| Fast parece igual a Melee | _enemyType não configurado | Abrir EnemyFast.prefab, verificar _enemyType = Fast |
| Boss não spawna | _bossPrefabs[] vazio na arena | Verificar BossFloorHandler: _bossPrefabs[0] = Boss1/2/3.prefab |
| Boss não faz nada | IBoss.Initialize() não é chamado | Verificar: prefab do boss tem o componente Boss correto (BossMelee/BossArea/BossHybridController) |
| Boss não tem corpo físico | CircleCollider2D (physics) não existe | Adicionar segundo CircleCollider2D com Is Trigger = false |
| Boss cai pelo chão | Sem collider físico ou layer errada | Verificar: CircleCollider2D Is Trigger = false, Layer = Enemy, Collision Matrix Enemy↔Ground = SIM |
| Boss não dá dano | _playerLayer não configurado | Verificar: _playerLayer = Player em todos os scripts de boss |
| Boss 1 não fica enraged | HP threshold errado ou condição | Enrage em 30% HP: _currentHealth <= _maxHealth * 0.3. Verificar _maxHealth |
| Boss 2 não cria danger zones | _dangerZonePrefab vazio | Arrastar DangerZone.prefab para _dangerZonePrefab |
| Danger zones infinitas | _maxDangerZones não configurado | Verificar _maxDangerZones = 5 |
| Danger zone não causa dano | BoxCollider2D Is Trigger não é true | Marcar Is Trigger = true |
| Boss 3 não muda de fase | _phaseTwoHPPercent errado | Verificar = 0.5 |
| Boss 3 não atira spread | _projectilePrefab vazio | Arrastar EnemyProjectile.prefab |
| Layer check não funciona em nenhum script | _playerLayer não configurado | Verificar TODOS os scripts que usam _playerLayer: EnemyController, BossMelee, BossArea, BossHybrid, RoomController |
| NullReference ao spawnar inimigo | EnemyController não existe no prefab | Verificar: todos os prefabs de inimigo têm EnemyController.cs |
| Inimigos todos melee | Prefabs novos não conectados | Verificar DungeonGenerator tem os 4 prefabs preenchidos |
| Sala spawna só ranged | Regra de balanceamento quebrada | Verificar GetEnemyPrefabsForFloor(): Melee sempre está na pool antes dos outros |
| Elite não é diferente | _isElite não está true | Abrir EliteEnemy.prefab, marcar _isElite = true |

---

### 15.21 Hierarquia Final dos Prefabs Criados

```
Assets/Prefabs/
├── Enemy.prefab (atualizado)
│   ├── _enemyType = Melee
│   └── _playerLayer = Player
├── EliteEnemy.prefab (atualizado)
│   ├── _isElite = true
│   └── _playerLayer = Player
├── EnemyFast.prefab (novo)
│   ├── _enemyType = Fast
│   ├── _moveSpeed = 5, _maxHealth = 15
│   └── Color: ciano, Scale 0.8
├── EnemyHeavy.prefab (novo)
│   ├── _enemyType = Heavy
│   ├── _chargeSpeed = 8, _maxHealth = 90
│   └── Color: laranja, Scale 1.5
├── EnemyRanged.prefab (novo)
│   ├── _enemyType = Ranged
│   ├── _projectilePrefab = EnemyProjectile
│   └── Color: roxo, Scale 0.9
├── EnemyProjectile.prefab (novo)
│   ├── Rigidbody2D (Gravity 0)
│   ├── CircleCollider2D (trigger)
│   ├── EnemyProjectile.cs
│   └── Layer: EnemyProjectile
├── DangerZone.prefab (novo)
│   ├── SpriteRenderer (vermelho A:0.4, Scale 2x2)
│   ├── BoxCollider2D (trigger, Size 2x2)
│   └── DangerZone.cs
├── Boss1.prefab (novo)
│   ├── BossMeleeController.cs
│   ├── 2× CircleCollider2D (trigger + physics)
│   ├── Color: vermelho escuro, Scale 2.5
│   └── Layer: Enemy
├── Boss2.prefab (novo)
│   ├── BossAreaController.cs
│   ├── _dangerZonePrefab = DangerZone
│   ├── _projectilePrefab = EnemyProjectile
│   ├── Color: azul, Scale 2.5
│   └── Layer: Enemy
├── Boss3.prefab (novo)
│   ├── BossHybridController.cs
│   ├── _projectilePrefab = EnemyProjectile
│   ├── Color: verde, Scale 2.5
│   └── Layer: Enemy
├── BossArena_5.prefab (atualizado)
│   └── BossFloorHandler._bossPrefabs[0] = Boss1
├── BossArena_10.prefab (atualizado)
│   └── BossFloorHandler._bossPrefabs[0] = Boss2
└── BossArena_15.prefab (atualizado)
    └── BossFloorHandler._bossPrefabs[0] = Boss3
```
