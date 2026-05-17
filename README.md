# Age Tower Defense

A real-time strategy / tower defense game built in **Unity 6** using the **Tiny Sword** pixel art asset pack.

Defend your castle against endless waves of enemy troops. Gather resources, build defensive structures, train your own army, and survive as long as you can.

---

## Gameplay

### Choose Your Faction
Select **Blue** or **Red** faction at the start. Your castle, buildings, pawns, and troops will all match your chosen color.

### Gather Resources
Send **Pawns** to harvest resource nodes scattered across the map:
- 🪵 **Wood** — from trees (axe)
- ⛏️ **Gold** — from ore stones (pickaxe)
- 🥩 **Food** — from sheep (knife)

Pawns automatically carry resources back to the castle and loop until the node is depleted. Food increases your troop cap; Gold unlocks upgrades.

### Build Structures
Click an empty **Building Slot** on the map to open the build menu. Each building costs Wood and spawns troops automatically over time:

| Building | Cost | Spawns | Cooldown |
|---|---|---|---|
| Barracks | 80 Wood | Warrior | 8s (4s upgraded) |
| Archery Tower | 60 Wood | Archer | 8s (4s upgraded) |
| Tower | 100 Wood | Lancer | 8s (4s upgraded) |
| Monastery | 70 Wood + 30 Food | Monk | 8s (4s upgraded) |
| House | 50 Wood | +1 Pawn slot | — |

Spend **100 Gold** to upgrade a building and halve its spawn cooldown.

### Troops
Your troops patrol automatically and attack any nearby enemy on sight:

- ⚔️ **Warrior** — balanced melee fighter
- 🏹 **Archer** — long-range, fires arrows
- 🗡️ **Lancer** — fast melee, holds the line at final position
- 🙏 **Monk** — heals nearby allied troops

### Defend the Castle
Enemy waves march from the southern map edge toward your castle every **30 seconds**. Waves grow larger and stronger over time. If the castle HP reaches zero — **Game Over**.

The castle shows fire effects when below 30% HP. Use the **Emergency Repair** button (costs Gold) or assign a Pawn to repair damaged buildings.

### Special Abilities
- **Troop Boost** — spend 150 Gold to give all active troops +50% stats for the current fight (2-minute cooldown)
- **Buy Pawn** — spend 50 Gold to recruit an extra Pawn

---

## Wave System

| Wave | Enemies | Composition |
|---|---|---|
| 1 | 5 | Warriors only |
| 2 | 8 | Warriors + Archers |
| 3 | 10 | Warriors, Archers, Lancers |
| 4 | 12 | Full mix |
| 5+ | 10 + wave×2 | Full mix, +25% stats per wave |

A countdown banner announces each incoming wave. Survive until the last wave — or as long as you can.

---

## Controls

| Action | Input |
|---|---|
| Send Pawn to resource | Click resource node |
| Cancel Pawn task | Click assigned node again |
| Place building | Click empty building slot → choose type |
| Repair building | Click damaged building (Pawn must be idle) |
| Move player warrior | WASD / Arrow keys |
| Attack (player warrior) | Left click |
| Pause | Esc |

---

## Built With

- **Unity 6000.3.9f1**
- **C#** — coroutine-based AI, physics movement, event-driven UI
- **Rigidbody2D** kinematic pawns with `MovePosition` for smooth physics
- **Y-Sort** depth system for correct 2D top-down layering
- **Wave Manager** with per-wave enemy composition and stat scaling
- **Projectile system** for archer arrows (tracks unit and building targets)

---

## Credits

**Created by [Laszlo Sierra](https://github.com/LaszloSM)**
Game design, programming, and all systems built from scratch in Unity 6.

**Pixel art assets** — [Tiny Sword](https://pixelfrog-assets.itch.io/) by **Pixel Frog**
→ https://pixelfrog-assets.itch.io/

Sprites, animations, tilemaps, UI elements, particle effects, and audio from the Tiny Sword asset pack. Please support their amazing work!

---

## License

© 2025 Laszlo Sierra. All rights reserved.
Visual and audio assets belong to their respective creators (see Credits above).
