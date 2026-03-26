using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private WeaponDataSO _weaponData;

    private SpriteRenderer _spriteRenderer;
    private PlayerController _playerController;

    public WeaponDataSO WeaponData => _weaponData;

    public void Initialize(WeaponDataSO data)
    {
        _weaponData = data;
    }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            _playerController = player;
        }
    }

    private void Update()
    {
        if (_playerController != null && _spriteRenderer != null)
        {
            _spriteRenderer.flipX = _playerController.FacingDirection.x < 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_weaponData == null)
        {
            Debug.LogError("WeaponPickup: _weaponData não atribuído no Inspector!", this);
            return;
        }

        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        WeaponController wc = other.GetComponentInParent<WeaponController>();
        if (wc == null) return;

        wc.EquipWeapon(_weaponData);
        Destroy(gameObject);
    }
}
