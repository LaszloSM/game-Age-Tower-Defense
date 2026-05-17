using NUnit.Framework;
using UnityEngine;

public class BuildingManagerTests
{
    BuildingManager _bm;

    [SetUp] public void SetUp() => _bm = new GameObject().AddComponent<BuildingManager>();
    [TearDown] public void TearDown() => Object.DestroyImmediate(_bm.gameObject);

    [Test] public void OccupySlot_MarksSlotOccupied()
    {
        _bm.OccupySlot(0);
        Assert.IsTrue(_bm.IsSlotOccupied(0));
    }

    [Test] public void FreeSlot_MarksSlotFree()
    {
        _bm.OccupySlot(2);
        _bm.FreeSlot(2);
        Assert.IsFalse(_bm.IsSlotOccupied(2));
    }

    [Test] public void OccupiedCount_TracksCorrectly()
    {
        _bm.OccupySlot(0);
        _bm.OccupySlot(1);
        Assert.AreEqual(2, _bm.OccupiedCount);
    }
}
