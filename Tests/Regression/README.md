# Gameplay upgrade regression checks

Requires .NET SDK 8. No NuGet packages or Unity installation are required.

Run from this directory so `global.json` selects the .NET 8 SDK:

```powershell
cd D:\code\FanhFight\Tests\Regression
dotnet run --project Regression.csproj
```

The runner links the actual `GameSession`, `AdventureProgress`, `GardenSave` and
`TouchHoldButton` and `PlayerAttack` sources. Small Unity test doubles cover their API dependencies.
It checks state transitions, level locks and records, save compatibility,
multitouch release behavior, attack cancellation, scene order, C# syntax and script metadata.
It does **not** verify rendering, physics, real Input System event routing,
PlayerPrefs disk persistence or a full Unity compilation.

## Unity 6000.6.7f1 playtest

1. Open `Assets/Scenes/Scene 3.unity` and enter Play mode. Choose **Adventure**.
   On a fresh save, only **Woodland** is available. Back/Esc restores menu focus.
2. Finish the first level via its door. **Lakeside** unlocks, and campaign points
   carry across the transition. Return to the menu and select an unlocked level:
   run points start at zero, while completed levels and best scores remain.
3. Complete the final level. Verify the win panel and clickable **Retry/Menu**.
   Replaying a level with fewer points must preserve its previous personal best.
4. Hold attack while taking lethal damage. The attack collider must turn off
   immediately and stay off during respawn/game over. Pause during respawn,
   resume, then verify that respawn completes before player input is accepted.
5. Pause and choose Retry. The new level must run at normal speed. Repeat
   immediately after hitting an enemy to cover the hit-stop effect.
6. In a mobile build, verify move + jump + attack with multiple fingers, release
   by dragging off a button, and pause while holding buttons. Verify **Enter**,
   **Down**, **Pause/Resume**, and touch buttons on the game-over screen.
7. Check landscape and portrait layouts with a notch/safe area. HUD, level
   selector and new touch buttons should remain inside the safe area.
8. In **Flower Garden**, create a flower and return to the menu while it is still
   growing. Reopen the garden and restart the app: the flower should retain its
   full size and appearance. **Reset Garden** should persist after restarting.

Existing zero-size flowers are recovered when loading. Malformed save entries
are skipped; the existing `garden` save format and unrelated player data remain.
