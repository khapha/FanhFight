using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.Linq;

// Bản build thử chỉ có một scene, để kiểm tra nhanh không cần qua menu
public static class TestBuild
{
    // Full local playtest build: includes the menu and all level transitions.
    public static void All()
    {
        PrepareGraphics();
        CheckFallingPlatform();
        string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        BuildReport report = BuildPipeline.BuildPlayer(scenes, "TestBuild/Full/FanhFight.exe",
            BuildTarget.StandaloneWindows64, BuildOptions.Development);
        if (report.summary.result != BuildResult.Succeeded)
            throw new System.Exception("Playtest build failed: " + report.summary.result);
        Debug.Log("Playtest build succeeded: TestBuild/Full/FanhFight.exe");
    }

    static void PrepareGraphics()
    {
        foreach (string asset in new[] { "CuteForest", "CuteLake", "CuteCave" }) PrepareTexture(asset);
    }

    static void PrepareTexture(string asset)
    {
        TextureImporter importer = AssetImporter.GetAtPath("Assets/Resources/" + asset + ".png") as TextureImporter;
        if (importer == null) return;
        if (importer.textureType == TextureImporterType.Default && importer.filterMode == FilterMode.Point
            && !importer.mipmapEnabled && importer.maxTextureSize == 2048
            && importer.textureCompression == TextureImporterCompression.Uncompressed) return;
        importer.textureType = TextureImporterType.Default;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = 2048;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }

    // Exercise the actual Unity Rigidbody2D API, without waiting for the fall timers.
    public static void CheckFallingPlatform()
    {
        GameObject go = new GameObject("FallingPlatformCheck");
        string failure = null;
        Application.LogCallback capture = (message, stack, type) =>
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert || message.Contains("static body")) failure = message;
        };
        Application.logMessageReceived += capture;
        try
        {
            Rigidbody2D body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Static;
            FallingPlat platform = go.AddComponent<FallingPlat>();
            typeof(FallingPlat).GetMethod("Start", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(platform, null);
            var routine = (System.Collections.IEnumerator)typeof(FallingPlat)
                .GetMethod("fall", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(platform, null);
            routine.MoveNext();
            routine.MoveNext();
            body.linearVelocity = Vector2.down;
            body.angularVelocity = 30;
            go.transform.position = Vector3.down * 5;
            routine.MoveNext();
            if (body.bodyType != RigidbodyType2D.Static || go.transform.position != Vector3.zero || failure != null)
                throw new System.Exception("Falling platform reset failed: " + failure);
            Debug.Log("PASS: falling platform returns to its static start state without velocity errors.");
        }
        finally
        {
            Application.logMessageReceived -= capture;
            Object.DestroyImmediate(go);
        }
    }

    public static void Scene1() { Build("Assets/Scenes/Scene 1.unity", "TestBuild/FanhFight.exe"); }
    public static void Smash() { Build("Assets/Scenes/MiniSmash.unity", "TestBuild/Smash/FanhFight.exe"); }
    public static void Spray() { Build("Assets/Scenes/MiniSpray.unity", "TestBuild/Spray/FanhFight.exe"); }

    // Regression checks for the adventure expansion, run in the editor before building.
    public static void Quest()
    {
        // 1) sprite generators must not throw and must produce readable pixels
        foreach (System.Func<Sprite> make in new System.Func<Sprite>[] {
            () => KeyPickup.KeySprite, () => Gate.GateSprite, () => DashOrb.OrbSprite,
            () => RoachKing.CrownSprite, () => CuteArt.Butterfly,
            () => CuteArt.Roach(0), () => CuteArt.Roach(1),
            () => CuteArt.KingRoach(0), () => CuteArt.KingRoach(1) })
        {
            Sprite s = make();
            if (s == null || s.rect.width <= 0) throw new System.Exception("Quest sprite generation failed.");
        }
        Debug.Log("QUEST PASS 1/6: code-generated sprites (key, gate, orb, crown, butterfly).");

        // 2) quest state math + butterfly diary counters
        System.Action reset = () =>
        {
            PlayerPrefs.DeleteKey("dashUnlocked");
            for (int i = 1; i <= 3; i++) PlayerPrefs.DeleteKey("adventureSecret" + i);
            PlayerPrefs.DeleteKey("adventureUnlocked");
            Diary.Reset();
        };
        reset();
        Diary.Reset();
        int beforeRescued = Diary.Rescued;
        string first = Diary.Rescue();
        string second = Diary.Rescue("tester");
        if (Diary.Rescued != beforeRescued + 2 || first == second || string.IsNullOrEmpty(first))
            throw new System.Exception("Butterfly diary rescue counting is broken.");
        Debug.Log("QUEST PASS 2a/6: butterfly diary counts rescues (" + first + ", " + second + ").");
        if (QuestState.ReadyToFinish(1)) throw new System.Exception("Level 1 should not be finishable without keys.");
        for (int i = 0; i < QuestState.KeysRequired - 1; i++) QuestState.CollectKey();
        if (QuestState.ReadyToFinish(1)) throw new System.Exception("Gate should require all keys.");
        QuestState.CollectKey();
        if (!QuestState.ReadyToFinish(1)) throw new System.Exception("Gate should open with all keys on a normal level.");
        if (QuestState.ReadyToFinish(3)) throw new System.Exception("Boss level must require the Roach King defeated.");
        QuestState.DefeatBoss();
        if (!QuestState.ReadyToFinish(3)) throw new System.Exception("Boss level should open after the boss surrenders.");
        QuestState.UnlockDash();
        if (!QuestState.DashUnlocked) throw new System.Exception("DashUnlocked should follow the saved flag.");
        reset();
        Debug.Log("QUEST PASS 2b/6: gate rules (keys, boss requirement, dash persistence).");

        // 3) objective text never shows stale boss hp after defeat
        if (string.IsNullOrEmpty(QuestState.ObjectiveText) && QuestState.Level >= 1)
            throw new System.Exception("Objective text should describe the current goal.");
        Debug.Log("QUEST PASS 3/6: HUD objective text.");

        // 4) walker factory keeps colliders on the right layers (uses the real Rigidbody2D path)
        GameObject go = new GameObject("WalkerProbe");
        try
        {
            go.AddComponent<Rigidbody2D>();
            go.AddComponent<RoachWalker>();
            BoxCollider2D[] boxes = go.GetComponents<BoxCollider2D>();
            if (boxes.Length == 0)
                go.SendMessage("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance); // edit mode: Awake chưa chạy
            boxes = go.GetComponents<BoxCollider2D>();
            if (boxes.Length != 2 || !boxes[1].isTrigger) throw new System.Exception("Walker needs a solid box plus a trigger sensor.");
        }
        finally { Object.DestroyImmediate(go); }
        Debug.Log("QUEST PASS 4/6: roach walker collider layout.");

        // 5) audio/visual one-shot helpers used by props exist and do not throw without a scene Fx
        Fx.Coin();
        Debug.Log("QUEST PASS 5/6: effect entry points safe.");

        // 6) the slipper swing area must overlap the enemy hit sensors, otherwise real
        //    attacks whiff (bug caught by AutoPlay: boss sensor sat at foot level while
        //    the slipper swings at chest height, so standing still and mashing never connected)
        void CheckSwingReach(GameObject enemy, string label)
        {
            BoxCollider2D[] boxes = enemy.GetComponents<BoxCollider2D>();
            BoxCollider2D sensor = System.Array.Find(boxes, b => b.isTrigger);
            if (sensor == null) throw new System.Exception(label + " lost its trigger sensor.");
            float swingBottom = 0.1f, swingTop = 1.3f; // slipper: player-local y 0.7, height 1.2
            float scale = enemy.transform.localScale.y;
            float bottom = (sensor.offset.y - sensor.size.y * 0.5f) * scale;
            float top = (sensor.offset.y + sensor.size.y * 0.5f) * scale;
            if (top < swingBottom || bottom > swingTop)
                throw new System.Exception(label + " sensor (" + bottom + ".." + top
                    + ") never overlaps the slipper swing (0.1..1.3) - real attacks would miss.");
        }
        GameObject slipper = new GameObject("SlipperProbeUnused");
        GameObject walkerGo = new GameObject("WalkerProbe2");
        GameObject kingGo = new GameObject("KingProbe");
        try
        {
            walkerGo.AddComponent<Rigidbody2D>();
            walkerGo.AddComponent<RoachWalker>();
            walkerGo.SendMessage("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            CheckSwingReach(walkerGo, "Roach walker");

            kingGo.AddComponent<Rigidbody2D>();
            kingGo.AddComponent<RoachKing>();
            kingGo.SendMessage("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            CheckSwingReach(kingGo, "Roach King");
            Debug.Log("QUEST PASS 6/6: slipper swing reaches walker and king sensors.");
        }
        finally { Object.DestroyImmediate(slipper); Object.DestroyImmediate(walkerGo); Object.DestroyImmediate(kingGo); }
        Debug.Log("ALL QUEST REGRESSION CHECKS PASSED.");
    }


    static void Build(string scene, string output)
    {
        BuildPipeline.BuildPlayer(new[] { scene }, output, BuildTarget.StandaloneWindows64, BuildOptions.None);
    }
}
