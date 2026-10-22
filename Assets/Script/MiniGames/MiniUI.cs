using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// Tiện ích chung cho mini-game: giao diện dựng bằng code, đọc chạm/chuột
public static class MiniUI
{
    static readonly System.Collections.Generic.List<Vector2> taps = new System.Collections.Generic.List<Vector2>(10);
    public static Canvas MakeCanvas(string name)
    {
        GameObject go = new GameObject(name);
        Canvas c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 10;
        CanvasScaler s = go.AddComponent<CanvasScaler>();
        s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        s.referenceResolution = new Vector2(1280, 720);
        s.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return c;
    }

    public static TextMeshProUGUI MakeText(Transform parent, string s, int size, Vector2 anchor, Vector2 pos)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.text = s;
        t.fontSize = size;
        t.alignment = TextAlignmentOptions.Center;
        t.color = new Color(0.3f, 0.18f, 0.1f);
        t.outlineWidth = 0.2f;
        t.outlineColor = Color.white;
        t.raycastTarget = false;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        RectTransform rt = t.rectTransform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(900, size * 1.6f);
        return t;
    }

    public static Button MakeButton(Transform parent, string label, Vector2 anchor, Vector2 pos, Vector2 size, System.Action onClick)
    {
        GameObject go = new GameObject(label);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.92f);
        Button b = go.AddComponent<Button>();
        ColorBlock cb = b.colors;
        cb.highlightedColor = new Color(1f, 0.9f, 0.6f);
        cb.selectedColor = new Color(1f, 0.9f, 0.6f);
        cb.pressedColor = new Color(0.9f, 0.75f, 0.4f);
        b.colors = cb;
        b.onClick.AddListener(() => onClick());
        RectTransform rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        TextMeshProUGUI t = MakeText(go.transform, label, (int)(size.y * 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        t.color = new Color(0.2f, 0.15f, 0.1f);
        t.outlineWidth = 0;
        t.rectTransform.sizeDelta = size;
        CuteTheme.Button(b);
        return b;
    }

    public static GameObject Panel(Transform parent, Color color)
    {
        GameObject go = new GameObject("Panel");
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = color;
        RectTransform rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return go;
    }

    public static void Select(Component c)
    {
        if (c != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(c.gameObject);
    }

    public static void BackToMenu()
    {
        GameSession.LoadScene(0);
    }

    // khung màn hình theo toạ độ thế giới (trừ lề)
    public static Rect WorldArea(Camera cam, float margin)
    {
        float h = cam.orthographicSize, w = h * cam.aspect;
        Vector2 c = cam.transform.position;
        return new Rect(c.x - w + margin, c.y - h + margin, (w - margin) * 2, (h - margin) * 2 - 1.2f);
    }

    // sàn gạch phủ kín khung nhìn
    public static void MakeFloor(Camera cam, Sprite sprite = null)
    {
        GameObject go = new GameObject("Floor");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite != null ? sprite : MiniArt.Floor;
        sr.drawMode = SpriteDrawMode.Tiled;
        float h = cam.orthographicSize * 2 + 2, w = h * 2.4f;
        sr.size = new Vector2(w, h);
        sr.sortingOrder = -10;
        go.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 0);
    }

    // ---- chạm / chuột ----
    static bool OverUI(int pointerId)
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(pointerId);
    }

    // những điểm vừa chạm/bấm trong khung hình này (nhiều ngón cùng lúc)
    public static System.Collections.Generic.List<Vector2> Taps(Camera cam)
    {
        var list = taps;
        list.Clear();
        if (Touchscreen.current != null)
        {
            foreach (var t in Touchscreen.current.touches)
                if (t.press.wasPressedThisFrame && !OverUI(t.touchId.ReadValue()))
                    list.Add(cam.ScreenToWorldPoint(t.position.ReadValue()));
        }
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && !OverUI(-1))
            list.Add(cam.ScreenToWorldPoint(Mouse.current.position.ReadValue()));
        return list;
    }

    // điểm đang giữ (chuột trái hoặc ngón đầu tiên), null nếu không giữ
    public static Vector2? Held(Camera cam)
    {
        if (Touchscreen.current != null)
            foreach (var t in Touchscreen.current.touches)
                if (t.press.isPressed && !OverUI(t.touchId.ReadValue()))
                    return cam.ScreenToWorldPoint(t.position.ReadValue());
        if (Mouse.current != null && Mouse.current.leftButton.isPressed && !OverUI(-1))
            return cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        return null;
    }
}
