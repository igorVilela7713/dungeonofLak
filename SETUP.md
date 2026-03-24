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
| Inimigo não pula | Verificar GroundCheck posição, _groundLayer marcado com Ground, _jumpForce configurado |
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

4. Configurar Walls (mesmo padrão do DungeonRoom):
   - `WallTop` (0, 6, 0) — BoxCollider2D Size (24, 0.5), Layer Ground
   - `WallBottom` (0, -6.5, 0) — BoxCollider2D Size (24, 0.5), Layer Ground

5. Conectar no Inspector:
   - **RoomController.cs**: `_doors`: [DoorLeft, DoorRight], `_roomCenter`: RoomCenter, `_roomType`: Boss
   - **BossFloorHandler.cs**: `_bossPrefab`: Enemy.prefab (por enquanto), `_bossSpawnPoint`: BossSpawnPoint, `_roomController`: self (arrastar BossArena_5)

6. Arrastar para `Assets/Prefabs/`, deletar da cena

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

4. Conectar no Inspector do RewardArea.cs: `_exitTrigger`: arrastar ExitTrigger
5. Arrastar para `Assets/Prefabs/`, deletar da cena

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
- Inspector: `_roomPrefab` = DungeonRoom.prefab, `_exitRoomPrefab` = ExitRoom.prefab, `_bossArenaPrefabs` = [BossArena_5, BossArena_10, BossArena_15], `_rewardRoomPrefab` = RewardRoom.prefab, `_eliteEnemyPrefab` = Enemy.prefab (com _isElite=true), `_trapPrefabs` = [TrapSpike.prefab], `_runePickupPrefab` = RunePickup.prefab, `_healingShrinePrefab` = HealingShrine.prefab, `_riskRewardAltarPrefab` = RiskRewardAltar.prefab, `_player` = Player, `_seed` = 0

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

4. **Verificar GameManager:**
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
| Boss não spawna | Verificar _bossPrefab e _bossSpawnPoint no BossFloorHandler |
| Andar não avança | Verificar FloorTransition no ExitTrigger, isTrigger=true no BoxCollider2D |
| Inimigos fracos | Verificar _hpScalingPerFloor e _dmgScalingPerFloor no FloorConfigSO |
| NullReference na geração | Verificar todos os prefabs atribuídos no Inspector do DungeonGenerator |
| FloorUI não aparece | Verificar FloorUI.cs conectado, TextMeshProUGUI arrastados |
| Trap não causa dano | Verificar isTrigger=true, layer "player" ou "Enemy" |
| Runas não dropam | Verificar _runePickupPrefab no DungeonGenerator |
| Runas não resetam | Verificar GameManager chama RunCurrency.ResetRunes() |
| Upgrade não aplica | Verificar RunUpgradeManager refs: _playerHealth, _playerController |
| HealingShrine não cura | Verificar isTrigger=true, PlayerHealth tem HealPercent() |
| RiskRewardAltar não responde | Verificar InputAction E habilitado, _playerInRange |
| Erro "Instance is null" | Verificar singleton GameObjects existem na cena antes de Play |
