using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class RunUpgradeManagerTests
{
    private GameObject _managerObj;
    private RunUpgradeManager _manager;

    [SetUp]
    public void SetUp()
    {
        // Reset singleton
        ResetSingleton();

        _managerObj = new GameObject("RunUpgradeManager");
        _manager = _managerObj.AddComponent<RunUpgradeManager>();
        _ = RunUpgradeManager.Instance; // trigger Awake
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_managerObj);
        ResetSingleton();
    }

    private void ResetSingleton()
    {
        FieldInfo field = typeof(RunUpgradeManager).GetField("_damageMultiplier",
            BindingFlags.NonPublic | BindingFlags.Instance);
        // Also reset static Instance
        var prop = typeof(RunUpgradeManager).GetProperty("Instance",
            BindingFlags.Public | BindingFlags.Static);
        prop?.SetValue(null, null);
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

    private RunUpgradeSO CreateUpgrade(string name, UpgradeType type, float value, int cost = 0)
    {
        RunUpgradeSO upgrade = ScriptableObject.CreateInstance<RunUpgradeSO>();
        upgrade.upgradeName = name;
        upgrade.upgradeType = type;
        upgrade.value = value;
        upgrade.cost = cost;
        upgrade.hasTradeOff = false;
        return upgrade;
    }

    private RunUpgradeSO CreateUpgradeWithTradeOff(string name,
        UpgradeType type, float value,
        UpgradeType tradeOffType, float tradeOffValue, int cost = 0)
    {
        RunUpgradeSO upgrade = ScriptableObject.CreateInstance<RunUpgradeSO>();
        upgrade.upgradeName = name;
        upgrade.upgradeType = type;
        upgrade.value = value;
        upgrade.cost = cost;
        upgrade.hasTradeOff = true;
        upgrade.tradeOffType = tradeOffType;
        upgrade.tradeOffValue = tradeOffValue;
        return upgrade;
    }

    // GetDamageMultiplier - Initial

    [Test]
    public void GetDamageMultiplier_Initial_Is1()
    {
        Assert.AreEqual(1f, _manager.GetDamageMultiplier());
    }

    // ApplyUpgrade - Damage

    [Test]
    public void ApplyUpgrade_Damage_MultipliesCorrectly()
    {
        RunUpgradeSO upgrade = CreateUpgrade("DmgUp", UpgradeType.Damage, 1.5f);
        _manager.ApplyUpgrade(upgrade);

        Assert.AreEqual(1.5f, _manager.GetDamageMultiplier(), 0.001f);
    }

    [Test]
    public void ApplyUpgrade_MultipleDamageUpgrades_StacksMultiplicatively()
    {
        RunUpgradeSO upgrade1 = CreateUpgrade("DmgUp1", UpgradeType.Damage, 1.5f);
        RunUpgradeSO upgrade2 = CreateUpgrade("DmgUp2", UpgradeType.Damage, 1.3f);

        _manager.ApplyUpgrade(upgrade1);
        _manager.ApplyUpgrade(upgrade2);

        float expected = 1.5f * 1.3f;
        Assert.AreEqual(expected, _manager.GetDamageMultiplier(), 0.001f);
    }

    // ApplyUpgrade - Null

    [Test]
    public void ApplyUpgrade_Null_ReturnsFalse()
    {
        bool result = _manager.ApplyUpgrade(null);
        Assert.IsFalse(result);
    }

    // ApplyUpgrade - TradeOff

    [Test]
    public void ApplyUpgrade_WithTradeOff_AppliesBothModifiers()
    {
        // Speed upgrade with damage tradeoff
        RunUpgradeSO upgrade = CreateUpgradeWithTradeOff(
            "SpeedUp", UpgradeType.Speed, 1.5f,
            UpgradeType.Damage, 0.8f);

        // Create a mock player controller
        GameObject playerObj = new GameObject("Player");
        MockPlayerController mockPC = playerObj.AddComponent<MockPlayerController>();

        SetPrivateField(_manager, "_playerController", mockPC);

        bool result = _manager.ApplyUpgrade(upgrade);

        Assert.IsTrue(result);
        // TradeOff of Damage 0.8 should multiply damage: 1.0 * 0.8 = 0.8
        Assert.AreEqual(0.8f, _manager.GetDamageMultiplier(), 0.001f);

        Object.DestroyImmediate(playerObj);
    }

    // ResetUpgrades

    [Test]
    public void ResetUpgrades_ResetsDamageMultiplier()
    {
        RunUpgradeSO upgrade = CreateUpgrade("DmgUp", UpgradeType.Damage, 2f);
        _manager.ApplyUpgrade(upgrade);
        _manager.ResetUpgrades();

        Assert.AreEqual(1f, _manager.GetDamageMultiplier(), 0.001f);
    }

    [Test]
    public void ResetUpgrades_ClearsAppliedList()
    {
        RunUpgradeSO upgrade1 = CreateUpgrade("DmgUp1", UpgradeType.Damage, 1.5f);
        RunUpgradeSO upgrade2 = CreateUpgrade("DmgUp2", UpgradeType.Damage, 1.3f);

        _manager.ApplyUpgrade(upgrade1);
        _manager.ApplyUpgrade(upgrade2);
        _manager.ResetUpgrades();

        var appliedList = GetPrivateField<System.Collections.Generic.List<RunUpgradeSO>>(
            _manager, "_appliedUpgrades");
        Assert.AreEqual(0, appliedList.Count);
    }

    // ApplyUpgrade - Speed

    [Test]
    public void ApplyUpgrade_Speed_SetsPlayerSpeed()
    {
        GameObject playerObj = new GameObject("Player");
        MockPlayerController mockPC = playerObj.AddComponent<MockPlayerController>();
        mockPC.SetBaseSpeed(5f);

        SetPrivateField(_manager, "_playerController", mockPC);
        SetPrivateField(_manager, "_baseMoveSpeed", 5f);

        RunUpgradeSO upgrade = CreateUpgrade("SpeedUp", UpgradeType.Speed, 1.5f);
        _manager.ApplyUpgrade(upgrade);

        Assert.AreEqual(7.5f, mockPC.MoveSpeed, 0.001f);

        Object.DestroyImmediate(playerObj);
    }

    // ResetUpgrades - restores player speed

    [Test]
    public void ResetUpgrades_RestoresPlayerSpeed()
    {
        GameObject playerObj = new GameObject("Player");
        MockPlayerController mockPC = playerObj.AddComponent<MockPlayerController>();
        mockPC.SetBaseSpeed(5f);

        SetPrivateField(_manager, "_playerController", mockPC);
        SetPrivateField(_manager, "_baseMoveSpeed", 5f);

        RunUpgradeSO upgrade = CreateUpgrade("SpeedUp", UpgradeType.Speed, 2f);
        _manager.ApplyUpgrade(upgrade);

        _manager.ResetUpgrades();

        Assert.AreEqual(5f, mockPC.MoveSpeed, 0.001f);

        Object.DestroyImmediate(playerObj);
    }

    // ApplyUpgrade returns true on success

    [Test]
    public void ApplyUpgrade_ValidUpgrade_ReturnsTrue()
    {
        RunUpgradeSO upgrade = CreateUpgrade("DmgUp", UpgradeType.Damage, 1.5f);
        bool result = _manager.ApplyUpgrade(upgrade);

        Assert.IsTrue(result);
    }
}

// Mock PlayerController for testing
public class MockPlayerController : MonoBehaviour
{
    private float _moveSpeed = 5f;
    public float MoveSpeed => _moveSpeed;

    public void SetMoveSpeed(float speed)
    {
        _moveSpeed = speed;
    }

    public void SetBaseSpeed(float speed)
    {
        _moveSpeed = speed;
    }
}
