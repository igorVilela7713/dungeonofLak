using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Room Prefabs")]
    [SerializeField] private GameObject _roomPrefab;
    [SerializeField] private GameObject _exitRoomPrefab;
    [SerializeField] private GameObject[] _bossArenaPrefabs;
    [SerializeField] private GameObject _rewardRoomPrefab;

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private GameObject _eliteEnemyPrefab;

    [Header("Trap Prefabs")]
    [SerializeField] private GameObject[] _trapPrefabs;

    [Header("Pickup Prefabs")]
    [SerializeField] private GameObject _runePickupPrefab;
    [SerializeField] private GameObject _healingShrinePrefab;
    [SerializeField] private GameObject _riskRewardAltarPrefab;

    [Header("References")]
    [SerializeField] private Transform _player;

    [Header("Generation")]
    [SerializeField] private int _seed;
    [SerializeField] private float _roomWidth = 20f;

    private System.Random _rng;
    private readonly List<GameObject> _generatedRooms = new List<GameObject>();
    private FloorConfigSO _floorConfig;

    public GameObject RunePickupPrefab => _runePickupPrefab;
    public GameObject EliteEnemyPrefab => _eliteEnemyPrefab;

    private void Awake()
    {
        if (_player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null)
            {
                PlayerController pc = FindFirstObjectByType<PlayerController>();
                if (pc != null) _player = pc.transform;
            }
            else
            {
                _player = playerObj.transform;
            }
        }
    }

    public void SetFloorConfig(FloorConfigSO config)
    {
        _floorConfig = config;
    }

    public void GenerateFloor(int floorNumber)
    {
        ClearFloor();

        if (_floorConfig == null && FloorManager.Instance != null)
        {
            _floorConfig = FloorManager.Instance.FloorConfig;
        }

        int seed = _seed == 0
            ? UnityEngine.Random.Range(int.MinValue, int.MaxValue)
            : _seed + floorNumber;
        _rng = new System.Random(seed);

        float currentRoomWidth = _roomWidth;
        if (_floorConfig != null)
        {
            currentRoomWidth = _floorConfig.GetRoomWidth(floorNumber);
        }

        int roomCount = 4;
        if (_floorConfig != null)
        {
            roomCount = _floorConfig.GetRoomCount(floorNumber);
        }

        // Geração em linha reta horizontal — salas lado a lado, sem gaps
        for (int i = 0; i < roomCount; i++)
        {
            bool isStart = (i == 0);
            bool isExit = (i == roomCount - 1);

            GameObject prefab = isExit ? _exitRoomPrefab : _roomPrefab;
            Vector3 worldPos = new Vector3(i * currentRoomWidth, 0f, 0f);

            GameObject room = Instantiate(prefab, worldPos, Quaternion.identity);
            _generatedRooms.Add(room);

            RoomController rc = room.GetComponent<RoomController>();
            if (rc != null)
            {
                if (!isStart && !isExit)
                {
                    RoomType type = GetRandomRoomType();
                    rc.InitializeReferences(_player, null);
                    rc.SetRoomType(type);

                    if (type == RoomType.Elite && _eliteEnemyPrefab != null)
                    {
                        rc.SetEnemyPrefab(_eliteEnemyPrefab);
                        rc.SetEliteMode(true);
                    }
                    else if (_enemyPrefab != null)
                    {
                        rc.SetEnemyPrefab(_enemyPrefab);
                    }
                }
                else
                {
                    rc.InitializeReferences(_player, null);
                    rc.SetRoomType(RoomType.Rest);
                }

                ConfigureDoorsForRoom(rc, i, roomCount);
            }

            if (isStart && _player != null)
            {
                _player.position = worldPos + new Vector3(0f, 1f, 0f);
            }

            if (!isStart && !isExit && _trapPrefabs != null && _trapPrefabs.Length > 0)
            {
                float trapChance = _floorConfig != null ? _floorConfig.TrapChance : 0.3f;
                if (_rng.NextDouble() < trapChance)
                {
                    SpawnTrapInRoom(room, worldPos);
                }
            }
        }
    }

    public void GenerateBossFloor(int floorNumber)
    {
        ClearFloor();

        if (_bossArenaPrefabs == null || _bossArenaPrefabs.Length == 0) return;

        int index = (floorNumber / 5) - 1;
        index = Mathf.Clamp(index, 0, _bossArenaPrefabs.Length - 1);

        GameObject arena = Instantiate(_bossArenaPrefabs[index], Vector3.zero, Quaternion.identity);
        _generatedRooms.Add(arena);

        BossFloorHandler handler = arena.GetComponent<BossFloorHandler>();
        if (handler != null)
        {
            handler.Initialize(_player);
        }

        Transform playerSpawn = arena.transform.Find("PlayerSpawnPoint");
        if (playerSpawn != null && _player != null)
        {
            _player.position = playerSpawn.position;
        }
    }

    public void GenerateRewardFloor()
    {
        ClearFloor();

        if (_rewardRoomPrefab == null) return;

        GameObject reward = Instantiate(_rewardRoomPrefab, Vector3.zero, Quaternion.identity);
        _generatedRooms.Add(reward);

        RewardArea area = reward.GetComponent<RewardArea>();
        if (area != null)
        {
            area.Initialize(_player);
        }

        Transform playerSpawn = reward.transform.Find("PlayerSpawnPoint");
        if (playerSpawn != null && _player != null)
        {
            _player.position = playerSpawn.position;
        }
    }

    public void ClearFloor()
    {
        for (int i = _generatedRooms.Count - 1; i >= 0; i--)
        {
            if (_generatedRooms[i] != null)
            {
                Destroy(_generatedRooms[i]);
            }
        }
        _generatedRooms.Clear();
    }

    private RoomType GetRandomRoomType()
    {
        int roll = _rng.Next(100);
        if (roll < 45) return RoomType.Combat;
        if (roll < 60) return RoomType.Elite;
        if (roll < 70) return RoomType.Reward;
        if (roll < 80) return RoomType.Rest;
        if (roll < 90) return RoomType.Event;
        if (roll < 95) return RoomType.Shop;
        return RoomType.Rest;
    }

    private void ConfigureDoorsForRoom(RoomController rc, int roomIndex, int roomCount)
    {
        Door[] allDoors = rc.GetDoors();
        if (allDoors == null || allDoors.Length == 0) return;

        bool hasLeft = roomIndex > 0;
        bool hasRight = roomIndex < roomCount - 1;

        int doorIndex = 0;
        if (doorIndex < allDoors.Length)
        {
            if (!hasLeft)
            {
                allDoors[doorIndex].Close();
            }
            doorIndex++;
        }

        if (doorIndex < allDoors.Length)
        {
            if (!hasRight)
            {
                allDoors[doorIndex].Close();
            }
        }

        // Atualizar lista de portas ativas (apenas as que existem)
        List<Door> activeDoors = new List<Door>();
        doorIndex = 0;
        if (doorIndex < allDoors.Length && hasLeft)
        {
            activeDoors.Add(allDoors[doorIndex]);
            doorIndex++;
        }
        if (doorIndex < allDoors.Length && hasRight)
        {
            activeDoors.Add(allDoors[doorIndex]);
        }

        rc.SetActiveDoors(activeDoors.ToArray());
    }

    private void SpawnTrapInRoom(GameObject room, Vector3 roomWorldPos)
    {
        if (_trapPrefabs == null || _trapPrefabs.Length == 0) return;

        int trapIndex = _rng.Next(_trapPrefabs.Length);
        float offsetX = (float)(_rng.NextDouble() * 6f - 3f);

        // Posicionar como filho do room — usar localPosition
        GameObject trap = Instantiate(_trapPrefabs[trapIndex], room.transform);
        trap.transform.localPosition = new Vector3(offsetX, -3f, 0f);
    }
}
