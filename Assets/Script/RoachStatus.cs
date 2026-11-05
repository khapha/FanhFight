using UnityEngine;

// Shared weapon reactions; AI reads these instead of overwriting an active knockback.
public class RoachStatus : MonoBehaviour
{
    public enum Species { Runner, Armored, Flyer }
    public Species Type { get; set; }
    public bool Telegraphing { get; set; }
    public bool Stunned => stunTime > 0;
    public bool Slowed => slowTime > 0;
    public float SpeedMultiplier => Slowed ? 0.45f : 1f;
    public Vector2 KnockbackVelocity => new Vector2(knockback * Mathf.Clamp01(knockbackTime / 0.22f), 0);
    public Color Tint => flashTime > 0 ? Color.white
        : Stunned ? new Color(1f, 0.86f, 0.45f)
        : Slowed ? new Color(0.5f, 0.95f, 0.8f) : Color.white;

    float slowTime, stunTime, knockback, knockbackTime, flashTime;

    public static int ResolveDamage(int damage, bool spray, Species type)
    {
        if (damage <= 0) return 0;
        return spray && type == Species.Armored ? Mathf.Max(1, Mathf.RoundToInt(damage * 0.45f)) : damage;
    }

    public void ApplyWeaponHit(int damage, bool spray, Vector2 source)
    {
        if (!GameSession.IsPlaying || damage <= 0) return;
        flashTime = 0.08f;
        if (spray)
        {
            slowTime = 1.1f;
        }
        else
        {
            bool perfect = Telegraphing;
            stunTime = perfect ? 0.8f : 0.22f;
            knockbackTime = 0.22f;
            knockback = (transform.position.x >= source.x ? 1f : -1f) * (Type == Species.Armored ? 2.2f : 3.5f);
            Telegraphing = false;
            if (perfect) Fx.Popup(transform.position + Vector3.up, "Perfect slap!", new Color(1f, 0.82f, 0.35f));
        }
        gameObject.SendMessage("Damage", ResolveDamage(damage, spray, Type), SendMessageOptions.DontRequireReceiver);
    }

    void Update()
    {
        if (!GameSession.IsPlaying) return;
        Tick(Time.deltaTime);
    }

    public void Tick(float deltaTime)
    {
        if (!GameSession.IsPlaying || deltaTime <= 0) return;
        slowTime = Mathf.Max(0, slowTime - deltaTime);
        stunTime = Mathf.Max(0, stunTime - deltaTime);
        knockbackTime = Mathf.Max(0, knockbackTime - deltaTime);
        flashTime = Mathf.Max(0, flashTime - deltaTime);
    }
}
