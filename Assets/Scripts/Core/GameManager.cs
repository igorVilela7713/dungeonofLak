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
        SceneManager.LoadScene(_sceneName);
    }
}
