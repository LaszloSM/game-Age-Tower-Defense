# Project Overview
- Game Title: Tower Defense Resource Management
- High-Level Concept: Players manage P.A.W.N.s to gather Wood, Gold, and Meat to build and sustain their defenses.
- Players: Single player.
- Inspiration / Reference Games: RTS/Tower Defense hybrids like Age of Empires or Kingdom.
- Tone / Art Direction: Tiny Swords (Stylized, colorful).
- Target Platform: PC (Windows).
- Screen Orientation / Resolution: Landscape.
- Render Pipeline: Built-in.

# Game Mechanics
## Core Gameplay Loop
1. Game starts: 3 PAWNs are spawned at the Castle.
2. Player selects a PAWN and clicks on a Resource Node (Tree, Gold Mine, Sheep).
3. PAWN moves to the node, playing the correct "Run" animation with the tool.
4. PAWN works at the node, playing the "Interact" animation and triggering the resource's "Work" animation (e.g., tree falling).
5. PAWN moves back to the Castle carrying the resource, playing the "Run [Resource]" animation.
6. Resources are added to the `ResourceManager`.

## Controls and Input Methods
- Mouse Click: Select PAWN, then Click on Resource/Building to assign task.
- Automated behavior: PAWNs return to work after depositing unless the node is depleted.

# UI
- Resource HUD (Existing): Displays Gold, Wood, and Food.
- Pawn Selection: Highlight selected PAWN.

# Key Asset & Context
- `Pawn.cs`: Main logic for movement, gathering, and animation control.
- `ResourceNode.cs`: Updated to handle animations (Chopped for trees, Highlight for gold).
- `PawnSpawner.cs`: Ensures 3 PAWNs are spawned at the Castle at start.
- `PawnAnimatorHelper.cs` (New): Utility to construct animation state names based on Faction, State, and Tool.

# Implementation Steps
1. **Update `Pawn.cs` Animation Logic**:
    - Add logic to construct animation names like `Pawn_[State] [Tool]_[Faction]`.
    - Implement tool selection based on `ResourceType`: Wood -> Axe, Gold -> Pickaxe, Food -> Knife, Repair -> Hammer.
    - Update `GatherRoutine` to play carrying animations when returning to Castle.

2. **Enhance `ResourceNode.cs`**:
    - Add `Animator` reference.
    - Add `PlayWorkAnimation()` method to trigger "Chopped" (Trees) or "Highlight" (Gold).
    - Ensure `Harvest()` calls this method.

3. **Configure Resource Prefabs**:
    - Add `Animator` to `GoldNode`, `SheepNode`, and `TreeNode` prefabs.
    - Assign the corresponding Animator Controllers and Animation Clips discovered earlier.

4. **Update `PawnSpawner.cs`**:
    - Modify `Start()` to spawn exactly 3 PAWNs at the Castle's position (or `castleSpawnPoint`).

5. **Verify Faction Visuals**:
    - Ensure `Pawn.cs` correctly identifies its faction (from the prefab or global setting) and uses it in animation strings.

# Verification & Testing
1. **Initial Spawn**: Start the game and verify 3 PAWNs appear at the Castle.
2. **Wood Gathering**: Assign PAWN to Tree. Check for "Axe" run, "Axe" interact, tree falling animation, and "Wood" carry run.
3. **Gold Gathering**: Assign PAWN to Gold. Check for "Pickaxe" run, "Pickaxe" interact, and "Gold" carry run.
4. **Meat Gathering**: Assign PAWN to Sheep. Check for "Knife" run, "Knife" interact, and "Meat" carry run.
5. **Faction Check**: Switch to Red faction and verify Red PAWN sprites and animations are used.
