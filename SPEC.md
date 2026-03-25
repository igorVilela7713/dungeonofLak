# SPEC — Fase 8: Sistema de Armas

## Decisões tomadas

| # | Decisão | Escolha |
|---|---------|---------|
| D1 | Onde armazenar arma equipada | `[SerializeField]` no WeaponController — player prefab carrega default |
| D2 | AttackPattern implementation | Enum + ajustar `localScale` da hitbox via código (um único prefab) |
| D3 | WeaponPickup spawn | Prefab com campo `WeaponDataSO` no Inspector — controle manual |
| D4 | Sistema visual | Child GameObjects (`Visual_Sword`, etc.) — ativar/desativar |
| D5 | Unlock permanente | Não agora — todas as armas via pickups. Deixar para Fase 11 |
| D6 | Renomear SwordHitbox | Manter nome `SwordHitbox` agora — funciona para todas as armas, menos refactoring. Renomear para `WeaponHitbox` quando a fase estabilizar (anotado como TODO futuro). |
| D7 | Offset de ataque | `_attackRange` no SO + `AttackPattern` define direção relativa |
| D8 | Hitbox size base | Prefab ajustado para Size = 0.8. Campo `BaseHitboxSize = 0.8f` no WeaponController |
| D9 | Swap de arma | Implementar `availableWeapons` + `currentWeaponIndex` agora. Q cicla entre disponíveis |
| D10 | Visual placeholder | Child visual por arma (forma geométrica + cor). Não usar sprite de idle puro |
| D11 | DamageMultiplier | Global da run. Aplica no hit. Continua após troca de arma. Separar de weapon-specific no futuro |
| D12 | Cena de teste | Criar `WeaponTestScene.unity` dedicada |

---

## Arquivos a Criar

| Path | Tipo | Descrição |
|------|------|-----------|
| `Assets/Scripts/Combat/WeaponType.cs` | Enum | `Sword`, `Spear`, `Axe`, `Dagger` |
| `Assets/Scripts/Combat/AttackPattern.cs` | Enum | `HorizontalSwing`, `ForwardThrust`, `OverheadSmash`, `QuickStab` |
| `Assets/Scripts/Combat/WeaponDataSO.cs` | ScriptableObject | Dados de uma arma (dano, cooldown, alcance, knockback, pattern, visual) |
| `Assets/Scripts/Combat/WeaponVisualController.cs` | MonoBehaviour | Ativa/desativa child visual conforme arma equipada |
| `Assets/Scripts/Combat/WeaponPickup.cs` | MonoBehaviour | Trigger que equipa arma ao player |
| `Assets/Scenes/WeaponTestScene.unity` | Scene | Cena dedicada para testar sistema de armas |

---

## Arquivos a Modificar

| Path | Mudanças |
|------|----------|
| `Assets/Scripts/Combat/WeaponController.cs` | Substituir campos hardcoded por leitura de `WeaponDataSO`. Adicionar `EquipWeapon()`, `availableWeapons` list, `currentWeaponIndex`, `BaseHitboxSize`. Usar `PlayerController.FacingDirection`. Integrar `RunUpgradeManager.DamageMultiplier`. Input de swap (tecla Q) com lógica de ciclo real. |
| `Assets/Scripts/Dungeon/RunUpgradeManager.cs` | Nenhuma mudança estrutural — já funciona. DamageMultiplier é global da run e aplica no momento do hit. |

---

## Pré-condições

Antes de implementar, confirmar:

1. **`RunUpgradeManager.GetDamageMultiplier()`** existe como propriedade/método público e retorna `float`. Singleton acessível via `RunUpgradeManager.Instance`. (Confirmado no código atual: `RunUpgradeManager.cs:88-91`.)
2. **`PlayerController.FacingDirection`** existe como `Vector2` e nunca é zero — quando o player para, mantém a última direção válida. (Confirmado no código atual: `PlayerController.cs:21,28,90-93` — `_facingDirection` só atualiza quando `_horizontalInput != 0`, caso contrário retém valor anterior.)
   > **Nota de escopo:** Nesta fase, `FacingDirection` é sempre horizontal (esquerda/direita) — o player usa física 2D com gravidade, não top-down. `GetAttackOffset` usa apenas `facingDir.x`. Suporte completo a ataque vertical ou 8 direções fica para fase posterior.
3. **`PlayerController.ApplyKnockback(Vector2, float)`** existe e é usado pelo `EnemyController` para knockback do player. (Confirmado: `PlayerController.cs:96-99`, `EnemyController.cs:176`.)
4. **Cada arma base terá um único `WeaponDataSO`** — não criar dois assets diferentes para a mesma `WeaponType`. A lista `_availableWeapons` compara por referência do SO.
5. **`IDamageable.TakeDamage(int amount)`** é o contrato mínimo para receber dano da arma. Qualquer inimigo, boss, dummy ou objeto destrutível que implemente essa interface será acertado pelo `OnHitboxTrigger`. Manter a interface enxuta (apenas `TakeDamage`).

---

## Implementação passo a passo

### Passo 1 — Enums (`WeaponType.cs` e `AttackPattern.cs`)

**Arquivo:** `Assets/Scripts/Combat/WeaponType.cs`

```csharp
public enum WeaponType
{
    Sword,
    Spear,
    Axe,
    Dagger
}
```

**Arquivo:** `Assets/Scripts/Combat/AttackPattern.cs`

```csharp
public enum AttackPattern
{
    HorizontalSwing,
    ForwardThrust,
    OverheadSmash,
    QuickStab
}
```

São arquivos independentes, sem dependências. Criar primeiro.

> **Nota sobre escopo do AttackPattern:** O enum atual controla apenas offset e feeling inicial do ataque. Animação, arco real de swing e multi-stage attacks ficam para fase posterior — não tentar expandir o padrão nesta fase.

---

### Passo 2 — WeaponDataSO

**Arquivo:** `Assets/Scripts/Combat/WeaponDataSO.cs`

ScriptableObject que centraliza TODOS os dados de uma arma. Segue o padrão de `RunUpgradeSO`.

```csharp
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Weapon Data")]
public class WeaponDataSO : ScriptableObject
{
    [Header("Identity")]
    public WeaponType weaponType;
    public string weaponName;

    [Header("Stats")]
    public int damage = 10;
    public float attackCooldown = 0.3f;
    public float attackDuration = 0.1f;
    public float attackRange = 0.8f;
    public float knockbackForce = 2f;

    [Header("Attack Pattern")]
    public AttackPattern attackPattern = AttackPattern.HorizontalSwing;

    [Header("Visual")]
    public Color placeholderColor = Color.white;
}
```

**Campos e por quê:**
- `weaponType` — identificador para o `WeaponVisualController` saber qual child ativar
- `damage`, `attackCooldown`, `attackDuration`, `knockbackForce` — substituem os campos hardcoded do `WeaponController` atual
- `attackRange` — substitui o `0.8f` hardcoded na linha 83 do `WeaponController`. Spear > Sword > Dagger
- `attackPattern` — define como a hitbox é posicionada (veja Passo 3)
- `placeholderColor` — cor do visual placeholder da arma (child marker no player)

**Imports:** Apenas `UnityEngine`. Sem dependências.

> **Nota sobre serialização:** Campos `public` no SO é uma concessão de velocidade para protótipo. Em produção, preferir `[SerializeField] private` com propriedades read-only. Manter como está por enquanto.

---

### Passo 3 — Refatorar WeaponController

**Arquivo:** `Assets/Scripts/Combat/WeaponController.cs`

Substituir o conteúdo atual. O que muda:

1. **Remover** campos hardcoded: `_damage`, `_attackCooldown`, `_attackDuration`, `_knockbackForce`
2. **Adicionar** `[SerializeField] private WeaponDataSO _defaultWeapon` — arma inicial
3. **Adicionar** `[SerializeField] private WeaponVisualController _visualController` — referência ao visual
4. **Adicionar** `private WeaponDataSO _equippedWeapon` — arma atual (setado em Awake com default)
5. **Adicionar** `private List<WeaponDataSO> _availableWeapons` — lista de armas disponíveis para ciclo
6. **Adicionar** `private int _currentWeaponIndex` — índice na lista
7. **Adicionar** constante `private const float BaseHitboxSize = 0.8f` — tamanho base do prefab (evita hardcode escondido)
8. **Adicionar** input de swap (tecla Q) — ciclo real entre armas disponíveis
9. **Corrigir** `SpawnHitbox()` para usar `_player.GetComponent<PlayerController>().FacingDirection` ao invés de `_player.right`
10. **Corrigir** `SpawnHitbox()` para usar `_equippedWeapon.attackRange` ao invés de `0.8f`
11. **Adicionar** `SpawnHitbox()` — ajustar `localScale` da hitbox conforme `_equippedWeapon.attackRange / BaseHitboxSize`
12. **Integrar** `OnHitboxTrigger()` — consultar `RunUpgradeManager.Instance.GetDamageMultiplier()` para dano final (global da run, mantém após troca de arma)
13. **Adicionar** `EquipWeapon(WeaponDataSO weapon)` — troca arma, bloqueia se `_isAttacking`, adiciona na lista se não existe
14. **Adicionar** propriedades públicas: `EquippedWeapon`

**Código completo:**

```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    private const float BaseHitboxSize = 0.8f;

    [Header("Setup")]
    [SerializeField] private WeaponDataSO _defaultWeapon;
    [SerializeField] private GameObject _hitboxPrefab;
    [SerializeField] private Transform _player;
    [SerializeField] private WeaponVisualController _visualController;

    private WeaponDataSO _equippedWeapon;
    private bool _isAttacking;
    private float _cooldownTimer;
    private InputAction _attackAction;
    private InputAction _swapAction;
    private GameObject _activeHitbox;
    private PlayerController _playerController;

    private readonly List<WeaponDataSO> _availableWeapons = new List<WeaponDataSO>();
    private int _currentWeaponIndex;

    public bool IsAttacking => _isAttacking;
    public WeaponDataSO EquippedWeapon => _equippedWeapon;

    private void Awake()
    {
        if (_defaultWeapon != null)
        {
            _equippedWeapon = _defaultWeapon;
            _availableWeapons.Add(_defaultWeapon);
            _currentWeaponIndex = 0;
        }
        else
        {
            Debug.LogError("WeaponController: _defaultWeapon não atribuído no Inspector!");
        }

        _playerController = _player.GetComponent<PlayerController>();

        _attackAction = new InputAction("Attack", InputActionType.Button);
        _attackAction.AddBinding("<Mouse>/leftButton");
        _attackAction.AddBinding("<Keyboard>/enter");
        _attackAction.AddBinding("<Gamepad>/buttonWest");

        _swapAction = new InputAction("SwapWeapon", InputActionType.Button);
        _swapAction.AddBinding("<Keyboard>/q");
        _swapAction.AddBinding("<Gamepad>/buttonNorth");

        if (_visualController != null && _equippedWeapon != null)
        {
            _visualController.EquipVisual(_equippedWeapon.weaponType);
        }
    }

    private void OnEnable()
    {
        _attackAction.Enable();
        _attackAction.performed += OnAttackPerformed;
        _swapAction.Enable();
        _swapAction.performed += OnSwapPerformed;
    }

    private void OnDisable()
    {
        _attackAction.performed -= OnAttackPerformed;
        _attackAction.Disable();
        _swapAction.performed -= OnSwapPerformed;
        _swapAction.Disable();
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        Attack();
    }

    private void OnSwapPerformed(InputAction.CallbackContext context)
    {
        CycleWeapon();
    }

    private void Update()
    {
        if (_cooldownTimer > 0)
        {
            _cooldownTimer -= Time.deltaTime;
        }
    }

    public void Attack()
    {
        if (_isAttacking) return;
        if (_cooldownTimer > 0) return;
        if (_equippedWeapon == null) return;

        _isAttacking = true;
        SpawnHitbox();
        StartCoroutine(AttackRoutine());
    }

    private System.Collections.IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(_equippedWeapon.attackDuration);
        if (_activeHitbox != null)
        {
            Destroy(_activeHitbox);
            _activeHitbox = null;
        }
        _isAttacking = false;
        _cooldownTimer = _equippedWeapon.attackCooldown;
    }

    private void SpawnHitbox()
    {
        if (_activeHitbox != null)
        {
            Destroy(_activeHitbox);
        }

        Vector2 dir = _playerController != null ? _playerController.FacingDirection : Vector2.right;
        Vector3 offset = GetAttackOffset(dir);

        Vector3 spawnPos = _player.position + offset;
        _activeHitbox = Instantiate(_hitboxPrefab, spawnPos, Quaternion.identity, _player);

        float hitboxScale = _equippedWeapon.attackRange / BaseHitboxSize;
        _activeHitbox.transform.localScale = new Vector3(hitboxScale, hitboxScale, 1f);

        SwordHitbox swordHitbox = _activeHitbox.GetComponent<SwordHitbox>();
        swordHitbox.Initialize(this);
    }

    private Vector3 GetAttackOffset(Vector2 facingDir)
    {
        float range = _equippedWeapon.attackRange;

        switch (_equippedWeapon.attackPattern)
        {
            case AttackPattern.HorizontalSwing:
                return new Vector3(facingDir.x * range, 0.2f, 0f);
            case AttackPattern.ForwardThrust:
                return new Vector3(facingDir.x * (range + 0.2f), 0f, 0f);
            case AttackPattern.OverheadSmash:
                return new Vector3(facingDir.x * 0.3f, range * 0.5f, 0f);
            case AttackPattern.QuickStab:
                return new Vector3(facingDir.x * range, 0f, 0f);
            default:
                return new Vector3(facingDir.x * range, 0f, 0f);
        }
    }

    public void OnHitboxTrigger(Collider2D other)
    {
        if (_equippedWeapon == null) return;

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            int finalDamage = _equippedWeapon.damage;

            // Multiplicador global da run — aplica no hit, mantém após troca de arma.
            // Se RunUpgradeManager não existe na cena, multiplicador = 1f (dano base do SO).
            if (RunUpgradeManager.Instance != null)
            {
                finalDamage = Mathf.RoundToInt(finalDamage * RunUpgradeManager.Instance.GetDamageMultiplier());
            }

            damageable.TakeDamage(finalDamage);

            // Knockback: usa AddForce direto no Rigidbody2D do alvo.
            // TODO futuro: expor ApplyKnockback(Vector2, float) em interface IKnockbackable
            // para que o inimigo controle como reage (massa, estado, resistência).
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 knockDir = (other.transform.position - _player.position).normalized;
                rb.AddForce(knockDir * _equippedWeapon.knockbackForce, ForceMode2D.Impulse);
            }
        }
    }

    public void EquipWeapon(WeaponDataSO weapon)
    {
        if (weapon == null) return;

        // Sempre adicionar na lista se não existe — mesmo durante ataque
        if (!_availableWeapons.Contains(weapon))
        {
            _availableWeapons.Add(weapon);
        }

        // Se estiver atacando, não equipar agora — arma entra no ciclo de Q
        if (_isAttacking) return;

        _equippedWeapon = weapon;
        _currentWeaponIndex = _availableWeapons.IndexOf(weapon);

        if (_visualController != null)
        {
            _visualController.EquipVisual(weapon.weaponType);
        }
    }

    private void CycleWeapon()
    {
        if (_availableWeapons.Count <= 1) return;
        if (_isAttacking) return;

        _currentWeaponIndex = (_currentWeaponIndex + 1) % _availableWeapons.Count;
        _equippedWeapon = _availableWeapons[_currentWeaponIndex];

        if (_visualController != null)
        {
            _visualController.EquipVisual(_equippedWeapon.weaponType);
        }
    }
}
```

**Mudanças críticas vs. código atual:**
- Linha 83 atual (`_player.position + _player.right * 0.8f`) → `GetAttackOffset(facingDir)` com `FacingDirection` + `attackRange`
- Linha 94 atual (`TakeDamage(_damage)`) → cálculo com `RunUpgradeManager.GetDamageMultiplier()` (global da run)
- Linha 100 atual (`_knockbackForce`) → `_equippedWeapon.knockbackForce`
- `const BaseHitboxSize = 0.8f` substitui o magic number na fórmula de scale
- `CycleWeapon()` agora tem lógica real: ciclo entre `_availableWeapons`, ignora se só 1 arma
- `EquipWeapon()` adiciona arma na lista automaticamente, mesmo durante ataque — se `_isAttacking`, adiciona à lista mas não equipa agora. Pickup é sempre consumido, arma entra no ciclo de Q.
- `Awake()` chama `_visualController.EquipVisual(_equippedWeapon.weaponType)` para sincronizar visual com arma default
- `SwordHitbox` ganha `HashSet<Collider2D> _hitTargets` — cada ataque só acerta o mesmo alvo uma vez. `Initialize()` limpa o set.

---

### Passo 4 — WeaponVisualController

**Arquivo:** `Assets/Scripts/Combat/WeaponVisualController.cs`

Componente simples que ativa/desativa child GameObjects conforme `WeaponType`. Cada child é um placeholder visual da arma — uma forma geométrica simples com cor distinta, não o sprite de idle do player.

**Por que não usar sprite de idle puro:** sem diferenciação visual, fica impossível testar se a troca de arma está funcionando. Cada arma precisa de um child marker visível.

```csharp
using UnityEngine;

public class WeaponVisualController : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private GameObject _visualSword;
    [SerializeField] private GameObject _visualSpear;
    [SerializeField] private GameObject _visualAxe;
    [SerializeField] private GameObject _visualDagger;

    private GameObject _currentVisual;

    public void EquipVisual(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Sword:
                SetActiveVisual(_visualSword);
                break;
            case WeaponType.Spear:
                SetActiveVisual(_visualSpear);
                break;
            case WeaponType.Axe:
                SetActiveVisual(_visualAxe);
                break;
            case WeaponType.Dagger:
                SetActiveVisual(_visualDagger);
                break;
        }
    }

    private void SetActiveVisual(GameObject visual)
    {
        if (_visualSword != null) _visualSword.SetActive(false);
        if (_visualSpear != null) _visualSpear.SetActive(false);
        if (_visualAxe != null) _visualAxe.SetActive(false);
        if (_visualDagger != null) _visualDagger.SetActive(false);

        if (visual != null)
        {
            visual.SetActive(true);
        }

        _currentVisual = visual;
    }
}
```

**Nenhuma dependência externa.** Recebe referências no Inspector.

---

### Passo 5 — WeaponPickup

**Arquivo:** `Assets/Scripts/Combat/WeaponPickup.cs`

Segue o padrão de `RunePickup` — trigger collision, detecta layer Player, chama `EquipWeapon` no `WeaponController`.

```csharp
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private WeaponDataSO _weaponData;

    public WeaponDataSO WeaponData => _weaponData;

    public void Initialize(WeaponDataSO data)
    {
        _weaponData = data;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_weaponData == null)
        {
            Debug.LogError("WeaponPickup: _weaponData não atribuído no Inspector!", this);
            return;
        }

        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        WeaponController wc = other.GetComponentInParent<WeaponController>();
        if (wc == null) return;

        wc.EquipWeapon(_weaponData);
        Destroy(gameObject);
    }
}
```

---

### Passo 6 — ScriptableObject Assets

Criar 4 assets via `Assets → Create → Combat → Weapon Data`:

**`Assets/Data/Weapons/WeaponSword.asset`:**
| Campo | Valor |
|-------|-------|
| weaponType | Sword |
| weaponName | "Sword" |
| damage | 10 |
| attackCooldown | 0.3 |
| attackDuration | 0.1 |
| attackRange | 0.8 |
| knockbackForce | 2.0 |
| attackPattern | HorizontalSwing |
| placeholderColor | R=0.85 G=0.85 B=0.9 A=1 (cinza claro) |

**`Assets/Data/Weapons/WeaponSpear.asset`:**
| Campo | Valor |
|-------|-------|
| weaponType | Spear |
| weaponName | "Spear" |
| damage | 8 |
| attackCooldown | 0.4 |
| attackDuration | 0.12 |
| attackRange | 1.3 |
| knockbackForce | 1.5 |
| attackPattern | ForwardThrust |
| placeholderColor | R=0.3 G=0.5 B=0.9 A=1 (azul) |

**`Assets/Data/Weapons/WeaponAxe.asset`:**
| Campo | Valor |
|-------|-------|
| weaponType | Axe |
| weaponName | "Axe" |
| damage | 18 |
| attackCooldown | 0.6 |
| attackDuration | 0.15 |
| attackRange | 0.7 |
| knockbackForce | 4.0 |
| attackPattern | OverheadSmash |
| placeholderColor | R=0.9 G=0.3 B=0.3 A=1 (vermelho) |

**`Assets/Data/Weapons/WeaponDagger.asset`:**
| Campo | Valor |
|-------|-------|
| weaponType | Dagger |
| weaponName | "Dagger" |
| damage | 5 |
| attackCooldown | 0.15 |
| attackDuration | 0.06 |
| attackRange | 0.5 |
| knockbackForce | 1.0 |
| attackPattern | QuickStab |
| placeholderColor | R=0.3 G=0.8 B=0.3 A=1 (verde) |

**Cores atualizadas** conforme Nota 3: sword=cinza, spear=azul, axe=vermelho, dagger=verde. Cada visual placeholder será uma child com SpriteRenderer usando essa cor.

---

## Setup na Unity

### 1. Criar estrutura de pastas de assets

```
Assets/Data/Weapons/        ← criar se não existe
Assets/Prefabs/              ← já existe
Assets/Scenes/               ← já existe
```

### 2. Criar ScriptableObject assets

1. `Assets → Create → Combat → Weapon Data` → renomear "WeaponSword"
2. Configurar campos conforme tabela do Passo 6
3. Repetir para Spear, Axe, Dagger
4. Mover todos para `Assets/Data/Weapons/`

### 3. Atualizar prefab do Player

Abrir `Assets/Prefabs/Player.prefab` no prefab editor.

**Convenção:** `WeaponController` vive no root do Player prefab. `WeaponPickup` usa `GetComponentInParent<WeaponController>()` para encontrá-lo a partir de qualquer collider child.

**Adicionar child GameObjects para visuais (placeholder):**

Cada visual é um Empty GameObject filho do Player com um SpriteRenderer. Usar QUALQUER sprite (um quadrado, ou o sprite de idle do player — não importa), apenas com a cor correta. O importante é que cada um seja visível e distinto.

```
Player (prefab root)
├── ... (componentes existentes)
├── Visual_Sword (Empty)
│   └── SpriteRenderer (qualquer sprite, color = R0.85 G0.85 B0.9)
├── Visual_Spear (Empty)
│   └── SpriteRenderer (qualquer sprite, color = R0.3 G0.5 B0.9)
├── Visual_Axe (Empty)
│   └── SpriteRenderer (qualquer sprite, color = R0.9 G0.3 B0.3)
├── Visual_Dagger (Empty)
│   └── SpriteRenderer (qualquer sprite, color = R0.3 G0.8 B0.3)
└── GroundCheck (existente)
```

Para cada visual:
1. Criar Empty como filho do Player
2. Renomear para `Visual_Sword` (etc.)
3. Adicionar SpriteRenderer — usar qualquer sprite como placeholder
4. No SpriteRenderer: Color conforme a cor do SO correspondente
5. Posicionar com `localPosition` consistente (ex: offset na mão/lado do personagem, todos na mesma posição)
6. Definir `Sorting Layer` e `Order in Layer` compatíveis — todos com mesmo sorting, acima do sprite do player, para não sumirem atrás dele
7. Desativar todos EXCETO Visual_Sword (default)

**Adicionar componente WeaponVisualController ao Player:**
1. No Player, Add Component → `WeaponVisualController`
2. Arrastar:
   - `_visualSword` → Visual_Sword
   - `_visualSpear` → Visual_Spear
   - `_visualAxe` → Visual_Axe
   - `_visualDagger` → Visual_Dagger

**Atualizar componente WeaponController no Player:**
1. Selecionar o componente WeaponController existente
2. Os campos antigos (`_damage`, `_attackCooldown`, etc.) foram removidos — o script novo não os tem mais
3. Configurar campos novos:
   - `_defaultWeapon` → arrastar `WeaponSword.asset` de `Assets/Data/Weapons/`
   - `_hitboxPrefab` → arrastar `SwordHitbox.prefab` (já existente)
   - `_player` → arrastar o próprio Player (this.transform)
   - `_visualController` → arrastar o componente `WeaponVisualController` do mesmo Player

**Ajustar SwordHitbox prefab:**
1. Abrir `Assets/Prefabs/SwordHitbox.prefab`
2. No BoxCollider2D: ajustar Size para `X = 0.8, Y = 0.8` (base oficial para o cálculo `attackRange / BaseHitboxSize`)
3. Salvar prefab

### 4. Criar WeaponPickup prefab

1. `Assets/Prefabs/` → Create Empty → renomear "WeaponPickup"
2. Adicionar componentes:
   - SpriteRenderer → sprite de item placeholder (ex: gem do SunnyLand)
   - CircleCollider2D → `Is Trigger = true`, Radius: 0.3
   - WeaponPickup.cs → `_weaponData`: deixar vazio (cada instância no scene define qual arma)
3. Arrastar para `Assets/Prefabs/` para criar prefab
4. Deletar da cena

### 5. Criar cena de teste (WeaponTestScene)

Criar uma cena dedicada para testar o sistema de armas isoladamente.

1. `File → New Scene → Basic (Built-in)`
2. `File → Save As → Assets/Scenes/WeaponTestScene.unity`
3. Não precisa adicionar ao Build Settings (é só para dev)

**Dependências explícitas:**
- NÃO carregar `FloorManager`, `DungeonGenerator` ou outros managers de dungeon — esta cena é isolada
- `RunUpgradeManager` é opcional na cena de teste. Se não presente, dano usa valor base do SO sem multiplicador. Se presente, usar versão mínima com `DontDestroyOnLoad` desabilitado.

**Hierarquia da cena:**
```
WeaponTestScene
├── Main Camera
│   └── CameraFollow.cs (_target = Player)
├── Player (Player.prefab)
├── Chão (SpriteRenderer grande, Layer = Ground)
├── Pickup_Spear (WeaponPickup + WeaponDataSO = WeaponSpear)
├── Pickup_Axe (WeaponPickup + WeaponDataSO = WeaponAxe)
├── Pickup_Dagger (WeaponPickup + WeaponDataSO = WeaponDagger)
└── DummyEnemy (Enemy.prefab para testar dano)
```

**Passo a passo para montar:**
1. Arrastar `Player.prefab` para a cena
2. Criar Empty "Chão" → adicionar SpriteRenderer com sprite grande + BoxCollider2D, Layer = Ground
3. Posicionar Chão abaixo do player
4. Criar 3 instâncias de `WeaponPickup.prefab` (arrastar da pasta Prefabs)
5. Em cada instância, configurar `_weaponData`:
   - Pickup_Spear → WeaponSpear.asset
   - Pickup_Axe → WeaponAxe.asset
   - Pickup_Dagger → WeaponDagger.asset
6. Posicionar pickups espalhados perto do player
7. (Opcional) Arrastar `Enemy.prefab` como dummy para testar dano

### 6. Testar

**Teste 1 — Arma default (Sword):**
1. Abrir `WeaponTestScene`
2. Play Mode
3. Atacar (mouse esquerdo / Enter) → hitbox aparece na direção que o player olha
4. Hitbox Size = 0.8 (scale = 1.0, pois attackRange/BaseHitboxSize = 0.8/0.8 = 1.0)
5. Visual_Sword (cinza) está visível

**Teste 2 — Troca de arma via pickup:**
1. Andar sobre Pickup_Axe → visual muda para vermelho (axe)
2. Atacar → hitbox mais lenta, mais knockback, offset diferente (OverheadSmash)
3. Andar sobre Pickup_Spear → visual muda para azul
4. Atacar → hitbox maior (attackRange=1.3, scale=1.625), mais à frente (ForwardThrust)

**Teste 3 — Swap com Q:**
1. Começar com Sword equipada (default)
2. Pressionar Q → arma muda para a próxima na lista (primeiro pickup coletado)
3. Pressionar Q novamente → ciclo continua
4. Se só 1 arma na lista → Q não faz nada

**Teste 4 — Bloqueio de troca durante ataque:**
1. Atacar → durante a animação, coletar pickup
2. Pickup é destruído, arma é adicionada à lista `_availableWeapons`, mas NÃO é equipada (_isAttacking bloqueia o equip)
3. Próximo ataque usa arma antiga
4. Pressionar Q agora inclui a arma coletada no ciclo

**Teste 5 — Dano com RunUpgradeManager (se disponível):**
1. Se RunUpgradeManager existe na cena, coletar upgrade de dano
2. Atacar inimigo → dano final = weapon.damage × DamageMultiplier

### 7. Configurar WeaponPickup nos prefabs de sala (futuro)

Para spawnar pickups em salas de reward, adicionar o prefab WeaponPickup como filho de `RewardRoom.prefab` com `_weaponData` configurado no Inspector.

---

## Atualização do SETUP.md

Adicionar nova seção após a Seção 13 (Fase 7). O bloco abaixo é o conteúdo exato a ser adicionado:

```
## 14. Fase 8 — Sistema de Armas

### 14.1 Arquivos Criados

| Arquivo | Função |
|---------|--------|
| `Assets/Scripts/Combat/WeaponType.cs` | Enum: Sword, Spear, Axe, Dagger |
| `Assets/Scripts/Combat/AttackPattern.cs` | Enum: HorizontalSwing, ForwardThrust, OverheadSmash, QuickStab |
| `Assets/Scripts/Combat/WeaponDataSO.cs` | ScriptableObject de dados de arma |
| `Assets/Scripts/Combat/WeaponVisualController.cs` | Controla visual da arma equipada |
| `Assets/Scripts/Combat/WeaponPickup.cs` | Pickup de arma no chão |
| `Assets/Scenes/WeaponTestScene.unity` | Cena dedicada para testar armas |

### 14.2 Arquivos Modificados

| Arquivo | O que mudou |
|---------|-------------|
| `Assets/Scripts/Combat/WeaponController.cs` | Refatorado: lê stats de WeaponDataSO, usa FacingDirection, suporta EquipWeapon(), lista de armas disponíveis com swap por Q, integrado com RunUpgradeManager.DamageMultiplier (global da run). EquipWeapon sempre adiciona à lista; durante ataque não equipa mas arma entra no ciclo. |
| `Assets/Prefabs/SwordHitbox.prefab` | BoxCollider2D Size ajustado para 0.8 (base oficial) |

### 14.3 ScriptableObjects — Armas

Criar em `Assets/Data/Weapons/`:

| Asset | weaponType | damage | attackRange | knockback | cooldown | pattern | cor placeholder |
|-------|-----------|--------|-------------|-----------|----------|---------|----------------|
| WeaponSword | Sword | 10 | 0.8 | 2.0 | 0.3 | HorizontalSwing | cinza (0.85, 0.85, 0.9) |
| WeaponSpear | Spear | 8 | 1.3 | 1.5 | 0.4 | ForwardThrust | azul (0.3, 0.5, 0.9) |
| WeaponAxe | Axe | 18 | 0.7 | 4.0 | 0.6 | OverheadSmash | vermelho (0.9, 0.3, 0.3) |
| WeaponDagger | Dagger | 5 | 0.5 | 1.0 | 0.15 | QuickStab | verde (0.3, 0.8, 0.3) |

Como criar: Assets → Create → Combat → Weapon Data

### 14.4 Prefab Player — Visuais de Arma

Hierarquia atualizada do Player:

```
Player
├── Rigidbody2D
├── BoxCollider2D
├── PlayerController.cs
├── PlayerHealth.cs
├── WeaponController.cs
│   ├── _defaultWeapon = WeaponSword.asset
│   ├── _hitboxPrefab = SwordHitbox.prefab
│   ├── _player = Player (this)
│   └── _visualController = WeaponVisualController (self)
├── WeaponVisualController.cs
│   ├── _visualSword = Visual_Sword
│   ├── _visualSpear = Visual_Spear
│   ├── _visualAxe = Visual_Axe
│   └── _visualDagger = Visual_Dagger
├── Visual_Sword (SpriteRenderer, color=cinza, ATIVO)
├── Visual_Spear (SpriteRenderer, color=azul, DESATIVO)
├── Visual_Axe (SpriteRenderer, color=vermelho, DESATIVO)
├── Visual_Dagger (SpriteRenderer, color=verde, DESATIVO)
└── GroundCheck
```

### 14.5 Prefab WeaponPickup

```
WeaponPickup (prefab)
├── SpriteRenderer (gem/item placeholder)
├── CircleCollider2D (isTrigger=true, radius=0.3)
└── WeaponPickup.cs
    └── _weaponData: configurar no Inspector de cada instância
```

### 14.6 Cena de Teste (WeaponTestScene)

Criar cena dedicada em `Assets/Scenes/WeaponTestScene.unity`:

```
WeaponTestScene
├── Main Camera (CameraFollow.cs, _target = Player)
├── Player (Player.prefab)
├── Chão (SpriteRenderer + BoxCollider2D, Layer = Ground)
├── Pickup_Spear (WeaponPickup, _weaponData = WeaponSpear.asset)
├── Pickup_Axe (WeaponPickup, _weaponData = WeaponAxe.asset)
├── Pickup_Dagger (WeaponPickup, _weaponData = WeaponDagger.asset)
└── DummyEnemy (Enemy.prefab, opcional)
```

### 14.7 Como Testar

1. Abrir cena **WeaponTestScene**
2. Play → atacar → verificar hitbox na direção correta (sword, cinza)
3. Andar sobre Pickup_Axe → visual muda para vermelho
4. Atacar com Axe → mais lento, mais dano, mais knockback, offset acima
5. Pressionar Q → ciclo entre armas disponíveis
6. Coletar Spear → Q agora cicla entre Sword, Axe, Spear
7. Verificar que coletar pickup durante ataque adiciona arma à lista mas não equipa (pickup não é perdido)
8. Verificar que RunUpgradeManager.DamageMultiplier afeta dano (se disponível na cena)

### 14.8 Troubleshooting

| Problema | Solução |
|----------|---------|
| Hitbox não aparece | Verificar _defaultWeapon conectado, _hitboxPrefab = SwordHitbox |
| Visual não muda | Verificar WeaponVisualController refs, Visual_* GameObjects ativos/desativos |
| Swap Q não funciona | Verificar _availableWeapons.Count > 1. Coletar pickup primeiro |
| Dano errado | Verificar valores no WeaponDataSO. Checar RunUpgradeManager.Instance existe |
| Troca durante ataque | Comportamento esperado: EquipWeapon() adiciona à lista mas não equipa se _isAttacking. Pickup não é perdido. |
| Hitbox acerta múltiplas vezes | Verificar HashSet _hitTargets no SwordHitbox. Initialize() deve limpar o set a cada ataque. |
| Hitbox tamanho errado | Verificar SwordHitbox BoxCollider2D Size = 0.8. Verificar attackRange no SO |
```

---

## Checklist de Implementação

- [x] Passo 1: Criar `WeaponType.cs` e `AttackPattern.cs`
  - Arquivo: `Assets/Scripts/Combat/WeaponType.cs`
  - Arquivo: `Assets/Scripts/Combat/AttackPattern.cs`
  - O que fazer: Enums simples, sem dependências

- [x] Passo 2: Criar `WeaponDataSO.cs`
  - Arquivo: `Assets/Scripts/Combat/WeaponDataSO.cs`
  - O que fazer: ScriptableObject com campos de arma. Seguir padrão de RunUpgradeSO.

- [x] Passo 3: Refatorar `WeaponController.cs`
  - Arquivo: `Assets/Scripts/Combat/WeaponController.cs`
  - O que fazer: Substituir hardcoded por WeaponDataSO. Adicionar EquipWeapon(), availableWeapons list, CycleWeapon() com lógica real, BaseHitboxSize const, GetAttackOffset(), RunUpgradeManager integration. EquipWeapon sempre adiciona à lista; se _isAttacking, não equipa mas arma entra no ciclo. Awake chama EquipVisual para sincronizar visual.

- [x] Passo 4: Criar `WeaponVisualController.cs`
  - Arquivo: `Assets/Scripts/Combat/WeaponVisualController.cs`
  - O que fazer: MonoBehaviour com 4 GameObject refs, método EquipVisual(WeaponType), ativar/desativar.

- [x] Passo 5: Criar `WeaponPickup.cs`
  - Arquivo: `Assets/Scripts/Combat/WeaponPickup.cs`
  - O que fazer: MonoBehaviour seguindo padrão de RunePickup. Guard _weaponData null (LogError + return). Trigger collision → EquipWeapon via GetComponentInParent.

- [x] Passo 6: Criar 4 ScriptableObject assets
  - Arquivos: `Assets/Data/Weapons/Weapon{Sword,Spear,Axe,Dagger}.asset`
  - O que fazer: Assets → Create → Combat → Weapon Data. Configurar valores da tabela (cores específicas por arma).

- [x] Passo 7: Ajustar SwordHitbox prefab
  - Prefab: `Assets/Prefabs/SwordHitbox.prefab`
  - O que fazer: BoxCollider2D Size = 0.8 (base oficial para cálculo de scale).

- [x] Passo 8: Atualizar Player prefab — visuais
  - Prefab: `Assets/Prefabs/Player.prefab`
  - O que fazer: Criar 4 child GameObjects (Visual_Sword/Spear/Axe/Dagger) com SpriteRenderer e cor placeholder. Adicionar WeaponVisualController. Conectar refs.

- [x] Passo 9: Atualizar Player prefab — WeaponController
  - Prefab: `Assets/Prefabs/Player.prefab`
  - O que fazer: Reconfigurar WeaponController: _defaultWeapon=Sword.asset, _visualController=referência.

- [x] Passo 10: Criar WeaponPickup prefab
  - Prefab: `Assets/Prefabs/WeaponPickup.prefab`
  - O que fazer: Create Empty, adicionar SpriteRenderer + CircleCollider2D (trigger) + WeaponPickup.cs. Converter em prefab.

- [x] Passo 11: Criar cena de teste
  - Cena: `Assets/Scenes/WeaponTestScene.unity`
  - O que fazer: Nova cena com Player, chão, 3 pickups (Spear, Axe, Dagger), dummy enemy opcional.

- [x] Passo 12: Testar em Play Mode
  - Cena: WeaponTestScene
  - O que fazer: Atacar com sword. Coletar pickups. Verificar swap com Q. Verificar visual, hitbox, dano, bloqueio de troca durante ataque.

- [x] Passo 13: Atualizar `SETUP.md`
  - Arquivo: `SETUP.md`
  - O que fazer: Adicionar Seção 14 com toda a documentação de setup da Fase 8 (código pronto no bloco acima).

---

## Perguntas

> Todas as 5 perguntas originais foram respondidas pelo usuário e endereçadas neste documento.
> As 20 notas de revisão foram incorporadas ao spec (ver seção "Notas de Revisão — Resolvidas" no final).
> Resumo das respostas aplicadas:

1. **SwordHitbox size** → Resolvido: Prefab ajustado para Size = 0.8. Campo `const BaseHitboxSize = 0.8f` no WeaponController.

2. **Swap de arma** → Resolvido: `availableWeapons` list + `currentWeaponIndex` implementados. Q cicla entre disponíveis. Pickup adiciona na lista.

3. **Visual placeholder** → Resolvido: Child visual com cor distinta por arma (cinza/azul/vermelho/verde). Não usar sprite de idle puro.

4. **DamageMultiplier** → Resolvido: Comportamento mantido. É global da run, aplica no hit, não é afetado por troca de arma. Comentário no código esclarece.

5. **Cena de teste** → Resolvido: `WeaponTestScene.unity` criada como cena dedicada.

---

## Validação

- [ ] Scripts compilam sem erro no Unity (compilação automática ao salvar)
- [ ] 4 ScriptableObject assets criados em `Assets/Data/Weapons/`
- [ ] Player prefab tem WeaponVisualController com 4 visuais conectados
- [ ] Player prefab tem WeaponController com _defaultWeapon = WeaponSword.asset
- [ ] SwordHitbox prefab tem BoxCollider2D Size = 0.8
- [ ] Sword ataca na direção correta (FacingDirection, não _player.right)
- [ ] Sword hitbox tem tamanho correto (attackRange=0.8, scale=1.0)
- [ ] WeaponPickup com _weaponData vazio não quebra (LogError + return)
- [ ] Visual_* posicionados com sorting acima do player (não some atrás do sprite)
- [ ] Ataques funcionam com FacingDirection horizontal (esquerda/direita apenas)
- [ ] Visual muda ao trocar de arma (cores distintas visíveis)
- [ ] Pressionar Q cicla entre armas disponíveis
- [ ] Coletar pickup durante ataque: arma entra na lista mas não equipa (pickup não é perdido)
- [ ] Ataque só acerta cada inimigo uma vez (HashSet no hitbox)
- [ ] Dano é afetado por RunUpgradeManager.DamageMultiplier (global da run)
- [ ] Cena WeaponTestScene existe e pode ser usada para teste rápido (sem dungeon managers)
- [ ] SETUP.md seção 14 existe com passo a passo completo

---

## Notas de Revisão — Resolvidas

Todas as 20 notas foram endereçadas e incorporadas ao spec:

| # | Nota | Resolução |
|---|------|-----------|
| 1 | Bloqueio de pickup durante ataque | `EquipWeapon()` sempre adiciona à lista; se `_isAttacking`, não equipa agora mas arma entra no ciclo de Q. Pickup sempre consumido. |
| 2 | Multi-hit no mesmo inimigo | `SwordHitbox` ganha `HashSet<Collider2D> _hitTargets` — cada ataque só acerta o mesmo alvo uma vez. Set limpo em `Initialize()`. |
| 3 | Knockback direto no Rigidbody2D | Mantido AddForce direto por agora. Adicionado TODO futuro para interface `IKnockbackable` — inimigo controla como reage. |
| 4 | FacingDirection nunca zero | Documentado em Pré-condições: `_facingDirection` só atualiza quando `_horizontalInput != 0`, caso contrário retém último valor válido. |
| 5 | Visual assume sword no Awake | `WeaponVisualController.Awake()` removido. `WeaponController.Awake()` chama `EquipVisual(_equippedWeapon.weaponType)` para sincronizar. |
| 6 | GetComponent no pickup | `WeaponPickup` usa `GetComponentInParent<WeaponController>()` — funciona com collider em child objects. |
| 7 | RunUpgradeManager.GetDamageMultiplier() | Confirmado como pré-condição. Método existe em `RunUpgradeManager.cs:88-91`, retorna `float`. |
| 8 | Duplicatas por tipo na lista | Documentado em Pré-condições: cada arma base tem um único `WeaponDataSO`. Comparação por referência é suficiente. |
| 9 | Cena de teste e dungeon managers | Seção de teste atualizada: NÃO carregar FloorManager/DungeonGenerator. RunUpgradeManager é opcional. |
| 10 | Comportamento do pickup ao equipar | Definido: adiciona à lista se não existe, equipa imediatamente se não atacando, sempre consome pickup. |
| 11 | Nome SwordHitbox | Decisão D6 atualizada: manter agora, renomear para `WeaponHitbox` quando fase estabilizar (TODO futuro). |
| 12 | WeaponDataSO campos públicos | Nota adicionada: concessão de velocidade para protótipo. Preferir `[SerializeField] private` com props read-only em produção. |
| 13 | AttackPattern limitado | Nota adicionada: padrão atual controla apenas offset/feeling. Animação e multi-stage attacks ficam para fase posterior. |
| 14 | Guard _weaponData null | `WeaponPickup.OnTriggerEnter2D` valida `_weaponData == null` com `LogError` + return antes de qualquer lógica. |
| 15 | WeaponController placement | Documentado: WeaponController vive no root do Player prefab. Pickup usa `GetComponentInParent`. |
| 16 | Direção só horizontal | Nota de escopo em Pré-condições: `FacingDirection` é sempre esquerda/direita (2D side-scroller). Ataque vertical fica para fase posterior. |
| 17 | GetAttackOffset usa x apenas | `GetAttackOffset` usa `facingDir.x` — consistente com movimentação horizontal. Decisão documentada. |
| 18 | Posicionamento dos placeholders | Passo de setup atualizado: posição local consistente, sorting acima do player, não sumir atrás do sprite. |
| 19 | RunUpgradeManager default | Comentário no código: se `Instance == null`, multiplicador = 1f (dano base do SO). |
| 20 | IDamageable contrato | Pré-condição adicionada: `IDamageable.TakeDamage(int)` é o contrato mínimo. Compatível com inimigos, bosses, dummies, objetos destrutíveis. |