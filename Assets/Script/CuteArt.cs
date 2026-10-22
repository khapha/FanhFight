using UnityEngine;

// Small decorative sprites follow the existing MiniArt code-generated pixel style.
public static class CuteArt
{
    static Sprite flower, butterfly, cloud, grass, dirt, stone, caveRim, sparkle, crystal, alert, wave;
    static readonly Sprite[] roach = new Sprite[2];

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

    public static Sprite Roach(int frame)
    {
        frame &= 1;
        if (roach[frame] != null) return roach[frame];
        Texture2D t = NewTexture(24, 18);
        for (int y = 0; y < 18; y++) for (int x = 0; x < 24; x++)
        {
            float body = (x - 14) * (x - 14) / 64f + (y - 8) * (y - 8) / 25f;
            float head = (x - 6) * (x - 6) / 25f + (y - 9) * (y - 9) / 25f;
            Color c = Color.clear;
            if (body <= 1 || head <= 1) c = CuteTheme.Ink;
            if (body < 0.72f) c = y > 8 ? new Color32(221, 161, 119, 255) : new Color32(180, 112, 91, 255);
            if (head < 0.72f) c = CuteTheme.Peach;
            if (x >= 3 && x <= 5 && y >= 9 && y <= 12) c = Color.white;
            if (x == 3 && y >= 10 && y <= 11) c = CuteTheme.Ink;
            if ((x == 4 || x == 8) && y >= 13 && y <= 15) c = CuteTheme.Ink;
            if (y == 15 && (x == 3 || x == 9)) c = CuteTheme.Pink;
            if (y == 2 + frame && (x == 10 || x == 15 || x == 19)) c = CuteTheme.Ink;
            if (x >= 11 && x <= 18 && y >= 12 + frame && y <= 14 + frame) c = new Color32(235, 239, 209, 255);
            t.SetPixel(x, y, c);
        }
        return roach[frame] = Finish(t, 16);
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
