using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string _targetScene;

    private bool _isTransitioning;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isTransitioning) return;
        if (other.gameObject.layer != LayerMask.NameToLayer("player")) return;

        _isTransitioning = true;
        SceneManager.LoadScene(_targetScene);
    }
}
