using UnityEngine;

public class FloorTransition : MonoBehaviour
{
    private bool _isTriggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isTriggered) return;
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        if (FloorManager.Instance == null) return;

        _isTriggered = true;
        FloorManager.Instance.NextFloor();
    }
}
