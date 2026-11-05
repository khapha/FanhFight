using UnityEngine;

// Local to one player. Pausing the game does not advance this clock.
public sealed class WeaponHeat
{
    public float Value { get; private set; }
    public bool Overheated { get; private set; }

    public void Tick(float deltaTime, bool firing)
    {
        if (deltaTime <= 0) return;
        Value = Mathf.Clamp(Value + (firing && !Overheated ? 0.30f : -0.38f) * deltaTime, 0, 1);
        if (Value >= 1) Overheated = true;
        else if (Value <= 0.2f) Overheated = false;
    }

    public void Reset() { Value = 0; Overheated = false; }
}
