using System.Collections;
using UnityEngine;

// Ground roaches patrol, warn, then commit to a short rush that can be dodged.
[RequireComponent(typeof(Rigidbody2D))]
public class RoachWalker : MonoBehaviour
{
    public float walkSpeed = 1.2f;
    public float aggroSpeed = 2.4f;
    public float aggroRange = 6f;

    Rigidbody2D body;
    SpriteRenderer sr;
    Player player;
    RoachStatus status;
    SpriteRenderer warning;
    enum State { Patrol, Windup, Rush, Recovery }
    State state;
    float stateTime, attackCooldown = 0.8f;
    int health;
    readonly RaycastHit2D[] terrainHits = new RaycastHit2D[16];
    float dir = -1f;
    bool defeated;
    float animTimer, flipCooldown;
    int frame;
    public int Health => health;
    public RoachStatus.Species Species => status.Type;

    public static RoachWalker SpawnAt(float x)
    {
        if (!AdventureProps.TryGround(x, out float y)) return null;
        return Create(new Vector2(x, y + 0.55f));
    }

    public static RoachWalker Create(Vector2 pos)
    {
        GameObject go = new GameObject("RoachWalker");
        go.transform.position = pos;
        go.tag = "Enemy";
        RoachWalker w = go.AddComponent<RoachWalker>();
        return w;
    }

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.freezeRotation = true;
        status = GetComponent<RoachStatus>();
        if (status == null) status = gameObject.AddComponent<RoachStatus>();
        status.Type = Mathf.FloorToInt(Mathf.Abs(transform.position.x) / 10f) % 3 == 1
            ? RoachStatus.Species.Armored : RoachStatus.Species.Runner;
        health = status.Type == RoachStatus.Species.Armored ? 60 : 24;
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = CuteArt.RoachVariant(0, (int)status.Type);
        sr.sortingOrder = 9;
        transform.localScale = Vector3.one * 1.15f;

        BoxCollider2D solid = gameObject.AddComponent<BoxCollider2D>();
        // sprite cao ~1.125 unit (scale 1.15): đặt collider xuống dưới tâm để chân chạm đất
        solid.size = new Vector2(0.95f, 0.55f);
        solid.offset = new Vector2(0, -0.29f);
        BoxCollider2D sensor = gameObject.AddComponent<BoxCollider2D>();
        sensor.isTrigger = true;
        // sensor chồng-trúng vươn lên tầm người (vùng dép vung ở y~0.7): nhát chém
        // trúng cả thân trên của gián bộ hành, không chỉ chân
        sensor.size = new Vector2(1.15f, 1.15f);
        sensor.offset = new Vector2(0, 0.25f);
        GameObject sign = new GameObject("AttackWarning");
        sign.transform.SetParent(transform, false);
        sign.transform.localPosition = new Vector3(0, 1.05f, 0);
        sign.transform.localScale = Vector3.one * 0.65f;
        warning = sign.AddComponent<SpriteRenderer>();
        warning.sprite = CuteArt.Alert;
        warning.sortingOrder = 12;
        warning.enabled = false;
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.GetComponent<Player>();
    }

    // tia chiếu bỏ qua trigger và vật có rigidbody (chỉ xét địa hình)
    bool TerrainAhead(Vector2 origin, Vector2 direction, float distance)
    {
        int count = Physics2D.Raycast(origin, direction, new ContactFilter2D { useTriggers = false }, terrainHits, distance);
        for (int i = 0; i < count; i++)
        {
            RaycastHit2D h = terrainHits[i];
            if (h.collider == null || h.collider.attachedRigidbody == body) continue;
            if (h.collider.isTrigger || h.collider.attachedRigidbody != null) continue;
            return true;
        }
        return false;
    }

    void FixedUpdate()
    {
        if (defeated) return;
        if (!GameSession.IsPlaying || player == null || !player.CanAct)
        {
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            warning.enabled = false;
            return;
        }
        if (status.Stunned)
        {
            Enter(State.Recovery, 0.65f);
            body.linearVelocity = new Vector2(status.KnockbackVelocity.x, body.linearVelocity.y);
            sr.color = status.Tint;
            warning.sprite = CuteArt.Sparkle;
            warning.color = new Color(1, 0.85f, 0.3f);
            warning.enabled = true;
            return;
        }
        float dt = Time.fixedDeltaTime;
        attackCooldown = Mathf.Max(0, attackCooldown - dt);
        stateTime -= dt;
        bool armored = status.Type == RoachStatus.Species.Armored;
        float dx = player.transform.position.x - transform.position.x;
        float dy = Mathf.Abs(player.transform.position.y - transform.position.y);
        if (state == State.Patrol && attackCooldown <= 0 && Mathf.Abs(dx) < 4.5f && dy < 1.6f)
        {
            if (Mathf.Abs(dx) > 0.15f) dir = Mathf.Sign(dx);
            Enter(State.Windup, armored ? 0.7f : 0.45f);
        }
        else if (state == State.Windup && stateTime <= 0) Enter(State.Rush, armored ? 0.5f : 0.42f);
        else if (state == State.Rush && stateTime <= 0) Enter(State.Recovery, armored ? 1f : 0.75f);
        else if (state == State.Recovery && stateTime <= 0)
        {
            Enter(State.Patrol, 0);
            attackCooldown = 0.8f;
        }
        float speed = state == State.Rush ? (armored ? 3.5f : 5.4f)
            : state == State.Patrol ? walkSpeed * (armored ? 0.65f : 1.15f) : 0;
        if (state == State.Patrol && Mathf.Abs(dx) < aggroRange && Mathf.Abs(dx) > 0.3f)
            dir = Mathf.Sign(dx);
        speed *= status.SpeedMultiplier;
        Vector2 origin = body.position;
        bool wall = TerrainAhead(origin + Vector2.up * 0.3f, new Vector2(dir, 0), 0.7f);
        bool hasGround = TerrainAhead(origin + new Vector2(dir * 0.65f, 0.1f), Vector2.down, 0.9f);
        if (wall || !hasGround)
        {
            speed = 0;
            if (state == State.Rush) Enter(State.Recovery, 0.8f);
            if (flipCooldown <= 0)
            {
                dir = -dir;
                flipCooldown = 0.3f;
            }
        }
        if (flipCooldown > 0) flipCooldown -= dt;

        body.linearVelocity = new Vector2(dir * speed, body.linearVelocity.y);

        // đổi khung chân theo nhịp bước
        animTimer += dt * Mathf.Max(speed, 0.3f) * 5f;
        if (animTimer >= 1f)
        {
            animTimer = 0;
            frame = 1 - frame;
            sr.sprite = CuteArt.RoachVariant(frame, (int)status.Type);
        }
        sr.flipX = dir > 0;
        sr.color = status.Tint;
        warning.enabled = status.Telegraphing;
        warning.sprite = CuteArt.Alert;
        warning.color = new Color(1f, 0.7f + Mathf.Sin(Time.time * 25) * 0.2f, 0.35f);
    }

    void Enter(State next, float duration)
    {
        state = next;
        stateTime = duration;
        status.Telegraphing = next == State.Windup;
        warning.enabled = status.Telegraphing;
    }

    void OnTriggerEnter2D(Collider2D collision) => TouchPlayer(collision);
    void OnTriggerStay2D(Collider2D collision) => TouchPlayer(collision);

    void TouchPlayer(Collider2D collision)
    {
        if (defeated || !GameSession.IsPlaying || status.Stunned || state != State.Rush) return;
        if (collision.isTrigger) return;
        if (collision.CompareTag("Player"))
        {
            Player touched = collision.GetComponent<Player>();
            if (touched != null && touched.CanTakeDamage)
            {
                collision.SendMessageUpwards("Damage", 1);
                touched.KnockBack(250f, transform.position);
                Enter(State.Recovery, 0.85f);
            }
        }
    }

    void Damage(int damage)
    {
        if (defeated || !GameSession.IsPlaying || damage <= 0) return;
        health = Mathf.Max(0, health - damage);
        if (health <= 0) Defeat();
    }

    void Defeat()
    {
        if (defeated || !GameSession.IsPlaying) return;
        defeated = true;
        status.Telegraphing = false;
        warning.enabled = false;
        foreach (Collider2D collider in GetComponents<Collider2D>()) collider.enabled = false;
        QuestState.RunSquished++;
        Diary.Rescue(); // mỗi con gián được cứu là một chú bướm mới vào nhật ký
        Fx.HitStop();
        Fx.Popup(transform.position, "+5", new Color(1f, 0.6f, 0.3f));
        GameMaster gm = GameObject.FindGameObjectWithTag("gamemaster")?.GetComponent<GameMaster>();
        if (gm != null) gm.points += 5;
        Fx.Burst(transform.position + Vector3.up * 0.3f, CuteTheme.Pink, 14, 2.5f, 0.22f);
        StartCoroutine(SquashCo());
    }

    IEnumerator SquashCo()
    {
        body.linearVelocity = Vector2.zero;
        Vector3 s = transform.localScale;
        transform.localScale = new Vector3(s.x * 1.35f, s.y * 0.3f, 1);
        yield return new WaitForSeconds(0.3f);
        Destroy(gameObject);
    }
}
