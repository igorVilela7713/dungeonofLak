using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    private const float BaseHitboxSize = 0.8f;

    [Header("Setup")]
    [SerializeField] private WeaponDataSO _defaultWeapon;
    [SerializeField] private GameObject _hitboxPrefab;
    [SerializeField] private Transform _player;
    [SerializeField] private WeaponVisualController _visualController;

    private WeaponDataSO _equippedWeapon;
    private bool _isAttacking;
    private float _cooldownTimer;
    private InputAction _attackAction;
    private InputAction _swapAction;
    private GameObject _activeHitbox;
    private PlayerController _playerController;

    private readonly List<WeaponDataSO> _availableWeapons = new List<WeaponDataSO>();
    private int _currentWeaponIndex;

    public bool IsAttacking => _isAttacking;
    public WeaponDataSO EquippedWeapon => _equippedWeapon;

    private void Awake()
    {
        if (_defaultWeapon != null)
        {
            _equippedWeapon = _defaultWeapon;
            _availableWeapons.Add(_defaultWeapon);
            _currentWeaponIndex = 0;
        }
        else
        {
            Debug.LogError("WeaponController: _defaultWeapon não atribuído no Inspector!");
        }

        _playerController = _player.GetComponent<PlayerController>();

        _attackAction = new InputAction("Attack", InputActionType.Button);
        _attackAction.AddBinding("<Mouse>/leftButton");
        _attackAction.AddBinding("<Keyboard>/enter");
        _attackAction.AddBinding("<Gamepad>/buttonWest");

        _swapAction = new InputAction("SwapWeapon", InputActionType.Button);
        _swapAction.AddBinding("<Keyboard>/q");
        _swapAction.AddBinding("<Gamepad>/buttonNorth");

        if (_visualController != null && _equippedWeapon != null)
        {
            _visualController.EquipVisual(_equippedWeapon.weaponType);
        }
    }

    private void OnEnable()
    {
        _attackAction.Enable();
        _attackAction.performed += OnAttackPerformed;
        _swapAction.Enable();
        _swapAction.performed += OnSwapPerformed;
    }

    private void OnDisable()
    {
        _attackAction.performed -= OnAttackPerformed;
        _attackAction.Disable();
        _swapAction.performed -= OnSwapPerformed;
        _swapAction.Disable();
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        Attack();
    }

    private void OnSwapPerformed(InputAction.CallbackContext context)
    {
        CycleWeapon();
    }

    private void Update()
    {
        if (_cooldownTimer > 0)
        {
            _cooldownTimer -= Time.deltaTime;
        }
    }

    public void Attack()
    {
        if (_isAttacking) return;
        if (_cooldownTimer > 0) return;
        if (_equippedWeapon == null) return;

        _isAttacking = true;
        SpawnHitbox();
        StartCoroutine(AttackRoutine());
    }

    private System.Collections.IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(_equippedWeapon.attackDuration);
        if (_activeHitbox != null)
        {
            Destroy(_activeHitbox);
            _activeHitbox = null;
        }
        _isAttacking = false;
        _cooldownTimer = _equippedWeapon.attackCooldown;
    }

    private void SpawnHitbox()
    {
        if (_activeHitbox != null)
        {
            Destroy(_activeHitbox);
        }

        Vector2 dir = _playerController != null ? _playerController.FacingDirection : Vector2.right;
        Vector3 offset = GetAttackOffset(dir);

        Vector3 spawnPos = _player.position + offset;
        _activeHitbox = Instantiate(_hitboxPrefab, spawnPos, Quaternion.identity, _player);

        float hitboxScale = _equippedWeapon.attackRange / BaseHitboxSize;
        _activeHitbox.transform.localScale = new Vector3(hitboxScale, hitboxScale, 1f);

        SwordHitbox swordHitbox = _activeHitbox.GetComponent<SwordHitbox>();
        swordHitbox.Initialize(this);
    }

    private Vector3 GetAttackOffset(Vector2 facingDir)
    {
        float range = _equippedWeapon.attackRange;

        switch (_equippedWeapon.attackPattern)
        {
            case AttackPattern.HorizontalSwing:
                return new Vector3(facingDir.x * range, 0.2f, 0f);
            case AttackPattern.ForwardThrust:
                return new Vector3(facingDir.x * (range + 0.2f), 0f, 0f);
            case AttackPattern.OverheadSmash:
                return new Vector3(facingDir.x * 0.3f, range * 0.5f, 0f);
            case AttackPattern.QuickStab:
                return new Vector3(facingDir.x * range, 0f, 0f);
            default:
                return new Vector3(facingDir.x * range, 0f, 0f);
        }
    }

    public void OnHitboxTrigger(Collider2D other)
    {
        if (_equippedWeapon == null) return;

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            int finalDamage = _equippedWeapon.damage;

            if (RunUpgradeManager.Instance != null)
            {
                finalDamage = Mathf.RoundToInt(finalDamage * RunUpgradeManager.Instance.GetDamageMultiplier());
            }

            damageable.TakeDamage(finalDamage);

            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 knockDir = (other.transform.position - _player.position).normalized;
                rb.AddForce(knockDir * _equippedWeapon.knockbackForce, ForceMode2D.Impulse);
            }
        }
    }

    public void EquipWeapon(WeaponDataSO weapon)
    {
        if (weapon == null) return;

        if (!_availableWeapons.Contains(weapon))
        {
            _availableWeapons.Add(weapon);
        }

        if (_isAttacking) return;

        _equippedWeapon = weapon;
        _currentWeaponIndex = _availableWeapons.IndexOf(weapon);

        if (_visualController != null)
        {
            _visualController.EquipVisual(weapon.weaponType);
        }
    }

    private void CycleWeapon()
    {
        if (_availableWeapons.Count <= 1) return;
        if (_isAttacking) return;

        _currentWeaponIndex = (_currentWeaponIndex + 1) % _availableWeapons.Count;
        _equippedWeapon = _availableWeapons[_currentWeaponIndex];

        if (_visualController != null)
        {
            _visualController.EquipVisual(_equippedWeapon.weaponType);
        }
    }
}
