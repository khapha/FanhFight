using System.Collections.Generic;
using UnityEngine;

// Original arm poses and separate UV slices keep the feet animated during attacks.
public class PlayerVisuals : MonoBehaviour
{
    const int FootPixels = 9;
    Player player;
    PlayerAttack attack;
    PlayerActionPoses poses;
    SpriteRenderer body, shadow, tool, stream, upper, feet, fingers;
    Transform muzzle;
    readonly Dictionary<Sprite, Sprite> upperSlices = new Dictionary<Sprite, Sprite>();
    readonly Dictionary<Sprite, Sprite> footSlices = new Dictionary<Sprite, Sprite>();
    bool wasGrounded;
    bool sprayLatched;
    float splashAt;

    public Vector3 GripPosition { get; private set; }
    public float GripError => tool == null ? 0 : Vector3.Distance(GripPosition,
        tool.transform.TransformPoint(WeaponLoadout.IsSpray ? WeaponArt.BottleGrip : WeaponArt.SlipperGrip));

    void Start()
    {
        player = GetComponent<Player>();
        attack = GetComponent<PlayerAttack>();
        body = GetComponent<SpriteRenderer>();
        poses = Resources.Load<PlayerActionPoses>("PlayerActionPoses");
        shadow = Add("FootShadow", MiniArt.Circle, body.sortingOrder - 1);
        shadow.color = new Color(0.16f, 0.3f, 0.25f, 0.2f);
        shadow.transform.localScale = new Vector3(0.8f, 0.13f, 1);
        feet = Add("AnimatedFeet", null, body.sortingOrder);
        upper = Add("ActionUpperBody", null, body.sortingOrder + 1);
        tool = Add("HeldTool", WeaponLoadout.IsSpray ? WeaponArt.Bottle : MiniArt.Slipper, body.sortingOrder + 3);
        fingers = Add("ToolGrip", WeaponArt.Hand, body.sortingOrder + 4);
        stream = Add("SprayStream", WeaponArt.Stream, body.sortingOrder + 2);
        muzzle = new GameObject("ToolNozzle").transform;
        muzzle.SetParent(tool.transform, false);
        muzzle.localPosition = WeaponArt.BottleNozzle;
        if (attack != null) attack.Muzzle = muzzle;
        wasGrounded = player.grounded;
        if (poses == null) Debug.LogError("Missing PlayerActionPoses resource.");
    }

    SpriteRenderer Add(string name, Sprite sprite, int order)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sharedMaterial = body.sharedMaterial;
        sr.sortingLayerID = body.sortingLayerID;
        sr.sortingOrder = order;
        return sr;
    }

    Sprite Slice(Sprite source, int bottom, int height)
    {
        Rect rect = source.rect;
        return Sprite.Create(source.texture, new Rect(rect.x, rect.y + bottom, rect.width, height),
            Vector2.one * 0.5f, source.pixelsPerUnit, 0, SpriteMeshType.FullRect);
    }

    Vector3 DrawBody(Sprite frame, Sprite reference)
    {
        if (!upperSlices.TryGetValue(frame, out Sprite top))
        {
            top = Slice(frame, FootPixels, (int)frame.rect.height - FootPixels);
            upperSlices.Add(frame, top);
        }
        int footHeight = Mathf.CeilToInt(FootPixels / frame.pixelsPerUnit * reference.pixelsPerUnit);
        if (!footSlices.TryGetValue(reference, out Sprite bottom))
        {
            bottom = Slice(reference, 0, footHeight);
            footSlices.Add(reference, bottom);
        }
        float baseY = reference.bounds.min.y;
        feet.sprite = bottom;
        feet.transform.localPosition = new Vector3((reference.rect.width * 0.5f - reference.pivot.x) / reference.pixelsPerUnit,
            baseY + bottom.rect.height * 0.5f / reference.pixelsPerUnit, 0);
        upper.sprite = top;
        upper.transform.localPosition = new Vector3((frame.rect.width * 0.5f - frame.pivot.x) / frame.pixelsPerUnit,
            baseY + (FootPixels + top.rect.height * 0.5f) / frame.pixelsPerUnit, 0);
        upper.color = feet.color = body.color;
        Vector2 hand = poses.Grip(frame);
        return transform.TransformPoint(new Vector3((hand.x - frame.pivot.x) / frame.pixelsPerUnit,
            baseY + hand.y / frame.pixelsPerUnit, 0));
    }

    static float Ease(float t) { t = Mathf.Clamp01(t); return t * t * (3 - 2 * t); }

    void LateUpdate()
    {
        if (player == null || body == null || poses == null || body.sprite == null) return;
        bool visible = attack != null && body.enabled && player.ourHealth > 0
            && GameSession.State != GameSession.Phase.Respawning;
        bool active = visible && attack.attacking && player.CanAct;
        shadow.enabled = player.grounded && player.ourHealth > 0 && GameSession.State != GameSession.Phase.Respawning;
        shadow.transform.localPosition = new Vector3(0, (body.bounds.min.y - transform.position.y + 0.03f) / transform.lossyScale.y, 0);
        float sx = Mathf.Max(Mathf.Abs(transform.lossyScale.x), 0.001f);
        float sy = Mathf.Max(Mathf.Abs(transform.lossyScale.y), 0.001f);
        float progress = attack != null ? Mathf.Clamp01(attack.Progress) : 0;
        Sprite frame = poses.Frame(WeaponLoadout.IsSpray, active, progress);
        if (WeaponLoadout.IsSpray)
        {
            if (active && progress >= 0.12f) sprayLatched = true;
            if (!active && (!(attack != null && attack.InputHeld && player.CanAct) || attack.Overheated)) sprayLatched = false;
            if (sprayLatched) frame = poses.spray;
        }
        // Let the original Animator draw the full run/jump sprite between attacks.
        bool moving = !player.grounded || player.Dashing
            || (player.r2 != null && Mathf.Abs(player.r2.linearVelocity.x) > 0.1f);
        bool holding = visible && (active || sprayLatched || !moving);
        body.forceRenderingOff = holding;
        upper.enabled = feet.enabled = tool.enabled = fingers.enabled = holding;
        GripPosition = DrawBody(frame, body.sprite);
        float angle, size;
        if (WeaponLoadout.IsSpray)
        {
            angle = active ? Mathf.Sin(Time.time * 28) * 1.2f : -35;
            size = 0.9f;
        }
        else
        {
            angle = !active ? -110 : progress < 0.3f ? Mathf.Lerp(-20, 15, Ease(progress / 0.3f))
                : progress < 0.64f ? Mathf.Lerp(15, -105, Ease((progress - 0.3f) / 0.34f))
                : Mathf.Lerp(-105, -125, Ease((progress - 0.64f) / 0.36f));
            size = 0.55f;
        }
        tool.transform.localScale = new Vector3(size / sx, size / sy, 1);
        tool.transform.localRotation = Quaternion.Euler(0, 0, angle);
        Vector3 handle = WeaponLoadout.IsSpray ? WeaponArt.BottleGrip : WeaponArt.SlipperGrip;
        tool.transform.position = GripPosition - tool.transform.TransformVector(handle);
        fingers.transform.position = GripPosition;
        fingers.transform.localScale = new Vector3(1 / sx, 1 / sy, 1);
        fingers.transform.localRotation = tool.transform.localRotation;
        fingers.color = tool.color = body.color;
        if (attack != null) attack.SyncMuzzle();

        stream.enabled = active && WeaponLoadout.IsSpray && WeaponLoadout.DamageWindow(progress);
        stream.transform.position = muzzle.position + new Vector3((transform.lossyScale.x > 0 ? 1 : -1) * WeaponLoadout.Reach * 0.5f, 0, 0);
        stream.transform.localScale = new Vector3(WeaponLoadout.Reach / stream.sprite.bounds.size.x / sx, 0.85f / sy, 1);
        stream.color = new Color(1, 1, 1, 0.92f + Mathf.Sin(Time.time * 35) * 0.05f);
        if (attack != null) attack.EmitSpray();
        if (player.grounded && !wasGrounded && player.CanAct) Fx.Dust(new Vector3(transform.position.x, body.bounds.min.y, 0));
        wasGrounded = player.grounded;
    }

    void OnDisable()
    {
        if (body != null) body.forceRenderingOff = false;
        foreach (SpriteRenderer part in new[] { upper, feet, tool, fingers, stream }) if (part != null) part.enabled = false;
        if (attack != null && attack.Muzzle == muzzle) attack.Muzzle = null;
    }

    void OnDestroy()
    {
        foreach (Sprite sprite in upperSlices.Values) Destroy(sprite);
        foreach (Sprite sprite in footSlices.Values) Destroy(sprite);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (player == null || !player.CanAct || !other.CompareTag("water") || Time.time < splashAt) return;
        splashAt = Time.time + 0.5f;
        Fx.Burst(transform.position, new Color(0.55f, 0.95f, 1), 12, 2, 0.12f);
    }
}
