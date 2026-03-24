# SPEC — Fase 7: Dungeon Procedural e Progressão de Andares

## Arquivos a Criar

| Path | Tipo | Descrição |
|------|------|-----------|
| `Assets/Scripts/Dungeon/FloorManager.cs` | Script | Singleton que controla andar atual, tipo (Normal/Boss/Reward), avanço de andar |
| `Assets/Scripts/Dungeon/DungeonGenerator.cs` | Script | Gera salas usando Random Walk em grid, instancia Room prefabs em world space |
| `Assets/Scripts/Dungeon/FloorTransition.cs` | Script | Trigger na sala de exit que avança para o próximo andar |
| `Assets/Scripts/Dungeon/DifficultyScaler.cs` | Script | Static utility com fórmulas de multiplicador de HP e dano por andar |
| `Assets/Scripts/Dungeon/BossFloorHandler.cs` | Script | Gerencia fluxo de andar de boss: arena, boss, transição para reward |
| `Assets/Scripts/Dungeon/RewardArea.cs` | Script | Sala segura pós-boss, placeholder para upgrade futuro |
| `Assets/Scripts/Dungeon/TrapBase.cs` | Script | Base para traps que causam dano ao player e inimigos (espinhos, fogo, etc.) |
| `Assets/Scripts/Dungeon/RoomType.cs` | Script | Enum `RoomType` e enum `FloorType` usados pelo sistema |
| `Assets/Scripts/Dungeon/RunCurrency.cs` | Script | Singleton que gerencia runas da run atual (dropadas por monstros, perdidas ao morrer) |
| `Assets/Scripts/Dungeon/RunePickup.cs` | Script | Pickup de runa dropado por inimigos ao morrer |
| `Assets/Scripts/Dungeon/RunUpgradeManager.cs` | Script | Gerencia upgrades temporários da run (dano, velocidade, vida, etc.) |
| `Assets/Scripts/Dungeon/RunUpgradeSO.cs` | ScriptableObject | Dados de cada upgrade: nome, tipo, valor, custo em runas, trade-off |
| `Assets/Scripts/Dungeon/HealingShrine.cs` | Script | Altar de cura entre andares (recupera % da vida) |
| `Assets/Scripts/Dungeon/RiskRewardAltar.cs` | Script | Altar de risco/recompensa: aceitar dano em troca de runas ou upgrade |
| `Assets/Scripts/UI/FloorUI.cs` | Script | Texto na tela mostrando andar atual e runas (TextMeshProUGUI) |
| `Assets/Data/FloorConfigSO.cs` | ScriptableObject | Configuração por andar: roomCount, enemyCount, difficultyMult, bossFloors |

## Arquivos a Modificar

| Path | Mudanças |
|------|----------|
| `Assets/Scripts/Enemies/EnemyController.cs` | Adicionar `_healthMultiplier` (float), `_damageMultiplier` (float). `ApplyDifficulty(float hpMult, float dmgMult)`. `_isElite` (bool) com cor amarela e escala 1.3x. Dropar `RunePickup` ao morrer. Adicionar propriedade `int RuneValue` baseada em dificuldade. |
| `Assets/Scripts/Rooms/RoomController.cs` | `InitializeReferences(Transform player, GameObject enemyPrefab)`, `InitializeDoors(Door[] doors)`, `SetActiveDoors(Door[] activeDoors)`. Campo `_roomType` (RoomType enum). `bool IsCleared`. Lógica de spawn por RoomType (Elite, Reward, RiskReward, etc.). |
| `Assets/Scripts/Core/GameManager.cs` | Em `OnPlayerDeath()`: chamar `FloorManager.Instance?.ResetFloor()`, `RunCurrency.Instance?.ResetRunes()`, `RunUpgradeManager.Instance?.ResetUpgrades()`, depois `SceneManager.LoadScene()`. |
| `Assets/Scripts/Core/SaveData.cs` | Adicionar `public int maxFloorReached = 1;` (não `currentFloor` — roguelike, sempre volta ao andar 1). Atualizar `CreateDefault()`. |
| `Assets/Scripts/Combat/PlayerHealth.cs` | Adicionar método público `HealPercent(float percent)` que cura percentual da vida máxima. Já existe `HealToFull()`. |

---

## Setup na Unity

### Passo 1: Criar RoomType.cs e FloorType.cs enums

1. Criar `Assets/Scripts/Dungeon/RoomType.cs` com o enum:
   ```csharp
   public enum RoomType { Combat, Elite, Reward, Rest, Event, Shop, Boss }
   public enum FloorType { Normal, Boss, Reward }
   ```

### Passo 2: Criar FloorConfigSO ScriptableObject

1. Criar script `Assets/Scripts/Dungeon/FloorConfigSO.cs`
2. Na Unity: `Assets → Create → Dungeon → Floor Config`
3. Criar asset em `Assets/Data/DefaultFloorConfig.asset`
4. Configurar no Inspector:
   - `_baseRoomCount`: 4
   - `_maxRoomCount`: 8
   - `_baseRoomSize`: 20 (width da sala em world units)
   - `_bossFloors`: [5, 10, 15]
   - `_hpScalingPerFloor`: 0.1
   - `_dmgScalingPerFloor`: 0.08
   - `_roomSizeScalingPerFloor`: 0.0 (para futuro: andares maiores)

### Passo 3: Criar DungeonRoom Prefab

Estrutura do prefab:

```
DungeonRoom (RoomController.cs)
├── RoomCenter (Empty, posição central da sala)
├── SpawnPoint1 (Empty, posição de spawn)
├── SpawnPoint2 (Empty, posição de spawn)
├── SpawnPoint3 (Empty, posição de spawn) — opcional
├── EliteSpawnPoint (Empty, posição de spawn de elite) — opcional
├── DoorLeft (Door prefab, lado esquerdo da sala)
├── DoorRight (Door prefab, lado direito da sala)
├── Ground (SpriteRenderer ou Tilemap, Layer = Ground)
├── Walls (BoxCollider2D nas bordas, Layer = Ground)
└── RoomTrigger (BoxCollider2D, isTrigger=true, tamanho da área jogável)
```

No Inspector do RoomController:

- `_spawnPoints`: arrastar SpawnPoint1, SpawnPoint2
- `_eliteSpawnPoint`: arrastar EliteSpawnPoint (se houver)
- `_enemyPrefab`: **deixar vazio** (DungeonGenerator seta via code)
- `_doors`: arrastar DoorLeft, DoorRight
- `_player`: **deixar vazio** (DungeonGenerator seta via code)
- `_roomCenter`: arrastar RoomCenter
- `_roomType`: definir no prefab (Combat por default)

No Inspector do RoomTrigger (BoxCollider2D):

- `isTrigger = true`
- Size: cobrir toda área jogável (ex: 18x10)
- Layer: Default (não precisa de layer específico)

DoorLeft / DoorRight:

- Posicionar nas bordas da sala (posição de entrada/saída)
- Inicialmente desativadas (DungeonGenerator ativa as que são usadas)
- Usam `Door.cs` com collider físico — o player atravessa quando aberta, bloqueia quando fechada

### Passo 4: Criar ExitRoom Prefab

Mesma estrutura do DungeonRoom, mas:

- Adicionar `FloorTransition.cs` em um Empty chamado "ExitTrigger"
- ExitTrigger: BoxCollider2D `isTrigger = true`, Size (1.5, 2.5), posicionado como portal de saída
- FloorTransition: sem campos configuráveis (auto-detecta FloorManager)
- Chão e paredes contínuos com a sala anterior (o player anda fisicamente até o exit)

### Passo 5: Criar BossArena Prefab (1 para cada boss)

Criar 3 prefabs separados, um para cada boss (andares 5, 10, 15). Cada um com controller próprio:

```
BossArena_5 (RoomController.cs, BossFloorHandler.cs)
├── RoomCenter (Empty)
├── BossSpawnPoint (Empty, posição central da arena)
├── PlayerSpawnPoint (Empty, posição de entrada do player)
├── DoorLeft (Door prefab — trava player na arena durante o combate)
├── DoorRight (Door prefab — trava player na arena durante o combate)
├── Ground (SpriteRenderer ou Tilemap, Layer = Ground)
├── Walls (BoxCollider2D nas bordas)
└── RoomTrigger (BoxCollider2D, isTrigger=true)
```

No Inspector do BossFloorHandler:

- `_bossPrefab`: arrastar Boss prefab específico (Boss_5, Boss_10 ou Boss_15)
- `_bossSpawnPoint`: arrastar BossSpawnPoint
- `_roomController`: arrastar RoomController (self)
- `_rewardFloorTransition`: arrastar FloorTransition (self, ou null se for gerado separadamente)

> Os 3 bosses terão controllers próprios com move sets distintos. Por enquanto serão placeholders com EnemyController + ApplyDifficulty(3x HP, 2x dano) e sprites diferentes. Na fase futura, substituir por scripts de boss específicos.

### Passo 6: Criar RewardRoom Prefab

```
RewardRoom (RewardArea.cs)
├── PlayerSpawnPoint (Empty, posição de entrada)
├── ExitTrigger (FloorTransition.cs, BoxCollider2D isTrigger=true)
├── Ground (SpriteRenderer ou Tilemap, Layer = Ground)
└── Walls (BoxCollider2D nas bordas)
```

No Inspector do RewardArea:

- `_exitTrigger`: arrastar ExitTrigger

### Passo 7: Criar Trap Prefab

```
TrapSpike (TrapBase.cs)
├── SpriteRenderer (sprite de espinho placeholder)
├── BoxCollider2D (isTrigger = true, tamanho do trap)
└── Animator (opcional, para animação de ativação)
```

No Inspector do TrapBase:

- `_damage`: 5
- `_damageInterval`: 0.5 (dano por segundo enquanto no trigger)
- `_trapType`: Spike
- `_affectsEnemies`: true (traps causam dano tanto no player quanto nos inimigos — interação tática)

> Traps são interativas: o jogador pode levar inimigos para cima de traps para causar dano. TrapBase verifica tanto a layer "player" quanto "enemy" e aplica dano via `IDamageable`.

### Passo 8: Criar RunePickup Prefab

```
RunePickup (RunePickup.cs)
├── SpriteRenderer (sprite de runa placeholder — ex: gem do SunnyLand)
├── CircleCollider2D (isTrigger = true, radius 0.3)
└── Animator (opcional, para brilho/flutuação)
```

No Inspector do RunePickup:

- `_runeValue`: 1 (default, EnemyController seta via code antes de destruir)

### Passo 9: Criar HealingShrine Prefab

```
HealingShrine (HealingShrine.cs)
├── SpriteRenderer (sprite de altar placeholder)
├── BoxCollider2D (isTrigger = true, Size 1.5x2)
└── Animator (opcional, para efeito de brilho)
```

No Inspector do HealingShrine:

- `_healPercent`: 0.25 (cura 25% da vida máxima)
- `_dialogueText`: "Você sente sua força retornar..."

### Passo 10: Criar RunUpgradeSO ScriptableObject

1. Criar script `Assets/Scripts/Dungeon/RunUpgradeSO.cs`
2. Na Unity: `Assets → Create → Dungeon → Run Upgrade`
3. Criar alguns assets em `Assets/Data/Upgrades/`:
   - `UpgradeDamage.asset` — tipo: Damage, valor: 1.2 (multiplicador), custo: 15 runas
   - `UpgradeSpeed.asset` — tipo: Speed, valor: 1.15, custo: 10 runas
   - `UpgradeHealth.asset` — tipo: MaxHealth, valor: 1.25, custo: 20 runas
   - `UpgradeCritChance.asset` — tipo: CritChance, valor: 0.1, custo: 25 runas
   - `UpgradeTradeoffDamage.asset` — tipo: Damage, valor: 1.5, custo: 10 runas, tradeOffType: MaxHealth, tradeOffValue: 0.75 ("+50% dano, -25% vida")

### Passo 11: Criar FloorUI na Cena

1. Abrir cena `Main.unity`
2. Clique direito na Hierarchy → **UI → Canvas**
   - Canvas Scaler → UI Scale Mode: **Scale With Screen Size**
   - Reference Resolution: **1920 × 1080**
   - Match Width Or Height: **0.5**
3. Clique direito no Canvas → **UI → Text - TextMeshPro**
   - Renomear para **"FloorText"**
   - Anchor: **top center** (arrastar no ícone de ancoragem)
   - Font Size: **32**
   - Color: **branco** (com outline para legibilidade)
   - Alignment: centro horizontal, centro vertical
   - Pos Y: offset do topo (ex: -30)
   - Texto inicial: "Floor 1"
4. Clique direito no Canvas → **UI → Text - TextMeshPro**
   - Renomear para **"RuneText"**
   - Anchor: **top right** (canto superior direito)
   - Font Size: **28**
   - Color: **amarelo** (runas)
   - Texto inicial: "Runas: 0"
5. Criar Empty GameObject: **"FloorUI"**
   - Adicionar `FloorUI.cs`
   - `_floorText`: arrastar o componente TextMeshProUGUI do FloorText
   - `_runeText`: arrastar o componente TextMeshProUGUI do RuneText

### Passo 12: Configurar Cena Main

1. Abrir cena `Main.unity`
2. Criar Empty GameObject: **"FloorManager"**
   - Adicionar `FloorManager.cs`
   - `_dungeonGenerator`: arrastar DungeonGenerator (próximo passo)
   - `_floorUI`: arrastar FloorUI
   - `_player`: arrastar Player da cena
   - `_floorConfig`: arrastar `DefaultFloorConfig.asset`
   - `_roomWidth`: 20
   - `_roomHeight`: 12
3. Criar Empty GameObject: **"DungeonGenerator"**
   - Adicionar `DungeonGenerator.cs`
   - `_roomPrefab`: arrastar DungeonRoom prefab
   - `_exitRoomPrefab`: arrastar ExitRoom prefab
   - `_bossArenaPrefabs`: array com [BossArena_5, BossArena_10, BossArena_15]
   - `_rewardRoomPrefab`: arrastar RewardRoom prefab
   - `_runePickupPrefab`: arrastar RunePickup prefab
   - `_healingShrinePrefab`: arrastar HealingShrine prefab
   - `_player`: arrastar Player da cena
   - `_seed`: 0 (gera seed aleatório no runtime) ou fixo para debug
4. Criar Empty GameObject: **"RunCurrency"**
   - Adicionar `RunCurrency.cs`
   - `_floorUI`: arrastar FloorUI
5. Criar Empty GameObject: **"RunUpgradeManager"**
   - Adicionar `RunUpgradeManager.cs`
   - `_playerHealth`: arrastar PlayerHealth do Player
   - `_playerController`: arrastar PlayerController do Player
6. No GameManager existente:
   - Verificar que `_sceneName` = "Hub"
7. **Remover a Room estática da cena** — DungeonGenerator cria salas dinamicamente
8. Garantir que Player existe na cena para teste (será reposicionado por FloorManager)

### Passo 13: Player Spawn Point

1. Criar Empty: **"PlayerSpawn"** na posição onde o player deve começar (0, 0)
2. FloorManager reposiciona o player na sala "start" gerada — PlayerSpawn não é mais necessário após geração

### Passo 14: Verificar Layers e Physics

Layers existentes já cobrem:

- Player (8) — player detectado via `LayerMask.NameToLayer("player")`
- Enemy (9)
- Ground (11)

Physics 2D Collision Matrix não precisa de alteração (salas não usam layer específico).

---

## Atualização do SETUP.md

Adicionar seção **"13. Fase 7 — Dungeon Procedural"** ao final do SETUP.md:

### Conteúdo a adicionar

```
## 13. Fase 7 — Dungeon Procedural

### 13.1 Arquivos Criados

| Arquivo | Função |
|---------|--------|
| Assets/Scripts/Dungeon/FloorManager.cs | Controla andar atual, progressão, tipo de andar |
| Assets/Scripts/Dungeon/DungeonGenerator.cs | Gera salas via Random Walk com seed |
| Assets/Scripts/Dungeon/FloorTransition.cs | Trigger para avançar andar |
| Assets/Scripts/Dungeon/DifficultyScaler.cs | Multiplicadores de dificuldade por andar |
| Assets/Scripts/Dungeon/BossFloorHandler.cs | Fluxo de andar de boss |
| Assets/Scripts/Dungeon/RewardArea.cs | Sala de recompensa pós-boss |
| Assets/Scripts/Dungeon/RoomType.cs | Enums RoomType e FloorType |
| Assets/Scripts/Dungeon/TrapBase.cs | Trap que causa dano no player e inimigos |
| Assets/Scripts/Dungeon/RunCurrency.cs | Economia de runas da run atual |
| Assets/Scripts/Dungeon/RunePickup.cs | Pickup de runa dropado por inimigos |
| Assets/Scripts/Dungeon/RunUpgradeManager.cs | Gerencia upgrades temporários da run |
| Assets/Scripts/Dungeon/RunUpgradeSO.cs | ScriptableObject de dados de upgrade |
| Assets/Scripts/Dungeon/HealingShrine.cs | Altar de cura |
| Assets/Scripts/Dungeon/RiskRewardAltar.cs | Altar de risco/recompensa |
| Assets/Scripts/UI/FloorUI.cs | Texto na tela com andar atual e runas |
| Assets/Data/FloorConfigSO.cs | ScriptableObject de configuração |

### 13.2 Prefabs Criados

| Prefab | Uso |
|--------|-----|
| DungeonRoom.prefab | Sala padrão para geração procedural |
| ExitRoom.prefab | Sala com FloorTransition para próximo andar |
| BossArena_5.prefab | Arena de boss do andar 5 |
| BossArena_10.prefab | Arena de boss do andar 10 |
| BossArena_15.prefab | Arena de boss do andar 15 |
| RewardRoom.prefab | Sala segura pós-boss |
| TrapSpike.prefab | Trap de espinho (placeholder) |
| RunePickup.prefab | Pickup de runa |
| HealingShrine.prefab | Altar de cura |
| RiskRewardAltar.prefab | Altar de risco/recompensa |

### 13.3 Configuração na Cena Main

[Igual ao Passo 9 acima, detalhado]

### 13.4 Como Testar

1. Abrir cena Main
2. Play Mode
3. FloorUI mostra "Floor 1" no topo da tela
4. FloorManager gera salas em andar 1 (tamanho base)
5. Player spawna na sala "start"
6. Entrar em sala Combat → inimigos spawneam
7. Entrar em sala Elite → inimigo elite spawna (amarelo, mais forte)
8. Matar todos → portas abrem
9. Chegar na sala exit → transição para andar 2
10. Andar 2 gera layout diferente (seed diferente ou mesma seed)
11. Andar 5 carrega BossArena_5
12. Matar boss → player recupera vida → transição para RewardRoom
13. RewardRoom → transição para andar 6
14. Andares mais altos têm salas maiores e mais inimigos
15. Trap de espinho causa dano ao player ao contato
16. Morte → volta para Hub, FloorManager é destruído
17. Voltar para dungeon → começa do andar 1

### 13.5 Troubleshooting

| Problema | Solução |
|----------|---------|
| Salas não aparecem | Verificar RoomPrefab no DungeonGenerator, tamanho de sala |
| Player não spawna | Verificar _player no FloorManager |
| Salas sobrepostas | Verificar _roomWidth/_roomHeight = tamanho real da sala |
| Portas não funcionam | Verificar Door refs no RoomController, InitializeDoors() |
| Boss não spawna | Verificar BossFloorHandler, _bossPrefab, _bossSpawnPoint |
| Andar não avança | Verificar FloorTransition no ExitTrigger, isTrigger=true |
| Inimigos fracos demais | Verificar DifficultyScaler, _hpScalingPerFloor configurado |
| NullReference na geração | Verificar todos os prefabs atribuídos no DungeonGenerator |
| FloorUI não mostra | Verificar FloorUI.cs no Canvas, TextMeshProUGUI conectado |
| Trap não causa dano | Verificar isTrigger=true no BoxCollider2D, player na layer certa |
| Seed gera layout igual | Verificar seed mudando (0 = aleatório, número fixo = reproduzível) |
| Runas não dropam | Verificar RunePickup prefab no DungeonGenerator, EnemyController instancia ao morrer |
| Runas não somam | Verificar RunCurrency.Instance existe na cena, AddRunes() é chamado |
| Runas não resetam ao morrer | Verificar GameManager.OnPlayerDeath() chama RunCurrency.ResetRunes() |
| Upgrade não aplica | Verificar RunUpgradeManager.Instance, _playerHealth e _playerController conectados |
| Trade-off não aplica | Verificar hasTradeOff=true no RunUpgradeSO, ambos os campos preenchidos |
| HealingShrine não cura | Verificar isTrigger=true no collider, PlayerHealth.HealPercent() existe |
| RiskRewardAltar não funciona | Verificar InputAction habilitado, _playerInRange, DialogueUI.Instance existe |
```

---

## Checklist de Implementação

### Fase A — Sistema Base

- [x] **Passo 1:** Criar `Assets/Scripts/Dungeon/RoomType.cs`
  - Arquivo: `Assets/Scripts/Dungeon/RoomType.cs`
  - O que fazer: Dois enums públicos no mesmo arquivo:
    ```csharp
    public enum RoomType { Combat, Elite, Reward, Rest, Event, Shop, Boss }
    public enum FloorType { Normal, Boss, Reward }
    ```

- [x] **Passo 2:** Criar `Assets/Scripts/Dungeon/FloorConfigSO.cs`
  - Arquivo: `Assets/Scripts/Dungeon/FloorConfigSO.cs`
  - O que fazer: ScriptableObject com `[CreateAssetMenu(menuName = "Dungeon/Floor Config")]`. Campos:
    - `_baseRoomCount` (int, 4)
    - `_maxRoomCount` (int, 8)
    - `_baseRoomWidth` (float, 20) — largura base da sala em world units
    - `_roomWidthScalingPerFloor` (float, 0.05) — andares mais altos = salas maiores
    - `_bossFloors` (int[], [5, 10, 15])
    - `_hpScalingPerFloor` (float, 0.1)
    - `_dmgScalingPerFloor` (float, 0.08)
    - `_trapChance` (float, 0.3) — chance de sala ter trap
    - Método público `GetRoomWidth(int floor)` que retorna `_baseRoomWidth * (1 + (_roomWidthScalingPerFloor * (floor - 1)))`

- [x] **Passo 3:** Criar `Assets/Scripts/Dungeon/FloorManager.cs`
  - Arquivo: `Assets/Scripts/Dungeon/FloorManager.cs`
  - O que fazer: MonoBehaviour singleton (pattern igual a `DialogueUI`).
  - Campos: `_currentFloor` (int), `_floorConfig` (FloorConfigSO), `_player` (Transform), `_dungeonGenerator` (DungeonGenerator), `_floorUI` (FloorUI).
  - Propriedades públicas: `static Instance`, `int CurrentFloor => _currentFloor`, `FloorType CurrentFloorType`.
  - Métodos:
    - `NextFloor()`: `_currentFloor++`, chamar `_dungeonGenerator.GenerateFloor(_currentFloor)`, atualizar UI via `_floorUI.UpdateFloor(_currentFloor)`.
    - `ResetFloor()`: `_currentFloor = 1`, chamar `_dungeonGenerator.ClearFloor()`, destruir FloorManager (já que é DontDestroyOnLoad mas precisa ser destruído ao morrer — ver GameManager modificação).
    - `IsBossFloor(int floor)`: verifica se floor está em `_floorConfig._bossFloors`.
    - `GetCurrentFloorType()`: retorna `FloorType.Boss` se `IsBossFloor()`, senão `FloorType.Normal`.
  - Em `Awake()`: singleton pattern com `DontDestroyOnLoad`. Se já existe instância, destruir a nova.
  - Em `Start()`: chamar `_dungeonGenerator.GenerateFloor(_currentFloor)` e `_floorUI.UpdateFloor(_currentFloor)`.

- [x] **Passo 4:** Criar `Assets/Scripts/Dungeon/DungeonGenerator.cs`
  - Arquivo: `Assets/Scripts/Dungeon/DungeonGenerator.cs`
  - O que fazer: MonoBehaviour.
  - Campos:
    - `_roomPrefab` (GameObject)
    - `_exitRoomPrefab` (GameObject)
    - `_bossArenaPrefabs` (GameObject[]) — array de 3 arenas
    - `_rewardRoomPrefab` (GameObject)
    - `_eliteEnemyPrefab` (GameObject) — inimigo elite separado
    - `_trapPrefabs` (GameObject[]) — array de tipos de trap
    - `_player` (Transform)
    - `_seed` (int, 0) — 0 = aleatório, >0 = seed fixo
    - `_roomWidth` (float, 20)
    - `_roomHeight` (float, 12)
  - Campo privado: `System.Random _rng`
  - Campo privado: `List<GameObject> _generatedRooms` (para cleanup)
  - Método `GenerateFloor(int floorNumber)`:
    1. Chamar `ClearFloor()` primeiro
    2. Determinar seed: se `_seed == 0`, usar `UnityEngine.Random.Range(int.MinValue, int.MaxValue)`. Senão usar `_seed + floorNumber` (seed por andar).
    3. Criar `System.Random` com seed
    4. Calcular roomCount: `_baseRoomCount + (floorNumber / 3)`, clamp em `_maxRoomCount`
    5. Calcular roomWidth atual: `_floorConfig.GetRoomWidth(floorNumber)`
    6. Executar Random Walk:
       ```csharp
       HashSet<Vector2Int> positions = new HashSet<Vector2Int>();
       Vector2Int current = Vector2Int.zero;
       positions.Add(current);
       for (int i = 0; i < roomCount; i++)
       {
           Vector2Int direction = GetRandomDirection(); // cima, baixo, esquerda, direita
           current += direction;
           positions.Add(current);
       }
       ```
    7. Converter `positions` para `List<Vector2Int>` e ordenar por distância da origem (para identificar start e exit)
    8. O primeiro (mais próximo da origem) = sala start
    9. O último (mais distante) = sala exit
    10. Para cada posição, decidir RoomType por peso:
        - Sala start: nunca ter inimigos
        - Sala exit: sem inimigos, com FloorTransition
        - Outras: Combat (45%), Elite (15%), Reward (10%), Rest (10%), Event/RiskReward (10%), Shop (5%), HealingShrine (5%)
        > Note: Shop e HealingShrine são ambos tipos de sala segura. Event é RiskReward (aceitar dano por recompensa)
    11. Instanciar prefab correto na posição world: `new Vector3(pos.x * roomWidth, pos.y * roomHeight, 0)`
    12. Configurar RoomController via `rc.InitializeReferences(_player, enemyPrefab)`
    13. Se sala é Elite: setar `rc.SetEliteMode(true)`
    14. Se sala tem trap (baseado em `_floorConfig._trapChance`): instanciar trap prefab em posição aleatória da sala
    15. Reposicionar player na sala start: `_player.position = startPosition + spawnOffset`
  - Método `GenerateBossFloor(int floorNumber)`:
    1. `ClearFloor()`
    2. Determinar qual boss arena usar: índice = `(floorNumber / 5) - 1` (andar 5 → 0, andar 10 → 1, andar 15 → 2)
    3. Instanciar `_bossArenaPrefabs[index]` na posição (0, 0, 0)
    4. Configurar referências via `BossFloorHandler.Initialize(_player)`
    5. Reposicionar player no `PlayerSpawnPoint` da arena
  - Método `GenerateRewardFloor()`:
    1. `ClearFloor()`
    2. Instanciar `_rewardRoomPrefab` na posição (0, 0, 0)
    3. Configurar referências
    4. Reposicionar player
  - Método `ClearFloor()`: destruir todos os GameObjects em `_generatedRooms`, limpar lista
  - Método `GetRandomDirection()`: retorna Vector2Int aleatório entre (1,0), (-1,0), (0,1), (0,-1)

- [x] **Passo 5:** Criar `Assets/Scripts/Dungeon/FloorTransition.cs`
  - Arquivo: `Assets/Scripts/Dungeon/FloorTransition.cs`
  - O que fazer: MonoBehaviour simples.
  - Campo `_isTriggered` (bool) para evitar double-fire.
  - `OnTriggerEnter2D`: se layer é "player" e `!_isTriggered`, setar `_isTriggered = true`, chamar `FloorManager.Instance.NextFloor()`

- [x] **Passo 6:** Criar `Assets/Scripts/Dungeon/DifficultyScaler.cs`
  - Arquivo: `Assets/Scripts/Dungeon/DifficultyScaler.cs`
  - O que fazer: Classe estática (não MonoBehaviour).
  - Métodos públicos estáticos:
    - `GetHPMultiplier(int floor, float scalingPerFloor)` → `1f + (floor - 1) * scalingPerFloor`
    - `GetDamageMultiplier(int floor, float scalingPerFloor)` → `1f + (floor - 1) * scalingPerFloor`
    - `GetRoomWidthMultiplier(int floor, float scalingPerFloor)` → `1f + (floor - 1) * scalingPerFloor`

### Fase B — Modificações em Scripts Existentes

- [x] **Passo 7:** Modificar `Assets/Scripts/Rooms/RoomController.cs`
  - Arquivo: `Assets/Scripts/Rooms/RoomController.cs`
  - O que fazer:
    1. Adicionar campo `[SerializeField] private RoomType _roomType = RoomType.Combat;`
    2. Adicionar campo `[SerializeField] private Transform _eliteSpawnPoint;` — ponto de spawn de inimigo elite
    3. Adicionar campo `[SerializeField] private GameObject _eliteEnemyPrefab;` — prefab do inimigo elite
    4. Adicionar campo privado `private bool _isEliteMode = false;`
    5. Adicionar método público `InitializeReferences(Transform player, GameObject enemyPrefab)` que seta `_player` e `_enemyPrefab`
    6. Adicionar método público `InitializeDoors(Door[] doors)` que seta `_doors`
    7. Adicionar método público `SetActiveDoors(Door[] activeDoors)` — desativa doors não usados na lista, ativa os usados
    8. Adicionar método público `SetEliteMode(bool isElite)` que seta `_isEliteMode`
    9. Adicionar propriedade pública `bool IsCleared => _enemiesAlive <= 0;`
    10. Adicionar propriedade pública `RoomType RoomType => _roomType;`
    11. Modificar `SpawnEnemies()`:
        - Se `_roomType` é Reward, Rest, ou Shop: não spawnar inimigos, abrir portas imediatamente. Se é Reward, instanciar HealingShrine se DungeonGenerator configurar
        - Se `_roomType` é Elite e `_eliteSpawnPoint` não é null: spawnar inimigo elite no `_eliteSpawnPoint` usando `_eliteEnemyPrefab`
        - Se `_roomType` é Event (RiskReward): não spawnar inimigos. Instanciar altar de risco/recompensa (placeholder: trigger que oferece runas em troca de dano ao player)
        - Se `_roomType` é Combat: comportamento atual (spawnar inimigos normais)
        - Se `_roomType` é Boss: comportamento atual (gerenciado por BossFloorHandler)

- [x] **Passo 8:** Modificar `Assets/Scripts/Enemies/EnemyController.cs`
  - Arquivo: `Assets/Scripts/Enemies/EnemyController.cs`
  - O que fazer:
    1. Adicionar campo `[SerializeField] private bool _isElite = false;`
    2. Adicionar campos privados: `private float _healthMultiplier = 1f;` `private float _damageMultiplier = 1f;`
    3. Adicionar método público:
       ```csharp
       public void ApplyDifficulty(float hpMult, float dmgMult)
       {
           _healthMultiplier = hpMult;
           _damageMultiplier = dmgMult;
           _maxHealth = Mathf.RoundToInt(_maxHealth * _healthMultiplier);
           _currentHealth = _maxHealth;
           _damageToPlayer = Mathf.RoundToInt(_damageToPlayer * _damageMultiplier);
       }
       ```
    4. No final de `Awake()`, adicionar: se `_isElite`, aplicar `SpriteRenderer.color = Color.yellow` e escala `transform.localScale *= 1.3f`
    5. Chamar `ApplyDifficulty()` **antes** de `Awake()` — RoomController ou DungeonGenerator chama logo após `Instantiate()`

- [x] **Passo 9:** Modificar `Assets/Scripts/Core/GameManager.cs`
  - Arquivo: `Assets/Scripts/Core/GameManager.cs`
  - O que fazer: No `OnPlayerDeath()`, adicionar antes de `SceneManager.LoadScene()`:
    ```csharp
    if (FloorManager.Instance != null)
    {
        FloorManager.Instance.ResetFloor();
        Destroy(FloorManager.Instance.gameObject);
    }
    if (RunCurrency.Instance != null)
    {
        RunCurrency.Instance.ResetRunes();
        Destroy(RunCurrency.Instance.gameObject);
    }
    if (RunUpgradeManager.Instance != null)
    {
        RunUpgradeManager.Instance.ResetUpgrades();
        Destroy(RunUpgradeManager.Instance.gameObject);
    }
    ```
    > FloorManager, RunCurrency e RunUpgradeManager usam DontDestroyOnLoad. Ao morrer, todos precisam ser destruídos para que ao re-entrar na dungeon, novas instâncias sejam criadas com estado limpo.

- [x] **Passo 10:** Modificar `Assets/Scripts/Core/SaveData.cs`
  - Arquivo: `Assets/Scripts/Core/SaveData.cs`
  - O que fazer: Adicionar campo `public int maxFloorReached = 1;`. Atualizar `CreateDefault()` para incluir o novo campo.
  > `maxFloorReached` é progressão permanente (recorde). O andar atual não é salvo — é roguelike, sempre volta ao andar 1 ao morrer.

- [x] **Passo 10b:** Modificar `Assets/Scripts/Combat/PlayerHealth.cs`
  - Arquivo: `Assets/Scripts/Combat/PlayerHealth.cs`
  - O que fazer: Adicionar método público:
    ```csharp
    public void HealPercent(float percent)
    {
        int healAmount = Mathf.RoundToInt(_maxHealth * percent);
        _currentHealth = Mathf.Min(_currentHealth + healAmount, _maxHealth);
    }
    ```

### Fase C — Boss, Reward, Run Economy, Traps

- [x] **Passo 11:** Criar `Assets/Scripts/Dungeon/BossFloorHandler.cs`

- [x] **Passo 11:** Criar `Assets/Scripts/Dungeon/BossFloorHandler.cs`
  - Arquivo: `Assets/Scripts/Dungeon/BossFloorHandler.cs`
  - O que fazer: MonoBehaviour.
  - Campos:
    - `_bossPrefab` (GameObject)
    - `_bossSpawnPoint` (Transform)
    - `_roomController` (RoomController)
    - `_playerHealAmount` (int, 100) — quantidade de cura ao matar boss
  - Método público `Initialize(Transform player)`: armazena referência do player, chama `SpawnBoss()`
  - Método `SpawnBoss()`: instancia `_bossPrefab` em `_bossSpawnPoint.position`, chama `boss.Initialize(_player)`, aplica difficulty scaling via `DifficultyScaler.GetHPMultiplier()` e `GetDamageMultiplier()`, adiciona `EnemyDeathTracker`
  - Quando boss morre (EnemyDeathTracker chama `OnEnemyKilled`):
    1. Curar o player: `_player.GetComponent<PlayerHealth>().HealToFull()` (player recupera vida ao matar boss)
    2. Chamar `FloorManager.Instance.NextFloor()`

- [x] **Passo 12:** Criar `Assets/Scripts/Dungeon/RewardArea.cs`
  - Arquivo: `Assets/Scripts/Dungeon/RewardArea.cs`
  - O que fazer: MonoBehaviour simples.
  - Campos: `_exitTrigger` (FloorTransition).
  - Em `Start()`: ativar `_exitTrigger`.
  - Placeholder: pode ter NPC ou altar futuro. Sem inimigos, sem portas fechadas.

- [x] **Passo 13:** Criar `Assets/Scripts/Dungeon/TrapBase.cs`
  - Arquivo: `Assets/Scripts/Dungeon/TrapBase.cs`
  - O que fazer: MonoBehaviour.
  - Campos:
    - `_damage` (int, 5)
    - `_damageInterval` (float, 0.5) — intervalo entre danos contínuos
    - `_trapType` (string, "Spike") — tipos: Spike, Blade, Fire, Poison
  - Campo privado: `_lastDamageTime` (float)
  - `OnTriggerEnter2D`: se layer é "player" ou "enemy" e cooldown passou, aplicar dano via `IDamageable`
  - `OnTriggerStay2D`: se layer é "player" ou "enemy" e `_damageInterval` passou desde `_lastDamageTime`, aplicar dano
  - Traps são interativas: o jogador pode levar inimigos para cima de traps para causar dano taticamente

### Fase D — Run Economy (Economia de Run)

- [x] **Passo 14:** Criar `Assets/Scripts/Dungeon/RunCurrency.cs`
  - Arquivo: `Assets/Scripts/Dungeon/RunCurrency.cs`
  - O que fazer: MonoBehaviour singleton (pattern igual a `DialogueUI` com `DontDestroyOnLoad`).
  - Campos:
    - `_currentRunes` (int, 0) — runas acumuladas na run atual
    - `_floorUI` (FloorUI) — para atualizar texto de runas
  - Propriedades públicas: `static Instance`, `int CurrentRunes => _currentRunes`
  - Métodos públicos:
    - `AddRunes(int amount)`: `_currentRunes += amount`, atualizar UI
    - `SpendRunes(int amount)`: retorna `bool` — se `_currentRunes >= amount`, subtrai e retorna true. Senão retorna false.
    - `ResetRunes()`: `_currentRunes = 0`, atualizar UI
  - Em `Awake()`: singleton pattern

- [x] **Passo 15:** Criar `Assets/Scripts/Dungeon/RunePickup.cs`
  - Arquivo: `Assets/Scripts/Dungeon/RunePickup.cs`
  - O que fazer: MonoBehaviour.
  - Campos:
    - `_runeValue` (int, 1) — valor da runa
  - Método público `Initialize(int value)`: seta `_runeValue`
  - `OnTriggerEnter2D`: se layer é "player", chamar `RunCurrency.Instance.AddRunes(_runeValue)`, destruir este objeto
  - EnemyController instancia RunePickup ao morrer no `DieSequence()`: valor proporcional à dificuldade (ex: 1 para inimigo normal, 3 para elite, 10 para boss)

- [x] **Passo 16:** Criar `Assets/Scripts/Dungeon/RunUpgradeSO.cs`
  - Arquivo: `Assets/Scripts/Dungeon/RunUpgradeSO.cs`
  - O que fazer: ScriptableObject com `[CreateAssetMenu(menuName = "Dungeon/Run Upgrade")]`.
  - Campos públicos:
    - `upgradeName` (string)
    - `description` (string)
    - `cost` (int) — custo em runas
    - `upgradeType` (enum: Damage, Speed, MaxHealth, CritChance)
    - `value` (float) — multiplicador ou valor adicional
    - `hasTradeOff` (bool) — se true, tem efeito negativo
    - `tradeOffType` (enum: Damage, Speed, MaxHealth, CritChance) — stat negativo
    - `tradeOffValue` (float) — valor do efeito negativo
  - Exemplo: "Fúria Berserker" — +50% dano (value=1.5), -25% vida (tradeOffType=MaxHealth, tradeOffValue=0.75), custo 10 runas

- [x] **Passo 17:** Criar `Assets/Scripts/Dungeon/RunUpgradeManager.cs`
  - Arquivo: `Assets/Scripts/Dungeon/RunUpgradeManager.cs`
  - O que fazer: MonoBehaviour singleton (pattern igual a `DialogueUI` com `DontDestroyOnLoad`).
  - Campos:
    - `_playerHealth` (PlayerHealth)
    - `_playerController` (PlayerController)
    - `_appliedUpgrades` (List<RunUpgradeSO>) — upgrades ativos na run
  - Propriedades públicas: `static Instance`
  - Métodos públicos:
    - `ApplyUpgrade(RunUpgradeSO upgrade)`: aplica upgrade ao player
      - Se upgradeType == Damage: armazenar multiplicador, aplicar em WeaponController
      - Se upgradeType == Speed: `_playerController._moveSpeed *= value` (usar reflection ou campo público)
      - Se upgradeType == MaxHealth: ajustar `_playerHealth._maxHealth`
      - Se upgradeType == CritChance: armazenar (implementação futura)
      - Se hasTradeOff: aplicar efeito negativo (ex: `tradeOffValue=0.75` em MaxHealth reduz vida)
    - `ResetUpgrades()`: resetar todos os stats do player para base, limpar lista
    - `GetDamageMultiplier()`: retorna multiplicador total de dano dos upgrades ativos
  - Em `Awake()`: singleton pattern

- [x] **Passo 18:** Criar `Assets/Scripts/Dungeon/HealingShrine.cs`
  - Arquivo: `Assets/Scripts/Dungeon/HealingShrine.cs`
  - O que fazer: MonoBehaviour.
  - Campos:
    - `_healPercent` (float, 0.25) — cura 25% da vida máxima
    - `_dialogueText` (string, "Você sente sua força retornar...")
    - `_isUsed` (bool)
  - `OnTriggerEnter2D`: se layer é "player" e `!_isUsed`, setar `_isUsed = true`, chamar `PlayerHealth.HealPercent(_healPercent)`, mostrar diálogo via `DialogueUI.Instance.Show(_dialogueText)`, mudar cor do SpriteRenderer para indicar uso

- [x] **Passo 18b:** Criar `Assets/Scripts/Dungeon/RiskRewardAltar.cs`
  - Arquivo: `Assets/Scripts/Dungeon/RiskRewardAltar.cs`
  - O que fazer: MonoBehaviour.
  - Campos:
    - `_damageCost` (int, 25) — dano que o player sofre ao aceitar
    - `_runeReward` (int, 20) — runas que o player recebe
    - `_dialogueText` (string, "Aceitar dano em troca de runas? Pressione E")
    - `_interactAction` (InputAction) — tecla E para interagir
    - `_playerInRange` (bool)
    - `_isUsed` (bool)
  - Padrão de interação igual a `NPCInteractable.cs`: detectar player no trigger, tecla E, aplicar dano e dar runas
  - `OnTriggerEnter2D`: se layer "player", setar `_playerInRange = true`, mostrar diálogo
  - `OnTriggerExit2D`: esconder diálogo
  - Ao pressionar E: se `_playerInRange && !_isUsed`, aplicar `_damageCost` via `IDamageable`, chamar `RunCurrency.Instance.AddRunes(_runeReward)`, setar `_isUsed = true`

### Fase E — UI

- [x] **Passo 19:** Criar `Assets/Scripts/UI/FloorUI.cs`
  - Arquivo: `Assets/Scripts/UI/FloorUI.cs`
  - O que fazer: MonoBehaviour. Segue padrão de `DialogueUI.cs`.
  - Campos:
    - `_floorText` (TextMeshProUGUI)
    - `_runeText` (TextMeshProUGUI)
  - Métodos públicos:
    - `UpdateFloor(int floorNumber)` → `_floorText.text = "Floor " + floorNumber;`
    - `UpdateRunes(int runeCount)` → `_runeText.text = "Runas: " + runeCount;`
    - `Show()` / `Hide()` — para mostrar/esconder o texto (ex: esconder no Hub)

### Fase F — Integração e Prefabs

- [x] **Passo 20:** Criar prefabs na Unity
  - Criar `Assets/Prefabs/DungeonRoom.prefab` — sala padrão com RoomController e RoomTrigger
  - Criar `Assets/Prefabs/ExitRoom.prefab` — igual DungeonRoom + FloorTransition no ExitTrigger
  - Criar `Assets/Prefabs/BossArena_5.prefab` — arena de boss do andar 5 (BossFloorHandler, RoomController)
  - Criar `Assets/Prefabs/BossArena_10.prefab` — arena de boss do andar 10
  - Criar `Assets/Prefabs/BossArena_15.prefab` — arena de boss do andar 15
  - Criar `Assets/Prefabs/RewardRoom.prefab` — sala segura com RewardArea
  - Criar `Assets/Prefabs/TrapSpike.prefab` — trap de espinho com TrapBase.cs
  - Criar `Assets/Prefabs/RunePickup.prefab` — pickup de runa com RunePickup.cs
  - Criar `Assets/Prefabs/HealingShrine.prefab` — altar de cura com HealingShrine.cs
  - Criar `Assets/Prefabs/RiskRewardAltar.prefab` — altar de risco/recompensa com RiskRewardAltar.cs

- [x] **Passo 21:** Criar ScriptableObjects de Upgrade
  - Criar pasta `Assets/Data/Upgrades/`
  - Criar `UpgradeDamage.asset` — tipo: Damage, valor: 1.2, custo: 15 runas
  - Criar `UpgradeSpeed.asset` — tipo: Speed, valor: 1.15, custo: 10 runas
  - Criar `UpgradeHealth.asset` — tipo: MaxHealth, valor: 1.25, custo: 20 runas
  - Criar `UpgradeBerserker.asset` — tipo: Damage valor: 1.5, custo: 10 runas, hasTradeOff: true, tradeOffType: MaxHealth, tradeOffValue: 0.75

- [x] **Passo 22:** Configurar cena Main
  - Criar GameObjects FloorManager, DungeonGenerator, RunCurrency, RunUpgradeManager na cena
  - Criar UI Canvas com FloorUI (FloorText + RuneText)
  - Configurar todas as referências no Inspector
  - Remover Room estática da cena
  - Ajustar posição do Player

- [x] **Passo 23:** Criar ScriptableObject FloorConfig
  - Criar `Assets/Data/DefaultFloorConfig.asset`
  - Configurar valores iniciais (ver Passo 2)

### Fase G — Testes e Validação

- [x] **Passo 24:** Testar geração de andar 1
  - Play Mode → salas geram → player spawna na start room → FloorUI mostra "Floor 1"

- [x] **Passo 25:** Testar tipos de sala
  - Verificar que sala Combat spawna inimigos normais
  - Verificar que sala Elite spawna inimigo elite (amarelo, maior)
  - Verificar que sala Reward não spawna inimigos
  - Verificar que sala Rest não spawna inimigos

- [x] **Passo 26:** Testar combate em sala procedural
  - Entrar em sala → inimigos spawneam → matar → portas abrem

- [x] **Passo 27:** Testar economia de runas
  - Matar inimigo → runa dropa → coletar → Runas: X na UI
  - Morrer → runas resetam para 0

- [x] **Passo 28:** Testar upgrades de run
  - Coletar runas suficientes → comprar upgrade (ex: +dano)
  - Verificar que dano aumentou
  - Morrer → upgrades resetam, stats voltam ao normal

- [x] **Passo 29:** Testar trade-offs
  - Comprar upgrade com trade-off (ex: Berserker: +50% dano, -25% vida)
  - Verificar que ambos os efeitos aplicaram

- [x] **Passo 30:** Testar traps interativas
  - Trap causa dano no player ao contato
  - Levar inimigo para cima da trap → inimigo toma dano

- [x] **Passo 31:** Testar healing shrine
  - Interagir com shrine → vida parcial restaurada → shrine muda de cor

- [x] **Passo 31b:** Testar risk/reward altar
  - Entrar em sala Event → altar aparece → pressionar E → sofre dano, ganha runas

- [x] **Passo 32:** Testar progressão de andar
  - Chegar na exit room → andar 2 gera → layout diferente → FloorUI atualiza

- [x] **Passo 33:** Testar cura entre andares
  - Terminar andar com vida parcialmente curada (auto-cura ou shrine)

- [x] **Passo 34:** Testar andar de boss (andar 5)
  - Avançar até andar 5 → BossArena_5 aparece → boss spawna

- [x] **Passo 35:** Testar cura pós-boss e reward
  - Matar boss → player recupera vida → transição para RewardRoom

- [x] **Passo 36:** Testar seed de geração
  - Mesmo seed → mesmo layout
  - Seed 0 → layout aleatório a cada run

- [x] **Passo 37:** Testar morte e reset
  - Morrer → volta para Hub → FloorManager, RunCurrency, RunUpgradeManager destruídos → voltar à dungeon → andar 1, runas 0, sem upgrades

---

## Decisões Resolvidas

Todas as 7 perguntas foram respondidas pelo usuário:

| # | Pergunta | Decisão |
|---|----------|---------|
| 1 | Tamanho da sala | Default 20x12 |
| 2 | Enemy prefab nos spawn points | Setar via code (DungeonGenerator) |
| 3 | Portas físicas vs teleport | Door.cs com collider físico |
| 4 | Boss prefab | Criar boss separado (3 prefabs distintos) |
| 5 | Conexão visual entre salas | Chão/paredes contínuos (player anda fisicamente) |
| 6 | FloorManager ao morrer | Destruir FloorManager, voltar ao andar 1 |
| 7 | UI do andar | Texto na tela com TextMeshProUGUI |

---

## Pontos que Devem Ser Considerados — Por Fase

### Fase 7 (AGORA) — Implementar

| Item | Script/Component | Descrição |
|------|-----------------|-----------|
| Tamanho de andar aumenta com progresso | `FloorConfigSO._roomWidthScalingPerFloor` + `DungeonGenerator.GetRoomWidth(floor)` | Andares mais altos = salas maiores e mais difíceis |
| 3 bosses com controller próprio | 3 prefabs separados (BossArena_5/10/15) | Placeholder por enquanto — move sets distintos futuro |
| Traps que causam dano (player + inimigos) | `TrapBase.cs` + `TrapSpike.prefab` | Interativo: jogador pode usar traps contra inimigos. Tipos: Spike, Blade, Fire, Poison |
| Variação de tipos de sala | `RoomType` enum | Combat, Elite, Reward, Rest, Event, Shop, Boss |
| Cura ao matar boss | `BossFloorHandler` | Chama `PlayerHealth.HealToFull()` após boss morrer |
| UI de andar + runas | `FloorUI.cs` | TextMeshProUGUI: "Floor X" no topo, "Runas: Y" no canto |
| Sistema de seeds | `DungeonGenerator` com `System.Random` | Seed reproduzível para debug e balanceamento |
| Inimigos elite | `EnemyController._isElite` + `RoomController` | Sprite amarelo, escala 1.3x, stats escalados, dropam mais runas |
| Economia de runas (drop, perda ao morrer) | `RunCurrency.cs` + `RunePickup.cs` | Monstros dropam runas ao morrer (1 normal, 3 elite, 10 boss). Perde tudo ao morrer |
| Upgrades temporários de run | `RunUpgradeManager.cs` + `RunUpgradeSO.cs` | Comprar com runas: +dano, +velocidade, +vida, +crítico. Resetam ao morrer |
| Trade-offs em upgrades | `RunUpgradeSO.hasTradeOff` | Ex: +50% dano, -25% vida. Dois efeitos aplicados simultaneamente |
| Salas de risco/recompensa | `RoomType.Event` + lógica | Trigger de evento: aceitar dano em troca de runas ou upgrade |
| Traps interativas (contra inimigos) | `TrapBase.cs` | Verifica layer "player" E "enemy" — traps causam dano em ambos |
| Recuperação entre andares | `HealingShrine.cs` + `FloorManager` | Auto-cura parcial ao avançar andar + shrines em salas de recompensa |
| Morte → Hub → andar 1 | `GameManager.OnPlayerDeath()` | Destrói FloorManager, RunCurrency, RunUpgradeManager. Recomeça do zero |

### Fase 8+ (FUTURO) — Documentar, Não Implementar

| Item | Fase Sugerida | Nota |
|------|---------------|------|
| Builds com sinergias (armas, upgrades, relíquias) | 8+10 | Weapon system já existe; upgrades e relíquias são futuras |
| Combinações de inimigos por andar | 9 | EnemySpawnConfig por floor range |
| Spawn contextual por bioma/andar | 9 | Bioma SO com enemy pools |
| Recursos limitados (cura escassa, consumíveis) | 10 | Item system com pickups |
| Salas secretas/ocultas | 7+ | Chance baixa no Random Walk, parede falsa — adicionar quando sistema estabilizar |
| Meta-progressão (desbloqueio de armas, NPCs) | 11+ | SaveData com flags de desbloqueio. Ex: zerar libera lança |
| Recompensas de boss (arma, relíquia) | 8+10 | BossFloorHandler com reward pool específico por boss |
| Eventos aleatórios (altares, maldições, NPCs) | 7+ | RoomType.Event com EventSO — expandir quando base estiver pronta |
| Modificadores de run ("inimigos mais rápidos") | 10+ | RunModifier SO aplicado no início da run |
| Pool de recompensas por raridade | 10 | RewardPool SO com weights. Cada vez que zera libera nova dificuldade |
| Pool separado (arma, passivo, cura, recurso) | 10 | RewardCategory enum + pools por categoria |
| NPCs/eventos dentro da dungeon | 7+ | RoomType.Event com NPC dungeon — expandir quando base estiver pronta |
| Atalhos/desbloqueios permanentes | 15+ | Meta-progression flags |
| Duração de run balanceada | tuning | Ajustar roomCount e dificuldade durante testes |
| Progressão de dificuldade por completar run | 10+ | Cada vez que o jogador chega ao fim, libera nova dificuldade

---

## Validação

- [x] Scripts compilam sem erro no Unity (verificar Console)
- [x] Setup no Inspector está documentado acima
- [x] Fluxo pode ser testado em Play Mode: andar 1 → combate → exit → andar 2
- [x] `SETUP.md` será atualizado com seção 13 (Dungeon Procedural)
- [x] Boss floor (andar 5) funciona corretamente
- [x] Reward area funciona após boss
- [x] Player recupera vida ao matar boss
- [x] Morte reseta floor para 1 e destrói FloorManager, RunCurrency, RunUpgradeManager
- [x] Difficulty scaling aplicado nos inimigos
- [x] Não há NullReference durante geração
- [x] Player reposiciona corretamente entre andares
- [x] FloorUI mostra andar atual e runas na tela
- [x] Inimigo elite aparece em salas Elite (amarelo, maior)
- [x] Trap causa dano no player E nos inimigos
- [x] Seed reproduzível: mesmo seed = mesmo layout
- [x] Andares mais altos têm salas maiores
- [x] Salas Reward/Rest/Event/Shop não spawna inimigos
- [x] Runas dropam ao matar inimigos e são coletadas pelo player
- [x] Runas resetam ao morrer
- [x] Upgrades de run aplicam stats corretamente
- [x] Trade-offs aplicam efeito positivo E negativo
- [x] HealingShrine cura parcialmente e muda de cor após uso
