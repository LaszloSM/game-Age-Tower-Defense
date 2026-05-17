# Project Overview
- Game Title: Age Tower Defense (Tiny Sword Style)
- High-Level Concept: A top-down 2D real-time tower defense game where players defend their castle from waves of enemies by building structures and managing resources.
- Players: Single player.
- Inspiration / Reference Games: Classic RTS and Tower Defense games (Age of Empires, Kingdom Rush).
- Tone / Art Direction: Bright, whimsical, hand-drawn medieval style (Tiny Sword asset pack).
- Target Platform: PC (Standalone Windows).
- Screen Orientation / Resolution: Landscape 1920x1080.
- Render Pipeline: Built-in.

# Game Mechanics
## Core Gameplay Loop
1. Gather resources (Wood, Gold, Food) using Pawns.
2. Build defensive structures (Barracks, Archery, Towers) in designated slots.
3. Train units to defend the Castle.
4. Survive increasingly difficult enemy waves.
5. Upgrade buildings and use special abilities (Troop Boost, Emergency Repair).

## Controls and Input Methods
- Mouse Click: Select units/buildings, assign pawns to nodes, click slots to build.
- ESC: Pause/Resume game.
- UI Buttons: Trigger builds, upgrades, and abilities.

# UI
- Top-Left: Castle Health Bar (Ribbon style) and Gold Counter.
- Top-Right: Resource Counters (Wood, Gold, Food).
- Bottom: Build Panel (context-sensitive), Pawn/Building Info panels.
- Overlays: Pause and Game Over menus with stats.

# Key Asset & Context
- **Terrain:** Tiny Sword `Tilemap_color1.png` for grass and stone walls (elevation).
- **Buildings:** Blue Castle (`Blue Buildings/Castle.png`), Blue House (`Blue Buildings/House1.png`).
- **Units:** Blue Warrior (`Blue Units/Warrior/`), Red TNT Goblins (`Red Units/`).
- **Decorations:** Pine Trees, Bushes, Mushrooms, Bones, Flags.

# Implementation Steps
## Phase 1: Map Layout & World Building
1. **Grid & Tilemap Setup in Game Scene**
   - Configure a Grid object with multiple Tilemap layers: `Ground` (Sort 0), `Elevation/Walls` (Sort 1), `Path` (Sort 2), `Decorations` (Sort 3).
   - Use `Tilemap_color1.png` to create a Tile Palette.
   - **Ground:** Paint the entire scene with mixed grass tiles.
   - **Elevation:** Recreate the "blue stone wall" defensive layout from the reference image, forming a multi-tiered defensive area around the castle.
2. **Key Structure Placement**
   - Place the `BlueCastle` prefab at the top-center on a raised elevation.
   - Place a `BlueHouse` prefab at the bottom-center to mimic the reference scene's composition.
3. **Environment Detailing**
   - Add Pine Trees (`Pawn and Resources/Wood/Trees/`) behind the castle.
   - Scatter Bushes, Mushrooms, and Bones across the grass to match the density in the image.
   - Place Red Flags on poles and Directional Signs near path junctions.

## Phase 2: Logic Integration (Based on existing plan)
1. **Pathing & Waypoints**
   - Set up `PathManager` with waypoints following the natural corridors created by the stone walls.
2. **Resource Nodes**
   - Place Trees, Gold Rocks, and Sheep in the areas around the defensive lines.
3. **Building Slots**
   - Position `BuildingSlot` prefabs alongside the paths and near the castle.

## Phase 3: UI Implementation
1. **Style Matching**
   - Build the HUD using `Tiny Sword/UI Elements/` (Banners, Ribbons, Papers).
   - Implement the Pink Ribbon Health Bar and the Gold Counter in the top-left as seen in the reference.

# Verification & Testing
1. **Visual Accuracy:** Compare the Game view with the reference image. The "feel" and density of decorations should match.
2. **Layering Check:** Ensure units walk behind walls but in front of the grass.
3. **Navigation Test:** Verify enemies follow the paths created by the walls and that waypoints are correctly positioned.
