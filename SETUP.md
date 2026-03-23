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
