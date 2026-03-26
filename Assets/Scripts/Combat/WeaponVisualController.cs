using UnityEngine;

public class WeaponVisualController : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private GameObject _visualSword;
    [SerializeField] private GameObject _visualSpear;
    [SerializeField] private GameObject _visualAxe;
    [SerializeField] private GameObject _visualDagger;

    private GameObject _currentVisual;
    private SpriteRenderer _currentSpriteRenderer;
    private PlayerController _playerController;
    private Vector3 _originalLocalPos;

    private void Awake()
    {
        _playerController = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        if (_playerController != null && _currentSpriteRenderer != null)
        {
            bool facingLeft = _playerController.FacingDirection.x < 0;
            _currentSpriteRenderer.flipX = facingLeft;

            if (_currentVisual != null)
            {
                Vector3 pos = _currentVisual.transform.localPosition;
                _currentVisual.transform.localPosition = new Vector3(
                    facingLeft ? -Mathf.Abs(_originalLocalPos.x) : Mathf.Abs(_originalLocalPos.x),
                    pos.y,
                    pos.z
                );
            }
        }
    }

    public void EquipVisual(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Sword:
                SetActiveVisual(_visualSword);
                break;
            case WeaponType.Spear:
                SetActiveVisual(_visualSpear);
                break;
            case WeaponType.Axe:
                SetActiveVisual(_visualAxe);
                break;
            case WeaponType.Dagger:
                SetActiveVisual(_visualDagger);
                break;
        }
    }

    private void SetActiveVisual(GameObject visual)
    {
        if (_visualSword != null) _visualSword.SetActive(false);
        if (_visualSpear != null) _visualSpear.SetActive(false);
        if (_visualAxe != null) _visualAxe.SetActive(false);
        if (_visualDagger != null) _visualDagger.SetActive(false);

        if (visual != null)
        {
            visual.SetActive(true);
        }

        _currentVisual = visual;
        _currentSpriteRenderer = visual != null ? visual.GetComponent<SpriteRenderer>() : null;

        if (visual != null)
        {
            _originalLocalPos = visual.transform.localPosition;
        }
    }
}
