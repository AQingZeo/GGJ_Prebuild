Scene Descriptions 

Bootstrap
Persistent foundation loaded first and never unloaded; contains game state, input routing, save/load services, and global UI including the pause overlay.

MenuBase
Base scene for the main menu; contains menu visuals and menu-only logic and is unloaded when entering gameplay.

ExploreBase
Base gameplay scene; contains the game world, player, cameras, and gameplay systems that reset when leaving exploration.

DialogueOverlay (Additive)
On-demand overlay for dialogue; contains dialogue UI and dialogue logic and is loaded only when dialogue is explicitly triggered.

CutsceneOverlay (Additive)
On-demand overlay for cutscenes; contains timeline/cutscene logic and any cutscene-specific cameras and UI, loaded only when explicitly requested.