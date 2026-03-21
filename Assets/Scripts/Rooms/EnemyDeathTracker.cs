using UnityEngine;

public class EnemyDeathTracker : MonoBehaviour
{
    private RoomController _room;
    
    public void Initialize(RoomController room)
    {
        _room = room;
    }
    
    private void OnDestroy()
    {
        if (_room != null)
        {
            _room.OnEnemyKilled();
        }
    }
}
