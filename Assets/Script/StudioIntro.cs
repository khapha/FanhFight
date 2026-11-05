using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

// Màn giới thiệu studio Khapha: hiện một lần mỗi lần mở game, phủ lên menu chính.
// Huy hiệu chữ K pixel nảy vào, chữ hiện dần, giữ một nhịp rồi mờ đi.
// Bấm phím / chạm / click bất kỳ để bỏ qua. Dựng bằng code như các UI khác.
public class StudioIntro : MonoBehaviour
{
    public const string StudioName = "KHAPHA";
    const float FadeIn = 0.55f, Hold = 1.6f, FadeOut = 0.5f, SkipAfter = 0.35f;

    static bool shown;
    public static bool Active { get; private set; }

    CanvasGroup group;
    RectTransform badge;
    CanvasGroup words;
    Image[] sparkles;
    bool skip;
    bool restoreNavigation;

    public static void ShowOnce()
    {
        if (shown) return;
        shown = true;
        Active = true;
        Canvas canvas = MiniUI.MakeCanvas("StudioIntro");
        canvas.sortingOrder = 200;
        canvas.gameObject.AddComponent<StudioIntro>().Build(canvas.transform);
    }

    void Build(Transform root)
    {
        group = gameObject.AddComponent<CanvasGroup>();
        group.alpha = 1;

        // nền kem phủ kín, chặn click xuống menu bên dưới
        Image bg = new GameObject("Background").AddComponent<Image>();
        bg.transform.SetParent(root, false);
        bg.color = CuteTheme.Cream;
        Stretch(bg.rectTransform);

        // dải bạc hà nhạt ở đáy cho có chiều sâu
        Image band = new GameObject("Band").AddComponent<Image>();
        band.transform.SetParent(root, false);
        band.color = new Color(CuteTheme.Mint.r, CuteTheme.Mint.g, CuteTheme.Mint.b, 0.55f);
        band.raycastTarget = false;
        RectTransform br = band.rectTransform;
        br.anchorMin = new Vector2(0, 0);
        br.anchorMax = new Vector2(1, 0.22f);
        br.offsetMin = br.offsetMax = Vector2.zero;

        Transform safe = SafeArea.Create(root);

        badge = new GameObject("Badge").AddComponent<Image>().rectTransform;
        badge.SetParent(safe, false);
        Image badgeImage = badge.GetComponent<Image>();
        badgeImage.sprite = BadgeSprite;
        badgeImage.raycastTarget = false;
        badge.anchorMin = badge.anchorMax = new Vector2(0.5f, 0.5f);
        badge.anchoredPosition = new Vector2(0, 95);
        badge.sizeDelta = new Vector2(176, 176);
        badge.localScale = Vector3.zero;

        GameObject wordRoot = new GameObject("Words", typeof(RectTransform));
        wordRoot.transform.SetParent(safe, false);
        RectTransform wr = (RectTransform)wordRoot.transform;
        wr.anchorMin = wr.anchorMax = new Vector2(0.5f, 0.5f);
        wr.sizeDelta = new Vector2(900, 300);
        words = wordRoot.AddComponent<CanvasGroup>();
        words.alpha = 0;
        Word(wordRoot.transform, StudioName, 86, new Vector2(0, -60), 14f, CuteTheme.Ink);
        Word(wordRoot.transform, "S T U D I O", 26, new Vector2(0, -122), 6f, CuteTheme.Pink);
        Word(wordRoot.transform, "presents", 20, new Vector2(0, -170), 1f, new Color(CuteTheme.Ink.r, CuteTheme.Ink.g, CuteTheme.Ink.b, 0.65f));

        // lấp lánh quanh huy hiệu
        Vector2[] spots = { new Vector2(-130, 160), new Vector2(128, 140), new Vector2(-110, 30), new Vector2(118, 20), new Vector2(0, 205) };
        sparkles = new Image[spots.Length];
        for (int i = 0; i < spots.Length; i++)
        {
            Image s = new GameObject("Sparkle").AddComponent<Image>();
            s.transform.SetParent(safe, false);
            s.sprite = CuteArt.Sparkle;
            s.color = i % 2 == 0 ? CuteTheme.Peach : CuteTheme.Pink;
            s.raycastTarget = false;
            s.rectTransform.anchorMin = s.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            s.rectTransform.anchoredPosition = spots[i];
            s.rectTransform.sizeDelta = Vector2.one * 26;
            s.transform.localScale = Vector3.zero;
            sparkles[i] = s;
        }

        // tắt điều hướng UI trong lúc intro để Enter/A không bấm nhầm nút menu phía dưới
        if (EventSystem.current != null && EventSystem.current.sendNavigationEvents)
        {
            EventSystem.current.sendNavigationEvents = false;
            restoreNavigation = true;
        }
        StartCoroutine(Run());
    }

    static void Word(Transform parent, string text, int size, Vector2 pos, float spacing, Color color)
    {
        TextMeshProUGUI t = MiniUI.MakeText(parent, text, size, new Vector2(0.5f, 0.5f), pos);
        CuteTheme.Text(t, size);
        t.color = color;
        t.characterSpacing = spacing;
    }

    IEnumerator Run()
    {
        float t = 0;
        bool landed = false;
        // vào: huy hiệu nảy (vượt 1.15 rồi về 1), chữ hiện dần ngay sau
        while (t < FadeIn + Hold && !skip)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / FadeIn);
            float scale = k < 0.7f ? Mathf.Lerp(0, 1.15f, k / 0.7f) : Mathf.Lerp(1.15f, 1f, (k - 0.7f) / 0.3f);
            badge.localScale = Vector3.one * scale;
            badge.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(-12f, 0f, k));
            words.alpha = Mathf.Clamp01((t - FadeIn * 0.6f) / 0.4f);
            if (!landed && k >= 0.7f) { landed = true; Fx.Coin(); }
            for (int i = 0; i < sparkles.Length; i++)
            {
                float phase = t * 3.2f + i * 1.3f;
                float s = t < FadeIn * 0.8f ? 0 : 0.6f + Mathf.Sin(phase) * 0.4f;
                sparkles[i].transform.localScale = Vector3.one * Mathf.Max(0, s);
            }
            if (t > SkipAfter && AnyPress()) skip = true;
            yield return null;
        }
        // ra: mờ toàn bộ
        float start = group.alpha;
        for (float f = 0; f < FadeOut; f += Time.unscaledDeltaTime)
        {
            group.alpha = Mathf.Lerp(start, 0, f / FadeOut);
            yield return null;
        }
        Finish();
    }

    static bool AnyPress()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) return true;
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame) return true;
        Gamepad g = Gamepad.current;
        return g != null && (g.buttonSouth.wasPressedThisFrame || g.buttonEast.wasPressedThisFrame
            || g.buttonNorth.wasPressedThisFrame || g.buttonWest.wasPressedThisFrame || g.startButton.wasPressedThisFrame);
    }

    void Finish()
    {
        if (restoreNavigation && EventSystem.current != null) EventSystem.current.sendNavigationEvents = true;
        restoreNavigation = false;
        Active = false;
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (restoreNavigation && EventSystem.current != null) EventSystem.current.sendNavigationEvents = true;
        Active = false;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    // huy hiệu pixel 16x16: khung bo góc màu mực, nền đào, chữ K hồng, chồi lá bạc hà trên đỉnh
    static Sprite badgeSprite;
    static Sprite BadgeSprite
    {
        get
        {
            if (badgeSprite != null) return badgeSprite;
            string[] rows = {
                ".....GG.........",
                "....GGGG........",
                "..OOOGGOOOOOOO..",
                ".OHHHHHHHHHHHHO.",
                "OPPPPPPPPPPPPPPO",
                "OPPKKPPPPPKKPPPO",
                "OPPKKPPPPKKPPPPO",
                "OPPKKPPPKKPPPPPO",
                "OPPKKPPKKPPPPPPO",
                "OPPKKKKKPPPPPPPO",
                "OPPKKPPKKPPPPPPO",
                "OPPKKPPPKKPPPPPO",
                "OPPKKPPPPKKPPPPO",
                "OPPKKPPPPPKKPPPO",
                ".OPPPPPPPPPPPPO.",
                "..OOOOOOOOOOOO..",
            };
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    Color c;
                    switch (rows[15 - y][x])
                    {
                        case 'O': c = CuteTheme.Ink; break;
                        case 'P': c = CuteTheme.Peach; break;
                        case 'H': c = new Color32(255, 234, 200, 255); break; // viền sáng
                        case 'K': c = CuteTheme.Pink; break;
                        case 'G': c = new Color32(110, 190, 120, 255); break;
                        default: c = Color.clear; break;
                    }
                    tex.SetPixel(x, y, c);
                }
            tex.Apply();
            return badgeSprite = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16);
        }
    }
}
