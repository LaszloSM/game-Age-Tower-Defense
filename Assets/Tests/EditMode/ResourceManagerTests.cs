using NUnit.Framework;
using UnityEngine;

public class ResourceManagerTests
{
    ResourceManager _rm;

    [SetUp]
    public void SetUp()
    {
        var go = new GameObject();
        _rm = go.AddComponent<ResourceManager>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_rm.gameObject);
    }

    [Test] public void TroopCap_NoFood_IsBaseline()
        => Assert.AreEqual(5, ResourceManager.CalculateTroopCap(0));

    [Test] public void TroopCap_20Food_Is7()
        => Assert.AreEqual(7, ResourceManager.CalculateTroopCap(20));

    [Test] public void TroopCap_150Food_ClampsAt20()
        => Assert.AreEqual(20, ResourceManager.CalculateTroopCap(150));

    [Test] public void CanSpend_WhenEnough_ReturnsTrue()
    {
        _rm.AddResource(ResourceType.Wood, 100);
        Assert.IsTrue(_rm.CanSpend(ResourceType.Wood, 80));
    }

    [Test] public void CanSpend_WhenNotEnough_ReturnsFalse()
    {
        _rm.AddResource(ResourceType.Wood, 50);
        Assert.IsFalse(_rm.CanSpend(ResourceType.Wood, 80));
    }

    [Test] public void Spend_DeductsCorrectly()
    {
        _rm.AddResource(ResourceType.Gold, 100);
        _rm.Spend(ResourceType.Gold, 40);
        Assert.AreEqual(60, _rm.GetAmount(ResourceType.Gold));
    }
}
