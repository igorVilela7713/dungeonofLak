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
