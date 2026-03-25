using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Weapon Data")]
public class WeaponDataSO : ScriptableObject
{
    [Header("Identity")]
    public WeaponType weaponType;
    public string weaponName;

    [Header("Stats")]
    public int damage = 10;
    public float attackCooldown = 0.3f;
    public float attackDuration = 0.1f;
    public float attackRange = 0.8f;
    public float knockbackForce = 2f;

    [Header("Attack Pattern")]
    public AttackPattern attackPattern = AttackPattern.HorizontalSwing;

    [Header("Visual")]
    public Color placeholderColor = Color.white;
}
