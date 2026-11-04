using UnityEngine;

// A loadout survives level transitions and retries; changing it is a menu action.
public static class WeaponLoadout
{
    public enum Kind { SprayBottle, Slipper }
    const string Key = "adventureWeapon";
    public static Kind Selected => PlayerPrefs.GetInt(Key, 0) == 1 ? Kind.Slipper : Kind.SprayBottle;
    public static bool IsSpray => Selected == Kind.SprayBottle;
    public static string Name => IsSpray ? "Spray Bottle" : "Slipper";
    public static string Action => IsSpray ? "Spray" : "Slap";
    public static float Reach => IsSpray ? 3.6f : 1.65f;
    public static float Duration => IsSpray ? 0.6f : 0.48f;
    public static float Start => IsSpray ? 0.85f : 0.35f;
    public static float Height => IsSpray ? -0.2f : 0.05f;
    public static bool DamageWindow(float progress) => IsSpray
        ? progress >= 0.16f && progress < 1
        : progress >= 0.3f && progress <= 0.72f;
    public static void Select(Kind kind)
    {
        PlayerPrefs.SetInt(Key, kind == Kind.Slipper ? 1 : 0);
        PlayerPrefs.Save();
    }
}
