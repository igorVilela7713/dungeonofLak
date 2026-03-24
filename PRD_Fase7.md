## Objetivo
Implementar a estrutura principal da dungeon com geração procedural de andares, progressão entre floors, salas especiais (boss e reward) e base para scaling de dificuldade — conforme descrito na Fase 7 do plano. O sistema deve permitir que o jogador navegue por 15 andares, com bosses nos andares 5, 10 e 15, áreas de recompensa pós-boss, e dificuldade crescente.

## Arquivos Relevantes
| Arquivo | Relevância | Motivo |
|---------|------------|--------|
| Assets/Scripts/Rooms/RoomController.cs | alta | Sistema existente de salas com spawn, portas e rastreamento de inimigos — base para o gerador |
| Assets/Scripts/Rooms/Door.cs | alta | Controle de portas (open/close via collider) — reutilizado nas salas geradas |
| Assets/Scripts/Rooms/EnemyDeathTracker.cs | alta | Notifica RoomController quando inimigos morrem — integração direta |
| Assets/Scripts/Core/GameManager.cs | alta | Gerencia morte do player e reload de cena — precisa integrar com FloorManager |
| Assets/Scripts/Core/SceneTransition.cs | média | Transição entre cenas (Hub → Dungeon) — pode precisar adaptar para transição entre andares |
| Assets/Scripts/Enemies/EnemyController.cs | alta | Inimigo com IDamageable, Initialize(Transform player) — precisa receber scaling de dificuldade |
| Assets/Scripts/Combat/PlayerHealth.cs | alta | Sistema de vida do player — usado para reset entre andares |
| Assets/Scripts/Movement/PlayerController.cs | média | Player side-scroller com física — posição inicial nas salas geradas |
| Assets/Scripts/Camera/CameraFollow.cs | baixa | Câmera segue player — pode precisar limites por sala |
| Assets/Scripts/Core/SaveData.cs | média | Estrutura de save — pode precisar adicionar currentFloor |
| Assets/Scripts/Core/SaveSystem.cs | média | Sistema de save JSON — integração com progressão |
| Assets/Scripts/Core/HubManager.cs | baixa | Hub — pode precisar iniciar dungeon no andar salvo |

## Assets / Prefabs / Scenes Relevantes
| Caminho | Tipo | Motivo |
|--------|------|--------|
| Assets/Scenes/Main.unity | Scene | Cena principal da dungeon — onde FloorManager e DungeonGenerator operarão |
| Assets/Scenes/Hub.unity | Scene | Hub — ponto de partida para entrar na dungeon |
| Assets/Prefabs/Room.prefab | Prefab | Sala existente — base para salas procedurais |
| Assets/Prefabs/Enemy.prefab | Prefab | Inimigo padrão — spawnado nas salas geradas |
| Assets/Prefabs/Player.prefab | Prefab | Player — precisa ser posicionado nas salas geradas |
| Assets/Prefabs/Door.prefab | Prefab | Porta — conexão entre salas procedurais |
| Assets/Prefabs/Ground.prefab | Prefab | Chão — usado no layout das salas |
| Assets/Prefabs/Wall.prefab | Prefab | Parede — usado no layout das salas |
| Assets/Tiles/Main Palete.prefab | Tile Palette | Palette de tiles do SunnyLand — para Tilemap das salas |
| Assets/SunnyLand Artwork/Environment/Tileset/ | Tile Assets | Tiles disponíveis para construção de salas |
| Assets/Data/ | Pasta | Pasta vazia — destino para ScriptableObjects de dificuldade/config |

## Padrões Encontrados no Projeto

### Convenções de código já estabelecidas
- `[SerializeField] private` para campos expostos no Inspector
- `[Header("...")]` para agrupar seções de campos
- `[RequireComponent]` quando dependência é obrigatória
- Método `Initialize(params)` para setup em runtime (EnemyController, SwordHitbox, EnemyDeathTracker)
- Interface `IDamageable` com `void TakeDamage(int amount)`
- `UnityEvent` para callbacks (PlayerHealth.OnDeath)
- `InputAction` criado em `Awake()` com bindings via código
- Corrotinas com `System.Collections.IEnumerator`
- Sem namespaces
- Allman-style braces, 4-space indent

### Padrão RoomController (referência principal)
```csharp
// RoomController.cs — padrão de sala existente
// - OnTriggerEnter2D detecta entrada do player (layer "player")
// - SpawnEnemies() instancia inimigos nos spawnPoints
// - EnemyDeathTracker.AddComponent notifica quando inimigo morre
// - Portas fecham quando player entra no centro
// - Portas abrem quando _enemiesAlive <= 0
```

### Padrão de inicialização de inimigos
```csharp
// EnemyController.Initialize(Transform player) — chamado por RoomController.SpawnEnemies()
// EnemyDeathTracker.Initialize(RoomController room) — adicionado via AddComponent
```

### Padrão de detecção de layer
```csharp
// Projeto usa: other.gameObject.layer != LayerMask.NameToLayer("player")
// Para filtrar colisões do player
```

## Documentação Externa
- **Procedural Dungeon com Prefabs**: Abordagem de grid simples — criar sala prefab base, instanciar em posições do grid, conectar via portas. Referência: https://www.ryadel.com/en/how-to-create-a-2d-roguelike-in-unity-part-3/
- **Unity BoardManager**: Tutorial oficial usa `Count(min, max)` para densidade e `LayoutObjectAtRandom` para colocar objetos. Referência: https://www.oreateai.com/blog/unpacking-the-unity-2d-roguelike-tutorial-from-prefabs-to-procedural-generation/
- **Difficulty Scaling por andar**: Usar multiplicadores por floor (ex: `1 + floor * 0.1f` para HP). Referência: https://github.com/Chizaruu/Unity-RL-Tutorial/tree/part-12-increasing-difficulty
- **Random Walk para dungeon**: Algoritmo simples de grid — mover em direção aleatória, marcar células como salas. Compatível com side-scroller.
- **Unity Tilemap**: `Tilemap.SetTile()` para pintar tiles programaticamente. TilePalette já existe no projeto.

## Componentes Necessários

### Scripts (novos)
1. **`FloorManager.cs`** — Controla andar atual, tipo de andar (Normal/Boss/Reward), progressão entre floors. Singleton ou referenciado pelo GameManager.
2. **`DungeonGenerator.cs`** — Gera layout de salas para andares normais. Usa Random Walk em grid. Instancia Room prefabs nas posições calculadas.
3. **`DifficultyScaler.cs`** — Fornece multiplicadores de HP e dano baseados no andar atual. Consultado por EnemyController e RoomController.
4. **`BossFloorHandler.cs`** — Detecta boss floor, carrega arena pré-fabricada, gerencia fluxo pós-boss → reward.
5. **`RewardArea.cs`** — Área segura pós-boss sem combate. Placeholder para upgrade futuro.

### Scripts (modificações)
6. **`EnemyController.cs`** — Adicionar método `ApplyDifficultyMultiplier(float hpMult, float dmgMult)` ou receber scalers no `Initialize()`.
7. **`RoomController.cs`** — Adicionar suporte para configuração externa de enemyPrefab e contagem por floor.
8. **`GameManager.cs`** — Integrar com FloorManager para reset/progressão.
9. **`SaveData.cs`** — Adicionar campo `currentFloor`.

### Prefabs (novos/modificados)
10. **`DungeonRoom.prefab`** — Sala base para geração procedural (com spawn points configuráveis, portas, trigger zone).
11. **`BossArena.prefab`** — Arena de boss pré-fabricada (andares 5, 10, 15).
12. **`RewardRoom.prefab`** — Sala de recompensa pós-boss.

### ScriptableObjects (novos)
13. **`FloorConfig.asset`** — Configuração por andar: enemyCount, difficultyMultiplier, roomCount, enemy types.

### Layers/Tags
- Reutilizar layer "player" existente
- Possível layer "room_trigger" para detecção de entrada

## Fluxo Esperado

### Fluxo Normal (andares 1-4, 6-9, 11-14)
1. FloorManager indica andar atual (ex: andar 3)
2. DungeonGenerator gera layout de salas conectadas em grid
3. Sala "start" é posicionada com player spawn point
4. Salas "combat" são posicionadas com spawn points de inimigos
5. Sala "exit" é posicionada com porta para próximo andar
6. Player entra em sala → RoomController ativa (padrão existente)
7. Player mata todos inimigos → portas abrem
8. Player chega na sala "exit" → FloorManager avança para andar+1
9. Limpa objetos do andar atual, gera próximo andar

### Fluxo Boss (andares 5, 10, 15)
1. FloorManager detecta tipo Boss
2. BossFloorHandler carrega BossArena (arena pré-fabricada, não procedural)
3. Boss é spawnado (EnemyController com stats escalados)
4. Player derrota boss
5. BossFloorHandler transiciona para RewardArea

### Fluxo Reward (após boss)
1. FloorManager indica tipo Reward
2. RewardArea é carregada (sala segura, sem inimigos)
3. Placeholder para upgrade/futuro
4. Player avança para próximo andar (andar+1)

### Fluxo de Morte
1. Player morre → GameManager.OnPlayerDeath()
2. Volta para Hub (cena atual)
3. FloorManager reseta andar para 1

### Fluxo de Dificuldade
1. DifficultyScaler consulta andar atual
2. Retorna multiplicadores: `hpMult = 1 + (floor * 0.1f)`, `dmgMult = 1 + (floor * 0.08f)`
3. EnemyController aplica multiplicadores ao spawnar

## Constraints
- **Simplicidade**: Não criar algoritmos complexos (sem BSP, sem Cellular Automata). Usar Random Walk em grid simples.
- **Prefabs de sala**: Salas são prefabs pré-fabricados (não gerar tilemap por tile programaticamente). Instanciar e conectar logicamente.
- **Side-scroller**: O jogo usa física 2D com gravidade e jumping (não top-down). Salas devem suportar plataforma.
- **Reutilizar RoomController**: Sistema existente de salas (trigger → spawn → portas) deve ser reutilizado.
- **Sem overengineering**: Não criar sistema de graph/A*. Grid simples com posições (x,y) e conexão lógica via portas.
- **Escopo incremental**: Fase 1: FloorManager + geração simples. Fase 2: Boss floors. Fase 3: Reward. Fase 4: Difficulty scaling.
- **Tilemap**: O projeto já tem tilemap assets (SunnyLand). Gerar chão/paredes via prefab, não via Tilemap.SetTile().
- **Cena única**: Toda a dungeon opera na mesma cena (Main.unity). Geração é runtime, não scene loading por andar.
- **Player persistente**: Player mantém stats entre andares (HP, etc.). Apenas level/rooms mudam.

## Riscos / Pontos de Atenção

### Riscos de Integração
1. **Player positioning**: DungeonGenerator precisa reposicionar player na sala "start" ao gerar novo andar. Usar `PlayerController.transform.position = startPosition`.
2. **Referências quebradas**: Ao destruir/instanciar salas, referências de spawn points, doors, player no RoomController podem quebrar. DungeonGenerator deve configurar RoomControllers após instanciar.
3. **EnemyController.Initialize()**: EnemyController já usa `Initialize(Transform player)`. RoomController chama isso ao spawnar. DungeonGenerator não deve duplicar essa lógica — deixar RoomController fazer.
4. **Camera limits**: CameraFollow segue player livremente. Com salas lado a lado, câmera pode mostrar sala errada. Considerar limitar câmera por sala ou usar transição suave.

### Riscos de Design
5. **Conexão entre salas**: Em side-scroller, "conexão" entre salas pode ser física (porta com collider) ou lógica (teleport). Porta existente (`Door.cs`) usa collider — pode funcionar como passagem física.
6. **Grid vs. layout físico**: Posições do grid (0,0), (1,0) etc. precisam ser convertidas para posições Unity (world space). Definir tamanho de sala (ex: 20x12 units) e usar offset.
7. **Overlap de salas**: Random Walk pode gerar salas sobrepostas se não verificar posições já ocupadas. Usar `HashSet<Vector2Int>` para posições usadas.
8. **Boss arena setup**: Arena de boss precisa ser pré-fabricada com spawn point de boss, limites visuais, e trigger de vitória. Criar como prefab separado.

### Riscos Técnicos
9. **Performance**: Instanciar/destruir muitos objetos por andar pode causar GC spikes. Considerar object pooling para inimigos (futuro) ou yield entre rooms.
10. **Save/load**: Se adicionar currentFloor ao save, precisa garantir que estado da dungeon possa ser restaurado corretamente (salas, inimigos, etc.). Para escopo inicial, não salvar estado da dungeon — apenas andar.
11. **Morte durante geração**: Se player morrer enquanto DungeonGenerator está gerando, pode causar null refs. Verificar null antes de acessar player references.

## Decisões a Tomar

1. **Tamanho da sala**: Qual o tamanho em world units de cada sala prefab? (Sugestão: 20x12 baseado no tilemap SunnyLand)
2. **Quantidade de salas por andar**: Fixo ou variável? (Sugestão: 3-7 salas por andar normal, aumentando com dificuldade)
3. **Geração por andar vs. acumulativa**: Gerar andar completo ao avançar (simples) vs. gerar sala por sala (exploratório)? (Sugestão: gerar andar completo)
4. **Transição entre andares**: Destruir tudo e recriar (simples) vs. transição suave com fade? (Sugestão: destruir e recriar para escopo inicial)
5. **Boss types**: Um único boss prefab com stats escalados vs. boss diferente por andar? (Sugestão: um único boss com stats escalados para fase inicial)
6. **Conexão física entre salas**: Portas como colliders que abrem/fecham (padrão existente) vs. trigger de teleporte? (Sugestão: reusar Door.cs com colliders)
7. **Enemy types por andar**: Mesmo EnemyPrefab para todos os andares (apenas stats diferentes) vs. tipos diferentes? (Sugestão: mesmo prefab, stats diferentes para fase inicial)
8. **FloorConfig via ScriptableObject ou hardcoded**: ScriptableObject permite balanceamento pelo Inspector. (Sugestão: ScriptableObject simples)
9. **Onde colocar FloorManager**: DontDestroyOnLoad (persiste entre andares) vs. recriar por cena? (Sugestão: DontDestroyOnLoad com singleton, resetar na morte)
10. **Grid de geração**: Quantas posições no grid? (Sugestão: 5x5 grid, 12-20 células ocupadas via Random Walk)
