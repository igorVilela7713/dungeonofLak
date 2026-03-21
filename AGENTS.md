# Agent Guidelines for Unity Roguelike Project

## Project Overview
- **Engine**: Unity 6000.3.11f1 (2D)
- **Language**: C#
- **Render Pipeline**: Universal Render Pipeline (URP)
- **Input System**: Unity Input System
- **Type**: Roguelike 2D game

## Build & Test Commands

### Unity Editor
- Open project in Unity Hub to run the game
- Use `Ctrl+Shift+B` to open Build Settings
- Build and Run: `Ctrl+B`

### Running Tests
1. Open Unity Editor
2. Open Test Runner: `Window > General > Test Runner`
3. Run All Tests or select specific test assembly
4. For command-line testing:
   ```
   Unity.exe -runTests -projectPath "C:\Users\igor7\My project" -testResults "path/to/results.xml"
   ```

### Code Analysis
- Use Visual Studio's built-in analyzer (included via `com.unity.ide.visualstudio`)
- Enable Roslyn analyzers in Visual Studio for real-time feedback

## Code Style Guidelines

### General Principles
1. **Simple > Clever**: Avoid overengineering; write readable code
2. **YAGNI**: Don't implement features until needed
3. **Separation of Concerns**: System ≠ Content ≠ Polish
4. **Incremental Implementation**: One feature at a time

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Classes | PascalCase | `PlayerController` |
| Methods | PascalCase | `TakeDamage()` |
| Properties | PascalCase | `MaxHealth` |
| Private fields | _camelCase | `_currentHealth` |
| Public fields | PascalCase | `Health` |
| Constants | PascalCase | `MaxSpeed` |
| Enums | PascalCase | `EnemyState` |
| Enum values | PascalCase | `EnemyState.Chasing` |
| Interfaces | IPascalCase | `IDamageable` |
| Namespaces | PascalCase | `Game.Combat` |

### File Organization
```
Assets/Scripts/
  ├── Core/           # GameManager, singletons
  ├── Combat/         # Damage, weapons
  ├── Movement/       # Player movement, physics
  ├── Enemies/        # Enemy AI
  ├── Rooms/          # Room logic
  ├── UI/             # User interface
  └── Data/           # ScriptableObjects
```

### Unity-Specific Guidelines

#### MonoBehaviour Scripts
```csharp
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 5f;
    
    [Header("Combat")]
    [SerializeField] private int _maxHealth = 100;
    
    private int _currentHealth;
    
    private void Awake()
    {
        _currentHealth = _maxHealth;
    }
    
    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        if (_currentHealth <= 0)
        {
            Die();
        }
    }
}
```

#### ScriptableObjects
```csharp
[CreateAssetMenu(fileName = "New Weapon", menuName = "Game/Weapon")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public int damage;
    public float attackSpeed;
    public GameObject prefab;
}
```

### Using Directives
Order imports:
1. `System` namespaces
2. `UnityEngine` namespaces
3. `UnityEngine.UI` (if needed)
4. Other Unity namespaces
5. Project namespaces (alphabetical)

### Serialization
- Use `[SerializeField]` for private fields that need serialization
- Use `[Header("Section Name")]` to organize Inspector fields
- Use `[Range(min, max)]` for numeric fields with limits
- Use `[RequireComponent(typeof(T))]` when a component is mandatory

### Error Handling
```csharp
// Use Debug.LogWarning for recoverable issues
Debug.LogWarning("Enemy not found in scene");

// Use Debug.LogError + return for critical failures
if (target == null)
{
    Debug.LogError("Target is null");
    return;
}

// Use exceptions sparingly in gameplay code
throw new System.InvalidOperationException("State transition not allowed");
```

### Coroutines
```csharp
private IEnumerator DieAfterDelay()
{
    yield return new WaitForSeconds(1f);
    Destroy(gameObject);
}
```

### Unity Object References
```csharp
// Check for null using null-coalescing
if (_target != null)
{
    _target.TakeDamage(_damage);
}

// Cache components in Awake for performance
private void Awake()
{
    _rigidbody = GetComponent<Rigidbody2D>();
    _spriteRenderer = GetComponent<SpriteRenderer>();
}
```

### Performance Considerations
1. Cache component references instead of calling `GetComponent` repeatedly
2. Use object pooling for frequently spawned objects
3. Avoid `Find` methods in Update; use public references or `GetComponentInChildren`
4. Use `[DisallowMultipleComponent]` to prevent duplicate scripts
5. Mark empty callback methods (Start, Update) as commented or removed

## Implementation Workflow

### Before Coding
1. Read existing `SPEC.md` or create one if needed
2. Identify components required (scripts, prefabs, assets)
3. Keep solutions simple; avoid premature abstraction

### During Implementation
1. Implement one feature at a time
2. Validate in Unity Editor after each change
3. Test in Play Mode before committing

### After Implementation
1. Check Console for errors/warnings
2. Verify in both Edit Mode and Play Mode
3. Test edge cases (null references, boundaries)

## Folder Structure
```
Assets/
├── Animations/    # Animation controllers, animator assets
├── Art/          # Sprites, textures
├── Audio/        # Sound effects, music
├── Data/         # ScriptableObjects
├── Prefabs/      # Reusable game objects
├── Scenes/       # Unity scenes (.unity files)
├── Scripts/      # C# source code
├── Settings/     # Project settings
└── UI/           # UI prefabs and scripts
```

## Git Workflow
- Commit meaningful changes frequently
- Use descriptive commit messages: `[FEATURE] Add player dash ability`
- Don't commit `Library/`, `Temp/`, `Logs/`, `UserSettings/`
- Test scene changes before committing
