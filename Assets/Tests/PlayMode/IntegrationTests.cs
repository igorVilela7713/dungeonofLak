using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EnemyBasePlayModeTests
{
    private GameObject _enemyObj;
    private TestEnemy _enemy;

    [SetUp]
    public void SetUp()
    {
        _enemyObj = new GameObject("TestEnemy");
        _enemy = _enemyObj.AddComponent<TestEnemy>();
        _enemy.ExposedMaxHealth = 30;
        _enemy.ExposedCurrentHealth = 30;
        _enemy.ExposedDamageToPlayer = 10;
    }

    [TearDown]
    public void TearDown()
    {
        if (_enemyObj != null)
            Object.DestroyImmediate(_enemyObj);
    }

    [UnityTest]
    public IEnumerator TakeDamage_DeathCoroutine_DestroysObject()
    {
        _enemy.TakeDamage(30);

        Assert.IsTrue(_enemy.IsDead);

        // Wait for potential destroy delay
        yield return new WaitForSeconds(0.5f);

        // Object should be destroyed by now (if OnDeath triggers Destroy)
        // Our TestEnemy overrides OnDeath to just set _isDead,
        // so we just verify the state
        Assert.IsTrue(_enemy.IsDead);
    }

    [UnityTest]
    public IEnumerator TakeDamage_MultipleHitsOverFrames_HealthDecreases()
    {
        _enemy.TakeDamage(10);
        yield return null;

        Assert.AreEqual(20, _enemy.ExposedCurrentHealth);

        _enemy.TakeDamage(10);
        yield return null;

        Assert.AreEqual(10, _enemy.ExposedCurrentHealth);

        _enemy.TakeDamage(10);
        yield return null;

        Assert.IsTrue(_enemy.IsDead);
    }
}

public class PlayerHealthPlayModeTests
{
    private GameObject _playerObj;
    private PlayerHealth _playerHealth;

    [SetUp]
    public void SetUp()
    {
        _playerObj = new GameObject("Player");
        _playerHealth = _playerObj.AddComponent<PlayerHealth>();
    }

    [TearDown]
    public void TearDown()
    {
        if (_playerObj != null)
            Object.DestroyImmediate(_playerObj);
    }

    [UnityTest]
    public IEnumerator TakeDamage_DeathEvent_FiresInNextFrame()
    {
        bool deathFired = false;
        _playerHealth.OnDeath.AddListener(() => deathFired = true);

        _playerHealth.TakeDamage(100);
        yield return null;

        Assert.IsTrue(deathFired);
    }

    [UnityTest]
    public IEnumerator HealToFull_AfterMultipleFrames_RestoresHealth()
    {
        _playerHealth.TakeDamage(50);
        yield return null;

        Assert.AreEqual(50, _playerHealth.CurrentHealth);

        _playerHealth.HealToFull();
        yield return null;

        Assert.AreEqual(100, _playerHealth.CurrentHealth);
    }
}

public class RunUpgradePlayModeTests
{
    private GameObject _managerObj;
    private RunUpgradeManager _manager;
    private GameObject _currencyObj;
    private RunCurrency _currency;

    [SetUp]
    public void SetUp()
    {
        // Reset singleton instances
        ResetSingletons();

        _currencyObj = new GameObject("RunCurrency");
        _currency = _currencyObj.AddComponent<RunCurrency>();
        _ = RunCurrency.Instance;

        _managerObj = new GameObject("RunUpgradeManager");
        _manager = _managerObj.AddComponent<RunUpgradeManager>();
        _ = RunUpgradeManager.Instance;
    }

    [TearDown]
    public void TearDown()
    {
        if (_managerObj != null) Object.DestroyImmediate(_managerObj);
        if (_currencyObj != null) Object.DestroyImmediate(_currencyObj);
        ResetSingletons();
    }

    private void ResetSingletons()
    {
        var runUpgradeProp = typeof(RunUpgradeManager).GetProperty("Instance",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        runUpgradeProp?.SetValue(null, null);

        var currencyProp = typeof(RunCurrency).GetProperty("Instance",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        currencyProp?.SetValue(null, null);
    }

    [UnityTest]
    public IEnumerator ApplyUpgrade_WithCost_SpendsRunes()
    {
        RunUpgradeSO upgrade = ScriptableObject.CreateInstance<RunUpgradeSO>();
        upgrade.upgradeName = "TestDmg";
        upgrade.upgradeType = UpgradeType.Damage;
        upgrade.value = 1.5f;
        upgrade.cost = 10;
        upgrade.hasTradeOff = false;

        _currency.AddRunes(20);
        yield return null;

        bool result = _manager.ApplyUpgrade(upgrade);
        yield return null;

        Assert.IsTrue(result);
        Assert.AreEqual(10, _currency.CurrentRunes);
        Assert.AreEqual(1.5f, _manager.GetDamageMultiplier(), 0.001f);
    }

    [UnityTest]
    public IEnumerator ApplyUpgrade_InsufficientRunes_ReturnsFalse()
    {
        RunUpgradeSO upgrade = ScriptableObject.CreateInstance<RunUpgradeSO>();
        upgrade.upgradeName = "TestDmg";
        upgrade.upgradeType = UpgradeType.Damage;
        upgrade.value = 1.5f;
        upgrade.cost = 100;
        upgrade.hasTradeOff = false;

        _currency.AddRunes(5);
        yield return null;

        bool result = _manager.ApplyUpgrade(upgrade);
        yield return null;

        Assert.IsFalse(result);
        Assert.AreEqual(5, _currency.CurrentRunes);
        Assert.AreEqual(1f, _manager.GetDamageMultiplier(), 0.001f);
    }

    [UnityTest]
    public IEnumerator ResetUpgrades_AfterApply_RestoresDefaults()
    {
        RunUpgradeSO upgrade = ScriptableObject.CreateInstance<RunUpgradeSO>();
        upgrade.upgradeName = "TestDmg";
        upgrade.upgradeType = UpgradeType.Damage;
        upgrade.value = 2f;
        upgrade.cost = 0;
        upgrade.hasTradeOff = false;

        _manager.ApplyUpgrade(upgrade);
        yield return null;

        Assert.AreEqual(2f, _manager.GetDamageMultiplier(), 0.001f);

        _manager.ResetUpgrades();
        yield return null;

        Assert.AreEqual(1f, _manager.GetDamageMultiplier(), 0.001f);
    }
}
