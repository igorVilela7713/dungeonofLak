using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private WeaponDataSO _weaponData;

    public WeaponDataSO WeaponData => _weaponData;

    public void Initialize(WeaponDataSO data)
    {
        _weaponData = data;
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
