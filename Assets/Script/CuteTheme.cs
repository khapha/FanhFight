using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Shared pixel UI palette. Text stays live and readable at any screen resolution.
public static class CuteTheme
{
    public static readonly Color Ink = new Color32(43, 79, 68, 255);
    public static readonly Color Cream = new Color32(255, 249, 229, 255);
    public static readonly Color Mint = new Color32(188, 232, 176, 255);
    public static readonly Color Peach = new Color32(255, 215, 160, 255);
    public static readonly Color Pink = new Color32(241, 137, 157, 255);
    static Sprite frame;

    public static Sprite Frame
    {
        get
        {
            if (frame != null) return frame;
            Texture2D t = new Texture2D(20, 20, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            for (int y = 0; y < 20; y++) for (int x = 0; x < 20; x++)
            {
                bool cut = (x < 2 || x > 17) && (y < 2 || y > 17);
                bool edge = x < 2 || x > 17 || y < 2 || y > 17;
                Color c = edge ? Ink : y < 5 ? new Color32(218, 225, 191, 255) : Cream;
                if (y == 17 && x > 2 && x < 17) c = Color.white;
                t.SetPixel(x, y, cut ? Color.clear : c);
            }
            t.Apply();
            frame = Sprite.Create(t, new Rect(0, 0, 20, 20), Vector2.one * 0.5f, 100, 0,
                SpriteMeshType.FullRect, new Vector4(5, 5, 5, 5));
            return frame;
        }
    }

    public static void Button(Button button, bool primary = false)
    {
        Image image = button.GetComponent<Image>();
        image.sprite = Frame;
        image.type = Image.Type.Sliced;
        image.pixelsPerUnitMultiplier = 0.45f;
        image.color = Color.white;
        ColorBlock colors = button.colors;
        colors.normalColor = primary ? Mint : Color.white;
        colors.highlightedColor = colors.selectedColor = Peach;
        colors.pressedColor = new Color32(164, 209, 158, 255);
        colors.disabledColor = new Color(0.77f, 0.81f, 0.74f, 0.75f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        Text(text, Mathf.Min(26, text.fontSize));
        if (button.GetComponent<Shadow>() == null)
        {
            Shadow shadow = button.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.14f, 0.3f, 0.23f, 0.22f);
            shadow.effectDistance = new Vector2(0, -5);
        }
    }

    public static void Text(TMP_Text text, float size)
    {
        text.fontSize = size;
        text.color = Ink;
        text.fontStyle = FontStyles.Bold;
        text.outlineWidth = 0;
        text.characterSpacing = 0.5f;
        text.raycastTarget = false;
    }

    public static Image Card(Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
    {
        GameObject go = new GameObject("PixelCard");
        go.transform.SetParent(parent, false);
        Image image = go.AddComponent<Image>();
        image.sprite = Frame;
        image.type = Image.Type.Sliced;
        image.pixelsPerUnitMultiplier = 0.45f;
        image.raycastTarget = false;
        RectTransform rt = image.rectTransform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        return image;
    }
}
