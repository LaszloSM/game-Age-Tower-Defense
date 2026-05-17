using NUnit.Framework;
using UnityEngine;

public class BuildingTests
{
    Building _b;

    [SetUp] public void SetUp() => _b = new GameObject().AddComponent<Building>();
    [TearDown] public void TearDown()
    {
        if (_b != null) Object.DestroyImmediate(_b.gameObject);
    }

    [Test] public void Building_StartsAtFullHealth()
    {
        _b.InitForTest(200f);
        Assert.AreEqual(200f, _b.CurrentHP);
    }

    [Test] public void TakeDamage_ReducesHP()
    {
        _b.InitForTest(200f);
        _b.TakeDamage(50f);
        Assert.AreEqual(150f, _b.CurrentHP);
    }

    [Test] public void TakeDamage_ToZero_FiresOnDestroyed()
    {
        _b.InitForTest(50f);
        bool fired = false;
        _b.OnDestroyed += () => fired = true;
        _b.TakeDamage(100f);
        Assert.IsTrue(fired);
    }

    [Test] public void Repair_DoesNotExceedMaxHP()
    {
        _b.InitForTest(200f);
        _b.TakeDamage(80f);
        _b.Repair(500f);
        Assert.AreEqual(200f, _b.CurrentHP);
    }

    [Test] public void IsFullHealth_TrueAtMax()
    {
        _b.InitForTest(100f);
        Assert.IsTrue(_b.IsFullHealth);
    }
}
