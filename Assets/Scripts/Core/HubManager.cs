using UnityEngine;

public class HubManager : MonoBehaviour
{
    [SerializeField] private PlayerHealth _playerHealth;

    private void Start()
    {
        if (SaveSystem.HasSave())
        {
            SaveData data = SaveSystem.Load();
        }
    }
}
