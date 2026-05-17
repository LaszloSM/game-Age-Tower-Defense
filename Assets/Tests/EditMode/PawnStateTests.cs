using NUnit.Framework;
using UnityEngine;

public class PawnStateTests
{
    Pawn _pawn;

    [SetUp] public void SetUp() => _pawn = new GameObject().AddComponent<Pawn>();
    [TearDown] public void TearDown() => Object.DestroyImmediate(_pawn.gameObject);

    [Test] public void Pawn_StartsIdle()
        => Assert.AreEqual(PawnState.Idle, _pawn.State);

    [Test] public void SetStateIdle_AfterDepletion()
    {
        _pawn.OnNodeDepleted();
        Assert.AreEqual(PawnState.Idle, _pawn.State);
    }
}
