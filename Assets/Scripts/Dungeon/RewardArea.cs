using UnityEngine;

public class RewardArea : MonoBehaviour
{
    [SerializeField] private FloorTransition _exitTrigger;

    public void Initialize(Transform player)
    {
        if (_exitTrigger != null)
        {
            _exitTrigger.gameObject.SetActive(true);
        }
    }
}
