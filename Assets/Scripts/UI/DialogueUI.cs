using UnityEngine;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    [SerializeField] private GameObject _dialoguePanel;
    [SerializeField] private TextMeshProUGUI _dialogueText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Show(string text)
    {
        _dialogueText.text = text;
        _dialoguePanel.SetActive(true);
    }

    public void Hide()
    {
        _dialoguePanel.SetActive(false);
    }
}
