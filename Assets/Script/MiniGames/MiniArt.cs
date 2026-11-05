using UnityEngine;

// Hình vẽ bằng pixel cho các mini-game (tự vẽ, không dùng ảnh ngoài)
public static class MiniArt
{
    static Sprite[] roach;
    static Sprite circle, flower, slipper, floor;

    // Top-down cockroaches keep the original 1 x 1.25 world footprint, head upward.
    public static Sprite[] Roach
    {
        get
        {
            if (roach == null) roach = new[] { MakeRoach(0), MakeRoach(1) };
            return roach;
        }
    }

    static Sprite MakeRoach(int frame)
    {
        const int w = 32, h = 40;
        Color32[] pixels = new Color32[w * h];
        Color32 ink = new Color32(40, 25, 21, 255);
        Color32 dark = new Color32(81, 38, 27, 255);
        Color32 shell = new Color32(137, 66, 38, 255);
        Color32 light = new Color32(188, 113, 64, 255);
        Color32 leg = new Color32(106, 65, 41, 255);
        System.Action<int, int, Color32> set = (x, y, c) =>
        {
            if (x >= 0 && x < w && y >= 0 && y < h) pixels[y * w + x] = c;
        };
        System.Action<float, float, float, float, Color32> line = (x0, y0, x1, y1, c) =>
        {
            int n = Mathf.CeilToInt(Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0))) + 1;
            for (int i = 0; i <= n; i++)
            {
                float t = i / (float)n;
                set(Mathf.RoundToInt(x0 + (x1 - x0) * t), Mathf.RoundToInt(y0 + (y1 - y0) * t), c);
            }
        };
        System.Action<float, float, float, float, Color32> ellipse = (cx, cy, rx, ry, c) =>
        {
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++)
                if ((x - cx) * (x - cx) / (rx * rx) + (y - cy) * (y - cy) / (ry * ry) <= 1f) set(x, y, c);
        };

        // Three splayed pairs with jointed tibiae; alternate tripods scuttle together.
        for (int side = -1; side <= 1; side += 2)
        {
            for (int pair = 0; pair < 3; pair++)
            {
                int stride = ((pair + frame + (side > 0 ? 1 : 0)) & 1) == 0 ? 2 : -2;
                float hipX = 15.5f + side * 5f;
                int hipY = 24 - pair * 5;
                float kneeX = 15.5f + side * 10f;
                int kneeY = hipY + (pair == 0 ? 2 : -2) + stride;
                float toeX = 15.5f + side * 14f;
                int toeY = hipY + (pair == 0 ? 5 : pair == 1 ? -2 : -7) + stride;
                line(hipX, hipY, kneeX, kneeY, ink);
                line(kneeX, kneeY, toeX, toeY, leg);
                line(toeX, toeY, toeX - side, toeY - 2, ink);
                set(Mathf.RoundToInt(kneeX + side), kneeY - 1, leg);
            }
        }
        ellipse(15.5f, 16, 8f, 11.5f, ink);
        ellipse(15.5f, 16.5f, 7f, 10f, dark);
        ellipse(15.5f, 18, 6f, 9f, shell);
        ellipse(15.5f, 29, 3.5f, 3.5f, ink);
        ellipse(15.5f, 29, 2.5f, 2.5f, dark);
        ellipse(15.5f, 25, 6f, 5f, ink);
        ellipse(15.5f, 25.5f, 5f, 4f, shell);
        line(12, 28, 16, 29, light);
        line(16, 29, 19, 27, light);

        // Folded forewing seam and slender veins, with exposed abdominal segments.
        line(15, 24, 15, 6, dark);
        line(16, 23, 16, 7, ink);
        line(11, 24, 10, 17, light);
        line(10, 17, 12, 10, light);
        line(20, 23, 21, 17, light);
        line(21, 17, 19, 11, dark);
        for (int y = 7; y <= 13; y += 3)
        {
            line(10, y + 1, 13, y, dark);
            line(18, y, 21, y + 1, dark);
        }
        set(11, 22, new Color32(226, 164, 99, 255));
        set(11, 21, new Color32(226, 164, 99, 255));
        int wiggle = frame == 0 ? 0 : 1;
        line(14, 31, 10, 35, ink);
        line(10, 35, 5, 38 - wiggle, leg);
        line(5, 38 - wiggle, 0, 39 - wiggle, ink);
        line(17, 31, 22, 35, ink);
        line(22, 35, 26, 38 + wiggle, leg);
        line(26, 38 + wiggle, 31, 38, ink);
        line(13, 6, 11, 2, ink);
        line(18, 6, 20, 2, ink);

        Texture2D texture = NewTex(w, h);
        texture.SetPixels32(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, w, h), Vector2.one * 0.5f, 32);
    }
    public static Sprite Circle
    {
        get
        {
            if (circle != null) return circle;
            Texture2D t = NewTex(32, 32);
            for (int y = 0; y < 32; y++)
                for (int x = 0; x < 32; x++)
                    t.SetPixel(x, y, new Vector2(x - 15.5f, y - 15.5f).magnitude <= 15.5f ? Color.white : Color.clear);
            t.Apply();
            return circle = Sprite.Create(t, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
        }
    }

    // bông hoa 5 cánh: gián bị xịt xong biến thành hoa
    public static Sprite Flower
    {
        get
        {
            if (flower != null) return flower;
            Texture2D t = NewTex(16, 16);
            Vector2 c = new Vector2(7.5f, 7.5f);
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    Vector2 p = new Vector2(x, y) - c;
                    float a = Mathf.Atan2(p.y, p.x);
                    float r = 4.5f + 3f * Mathf.Abs(Mathf.Cos(a * 2.5f));
                    Color col = Color.clear;
                    if (p.magnitude <= r) col = Color.white;
                    if (p.magnitude <= 2.6f) col = new Color(1f, 0.85f, 0.3f);
                    t.SetPixel(x, y, col);
                }
            t.Apply();
            return flower = Sprite.Create(t, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16);
        }
    }

    // chiếc dép lê màu hồng nhìn từ trên xuống, có viền và quai
    public static Sprite Slipper
    {
        get
        {
            if (slipper != null) return slipper;
            int w = 12, h = 22;
            Texture2D t = NewTex(w, h);
            System.Func<int, int, bool> sole = (x, y) =>
            {
                if (x < 0 || y < 0 || x >= w || y >= h) return false;
                float nx = (x - 5.5f) / 6f, ny = (y - 10.5f) / 11f;
                return nx * nx + ny * ny * 0.9f <= 1f;
            };
            Color pink = new Color(1f, 0.6f, 0.75f);
            Color pinkLight = new Color(1f, 0.75f, 0.85f);
            Color edge = new Color(0.75f, 0.3f, 0.45f);
            Color strap = new Color(0.95f, 0.35f, 0.55f);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    Color col = Color.clear;
                    if (sole(x, y))
                    {
                        bool border = !sole(x - 1, y) || !sole(x + 1, y) || !sole(x, y - 1) || !sole(x, y + 1);
                        col = border ? edge : ((x + y) % 3 == 0 ? pinkLight : pink);
                        // quai dép ngang phía mũi
                        if (!border && y >= 13 && y <= 16) col = (y == 13 || y == 16) ? edge : strap;
                        // tim nhỏ trên quai
                        if (y == 15 && (x == 5 || x == 6)) col = Color.white;
                    }
                    t.SetPixel(x, y, col);
                }
            t.Apply();
            return slipper = Sprite.Create(t, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 10);
        }
    }

    static Sprite grass, bottle;

    // bãi cỏ cho khu vườn
    public static Sprite Grass
    {
        get
        {
            if (grass != null) return grass;
            Texture2D t = NewTex(16, 16);
            t.wrapMode = TextureWrapMode.Repeat;
            System.Random rnd = new System.Random(7);
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    double v = rnd.NextDouble();
                    Color col = new Color(0.45f, 0.72f, 0.38f);
                    if (v < 0.15) col = new Color(0.38f, 0.63f, 0.32f);
                    else if (v < 0.25) col = new Color(0.55f, 0.8f, 0.45f);
                    t.SetPixel(x, y, col);
                }
            t.Apply();
            return grass = Sprite.Create(t, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 8, 0, SpriteMeshType.FullRect);
        }
    }

    // bình xịt: thân xanh lá, nhãn trắng, vòi đỏ ở trên
    static readonly string[] BottleArt = {
        "....RRR...",
        "....RRRR..",
        "....GG....",
        "...OOOO...",
        "..OGGGGO..",
        ".OGGGGGGO.",
        ".OGLGGGGO.",
        ".OGLGGGGO.",
        ".OWWWWWWO.",
        ".OWKWKWWO.",
        ".OWWWWWWO.",
        ".OWKKWWWO.",
        ".OWWWWWWO.",
        ".OGLGGGGO.",
        ".OGLGGGGO.",
        ".OGGGGGGO.",
        ".OGGGGGGO.",
        "..OOOOOO..",
    };

    public static Sprite Bottle
    {
        get
        {
            if (bottle != null) return bottle;
            int w = 10, h = BottleArt.Length;
            Texture2D t = NewTex(w, h);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    Color col;
                    switch (BottleArt[y][x])
                    {
                        case 'R': col = new Color(0.9f, 0.25f, 0.25f); break;
                        case 'G': col = new Color(0.3f, 0.7f, 0.35f); break;
                        case 'L': col = new Color(0.55f, 0.9f, 0.55f); break;
                        case 'O': col = new Color(0.12f, 0.3f, 0.15f); break;
                        case 'W': col = Color.white; break;
                        case 'K': col = new Color(0.3f, 0.3f, 0.3f); break;
                        default: col = Color.clear; break;
                    }
                    t.SetPixel(x, h - 1 - y, col);
                }
            t.Apply();
            // gốc ở chân bình, để vòi nằm phía trên điểm chạm
            return bottle = Sprite.Create(t, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), 10);
        }
    }

    // sàn gạch bếp ca-rô
    public static Sprite Floor
    {
        get
        {
            if (floor != null) return floor;
            Texture2D t = NewTex(16, 16);
            t.wrapMode = TextureWrapMode.Repeat;
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                {
                    bool a = (x < 8) == (y < 8);
                    Color col = a ? new Color(0.93f, 0.88f, 0.78f) : new Color(0.86f, 0.8f, 0.7f);
                    if (x % 8 == 0 || y % 8 == 0) col *= 0.93f;
                    col.a = 1;
                    t.SetPixel(x, y, col);
                }
            t.Apply();
            return floor = Sprite.Create(t, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 8, 0, SpriteMeshType.FullRect);
        }
    }

    static Texture2D NewTex(int w, int h)
    {
        return new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
    }
}
