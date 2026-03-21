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
    
    private void Die()
    {
        OnDeath?.Invoke();
    }
}
