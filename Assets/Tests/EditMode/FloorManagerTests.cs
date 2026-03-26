using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class FloorManagerTests
{
    private GameObject _floorManagerObj;
    private FloorManager _floorManager;
    private GameObject _dungeonGenObj;
    private DungeonGenerator _dungeonGenerator;
    private FloorConfigSO _floorConfig;
    private GameObject _dummyPrefab;

    [SetUp]
    public void SetUp()
    {
        // Reset singleton
        ResetSingleton();

        _floorConfig = ScriptableObject.CreateInstance<FloorConfigSO>();

        // Create a dummy prefab to avoid NullReferenceException during Instantiate
        _dummyPrefab = new GameObject("DummyPrefab");

        _dungeonGenObj = new GameObject("DungeonGenerator");
        _dungeonGenerator = _dungeonGenObj.AddComponent<DungeonGenerator>();

        // Set required serialized fields so GenerateFloor doesn't crash
        SetSerializedField(_dungeonGenerator, "_roomPrefab", _dummyPrefab);
        SetSerializedField(_dungeonGenerator, "_exitRoomPrefab", _dummyPrefab);

        _floorManagerObj = new GameObject("FloorManager");
        _floorManager = _floorManagerObj.AddComponent<FloorManager>();

        // Set private serialized fields via reflection
        SetPrivateField(_floorManager, "_dungeonGenerator", _dungeonGenerator);
        SetPrivateField(_floorManager, "_floorConfig", _floorConfig);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_floorManagerObj);
        Object.DestroyImmediate(_dungeonGenObj);
        Object.DestroyImmediate(_floorConfig);
        Object.DestroyImmediate(_dummyPrefab);
        ResetSingleton();
    }

    private void ResetSingleton()
    {
        var field = typeof(FloorManager).GetProperty("Instance",
            BindingFlags.Public | BindingFlags.Static);
        field?.SetValue(null, null);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(target, value);
    }

    private static T GetPrivateField<T>(object target, string fieldName)
    {
        FieldInfo field = target.GetType().GetField(fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance);
        return (T)field?.GetValue(target);
    }

    private static void SetSerializedField(DungeonGenerator target, string fieldName, GameObject value)
    {
        FieldInfo field = typeof(DungeonGenerator).GetField(fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(target, value);
    }

    // CurrentFloor

    [Test]
    public void CurrentFloor_InitialState_Is1()
    {
        Assert.AreEqual(1, _floorManager.CurrentFloor);
    }

    // NextFloor - Normal flow

    [Test]
    public void NextFloor_NormalFloor_IncrementsFloor()
    {
        _floorManager.NextFloor();
        Assert.AreEqual(2, _floorManager.CurrentFloor);
    }

    [Test]
    public void NextFloor_CalledMultipleTimes_IncrementsCorrectly()
    {
        _floorManager.NextFloor(); // floor 1 -> 2
        _floorManager.NextFloor(); // floor 2 -> 3
        _floorManager.NextFloor(); // floor 3 -> 4

        Assert.AreEqual(4, _floorManager.CurrentFloor);
    }

    // ResetFloor

    [Test]
    public void ResetFloor_AfterProgress_ResetsTo1()
    {
        _floorManager.NextFloor();
        _floorManager.NextFloor();
        _floorManager.ResetFloor();

        Assert.AreEqual(1, _floorManager.CurrentFloor);
    }

    [Test]
    public void ResetFloor_SetsRewardFloorFalse()
    {
        _floorManager.NextFloor(); // floor 2
        _floorManager.NextFloor(); // floor 3
        _floorManager.ResetFloor();

        bool isReward = GetPrivateField<bool>(_floorManager, "_isRewardFloor");
        Assert.IsFalse(isReward);
    }

    [Test]
    public void ResetFloor_CanAdvanceAfterReset()
    {
        _floorManager.NextFloor();
        _floorManager.NextFloor();
        _floorManager.ResetFloor();
        _floorManager.NextFloor();

        Assert.AreEqual(2, _floorManager.CurrentFloor);
    }

    // IsBossFloor

    [Test]
    public void IsBossFloor_Floor5_ReturnsTrue()
    {
        Assert.IsTrue(_floorManager.IsBossFloor(5));
    }

    [Test]
    public void IsBossFloor_Floor10_ReturnsTrue()
    {
        Assert.IsTrue(_floorManager.IsBossFloor(10));
    }

    [Test]
    public void IsBossFloor_Floor15_ReturnsTrue()
    {
        Assert.IsTrue(_floorManager.IsBossFloor(15));
    }

    [Test]
    public void IsBossFloor_Floor1_ReturnsFalse()
    {
        Assert.IsFalse(_floorManager.IsBossFloor(1));
    }

    [Test]
    public void IsBossFloor_Floor3_ReturnsFalse()
    {
        Assert.IsFalse(_floorManager.IsBossFloor(3));
    }

    [Test]
    public void IsBossFloor_Floor7_ReturnsFalse()
    {
        Assert.IsFalse(_floorManager.IsBossFloor(7));
    }

    // GetCurrentFloorType

    [Test]
    public void GetCurrentFloorType_Floor1_ReturnsNormal()
    {
        Assert.AreEqual(FloorType.Normal, _floorManager.GetCurrentFloorType());
    }

    [Test]
    public void GetCurrentFloorType_BossFloor_ReturnsBoss()
    {
        // Advance to floor 4, then NextFloor will set floor to 5 (boss)
        _floorManager.NextFloor(); // 2
        _floorManager.NextFloor(); // 3
        _floorManager.NextFloor(); // 4
        _floorManager.NextFloor(); // 5 (boss)

        Assert.AreEqual(FloorType.Boss, _floorManager.GetCurrentFloorType());
    }

    [Test]
    public void GetCurrentFloorType_AfterMultipleAdvances_ReturnsCorrectType()
    {
        for (int i = 0; i < 4; i++)
            _floorManager.NextFloor(); // floor 5

        Assert.AreEqual(FloorType.Boss, _floorManager.GetCurrentFloorType());

        // Go past boss
        _floorManager.NextFloor(); // reward
        _floorManager.NextFloor(); // floor 6

        Assert.AreEqual(FloorType.Normal, _floorManager.GetCurrentFloorType());
    }

    // Boss -> Reward flow

    [Test]
    public void NextFloor_OnBossFloor_SetsRewardFlag()
    {
        _floorManager.NextFloor(); // 2
        _floorManager.NextFloor(); // 3
        _floorManager.NextFloor(); // 4
        _floorManager.NextFloor(); // 5 (boss)
        _floorManager.NextFloor(); // reward

        bool isReward = GetPrivateField<bool>(_floorManager, "_isRewardFloor");
        Assert.IsTrue(isReward);
    }

    [Test]
    public void NextFloor_FromRewardFloor_IncrementsAndClearsFlag()
    {
        _floorManager.NextFloor(); // 2
        _floorManager.NextFloor(); // 3
        _floorManager.NextFloor(); // 4
        _floorManager.NextFloor(); // 5 (boss)
        _floorManager.NextFloor(); // reward

        bool isRewardBefore = GetPrivateField<bool>(_floorManager, "_isRewardFloor");
        Assert.IsTrue(isRewardBefore);

        _floorManager.NextFloor(); // floor 6

        Assert.AreEqual(6, _floorManager.CurrentFloor);
        bool isRewardAfter = GetPrivateField<bool>(_floorManager, "_isRewardFloor");
        Assert.IsFalse(isRewardAfter);
    }

    [Test]
    public void NextFloor_BossToRewardToNext_CorrectFloorType()
    {
        // Advance to boss floor
        for (int i = 0; i < 4; i++)
            _floorManager.NextFloor();

        Assert.AreEqual(5, _floorManager.CurrentFloor);
        Assert.AreEqual(FloorType.Boss, _floorManager.GetCurrentFloorType());

        // Boss defeated -> reward
        _floorManager.NextFloor();
        bool isReward = GetPrivateField<bool>(_floorManager, "_isRewardFloor");
        Assert.IsTrue(isReward);

        // Reward -> next floor
        _floorManager.NextFloor();
        Assert.AreEqual(6, _floorManager.CurrentFloor);
        Assert.AreEqual(FloorType.Normal, _floorManager.GetCurrentFloorType());
    }

    // FloorConfig accessor

    [Test]
    public void FloorConfig_ReturnsAssignedConfig()
    {
        Assert.AreSame(_floorConfig, _floorManager.FloorConfig);
    }

    // OnFloorChanged event

    [Test]
    public void NextFloor_FiresOnFloorChanged()
    {
        int eventFloor = 0;
        _floorManager.OnFloorChanged.AddListener((floor) => eventFloor = floor);

        _floorManager.NextFloor();

        // OnFloorChanged fires with _currentFloor which is now 2
        Assert.AreEqual(2, eventFloor);
    }
}
