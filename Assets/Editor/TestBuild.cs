using UnityEditor;

// Báº£n build thá»­ chá»‰ cÃ³ má»™t mÃ n, Ä‘á»ƒ kiá»ƒm tra gameplay khÃ´ng cáº§n qua menu
public static class TestBuild
{
    public static void Scene1()
    {
        BuildPipeline.BuildPlayer(new[] { "Assets/Scenes/Scene 1.unity" },
            "TestBuild/FanhFight.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
    }
}
