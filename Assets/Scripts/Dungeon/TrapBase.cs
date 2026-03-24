using UnityEngine;

public class TrapBase : MonoBehaviour
{
    [Header("Trap Settings")]
    [SerializeField] private int _damage = 5;
    [SerializeField] private float _damageInterval = 0.5f;
    [SerializeField] private string _trapType = "Spike";

    private float _lastDamageTime;

    public string TrapType => _trapType;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsValidTarget(other)) return;

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(_damage);
            _lastDamageTime = Time.time;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!IsValidTarget(other)) return;
        if (Time.time - _lastDamageTime < _damageInterval) return;

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(_damage);
            _lastDamageTime = Time.time;
        }
    }

    private bool IsValidTarget(Collider2D other)
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");

        return other.gameObject.layer == playerLayer || other.gameObject.layer == enemyLayer;
    }
}
