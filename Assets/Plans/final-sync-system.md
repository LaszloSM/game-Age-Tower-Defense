# Project Overview - Final Synchronization
- Goal: Fix camera offset, unify resource logic, and ensure PAWN visual fidelity.
- Assets: Tiny Swords PAWNs and Resources.

# Implementation Steps

## 1. Fix Scene Synchronization
- **Camera**: Move Main Camera to `(1.6, 17.6, -10)` in `Game.unity`.
- **Spawners**: Move `PlayerSpawner` and `PawnSpawner` to the Castle area.
- **EventSystem**: Ensure a functional EventSystem is in the scene for clicking.

## 2. Universal Resource Logic (`ResourceNode.cs`)
- **Action**: Update `ResourceNode` to handle all types (Wood, Gold, Meat) in a single script.
- **Visuals**:
    - Manage `Idle` and `Work` animations.
    - Manage `Full` and `Depleted` sprites.
    - Custom re-growth/re-spawn logic.

## 3. PAWN Visual Polish
- **Sprites**: Set default sprite to `Pawn_Idle` in prefabs.
- **Animator**: Verify the Animator is playing `Pawn_Idle_[Faction]` by default.
- **Sorting**: Maintain `Sorting Order 10`.

## 4. Automatic Scene Re-Binding
- Use a script to re-process all 66 children of `Decoraciones`:
    - Assign `ResourceNode`.
    - Detect type by name (Tree, Gold, Sheep).
    - Assign correct Animator Controllers.
    - Assign correct Sprites (including Stumps for trees).

# Key Scripts
- `ResourceNode.cs`: Data-driven resource behavior.
- `Pawn.cs`: Faction and tool-based animation management.
- `SheepAI.cs`: Independent movement logic.

# Verification
1. Camera centered on Castle.
2. 3 PAWNs visible.
3. Resources respond to clicks and play animations.
4. Resources respawn with correct visuals.
