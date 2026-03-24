using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class FloorManager : MonoBehaviour
{
    public static FloorManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private DungeonGenerator _dungeonGenerator;
    [SerializeField] private FloorUI _floorUI;
    [SerializeField] private Transform _player;
    [SerializeField] private FloorConfigSO _floorConfig;

    private int _currentFloor = 1;
    private bool _isRewardFloor;

    public int CurrentFloor => _currentFloor;
    public FloorConfigSO FloorConfig => _floorConfig;

    public UnityEvent<int> OnFloorChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _dungeonGenerator.GenerateFloor(_currentFloor);
        UpdateUI();
    }

    private void Update()
    {
        // DEBUG: N = pular andar
        if (Keyboard.current != null && Keyboard.current.nKey.wasPressedThisFrame)
        {
            NextFloor();
        }
    }

    public void NextFloor()
    {
        if (_isRewardFloor)
        {
            // Saindo da RewardRoom → próximo andar normal
            _isRewardFloor = false;
            _currentFloor++;
            GenerateNextFloor();
        }
        else if (_floorConfig.IsBossFloor(_currentFloor))
        {
            // Boss derrotado → RewardRoom (andar de recompensa)
            _isRewardFloor = true;
            _dungeonGenerator.GenerateRewardFloor();
        }
        else
        {
            // Andar normal → próximo andar
            _currentFloor++;
            GenerateNextFloor();
        }

        UpdateUI();
        OnFloorChanged?.Invoke(_currentFloor);
    }

    private void GenerateNextFloor()
    {
        if (_floorConfig.IsBossFloor(_currentFloor))
        {
            _dungeonGenerator.GenerateBossFloor(_currentFloor);
        }
        else
        {
            _dungeonGenerator.GenerateFloor(_currentFloor);
        }
    }

    public void ResetFloor()
    {
        _currentFloor = 1;
        _isRewardFloor = false;
        _dungeonGenerator.ClearFloor();
    }

    public FloorType GetCurrentFloorType()
    {
        if (_floorConfig.IsBossFloor(_currentFloor))
            return FloorType.Boss;
        return FloorType.Normal;
    }

    public bool IsBossFloor(int floor)
    {
        return _floorConfig.IsBossFloor(floor);
    }

    private void UpdateUI()
    {
        if (_floorUI != null)
        {
            _floorUI.UpdateFloor(_currentFloor);
        }
    }
}
