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
    [SerializeField] private Transform _roomCenter;
    
    private int _enemiesAlive;
    private bool _isActive;
    private bool _doorsClosed;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isActive) return;
        if (other.gameObject.layer != LayerMask.NameToLayer("player")) return;
        
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
        if (_spawnPoints == null || _enemyPrefab == null) return;
        
        foreach (Transform point in _spawnPoints)
        {
            GameObject enemy = Instantiate(_enemyPrefab, point.position, Quaternion.identity);
            
            var controller = enemy.GetComponent<EnemyController>();
            if (controller != null)
            {
                controller.Initialize(_player);
            }
            
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
