# Project Overview - Updated
- Goal: Fix visibility and logic for PAWNs and Resources.
- PAWNs: Must be visible (Sorting Order) and functional.
- Resources: Must have Idle/Work animations and movement (Sheep).

# Implementation Steps
1. **Create SheepAI.cs**: Simple wandering logic for sheep.
2. **Update ResourceNode.cs**: Support for Idle animations and better synchronization with PAWN interaction.
3. **Configure Resource Prefabs**:
    - Update `SheepNode` with `SheepAI`.
    - Ensure all nodes have `idleAnimationName` and `workAnimationName`.
    - Verify/Fix controllers for Gold and Trees.
4. **Fix PAWN Visibility**: Set Sorting Order to 10 in prefabs and scene instances.
5. **Ensure Logic Integration**: Check that clicking works (PawnClickHandler is on Pawns).

# Key Scripts
- `SheepAI.cs`: Handles random movement within a radius.
- `ResourceNode.cs`: Triggers animations and handles harvest.
- `Pawn.cs`: Already updated but will double check tool assignment.
