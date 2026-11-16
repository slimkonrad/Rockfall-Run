# Rockfall Run 2: The Three Keys

**CPSC 386, Project 2** · Konner Rigby

- **Source code:** [github.com/slimkonrad/Rockfall-Run/tree/project-2](https://github.com/slimkonrad/Rockfall-Run/tree/project-2) (branch **project-2**; the main branch is the Project 1 version)
- **Build:** Windows, submitted as a zip. Unzip and run Rockfall Run.exe.
- **Engine:** Unity 6.5 (6000.5.9f1), Universal 2D template

## How to play

Collect all three keys to unlock the gate, then reach the glowing flag. Pooled rocks fall from above, and one hit restarts the level. Coins are optional bonus points, scattered by procedural generation. The best completion time is saved and shown on the Main Menu.

| Action | Keys |
| --- | --- |
| Move left / right | A / D |
| Jump | Space or W (only while standing on ground) |
| Fast-fall in mid-air | S |
| Pause, resume, music volume | Esc (the volume slider is in the pause menu) |

## Design decisions

Project 2 extends the Project 1 game instead of starting over, so every new system has a job in the same level.

- **Puzzle logic in a pure C# class.** PuzzleTracker has no Unity dependencies and only stores state (a HashSet of key IDs and a score) and raises events. PuzzleController is the thin MonoBehaviour that connects it to the scene. Nothing polls every frame: pickups report in through triggers, and the gate opens from the PuzzleCompleted event.
- **Data in ScriptableObjects.** Keys and coins share one Collectible script and differ only by their CollectibleData asset (points, whether it counts toward the puzzle, burst color, sound). Adding a new pickup type needs a new asset, not new code.
- **Gate as its own tilemap.** The gate is painted like the level, but on a separate tilemap that can be switched off in one call when the puzzle is solved.
- **Procedural coins can't break the level.** Coins are optional bonus points and only spawn on standable surfaces inside the camera view. The three keys are hand-placed, so every seed is beatable.
- **Animating a child object.** The player sprite is a single photo, so the Animator moves a child Visual object (bob, waddle, squash and stretch) while physics stays on the parent. This keeps the Rigidbody2D and colliders unaffected by animation.
- **One shared particle system per effect.** Pickup sparkles reuse one particle system moved to each pickup with Emit(), instead of creating effects at runtime.
- **Original audio.** The music and sound effects were made for this project, so there are no licensing questions.

## Scripts

All scripts are in Assets/Scripts. The first group is new in Project 2; the rest are carried over from Project 1.

| Script | Attached to | What it does |
| --- | --- | --- |
| PlayerController | Player | Moves and jumps with the Move and Jump actions from the PlayerControls Input Actions asset, using Rigidbody2D velocity in FixedUpdate. A Physics2D.OverlapCircle at the feet (GroundCheck) gates the jump. Holding S fast-falls. Flips the sprite, fires jump dust and the jump sound, and sends Speed, IsGrounded and VerticalVelocity to the Animator. |
| PlayerFootsteps | Player | Receives the Footstep Animation Event from the Walk clip and plays a footstep sound and dust puff. Ignores the event when Walk is only lightly weighted in the blend tree. |
| PuzzleTracker | (pure C#, no GameObject) | Holds the puzzle and score logic: a HashSet of collected key IDs, the score, and ProgressChanged and PuzzleCompleted events. |
| PuzzleController | PuzzleController | Creates the PuzzleTracker, receives pickups, updates the Keys/Score UI, plays the pickup sparkle and sound, and opens the gate (with burst and sound) when the tracker reports completion. |
| Collectible | Key and Coin prefabs | On trigger with the Player, reports its CollectibleData and unique ID to PuzzleController and disappears. |
| CollectibleData | ScriptableObject assets (KeyData, CoinData) | Data for a pickup type: name, points, whether it counts toward the puzzle, burst color and pickup sound. Created with CreateAssetMenu. |
| CoinSpawner | CoinSpawner | Places coins above walkable ground tiles where Perlin noise is above a threshold. Seed, Randomize Seed, Noise Scale and Threshold are exposed in the Inspector. |
| AudioManager | AudioManager prefab (both scenes) | Persistent singleton (DontDestroyOnLoad, duplicates destroy themselves). Plays looping music and one-shot sound effects through the MainMixer Music and SFX groups, and saves and restores both volumes with PlayerPrefs. |
| VolumeSlider | Music slider in the pause menu | Shows the saved volume and updates the AudioManager (and the saved value) when moved. |
| LevelManager | LevelManager | Timer, restart on death, win panel, and best time saved with PlayerPrefs. (Project 1) |
| PlayerTriggerHandler | Player | Hazard trigger restarts the level, Finish trigger wins. (Project 1) |
| PauseMenu | LevelManager | Esc pauses with Time.timeScale = 0; Resume and Quit to Menu buttons. (Project 1) |
| MainMenu | MenuController | Start and Quit buttons and the best-time label. (Project 1) |
| SimplePool, RockSpawner, FallingRock | RockPool, RockSpawner, Rock prefab | Pooled falling-rock hazards. (Project 1) |
| CameraFit | Main Camera | Zooms the camera so the whole level fits any screen shape. (Project 1) |

## Scenes and key GameObjects

**MainMenu** (build index 0): title, best-time text, Start and Quit buttons (MenuController), and the AudioManager, which starts the music and survives into Level1.

**Level1** (build index 1):

- **Grid > Ground**: the level tilemap (floor, walls, platforms) with Tilemap Collider 2D, on the Ground layer and tag.
- **Grid > Gate**: a column of lock blocks on its own tilemap and collider, blocking the flag until all keys are collected.
- **Player**: Rigidbody2D, Box Collider 2D, PlayerController, PlayerTriggerHandler, PlayerFootsteps and an Animator (Player controller). Children: **Visual** (sprite, moved by the animation clips), **GroundCheck** (OverlapCircle point), **JumpDust** (particles) and **Lantern** (2D light).
- **Key, Key 2, Key 3**: hand-placed Key prefabs using KeyData.
- **CoinSpawner**: generates Coin prefabs (CoinData) at start.
- **PuzzleController**: owns the PuzzleTracker; linked to the Gate, the ProgressText UI, PickupBurst and GateBurst.
- **RockPool, RockSpawner, Finish, LevelManager, Canvas** (timer, keys/score, win and pause panels with the volume slider), and an **AudioManager** copy that removes itself when one already exists.

**How they interact:** touching a key or coin calls PuzzleController.Collect, which fires the sparkle and sound and updates the PuzzleTracker. The tracker raises ProgressChanged to refresh the UI and, on the third key, PuzzleCompleted, which switches off the Gate. The player can then reach the Finish trigger, and LevelManager shows the win panel and saves the best time. Rocks touching the player restart the level. The Animator picks Idle/Walk, Jump or Fall from the parameters PlayerController sets each frame.

## Requirements

| # | Requirement | How it is met |
| --- | --- | --- |
| 1 | Platformer movement | PlayerController reads Move and Jump from the PlayerControls Input Actions asset and moves a Rigidbody2D. Jumping is gated by Physics2D.OverlapCircle at GroundCheck on the Ground layer, so there is no mid-air jump. |
| 2 | Puzzle state tracking | PuzzleTracker stores collected key IDs in a HashSet, driven by Collectible triggers (no per-frame checks). Its PuzzleCompleted event opens the gate; reaching the flag then shows the win screen. |
| 3 | Audio and persistence | Music and five sound effects (jump, footstep, coin, key, gate) play through MainMixer's Music and SFX groups. AudioManager uses DontDestroyOnLoad with a duplicate guard, so music continues from MainMenu into Level1. Music and SFX volumes are saved with PlayerPrefs and restored on launch. |
| 4 | ScriptableObject and pure C# | CollectibleData (CreateAssetMenu) has two assets used in the game, KeyData and CoinData. PuzzleTracker is a pure C# class used by PuzzleController. |
| 5 | Animator and blend tree | The Player controller has Locomotion (1D blend tree: Idle at Speed 0, Walk at Speed 7), Jump and Fall states, driven by Speed, IsGrounded and VerticalVelocity set from PlayerController. |
| 6 | Animation event | The Walk clip has Footstep Animation Events at frames 20 and 40, which play a footstep sound and dust puff through PlayerFootsteps. |
| 7 | Particle effect from code | Jump dust (Emit on jump), pickup sparkle in the item's color (Emit at the pickup), and a gold gate burst (Play when the gate opens). |
| 8 | Procedural generation | CoinSpawner places coins with Perlin noise. Seed, Noise Scale and Threshold are exposed in the Inspector. Coins are optional and keys are hand-placed, so the level is always beatable. |

## Third-party assets, packages and assistance

- **[Kenney Pixel Platformer](https://kenney.nl/assets/pixel-platformer)** (CC0): level tiles, keys, gems used as coins, lock blocks and the finish flag.
- **Player sprite:** custom sprite made from a photo of a friend, used with his permission.
- **Rock sprite, music and sound effects:** original assets made for this project.
- **Unity packages:** Input System, Universal Render Pipeline (2D Renderer and 2D Lights), 2D Tilemap Editor, TextMeshPro and the built-in Audio Mixer, all standard with the Universal 2D template.
- **AI assistance:** Claude (Anthropic) was used as a step-by-step guide. It helped plan the design, draft and explain the scripts, walk through the Unity Editor setup, and create the rock sprite, music and sound effects.
