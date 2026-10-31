using System.Collections;
using UnityEngine;

// Gián bộ hành tuần tra: đi qua đi lại trên platform, quay đầu ở mép đất hoặc
// trước tường. Thấy player gần thì bò tới nhanh hơn một chút (vẫn chậm chode).
// Nhìn ngốc nghếch, bị đập là bẹp thành làn khói - không gì đáng sợ.
[RequireComponent(typeof(Rigidbody2D))]
public class RoachWalker : MonoBehaviour
{
    public float walkSpeed = 1.2f;
    public float aggroSpeed = 2.4f;
    public float aggroRange = 6f;

    Rigidbody2D body;
    SpriteRenderer sr;
    Transform playerT;
    float dir = -1f;
    bool defeated;
    float animTimer, flipCooldown;
    int frame;

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
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = CuteArt.Roach(0);
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
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerT = p.transform;
    }

    // tia chiếu bỏ qua trigger và vật có rigidbody (chỉ xét địa hình)
    bool TerrainAhead(Vector2 origin, Vector2 direction, float distance, bool wantHit)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, distance);
        foreach (RaycastHit2D h in hits)
        {
            if (h.collider == null || h.collider.attachedRigidbody == body) continue;
            if (h.collider.isTrigger || h.collider.attachedRigidbody != null) continue;
            return wantHit; // gặp địa hình
        }
        return !wantHit;
    }

    void FixedUpdate()
    {
        if (defeated) return;
        if (!GameSession.IsPlaying)
        {
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            return;
        }
        float speed = walkSpeed;
        if (playerT != null)
        {
            float dx = playerT.position.x - transform.position.x;
            if (Mathf.Abs(dx) < aggroRange)
            {
                speed = aggroSpeed;
                if (Mathf.Abs(dx) > 0.3f) dir = Mathf.Sign(dx);
            }
        }

        // quẹt râu kiểm tra phía trước: tường thì quay đầu, hết đất thì dừng lại nhìn ngơ
        Vector2 origin = body.position;
        bool wall = TerrainAhead(origin + Vector2.up * 0.3f, new Vector2(dir, 0), 0.7f, true);
        bool hasGround = TerrainAhead(origin + new Vector2(dir * 0.55f, 0.1f), Vector2.down, 0.8f, false);
        if (wall || !hasGround)
        {
            if (flipCooldown <= 0)
            {
                dir = -dir;
                flipCooldown = 0.3f;
            }
        }
        if (flipCooldown > 0) flipCooldown -= Time.fixedDeltaTime;

        body.linearVelocity = new Vector2(dir * speed, body.linearVelocity.y);

        // đổi khung chân theo nhịp bước
        animTimer += Time.fixedDeltaTime * speed * 5f;
        if (animTimer >= 1f)
        {
            animTimer = 0;
            frame = 1 - frame;
            sr.sprite = CuteArt.Roach(frame);
        }
        sr.flipX = dir > 0;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (defeated || !GameSession.IsPlaying) return;
        if (collision.isTrigger) return;
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            if (player != null && player.CanTakeDamage)
            {
                collision.SendMessageUpwards("Damage", 1);
                player.KnockBack(300f, transform.position);
            }
        }
    }

    void Damage(int damage)
    {
        if (damage > 0) Defeat();
    }

    void Defeat()
    {
        if (defeated || !GameSession.IsPlaying) return;
        defeated = true;
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
