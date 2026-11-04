using UnityEngine;

// Side-facing bottle: large white label, mint body, pink trigger and a clear nozzle.
public static class WeaponArt
{
    static Sprite bottle, stream, hand;
    public static Sprite Bottle => bottle != null ? bottle : bottle = MakeBottle();
    public static Sprite Stream => stream != null ? stream : stream = MakeStream();
    public static Sprite Hand => hand != null ? hand : hand = MakeHand();
    public static Vector3 BottleGrip => new Vector3(4.5f / 24, 5.5f / 24, 0);
    public static Vector3 BottleNozzle => new Vector3(8f / 24, 8.5f / 24, 0);
    public static Vector3 SlipperGrip => new Vector3(0, -0.9f, 0);

    static Sprite MakeHand()
    {
        string[] rows = { "..KKK..", ".KSSSK.", "KSSSSSK", "KSSLSSK", ".KSSSK.", "..KKK.." };
        Texture2D texture = new Texture2D(7, 6, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
        for (int y = 0; y < 6; y++) for (int x = 0; x < 7; x++)
        {
            char c = rows[y][x];
            texture.SetPixel(x, 5 - y, c == 'K' ? new Color32(149, 74, 61, 255)
                : c == 'S' ? new Color32(255, 197, 167, 255)
                : c == 'L' ? new Color32(255, 226, 200, 255) : Color.clear);
        }
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 7, 6), Vector2.one * 0.5f, 24);
    }

    static Sprite MakeBottle()
    {
        string[] rows = {
            ".....KKKKKKKKK..", ".....KPPPPPPPK..", ".....KPPPPPPPKKK", ".....KKKKKPPPKWK",
            ".....KLLKKKKKKKK", ".....KLLK..KPK..", "....KLLLLK.KPK..", "...KLLLLLLKKKK..",
            "..KLLLLLLLLK....", "..KLWLLLLLLK....", "..KLWLLLLLLK....", "..KLWLLLLLLK....",
            "..KWWWWWWWWK....", "..KWWLLLWWWK....", "..KWWLWLWWWK....", "..KWWLLLWWWK....",
            "..KWWWWWWWWK....", "..KLLLLLLLLK....", "..KLWLLLLLLK....", "..KLWLLLLLLK....",
            "..KLLLLLLLLK....", "..KLLLLLLLLK....", "...KKKKKKKK.....", "................"
        };
        Texture2D texture = new Texture2D(16, rows.Length, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
        for (int y = 0; y < rows.Length; y++) for (int x = 0; x < 16; x++)
        {
            char c = rows[y][x];
            texture.SetPixel(x, rows.Length - y - 1, c == 'K' ? CuteTheme.Ink
                : c == 'P' ? CuteTheme.Pink : c == 'L' ? new Color32(88, 211, 174, 255)
                : c == 'W' ? Color.white : Color.clear);
        }
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 16, rows.Length), Vector2.one * 0.5f, 24);
    }

    static Sprite MakeStream()
    {
        Texture2D texture = new Texture2D(96, 24, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
        for (int y = 0; y < 24; y++) for (int x = 0; x < 96; x++)
        {
            float spread = 2 + x / 10f;
            float distance = Mathf.Abs(y - 11.5f);
            bool inside = distance <= spread && x < 94 - Mathf.Abs(y - 11.5f) * 0.35f;
            Color color = distance > spread - 1 ? new Color(0.12f, 0.65f, 0.65f, 0.72f)
                : distance < spread * 0.3f ? new Color(0.98f, 1, 1, 0.9f)
                : new Color(0.3f, 0.95f, 0.78f, 0.65f);
            if ((x + y * 3) % 17 == 0) color = Color.white;
            texture.SetPixel(x, y, inside ? color : Color.clear);
        }
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 96, 24), Vector2.one * 0.5f, 24);
    }
}
