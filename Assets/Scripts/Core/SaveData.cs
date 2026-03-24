[System.Serializable]
public class SaveData
{
    public int currentHealth;
    public int maxHealth;
    public int maxFloorReached = 1;

    public static SaveData CreateDefault()
    {
        return new SaveData
        {
            currentHealth = 100,
            maxHealth = 100,
            maxFloorReached = 1
        };
    }
}
