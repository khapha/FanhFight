#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Dựng màn 4 "Sunset Meadow" — mở Scene 1 làm template (world hand-made:
// Ground/Spikes/door/GameMaster/HUD...) rồi lưu thành file scene mới.
// Toàn bộ props gameplay (keys/walkers/dancer/gate/npc) do AdventureProps spawn runtime.
public static class MeadowSceneBuilder
{
    [MenuItem("Tools/Build Meadow Scene")]
    public static void Build()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Scene 1.unity", OpenSceneMode.Single);
        Debug.Log("MEADOW: template opened, " + scene.rootCount + " root objects");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Sunset Meadow.unity");
        Debug.Log("MEADOW: saved Assets/Scenes/Sunset Meadow.unity");
    }
}
#endif
