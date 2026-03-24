using UnityEngine;

public class RunCurrency : MonoBehaviour
{
    public static RunCurrency Instance { get; private set; }

    [SerializeField] private FloorUI _floorUI;

    private int _currentRunes;

    public int CurrentRunes => _currentRunes;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddRunes(int amount)
    {
        _currentRunes += amount;
        UpdateUI();
    }

    public bool SpendRunes(int amount)
    {
        if (_currentRunes < amount) return false;
        _currentRunes -= amount;
        UpdateUI();
        return true;
    }

    public void ResetRunes()
    {
        _currentRunes = 0;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_floorUI != null)
        {
            _floorUI.UpdateRunes(_currentRunes);
        }
    }
}
