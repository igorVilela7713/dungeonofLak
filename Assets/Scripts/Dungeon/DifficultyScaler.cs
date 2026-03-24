public static class DifficultyScaler
{
    public static float GetHPMultiplier(int floor, float scalingPerFloor)
    {
        return 1f + (floor - 1) * scalingPerFloor;
    }

    public static float GetDamageMultiplier(int floor, float scalingPerFloor)
    {
        return 1f + (floor - 1) * scalingPerFloor;
    }

    public static float GetRuneMultiplier(int floor, float scalingPerFloor)
    {
        return 1f + (floor - 1) * scalingPerFloor;
    }

    public static float GetRoomWidthMultiplier(int floor, float scalingPerFloor)
    {
        return 1f + (floor - 1) * scalingPerFloor;
    }
}
