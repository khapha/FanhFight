using UnityEngine;

// Original character frames supply the shoulders, arms and hands; feet remain animated.
public class PlayerActionPoses : ScriptableObject
{
    public Sprite spray, raised, strike, lowered;

    public Sprite Frame(bool spraying, bool active, float progress)
    {
        if (!active) return lowered;
        if (spraying) return progress < 0.12f ? strike : spray;
        return progress < 0.3f ? raised : progress < 0.64f ? strike : lowered;
    }

    // Pixel positions in the original sprite rectangle, measured from its bottom-left.
    public Vector2 Grip(Sprite frame)
    {
        if (frame == raised) return new Vector2(41, 40);
        if (frame == strike) return new Vector2(42, 22);
        if (frame == spray) return new Vector2(38, 21);
        return new Vector2(37, 14);
    }
}
