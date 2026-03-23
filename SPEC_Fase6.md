# SPEC — FASE 6: Hub

---

## Arquivos a Criar

| Path | Tipo | Descrição |
|------|------|-----------|
| `Assets/Scripts/Core/SaveData.cs` | Script | Classe serializável (`[System.Serializable]`) com dados a salvar |
| `Assets/Scripts/Core/SaveSystem.cs` | Script | Salva/carrega JSON em `Application.persistentDataPath` |
| `Assets/Scripts/Core/SceneTransition.cs` | Script | Trigger collider que carrega outra cena via `SceneManager.LoadScene` |
| `Assets/Scripts/NPCs/NPCInteractable.cs` | Script | NPC com trigger, input de interação, mostra/esconde texto |
| `Assets/Scripts/NPCs/SaveInteractable.cs` | Script | NPC de save — salva dados do player em JSON ao interagir |
| `Assets/Scripts/UI/DialogueUI.cs` | Script | Controla um Canvas com TextMeshPro para mostrar diálogos |
| `Assets/Scripts/Camera/CameraFollow.cs` | Script | Câmera segue player com `SmoothDamp` em `LateUpdate` |
| `Assets/Scenes/Hub.unity` | Scene | Cena do Hub (criar via Unity Editor, não por código) |

---

## Arquivos a Modificar

| Path | Mudanças |
|------|----------|
| `Assets/Scripts/Core/GameManager.cs` | Mudar `_sceneName` default de `"Main"` para `"Hub"`. Na morte do player, carregar Hub em vez de recarregar a dungeon. |
| `Assets/Scripts/Combat/PlayerHealth.cs` | Adicionar método `public void HealToFull()` para restaurar HP a partir do save |

---

## Detalhamento de Cada Arquivo

### 1. `SaveData.cs`

Classe pura de dados, sem MonoBehaviour.

```csharp
[System.Serializable]
public class SaveData
{
    public int currentHealth;
    public int maxHealth;

    public static SaveData CreateDefault()
    {
        return new SaveData
        {
            currentHealth = 100,
            maxHealth = 100
        };
    }
}
```

- `[System.Serializable]` obrigatório para `JsonUtility`
- Campos públicos (não propriedades) — `JsonUtility` não serializa properties
- Método estático `CreateDefault()` para fallback quando não existe save

### 2. `SaveSystem.cs`

Classe estática (sem MonoBehaviour). Métodos simples sem instância.

```csharp
using UnityEngine;
using System.IO;

public static class SaveSystem
{
    private static string SavePath =>
        Path.Combine(Application.persistentDataPath, "save.json");

    public static void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }

    public static SaveData Load()
    {
        if (!File.Exists(SavePath))
        {
            return SaveData.CreateDefault();
        }

        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static bool HasSave()
    {
        return File.Exists(SavePath);
    }

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }
    }
}
```

- Classe `static` — não precisa de GameObject, chamada direta: `SaveSystem.Save(data)`
- `Path.Combine` para compatibilidade cross-platform
- `JsonUtility.ToJson(data, true)` — `true` = pretty print legível
- Try/catch não é estritamente necessário aqui — `File.WriteAllText` já lança exceção clara. Se houver problemas em runtime, adicionar depois.

### 3. `SceneTransition.cs`

MonoBehaviour com trigger collider. Detecta player e carrega cena.

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string _targetScene;

    private bool _isTransitioning;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isTransitioning) return;
        if (other.gameObject.layer != LayerMask.NameToLayer("player")) return;

        _isTransitioning = true;
        SceneManager.LoadScene(_targetScene);
    }
}
```

- `_targetScene` configurável no Inspector — pode ser `"Main"` (dungeon) ou `"Hub"`
- Flag `_isTransitioning` evita double-load se trigger disparar múltiplas vezes
- Checa layer "player" como os outros scripts do projeto

### 4. `NPCInteractable.cs`

MonoBehaviour com trigger collider e input de interação code-based. Responsável apenas por diálogo — sem lógica de save.

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class NPCInteractable : MonoBehaviour
{
    [SerializeField] private string _dialogueText = "Hello, traveler!";

    private InputAction _interactAction;
    private bool _playerInRange;
    private bool _isShowingDialogue;

    public string DialogueText => _dialogueText;

    private void Awake()
    {
        _interactAction = new InputAction("Interact", InputActionType.Button);
        _interactAction.AddBinding("<Keyboard>/e");
        _interactAction.AddBinding("<Gamepad>/buttonEast");
    }

    private void OnEnable()
    {
        _interactAction.Enable();
        _interactAction.performed += OnInteractPerformed;
    }

    private void OnDisable()
    {
        _interactAction.performed -= OnInteractPerformed;
        _interactAction.Disable();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("player")) return;
        _playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("player")) return;
        _playerInRange = false;

        if (_isShowingDialogue)
        {
            DialogueUI.Instance.Hide();
            _isShowingDialogue = false;
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (!_playerInRange) return;

        if (_isShowingDialogue)
        {
            DialogueUI.Instance.Hide();
            _isShowingDialogue = false;
        }
        else
        {
            DialogueUI.Instance.Show(_dialogueText);
            _isShowingDialogue = true;
        }
    }
}
```

- Input `E` / Gamepad B — mesma estrutura de `PlayerController.Awake()`
- `_playerInRange` flag via `OnTriggerEnter2D` / `OnTriggerExit2D`
- Toggle de diálogo (mostra/esconde ao pressionar E)
- `DialogueUI.Instance` — referência estática simples (ver abaixo)
- `_dialogueText` via `[SerializeField]` — configurável por NPC no Inspector
- **Sem lógica de save** — save fica no `SaveInteractable.cs` separado

### 5. `SaveInteractable.cs`

Script separado para NPCs de checkpoint/save. Mesmo padrão de trigger + input do `NPCInteractable`, mas salva dados ao interagir.

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class SaveInteractable : MonoBehaviour
{
    [Header("Save Settings")]
    [SerializeField] private PlayerHealth _playerHealth;

    [Header("Dialogue")]
    [SerializeField] private string _dialogueText = "Progresso salvo!";

    private InputAction _interactAction;
    private bool _playerInRange;
    private bool _isShowingDialogue;

    private void Awake()
    {
        _interactAction = new InputAction("Interact", InputActionType.Button);
        _interactAction.AddBinding("<Keyboard>/e");
        _interactAction.AddBinding("<Gamepad>/buttonEast");
    }

    private void OnEnable()
    {
        _interactAction.Enable();
        _interactAction.performed += OnInteractPerformed;
    }

    private void OnDisable()
    {
        _interactAction.performed -= OnInteractPerformed;
        _interactAction.Disable();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("player")) return;
        _playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("player")) return;
        _playerInRange = false;

        if (_isShowingDialogue)
        {
            DialogueUI.Instance.Hide();
            _isShowingDialogue = false;
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (!_playerInRange) return;

        if (_isShowingDialogue)
        {
            DialogueUI.Instance.Hide();
            _isShowingDialogue = false;
        }
        else
        {
            SaveData data = new SaveData
            {
                currentHealth = _playerHealth.CurrentHealth,
                maxHealth = _playerHealth.MaxHealth
            };
            SaveSystem.Save(data);

            DialogueUI.Instance.Show(_dialogueText);
            _isShowingDialogue = true;
        }
    }
}
```

- Mesmo padrão de trigger + input do `NPCInteractable` — consistência com o projeto
- Ao interagir: cria `SaveData` com HP atual e salva via `SaveSystem.Save()`
- Depois de salvar: mostra texto de confirmação ("Progresso salvo!")
- `_playerHealth` configurável no Inspector — arrastar o Player
- Script separado do NPCInteractable para manter responsabilidade única

### 6. `DialogueUI.cs`

MonoBehaviour que controla um Canvas com texto. Singleton simples via `Instance`.

```csharp
using UnityEngine;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    [SerializeField] private GameObject _dialoguePanel;
    [SerializeField] private TextMeshProUGUI _dialogueText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Show(string text)
    {
        _dialogueText.text = text;
        _dialoguePanel.SetActive(true);
    }

    public void Hide()
    {
        _dialoguePanel.SetActive(false);
    }
}
```

- Singleton simples para acesso fácil de `NPCInteractable`
- `_dialoguePanel` — GameObject do painel (Panel no Canvas), ativado/desativado
- `_dialogueText` — componente TextMeshProUGUI dentro do painel
- `Show()` ativa painel e seta texto. `Hide()` desativa painel.

### 7. `CameraFollow.cs`

MonoBehaviour na Main Camera. Segue player com suavização.

```csharp
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _smoothTime = 0.15f;

    private Vector3 _velocity = Vector3.zero;

    private void LateUpdate()
    {
        if (_target == null) return;

        Vector3 targetPosition = new Vector3(
            _target.position.x,
            _target.position.y,
            transform.position.z
        );

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref _velocity,
            _smoothTime
        );
    }
}
```

- `_target` configurável no Inspector — arrastar o Player
- `_smoothTime` = 0.15s — ajuste fino para sensibilidade de câmera
- `Vector3.SmoothDamp` — mais suave que Lerp, não tem problema de frame-rate dependency
- Eixo Z preservado (`transform.position.z`) — câmera 2D não muda Z
- `LateUpdate` — executa depois de todo Update/FixedUpdate, evita jitter

### 8. Modificação em `GameManager.cs`

Mudar o comportamento de morte: em vez de recarregar a cena atual, carregar o Hub.

```csharp
// ANTES:
[SerializeField] private string _sceneName = "Main";

private void OnPlayerDeath()
{
    SceneManager.LoadScene(_sceneName);
}

// DEPOIS:
[SerializeField] private string _sceneName = "Hub";

private void OnPlayerDeath()
{
    SceneManager.LoadScene(_sceneName);
}
```

- Apenas mudar o default de `_sceneName` de `"Main"` para `"Hub"`
- Na dungeon, o GameManager terá `_sceneName` configurado como `"Hub"` no Inspector
- Não precisa de lógica condicional — o valor é configurável por cena

### 9. Modificação em `PlayerHealth.cs`

Adicionar método para restaurar vida a partir do save.

```csharp
public void HealToFull()
{
    _currentHealth = _maxHealth;
}
```

- Chamado pelo `HubManager` ao iniciar o Hub se existir save
- Método público simples, sem lógica complexa

---

## Setup na Unity

### Step 1: Criar cena Hub

1. File → New Scene → Basic (Built-in)
2. Salvar como `Assets/Scenes/Hub.unity`
3. Adicionar ao Build Settings (File → Build Settings → Add Open Scenes)
4. A cena `Main` já deve estar no Build Settings — garantir que ambas estão listadas

### Step 2: Configurar o chão do Hub

Na cena `Hub.unity`:

1. Criar um Tilemap ou usar sprites do SunnyLand como chão
2. Criar paredes/bordas para o player não sair da área
3. Layer do chão: **Ground** (já existe)
4. Layer das paredes: **Default** ou **Ground**
5. Posicionar spawn point do player (Empty GameObject "PlayerSpawn")

### Step 3: Instanciar o Player

1. Arrastar o **Player.prefab** para a cena Hub
2. Posicionar no spawn point
3. O PlayerController, PlayerHealth, WeaponController já vêm do prefab
4. WeaponController pode ficar — ataque não interfere no Hub

### Step 4: Criar UI de Diálogo

1. Right-click na Hierarchy → **UI → Canvas**
   - Canvas Scaler: Scale With Screen Size, Reference Resolution 1920x1080
   - Render Mode: Screen Space - Overlay
2. Dentro do Canvas, criar **UI → Panel** (renomear para "DialoguePanel")
   - Anchor: bottom center
   - Size: ~800x200
   - Cor de fundo: preto semi-transparente (Alpha ~0.8)
   - Desativar o Panel por default (unchecked no Inspector)
3. Dentro do DialoguePanel, criar **UI → Text - TextMeshPro**
   - Renomear para "DialogueText"
   - Configurar fonte, tamanho 24, cor branca
   - Anchor: stretch para preencher o panel
4. Criar Empty GameObject "DialogueUI"
   - Adicionar componente **DialogueUI.cs**
   - `_dialoguePanel`: arrastar DialoguePanel
   - `_dialogueText`: arrastar DialogueText (componente TextMeshProUGUI)

### Step 5: Criar NPC

1. Criar Empty GameObject "NPC" na cena Hub
2. Adicionar **SpriteRenderer** (sprite placeholder do SunnyLand, ex: personagem)
3. Adicionar **BoxCollider2D** — marcar **isTrigger = true**
   - Ajustar tamanho para a área de interação (~1x2)
4. Adicionar **NPCInteractable.cs**
   - `_dialogueText`: digitar o texto do NPC (ex: "Bem-vindo a Mixhull! A dungeon fica ao leste.")
5. Layer: pode ser Default (não precisa de layer específico)
6. Posicionar na área do Hub

### Step 6: Criar Portal para a Dungeon

1. Criar Empty GameObject "DungeonPortal" na cena Hub
2. Adicionar **SpriteRenderer** (sprite placeholder de porta/portal)
3. Adicionar **BoxCollider2D** — marcar **isTrigger = true**
   - Ajustar tamanho (~1.5x2)
4. Adicionar **SceneTransition.cs**
   - `_targetScene`: digitar `"Main"`
5. Posicionar em um canto do Hub (ex: lado direito)

### Step 7: Configurar a Main Camera

1. Selecionar a **Main Camera** na cena Hub
2. Adicionar **CameraFollow.cs**
   - `_target`: arrastar o Player da cena
   - `_smoothTime`: 0.15 (padrão)
3. Repetir na cena Main (dungeon) se quiser câmera seguindo lá também

### Step 8: Criar HubManager

1. Criar Empty GameObject "HubManager" na cena Hub
2. Adicionar componente **HubManager.cs** (script simples — ver abaixo)
3. Responsabilidades:
   - Carregar save ao iniciar (se existir)
   - Restaurar HP do player

HubManager.cs é um script pequeno:

```csharp
using UnityEngine;

public class HubManager : MonoBehaviour
{
    [SerializeField] private PlayerHealth _playerHealth;

    private void Start()
    {
        if (SaveSystem.HasSave())
        {
            SaveData data = SaveSystem.Load();
            // Futuro: restaurar HP, inventário, etc.
        }
    }
}
```

- `_playerHealth`: arrastar o PlayerHealth do Player na cena
- Por enquanto só carrega o save (preparação para futuro)
- Se quiser restaurar HP: `_playerHealth.HealToFull()` após ler o save

### Step 9: Criar NPC de Save (checkpoint)

1. Criar outro GameObject na cena Hub, renomear para "SaveNPC"
2. Adicionar **SpriteRenderer** (sprite placeholder)
3. Adicionar **BoxCollider2D** — marcar **isTrigger = true**
4. Adicionar **SaveInteractable.cs** (script separado do NPCInteractable)
   - `_playerHealth`: arrastar o PlayerHealth do Player na cena
   - `_dialogueText`: "Progresso salvo!"
5. Posicionar na área do Hub

O SaveInteractable tem a mesma estrutura de trigger + input que o NPCInteractable, mas ao interagir chama `SaveSystem.Save()` com os dados atuais do player.

### Step 10: Configurar GameManager na Dungeon (cena Main)

1. Abrir cena `Main`
2. Selecionar o GameManager
3. No Inspector: mudar `_sceneName` de `"Main"` para `"Hub"`
4. Agora quando o player morre na dungeon, volta para o Hub

### Step 11: Build Settings

1. File → Build Settings
2. Garantir que as cenas estão na ordem:
   - Index 0: `Assets/Scenes/Hub.unity` (cena inicial)
   - Index 1: `Assets/Scenes/Main.unity` (dungeon)
3. O jogo sempre inicia no Hub (index 0)

---

## Atualização do SETUP.md

Adicionar seção **12. Fase 6 — Hub** com:

### 12.1 Cena Hub

```
Criar cena "Hub" (Assets/Scenes/Hub.unity)
  → Adicionar ao Build Settings (index 0)
  → Player prefab instanciado na cena
  → Chão com tilemap ou sprites (Layer = Ground)
  → Paredes/bordas para conter o player
```

### 12.2 UI de Diálago

```
Criar Canvas (Screen Space - Overlay)
  → DialoguePanel (Panel, desativado por default)
      → DialogueText (TextMeshProUGUI)
Criar Empty "DialogueUI"
  → DialogueUI.cs
    - _dialoguePanel: arrastar DialoguePanel
    - _dialogueText: arrastar DialogueText
```

### 12.3 Prefab: NPC

```
Criar GameObject "NPC"
  → SpriteRenderer (placeholder, Layer = Default)
  → BoxCollider2D (isTrigger = true)
  → NPCInteractable.cs
    - _dialogueText: texto do NPC
```

### 12.3b NPC de Save

```
Criar GameObject "SaveNPC"
  → SpriteRenderer (placeholder)
  → BoxCollider2D (isTrigger = true)
  → SaveInteractable.cs
    - _playerHealth: arrastar PlayerHealth do Player
    - _dialogueText: "Progresso salvo!"
```

### 12.4 Portal para Dungeon

```
Criar GameObject "DungeonPortal"
  → SpriteRenderer (placeholder)
  → BoxCollider2D (isTrigger = true)
  → SceneTransition.cs
    - _targetScene: "Main"
```

### 12.5 Camera Follow

```
Na Main Camera de AMBAS as cenas (Hub e Main):
  → CameraFollow.cs
    - _target: arrastar Player
    - _smoothTime: 0.15
```

### 12.6 GameManager

```
Na cena Main (dungeon):
  GameManager._sceneName = "Hub" (mudar de "Main")

Na cena Hub:
  HubManager.cs
    - _playerHealth: arrastar PlayerHealth do Player
```

### 12.7 Build Settings

```
File → Build Settings:
  Index 0: Hub.unity
  Index 1: Main.unity
```

### 12.8 Hierarquia da Cena Hub

```
Scene "Hub"
├── Main Camera (CameraFollow.cs)
├── Player (Player.prefab)
├── HubManager (HubManager.cs)
├── DialogueUI (DialogueUI.cs)
│   └── Canvas
│       └── DialoguePanel (desativado)
│           └── DialogueText (TextMeshProUGUI)
├── NPC (NPCInteractable.cs, BoxCollider2D trigger)
├── SaveNPC (SaveInteractable.cs, BoxCollider2D trigger)
├── DungeonPortal (SceneTransition.cs, BoxCollider2D trigger)
├── PlayerSpawn (Empty, posição de spawn)
└── [Chão/Paredes via tilemap]
```

### 12.9 Troubleshooting (acrescentar)

| Problema | Solução |
|----------|---------|
| Cena Hub não abre | Verificar Build Settings: Hub deve estar index 0 |
| Transição não funciona | Verificar _targetScene no SceneTransition, Scene no Build Settings |
| Diálogo não aparece | Verificar DialogueUI.Instance, Panel desativado por default |
| NPC não detecta player | Verificar isTrigger=true, Layer do player |
| Save não persiste | Verificar Application.persistentDataPath, permissões de escrita |
| Câmera treme | Usar LateUpdate, SmoothDamp, verificar Pixel Perfect Camera |
| Player não spawna no Hub | Verificar Player.prefab na cena, posição do PlayerSpawn |

---

## Checklist de Implementação

- [x] Passo 1: Criar `SaveData.cs`
  - Arquivo: `Assets/Scripts/Core/SaveData.cs`
  - Classe `[System.Serializable]` com `currentHealth`, `maxHealth`, `CreateDefault()`

- [x] Passo 2: Criar `SaveSystem.cs`
  - Arquivo: `Assets/Scripts/Core/SaveSystem.cs`
  - Classe static: `Save()`, `Load()`, `HasSave()`, `DeleteSave()`
  - Usa `JsonUtility` + `File.WriteAllText/ReadAllText`

- [x] Passo 3: Criar `SceneTransition.cs`
  - Arquivo: `Assets/Scripts/Core/SceneTransition.cs`
  - MonoBehaviour com trigger, campo `_targetScene`, flag anti-duplo-load

- [x] Passo 4: Criar `DialogueUI.cs`
  - Arquivo: `Assets/Scripts/UI/DialogueUI.cs`
  - MonoBehaviour singleton com `Show(string)` e `Hide()`
  - Controla Panel + TextMeshProUGUI

- [x] Passo 5: Criar `NPCInteractable.cs`
  - Arquivo: `Assets/Scripts/NPCs/NPCInteractable.cs`
  - MonoBehaviour com trigger, input E, toggle de diálogo
  - Apenas lógica de diálogo — sem save

- [x] Passo 6: Criar `SaveInteractable.cs`
  - Arquivo: `Assets/Scripts/NPCs/SaveInteractable.cs`
  - Mesmo padrão de trigger + input do NPCInteractable
  - Ao interagir: salva SaveData via SaveSystem.Save() e mostra texto de confirmação

- [x] Passo 7: Criar `CameraFollow.cs`
  - Arquivo: `Assets/Scripts/Camera/CameraFollow.cs`
  - MonoBehaviour com SmoothDamp em LateUpdate

- [x] Passo 8: Criar `HubManager.cs`
  - Arquivo: `Assets/Scripts/Core/HubManager.cs`
  - MonoBehaviour que carrega save no Start

- [x] Passo 9: Modificar `GameManager.cs`
  - Arquivo: `Assets/Scripts/Core/GameManager.cs`
  - Mudar default de `_sceneName` para `"Hub"`

- [x] Passo 10: Adicionar `HealToFull()` a `PlayerHealth.cs`
  - Arquivo: `Assets/Scripts/Combat/PlayerHealth.cs`
  - Método público: `_currentHealth = _maxHealth`

- [x] Passo 11: Criar cena Hub no Unity Editor
  - File → New Scene → salvar como `Assets/Scenes/Hub.unity`

- [x] Passo 12: Montar UI de diálogo na cena Hub
  - Canvas + DialoguePanel (desativado) + DialogueText (TMP)
  - GameObject DialogueUI com script

- [x] Passo 13: Criar NPC na cena Hub
  - SpriteRenderer + BoxCollider2D (trigger) + NPCInteractable

- [x] Passo 14: Criar SaveNPC na cena Hub
  - SpriteRenderer + BoxCollider2D (trigger) + SaveInteractable

- [x] Passo 15: Criar DungeonPortal na cena Hub
  - Sprite + BoxCollider2D (trigger) + SceneTransition (_targetScene = "Main")

- [x] Passo 16: Configurar CameraFollow em ambas as cenas
  - Main Camera → CameraFollow.cs → _target = Player

- [x] Passo 17: Criar HubManager na cena Hub
  - Empty GameObject + HubManager.cs → _playerHealth = PlayerHealth

- [x] Passo 18: Configurar GameManager na cena Main
  - _sceneName = "Hub"

- [x] Passo 19: Build Settings
  - Index 0: Hub.unity, Index 1: Main.unity

- [x] Passo 20: Atualizar SETUP.md
  - Adicionar seção 12 com toda a configuração do Hub

- [x] Passo 21: Testar em Play Mode
  - Fluxo completo: Hub → portal → dungeon → morrer → Hub

---

## Perguntas (Resolvidas)

1. **NPC de save: script separado ou campo no NPCInteractable?**
   - **Decisão**: Opção B — `SaveInteractable.cs` separado (mais limpo, responsabilidade única)
   - Status: implementado no spec como Passo 6

2. **HubManager: necessário ou opcional?**
   - **Decisão**: Manter como script separado — vai crescer com NPCs desbloqueáveis e progressão
   - Status: confirmado, Passo 8

3. **Câmera no Hub e na dungeon: mesmo script nas duas cenas?**
   - **Decisão**: Sim — CameraFollow.cs em ambas as cenas, cada uma com sua Main Camera
   - Status: confirmado, Passo 16

4. **SaveNPC: um ou vários?**
   - **Decisão**: 1 SaveNPC no Hub inicialmente
   - Status: confirmado, Passo 14

---

## Validação

- [x] Scripts compilam sem erro no Unity Console
- [x] Setup no Inspector está documentado no SETUP.md
- [x] Fluxo pode ser testado em Play Mode:
  1. Jogo abre no Hub
  2. Player se move normalmente
  3. Player chega em NPC → texto aparece
  4. Player pressiona E → texto some
  5. Player entra no Portal → cena muda para dungeon
  6. Gameplay normal na dungeon
  7. Player morre → volta para o Hub
  8. Player interage com SaveNPC → dados salvos
  9. Reiniciar jogo → HP restaurado do save
- [x] `SETUP.md` atualizado com seção 12 (Hub)
