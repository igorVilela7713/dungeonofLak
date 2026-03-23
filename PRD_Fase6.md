# PRD — FASE 6: Hub

## Objetivo

Implementar a cena Hub (cidade de Mixhull) como ponto de partida do jogador antes de entrar na dungeon. A Fase 6 inclui quatro sistemas: transição de cenas (Hub ↔ Dungeon), NPC interativo básico com texto de diálogo, sistema de save/load em JSON local, e câmera que segue o player. O Hub serve como área segura onde o jogador pode descansar, interagir com NPCs e salvar progresso antes de enfrentar a dungeon.

---

## Arquivos Relevantes

| Arquivo | Relevância | Motivo |
|---------|------------|--------|
| Assets/Scripts/Core/GameManager.cs | alta | Gerencia fluxo do jogo (reload de cena na morte). Deve ser expandido ou substituído para lidar com transição Hub ↔ Dungeon |
| Assets/Scripts/Movement/PlayerController.cs | alta | Player se move no Hub igual na dungeon — precisa funcionar em ambas as cenas |
| Assets/Scripts/Combat/PlayerHealth.cs | alta | Dados de vida do player devem persistir entre cenas e ser salvos no JSON |
| Assets/Scripts/Combat/WeaponController.cs | média | Referência para entender padrão de Input System code-based |
| Assets/Scripts/Rooms/Door.cs | média | Pode servir de referência para portas/trigger no Hub |
| Assets/Scripts/Enemies/EnemyController.cs | baixa | Não existe no Hub, mas pode ser referência de padrão de Initialize() |

---

## Assets / Prefabs / Scenes Relevantes

| Caminho | Tipo | Motivo |
|--------|------|--------|
| Assets/Scenes/Main.unity | Scene | Cena atual (dungeon). O GameManager precisará carregar essa cena a partir do Hub |
| Assets/Prefabs/Player.prefab | Prefab | Player precisa existir tanto no Hub quanto na dungeon. Deve ser reutilizado |
| Assets/SunnyLand Artwork/Environment/props/ | Sprites | Props de cenário (houses, doors, trees, crates) para decorar o Hub |
| Assets/SunnyLand Artwork/Environment/Tileset/ | Tilemap | Tileset para construir o chão/paredes do Hub |
| Assets/SunnyLand Artwork/Sprites/player/ | Sprites | Player idle/run sprites (já existem animações no art pack) |
| Assets/UI/ | Diretório | Vazio — será usado para criar UI de diálogo e HUD básico |

---

## Padrões Encontrados no Projeto

### 1. Input System (code-based, sem asset)

O projeto cria `InputAction` em `Awake()` e habilita em `OnEnable()`. Exemplo de `PlayerController.cs`:

```csharp
_moveAction = new InputAction("Move", InputActionType.Value);
_moveAction.AddCompositeBinding("1DAxis")
    .With("Negative", "<Keyboard>/a")
    .With("Positive", "<Keyboard>/d");
```

Qualquer novo input (interação, navegação de menu) deve seguir esse padrão.

### 2. Interface IDamageable

```csharp
public interface IDamageable
{
    void TakeDamage(int amount);
}
```

O projeto usa interfaces mínimas. Para interação com NPC, pode-se criar `IInteractable` seguindo o mesmo padrão.

### 3. Inicialização via Initialize()

`EnemyController.Initialize(Transform player)` e `SwordHitbox.Initialize(WeaponController)` mostram que o projeto injeta dependências via método `Initialize()` em vez de `GetComponent` ou `FindObjectOfType`. NPCs devem seguir o mesmo padrão se precisarem de referências externas.

### 4. UnityEvent para comunicação

`PlayerHealth.OnDeath` é `UnityEvent` — usado pelo `GameManager` para reagir à morte do player. Esse padrão pode ser reutilizado para eventos de NPC (ex: `OnInteract`).

### 5. RequireComponent

```csharp
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
```

Scripts que precisam de componentes físicos usam `[RequireComponent]`.

### 6. Cena única — SceneManager.LoadScene()

`GameManager.OnPlayerDeath()` usa `SceneManager.LoadScene(_sceneName)` para reload. Para transição Hub ↔ Dungeon, o mesmo método será usado com nomes de cena diferentes.

---

## Documentação Externa

| Referência | URL | Uso |
|-----------|-----|-----|
| SceneManager | https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.html | Transição de cenas, carregar/descarregar scenes |
| JsonUtility | https://docs.unity3d.com/ScriptReference/JsonUtility.html | Serialização de save data para JSON |
| System.IO.File | https://docs.microsoft.com/dotnet/api/system.io.file | Leitura/escrita do arquivo de save |
| Unity UI (uGUI) | https://docs.unity3d.com/Manual/com.unity.ugui.html | Canvas, TextMeshPro, Button para diálogos |
| Trigger Colliders (2D) | https://docs.unity3d.com/Manual/Collider2D.html | Detectar entrada do player em zona de NPC/portal |
| Application.persistentDataPath | https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html | Caminho seguro para salvar JSON em qualquer plataforma |

---

## Componentes Necessários

### Scripts novos

| Script | Local | Função |
|--------|-------|--------|
| HubManager.cs | Assets/Scripts/Core/ | Controla fluxo do Hub: spawn do player, interações, saída para dungeon |
| SceneTransition.cs | Assets/Scripts/Core/ | Portal/trigger que carrega outra cena ao player entrar |
| NPCInteractable.cs | Assets/Scripts/NPCs/ | NPC com trigger de interação e exibição de texto |
| DialogueUI.cs | Assets/Scripts/UI/ | Mostra/esconde texto de diálogo na tela (Canvas + TextMeshPro) |
| SaveSystem.cs | Assets/Scripts/Core/ | Salva e carrega dados do player em JSON |
| SaveData.cs | Assets/Scripts/Core/ | Classe serializável com os dados a salvar (HP, posição, inventário futuro) |
| CameraFollow.cs | Assets/Scripts/Camera/ | Câmera segue o player com suavização (SmoothDamp) |

### Prefabs novos

| Prefab | Função |
|--------|--------|
| NPC.prefab | Sprite + Collider2D (trigger) + NPCInteractable.cs |
| Portal.prefab | Sprite + Collider2D (trigger) + SceneTransition.cs |

### Scenes

| Scene | Função |
|-------|--------|
| Hub.unity (nova) | Área do Hub com player, NPCs, portal para dungeon, chão via tilemap |

### Modificações em scripts existentes

| Script | Mudança |
|--------|---------|
| GameManager.cs | Deve suportar carregar Hub ou Dungeon. Pode ser renomeado/reorganizado, ou ter lógica condicional baseada na cena atual |

---

## Fluxo Esperado

### Fluxo principal

1. **Jogo inicia** → carrega cena `Hub`
2. **Player spawna no Hub** em posição definida
3. **Player se move livremente** pelo Hub (mesmo PlayerController, sem inimigos)
4. **Player chega em NPC** → aparece texto de diálogo na tela
   - Player pressiona input de interação (E / Gamepad B)
   - Texto do NPC aparece em Canvas
   - Player pressiona novamente → texto some
5. **Player chega em Portal** (ex: porta da dungeon) → SceneTransition carrega cena `Main` (dungeon)
6. **Na dungeon**, gameplay normal (rooms, enemies, combat)
7. **Player morre na dungeon** → GameManager carrega `Hub` (não reload da dungeon)
8. **Player entra em portal no Hub novamente** → recomeça dungeon

### Fluxo de save

1. Player interage com NPC específico (ex: cama, checkpoint)
2. SaveSystem serializa SaveData para JSON em `Application.persistentDataPath`
3. SaveData contém: `_maxHealth`, `_currentHealth`, posição (futuro), inventário (futuro)
4. Na inicialização do Hub, SaveSystem tenta carregar → se existe save, restaura dados

### Fluxo da câmera

1. CameraFollow.cs anexado à Main Camera
2. Em `LateUpdate`, interpola posição da câmera para seguir o player (SmoothDamp)
3. Eixo Z da câmera não muda
4. Funciona tanto no Hub quanto na dungeon (mesmo script, mesmo player)

---

## Constraints

- **Simplicidade primeiro**: sem sistemas genéricos de diálogo, sem state machines complexas, sem singletons desnecessários
- **Reutilizar Player.prefab**: player funciona igual no Hub e na dungeon, sem duplicar
- **JSON simples**: `JsonUtility` para save, sem SQLite, sem criptografia
- **Sem Cinemachine**: câmera com SmoothDamp manual, como pedido no plano
- **Código consistente com projeto**: Allman braces, 4 spaces, `[SerializeField] private`, code-based Input System
- **Placeholder antes de arte final**: usar sprites do SunnyLand para cenário do Hub
- **Uma cena por vez**: não usar additive scene loading — SceneManager.LoadScene simples
- **Interface mínima**: se `IInteractable` for criado, ter só 1 método como `IDamageable`
- **No namespaces** (convenção do projeto)

---

## Riscos / Pontos de Atenção

| Risco | Impacto | Mitigação |
|-------|---------|-----------|
| Player não funciona ao trocar de cena | Player com Input System pode ter problemas ao DontDestroyOnLoad ou ao reiniciar | Usar Instantiate do Player prefab em cada cena, não DontDestroyOnLoad |
| Save corrompido ou caminho inexistente | Crash em plataformas sem permissão de escrita | Usar try/catch e `Application.persistentDataPath` |
| GameManager conflita com nova cena | GameManager está na cena Main e reloada "Main" na morte | Criar HubManager separado para o Hub, ou fazer GameManager detectar cena atual |
| Câmera com jitter em pixel-perfect | Movimento suave pode conflitar com pixel snap | Garantir que Pixel Perfect Camera está configurado, rodar em LateUpdate |
| NPC trigger dispara múltiplas vezes | OnTriggerEnter2D pode disparar repetidamente | Usar flag `_isInteracting` ou `GetKeyDown` em vez de `performed` contínuo |
| Cena Hub sem referência ao player | EnemyDeathTracker, RoomController etc. dependem de referência ao player | Hub não tem inimigos — não há dependência de RoomController na cena Hub |
| Input de interação conflita com ataque | Se usar mesma tecla, player ataca ao interagir | Usar tecla diferente (E para interagir, Enter/click para atacar) |
| JsonUtility não serializa dicionários ou listas complexas | SaveData futuro com inventário pode precisar de estrutura diferente | Por enquanto só salvar HP e dados simples. Evitar inventário na Fase 6 |

---

## Decisões a Tomar

1. **Player no Hub: compartilhado ou instanciado?**
   - Opção A: Player prefab instanciado em cada cena (simples, sem problemas de DontDestroyOnLoad)
   - Opção B: DontDestroyOnLoad no player (persiste entre cenas, mas complica setup)
   - **Recomendado**: Opção A — instanciar em cada cena. SaveSystem cuida de restaurar HP.

2. **GameManager: um ou dois?**
   - Opção A: GameManager único com lógica condicional (`if scene == "Hub"`)
   - Opção B: HubManager separado na cena Hub, GameManager só na dungeon
   - **Recomendado**: Opção B — separação de responsabilidades.

3. **Input de interação: qual tecla?**
   - Opção A: E (keyboard) + Gamepad B (buttonEast)
   - Opção B: F (keyboard) + Gamepad B
   - **Recomendado**: Opção A — padrão comum em jogos 2D.

4. **Save automático ou manual?**
   - Opção A: Save manual (player interage com NPC/checkpoint para salvar)
   - Opção B: Save automático ao entrar/sair do Hub
   - **Recomendado**: Opção A — mais controle, menos risco de salvar estado ruim.

5. **Cena Hub: nova cena ou adicionar à Main?**
   - Opção A: Cena separada `Hub.unity`
   - Opção B: Tudo na `Main.unity` com GameObjects desligados
   - **Recomendado**: Opção A — separação clara, SceneManager.LoadScene funciona naturalmente.

6. **Estrutura do SaveData: o que salvar na Fase 6?**
   - Mínimo: `_currentHealth`, `_maxHealth`
   - Futuro: posição, inventário, NPCs desbloqueados, andar atual
   - **Recomendado**: Só HP por enquanto. Expandir nas fases futuras.

7. **Câmera: ortographic size fixo ou ajustável?**
   - **Recomendado**: Fixo. Usar o valor atual da câmera. Não complicar.
