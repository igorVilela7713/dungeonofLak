using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class RunCurrencyTests
{
    private GameObject _currencyObj;
    private RunCurrency _currency;

    [SetUp]
    public void SetUp()
    {
        // Reset singleton
        ResetSingleton();

        _currencyObj = new GameObject("RunCurrency");
        _currency = _currencyObj.AddComponent<RunCurrency>();
        _ = RunCurrency.Instance; // trigger Awake
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_currencyObj);
        ResetSingleton();
    }

    private void ResetSingleton()
    {
        var field = typeof(RunCurrency).GetProperty("Instance",
            BindingFlags.Public | BindingFlags.Static);
        field?.SetValue(null, null);
    }

    // Initial state

    [Test]
    public void CurrentRunes_Initial_IsZero()
    {
        Assert.AreEqual(0, _currency.CurrentRunes);
    }

    // AddRunes

    [Test]
    public void AddRunes_IncreasesBalance()
    {
        _currency.AddRunes(10);

        Assert.AreEqual(10, _currency.CurrentRunes);
    }

    [Test]
    public void AddRunes_CalledMultipleTimes_Accumulates()
    {
        _currency.AddRunes(5);
        _currency.AddRunes(3);
        _currency.AddRunes(7);

        Assert.AreEqual(15, _currency.CurrentRunes);
    }

    // SpendRunes

    [Test]
    public void SpendRunes_WithSufficientFunds_ReturnsTrueAndDecreases()
    {
        _currency.AddRunes(20);
        bool result = _currency.SpendRunes(10);

        Assert.IsTrue(result);
        Assert.AreEqual(10, _currency.CurrentRunes);
    }

    [Test]
    public void SpendRunes_WithInsufficientFunds_ReturnsFalseAndNoChange()
    {
        _currency.AddRunes(5);
        bool result = _currency.SpendRunes(10);

        Assert.IsFalse(result);
        Assert.AreEqual(5, _currency.CurrentRunes);
    }

    [Test]
    public void SpendRunes_ExactBalance_ReturnsTrueAndZero()
    {
        _currency.AddRunes(10);
        bool result = _currency.SpendRunes(10);

        Assert.IsTrue(result);
        Assert.AreEqual(0, _currency.CurrentRunes);
    }

    [Test]
    public void SpendRunes_ZeroCost_ReturnsTrue()
    {
        _currency.AddRunes(5);
        bool result = _currency.SpendRunes(0);

        Assert.IsTrue(result);
        Assert.AreEqual(5, _currency.CurrentRunes);
    }

    // ResetRunes

    [Test]
    public void ResetRunes_SetsBalanceToZero()
    {
        _currency.AddRunes(100);
        _currency.ResetRunes();

        Assert.AreEqual(0, _currency.CurrentRunes);
    }

    [Test]
    public void ResetRunes_WhenAlreadyZero_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _currency.ResetRunes());
        Assert.AreEqual(0, _currency.CurrentRunes);
    }

    // Spend then add

    [Test]
    public void SpendAndAdd_Combination_CorrectBalance()
    {
        _currency.AddRunes(50);
        _currency.SpendRunes(30);
        _currency.AddRunes(15);

        Assert.AreEqual(35, _currency.CurrentRunes);
    }
}
