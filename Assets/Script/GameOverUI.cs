using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

// Màn hình Game Over / Thắng, tự tạo bằng code nên không cần sửa scene
public class GameOverUI : MonoBehaviour
{
    public static bool IsShowing = false;

    public static void Show(string title, int points, int highscore)
    {
        if (IsShowing) return;
        IsShowing = true;
        Time.timeScale = 0;

        GameObject root = new GameObject("GameOverUI");
        root.AddComponent<GameOverUI>();
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);

        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(root.transform, false);
        Image img = bg.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.75f);
        Stretch(img.rectTransform);

        AddText(root.transform, title, 80, new Vector2(0, 120));
        AddText(root.transform, "Points: " + points + "    HighScore: " + highscore, 40, new Vector2(0, 10));
        AddText(root.transform, "R / A: Choi lai     M / B: Menu", 32, new Vector2(0, -100));
    }

    static void AddText(Transform parent, string s, int size, Vector2 pos)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.text = s;
        t.fontSize = size;
        t.alignment = TextAlignmentOptions.Center;
        t.color = Color.white;
        t.rectTransform.sizeDelta = new Vector2(1200, size * 1.5f);
        t.rectTransform.anchoredPosition = pos;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    void Update()
    {
        if (GameInput.Restart.WasPressedThisFrame())
            Load(SceneManager.GetActiveScene().buildIndex);
        if (GameInput.Menu.WasPressedThisFrame())
            Load(0);
    }

    void Load(int index)
    {
        IsShowing = false;
        Time.timeScale = 1;
        SceneManager.LoadScene(index);
    }
}
