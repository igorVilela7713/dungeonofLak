using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    private WeaponController _controller;
    
    public void Initialize(WeaponController controller)
    {
        _controller = controller;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        _controller?.OnHitboxTrigger(other);
    }
}
