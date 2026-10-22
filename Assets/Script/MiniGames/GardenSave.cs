using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

// Persistent flower data is independent of the scale used by the grow animation.
public static class GardenSave
{
    public struct Flower
    {
        public Vector2 Position;
        public int Color;
        public float Size, Rotation;

        public Flower(Vector2 position, int color, float size, float rotation)
        {
            Position = position;
            Color = color;
            Size = size;
            Rotation = rotation;
        }
    }

    public static string Write(IEnumerable<Flower> flowers)
    {
        StringBuilder save = new StringBuilder();
        foreach (Flower flower in flowers)
            save.Append(string.Format(CultureInfo.InvariantCulture, "{0:F2},{1:F2},{2},{3:F2},{4:F0};",
                flower.Position.x, flower.Position.y, flower.Color, flower.Size, flower.Rotation));
        return save.ToString();
    }

    public static List<Flower> Read(string saved, int limit, int colorCount)
    {
        List<Flower> flowers = new List<Flower>();
        foreach (string item in (saved ?? "").Split(';'))
        {
            if (flowers.Count >= limit) break;
            string[] values = item.Split(',');
            if (values.Length != 5 || colorCount < 1) continue;
            if (!Number(values[0], out float x) || !Number(values[1], out float y)
                || !int.TryParse(values[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int color)
                || !Number(values[3], out float size) || !Number(values[4], out float rotation)) continue;
            // Recover flowers saved at scale zero by older versions.
            flowers.Add(new Flower(new Vector2(x, y), Mathf.Clamp(color, 0, colorCount - 1),
                size <= 0 ? 1.7f : Mathf.Clamp(size, 0.5f, 3f), rotation));
        }
        return flowers;
    }

    static bool Number(string value, out float number)
    {
        return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out number)
            && !float.IsNaN(number) && !float.IsInfinity(number);
    }
}
