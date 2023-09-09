# No Man's Gun

No Man's Gun is a physics-based, hardcore, time-trial 2D pixel art game, optimised for Android, written in C#, and powered by Unity. It was created as a project to learn C#, understand what a engine like Unity Engine does, and learn about physics-based programming.

_This repo only contains the C# scripts and no other files or assets related to the game. Plastic SCM was used for full version control for this project as it natively integrates with Unity._

![NMG-banner 2](https://user-images.githubusercontent.com/120580433/216582742-1506c5e5-1a69-40f8-a801-f0ee3eadc845.png)

### Technical highlights
+ Designed for Android with touch controls but also ported to Windows desktop
+ Integration with the Lootloocker API to enable online leaderboards
+ State machine to handle game states
+ Scriptable Objects to enable scalable level creation and management
+ Ghost replay/beat your own ghost feature (using Scriptable Objects)
+ Object pooling to pre-spawn and manage frequently used objects such as particles
+ Player score and preference save file management
+ High-performance game achitecture to allow run on mid-end mobiles
+ Blit render texture effects such as shockwaves
+ Parallex background scrolling
+ Touch optimised UI
+ Making use of AnimatorOverrideControllers and player states to enable fully swappable player skins and stats at runtime
+ Unity 2D Tilemaps for level creation of 18 unique and challenging levels
+ AI-generated background music

![NMG Banner 01](https://github.com/kimgoetzke/game-no-mans-gun/assets/120580433/fbb6e154-45e2-477c-85bd-a30793130dcf)
