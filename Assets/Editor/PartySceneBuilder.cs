#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Công cụ dựng scene "ButterflyParty" — tiệc hóa hoa: toàn bướm bay + hoa + nhạc menu.
// Scene sinh bằng code (không asset ngoài), thêm vào Build Settings sau màn cuối.
public static class PartySceneBuilder
{
    [MenuItem("Tools/Build Butterfly Party Scene")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // camera + nền hồng pastel
        var camGo = new GameObject("Main Camera");
        var cam = camGo.AddComponent<Camera>();
        cam.backgroundColor = new Color(1f, 0.85f, 0.92f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        camGo.tag = "MainCamera";
        camGo.AddComponent<AudioListener>();

        // ánh sáng mặc định
        var lightGo = new GameObject("Directional Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;

        // bàn tiệc bướm chạy bằng code lúc Play
        var party = new GameObject("ButterflyParty");
        party.AddComponent<ButterflyPartyScene>();

        const string path = "Assets/Scenes/ButterflyParty.unity";
        EditorSceneManager.SaveScene(scene, path);

        // đăng ký vào Build Settings (nếu chưa có)
        var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!list.Exists(s => s.path == path))
        {
            list.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = list.ToArray();
        }
        Debug.Log("PARTY SCENE READY: " + path + " (build scenes: " + EditorBuildSettings.scenes.Length + ")");
    }
}
#endif
