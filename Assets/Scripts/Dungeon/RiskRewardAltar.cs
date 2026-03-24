using UnityEngine;
using UnityEngine.InputSystem;

public class RiskRewardAltar : MonoBehaviour
{
    [Header("Risk/Reward Settings")]
    [SerializeField] private int _damageCost = 25;
    [SerializeField] private int _runeReward = 20;
    [SerializeField] private string _dialogueText = "Aceitar dano em troca de runas? Pressione E";

    private InputAction _interactAction;
    private bool _playerInRange;
    private bool _isUsed;

    private void Awake()
    {
        _interactAction = new InputAction("Interact", InputActionType.Button);
        _interactAction.AddBinding("<Keyboard>/e");
        _interactAction.AddBinding("<Gamepad>/buttonEast");
    }

    private void OnEnable()
    {
        _interactAction.Enable();
        _interactAction.performed += OnInteractPerformed;
    }

    private void OnDisable()
    {
        _interactAction.performed -= OnInteractPerformed;
        _interactAction.Disable();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        _playerInRange = true;

        if (!_isUsed && DialogueUI.Instance != null)
        {
            DialogueUI.Instance.Show(_dialogueText);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        _playerInRange = false;

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.Hide();
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (!_playerInRange || _isUsed) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            PlayerController pc = FindFirstObjectByType<PlayerController>();
            if (pc != null) player = pc.gameObject;
        }
        if (player == null) return;

        IDamageable damageable = player.GetComponent<IDamageable>();
        if (damageable != null)
        {
            _isUsed = true;
            damageable.TakeDamage(_damageCost);

            if (RunCurrency.Instance != null)
            {
                RunCurrency.Instance.AddRunes(_runeReward);
            }

            if (DialogueUI.Instance != null)
            {
                DialogueUI.Instance.Hide();
            }

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.gray;
            }
        }
    }
}
