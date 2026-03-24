using UnityEngine;

public class RunePickup : MonoBehaviour
{
    [SerializeField] private int _runeValue = 1;

    public void Initialize(int value)
    {
        _runeValue = value;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        if (RunCurrency.Instance == null) return;

        RunCurrency.Instance.AddRunes(_runeValue);
        Destroy(gameObject);
    }
}
