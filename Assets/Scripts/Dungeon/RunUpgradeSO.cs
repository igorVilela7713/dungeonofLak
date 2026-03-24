using UnityEngine;

public enum UpgradeType
{
    Damage,
    Speed,
    MaxHealth,
    CritChance
}

[CreateAssetMenu(menuName = "Dungeon/Run Upgrade")]
public class RunUpgradeSO : ScriptableObject
{
    public string upgradeName;
    [TextArea] public string description;
    public int cost;
    public UpgradeType upgradeType;
    public float value;
    public bool hasTradeOff;
    public UpgradeType tradeOffType;
    public float tradeOffValue;
}
