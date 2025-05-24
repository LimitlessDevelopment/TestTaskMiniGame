# TestTaskMiniGame

A Test Task demo built in Unity 6000.0.23 (URP).

## 📝 Scripts Overview

- **BoardManager.cs**  
  Generates all prefab–color–animal combinations, spawns figures in balanced bursts, tracks active figures, and fires `OnDoneInstantiating` when done.

- **ActionBar.cs**  
  Collects clicked figures, stacks identical combos into the same slot, removes any stack of 3, and raises `OnPlayerWin` / `OnPlayerLose` events.

- **Figure.cs**  
  Handles click/tap input, disables its `Rigidbody2D`, and animates an ease-in-out into the Action Bar.

- **FreezeFigurine.cs**  
  Subscribes to the board’s removal count, picks a random subset of figures to “freeze” (apply radial gray shader), and later “unfreezes” them when enough have been removed.

- **ParticlesEnable.cs**  
  Listens for slot-finish events from `ActionBar`, moves a one-shot `ParticleSystem` prefab to that slot position, and plays a soft-glow burst.

---



## ✨ Features

- Balanced procedural spawn of figures  
- Smooth flight with spin & soft-glow on arrival  
- Radial URP shader for color→gray “freeze” effect  
- Extensible combo-stacking via C# events  
