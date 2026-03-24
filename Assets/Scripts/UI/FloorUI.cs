using UnityEngine;
using TMPro;

public class FloorUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _floorText;
    [SerializeField] private TextMeshProUGUI _runeText;

    public void UpdateFloor(int floorNumber)
    {
        if (_floorText != null)
        {
            _floorText.text = "Floor " + floorNumber;
        }
    }

    public void UpdateRunes(int runeCount)
    {
        if (_runeText != null)
        {
            _runeText.text = "Runas: " + runeCount;
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
