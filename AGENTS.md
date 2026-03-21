# Agent Guidelines - Unity Roguelike

## Project Overview
- **Engine**: Unity 6000.3.11f1 (2D) with Universal Render Pipeline (URP)
- **Language**: C#
- **Input**: Unity Input System (code-based `InputAction` bindings, no Input Actions asset)
- **Type**: 2D roguelike with rooms, combat, and enemy AI
- **Tests**: No test assemblies exist yet. Use Unity Test Framework (`com.unity.test-framework 1.6.0`) when adding tests via `Window > General > Test Runner`.

## Build & Test Commands
- **Open project**: Unity Hub
- **Build Settings**: `Ctrl+Shift+B`
- **Build and Run**: `Ctrl+B`
- **Run tests** (when added): Test Runner window (`Window > General > Test Runner`) or CLI:
  ```
  Unity.exe -runTests -projectPath "C:\Users\igor7\My project" -testResults results.xml
  ```
- **No CLI lint/format tool** is configured. Use Visual Studio / Rider analyzers. Run a manual code review pass before committing.

## Project Structure
```
Assets/Scripts/
  Core/           GameManager (scene reload on death)
  Combat/         IDamageable, PlayerHealth, WeaponController, SwordHitbox
  Movement/       PlayerController (2D physics, Input System)
  Enemies/        EnemyController (chase AI, implements IDamageable)
  Rooms/          RoomController, Door, EnemyDeathTracker

Assets/
  Animations/     Animator controllers
  Art/            Sprites, textures
  Audio/          SFX, music
  Data/           ScriptableObjects
  Prefabs/        Reusable GameObjects
  Scenes/         .unity files
  Settings/       Project settings
  UI/             UI prefabs and scripts
```

## Code Style

### Naming Conventions
| Element | Convention | Example |
|---------|------------|---------|
| Classes / MonoBehaviours | PascalCase | `PlayerController` |
| Interfaces | IPascalCase | `IDamageable` |
| Methods / Properties | PascalCase | `TakeDamage()`, `MaxHealth` |
| Public fields | PascalCase | `OnDeath` |
| Private fields | _camelCase | `_currentHealth` |
| Constants | PascalCase | `MaxSpeed` |
| Enum values | PascalCase | `EnemyState.Chasing` |

> Note: This codebase does **not** use namespaces. Do not add them unless refactoring the entire project to adopt them.

### Import Order
1. `System` namespaces
2. `UnityEngine` namespaces (including `UnityEngine.InputSystem`)
3. `UnityEngine.UI` (if used)
4. Other Unity packages
5. Blank line separator before type declarations

### Formatting
- Allman-style braces (opening brace on its own line)
- 4-space indentation
- One blank line between methods
- `[Header("Section")]` groups serialized fields in the Inspector
- Use `[SerializeField] private` over `public` for exposed fields

### MonoBehaviour Pattern
```csharp
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 5f;

    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()  { /* subscribe */ }
    private void OnDisable() { /* unsubscribe */ }
}
```

### Input System
- Create `InputAction` instances in `Awake()` with code-based bindings (no `.inputactions` asset).
- Enable/disable in `OnEnable()`/`OnDisable()` and subscribe/unsubscribe events there.
- See `PlayerController.cs` and `WeaponController.cs` for examples.

### Interfaces
- Prefix with `I`: `IDamageable`
- Keep minimal: `IDamageable` only defines `void TakeDamage(int amount)`.
- Use `GetComponent<IDamageable>()` to decouple damage logic from concrete types.

### Serialization
- `[SerializeField] private` for Inspector-exposed fields
- `[Header("...")]` to group sections
- `[Range(min, max)]` for clamped floats/ints
- `[RequireComponent(typeof(T))]` when a component dependency is mandatory

### Error Handling
```csharp
Debug.LogWarning("Enemy not found in scene");   // recoverable
if (target == null) { Debug.LogError("..."); return; }  // critical
throw new System.InvalidOperationException("...");      // rare, in systems code
```

### Performance
1. Cache `GetComponent` results in `Awake()`
2. Avoid `Find` / `FindObjectOfType` in `Update()`
3. Use object pooling for frequently spawned objects
4. `[DisallowMultipleComponent]` to prevent duplicate scripts
5. Remove empty lifecycle methods (`Start`, `Update`)

### Coroutines
```csharp
private IEnumerator AttackRoutine()
{
    yield return new WaitForSeconds(_duration);
    _isAttacking = false;
}
```

## Git Workflow
- Commit frequently with prefix tags: `[FEATURE]`, `[FIX]`, `[REFACTOR]`
- Never commit: `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `*.csproj`, `*.sln`
- Test in Play Mode before committing scene or prefab changes
