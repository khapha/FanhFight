using UnityEngine;

// Small decorative sprites follow the existing MiniArt code-generated pixel style.
public static class CuteArt
{
    static Sprite flower, butterfly, cloud, grass, dirt, stone, caveRim, sparkle, crystal, alert, wave;
    static readonly Sprite[] roachVariants = new Sprite[12];
    static readonly Sprite[] kingRoach = new Sprite[2];

    public static Sprite Sparkle => sparkle != null ? sparkle : sparkle = Pixels(new[] {
        "...Y...", "...Y...", "..YYY..", "YYYYYYY", "..YYY..", "...Y...", "...Y..."
    }, 16);
    public static Sprite Crystal => crystal != null ? crystal : crystal = Pixels(new[] {
        "....L...", "...LLL..", "...LWL..", "..LLWLL.", "..LLWLL.", "..LLWLL.", "..LLLLL.", "...GGG.."
    }, 12);
    public static Sprite Alert => alert != null ? alert : alert = Pixels(new[] {
        "..YYYYY..", ".YYYYYYY.", "YYYGGGYYY", "YYYGGGYYY", "YYYGGGYYY",
        "YYYYYYYYY", "YYYGGGYYY", ".YYYYYYY.", "..YYY...."
    }, 14);

    public static Sprite Wave
    {
        get
        {
            if (wave != null) return wave;
            Texture2D t = NewTexture(32, 4);
            for (int y = 0; y < 4; y++) for (int x = 0; x < 32; x++)
                t.SetPixel(x, y, y == (x / 4) % 2 + 1 ? CuteTheme.Cream : new Color(0.35f, 0.85f, 0.9f, 0.35f));
            t.wrapMode = TextureWrapMode.Repeat;
            return wave = Finish(t, 16);
        }
    }

    public static Sprite Roach(int frame) => RoachVariant(frame, 0);

    // All variants retain the original 1.5 x 1.125 world footprint and face left.
    // The extra source pixels describe insect anatomy without changing colliders.
    public static Sprite RoachVariant(int frame, int variant, bool flying = false)
    {
        frame &= 1;
        variant = Mathf.Clamp(variant, 0, 2);
        flying = variant == 2 && flying;
        int index = variant * 4 + (flying ? 2 : 0) + frame;
        if (roachVariants[index] == null)
            roachVariants[index] = BuildRoach(frame, variant, flying, false);
        return roachVariants[index];
    }

    public static Sprite KingRoach(int frame)
    {
        frame &= 1;
        if (kingRoach[frame] == null) kingRoach[frame] = BuildRoach(frame, 1, false, true);
        return kingRoach[frame];
    }

    // Small head under a broad pronotum, overlapping forewings, six jointed legs,
    // long antennae and paired cerci give the silhouette its cockroach identity.
    static Sprite BuildRoach(int frame, int variant, bool flying, bool king)
    {
        var p = new RoachPixels();
        Color32 ink = new Color32(38, 24, 22, 255);
        Color32 leg = new Color32(105, 58, 39, 255);
        Color32 farLeg = new Color32(74, 47, 37, 255);
        Color32 shadow = new Color32(77, 35, 27, 255);
        Color32 shell = new Color32(137, 64, 38, 255);
        Color32 light = new Color32(190, 108, 58, 255);
        Color32 glint = new Color32(227, 165, 97, 255);
        bool armored = variant == 1;
        if (armored)
        {
            shadow = new Color32(46, 45, 39, 255);
            shell = new Color32(83, 86, 68, 255);
            light = new Color32(132, 139, 99, 255);
            glint = new Color32(192, 184, 119, 255);
        }
        if (variant == 2)
        {
            shadow = new Color32(78, 43, 30, 255);
            shell = new Color32(152, 91, 46, 255);
            light = new Color32(194, 139, 76, 255);
        }
        if (king)
        {
            shadow = new Color32(65, 38, 57, 255);
            shell = new Color32(112, 61, 92, 255);
            light = new Color32(162, 106, 130, 255);
            glint = new Color32(227, 183, 101, 255);
        }

        // Two veined hindwings spread above the hard wing cases while flying.
        if (variant == 2 && flying)
        {
            int flap = frame * 3;
            Color32 edge = new Color32(131, 122, 90, 235);
            Color32 membrane = new Color32(226, 217, 173, 215);
            p.Polygon(new[] { 22, 19, 22, 28 - flap, 27, 34 - flap, 32, 35 - flap, 36, 31 - flap, 29, 21 }, edge);
            p.Polygon(new[] { 24, 21, 24, 28 - flap, 28, 32 - flap, 32, 33 - flap, 33, 30 - flap, 28, 22 }, membrane);
            p.Line(24, 21, 30, 32 - flap, edge);
            p.Line(26, 24, 32, 29 - flap, edge);
            p.Polygon(new[] { 28, 20, 35, 30 - flap, 42, 34 - flap, 46, 31 - flap, 46, 28 - flap, 35, 20 }, edge);
            p.Polygon(new[] { 30, 21, 36, 29 - flap, 42, 32 - flap, 44, 30 - flap, 44, 28 - flap, 35, 22 }, membrane);
            p.Line(30, 21, 43, 31 - flap, edge);
            p.Line(35, 25, 44, 28 - flap, edge);
        }

        // The far tripod leads the near one by half a stride. Each leg has a knee.
        for (int i = 0; i < 3; i++)
        {
            int stride = ((i + frame) & 1) == 0 ? -2 : 2;
            int hip = 19 + i * 5;
            int knee = i == 0 ? 14 : i == 1 ? 26 : 37;
            int toe = i == 0 ? 10 : i == 1 ? 26 : 42;
            p.Line(hip, 14, knee + stride, 9, farLeg);
            p.Line(knee + stride, 9, toe + stride, 5 + (stride > 0 ? 1 : 0), farLeg);
        }

        // Flattened abdomen and the shield-shaped pronotum overlap the small head.
        float ry = armored ? 8f : 6.5f;
        p.Ellipse(34, 17, 12, ry, ink);
        p.Ellipse(34, 17.5f, 11, ry - 1f, shadow);
        p.Ellipse(33, 18.5f, 10, ry - 2f, shell);
        p.Ellipse(21, 18.5f, armored ? 8f : 7f, armored ? 6.5f : 5.5f, ink);
        p.Ellipse(21, 19, armored ? 7f : 6f, armored ? 5.5f : 4.5f, shell);
        p.Ellipse(13.5f, 17.5f, 4f, 3.5f, ink);
        p.Ellipse(13.5f, 18, 3f, 2.5f, shadow);
        p.Line(15, 20, 18, 22, light);
        p.Line(18, 22, 23, 23, light);
        p.Line(24, 22, 25, 20, light);

        // A tapered seam and fine longitudinal veins distinguish folded wing cases.
        p.Line(27, 21, 35, 20, shadow);
        p.Line(35, 20, 43, 17, shadow);
        p.Line(43, 17, 45, 16, ink);
        p.Line(27, 23, 34, 23, light);
        p.Line(34, 23, 40, 21, light);
        p.Line(28, 18, 35, 16, light);
        p.Line(35, 16, 41, 14, shadow);
        p.Line(28, 16, 34, 14, shadow);
        p.Line(29, 13, 34, 12, light);
        p.Line(34, 12, 39, 12, shadow);
        p.Set(29, 23, glint);
        p.Set(30, 23, glint);
        p.Set(19, 23, glint);

        // Segmented exposed abdominal edge, rather than a round unbroken blob.
        for (int x = 29; x <= 41; x += 4)
        {
            p.Line(x, 11, x + 1, 13, ink);
            p.Set(x + 1, 11, shell);
        }
        if (armored)
        {
            // Raised plates have dark joints and a pale rim; the body is broader.
            for (int x = 27; x <= 39; x += 5)
            {
                p.Line(x, 23, x + 2, 20, shadow);
                p.Line(x + 2, 20, x + 1, 15, shadow);
                p.Line(x + 1, 23, x + 3, 20, light);
            }
            p.Line(16, 23, 20, 25, light);
            p.Line(20, 25, 24, 24, glint);
        }

        // Front, middle and hind legs fan out in different directions, with spines.
        for (int i = 0; i < 3; i++)
        {
            int stride = ((i + frame) & 1) == 0 ? 2 : -2;
            int hip = 18 + i * 6;
            int knee = i == 0 ? 16 : i == 1 ? 28 : 39;
            int toe = i == 0 ? 12 : i == 1 ? 29 : 45;
            int footY = 3 + (stride < 0 ? 1 : 0);
            p.Line(hip, 14, knee + stride, 9, ink);
            p.Line(hip, 13, knee + stride, 8, leg);
            p.Line(knee + stride, 9, toe + stride, footY, ink);
            p.Line(knee + stride, 8, toe + stride, footY + 1, leg);
            p.Set(knee + stride + 1, 7, leg);
            p.Line(toe + stride, footY, toe + stride - 2, footY, ink);
        }

        // A tiny compound eye and mandibles keep the face insect-like.
        p.Set(11, 19, new Color32(213, 148, 88, 255));
        p.Set(11, 18, ink);
        p.Line(10, 16, 8, 15, leg);
        p.Line(11, 15, 10, 14, ink);
        int wiggle = frame == 0 ? 0 : 1;
        p.Line(12, 21, 8, 25, ink);
        p.Line(8, 25, 4, 29 + wiggle, leg);
        p.Line(4, 29 + wiggle, 0, 30 + wiggle, ink);
        p.Line(11, 20, 6, 22, leg);
        p.Line(6, 22, 2, 25 - wiggle, ink);
        p.Line(2, 25 - wiggle, 0, 27 - wiggle, leg);
        p.Line(44, 14, 47, 12, ink);
        p.Line(44, 16, 47, 15 + wiggle, leg);

        Texture2D texture = NewTexture(RoachPixels.Width, RoachPixels.Height);
        texture.SetPixels32(p.Colors);
        return Finish(texture, 32);
    }

    // Raster helpers are used only on a cache miss; no pixel work happens per frame.
    sealed class RoachPixels
    {
        public const int Width = 48, Height = 36;
        public readonly Color32[] Colors = new Color32[Width * Height];
        public void Set(int x, int y, Color32 color)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height) Colors[y * Width + x] = color;
        }
        public void Line(float x0, float y0, float x1, float y1, Color32 color)
        {
            int count = Mathf.CeilToInt(Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0))) + 1;
            for (int i = 0; i <= count; i++)
            {
                float t = i / (float)count;
                Set(Mathf.RoundToInt(x0 + (x1 - x0) * t), Mathf.RoundToInt(y0 + (y1 - y0) * t), color);
            }
        }
        public void Ellipse(float cx, float cy, float rx, float ry, Color32 color)
        {
            for (int y = 0; y < Height; y++) for (int x = 0; x < Width; x++)
                if ((x - cx) * (x - cx) / (rx * rx) + (y - cy) * (y - cy) / (ry * ry) <= 1f) Set(x, y, color);
        }
        public void Polygon(int[] vertices, Color32 color)
        {
            for (int y = 0; y < Height; y++) for (int x = 0; x < Width; x++)
            {
                bool inside = false;
                for (int i = 0, j = vertices.Length - 2; i < vertices.Length; j = i, i += 2)
                {
                    float ax = vertices[i], ay = vertices[i + 1], bx = vertices[j], by = vertices[j + 1];
                    if ((ay > y) != (by > y) && x < (bx - ax) * (y - ay) / (by - ay) + ax) inside = !inside;
                }
                if (inside) Set(x, y, color);
            }
        }
    }
    public static Sprite Flower => flower != null ? flower : flower = Pixels(new[] {
        "....PP....", "...PPPP...", ".PPPPPPPP.", ".PPYYYYPP.", "..PYYYYP..",
        "...PPPP...", "....GG....", "..GLGG....", "....GGLG..", "....GG...."
    }, 12);

    public static Sprite Butterfly => butterfly != null ? butterfly : butterfly = Pixels(new[] {
        "P......P", "PP.GG.PP", "PPPGGPPP", ".PPGGPP.", ".PPGGPP.", "P..GG..P"
    }, 18);

    public static Sprite Cloud
    {
        get
        {
            if (cloud != null) return cloud;
            Texture2D t = NewTexture(48, 22);
            for (int y = 0; y < 22; y++) for (int x = 0; x < 48; x++)
            {
                bool inside = (new Vector2(x - 13, y - 10)).sqrMagnitude < 81
                    || (new Vector2(x - 25, y - 12)).sqrMagnitude < 121
                    || (new Vector2(x - 36, y - 8)).sqrMagnitude < 49
                    || (x > 7 && x < 41 && y > 2 && y < 10);
                t.SetPixel(x, y, !inside ? Color.clear : y < 6 ? new Color32(218, 238, 219, 255) : CuteTheme.Cream);
            }
            cloud = Finish(t, 16);
            return cloud;
        }
    }

    public static Sprite Grass => grass != null ? grass : grass = Tile(true);
    public static Sprite Dirt => dirt != null ? dirt : dirt = Tile(false);
    public static Sprite Stone => stone != null ? stone : stone = Tile(false, true);
    public static Sprite CaveRim => caveRim != null ? caveRim : caveRim = Tile(true, true);

    static Sprite Tile(bool grassy, bool cave = false)
    {
        Texture2D t = NewTexture(16, 16);
        for (int y = 0; y < 16; y++) for (int x = 0; x < 16; x++)
        {
            Color c = new Color32(190, 143, 99, 255);
            if (x >= 3 && x <= 4 && y >= 5 && y <= 6) c = new Color32(213, 169, 121, 255);
            if (x >= 11 && x <= 12 && y >= 11 && y <= 12) c = new Color32(173, 128, 91, 255);
            if (x == 8 && y == 1) c = new Color32(213, 169, 121, 255);
            if (grassy && y >= 11 + (x % 5 == 0 ? -2 : 0))
                c = y >= 14 ? new Color32(187, 224, 126, 255) : new Color32(119, 183, 105, 255);
            if (grassy && y == 15) c = new Color32(216, 239, 153, 255);
            if (cave)
            {
                c = new Color32(139, 137, 180, 255);
                if (x >= 3 && x <= 5 && y >= 5 && y <= 6) c = new Color32(163, 160, 198, 255);
                if (x >= 11 && x <= 12 && y >= 11 && y <= 12) c = new Color32(117, 116, 160, 255);
                if (grassy && y >= 13) c = y == 15 ? new Color32(209, 209, 235, 255) : new Color32(166, 171, 208, 255);
            }
            t.SetPixel(x, y, c);
        }
        t.wrapMode = TextureWrapMode.Repeat;
        return Finish(t, 16);
    }

    static Sprite Pixels(string[] rows, int ppu)
    {
        Texture2D t = NewTexture(rows[0].Length, rows.Length);
        for (int y = 0; y < rows.Length; y++) for (int x = 0; x < rows[0].Length; x++)
        {
            char c = rows[y][x];
            t.SetPixel(x, rows.Length - y - 1, c == 'P' ? CuteTheme.Pink : c == 'Y' ? CuteTheme.Peach
                : c == 'G' ? CuteTheme.Ink : c == 'L' ? CuteTheme.Mint : c == 'W' ? Color.white : Color.clear);
        }
        return Finish(t, ppu);
    }

    static Texture2D NewTexture(int w, int h)
    {
        return new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
    }

    static Sprite Finish(Texture2D t, int ppu)
    {
        t.Apply();
        return Sprite.Create(t, new Rect(0, 0, t.width, t.height), Vector2.one * 0.5f, ppu, 0, SpriteMeshType.FullRect);
    }
}
