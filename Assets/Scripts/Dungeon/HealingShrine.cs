using UnityEngine;

public class HealingShrine : MonoBehaviour
{
    [SerializeField] private float _healPercent = 0.25f;
    [SerializeField] private string _dialogueText = "Você sente sua força retornar...";

    private bool _isUsed;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isUsed) return;
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        if (ph == null) return;

        _isUsed = true;
        ph.HealPercent(_healPercent);

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.Show(_dialogueText);
            Invoke(nameof(HideDialogue), 2f);
        }

        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = Color.gray;
        }
    }

    private void HideDialogue()
    {
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.Hide();
        }
    }
}
