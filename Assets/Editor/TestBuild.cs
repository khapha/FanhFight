using UnityEditor;

// Bản build thử chỉ có một màn, để kiểm tra gameplay không cần qua menu
public static class TestBuild
{
    public static void Scene1()
    {
        BuildPipeline.BuildPlayer(new[] { "Assets/Scenes/Scene 4.unity" },
            "TestBuild/FanhFight.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
    }
}
