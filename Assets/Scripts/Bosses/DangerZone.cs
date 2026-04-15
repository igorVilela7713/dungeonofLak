using UnityEngine;

public class DangerZone : MonoBehaviour
{
    [SerializeField] private int _damagePerTick = 5;
    [SerializeField] private float _tickInterval = 0.5f;
    [SerializeField] private float _duration = 4f;

    private float _tickTimer;
    private SpriteRenderer _sr;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr != null) _sr.color = new Color(1f, 0f, 0f, 0.4f);
    }

    public void Initialize(float duration)
    {
        _duration = duration;
        Destroy(gameObject, _duration);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        _tickTimer -= Time.deltaTime;
        if (_tickTimer > 0) return;

        int playerLayer = LayerMask.NameToLayer("Player");
        if (other.gameObject.layer == playerLayer)
        {
            IDamageable d = other.GetComponent<IDamageable>();
            if (d != null) d.TakeDamage(_damagePerTick);
            _tickTimer = _tickInterval;
        }
    }
}
