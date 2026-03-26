using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealthTests
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
        Object.DestroyImmediate(_playerObj);
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

    // Initial state

    [Test]
    public void MaxHealth_Default_Is100()
    {
        Assert.AreEqual(100, _playerHealth.MaxHealth);
    }

    [Test]
    public void CurrentHealth_InitialState_EqualsMaxHealth()
    {
        Assert.AreEqual(_playerHealth.MaxHealth, _playerHealth.CurrentHealth);
    }

    // TakeDamage

    [Test]
    public void TakeDamage_ReducesHealth()
    {
        _playerHealth.TakeDamage(30);

        Assert.AreEqual(70, _playerHealth.CurrentHealth);
    }

    [Test]
    public void TakeDamage_MultipleHits_Accumulates()
    {
        _playerHealth.TakeDamage(20);
        _playerHealth.TakeDamage(15);

        Assert.AreEqual(65, _playerHealth.CurrentHealth);
    }

    [Test]
    public void TakeDamage_WhenHealthReachesZero_HealthIsZero()
    {
        _playerHealth.TakeDamage(100);

        Assert.AreEqual(0, _playerHealth.CurrentHealth);
    }

    [Test]
    public void TakeDamage_Overkill_HealthClampsToZero()
    {
        _playerHealth.TakeDamage(200);

        Assert.AreEqual(0, _playerHealth.CurrentHealth);
    }

    // HealToFull

    [Test]
    public void HealToFull_AfterDamage_RestoresHealth()
    {
        _playerHealth.TakeDamage(50);
        _playerHealth.HealToFull();

        Assert.AreEqual(_playerHealth.MaxHealth, _playerHealth.CurrentHealth);
    }

    // HealPercent

    [Test]
    public void HealPercent_HealsCorrectAmount()
    {
        _playerHealth.TakeDamage(60); // HP = 40
        _playerHealth.HealPercent(0.25f); // 25% of 100 = 25

        Assert.AreEqual(65, _playerHealth.CurrentHealth);
    }

    [Test]
    public void HealPercent_DoesNotExceedMaxHealth()
    {
        _playerHealth.TakeDamage(10); // HP = 90
        _playerHealth.HealPercent(0.5f); // 50% of 100 = 50

        Assert.AreEqual(100, _playerHealth.CurrentHealth);
    }

    [Test]
    public void HealPercent_FromZero_Heals()
    {
        _playerHealth.TakeDamage(200); // HP = 0
        _playerHealth.HealPercent(0.5f); // 50% of 100 = 50

        Assert.AreEqual(50, _playerHealth.CurrentHealth);
    }

    // SetMaxHealth

    [Test]
    public void SetMaxHealth_ChangesMaxHealth()
    {
        _playerHealth.SetMaxHealth(150);

        Assert.AreEqual(150, _playerHealth.MaxHealth);
    }

    [Test]
    public void SetMaxHealth_WhenCurrentExceedsNew_ClampsCurrent()
    {
        // Default max is 100, current is 100
        _playerHealth.SetMaxHealth(50);

        Assert.AreEqual(50, _playerHealth.CurrentHealth);
    }

    [Test]
    public void SetMaxHealth_WhenCurrentBelowNew_DoesNotChangeCurrent()
    {
        _playerHealth.TakeDamage(50); // HP = 50
        _playerHealth.SetMaxHealth(150);

        Assert.AreEqual(50, _playerHealth.CurrentHealth);
        Assert.AreEqual(150, _playerHealth.MaxHealth);
    }

    // Death event

    [Test]
    public void TakeDamage_WhenHealthReachesZero_FiresOnDeath()
    {
        bool deathFired = false;
        _playerHealth.OnDeath.AddListener(() => deathFired = true);

        _playerHealth.TakeDamage(100);

        Assert.IsTrue(deathFired);
    }

    [Test]
    public void TakeDamage_WhenHealthStaysAboveZero_DoesNotFireOnDeath()
    {
        bool deathFired = false;
        _playerHealth.OnDeath.AddListener(() => deathFired = true);

        _playerHealth.TakeDamage(50);

        Assert.IsFalse(deathFired);
    }
}
