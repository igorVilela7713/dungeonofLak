# SPEC — Fase 3: Loop (Sala + Morte + Restart)

## 1. Arquivos a Criar

```
Assets/Scripts/
├── Core/
│   └── GameManager.cs          # Gerencia morte do player e restart
└── Rooms/
    ├── RoomController.cs       # Gerencia sala: spawn, portas, limpeza
    └── Door.cs                 # Porta que abre/fecha
```

---

## 2. Estrutura de Classes

### GameManager (MonoBehaviour)

Responsabilidade: Escutar morte do player e recarregar cena.

```
Campos:
- _sceneName (string) — nome da cena para reload, default "Main"

Métodos:
- Awake() — vincular PlayerHealth.OnDeath ao OnPlayerDeath
- OnPlayerDeath() — recarregar cena
```

### RoomController (MonoBehaviour)

Responsabilidade: Quando player entra na sala, spawnar inimigos e fechar portas.

```
Campos:
- _spawnPoints (Transform[]) — posições de spawn
- _enemyPrefab (GameObject) — prefab do inimigo
- _doors (Door[]) — portas da sala
- _enemiesAlive (int) — contador de inimigos vivos
- _isActive (bool) — sala está em combate

Métodos:
- OnTriggerEnter2D(Collider2D other) — detectar player entrando
- SpawnEnemies() — instanciar inimigos nos spawn points
- OnEnemyKilled() — decrementar contador, verificar se sala limpa
- OpenDoors() — abrir todas as portas
- CloseDoors() — fechar todas as portas
```

### Door (MonoBehaviour)

Responsabilidade: Controlar collider de porta (ativo = bloqueado).

```
Campos:
- _collider (Collider2D) — referência ao collider
- _isOpen (bool) — estado da porta

Métodos:
- Open() — desativar collider, _isOpen = true
- Close() — ativar collider, _isOpen = false
- IsOpen (propriedade)
```

---

## 3. Fluxo Lógico

### Passo a Passo: Sala

```
1. Player entra no Collider2D da sala (isTrigger = true)
       ↓
2. RoomController.OnTriggerEnter2D()
   → verifica se other.layer == "Player"
   → verifica se !_isActive
   ↓
3. _isActive = true
   → CloseDoors() — ativa collider de cada porta
   → SpawnEnemies() — instancia inimigos nos spawn points
       ↓
4. Inimigos são instanciados
   → enemy.GetComponent<EnemyController>() recebe referência ao player
   → enemy.AddComponent<RoomEnemyTracker>() — vincula referência à sala
       ↓
5. Inimigo morre (EnemyController.TakeDamage → Destroy)
   → OnEnemyKilled() — chamado por RoomEnemyTracker.OnDestroy()
       ↓
6. _enemiesAlive == 0
   → _isActive = false
   → OpenDoors() — desativa collider de cada porta
```

### Passo a Passo: Morte/Restart

```
1. PlayerHealth.OnDeath é disparado (HP <= 0)
       ↓
2. GameManager.OnPlayerDeath()
       ↓
3. SceneManager.LoadScene("Main")
       ↓
4. Cena recarrega completamente
```

---

## 4. Pseudocódigo Detalhado

### GameManager

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private string _sceneName = "Main";
    [SerializeField] private PlayerHealth _playerHealth;
    
    private void Awake()
    {
        if (_playerHealth != null)
        {
            _playerHealth.OnDeath.AddListener(OnPlayerDeath);
        }
    }
    
    private void OnPlayerDeath()
    {
        SceneManager.LoadScene(_sceneName);
    }
}
```

### RoomController

```csharp
using UnityEngine;

public class RoomController : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private GameObject _enemyPrefab;
    
    [Header("Doors")]
    [SerializeField] private Door[] _doors;
    
    [Header("References")]
    [SerializeField] private Transform _player;
    
    private int _enemiesAlive;
    private bool _isActive;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isActive) return;
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        
        _isActive = true;
        CloseDoors();
        SpawnEnemies();
    }
    
    private void SpawnEnemies()
    {
        if (_spawnPoints == null) return;
        
        foreach (Transform point in _spawnPoints)
        {
            GameObject enemy = Instantiate(_enemyPrefab, point.position, Quaternion.identity);
            
            // Configurar enemy
            var controller = enemy.GetComponent<EnemyController>();
            if (controller != null)
            {
                controller.Initialize(_player);
            }
            
            // Vincular callback quando inimigo morrer
            var tracker = enemy.AddComponent<EnemyDeathTracker>();
            tracker.Initialize(this);
            
            _enemiesAlive++;
        }
    }
    
    public void OnEnemyKilled()
    {
        _enemiesAlive--;
        
        if (_enemiesAlive <= 0)
        {
            _isActive = false;
            OpenDoors();
        }
    }
    
    private void CloseDoors()
    {
        foreach (Door door in _doors)
        {
            door.Close();
        }
    }
    
    private void OpenDoors()
    {
        foreach (Door door in _doors)
        {
            door.Open();
        }
    }
}
```

### Door

```csharp
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Collider2D _collider;
    private bool _isOpen = true;
    
    public bool IsOpen => _isOpen;
    
    public void Open()
    {
        _collider.enabled = false;
        _isOpen = true;
    }
    
    public void Close()
    {
        _collider.enabled = true;
        _isOpen = false;
    }
}
```

### EnemyDeathTracker (necessário para RoomController saber quando inimigo morreu)

```csharp
using UnityEngine;

public class EnemyDeathTracker : MonoBehaviour
{
    private RoomController _room;
    
    public void Initialize(RoomController room)
    {
        _room = room;
    }
    
    private void OnDestroy()
    {
        if (_room != null)
        {
            _room.OnEnemyKilled();
        }
    }
}
```

---

## 5. Integração na Unity

### GameManager

```
GameObject: "GameManager" (vazio)
Componentes:
  - GameManager.cs
  - Drag PlayerHealth do Player para _playerHealth

Posição: Não importa (singleton simples)
```

### RoomController

```
GameObject: "Room" (pai da sala)
Componentes:
  - BoxCollider2D (isTrigger = true, tamanho cobrindo sala)
  - RoomController.cs

Child Objects:
  - "SpawnPoint1" (empty GameObject, posição de spawn)
  - "SpawnPoint2" (empty GameObject, posição de spawn)
  - "Door1" (prefab Door)
  - "Door2" (prefab Door)

Inspector:
  - _spawnPoints: arrastar SpawnPoint1, SpawnPoint2
  - _enemyPrefab: arrastar Enemy.prefab
  - _doors: arrastar Door1, Door2
  - _player: arrastar Player
```

### Door

```
GameObject: "Door"
Componentes:
  - BoxCollider2D (tamanho como porta, isTrigger = false)
  - Door.cs (sprite de porta, opcional)
  - SpriteRenderer (visual da porta)
```

### Player (modificação necessária)

```
No PlayerController, adicionar:
  - Rigidbody2D (já existe)

No EnemyController, adicionar método público:
  - Initialize(Transform player) — para RoomController configurar
```

---

## 6. Modificações no EnemyController Existente

Adicionar método Initialize para RoomController poder configurar o player:

```csharp
public void Initialize(Transform player)
{
    _player = player;
}
```

---

## 7. Layers e Colisão

| Layer | Índice | Colide com |
|-------|--------|------------|
| Player | 8 | Enemy, Walls |
| Enemy | 9 | Player, Walls |
| PlayerAttack | 10 | Enemy |

**Collision Matrix:**
- Player ↔ Walls: SIM
- Enemy ↔ Walls: SIM
- Player ↔ Door: SIM (quando collider ativo)

---

## 8. Critérios de Validação

| Teste | Resultado Esperado |
|-------|-------------------|
| Player entra na sala | Portas fecham, inimigos spawneam |
| Player mata todos inimigos | Portas abrem |
| Player morre | Cena reinicia ("Main") |
| Inimigos seguem player | Inimigos ativos durante combate |
| Porta bloqueia passagem | Player não atravessa porta fechada |
