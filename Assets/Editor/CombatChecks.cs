using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// Run before a build: real Unity components and generated sprites, without a live scene.
public static class CombatChecks
{
    const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;
    const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;

    static void Require(bool condition, string message)
    {
        if (!condition) throw new Exception("Combat regression: " + message);
    }

    public static void Run()
    {
        // Editor checks must preserve the paused/finished state of an open project.
        FieldInfo state = typeof(GameSession).GetField("<State>k__BackingField", PrivateStatic);
        FieldInfo resume = typeof(GameSession).GetField("resumeState", PrivateStatic);
        object savedState = state.GetValue(null), savedResume = resume.GetValue(null);
        float savedScale = Time.timeScale;
        var created = new List<GameObject>();
        try
        {
            state.SetValue(null, GameSession.Phase.Playing);
            resume.SetValue(null, GameSession.Phase.Playing);
            Time.timeScale = 1;
            CheckReactions(created);
            CheckSprites();
            Debug.Log("ALL COMBAT CHECKS PASSED: armor, slow, stun, knockback, pause and cached roach sprites.");
        }
        finally
        {
            foreach (GameObject go in created) if (go != null) UnityEngine.Object.DestroyImmediate(go);
            state.SetValue(null, savedState);
            resume.SetValue(null, savedResume);
            Time.timeScale = savedScale;
        }
    }

    static RoachWalker Walker(float x, List<GameObject> created)
    {
        GameObject go = new GameObject("CombatCheckRoach");
        created.Add(go);
        go.transform.position = new Vector3(x, 0, 0);
        go.AddComponent<Rigidbody2D>();
        RoachWalker walker = go.AddComponent<RoachWalker>();
        // AddComponent does not always invoke Awake outside Play mode.
        if (go.GetComponent<RoachStatus>() == null)
            typeof(RoachWalker).GetMethod("Awake", PrivateInstance).Invoke(walker, null);
        return walker;
    }

    static void CheckReactions(List<GameObject> created)
    {
        RoachWalker runner = Walker(0, created), armor = Walker(10, created);
        RoachStatus runnerStatus = runner.GetComponent<RoachStatus>();
        RoachStatus armorStatus = armor.GetComponent<RoachStatus>();
        Require(runner.Species == RoachStatus.Species.Runner && runner.Health == 24,
            "runner health/species initialization");
        Require(armor.Species == RoachStatus.Species.Armored && armor.Health == 60,
            "armored health/species initialization");
        runnerStatus.ApplyWeaponHit(12, true, new Vector2(-2, 0));
        armorStatus.ApplyWeaponHit(12, true, new Vector2(8, 0));
        Require(runner.Health == 12 && armor.Health == 55, "spray damage must be reduced only by armor");
        Require(runnerStatus.Slowed && armorStatus.Slowed
            && Mathf.Approximately(runnerStatus.SpeedMultiplier, 0.45f)
            && Mathf.Approximately(armorStatus.SpeedMultiplier, 0.45f), "spray must slow both species");

        armorStatus.Telegraphing = true;
        armorStatus.ApplyWeaponHit(32, false, new Vector2(8, 0));
        Require(armor.Health == 23 && armorStatus.Stunned && !armorStatus.Telegraphing,
            "a timed slipper hit must bypass armor and interrupt its warning");
        Require(armorStatus.KnockbackVelocity.x > 0, "left-side player must push the roach right");
        armorStatus.Tick(0.3f);
        Require(armorStatus.Stunned, "perfect slap must outlast the ordinary short stun");
        Require(armorStatus.KnockbackVelocity == Vector2.zero, "knockback must expire independently of stun");

        GameSession.Pause();
        int hp = armor.Health;
        armorStatus.Tick(5);
        armorStatus.ApplyWeaponHit(1, true, Vector2.zero);
        Require(armorStatus.Stunned && armorStatus.Slowed && armor.Health == hp,
            "pause must freeze status timers and reject incoming damage");
        GameSession.Resume();
        armorStatus.Tick(1.2f);
        Require(!armorStatus.Stunned && !armorStatus.Slowed && armorStatus.SpeedMultiplier == 1,
            "status effects must expire after resumed game time");

        armorStatus.ApplyWeaponHit(1, false, new Vector2(12, 0));
        Require(armor.Health == hp - 1 && armorStatus.KnockbackVelocity.x < 0,
            "right-side player must push left, with one damage dispatch");
        armorStatus.Tick(0.3f);
        Require(!armorStatus.Stunned, "ordinary slap must use the shorter stun");
        armorStatus.ApplyWeaponHit(0, true, Vector2.zero);
        Require(!armorStatus.Slowed, "zero-damage probes must not apply slow");
        Debug.Log("COMBAT PASS: real HP and weapon reactions, armor, timing, knockback direction and pause.");
    }

    static ulong PixelHash(Sprite sprite)
    {
        Require(sprite != null && sprite.texture != null && sprite.rect.width > 0 && sprite.rect.height > 0,
            "generated roach sprite missing");
        Require(sprite.texture.filterMode == FilterMode.Point, "roach sprites must retain point filtering");
        Color32[] pixels = sprite.texture.GetPixels32();
        bool visible = false;
        ulong hash = 14695981039346656037UL;
        foreach (Color32 p in pixels)
        {
            visible |= p.a != 0;
            unchecked { hash = (hash ^ p.r) * 1099511628211UL; hash = (hash ^ p.g) * 1099511628211UL;
                hash = (hash ^ p.b) * 1099511628211UL; hash = (hash ^ p.a) * 1099511628211UL; }
        }
        Require(visible, "generated roach sprite is fully transparent");
        return hash;
    }

    static void CheckSprites()
    {
        var distinct = new HashSet<ulong>();
        for (int variant = 0; variant < 3; variant++)
        {
            for (int frame = 0; frame < 2; frame++)
            {
                Sprite sprite = CuteArt.RoachVariant(frame, variant);
                Require(sprite == CuteArt.RoachVariant(frame, variant), "sprite cache created duplicate objects");
                Require(Mathf.Approximately(sprite.bounds.size.x, 1.5f)
                    && Mathf.Approximately(sprite.bounds.size.y, 1.125f), "roach world footprint changed");
                Require(distinct.Add(PixelHash(sprite)), "roach species or walking frames are visually identical");
                if (variant == 2)
                {
                    Sprite flying = CuteArt.RoachVariant(frame, variant, true);
                    Require(flying == CuteArt.RoachVariant(frame, variant, true), "flying sprite cache failed");
                    Require(distinct.Add(PixelHash(flying)), "open wings must differ from folded wings");
                }
            }
        }
        Sprite[] topDown = MiniArt.Roach;
        Require(topDown != null && topDown.Length == 2 && PixelHash(topDown[0]) != PixelHash(topDown[1]),
            "top-down minigame roaches need two visible animation frames");
        Debug.Log("COMBAT PASS: distinct cached runner, armored, flyer, open-wing and minigame roach frames.");
    }
}
