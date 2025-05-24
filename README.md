# TestTaskMiniGame

A Test Task “match-and-remove” demo built in Unity 6000.0.23 (URP). Click falling figures to send them into an Action Bar—stack three identical in one slot to clear them. Empty the field → Win; fill the bar → Lose.

## ⚙️ Key Features
- **Balanced Spawn**: even distribution of prefab-color-animal combos, with burst  
- **Action Bar**: stacks same combos in one slot, auto-removes triplets, fires Lose event if full  
- **Win/Lose Events**: wired via C# events (`OnWin`, `OnLose`), no runtime `FindObject…` calls  
- **Soft Glow**: one-shot ParticleSystem with additive material on arrival  
- **Freeze Effect**: URP 2D HLSL shader (`ColorToGrayRadial`) for radial color→gray wipe
