using UnityEditor;

// Bản build thử chỉ có Scene 1, để kiểm tra gameplay không cần qua menu
public static class TestBuild
{
    public static void Scene1()
    {
        BuildPipeline.BuildPlayer(new[] { "Assets/Scenes/Scene 1.unity" },
            "TestBuild/FanhFight.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
    }
}
