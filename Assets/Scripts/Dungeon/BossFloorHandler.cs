using UnityEngine;

public class BossFloorHandler : MonoBehaviour
{
    [Header("Boss Settings")]
    [SerializeField] private GameObject _bossPrefab;
    [SerializeField] private GameObject[] _bossPrefabs;
    [SerializeField] private Transform _bossSpawnPoint;
    [SerializeField] private RoomController _roomController;

    private Transform _player;
    private bool _bossDefeated;

    private void OnEnable()
    {
        if (_roomController == null)
        {
            _roomController = GetComponent<RoomController>();
        }
    }

    public void Initialize(Transform player)
    {
        _player = player;
        _bossDefeated = false;
        SpawnBoss();
    }

    private void SpawnBoss()
    {
        if (_bossSpawnPoint == null || _player == null) return;

        GameObject bossPrefabToSpawn = _bossPrefab;

        if (_bossPrefabs != null && _bossPrefabs.Length > 0)
        {
            int floor = FloorManager.Instance != null ? FloorManager.Instance.CurrentFloor : 5;
            int index = Mathf.Clamp((floor / 5) - 1, 0, _bossPrefabs.Length - 1);
            bossPrefabToSpawn = _bossPrefabs[index];
        }

        if (bossPrefabToSpawn == null) return;

        GameObject boss = Instantiate(bossPrefabToSpawn, _bossSpawnPoint.position, Quaternion.identity);

        IBoss bossInterface = boss.GetComponent<IBoss>();
        if (bossInterface != null)
        {
            bossInterface.Initialize(_player);
        }

        EnemyBase eb = boss.GetComponent<EnemyBase>();
        if (eb != null)
        {
            FloorConfigSO config = FloorManager.Instance != null ? FloorManager.Instance.FloorConfig : null;
            if (config != null)
            {
                int floor = FloorManager.Instance.CurrentFloor;
                float hpMult = DifficultyScaler.GetHPMultiplier(floor, config.HpScalingPerFloor) * 3f;
                float dmgMult = DifficultyScaler.GetDamageMultiplier(floor, config.DmgScalingPerFloor) * 2f;
                float runeMult = DifficultyScaler.GetRuneMultiplier(floor, config.HpScalingPerFloor) * 5f;
                eb.ApplyDifficulty(hpMult, dmgMult, runeMult);
            }
            else
            {
                eb.ApplyDifficulty(3f, 2f, 5f);
            }
        }

        EnemyDeathTracker tracker = boss.AddComponent<EnemyDeathTracker>();
        tracker.Initialize(_roomController != null ? _roomController : GetComponent<RoomController>());

        if (_roomController != null)
        {
            _roomController.IncrementEnemiesAlive();
        }
    }

    private void Update()
    {
        if (_bossDefeated) return;
        if (_roomController == null) return;

        if (_roomController.IsCleared)
        {
            OnBossDefeated();
        }
    }

    public void OnBossDefeated()
    {
        if (_bossDefeated) return;
        _bossDefeated = true;

        if (_player != null)
        {
            PlayerHealth ph = _player.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.HealToFull();
            }
        }

        if (FloorManager.Instance != null)
        {
            FloorManager.Instance.NextFloor();
        }
    }
}
