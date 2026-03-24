using UnityEngine;

public class RoomController : MonoBehaviour
{
    [Header("Room Settings")]
    [SerializeField] private RoomType _roomType = RoomType.Combat;

    [Header("Spawn Settings")]
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private GameObject _enemyPrefab;

    [Header("Elite Settings")]
    [SerializeField] private Transform _eliteSpawnPoint;
    [SerializeField] private GameObject _eliteEnemyPrefab;

    [Header("Doors")]
    [SerializeField] private Door[] _doors;

    [Header("References")]
    [SerializeField] private Transform _player;
    [SerializeField] private Transform _roomCenter;

    private int _enemiesAlive;
    private bool _isActive;
    private bool _doorsClosed;
    private bool _isEliteMode;

    public bool IsCleared => _enemiesAlive <= 0;
    public RoomType RoomType => _roomType;

    public void SetRoomType(RoomType type)
    {
        _roomType = type;
    }

    public void SetEliteMode(bool isElite)
    {
        _isEliteMode = isElite;
    }

    public void InitializeReferences(Transform player, GameObject enemyPrefab)
    {
        _player = player;
        if (enemyPrefab != null)
        {
            _enemyPrefab = enemyPrefab;
        }
    }

    public void SetEnemyPrefab(GameObject prefab)
    {
        _enemyPrefab = prefab;
    }

    public void InitializeDoors(Door[] doors)
    {
        _doors = doors;
    }

    public void SetActiveDoors(Door[] activeDoors)
    {
        _doors = activeDoors;
    }

    public Door[] GetDoors()
    {
        return _doors;
    }

    public GameObject GetEnemyPrefab()
    {
        return _enemyPrefab;
    }

    public void IncrementEnemiesAlive()
    {
        _enemiesAlive++;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isActive) return;
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        _isActive = true;
        SpawnEnemies();
    }

    private void Update()
    {
        if (!_isActive || _doorsClosed) return;
        if (_player == null || _roomCenter == null) return;

        if (Vector2.Distance(_player.position, _roomCenter.position) < 1f)
        {
            _doorsClosed = true;
            CloseDoors();
        }
    }

    private void SpawnEnemies()
    {
        switch (_roomType)
        {
            case RoomType.Reward:
            case RoomType.Rest:
            case RoomType.Shop:
                OpenDoors();
                break;

            case RoomType.Elite:
                SpawnElite();
                break;

            case RoomType.Event:
                OpenDoors();
                break;

            case RoomType.Combat:
            case RoomType.Boss:
            default:
                SpawnCombatEnemies();
                break;
        }
    }

    private void SpawnCombatEnemies()
    {
        if (_spawnPoints == null || _enemyPrefab == null) return;

        foreach (Transform point in _spawnPoints)
        {
            GameObject enemy = Instantiate(_enemyPrefab, point.position, Quaternion.identity);

            var controller = enemy.GetComponent<EnemyController>();
            if (controller != null)
            {
                controller.Initialize(_player);

                if (FloorManager.Instance != null && FloorManager.Instance.FloorConfig != null)
                {
                    int floor = FloorManager.Instance.CurrentFloor;
                    var config = FloorManager.Instance.FloorConfig;
                    float hpMult = DifficultyScaler.GetHPMultiplier(floor, config.HpScalingPerFloor);
                    float dmgMult = DifficultyScaler.GetDamageMultiplier(floor, config.DmgScalingPerFloor);
                    float runeMult = DifficultyScaler.GetRuneMultiplier(floor, config.HpScalingPerFloor);
                    controller.ApplyDifficulty(hpMult, dmgMult, runeMult);
                }
            }

            var tracker = enemy.AddComponent<EnemyDeathTracker>();
            tracker.Initialize(this);

            _enemiesAlive++;
        }
    }

    private void SpawnElite()
    {
        if (_eliteSpawnPoint != null && _eliteEnemyPrefab != null)
        {
            GameObject elite = Instantiate(_eliteEnemyPrefab, _eliteSpawnPoint.position, Quaternion.identity);

            var controller = elite.GetComponent<EnemyController>();
            if (controller != null)
            {
                controller.Initialize(_player);
            }

            var tracker = elite.AddComponent<EnemyDeathTracker>();
            tracker.Initialize(this);

            _enemiesAlive++;
        }
        else
        {
            SpawnCombatEnemies();
        }
    }

    public void OnEnemyKilled()
    {
        _enemiesAlive--;

        if (_enemiesAlive <= 0)
        {
            _isActive = false;
            _doorsClosed = false;
            OpenDoors();
        }
    }

    private void CloseDoors()
    {
        if (_doors == null) return;
        foreach (Door door in _doors)
        {
            door.Close();
        }
    }

    private void OpenDoors()
    {
        if (_doors == null) return;
        foreach (Door door in _doors)
        {
            door.Open();
        }
    }
}
