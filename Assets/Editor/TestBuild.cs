using UnityEditor;

// Bản build thử chỉ có một scene, để kiểm tra nhanh không cần qua menu
public static class TestBuild
{
    public static void Scene1() { Build("Assets/Scenes/Scene 1.unity", "TestBuild/FanhFight.exe"); }
    public static void Smash() { Build("Assets/Scenes/MiniSmash.unity", "TestBuild/Smash/FanhFight.exe"); }
    public static void Spray() { Build("Assets/Scenes/MiniSpray.unity", "TestBuild/Spray/FanhFight.exe"); }

    static void Build(string scene, string output)
    {
        BuildPipeline.BuildPlayer(new[] { scene }, output, BuildTarget.StandaloneWindows64, BuildOptions.None);
    }
}
