using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    
    private Rigidbody2D _rigidbody;
    private Vector2 _facingDirection = Vector2.right;
    
    public Vector2 FacingDirection => _facingDirection;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }
    
    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector2 direction = new Vector2(horizontal, vertical);
        
        Move(direction);
        
        if (direction.x != 0)
        {
            _facingDirection = new Vector2(direction.x, 0);
        }
    }
    
    private void Move(Vector2 direction)
    {
        _rigidbody.velocity = direction.normalized * _moveSpeed;
    }
}
