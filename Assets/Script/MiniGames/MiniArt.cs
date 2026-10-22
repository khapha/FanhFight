using UnityEngine;

// Hình vẽ bằng pixel cho các mini-game (tự vẽ, không dùng ảnh ngoài)
public static class MiniArt
{
    static Sprite[] roach;
    static Sprite circle, flower, slipper, floor;

    // gián nhìn từ trên xuống, đầu hướng lên, mắt to cho đỡ sợ
    static readonly string[] Body = {
        "..A..........A..",
        "...A........A...",
        "....A......A....",
        ".....OOOOOO.....",
        "....OWWBBWWO....",
        "....OWKBBKWO....",
        ".....OBBBBO.....",
        "....OOOOOOOO....",
        "...OLLBBBBBBO...",
        "...OLBBBBBBBO...",
        "...OLBBBBBBBO...",
        "...OLBBBBBBBO...",
        "...OLBBBBBBBO...",
        "...OLBBBBBBBO...",
        "...OLBBBBBBBO...",
        "...OBBBBBBBBO...",
        "....OBBBBBBO....",
        ".....OBBBBO.....",
        "......OOOO......",
        "................",
    };

    public static Sprite[] Roach
    {
        get
        {
            if (roach == null)
                // hai khung hình: chân trước-sau so le nhau
                roach = new[] { MakeRoach(new[] { 8, 11, 14 }, new[] { 9, 12, 15 }),
                                MakeRoach(new[] { 9, 12, 15 }, new[] { 8, 11, 14 }) };
            return roach;
        }
    }

    static Sprite MakeRoach(int[] outRows, int[] inRows)
    {
        int w = 16, h = Body.Length;
        Texture2D t = NewTex(w, h);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                t.SetPixel(x, h - 1 - y, Col(Body[y][x]));
        Color leg = Col('A');
        // chân duỗi ra ngoài ở các hàng outRows, thu sát thân ở inRows
        foreach (int r in outRows) { Px(t, h, 0, r, leg); Px(t, h, 1, r, leg); Px(t, h, 14, r, leg); Px(t, h, 15, r, leg); Px(t, h, 2, r + 1, leg); Px(t, h, 13, r + 1, leg); }
        foreach (int r in inRows) { Px(t, h, 2, r, leg); Px(t, h, 13, r, leg); }
        t.Apply();
        return Sprite.Create(t, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16);
    }

    static void Px(Texture2D t, int h, int x, int y, Color c) { t.SetPixel(x, h - 1 - y, c); }

    static Color Col(char c)
    {
        switch (c)
        {
            case 'O': return new Color(0.2f, 0.12f, 0.08f);
            case 'B': return new Color(0.58f, 0.35f, 0.2f);
            case 'L': return new Color(0.78f, 0.53f, 0.32f);
            case 'A': return new Color(0.32f, 0.2f, 0.12f);
            case 'W': return Color.white;
            case 'K': return Color.black;
            default: return Color.clear;
        }
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
