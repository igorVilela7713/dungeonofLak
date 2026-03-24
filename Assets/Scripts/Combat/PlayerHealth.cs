using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private int _maxHealth = 100;

    private int _currentHealth;

    public UnityEvent OnDeath;

    public int MaxHealth => _maxHealth;
    public int CurrentHealth => _currentHealth;

    private void Awake()
    {
        _currentHealth = _maxHealth;
    }

    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            Die();
        }
    }

    public void HealToFull()
    {
        _currentHealth = _maxHealth;
    }

    public void HealPercent(float percent)
    {
        int healAmount = Mathf.RoundToInt(_maxHealth * percent);
        _currentHealth = Mathf.Min(_currentHealth + healAmount, _maxHealth);
    }

    public void SetMaxHealth(int newMax)
    {
        _maxHealth = newMax;
        if (_currentHealth > _maxHealth)
        {
            _currentHealth = _maxHealth;
        }
    }

    private void Die()
    {
        OnDeath?.Invoke();
    }
}
