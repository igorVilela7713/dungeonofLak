# Testes Automatizados

## Visão Geral

Suite de testes para os sistemas centrais do roguelike 2D, construída com **Unity Test Framework (NUnit)**.

- **105 testes** distribuídos em 11 arquivos
- **10 Edit Mode** + **1 Play Mode**
- Zero código de produção modificado

---

## Estrutura

```
Assets/Tests/
├── EditMode/
│   ├── EditModeTests.asmdef
│   ├── DifficultyScalerTests.cs      (16)
│   ├── FloorConfigSO_Tests.cs        (15)
│   ├── FloorManagerTests.cs          (16)
│   ├── DungeonGeneratorTests.cs      (13)
│   ├── EnemyBaseTests.cs             (14)
│   ├── EnemyControllerTests.cs       (4)
│   ├── PlayerHealthTests.cs          (13)
│   ├── WeaponControllerTests.cs      (6)
│   ├── RunUpgradeManagerTests.cs     (8)
│   └── RunCurrencyTests.cs           (9)
└── PlayMode/
    ├── PlayModeTests.asmdef
    └── IntegrationTests.cs
```

---

## Cobertura por Sistema

### DifficultyScaler

| Teste | Descrição |
|-------|-----------|
| GetHPMultiplier_Floor1 | Retorna valor base (1.0) |
| GetHPMultiplier_Floor0 | Retorna abaixo do base |
| GetHPMultiplier_Floor50 | Alto andar, valor alto |
| GetHPMultiplier_Parameterized | [TestCase] com múltiplos andares |
| GetDamageMultiplier_Parameterized | Escala de dano por andar |
| GetRuneMultiplier_Parameterized | Escala de runas por andar |
| AllMultipliers_ProgressiveGrowth | Crescimento monótono |
| AllMultipliers_UseSameFormula | Consistência entre fórmulas |

### FloorConfigSO

| Teste | Descrição |
|-------|-----------|
| IsBossFloor_Floor5/10/15 | Andares de boss identificados |
| IsBossFloor_Parameterized | Todos os andares 1-20 |
| GetRoomCount_Floor1 | Contagem base |
| GetRoomCount_HighFloor | Clamped ao máximo (8) |
| GetRoomWidth | Retorna largura base |

### FloorManager

| Teste | Descrição |
|-------|-----------|
| CurrentFloor_InitialState | Começa em 1 |
| NextFloor_NormalFloor | Incrementa para 2 |
| NextFloor_CalledMultipleTimes | Incrementa corretamente |
| ResetFloor_AfterProgress | Reseta para 1 |
| GetCurrentFloorType_BossFloor | FloorType.Boss no andar 5 |
| NextFloor_OnBossFloor_SetsRewardFlag | Boss → Reward |
| NextFloor_FromRewardFloor | Reward → próximo andar |
| NextFloor_FiresOnFloorChanged | Evento disparado |

### DungeonGenerator (via reflexão)

| Teste | Descrição |
|-------|-----------|
| GetEnemyPrefabsForFloor_Floor1-2 | Apenas inimigos básicos |
| GetEnemyPrefabsForFloor_Floor3-4 | + inimigos rápidos |
| GetEnemyPrefabsForFloor_Floor5-6 | + inimigos pesados |
| GetEnemyPrefabsForFloor_Floor7+ | + ranged |
| GetEnemyPrefabsForFloor_NoNullPrefabs | Nenhum prefab nulo |
| GetEnemyPrefabsForFloor_NeverShrinks | Pool cresce monotonicamente |

### EnemyBase

| Teste | Descrição |
|-------|-----------|
| TakeDamage_ReducesHealth | HP diminui corretamente |
| TakeDamage_WhenHealthReachesZero | Marca como morto |
| TakeDamage_WhenAlreadyDead | Não reduz mais HP |
| ApplyDifficulty_ScalesHealth | Escala de vida |
| ApplyDifficulty_ScalesDamage | Escala de dano |
| ApplyDifficulty_Parameterized | Múltiplos cenários |
| RuneValue_NormalEnemy | Valor base |
| TakeDamage_GradualDamage | Morte no limiar correto |

### EnemyController

| Teste | Descrição |
|-------|-----------|
| TakeDamage_ReducesHealth | Herdado de EnemyBase |
| TakeDamage_FatalDamage | Morte ao zerar HP |
| ApplyDifficulty_ScalesHealthAndDamage | Escala correta |
| Initialize_SetsPlayerReference | Referência ao jogador |

### PlayerHealth

| Teste | Descrição |
|-------|-----------|
| MaxHealth_Default | Padrão é 100 |
| TakeDamage_ReducesHealth | Redução correta |
| TakeDamage_Overkill | HP clampado a 0 |
| HealToFull_AfterDamage | Restaura vida |
| HealPercent_DoesNotExceedMax | Não excede máximo |
| SetMaxHealth_WhenCurrentExceeds | Clampa HP atual |
| TakeDamage_WhenHealthReachesZero | Dispara OnDeath |

### WeaponController

| Teste | Descrição |
|-------|-----------|
| EquipWeapon_SetsEquippedWeapon | Equipa arma |
| EquipWeapon_SwitchesWeapon | Troca de arma |
| EquipWeapon_NullWeapon | Não troca se null |
| OnHitboxTrigger_DealsCorrectDamage | Dano correto aplicado |
| OnHitboxTrigger_DifferentWeapons | Dano varia por arma |

### RunUpgradeManager

| Teste | Descrição |
|-------|-----------|
| GetDamageMultiplier_Initial | Começa em 1.0 |
| ApplyUpgrade_Damage | Multiplica corretamente |
| ApplyUpgrade_MultipleDamageUpgrades | Stack multiplicativo |
| ApplyUpgrade_WithTradeOff | Aplica bônus e penalidade |
| ResetUpgrades_ResetsDamageMultiplier | Reseta para 1.0 |

### RunCurrency

| Teste | Descrição |
|-------|-----------|
| CurrentRunes_Initial | Começa em 0 |
| AddRunes_IncreasesBalance | Adiciona corretamente |
| SpendRunes_Sufficient | Retorna true, diminui |
| SpendRunes_Insufficient | Retorna false, sem alteração |
| ResetRunes_SetsBalanceToZero | Reseta saldo |

### PlayMode (IntegrationTests)

| Teste | Descrição |
|-------|-----------|
| TakeDamage_DeathCoroutine | Destrói objeto após morte |
| TakeDamage_MultipleHitsOverFrames | HP diminui entre frames |
| TakeDamage_DeathEvent_FiresInNextFrame | OnDeath em frame seguinte |
| ApplyUpgrade_WithCost_SpendsRunes | Gasta runas ao aplicar upgrade |
| ApplyUpgrade_InsufficientRunes | Falha com runas insuficientes |
| ResetUpgrades_AfterApply | Restaura padrões |

---

## Técnicas Utilizadas

| Técnica | Uso |
|---------|-----|
| `[Test]` | Testes unitários básicos |
| `[TestCase]` | Parâmetros inline |
| `[SetUp] / [TearDown]` | Preparação e limpeza |
| `[UnityTest]` | Testes com `yield return` (PlayMode) |
| **Reflexão** | Acesso a métodos/campos privados |
| **Subclasses de teste** | Evitar RequireComponent e física |
| **Mocks** | `MockDamageable`, `MockPlayerController` |
| **ScriptableObject.CreateInstance** | Criar SOs sem assets |

---

## O que NÃO é testado

- Animações e SpriteRenderer
- UI visual
- Física complexa (colisões detalhadas)
- Camera behavior
- VFX / partículas
- Cenas reais

---

## Como Rodar

### Via Unity Editor

1. Abra o projeto no Unity
2. Navegue até **Window > General > Test Runner**
3. Selecione a aba:
   - **EditMode** — testes rápidos de lógica
   - **PlayMode** — testes com runtime
4. Clique em **Run All** para executar todos
5. Clique em um teste específico para rodar individualmente

### Via Linha de Comando (CI)

```bash
"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" ^
  -runTests ^
  -projectPath "C:\Users\igor7\My project" ^
  -testResults results.xml ^
  -testPlatform EditMode
```

Plataformas disponíveis: `EditMode`, `PlayMode`

### Resultados

O arquivo `results.xml` é gerado no formato NUnit XML, compatível com GitHub Actions e outros CI.

---

## Prompt de Atualização por Fase

Ao final de cada fase de desenvolvimento, copie e cole o prompt abaixo. Ele instrui a geração de novos testes e atualização automática da documentação.

```
Estou finalizando uma fase de desenvolvimento do meu jogo roguelike 2D em Unity (C#).

## O que foi feito nesta fase:
[Descreva aqui os sistemas criados, modificados ou refatorados nesta fase]
- Ex: "Criei o sistema de StatusEffect com Poison, Burn e Freeze"
- Ex: "Refatorei o EnemyController para usar State Machine"
- Ex: "Adicionei o InventoryManager com slots e itens"

## O que precisa ser feito:

### 1. Analisar o código novo/modificado
- Ler todos os arquivos .cs criados ou modificados nesta fase
- Identificar métodos públicos, propriedades e lógica testável
- Identificar dependências (MonoBehaviour, ScriptableObject, singletons)
- Mapear quais métodos são puros lógica vs quais dependem de Unity runtime

### 2. Criar testes Edit Mode
Para cada sistema novo ou modificado:
- Criar arquivo em `Assets/Tests/EditMode/NomeDoSistemaTests.cs`
- Usar a estrutura AAA (Arrange / Act / Assert)
- Usar `[Test]` e `[TestCase]` quando aplicável
- Usar `ScriptableObject.CreateInstance<T>()` para SOs
- Usar reflexão para campos/métodos privados quando necessário
- Criar subclasses de teste para evitar RequireComponent
- Criar mocks quando o sistema depende de interfaces externas
- Incluir `[SetUp]` e `[TearDown]` com limpeza de singletons
- Edge cases: valores zero, negativos, limites, null

### 3. Criar testes Play Mode (apenas se necessário)
- Apenas quando o teste depende de corrotinas, física ou frame timing
- Usar `[UnityTest]` com `yield return`
- Criar em `Assets/Tests/PlayMode/NomeDoSistemaPlayModeTests.cs`
- Adicionar ao `IntegrationTests.cs` se forem poucos

### 4. Atualizar TESTS.md
- Adicionar seção "### NomeDoSistema" em "Cobertura por Sistema"
- Listar cada novo teste com descrição na tabela
- Atualizar contagem total de testes no topo
- Atualizar estrutura de pastas se novos arquivos foram criados
- Atualizar "Técnicas Utilizadas" se novas técnicas foram usadas

### 5. Validar
- Verificar que todos os testes compilam (sem erros de referência)
- Verificar que o asmdef referencia `Assembly-CSharp`
- Verificar que singletons são resetados em SetUp/TearDown
- Verificar que objetos criados são destruídos em TearDown
- Confirmar que nenhum código de produção foi modificado sem necessidade

## Regras:
- NÃO modificar código de produção sem necessidade absoluta
- SEMPRE destruir objetos em TearDown (Object.DestroyImmediate)
- SEMPRE resetar singletons estáticos em TearDown
- PREFERIR Edit Mode sobre Play Mode
- PREFERIR lógica pura sobre dependências de Unity
- MANTER consistência com o estilo dos testes existentes
```
