using NUnit.Framework;
using UnityEngine;

public class UnitTests
{
    Unit _u;

    [SetUp] public void SetUp() => _u = new GameObject().AddComponent<Unit>();
    [TearDown] public void TearDown() => Object.DestroyImmediate(_u.gameObject);

    [Test] public void Unit_StartsAtFullHP()
    {
        _u.InitForTest(100f, Faction.Blue);
        Assert.AreEqual(100f, _u.CurrentHP);
    }

    [Test] public void TakeDamage_ReducesHP()
    {
        _u.InitForTest(100f, Faction.Blue);
        _u.TakeDamage(30f);
        Assert.AreEqual(70f, _u.CurrentHP);
    }

    [Test] public void TakeDamage_ToZero_FiresOnDied()
    {
        bool died = false;
        _u.InitForTest(50f, Faction.Blue);
        _u.OnDied += () => died = true;
        _u.TakeDamage(999f);
        Assert.IsTrue(died);
    }

    [Test] public void Heal_DoesNotExceedMaxHP()
    {
        _u.InitForTest(100f, Faction.Blue);
        _u.TakeDamage(20f);
        _u.Heal(999f);
        Assert.AreEqual(100f, _u.CurrentHP);
    }

    [Test] public void Faction_SetCorrectly()
    {
        _u.InitForTest(100f, Faction.Red);
        Assert.AreEqual(Faction.Red, _u.UnitFaction);
    }
}
