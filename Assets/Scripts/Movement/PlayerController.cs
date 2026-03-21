using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;

    [Header("Jump Settings")]
    [SerializeField] private float _jumpForce = 10f;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask _groundLayer;

    [Header("Combat")]
    [SerializeField] private float _knockbackForce = 5f;
    [SerializeField] private float _invincibilityDuration = 0.5f;

    private Rigidbody2D _rigidbody;
    private Vector2 _facingDirection = Vector2.right;
    private float _horizontalInput;
    private bool _isGrounded;
    private bool _isInvincible;
    private InputAction _moveAction;
    private InputAction _jumpAction;

    public Vector2 FacingDirection => _facingDirection;
    public bool IsInvincible => _isInvincible;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.gravityScale = 3f;
        _rigidbody.freezeRotation = true;

        _moveAction = new InputAction("Move", InputActionType.Value);
        _moveAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/a")
            .With("Negative", "<Keyboard>/leftArrow")
            .With("Positive", "<Keyboard>/d")
            .With("Positive", "<Keyboard>/rightArrow");
        _moveAction.AddBinding("<Gamepad>/leftStick/x");

        _jumpAction = new InputAction("Jump", InputActionType.Button);
        _jumpAction.AddBinding("<Keyboard>/w");
        _jumpAction.AddBinding("<Keyboard>/upArrow");
        _jumpAction.AddBinding("<Keyboard>/space");
        _jumpAction.AddBinding("<Gamepad>/buttonSouth");
    }

    private void OnEnable()
    {
        _moveAction.Enable();
        _jumpAction.Enable();
        _jumpAction.performed += OnJumpPerformed;
    }

    private void OnDisable()
    {
        _jumpAction.performed -= OnJumpPerformed;
        _moveAction.Disable();
        _jumpAction.Disable();
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (_isGrounded)
        {
            _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, _jumpForce);
        }
    }

    private void FixedUpdate()
    {
        _isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);
    }

    private void Update()
    {
        _horizontalInput = _moveAction.ReadValue<float>();
        _rigidbody.linearVelocity = new Vector2(_horizontalInput * _moveSpeed, _rigidbody.linearVelocity.y);

        if (_horizontalInput != 0)
        {
            _facingDirection = new Vector2(_horizontalInput > 0 ? 1 : -1, 0);
        }
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        _rigidbody.AddForce(direction * force, ForceMode2D.Impulse);
        StartCoroutine(InvincibilityRoutine());
    }

    private System.Collections.IEnumerator InvincibilityRoutine()
    {
        _isInvincible = true;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            for (int i = 0; i < 4; i++)
            {
                sr.color = Color.red;
                yield return new WaitForSeconds(0.08f);
                sr.color = Color.white;
                yield return new WaitForSeconds(0.08f);
            }
        }
        else
        {
            yield return new WaitForSeconds(_invincibilityDuration);
        }

        _isInvincible = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (_groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
        }
    }
}
