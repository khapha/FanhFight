using UnityEngine;
using UnityEngine.SceneManagement;

// Nút cảm ứng (trái/phải/nhảy/chém) chỉ hiện trên điện thoại
public static class TouchButtons
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.GetComponent<Canvas>() == null) continue;
                Transform buttons = root.transform.Find("Button");
                if (buttons != null && buttons.Find("Left") != null)
                    buttons.gameObject.SetActive(Application.isMobilePlatform);
            }
        };
    }
}
