# PRD — Fase 8: Sistema de Armas e Identidade de Combate

## Objetivo

Implementar um sistema de armas modular para o roguelike 2D, onde cada arma (Sword, Spear, Axe, Dagger) possui dados próprios via ScriptableObject, visual distinto, e troca funcional em runtime. O sistema deve substituir os valores hardcoded no `WeaponController` atual por um fluxo data-driven, preparando terreno para pickups, upgrades e reward rooms, sem criar abstrações prematuras.

---

## Arquivos Relevantes

| Arquivo | Relevância | Motivo |
|---------|------------|--------|
| `Assets/Scripts/Combat/WeaponController.cs` | **alta** | Controller principal de ataque — precisa ser refatorado para ler stats de ScriptableObject ao invés de campos hardcoded |
| `Assets/Scripts/Combat/SwordHitbox.cs` | **alta** | Hitbox genérica que pode ser reutilizada por todas as armas |
| `Assets/Scripts/Combat/IDamageable.cs` | **média** | Interface de dano — não precisa mudar, mas é dependência direta |
| `Assets/Scripts/Combat/PlayerHealth.cs` | **média** | Vida do player — integração indireta via knockback/dano recebido |
| `Assets/Scripts/Movement/PlayerController.cs` | **alta** | Contém `_facingDirection` usado pelo sistema de ataque; precisa expor direção para o WeaponController direcionar hitbox |
| `Assets/Scripts/Enemies/EnemyController.cs` | **média** | Recebe dano — deve continuar funcionando sem alteração |
| `Assets/Scripts/Dungeon/RunUpgradeManager.cs` | **média** | Já tem `DamageMultiplier` — precisa integrar com o novo sistema de armas |
| `Assets/Scripts/Dungeon/RunUpgradeSO.cs` | **baixa** | Padrão de ScriptableObject usado no projeto — referência para `WeaponDataSO` |
| `Assets/Scripts/Dungeon/FloorConfigSO.cs` | **baixa** | Outro padrão de ScriptableObject — referência de estrutura |
| `Assets/Scripts/Core/SaveData.cs` | **baixa** | Pode precisar adicionar campo de arma desbloqueada no futuro |
| `Assets/Scripts/Dungeon/RunePickup.cs` | **média** | Padrão de pickup existente — referência para `WeaponPickup` |
| `Assets/Scripts/Rooms/RoomController.cs` | **baixa** | Não deve ser alterado, mas armas podem spawnar em salas de reward |

---

## Assets / Prefabs / Scenes Relevantes

| Caminho | Tipo | Motivo |
|---------|------|--------|
| `Assets/Prefabs/SwordHitbox.prefab` | Prefab | Hitbox existente — será a base para todas as hitboxes de arma |
| `Assets/Prefabs/Player.prefab` | Prefab | Player — precisa receber referência ao `WeaponVisualController` e ao novo `WeaponController` refatorado |
| `Assets/Scripts/Dungeon/FloorConfigSO.cs` | ScriptableObject | Padrão de SO — modelo para `WeaponDataSO` |
| `Assets/Data/Upgrades/` | Pasta de SOs | Padrão de ScriptableObjects de upgrade — modelo para assets de armas |
| `Assets/Data/` | Pasta | Local destino para `WeaponDataSO` assets (ex: `Assets/Data/Weapons/`) |
| `Assets/Prefabs/Room.prefab` | Prefab | Salas onde pickups de arma podem ser spawnados |
| `Assets/Prefabs/RewardRoom.prefab` | Prefab | Reward room — destino natural para pickups de arma |

---

## Padrões Encontrados no Projeto

### 1. ScriptableObject para dados (padrão RunUpgradeSO)
```csharp
// Assets/Scripts/Dungeon/RunUpgradeSO.cs
[CreateAssetMenu(menuName = "Dungeon/Run Upgrade")]
public class RunUpgradeSO : ScriptableObject
{
    public string upgradeName;
    public int cost;
    public UpgradeType upgradeType;
    public float value;
    // ...
}
```
**Aplicação**: `WeaponDataSO` deve seguir o mesmo padrão — `[CreateAssetMenu]`, campos públicos simples, um asset por arma.

### 2. WeaponController atual (hardcoded)
```csharp
// Assets/Scripts/Combat/WeaponController.cs:7-11
[SerializeField] private int _damage = 10;
[SerializeField] private float _attackCooldown = 0.3f;
[SerializeField] private float _attackDuration = 0.1f;
[SerializeField] private float _knockbackForce = 2f;
```
**Problema**: Valores fixos no Inspector. Não suporta troca de arma em runtime. Cada instância do prefab teria os mesmos valores.

### 3. Hitbox spawn — posição fixa
```csharp
// Assets/Scripts/Combat/WeaponController.cs:83
Vector3 spawnPos = _player.position + _player.right * 0.8f;
```
**Problema**: Sempre spawna na direção `_player.right`. Não considera `_facingDirection` do `PlayerController` (que pode ser Vector2.left). Offset hardcoded (`0.8f`) — cada arma precisa de alcance diferente.

### 4. Input System — padrão code-based
```csharp
// Assets/Scripts/Combat/WeaponController.cs:23-26
_attackAction = new InputAction("Attack", InputActionType.Button);
_attackAction.AddBinding("<Mouse>/leftButton");
_attackAction.AddBinding("<Keyboard>/enter");
_attackAction.AddBinding("<Gamepad>/buttonWest");
```
**Aplicação**: O input de ataque deve permanecer no `WeaponController`. A troca de arma pode usar o mesmo padrão (ex: tecla Q para swap).

### 5. Initialize pattern para runtime setup
```csharp
// Assets/Scripts/Combat/SwordHitbox.cs:8
public void Initialize(WeaponController controller)
```
**Aplicação**: `WeaponPickup` e `WeaponVisualController` devem usar `Initialize()` para receber referências.

### 6. Singleton para managers
```csharp
// FloorManager, RunCurrency, RunUpgradeManager, DialogueUI
public static ClassName Instance { get; private set; }
```
**Decisão**: O sistema de armas NÃO deve ser singleton. Cada arma é um componente no Player, não um manager global.

---

## Documentação Externa

- **ScriptableObject-based weapon/gun pattern**: Padrão data-driven para armas em Unity. Separar dados (SO) de lógica (component). Fonte: https://github.com/llamacademy/scriptable-object-based-guns
- **Unity ScriptableObject para config**: `[CreateAssetMenu]` permite criar assets via menu do Unity sem código extra. Documentação oficial: https://docs.unity3d.com/ScriptReference/CreateAssetMenuAttribute.html
- **Weapon swap em runtime**: Abordagem comum — manter referência ao SO equipado, ler stats quando necessário, trocar visual via child GameObjects ou SpriteRenderer separado.
- **Roguelike weapon identity (Hades, Dead Cells, Enter the Gungeon)**: Cada arma muda não só números mas também padrão de hitbox, timing e feedback visual. A identidade vem da combinação de dano + alcance + cooldown + knockback + padrão de ataque.

---

## Componentes Necessários

### Scripts novos
- **`WeaponDataSO.cs`** — ScriptableObject com todos os dados de uma arma (dano, cooldown, alcance, knockback, padrão de ataque, sprite placeholder)
- **`WeaponType.cs`** — Enum: `Sword`, `Spear`, `Axe`, `Dagger`
- **`AttackPattern.cs`** — Enum: `HorizontalSwing`, `ForwardThrust`, `OverheadSmash`, `QuickStab` (define como a hitbox se comporta)
- **`WeaponPickup.cs`** — MonoBehaviour para coletar arma no chão (seguir padrão de `RunePickup`)
- **`WeaponVisualController.cs`** — Controla qual child visual está ativo conforme arma equipada

### Scripts a refatorar
- **`WeaponController.cs`** — Ler stats de `WeaponDataSO` ao invés de campos hardcoded; usar `PlayerController.FacingDirection` para direcionar hitbox; suportar troca de arma via `EquipWeapon(WeaponDataSO)`

### ScriptableObjects (assets)
- **`WeaponSword.asset`** — Dados da espada (arma padrão)
- **`WeaponSpear.asset`** — Dados da lança
- **`WeaponAxe.asset`** — Dados do machado
- **`WeaponDagger.asset`** — Dados da adaga

### Prefabs
- Atualizar **`Player.prefab`** com nova hierarquia de armas visuais (child GameObjects para cada arma placeholder)
- Criar **`WeaponPickup.prefab`** com collider trigger + WeaponPickup.cs

### Modificações existentes
- **`RunUpgradeManager.cs`** — `DamageMultiplier` deve ser consultado pelo `WeaponController` ao calcular dano final
- **`SaveData.cs`** — Possivelmente adicionar campo de arma desbloqueada (futuro, não obrigatório agora)

---

## Fluxo Esperado

### Fluxo de combate com sistema de armas
1. Player inicia com **Sword** equipada (default via `[SerializeField]` no Inspector)
2. `WeaponController` lê stats do `WeaponDataSO` equipado (dano, cooldown, alcance, knockback, attackPattern)
3. Ao pressionar ataque, `WeaponController` calcula posição da hitbox baseado em:
   - `_facingDirection` do `PlayerController`
   - `_attackRange` do `WeaponDataSO`
   - `_attackPattern` do `WeaponDataSO` (define offset e tamanho da hitbox)
4. Hitbox é spawnada, detecta colisão, aplica dano + knockback (com `RunUpgradeManager.DamageMultiplier` aplicado)
5. Após `_attackDuration`, hitbox é destruída, cooldown começa

### Fluxo de troca de arma
1. Player coleta `WeaponPickup` (trigger collision)
2. `WeaponPickup` chama `WeaponController.EquipWeapon(WeaponDataSO)`
3. `WeaponController` atualiza referência ao SO
4. `WeaponVisualController` desativa visual antigo, ativa visual novo
5. Próximo ataque usa novos stats

### Fluxo de visual
1. Player tem child objects: `Visual_Sword`, `Visual_Spear`, `Visual_Axe`, `Visual_Dagger`
2. Apenas um está ativo por vez
3. `WeaponVisualController.EquipVisual(WeaponType)` ativa o correto
4. Inicialmente são sprites placeholder (pode ser sprite vazio colorido, ou SpriteRenderer com cor)

---

## Constraints

- **Simplicidade**: Não criar sistemas genéricos abstratos. Uma classe concreta `WeaponDataSO` com campos simples. Uma classe `WeaponController` que lê do SO. Sem interfaces de arma, sem factory, sem strategy pattern.
- **Unity 2D**: Projeto é side-scroller 2D com gravidade. Armas são hitboxes temporárias, não projéteis.
- **Pixel art**: Visual será placeholder por enquanto. O sistema deve funcionar com sprites simples coloridos.
- **Sem sistemas genéricos**: Não criar um "IWeapon" ou "WeaponBase" abstrato. Cada arma é apenas um `WeaponDataSO` com diferentes valores. O `WeaponController` é único.
- **Integração real**: Cada arma precisa ser configurável no Inspector via `WeaponDataSO`. Troca visual precisa funcionar com hierarquia de GameObjects filhos no Player.
- **Input**: Code-based `InputAction` (padrão do projeto). Sem asset de Input Actions.
- **No namespaces**: Convenção do projeto não usar namespaces.
- **Allman braces, 4-space indent, `[SerializeField] private`**: Seguir estilo existente.

---

## Riscos / Pontos de Atenção

### Risco 1: Hitbox spawn position com _player.right
O `WeaponController` atual usa `_player.right` para posicionar hitbox. O `PlayerController` usa `_facingDirection` (Vector2 com -1 ou 1 no X). Se o player estiver virado para a esquerda, `_player.right` ainda aponta para direita (é o eixo X positivo do transform). **Solução**: Usar `_facingDirection` do `PlayerController` para posicionar hitbox corretamente, não `_player.right`.

### Risco 2: RunUpgradeManager.DamageMultiplier não integrado
O `RunUpgradeManager` já calcula um `DamageMultiplier`, mas o `WeaponController` atual lê `_damage` diretamente do Inspector. Se não integrarmos, upgrades de dano não afetarão as armas. **Solução**: `WeaponController.OnHitboxTrigger` deve consultar `RunUpgradeManager.Instance.GetDamageMultiplier()` ao calcular dano final (se a instância existir).

### Risco 3: Troca de arma durante ataque
Se o player trocar de arma enquanto o ataque está em andamento (hitbox ativa), a hitbox antiga pode aplicar dano com stats errados ou ser destruída abruptamente. **Solução**: Bloquear troca de arma enquanto `_isAttacking == true`, ou destruir hitbox ativa antes de trocar.

### Risco 4: SwordHitbox prefab — tamanho fixo
O `SwordHitbox.prefab` tem um tamanho de collider fixo. Armas com alcance diferente (spear > sword > dagger) precisam de hitboxes com tamanhos diferentes. **Solução**: No `SpawnHitbox()`, ajustar `localScale` da hitbox baseado no `_attackRange` do SO, OU criar hitboxes com tamanho base 1x1 e escalar via código.

### Risco 5: Player sempre deve ter arma
O player nunca deve ficar sem arma equipada. Se o `WeaponDataSO` default não estiver atribuído no Inspector, o ataque pode quebrar. **Solução**: Verificação em `Awake()` — se `_equippedWeapon == null`, logar erro e não permitir ataque.

### Risco 6: DontDestroyOnLoad e referências
`FloorManager`, `RunCurrency`, `RunUpgradeManager` usam `DontDestroyOnLoad`. O `WeaponController` está no Player que pode ser recriado entre cenas. Referências a SOs são seguras (assets são sempre válidos), mas referências a componentes podem quebrar. **Solução**: Usar `FindFirstObjectByType` ou tag para reencontrar referências se necessário.

### Risco 7: Setup na Unity
- Os assets de `WeaponDataSO` precisam ser criados manualmente via `Create > Dungeon > Weapon Data`
- O prefab do Player precisa ser atualizado com a hierarquia de visuals
- Os campos do `WeaponController` no Inspector precisam ser reconfigurados (remover campos antigos, adicionar referência ao SO)
- Testar em Play Mode após cada arma adicionada

---

## Decisões a Tomar

### D1: Onde armazenar a arma equipada?
**Opção A**: Campo `[SerializeField]` no `WeaponController` — simples, configurável no Inspector. O player sempre inicia com a mesma arma. **Opção B**: Campo no `GameManager` ou singleton — permite persistir arma entre cenas. **Recomendação**: Opção A por simplicidade. O Player prefab já é DontDestroyOnLoad implícito via FloorManager pattern.

### D2: AttackPattern — como implementar?
**Opção A**: Enum que define apenas o offset de spawn da hitbox (HorizontalSwing = offset horizontal, ForwardThrust = offset mais à frente). Hitbox sempre é um retângulo. **Opção B**: Enum + cada padrão tem hitbox prefab próprio. **Recomendação**: Opção A por simplicidade. Um único `SwordHitbox` (renomeado para `WeaponHitbox`) com `localScale` ajustado via código.

### D3: WeaponPickup — como spawnar?
**Opção A**: Prefab configurável com campo `WeaponDataSO` no Inspector — cada pickup define qual arma entrega. Spawnado pelo `DungeonGenerator` ou `RoomController`. **Opção B**: Sistema de pool de armas desbloqueadas — o jogo escolhe aleatoriamente. **Recomendação**: Opção A. Controle manual no Inspector. Sem pool por enquanto.

### D4: Visual — hierarchy ou SpriteRenderer swap?
**Opção A**: Child GameObjects (`Visual_Sword`, etc.) — ativar/desativar. **Opção B**: SpriteRenderer no Player com sprite trocado via código. **Recomendação**: Opção A. Mais simples de configurar no Inspector, não precisa carregar sprites via Resources ou endereçáveis.

### D5: Armas desbloqueadas permanentemente — implementar agora?
**Opção A**: Sim — campo `List<WeaponType> unlockedWeapons` em algum lugar. **Opção B**: Não — todas as armas estão sempre disponíveis via pickups. **Recomendação**: Opção B por agora. Preparar campo, mas não implementar lógica de unlock. Deixar para Fase 11 (Meta Progression).

### D6: Renomear SwordHitbox para WeaponHitbox?
O `SwordHitbox` será usado por todas as armas, não só a espada. **Recomendação**: Sim, renomear para `WeaponHitbox` e atualizar referências no prefab e no `WeaponController`. Ou manter como `SwordHitbox` por simplicidade (só um nome) e adicionar comentário de que é genérico. Decisão do desenvolvedor.

### D7: Onde colocar o offset de ataque — no SO ou no AttackPattern?
**Opção A**: `_attackRange` no SO define distância do centro. `AttackPattern` define direção relativa. **Opção B**: Cada arma define offset completo (Vector2) no SO. **Recomendação**: Opção A. Mais limpo e permite ajuste independente de alcance vs. padrão.
