using NUnit.Framework;

public class DifficultyScalerTests
{
    // GetHPMultiplier

    [Test]
    public void GetHPMultiplier_Floor1_ReturnsBaseValue()
    {
        float result = DifficultyScaler.GetHPMultiplier(1, 0.1f);
        Assert.AreEqual(1f, result);
    }

    [Test]
    public void GetHPMultiplier_Floor0_ReturnsLessThanBase()
    {
        float result = DifficultyScaler.GetHPMultiplier(0, 0.1f);
        Assert.AreEqual(0.9f, result, 0.001f);
    }

    [Test]
    public void GetHPMultiplier_Floor2_ScalesCorrectly()
    {
        float result = DifficultyScaler.GetHPMultiplier(2, 0.1f);
        Assert.AreEqual(1.1f, result, 0.001f);
    }

    [Test]
    public void GetHPMultiplier_Floor5_ScalesCorrectly()
    {
        float result = DifficultyScaler.GetHPMultiplier(5, 0.1f);
        Assert.AreEqual(1.4f, result, 0.001f);
    }

    [Test]
    public void GetHPMultiplier_Floor50_HighValue()
    {
        float result = DifficultyScaler.GetHPMultiplier(50, 0.1f);
        Assert.AreEqual(5.9f, result, 0.001f);
    }

    [TestCase(1, 0.1f, 1.0f)]
    [TestCase(2, 0.1f, 1.1f)]
    [TestCase(5, 0.1f, 1.4f)]
    [TestCase(10, 0.1f, 1.9f)]
    [TestCase(50, 0.1f, 5.9f)]
    public void GetHPMultiplier_Parameterized(int floor, float scaling, float expected)
    {
        float result = DifficultyScaler.GetHPMultiplier(floor, scaling);
        Assert.AreEqual(expected, result, 0.001f);
    }

    [Test]
    public void GetHPMultiplier_ZeroScaling_AlwaysReturnsBase()
    {
        float result = DifficultyScaler.GetHPMultiplier(10, 0f);
        Assert.AreEqual(1f, result);
    }

    // GetDamageMultiplier

    [Test]
    public void GetDamageMultiplier_Floor1_ReturnsBaseValue()
    {
        float result = DifficultyScaler.GetDamageMultiplier(1, 0.1f);
        Assert.AreEqual(1f, result);
    }

    [TestCase(1, 0.08f, 1.0f)]
    [TestCase(5, 0.08f, 1.32f)]
    [TestCase(10, 0.08f, 1.72f)]
    public void GetDamageMultiplier_Parameterized(int floor, float scaling, float expected)
    {
        float result = DifficultyScaler.GetDamageMultiplier(floor, scaling);
        Assert.AreEqual(expected, result, 0.001f);
    }

    // GetRuneMultiplier

    [Test]
    public void GetRuneMultiplier_Floor1_ReturnsBaseValue()
    {
        float result = DifficultyScaler.GetRuneMultiplier(1, 0.1f);
        Assert.AreEqual(1f, result);
    }

    [TestCase(1, 0.05f, 1.0f)]
    [TestCase(10, 0.05f, 1.45f)]
    [TestCase(20, 0.05f, 1.95f)]
    public void GetRuneMultiplier_Parameterized(int floor, float scaling, float expected)
    {
        float result = DifficultyScaler.GetRuneMultiplier(floor, scaling);
        Assert.AreEqual(expected, result, 0.001f);
    }

    // GetRoomWidthMultiplier

    [Test]
    public void GetRoomWidthMultiplier_Floor1_ReturnsBaseValue()
    {
        float result = DifficultyScaler.GetRoomWidthMultiplier(1, 0.05f);
        Assert.AreEqual(1f, result);
    }

    [Test]
    public void GetRoomWidthMultiplier_Floor20_ScalesCorrectly()
    {
        float result = DifficultyScaler.GetRoomWidthMultiplier(20, 0.05f);
        Assert.AreEqual(1.95f, result, 0.001f);
    }

    // Progressive growth

    [Test]
    public void AllMultipliers_ProgressiveGrowth_IncreasesMonotonically()
    {
        float scaling = 0.1f;
        float previousHP = DifficultyScaler.GetHPMultiplier(1, scaling);

        for (int floor = 2; floor <= 20; floor++)
        {
            float currentHP = DifficultyScaler.GetHPMultiplier(floor, scaling);
            Assert.Greater(currentHP, previousHP,
                $"HP multiplier should increase from floor {floor - 1} to {floor}");
            previousHP = currentHP;
        }
    }

    [Test]
    public void AllMultipliers_UseSameFormula_ConsistentResults()
    {
        int floor = 7;
        float scaling = 0.1f;

        float hp = DifficultyScaler.GetHPMultiplier(floor, scaling);
        float dmg = DifficultyScaler.GetDamageMultiplier(floor, scaling);
        float rune = DifficultyScaler.GetRuneMultiplier(floor, scaling);
        float width = DifficultyScaler.GetRoomWidthMultiplier(floor, scaling);

        Assert.AreEqual(hp, dmg, "HP and Damage multipliers should use the same formula");
        Assert.AreEqual(hp, rune, "HP and Rune multipliers should use the same formula");
        Assert.AreEqual(hp, width, "HP and Width multipliers should use the same formula");
    }
}
