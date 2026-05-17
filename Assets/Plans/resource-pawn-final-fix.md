# Project Overview - Fix & Polish
- Goal: Ensure PAWNs are visible, resources have Idle/Work animations, and Sheep have movement.
- Faction: Blue/Red must be respected in all visuals.

# Implementation Steps
1. **Properly Implement Scripts**: Use `CodeEdit` to ensure `SheepAI.cs`, `ResourceNode.cs`, and `Pawn.cs` are correctly written and compiled.
2. **Configure Resource Prefabs**:
    - **Sheep**: Add `SheepAI`, set `idleAnimationName` to "Sheep_Idle", `workAnimationName` to "Sheep_Grass" (or none if harvest is immediate).
    - **Gold**: Set `idleAnimationName` to "Gold Resource_Static", `workAnimationName` to "Gold Resource_Highlight".
    - **Tree**: Set `idleAnimationName` to "Tree 1_Idle", `workAnimationName` to "Tree 1_Chopped".
3. **Fix Visibility**:
    - Update `BluePawn` and `RedPawn` prefabs: Set `SpriteRenderer.sortingOrder = 10`.
    - Update existing scene instances if any.
4. **Final Polish**: Verify `Pawn.cs` starts with "Idle" animation and respects faction.
