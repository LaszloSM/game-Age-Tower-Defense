# Project Overview - Fix Camera, Spawn, and Resource Logic
- Goal: Fix the "disappearing decorations" (camera issue), ensure PAWNs are visible, and implement robust individual resource logic.
- Assets: Tiny Swords PAWNs and Resources.

# Implementation Steps

## 1. Fix Camera and Player Spawn
- **Action**: Move `PlayerSpawner` and its `spawnPoint` to the same Y-level as the Castle (Y=17.6).
- **Reason**: The `CameraFollow` script snaps to the player. If the player spawns at (0,0), the camera moves away from the castle and decorations (at Y=17.6).

## 2. Robust Individual Resource Logic (`ResourceNode.cs`)
- **Action**: Update `ResourceNode.cs` to handle individual state transitions and visual feedback.
- **Features**:
    - Independent respawn timer.
    - Sprite swap for "Full" and "Depleted" states (e.g., Tree -> Stump).
    - Animation triggers for `Idle` and `Work`.
    - `Harvest()` method that handles local depletion logic.

## 3. Update PAWN Logic (`Pawn.cs`)
- **Action**: Ensure `Pawn.cs` correctly plays animations based on Faction and Tool.
- **Verification**: Ensure the naming convention `Pawn_[State] [Tool]_[Faction]` matches your animator controllers.

## 4. Setup Scene Objects
- **Action**: Use a script to:
    - Set `SortingOrder = 10` for PAWNs.
    - Assign correct `idleAnimationName` and `workAnimationName` to all scene `ResourceNodes`.
    - Map specific "Stump" sprites to tree nodes based on their names.
    - Position `PlayerSpawner` at `(0, 17, 0)`.

# Key Scripts
- `ResourceNode.cs`: Refined for better individual behavior.
- `Pawn.cs`: Already mostly correct, will ensure Start() plays Idle.
- `SheepAI.cs`: Already implemented for sheep movement.

# Verification & Testing
1. **Initial Start**: Verify camera starts at the Castle/Player and decorations are visible.
2. **PAWN Visibility**: Verify 3 PAWNs are visible near the castle.
3. **Gathering Loop**: Verify PAWN moves to resource, plays interact animation, resource plays work animation, and PAWN returns with resources.
4. **Respawn**: Verify resource turns into stump/empty state and reappears after `respawnSeconds`.
