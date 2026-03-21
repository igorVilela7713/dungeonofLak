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
