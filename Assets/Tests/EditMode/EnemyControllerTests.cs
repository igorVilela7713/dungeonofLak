using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// Test subclass of EnemyController that avoids physics-dependent code
public class TestEnemyController : EnemyController
{
    // Expose protected fields
    public int ExposedMaxHealth
    {
        get => _maxHealth;
        set => _maxHealth = value;
    }

    public int ExposedCurrentHealth
    {
        get => _currentHealth;
        set => _currentHealth = value;
    }

    public int ExposedDamageToPlayer
    {
        get => _damageToPlayer;
        set => _damageToPlayer = value;
    }

    public int ExposedRuneValue
    {
        get => _runeValue;
        set => _runeValue = value;
    }

    public bool ExposedIsDead
    {
        get => _isDead;
        set => _isDead = value;
    }

    public bool ExposedIsElite
    {
        get => _isElite;
        set => _isElite = value;
    }

    protected override void Awake()
    {
        _currentHealth = _maxHealth;
        // Skip base Awake to avoid physics setup
    }

    protected override void OnDeath()
    {
        _isDead = true;
    }
}

public class EnemyControllerTests
{
    private GameObject _enemyObj;
    private TestEnemyController _enemy;

    [SetUp]
    public void SetUp()
    {
        _enemyObj = new GameObject("TestEnemyController");
        _enemy = _enemyObj.AddComponent<TestEnemyController>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_enemyObj);
    }

    // TakeDamage (inherited from EnemyBase)

    [Test]
    public void TakeDamage_ReducesHealth()
    {
        _enemy.ExposedMaxHealth = 50;
        _enemy.ExposedCurrentHealth = 50;

        _enemy.TakeDamage(20);

        Assert.AreEqual(30, _enemy.ExposedCurrentHealth);
        Assert.IsFalse(_enemy.IsDead);
    }

    [Test]
    public void TakeDamage_FatalDamage_TriggersDeath()
    {
        _enemy.ExposedMaxHealth = 30;
        _enemy.ExposedCurrentHealth = 30;

        _enemy.TakeDamage(30);

        Assert.IsTrue(_enemy.IsDead);
    }

    // ApplyDifficulty (inherited from EnemyBase)

    [Test]
    public void ApplyDifficulty_ScalesHealthAndDamage()
    {
        _enemy.ExposedMaxHealth = 30;
        _enemy.ExposedCurrentHealth = 30;
        _enemy.ExposedDamageToPlayer = 10;

        _enemy.ApplyDifficulty(1.4f, 1.2f);

        Assert.AreEqual(42, _enemy.ExposedCurrentHealth);
        Assert.AreEqual(12, _enemy.ExposedDamageToPlayer);
    }

    // Initialize

    [Test]
    public void Initialize_SetsPlayerReference()
    {
        GameObject playerObj = new GameObject("Player");

        _enemy.Initialize(playerObj.transform);

        FieldInfo playerField = typeof(EnemyController).GetField("_player",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Transform playerRef = (Transform)playerField?.GetValue(_enemy);

        Assert.AreSame(playerObj.transform, playerRef);

        Object.DestroyImmediate(playerObj);
    }
}
