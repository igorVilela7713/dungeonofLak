using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class WeaponControllerTests
{
    private GameObject _weaponCtrlObj;
    private WeaponController _weaponController;

    [SetUp]
    public void SetUp()
    {
        _weaponCtrlObj = new GameObject("WeaponController");
        _weaponController = _weaponCtrlObj.AddComponent<WeaponController>();

        // Set a player transform (required by WeaponController)
        GameObject playerObj = new GameObject("Player");
        SetPrivateField(_weaponController, "_player", playerObj.transform);
    }

    [TearDown]
    public void TearDown()
    {
        Transform player = GetPrivateField<Transform>(_weaponController, "_player");
        if (player != null) Object.DestroyImmediate(player.gameObject);
        Object.DestroyImmediate(_weaponCtrlObj);
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

    private WeaponDataSO CreateWeapon(string name, int damage, WeaponType type)
    {
        WeaponDataSO weapon = ScriptableObject.CreateInstance<WeaponDataSO>();
        weapon.weaponName = name;
        weapon.weaponType = type;
        weapon.damage = damage;
        weapon.attackCooldown = 0.3f;
        weapon.attackDuration = 0.1f;
        weapon.attackRange = 0.8f;
        weapon.knockbackForce = 2f;
        weapon.attackPattern = AttackPattern.HorizontalSwing;
        return weapon;
    }

    private WeaponDataSO CreateWeaponWithStats(string name, int damage,
        float cooldown, float duration, WeaponType type)
    {
        WeaponDataSO weapon = ScriptableObject.CreateInstance<WeaponDataSO>();
        weapon.weaponName = name;
        weapon.weaponType = type;
        weapon.damage = damage;
        weapon.attackCooldown = cooldown;
        weapon.attackDuration = duration;
        weapon.attackRange = 0.8f;
        weapon.knockbackForce = 2f;
        weapon.attackPattern = AttackPattern.HorizontalSwing;
        return weapon;
    }

    // EquippedWeapon - Initial

    [Test]
    public void EquippedWeapon_WithoutDefault_IsNull()
    {
        // No default weapon set
        Assert.IsNull(_weaponController.EquippedWeapon);
    }

    // EquipWeapon

    [Test]
    public void EquipWeapon_SetsEquippedWeapon()
    {
        WeaponDataSO sword = CreateWeapon("Sword", 10, WeaponType.Sword);
        _weaponController.EquipWeapon(sword);

        Assert.AreSame(sword, _weaponController.EquippedWeapon);
    }

    [Test]
    public void EquipWeapon_SwitchesWeapon()
    {
        WeaponDataSO sword = CreateWeapon("Sword", 10, WeaponType.Sword);
        WeaponDataSO spear = CreateWeapon("Spear", 15, WeaponType.Spear);

        _weaponController.EquipWeapon(sword);
        _weaponController.EquipWeapon(spear);

        Assert.AreSame(spear, _weaponController.EquippedWeapon);
    }

    [Test]
    public void EquipWeapon_NullWeapon_DoesNotChange()
    {
        WeaponDataSO sword = CreateWeapon("Sword", 10, WeaponType.Sword);
        _weaponController.EquipWeapon(sword);

        _weaponController.EquipWeapon(null);

        Assert.AreSame(sword, _weaponController.EquippedWeapon);
    }

    // OnHitboxTrigger - Damage calculation

    [Test]
    public void OnHitboxTrigger_WithoutWeapon_DoesNotThrow()
    {
        GameObject targetObj = new GameObject("Target");
        // Add required IDamageable component
        targetObj.AddComponent<MockDamageable>();
        Collider2D col = targetObj.AddComponent<BoxCollider2D>();

        Assert.DoesNotThrow(() => _weaponController.OnHitboxTrigger(col));

        Object.DestroyImmediate(targetObj);
    }

    [Test]
    public void OnHitboxTrigger_DealsCorrectDamage()
    {
        WeaponDataSO sword = CreateWeapon("Sword", 25, WeaponType.Sword);
        _weaponController.EquipWeapon(sword);

        GameObject targetObj = new GameObject("Target");
        MockDamageable damageable = targetObj.AddComponent<MockDamageable>();
        Collider2D col = targetObj.AddComponent<BoxCollider2D>();

        _weaponController.OnHitboxTrigger(col);

        Assert.AreEqual(25, damageable.LastDamageReceived);

        Object.DestroyImmediate(targetObj);
    }

    [Test]
    public void OnHitboxTrigger_DifferentWeapons_DealDifferentDamage()
    {
        WeaponDataSO sword = CreateWeapon("Sword", 10, WeaponType.Sword);
        WeaponDataSO axe = CreateWeapon("Axe", 30, WeaponType.Axe);

        _weaponController.EquipWeapon(sword);

        GameObject target1 = new GameObject("Target1");
        MockDamageable dmg1 = target1.AddComponent<MockDamageable>();
        Collider2D col1 = target1.AddComponent<BoxCollider2D>();
        _weaponController.OnHitboxTrigger(col1);

        Object.DestroyImmediate(target1);

        _weaponController.EquipWeapon(axe);

        GameObject target2 = new GameObject("Target2");
        MockDamageable dmg2 = target2.AddComponent<MockDamageable>();
        Collider2D col2 = target2.AddComponent<BoxCollider2D>();
        _weaponController.OnHitboxTrigger(col2);

        Assert.AreEqual(10, dmg1.LastDamageReceived);
        Assert.AreEqual(30, dmg2.LastDamageReceived);

        Object.DestroyImmediate(target2);
    }

    // IsAttacking initial state

    [Test]
    public void IsAttacking_InitialState_IsFalse()
    {
        Assert.IsFalse(_weaponController.IsAttacking);
    }

    // Cleanup

    [TearDown]
    public void CleanupScriptableObjects()
    {
        // ScriptableObjects created in tests will be GC'd
    }
}

// Mock IDamageable implementation
public class MockDamageable : MonoBehaviour, IDamageable
{
    public int LastDamageReceived { get; private set; }
    public int TotalDamageReceived { get; private set; }
    public int TimesDamaged { get; private set; }

    public void TakeDamage(int amount)
    {
        LastDamageReceived = amount;
        TotalDamageReceived += amount;
        TimesDamaged++;
    }
}
