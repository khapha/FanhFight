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

Weapon-role checks also run in the .NET suite: sustained spray overheats and
stops its collider, held input cannot bypass the cooldown, a cooled bottle can
fire again, pause freezes heat, death/respawn resets it, and the slipper has no
heat limit. Multi-collider targets receive one weapon reaction per swing, with
the player's position retained as the knockback source. These use API doubles;
actual enemy slow/stun/armor and collision movement require the Unity checks.

1. Open `Assets/Scenes/Scene 3.unity` and enter Play mode. Choose **Adventure**.
   On a fresh save, only **Woodland** is available. Choose a level, then choose
   **Spray Bottle** or **Slipper**. Back returns to level selection.
2. Finish the first level via its door. **Lakeside** unlocks, and campaign points
   carry across the transition. Return to the menu and select an unlocked level:
   run points start at zero, while completed levels and best scores remain.
3. Complete all four levels, including **Sunset Meadow**. Verify the win panel and clickable **Retry/Menu**.
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
9. Hold the spray until its heat meter locks, including with a held touch button.
   Damage and mist should stop; holding must resume only after cooling. Pause
   during cooling and verify the meter stays still. Die/retry and verify the
   new life starts with a ready bottle. The slipper must swing indefinitely.
10. Compare runner, armored and flying roaches. Spray should slow all three;
    armor should resist spray more than slipper hits. Hit a telegraphing roach
    with the slipper: the warning should disappear, its attack should be
    interrupted, and it should be stunned and pushed away from the player.
    Verify recovery and stun cannot deal contact damage, and flying enemies
    stop moving while paused. Check both player-facing directions.

Existing zero-size flowers are recovered when loading. Malformed save entries
are skipped; the existing `garden` save format and unrelated player data remain.

## Full runtime checks

Build the development player with Unity's `-executeMethod TestBuild.VerifyAndBuild`.
This runs the editor quest/sprite checks and preserves the PlayerPrefs keys those
checks touch. The output is `TestBuild/Full/FanhFight.exe`.

Run the player with `-cute-autoplay <absolute-screenshot-folder>` for the four-level
smoke test. It follows the level/tool menu, exercises tool hitboxes in both
directions, checks blocked exits, collects keys, defeats both bosses and boots
the mini-games and Butterfly Party. It backs up/restores progress, diary and tool
selection, and returns exit code 1 on a nested test failure.

Use `-cute-slipper` for the slipper loadout and `-cute-autoplay-level 4` for a
focused Sunset Meadow run. A visible window is required for screenshots;
`-batchmode -nographics` runs only the logic/physics checks.

The bot uses god mode and fallback teleports. A passing run verifies interaction
and progression, not that every jump is reachable by an unaided player, nor
physical multitouch/mobile performance.
