using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

// Màn hình Game Over / Thắng, tự tạo bằng code nên không cần sửa scene
public class GameOverUI : MonoBehaviour
{
    public static bool IsShowing => GameSession.State == GameSession.Phase.Finished;
    float readyAt;
    bool loading;

    public static void Show(string title, int points, int highscore)
    {
        if (IsShowing) return;
        GameSession.Finish();

        GameObject root = new GameObject("GameOverUI");
        GameOverUI ui = root.AddComponent<GameOverUI>();
        ui.readyAt = Time.unscaledTime + 0.25f;
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        root.AddComponent<GraphicRaycaster>();

        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(root.transform, false);
        Image img = bg.AddComponent<Image>();
        img.color = new Color(0.12f, 0.28f, 0.23f, 0.45f);
        Stretch(img.rectTransform);

        Transform content = SafeArea.Create(root.transform);
        content = CuteTheme.Card(content, Vector2.one * 0.5f, Vector2.zero, new Vector2(620, 430)).transform;
        AddText(content, title == "YOU WIN!" ? "You did it!" : "Game Over", 54, new Vector2(0, 130));
        AddText(content, "Score: " + points + "\nBest: " + highscore, 28, new Vector2(0, 35));
        Button retry = MiniUI.MakeButton(content, "Retry", new Vector2(0.5f, 0.5f),
            new Vector2(-120, -100), new Vector2(210, 64), () => ui.Load(SceneManager.GetActiveScene().buildIndex));
        MiniUI.MakeButton(content, "Menu", new Vector2(0.5f, 0.5f),
            new Vector2(120, -100), new Vector2(210, 64), () => ui.Load(0));
        AddText(content, "R / A: Retry    M / B: Menu", 18, new Vector2(0, -170));
        MiniUI.Select(retry);
    }

    static void AddText(Transform parent, string s, int size, Vector2 pos)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.text = s;
        CuteTheme.Text(t, size);
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        t.rectTransform.sizeDelta = new Vector2(600, size * 2.5f);
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
        if (Time.unscaledTime < readyAt || loading) return;
        if (GameInput.Restart.WasPressedThisFrame())
            Load(SceneManager.GetActiveScene().buildIndex);
        else if (GameInput.Menu.WasPressedThisFrame())
            Load(0);
    }

    void Load(int index)
    {
        if (loading || Time.unscaledTime < readyAt) return;
        loading = true;
        GameSession.LoadScene(index);
    }
}
