using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// Minimal test subclass to avoid RequireComponent issues in EditMode
public class TestEnemy : EnemyBase
{
    // Expose protected fields for test setup
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

    // Override Awake to skip RequireComponent-dependent code
    protected override void Awake()
    {
        _currentHealth = _maxHealth;
        // Skip rigidbody/spriteRenderer setup
    }

    // Override OnDeath to avoid DropRune (needs DungeonGenerator in scene)
    protected override void OnDeath()
    {
        _isDead = true;
    }
}

public class EnemyBaseTests
{
    private GameObject _enemyObj;
    private TestEnemy _enemy;

    [SetUp]
    public void SetUp()
    {
        _enemyObj = new GameObject("TestEnemy");
        _enemy = _enemyObj.AddComponent<TestEnemy>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_enemyObj);
    }

    // TakeDamage

    [Test]
    public void TakeDamage_ReducesHealth()
    {
        _enemy.ExposedMaxHealth = 100;
        _enemy.ExposedCurrentHealth = 100;

        _enemy.TakeDamage(30);

        Assert.AreEqual(70, _enemy.ExposedCurrentHealth);
    }

    [Test]
    public void TakeDamage_MultipleHits_Accumulates()
    {
        _enemy.ExposedMaxHealth = 100;
        _enemy.ExposedCurrentHealth = 100;

        _enemy.TakeDamage(20);
        _enemy.TakeDamage(15);

        Assert.AreEqual(65, _enemy.ExposedCurrentHealth);
    }

    [Test]
    public void TakeDamage_WhenHealthReachesZero_SetsIsDead()
    {
        _enemy.ExposedMaxHealth = 30;
        _enemy.ExposedCurrentHealth = 30;

        _enemy.TakeDamage(30);

        Assert.IsTrue(_enemy.IsDead);
    }

    [Test]
    public void TakeDamage_WhenHealthBelowZero_SetsIsDead()
    {
        _enemy.ExposedMaxHealth = 30;
        _enemy.ExposedCurrentHealth = 10;

        _enemy.TakeDamage(50);

        Assert.IsTrue(_enemy.IsDead);
    }

    [Test]
    public void TakeDamage_WhenAlreadyDead_DoesNotReduceHealth()
    {
        _enemy.ExposedMaxHealth = 30;
        _enemy.ExposedCurrentHealth = 0;
        _enemy.ExposedIsDead = true;

        _enemy.TakeDamage(10);

        Assert.AreEqual(0, _enemy.ExposedCurrentHealth);
    }

    [Test]
    public void TakeDamage_ExactHealthToZero_TriggersDeath()
    {
        _enemy.ExposedMaxHealth = 50;
        _enemy.ExposedCurrentHealth = 10;

        _enemy.TakeDamage(10);

        Assert.IsTrue(_enemy.IsDead);
        Assert.AreEqual(0, _enemy.ExposedCurrentHealth);
    }

    // ApplyDifficulty

    [Test]
    public void ApplyDifficulty_ScalesHealth()
    {
        _enemy.ExposedMaxHealth = 30;
        _enemy.ExposedCurrentHealth = 30;

        _enemy.ApplyDifficulty(2f, 1f);

        Assert.AreEqual(60, _enemy.ExposedCurrentHealth);
    }

    [Test]
    public void ApplyDifficulty_ScalesDamage()
    {
        _enemy.ExposedDamageToPlayer = 10;

        _enemy.ApplyDifficulty(1f, 1.5f);

        Assert.AreEqual(15, _enemy.ExposedDamageToPlayer);
    }

    [Test]
    public void ApplyDifficulty_ScalesRuneValue()
    {
        _enemy.ExposedRuneValue = 2;

        _enemy.ApplyDifficulty(1f, 1f, 3f);

        Assert.AreEqual(6, _enemy.ExposedRuneValue);
    }

    [Test]
    public void ApplyDifficulty_RuneValueMinIs1()
    {
        _enemy.ExposedRuneValue = 1;

        // 1 * 0.1 = 0.1 -> RoundToInt(0.1) = 0 -> Max(1, 0) = 1
        _enemy.ApplyDifficulty(1f, 1f, 0.1f);

        Assert.AreEqual(1, _enemy.ExposedRuneValue);
    }

    [Test]
    public void ApplyDifficulty_SetsCurrentHealthToScaledMax()
    {
        _enemy.ExposedMaxHealth = 30;
        _enemy.ExposedCurrentHealth = 10; // damaged

        _enemy.ApplyDifficulty(2f, 1f);

        Assert.AreEqual(60, _enemy.ExposedCurrentHealth);
    }

    [TestCase(1f, 1f, 30, 10)]
    [TestCase(1.4f, 1.2f, 42, 12)]
    [TestCase(2f, 2f, 60, 20)]
    [TestCase(0.5f, 0.5f, 15, 5)]
    public void ApplyDifficulty_Parameterized(float hpMult, float dmgMult,
        int expectedHP, int expectedDmg)
    {
        _enemy.ExposedMaxHealth = 30;
        _enemy.ExposedCurrentHealth = 30;
        _enemy.ExposedDamageToPlayer = 10;

        _enemy.ApplyDifficulty(hpMult, dmgMult);

        Assert.AreEqual(expectedHP, _enemy.ExposedCurrentHealth);
        Assert.AreEqual(expectedDmg, _enemy.ExposedDamageToPlayer);
    }

    // RuneValue - Elite

    [Test]
    public void RuneValue_NormalEnemy_ReturnsBaseValue()
    {
        _enemy.ExposedRuneValue = 3;

        Assert.AreEqual(3, _enemy.RuneValue);
    }

    // RuneValue with difficulty scaling

    [Test]
    public void RuneValue_AfterDifficultyScaling_ReturnsScaledValue()
    {
        _enemy.ExposedRuneValue = 2;
        _enemy.ApplyDifficulty(1f, 1f, 2f);

        Assert.AreEqual(4, _enemy.RuneValue);
    }

    // Multiple damage scenarios

    [Test]
    public void TakeDamage_GradualDamage_DeathAtCorrectThreshold()
    {
        _enemy.ExposedMaxHealth = 100;
        _enemy.ExposedCurrentHealth = 100;

        _enemy.TakeDamage(40);
        Assert.IsFalse(_enemy.IsDead);
        Assert.AreEqual(60, _enemy.ExposedCurrentHealth);

        _enemy.TakeDamage(30);
        Assert.IsFalse(_enemy.IsDead);
        Assert.AreEqual(30, _enemy.ExposedCurrentHealth);

        _enemy.TakeDamage(30);
        Assert.IsTrue(_enemy.IsDead);
        Assert.AreEqual(0, _enemy.ExposedCurrentHealth);
    }

    // ApplyDifficulty preserves death threshold

    [Test]
    public void ApplyDifficulty_ThenTakeDamage_CorrectDeathThreshold()
    {
        _enemy.ExposedMaxHealth = 30;
        _enemy.ExposedCurrentHealth = 30;
        _enemy.ExposedDamageToPlayer = 10;

        _enemy.ApplyDifficulty(2f, 1f); // HP: 60, DMG: 10

        _enemy.TakeDamage(59);
        Assert.IsFalse(_enemy.IsDead);

        _enemy.TakeDamage(1);
        Assert.IsTrue(_enemy.IsDead);
    }
}
