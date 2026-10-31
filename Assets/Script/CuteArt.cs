using UnityEngine;

// Small decorative sprites follow the existing MiniArt code-generated pixel style.
public static class CuteArt
{
    static Sprite flower, butterfly, cloud, grass, dirt, stone, caveRim, sparkle, crystal, alert, wave;
    static readonly Sprite[] roach = new Sprite[2];
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

    public static Sprite Roach(int frame)
    {
        frame &= 1;
        if (roach[frame] != null) return roach[frame];
        return roach[frame] = BuildRoach(frame, false);
    }

    // Vua Gián có sprite riêng: thân tím hoàng gia, bụng vàng, lông mày hề,
    // chốt vàng trên râu — nhìn phát biết ngay là boss chứ không phải gián thường
    public static Sprite KingRoach(int frame)
    {
        frame &= 1;
        if (kingRoach[frame] != null) return kingRoach[frame];
        return kingRoach[frame] = BuildRoach(frame, true);
    }

    // Gián cute ĐÚNG GIẢI PHẪU: thân bầu dục dẹp, đầu nhỏ nhô ra, 2 râu cong về trước,
    // 6 chân mọc từ đáy thân, 2 râu đuôi (cerci) — nhưng vẫn mắt to má hồng vô hại.
    // Toạ độ y-up trùng với gốc texture Unity (đáy-trái) nên vẽ thẳng không cần lật.
    static Sprite BuildRoach(int frame, bool king)
    {
        const int W = 24, H = 18;
        Color32 clear = new Color32(0, 0, 0, 0);
        Color32 ink = new Color32(34, 28, 34, 255);
        Color32 white = new Color32(255, 255, 255, 255);
        Color32 blush = new Color32(244, 158, 148, 255);
        Color32 gold = new Color32(244, 196, 76, 255);
        Color32 mid = king ? new Color32(122, 74, 128, 255) : new Color32(176, 116, 82, 255);
        Color32 dark = king ? new Color32(84, 48, 94, 255) : new Color32(128, 78, 58, 255);
        Color32 hi = king ? new Color32(176, 126, 182, 255) : new Color32(221, 168, 120, 255);
        Color32 belly = king ? gold : new Color32(140, 86, 64, 255);
        Color32 headCol = king ? new Color32(100, 58, 108, 255) : new Color32(150, 92, 66, 255);
        Color32 legCol = king ? ink : new Color32(96, 70, 66, 255);
        Color32 farLeg = new Color32(110, 84, 78, 255);

        float bob = frame * 0.5f;
        float acx = king ? 14.5f : 15f, acy = 9.8f + bob;
        float arx = king ? 8.6f : 7.4f, ary = king ? 6f : 5.2f;
        float hcx = king ? 5.2f : 5.6f, hcy = 10f + bob;
        float hrx = king ? 3.9f : 3.2f, hry = king ? 3.4f : 2.9f;

        Texture2D t = NewTexture(W, H);
        Color32[] px = new Color32[W * H];
        for (int i = 0; i < px.Length; i++) px[i] = clear;
        System.Action<int, int, Color32> set = (x, y, c) =>
        {
            if (x >= 0 && x < W && y >= 0 && y < H) px[y * W + x] = c;
        };
        System.Action<float, float, float, float, Color32> seg = (x0, y0, x1, y1, col) =>
        {
            int n = (int)(Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0)) * 2f) + 1;
            for (int i = 0; i <= n; i++)
            {
                float tt = n == 0 ? 0f : i / (float)n;
                set(Mathf.RoundToInt(x0 + (x1 - x0) * tt), Mathf.RoundToInt(y0 + (y1 - y0) * tt), col);
            }
        };

        // 6 chân: 3 cặp so le (frame đổi chân nhấc), màu chân nhạt hơn outline để tách rõ
        float[] hips = king ? new[] { 9.8f, 13.8f, 17.6f } : new[] { 8.8f, 14f, 18.8f };
        for (int i = 0; i < hips.Length; i++)
        {
            float lx = hips[i];
            int ph = (i + frame) % 2;
            float d = i % 2 == 0 ? -1f : 1f;
            float kx = lx + d * (1.7f + ph * 0.9f);
            seg(lx, 5.6f + bob, kx, 4f, legCol);
            seg(kx, 4f, kx + d * 0.6f, 2.2f + ph * 0.4f, legCol);
            float fx = lx - d * 1.6f;
            seg(fx, 5.8f + bob, fx + d * 0.8f, 3.6f, farLeg);
        }

        // thân + đầu + highlight + bụng
        for (int y = 0; y < H; y++) for (int x = 0; x < W; x++)
        {
            float bx = x, by = y;
            float body = (bx - acx) * (bx - acx) / (arx * arx) + (by - acy) * (by - acy) / (ary * ary);
            float head = (bx - hcx) * (bx - hcx) / (hrx * hrx) + (by - hcy) * (by - hcy) / (hry * hry);
            Color32 c = clear;
            if (body <= 1f) c = body > 0.66f ? dark : mid;
            if (head <= 1f) c = head > 0.62f ? hi : headCol; // viền đầu sáng tách đầu khỏi thân
            if (body <= 0.5f && by > acy)
            {
                float hl = (bx - acx + 2.4f) * (bx - acx + 2.4f) / (arx * 0.66f * (arx * 0.66f))
                         + (by - acy - 1f) * (by - acy - 1f) / (ary * 0.42f * (ary * 0.42f));
                if (hl < 1f) c = hi;
            }
            if (body <= 0.8f && Mathf.Abs(by - acy - 0.4f) < 0.6f && bx > hcx + 3f) c = dark; // đường giữa 2 cánh
            if (body <= 0.78f && by < acy - ary * 0.45f) c = belly; // bụng dưới (vàng cho boss)
            if (c.a != 0) set(x, y, c);
        }

        // 2 râu cong mềm về trước, tách riêng 2 màu + 2 độ cong; boss có chốt vàng đầu râu
        float wig = frame == 1 ? 0.8f : -0.8f;
        for (int a = 0; a < 2; a++)
        {
            float sx = 5.4f, sy = a == 0 ? 13.6f : 12.6f, dy0 = a == 0 ? 0f : 0.55f;
            Color32 col = ink;
            if (!king)
            {
                sy = a == 0 ? 13.8f : 12.2f;
                dy0 = a == 0 ? 0f : 0.9f;
                col = a == 0 ? ink : dark;
            }
            Vector2 prev = Vector2.zero;
            bool has = false;
            for (int i = 0; i <= 8; i++)
            {
                float tt = i / 8f;
                float x1 = sx - 4.3f * tt;
                float y1 = sy + 2.3f * tt * tt + Mathf.Sin(tt * 3.1f + wig + dy0) * 0.7f;
                if (has) seg(prev.x, prev.y, x1, y1, col);
                prev = new Vector2(x1, y1);
                has = true;
            }
            if (king)
            {
                int ex = Mathf.Clamp(Mathf.RoundToInt(prev.x), 0, W - 1);
                int ey = Mathf.Clamp(Mathf.RoundToInt(prev.y), 0, H - 1);
                set(ex, ey, gold);
                set(Mathf.Min(W - 1, ex + 1), ey, gold);
            }
        }

        // 2 râu đuôi (cerci) nhô rõ khỏi mỏm sau, màu đậm tương phản
        float rear = acx + arx - (king ? 2f : 1.4f);
        if (king)
        {
            seg(rear, 7.0f + bob, Mathf.Min(rear + 2.2f, 23.2f), 5.6f + bob, ink);
            seg(rear + 0.8f, 7.0f + bob, Mathf.Min(rear + 2.4f, 23.3f), 6.1f + bob, ink);
            seg(rear, 8.6f + bob, Mathf.Min(rear + 2.2f, 23.2f), 7.8f + bob, ink);
            seg(rear + 0.8f, 8.6f + bob, Mathf.Min(rear + 2.4f, 23.3f), 9.1f + bob, ink);
        }
        else
        {
            seg(rear, 6.4f + bob, Mathf.Min(rear + 2.6f, 23.4f), 5.2f + bob, ink);
            seg(rear + 0.8f, 6.0f + bob, Mathf.Min(rear + 2.8f, 23.5f), 5.5f + bob, ink);
            seg(rear, 7.8f + bob, Mathf.Min(rear + 2.6f, 23.4f), 7.2f + bob, ink);
            seg(rear + 0.8f, 7.4f + bob, Mathf.Min(rear + 2.8f, 23.5f), 7.5f + bob, ink);
        }

        // mắt to tròn long lanh + má hồng + (boss) lông mày hề
        float erx = king ? 2f : 1.6f, ery = king ? 2.2f : 1.8f;
        float ecx = king ? 3.9f : 4.1f, ecy = 11.6f + bob;
        for (int y = 0; y < H; y++) for (int x = 0; x < W; x++)
        {
            float bx = x, by = y;
            float e = (bx - ecx) * (bx - ecx) / (erx * erx) + (by - ecy) * (by - ecy) / (ery * ery);
            if (e <= 1f) set(x, y, white);
            float e2 = (bx - 7.4f) * (bx - 7.4f) / (1.05f * 1.05f) + (by - 12f + bob) * (by - 12f + bob) / (1.15f * 1.15f);
            if (e2 <= 1f) set(x, y, white);
            float pr = (bx - (ecx - 0.5f)) * (bx - (ecx - 0.5f)) / 0.64f + (by - (ecy + 0.2f)) * (by - (ecy + 0.2f)) / 0.7225f;
            if (pr <= 1f) set(x, y, ink);
            float pr2 = (bx - 7.1f) * (bx - 7.1f) / 0.3025f + (by - 12.2f + bob) * (by - 12.2f + bob) / 0.36f;
            if (pr2 <= 1f) set(x, y, ink);
        }
        set(Mathf.Min(W - 1, Mathf.RoundToInt(ecx) + 1), Mathf.Clamp(Mathf.RoundToInt(ecy) + 1, 0, H - 1), white);
        int blushY = 10 + (int)bob;
        set(1, Mathf.Clamp(blushY, 0, H - 1), blush);
        set(9, Mathf.Clamp(blushY, 0, H - 1), blush);
        if (king)
        {
            seg(1.6f, 14.6f + bob, 5.4f, 13.8f + bob, ink);
            seg(6.6f, 14.9f + bob, 9.9f, 14.1f + bob, ink);
            seg(1.6f, 14.2f + bob, 5.4f, 13.4f + bob, ink);
            seg(6.6f, 14.5f + bob, 9.9f, 13.7f + bob, ink);
        }

        t.SetPixels32(px);
        return Finish(t, 16);
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
