using UnityEngine;
using UnityEngine.InputSystem;

public class NPCInteractable : MonoBehaviour
{
    [SerializeField] private string _dialogueText = "Hello, traveler!";

    private InputAction _interactAction;
    private bool _playerInRange;
    private bool _isShowingDialogue;

    public string DialogueText => _dialogueText;

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
        if (other.gameObject.layer != LayerMask.NameToLayer("player")) return;
        _playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("player")) return;
        _playerInRange = false;

        if (_isShowingDialogue)
        {
            DialogueUI.Instance.Hide();
            _isShowingDialogue = false;
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (!_playerInRange) return;

        if (_isShowingDialogue)
        {
            DialogueUI.Instance.Hide();
            _isShowingDialogue = false;
        }
        else
        {
            DialogueUI.Instance.Show(_dialogueText);
            _isShowingDialogue = true;
        }
    }
}
