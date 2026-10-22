using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

// Sửa giao diện trong mọi scene:
// - Canvas vẽ kiểu Overlay để không bị nhân vật/gạch đè lên
// - EventSystem dùng module của Input System để bấm được nút
public static class UIFix
{
    public static void Run()
    {
        foreach (EditorBuildSettingsScene s in EditorBuildSettings.scenes)
        {
            var scene = EditorSceneManager.OpenScene(s.path);
            int canvases = 0;
            foreach (Canvas c in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!c.isRootCanvas || c.renderMode == RenderMode.ScreenSpaceOverlay) continue;
                c.renderMode = RenderMode.ScreenSpaceOverlay;
                c.sortingOrder = 10;
                canvases++;
            }
            foreach (EventSystem es in Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                StandaloneInputModule old = es.GetComponent<StandaloneInputModule>();
                if (old != null) Object.DestroyImmediate(old);
                if (es.GetComponent<InputSystemUIInputModule>() == null)
                    es.gameObject.AddComponent<InputSystemUIInputModule>();
            }
            EditorSceneManager.SaveScene(scene);
            Debug.Log("UIFix: " + s.path + " overlay canvases " + canvases);
        }
    }
}
