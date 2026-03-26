using NUnit.Framework;
using UnityEngine;

public class FloorConfigSO_Tests
{
    private FloorConfigSO _config;

    [SetUp]
    public void SetUp()
    {
        _config = ScriptableObject.CreateInstance<FloorConfigSO>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_config);
    }

    // IsBossFloor

    [Test]
    public void IsBossFloor_Floor5_ReturnsTrue()
    {
        Assert.IsTrue(_config.IsBossFloor(5));
    }

    [Test]
    public void IsBossFloor_Floor10_ReturnsTrue()
    {
        Assert.IsTrue(_config.IsBossFloor(10));
    }

    [Test]
    public void IsBossFloor_Floor15_ReturnsTrue()
    {
        Assert.IsTrue(_config.IsBossFloor(15));
    }

    [Test]
    public void IsBossFloor_Floor1_ReturnsFalse()
    {
        Assert.IsFalse(_config.IsBossFloor(1));
    }

    [Test]
    public void IsBossFloor_Floor3_ReturnsFalse()
    {
        Assert.IsFalse(_config.IsBossFloor(3));
    }

    [Test]
    public void IsBossFloor_Floor7_ReturnsFalse()
    {
        Assert.IsFalse(_config.IsBossFloor(7));
    }

    [Test]
    public void IsBossFloor_Floor0_ReturnsFalse()
    {
        Assert.IsFalse(_config.IsBossFloor(0));
    }

    [Test]
    public void IsBossFloor_Floor50_ReturnsFalse()
    {
        Assert.IsFalse(_config.IsBossFloor(50));
    }

    [TestCase(5, true)]
    [TestCase(10, true)]
    [TestCase(15, true)]
    [TestCase(1, false)]
    [TestCase(2, false)]
    [TestCase(3, false)]
    [TestCase(4, false)]
    [TestCase(6, false)]
    [TestCase(9, false)]
    [TestCase(11, false)]
    [TestCase(20, false)]
    public void IsBossFloor_Parameterized(int floor, bool expected)
    {
        Assert.AreEqual(expected, _config.IsBossFloor(floor));
    }

    // GetRoomCount

    [Test]
    public void GetRoomCount_Floor1_ReturnsBaseRoomCount()
    {
        int count = _config.GetRoomCount(1);
        Assert.AreEqual(4, count);
    }

    [Test]
    public void GetRoomCount_Floor3_IncreasesRoomCount()
    {
        int count = _config.GetRoomCount(3);
        Assert.AreEqual(5, count);
    }

    [Test]
    public void GetRoomCount_HighFloor_ClampedToMax()
    {
        int count = _config.GetRoomCount(100);
        Assert.AreEqual(8, count);
    }

    [Test]
    public void GetRoomCount_Floor0_ReturnsBaseRoomCount()
    {
        int count = _config.GetRoomCount(0);
        Assert.AreEqual(4, count);
    }

    // GetRoomWidth

    [Test]
    public void GetRoomWidth_AnyFloor_ReturnsBaseRoomWidth()
    {
        float width = _config.GetRoomWidth(1);
        Assert.AreEqual(20f, width);
    }

    [Test]
    public void GetRoomWidth_HighFloor_StillReturnsBaseWidth()
    {
        float width = _config.GetRoomWidth(50);
        Assert.AreEqual(20f, width);
    }
}
