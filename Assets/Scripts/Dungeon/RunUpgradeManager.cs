using System.Collections.Generic;
using UnityEngine;

public class RunUpgradeManager : MonoBehaviour
{
    public static RunUpgradeManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private PlayerController _playerController;

    private readonly List<RunUpgradeSO> _appliedUpgrades = new List<RunUpgradeSO>();
    private float _baseMoveSpeed;
    private int _baseMaxHealth;
    private float _damageMultiplier = 1f;

    public float DamageMultiplier => _damageMultiplier;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (_playerController != null)
        {
            _baseMoveSpeed = _playerController.MoveSpeed;
        }
        if (_playerHealth != null)
        {
            _baseMaxHealth = _playerHealth.MaxHealth;
        }
    }

    public void SetReferences(PlayerHealth health, PlayerController controller)
    {
        _playerHealth = health;
        _playerController = controller;
        if (_playerController != null)
        {
            _baseMoveSpeed = _playerController.MoveSpeed;
        }
        if (_playerHealth != null)
        {
            _baseMaxHealth = _playerHealth.MaxHealth;
        }
    }

    public bool ApplyUpgrade(RunUpgradeSO upgrade)
    {
        if (upgrade == null) return false;

        if (RunCurrency.Instance != null)
        {
            if (!RunCurrency.Instance.SpendRunes(upgrade.cost)) return false;
        }

        ApplyModifier(upgrade.upgradeType, upgrade.value);

        if (upgrade.hasTradeOff)
        {
            ApplyModifier(upgrade.tradeOffType, upgrade.tradeOffValue);
        }

        _appliedUpgrades.Add(upgrade);
        return true;
    }

    public void ResetUpgrades()
    {
        _appliedUpgrades.Clear();
        _damageMultiplier = 1f;

        if (_playerController != null)
        {
            _playerController.SetMoveSpeed(_baseMoveSpeed);
        }
        if (_playerHealth != null)
        {
            _playerHealth.SetMaxHealth(_baseMaxHealth);
        }
    }

    public float GetDamageMultiplier()
    {
        return _damageMultiplier;
    }

    private void ApplyModifier(UpgradeType type, float value)
    {
        switch (type)
        {
            case UpgradeType.Damage:
                _damageMultiplier *= value;
                break;
            case UpgradeType.Speed:
                if (_playerController != null)
                {
                    _playerController.SetMoveSpeed(_baseMoveSpeed * value);
                }
                break;
            case UpgradeType.MaxHealth:
                if (_playerHealth != null)
                {
                    int newMax = Mathf.RoundToInt(_baseMaxHealth * value);
                    _playerHealth.SetMaxHealth(newMax);
                }
                break;
            case UpgradeType.CritChance:
                break;
        }
    }
}
