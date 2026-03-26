using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class DungeonGeneratorTests
{
    private GameObject _generatorObj;
    private DungeonGenerator _generator;

    [SetUp]
    public void SetUp()
    {
        _generatorObj = new GameObject("DungeonGenerator");
        _generator = _generatorObj.AddComponent<DungeonGenerator>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_generatorObj);
    }

    // Helper: invoke private GetEnemyPrefabsForFloor via reflection
    private GameObject[] InvokeGetEnemyPrefabsForFloor(int floor)
    {
        MethodInfo method = typeof(DungeonGenerator).GetMethod("GetEnemyPrefabsForFloor",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(method, "GetEnemyPrefabsForFloor method not found");
        return (GameObject[])method.Invoke(_generator, new object[] { floor });
    }

    // Helper: set private serialized field
    private static void SetSerializedField(DungeonGenerator target, string fieldName, GameObject value)
    {
        FieldInfo field = typeof(DungeonGenerator).GetField(fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(target, value);
    }

    private static void SetSerializedArrayField(DungeonGenerator target, string fieldName, GameObject[] value)
    {
        FieldInfo field = typeof(DungeonGenerator).GetField(fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(target, value);
    }

    private void SetupEnemyPrefabs()
    {
        SetSerializedField(_generator, "_enemyPrefab", new GameObject("EnemyBasic"));
        SetSerializedField(_generator, "_enemyFastPrefab", new GameObject("EnemyFast"));
        SetSerializedField(_generator, "_enemyHeavyPrefab", new GameObject("EnemyHeavy"));
        SetSerializedField(_generator, "_enemyRangedPrefab", new GameObject("EnemyRanged"));
    }

    // GetEnemyPrefabsForFloor - Basic enemy availability

    [Test]
    public void GetEnemyPrefabsForFloor_Floor1_ReturnsOnlyBasicEnemy()
    {
        SetupEnemyPrefabs();
        GameObject[] result = InvokeGetEnemyPrefabsForFloor(1);

        Assert.AreEqual(1, result.Length);
        Assert.AreEqual("EnemyBasic", result[0].name);
    }

    [Test]
    public void GetEnemyPrefabsForFloor_Floor2_ReturnsOnlyBasicEnemy()
    {
        SetupEnemyPrefabs();
        GameObject[] result = InvokeGetEnemyPrefabsForFloor(2);

        Assert.AreEqual(1, result.Length);
        Assert.AreEqual("EnemyBasic", result[0].name);
    }

    [Test]
    public void GetEnemyPrefabsForFloor_Floor3_ReturnsBasicAndFast()
    {
        SetupEnemyPrefabs();
        GameObject[] result = InvokeGetEnemyPrefabsForFloor(3);

        Assert.AreEqual(2, result.Length);
    }

    [Test]
    public void GetEnemyPrefabsForFloor_Floor4_ReturnsBasicAndFast()
    {
        SetupEnemyPrefabs();
        GameObject[] result = InvokeGetEnemyPrefabsForFloor(4);

        Assert.AreEqual(2, result.Length);
    }

    [Test]
    public void GetEnemyPrefabsForFloor_Floor5_ReturnsBasicFastHeavy()
    {
        SetupEnemyPrefabs();
        GameObject[] result = InvokeGetEnemyPrefabsForFloor(5);

        Assert.AreEqual(3, result.Length);
    }

    [Test]
    public void GetEnemyPrefabsForFloor_Floor6_ReturnsBasicFastHeavy()
    {
        SetupEnemyPrefabs();
        GameObject[] result = InvokeGetEnemyPrefabsForFloor(6);

        Assert.AreEqual(3, result.Length);
    }

    [Test]
    public void GetEnemyPrefabsForFloor_Floor7_ReturnsAllFourTypes()
    {
        SetupEnemyPrefabs();
        GameObject[] result = InvokeGetEnemyPrefabsForFloor(7);

        Assert.AreEqual(4, result.Length);
    }

    [Test]
    public void GetEnemyPrefabsForFloor_Floor50_ReturnsAllFourTypes()
    {
        SetupEnemyPrefabs();
        GameObject[] result = InvokeGetEnemyPrefabsForFloor(50);

        Assert.AreEqual(4, result.Length);
    }

    // No null prefabs in result

    [Test]
    public void GetEnemyPrefabsForFloor_NoNullPrefabs()
    {
        SetupEnemyPrefabs();
        GameObject[] result = InvokeGetEnemyPrefabsForFloor(10);

        for (int i = 0; i < result.Length; i++)
        {
            Assert.IsNotNull(result[i], $"Prefab at index {i} should not be null");
        }
    }

    // Missing prefabs handling

    [Test]
    public void GetEnemyPrefabsForFloor_OnlyBasicPrefab_ReturnsOne()
    {
        SetSerializedField(_generator, "_enemyPrefab", new GameObject("EnemyBasic"));

        GameObject[] result = InvokeGetEnemyPrefabsForFloor(10);

        Assert.AreEqual(1, result.Length);
    }

    [Test]
    public void GetEnemyPrefabsForFloor_NoPrefabs_ReturnsEmpty()
    {
        // Don't set any prefabs
        GameObject[] result = InvokeGetEnemyPrefabsForFloor(10);

        Assert.AreEqual(0, result.Length);
    }

    [Test]
    public void GetEnemyPrefabsForFloor_OnlyFastPrefab_ReturnsEmpty()
    {
        SetSerializedField(_generator, "_enemyFastPrefab", new GameObject("EnemyFast"));

        // Fast prefab requires floor >= 3, but basic is null
        GameObject[] result = InvokeGetEnemyPrefabsForFloor(3);

        Assert.AreEqual(1, result.Length);
    }

    [Test]
    public void GetEnemyPrefabsForFloor_Floor2_WithAllPrefabs_ReturnsOne()
    {
        SetupEnemyPrefabs();
        GameObject[] result = InvokeGetEnemyPrefabsForFloor(2);

        // Floor 2: only basic enemy (floor < 3 for fast)
        Assert.AreEqual(1, result.Length);
        Assert.AreEqual("EnemyBasic", result[0].name);
    }

    // Consistency

    [Test]
    public void GetEnemyPrefabsForFloor_NeverShrinks_WhenFloorIncreases()
    {
        SetupEnemyPrefabs();

        int previousCount = InvokeGetEnemyPrefabsForFloor(1).Length;

        for (int floor = 2; floor <= 10; floor++)
        {
            int currentCount = InvokeGetEnemyPrefabsForFloor(floor).Length;
            Assert.GreaterOrEqual(currentCount, previousCount,
                $"Enemy pool should not shrink from floor {floor - 1} to {floor}");
            previousCount = currentCount;
        }
    }

    // ClearFloor

    [Test]
    public void ClearFloor_DoesNotThrow_WhenNoRoomsGenerated()
    {
        Assert.DoesNotThrow(() => _generator.ClearFloor());
    }
}
