using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

// Tạo scene cho các mini-game và thêm vào Build Settings (sau các màn phiêu lưu)
public static class MiniGameSetup
{
    public static void Run()
    {
        Create("Assets/Scenes/MiniSmash.unity", typeof(SmashGame));
        Create("Assets/Scenes/MiniSpray.unity", typeof(SprayGame));

        var scenes = EditorBuildSettings.scenes.ToList();
        foreach (string p in new[] { "Assets/Scenes/MiniSmash.unity", "Assets/Scenes/MiniSpray.unity" })
            if (!scenes.Any(s => s.path == p)) scenes.Add(new EditorBuildSettingsScene(p, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("MiniGameSetup: " + string.Join(", ", scenes.Select(s => s.path)));
    }

    static void Create(string path, System.Type game)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        camGo.transform.position = new Vector3(0, 0, -10);
        Camera cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.86f, 0.8f, 0.7f);
        camGo.AddComponent<AudioListener>();

        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();

        new GameObject("Game").AddComponent(game);
        EditorSceneManager.SaveScene(scene, path);
    }
}
