using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private string _sceneName = "Hub";
    [SerializeField] private PlayerHealth _playerHealth;

    private void Awake()
    {
        if (_playerHealth != null)
        {
            _playerHealth.OnDeath.AddListener(OnPlayerDeath);
        }
    }

    private void OnPlayerDeath()
    {
        if (FloorManager.Instance != null)
        {
            FloorManager.Instance.ResetFloor();
            Destroy(FloorManager.Instance.gameObject);
        }
        if (RunCurrency.Instance != null)
        {
            RunCurrency.Instance.ResetRunes();
            Destroy(RunCurrency.Instance.gameObject);
        }
        if (RunUpgradeManager.Instance != null)
        {
            RunUpgradeManager.Instance.ResetUpgrades();
            Destroy(RunUpgradeManager.Instance.gameObject);
        }

        SceneManager.LoadScene(_sceneName);
    }
}
