using System.Collections.Generic;
using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    private WeaponController _controller;
    private readonly HashSet<Collider2D> _hitTargets = new HashSet<Collider2D>();

    public void Initialize(WeaponController controller)
    {
        _controller = controller;
        _hitTargets.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hitTargets.Contains(other)) return;
        _hitTargets.Add(other);
        _controller?.OnHitboxTrigger(other);
    }
}
