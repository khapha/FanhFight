#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Đảm bảo màn 4 (Sunset Meadow, Scene 3.unity) nằm trong Build Settings.
// Chạy 1 lần sau khi thêm màn mới vào AdventureProgress.Scenes.
public static class EnsureSceneInBuild
{
    [MenuItem("Tools/Ensure All Adventure Scenes In Build")]
    public static void Run()
    {
        var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (string scene in AdventureProgress.Scenes)
        {
            string path = "Assets/Scenes/" + scene + ".unity";
            if (!list.Exists(s => s.path == path))
            {
                list.Add(new EditorBuildSettingsScene(path, true));
                Debug.Log("Added to build: " + path);
            }
        }
        // KHÔNG đổi thứ tự: scene menu đã đứng đúng vị trí trong Build Settings,
        // chỉ bổ sung scene còn thiếu vào cuối
        EditorBuildSettings.scenes = list.ToArray();
        Debug.Log("Build settings now has " + EditorBuildSettings.scenes.Length + " scenes.");
    }

    static int Order(string path)
    {
        if (path.EndsWith("MainMenu.unity")) return 0;
        for (int i = 0; i < AdventureProgress.Scenes.Length; i++)
            if (path == "Assets/Scenes/" + AdventureProgress.Scenes[i] + ".unity") return 1 + i;
        if (path.Contains("MiniSmash")) return 20;
        if (path.Contains("MiniSpray")) return 21;
        if (path.Contains("ButterflyParty")) return 22;
        return 30;
    }
}
#endif
