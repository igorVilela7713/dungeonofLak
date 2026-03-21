using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Collider2D _collider;
    private bool _isOpen = true;
    
    public bool IsOpen => _isOpen;
    
    public void Open()
    {
        _collider.enabled = false;
        _isOpen = true;
    }
    
    public void Close()
    {
        _collider.enabled = true;
        _isOpen = false;
    }
}
