# PRD — FASE 9: Inimigos Avançados e Ecossistema de Combate

## Objetivo

Expandir o sistema de inimigos do jogo, que hoje possui apenas um tipo (EnemyController melee chaser), para incluir variedade real de combate com arquétipos distintos (Fast, Heavy, Ranged), inimigos Elite com comportamento diferenciado, e bosses com identidade própria nos andares 5, 10 e 15. O objetivo é que cada tipo de inimigo exija uma resposta diferente do jogador, criando combate não-repetitivo e decisões táticas durante a run.

---

## Arquivos Relevantes

| Arquivo | Relevância | Motivo |
|---------|------------|--------|
| `Assets/Scripts/Enemies/EnemyController.cs` | **alta** | Único script de inimigo atual — monolítico (movimento, vida, dano, morte). Precisa ser refatorado ou estendido para suportar múltiplos arquétipos |
| `Assets/Scripts/Combat/IDamageable.cs` | **alta** | Interface usada por todos os inimigos e player para receber dano |
| `Assets/Scripts/Combat/WeaponController.cs` | **média** | Sistema de armas do player — os novos inimigos devem interagir corretamente com knockback e dano das armas |
| `Assets/Scripts/Combat/SwordHitbox.cs` | **média** | Hitbox de ataque do player — precisa continuar funcionando com novos tipos de inimigo |
| `Assets/Scripts/Rooms/RoomController.cs` | **alta** | Controla spawn de inimigos por sala — atualmente só suporta 1 prefab de inimigo normal e 1 de elite. Precisa suportar múltiplos tipos |
| `Assets/Scripts/Rooms/EnemyDeathTracker.cs` | **média** | Componente adicionado dinamicamente para rastrear mortes — precisa funcionar com novos tipos |
| `Assets/Scripts/Dungeon/DungeonGenerator.cs` | **alta** | Gera andares e salas — precisa expor referências para múltiplos prefabs de inimigo e distribuir tipos por sala |
| `Assets/Scripts/Dungeon/BossFloorHandler.cs` | **alta** | Spawna boss atualmente como EnemyController com multiplicadores — precisa spawnar bosses reais com controller próprio |
| `Assets/Scripts/Dungeon/DifficultyScaler.cs` | **média** | Escala HP/dano por andar — precisa funcionar com novos arquétipos (scaling pode diferir por tipo) |
| `Assets/Scripts/Dungeon/FloorConfigSO.cs` | **média** | ScriptableObject de configuração de andar — pode precisar de campos para controle de spawn de tipos |
| `Assets/Scripts/Dungeon/FloorManager.cs` | **baixa** | Gerencia progressão de andares — bosses se integram aqui via BossFloorHandler |
| `Assets/Scripts/Dungeon/RoomType.cs` | **baixa** | Enum de tipos de sala — Elite já existe como tipo |
| `Assets/Scripts/Movement/PlayerController.cs` | **média** | Player precisa reagir a novos tipos de ataque (projéteis, knockback de heavy) |
| `Assets/Scripts/Combat/PlayerHealth.cs` | **média** | Sistema de vida do player — dano de novos inimigos se conecta aqui |
| `Assets/Scripts/Dungeon/RunUpgradeManager.cs` | **baixa** | Multiplicador de dano do player — bosses devem ser balanceados considerando isso |

---

## Assets / Prefabs / Scenes Relevantes

| Caminho | Tipo | Motivo |
|---------|------|--------|
| `Assets/Prefabs/Enemy.prefab` | Prefab | Inimigo base atual — referência para criar novos prefabs |
| `Assets/Prefabs/EliteEnemy.prefab` | Prefab | Elite atual — será refeito para usar novo sistema |
| `Assets/Prefabs/BossArena_5.prefab` | Prefab | Arena do Boss 1 — precisa receber o script de boss correto |
| `Assets/Prefabs/BossArena_10.prefab` | Prefab | Arena do Boss 2 |
| `Assets/Prefabs/BossArena_15.prefab` | Prefab | Arena do Boss 3 |
| `Assets/Prefabs/DungeonRoom.prefab` | Prefab | Sala de dungeon — RoomController precisa spawnar tipos variados |
| `Assets/Prefabs/SwordHitbox.prefab` | Prefab | Hitbox do player — deve continuar funcionando com novos inimigos |
| `Assets/Scenes/Main.unity` | Scene | Cena principal — DungeonGenerator e referências de prefab são configuradas aqui |
| `Assets/Data/DefaultFloorConfig.asset` | ScriptableObject | Config de andar — pode precisar de novos campos |

---

## Padrões Encontrados no Projeto

### 1. EnemyController atual — monolítico
O `EnemyController.cs` (180 lines) é o único script de inimigo. Ele concentra:
- Movimento (chase + jump)
- HP e dano
- Detecção de chão
- Aplicação de dano ao player via `OnTriggerEnter2D`
- Knockback no player
- Drop de runas
- Flag `_isElite` (só muda cor, escala e valor de runas — sem mudança de comportamento)
- Sequência de morte com coroutine

Não há separação entre lógica de movimento, combate e comportamento. O padrão de IA é apenas "chase horizontal + jump se player estiver acima".

### 2. RoomController — spawn de inimigos
`RoomController.SpawnCombatEnemies()` instancia `_enemyPrefab` em cada `_spawnPoints` e adiciona `EnemyDeathTracker` dinamicamente. Suporta elite via `_eliteEnemyPrefab` separado mas sem comportamento diferente.

```csharp
private void SpawnCombatEnemies()
{
    foreach (Transform point in _spawnPoints)
    {
        GameObject enemy = Instantiate(_enemyPrefab, point.position, Quaternion.identity);
        var controller = enemy.GetComponent<EnemyController>();
        if (controller != null)
        {
            controller.Initialize(_player);
            // ApplyDifficulty...
        }
        var tracker = enemy.AddComponent<EnemyDeathTracker>();
        tracker.Initialize(this);
        _enemiesAlive++;
    }
}
```

### 3. DungeonGenerator — referências de prefab
`DungeonGenerator` tem campos separados para `_enemyPrefab` e `_eliteEnemyPrefab`. Será necessário adicionar campos para novos tipos ou criar um sistema de seleção.

### 4. BossFloorHandler — boss como EnemyController com multiplicadores
Atualmente spawna o `_bossPrefab` (que é um `EnemyController` normal) e aplica multiplicadores hardcoded (3x HP, 2x DMG, 5x runes). Não há comportamento de boss real.

```csharp
ec.ApplyDifficulty(hpMult * 3f, dmgMult * 2f, runeMult * 5f);
```

### 5. DifficultyScaler — estático e genérico
Classe estática com multiplicadores lineares por andar. Todos os tipos recebem os mesmos multiplicadores via `ApplyDifficulty`.

### 6. IDamageable — interface mínima
```csharp
public interface IDamageable
{
    void TakeDamage(int amount);
}
```

Usada por `PlayerHealth`, `EnemyController` e `TrapBase`.

---

## Documentação Externa

### Enemy AI Patterns em Unity 2D
- **State Machine básico**: Idle → Chase → Attack → Retreat é o padrão mais comum para roguelikes 2D. Pode ser implementado com enum + switch no Update, sem precisar de classes separadas de estado.
- **Ranged enemies**: Precisam de um sistema de projétil simples (GameObject com Rigidbody2D, Collider2D trigger, script de projétil que aplica dano via `IDamageable`).
- **Boss patterns**: Unity geralmente usa coroutines para sequências de ataque com pausas entre padrões.

### Referências
- Unity 2D Roguelike tutorial (Enemy AI state pattern)
- Brackeys-style enemy AI com chase/detection range
- Projéteis em Unity 2D: `Rigidbody2D` com `linearVelocity` setada uma vez, `OnTriggerEnter2D` para hit detection

### Boas práticas para o contexto deste projeto
- Manter tudo em scripts simples (1 script por comportamento principal)
- Usar `[Header]` e `[SerializeField]` para exposição no Inspector
- Evitar herança complexa — composição com componentes separados
- Prefabs configurados no Inspector com referências já setadas

---

## Componentes Necessários

### Scripts novos
- **`EnemyType.cs`** — Enum: `Melee`, `Fast`, `Heavy`, `Ranged`, `Boss1`, `Boss2`, `Boss3`
- **`EnemyProjectile.cs`** — Script para projétil do inimigo ranged (movimento em linha reta, aplica dano via `IDamageable`, se destroi após hit ou timeout)
- **`BossMeleeController.cs`** — Boss 1: perseguição agressiva, ataques rápidos em sequência
- **`BossAreaController.cs`** — Boss 2: ataques em área, controle de espaço, zonas de perigo
- **`BossHybridController.cs`** — Boss 3: mistura melee + ranged, múltiplos padrões

### Scripts modificados
- **`EnemyController.cs`** — Adicionar suporte a `EnemyType` com comportamentos variados (velocidade, distância de ataque, padrão de movimento, retreat para ranged). Manter como script único com lógica condicional por tipo, não criar subclasses.
- **`RoomController.cs`** — Suportar spawn de tipos variados de inimigo (lista de EnemyType com probabilidades)
- **`DungeonGenerator.cs`** — Expor referências para prefabs de cada tipo de inimigo e distribuir por sala/floor
- **`DifficultyScaler.cs`** — Suportar multiplicadores diferentes por tipo de inimigo
- **`BossFloorHandler.cs`** — Spawnar o boss correto com base no andar, usando o script de boss apropriado

### Prefabs novos
- **`EnemyFast.prefab`** — Inimigo rápido (baseado em Enemy.prefab, com EnemyType.Fast)
- **`EnemyHeavy.prefab`** — Inimigo pesado (baseado em Enemy.prefab, com EnemyType.Heavy)
- **`EnemyRanged.prefab`** — Inimigo ranged (baseado em Enemy.prefab, com EnemyType.Ranged)
- **`Boss1.prefab`** — Boss melee agressivo
- **`Boss2.prefab`** — Boss de controle de área
- **`Boss3.prefab`** — Boss híbrido
- **`EnemyProjectile.prefab`** — Projétil de inimigo ranged (SpriteRenderer + Rigidbody2D + Collider2D trigger + EnemyProjectile script)

### Prefabs modificados
- **`EliteEnemy.prefab`** — Atualizar para usar novo sistema de elite com comportamento diferenciado

### ScriptableObjects (opcionais)
- **`EnemyArchetypeSO`** — ScriptableObject com stats base de cada arquétipo (HP, speed, damage, attack range, behavior parameters). Alternativa: manter tudo no Inspector do prefab para simplicidade.

---

## Fluxo Esperado

### Fluxo de spawn de inimigos normais
1. `DungeonGenerator.GenerateFloor()` gera salas com `RoomController`
2. `RoomController` decide quais tipos de inimigo spawnar com base no andar e tipo de sala
3. Cada inimigo é instanciado com o prefab correto (`EnemyFast`, `EnemyHeavy`, `EnemyRanged` ou `Enemy` base)
4. `EnemyController.Initialize()` recebe referência do player
5. `EnemyController.ApplyDifficulty()` aplica escalonamento do `DifficultyScaler`
6. Inimigos executam seu comportamento conforme `EnemyType`
7. Ao morrer, `EnemyDeathTracker.OnDestroy()` notifica `RoomController.OnEnemyKilled()`

### Fluxo de inimigos Elite
1. Salas `RoomType.Elite` ou salas normais com chance de elite
2. Spawn de inimigo com `EnemyType` normal mas flag `_isElite = true`
3. Elite recebe multiplicadores maiores (HP x2.5, Dano x1.5), tamanho aumentado, cor alterada
4. Elites de ranged podem ter projéteis mais rápidos; elites de heavy podem ter knockback maior

### Fluxo de boss
1. `FloorManager` detecta boss floor (5, 10, 15)
2. `DungeonGenerator.GenerateBossFloor()` instancia arena correspondente
3. `BossFloorHandler` instancia o prefab de boss correto (não mais um EnemyController genérico)
4. Boss executa padrões de ataque via coroutines (múltiplos ataques com pausas)
5. Ao morrer, `BossFloorHandler.OnBossDefeated()` cura player e avança para reward room

### Comportamento por arquétipo

**Melee (base)**: Comportamento atual — chase horizontal, jump se player acima, dano por contato.

**Fast**: Velocidade alta (~3x base), HP baixo (~0.5x), dano médio-baixo. Chase agressivo, sem pausas. Pressão constante.

**Heavy**: Velocidade baixa (~0.5x), HP alto (~3x), dano alto, knockback forte. Chase lento, pausas entre movimentos. Pode ter "charge" (movimento rápido em linha reta por curta distância).

**Ranged**: Velocidade média-baixa, HP médio-baixo. Mantém distância do player (~4-6 unidades). Atira projéteis a intervalos. Se player se aproxima muito, faz retreat rápido.

**Boss 1 (Melee Agressivo)**: Chase constante, ataques rápidos em sequência (2-3 hits com pausa curta). Pode ter ataque especial em área quando HP baixo.

**Boss 2 (Area Control)**: Não chase diretamente. Cria zonas de perigo no chão (áreas com dano over time ou projéteis que caem). Posiciona-se estrategicamente. Alternar entre ativo e passivo.

**Boss 3 (Hybrid)**: Alterna entre melee e ranged. Fase 1: melee com charges. Fase 2 (50% HP): ranged com projéteis múltiplos. Pode ter transição visual.

---

## Constraints

- **Simplicidade primeiro**: Manter `EnemyController` como script único com lógica condicional por `EnemyType`, não criar hierarquia de classes. Só bosses ganham scripts próprios.
- **Sem overengineering**: Não criar sistema genérico de AI com state machines abstratos. Usar enum + switch simples.
- **Placeholders**: Todos os inimigos usam sprites/cores placeholder. Não depender de arte final.
- **Consistência com projeto**: Seguir padrões de `[SerializeField]`, `[Header]`, Allman braces, sem namespaces.
- **Integração real**: Todos os prefabs devem funcionar com `RoomController`, `EnemyDeathTracker`, `DifficultyScaler` existentes.
- **Performance**: Projéteis devem se auto-destruir após timeout (não acumular objetos). Não usar `FindObjectOfType` em Update.
- **Balanceamento**: Valores iniciais são aproximados. O importante é a sensação diferente, não números perfeitos.
- **Unity 2D**: Tudo usa Rigidbody2D, Collider2D, SpriteRenderer. Projéteis usam Rigidbody2D com velocidade linear constante.

---

## Riscos / Pontos de Atenção

### Riscos de integração
1. **EnemyDeathTracker adicionado dinamicamente**: `RoomController` usa `enemy.AddComponent<EnemyDeathTracker>()` — funciona com qualquer MonoBehaviour que seja destruído. Sem risco aqui.
2. **BossFloorHandler spawna EnemyController genérico**: Ao trocar para scripts de boss específicos, o `EnemyDeathTracker` e `ApplyDifficulty` precisam ser compatíveis. Solução: bosses podem herdar de `EnemyController` ou implementar `IDamageable` diretamente com `EnemyDeathTracker` separado.
3. **DungeonGenerator tem referências hardcoded**: Adicionar novos campos de prefab (`_enemyFastPrefab`, etc.) requer configurar no Inspector da cena Main.unity.
4. **Projéteis e layers**: Projéteis de inimigo precisam estar em layer correta para não colidir com outros inimigos. Verificar configuração de layers no projeto.
5. **RoomController._enemyPrefab único**: Atualmente cada sala tem 1 tipo de inimigo. Para misturar tipos na mesma sala, precisar de lógica adicional no spawn.

### Edge cases
- Inimigo ranged com projétil que não acerta nada (timeout no projétil)
- Boss morrendo durante animação de ataque (parar coroutines)
- Elite ranged que fica preso em loop de retreat se player não se aproxima
- Heavy enemy que não consegue alcançar player em plataforma alta (precisa de jump ou pathfinding simples)
- Múltiplos projéteis na tela ao mesmo tempo (performance)

### Problemas comuns
- Projétil passando through de paredes (precisa de layer collision matrix correto)
- Boss com HP muito alto ficando tedioso (balancear com dano das armas do player)
- Inimigos se empilhando uns nos outros (ignorar colisão entre inimigos no Rigidbody2D)

---

## Decisões a Tomar

1. **Herança vs composição para arquétipos**: Criar subclasses de `EnemyController` (`EnemyFast`, `EnemyHeavy`) ou manter tudo em `EnemyController` com switch por `EnemyType`? **Recomendação**: Manter em `EnemyController` único para simplicidade. Só bosses ganham scripts próprios por terem padrões complexos demais.

2. **ScriptableObject para arquétipos ou Inspector?** Usar `EnemyArchetypeSO` para centralizar stats de cada tipo, ou configurar direto nos prefabs? **Recomendação**: Configurar nos prefabs via Inspector para simplicidade. ScriptableObject só se for reutilizar os mesmos stats em muitos prefabs.

3. **Sistema de projétil genérico ou específico?** Criar um `Projectile.cs` genérico ou `EnemyProjectile.cs` específico? **Recomendação**: `EnemyProjectile.cs` específico por enquanto. Generalizar depois se player também tiver projéteis.

4. **Composição de salas com tipos mistos**: Cada sala spawna apenas 1 tipo de inimigo, ou pode misturar? **Recomendação**: Começar com 1 tipo por sala para simplificar. Misturar depois via lista de possibilidades com pesos.

5. **Boss como EnemyController estendido ou script separado?** Bosses podem herdar de `EnemyController` (reutilizando HP, TakeDamage, morte) e adicionar lógica de padrões, ou ser scripts completamente separados que implementam `IDamageable`. **Recomendação**: Scripts separados (`BossMeleeController`, etc.) que implementam `IDamageable` — bosses têm lógica complexa demais para caber em switch/case.

6. **Elite como comportamento diferente ou só stats?** O plano diz que elite deve ter "ataque especial / maior agressividade". Isso significa comportamento diferente de verdade (ex: ranged elite atira 2 projéteis) ou basta stats maiores? **Recomendação**: Comportamento ligeiramente diferente por tipo. Ranged elite atira mais rápido, heavy elite tem knockback maior, fast elite tem speed ainda maior.

7. **Scaling diferenciado por tipo?** `DifficultyScaler` aplica mesmo multiplicador para todos. Deveria ter multiplicadores diferentes por tipo (ex: heavy escala mais em HP, fast escala mais em speed)? **Recomendação**: Multiplicadores base do DifficultyScaler + fator fixo por tipo aplicado no prefab/EnemyController. Não mudar DifficultyScaler agora.
