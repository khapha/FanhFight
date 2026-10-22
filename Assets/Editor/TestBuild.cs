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

    static void Build(string scene, string output)
    {
        BuildPipeline.BuildPlayer(new[] { scene }, output, BuildTarget.StandaloneWindows64, BuildOptions.None);
    }
}
