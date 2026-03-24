using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon/Floor Config")]
public class FloorConfigSO : ScriptableObject
{
    [Header("Room Count")]
    [SerializeField] private int _baseRoomCount = 4;
    [SerializeField] private int _maxRoomCount = 8;

    [Header("Room Size")]
    [SerializeField] private float _baseRoomWidth = 20f;
    [SerializeField] private float _roomWidthScalingPerFloor = 0.05f;

    [Header("Boss")]
    [SerializeField] private int[] _bossFloors = { 5, 10, 15 };

    [Header("Difficulty Scaling")]
    [SerializeField] private float _hpScalingPerFloor = 0.1f;
    [SerializeField] private float _dmgScalingPerFloor = 0.08f;

    [Header("Traps")]
    [SerializeField] private float _trapChance = 0.3f;

    public int BaseRoomCount => _baseRoomCount;
    public int MaxRoomCount => _maxRoomCount;
    public float BaseRoomWidth => _baseRoomWidth;
    public float RoomWidthScalingPerFloor => _roomWidthScalingPerFloor;
    public int[] BossFloors => _bossFloors;
    public float HpScalingPerFloor => _hpScalingPerFloor;
    public float DmgScalingPerFloor => _dmgScalingPerFloor;
    public float TrapChance => _trapChance;

    public float GetRoomWidth(int floor)
    {
        return _baseRoomWidth;
    }

    public int GetRoomCount(int floor)
    {
        int count = _baseRoomCount + (floor / 3);
        return Mathf.Min(count, _maxRoomCount);
    }

    public bool IsBossFloor(int floor)
    {
        if (_bossFloors == null) return false;
        for (int i = 0; i < _bossFloors.Length; i++)
        {
            if (_bossFloors[i] == floor) return true;
        }
        return false;
    }
}
