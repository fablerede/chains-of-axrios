# Chains of Axrios

A solo-developed, three-faction multiplayer RPG built in Unity 6.2, inspired by the RvR structure of Dark Age of Camelot. Every system — networking, combat, AI, inventory, UI, quests, and more — designed and implemented by one developer.

## About the Game

Chains of Axrios is set across a world of 13 planes connected by Fashian Crystals. Three factions — the Royals of Suthcliff, the Rangers of Greymesa, and the Raiders of Al'Nimbra — compete for control in large-scale realm vs. realm combat. The game features deep lore, original cosmology, and a class system built around meaningful faction identity.

## Tech Stack

- **Engine:** Unity 6.2 (URP)
- **Language:** C#
- **Networking:** FishNet (client-server architecture)
- **3D Assets:** Meshy AI remeshing + free medieval modular asset packs

## Systems Implemented

- **Networking** — Full client-server multiplayer via FishNet; networked entities, state sync, and authority management
- **Ability System** — ScriptableObject-based architecture with abstract `CanUse()` / `Execute()` pattern; supports class-specific mechanics (poison, healing, buffs)
- **Combat** — Attack Power vs. EAF resolution, seven damage types, hit quality tiers, status effects (Poison with PoisonCounter/PoisonManager)
- **Inventory & Equipment** — Item stacking, WorldItem loot drops, LootTables, ItemRegistry, bag and equipment slot management
- **AI** — Enemy aggro, leash reset, pathfinding, and entity state machines
- **Quest & Dialogue** — NPCEntity with full dialogue trees and quest assignment via ScriptableObject data
- **Save / Load** — Full serialized save system covering player state, inventory, quests, and world state
- **UI Systems** — HotbarUI, SkillScreenUI, BagWindowUI, QuestJournalUI, ChatWindowUI, GameMenuUI, tooltips
- **Classes** — Paladin and Apothecary fully implemented; 12-class roster designed across three factions

## Project Status

Active development. Current focus: Suthcliff city zone layout with URP volumetric fog.

## About the Developer

Solo project by Maxamillian Marsh. BS in Game Design. All design, code, and systems architecture by one person.
