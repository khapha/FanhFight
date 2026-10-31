using System.Collections;
using UnityEngine;

// GIÁN VŨ CÔNG — boss phụ màn Sunset Meadow. Không đánh nhau: chú ấy TỰ TIN
// nhảy múa giữa sân khấu hoa, và cách "hạ" là chạm vào chú 4 lần (mỗi lần chú
// xoay tít rồi CÚI CHÀO màn xử, khán giả bướm vỗ tay). Vui nhộn, không đáng sợ.
[RequireComponent(typeof(Rigidbody2D))]
public class RoachDancer : MonoBehaviour
{
    public static void SpawnAt(float x, float stageLeft)
    {
        if (!AdventureProps.TryGround(x, out float y)) return;
        GameObject go = new GameObject("RoachDancer");
        go.transform.position = new Vector2(x, y + 0.5f);
        go.tag = "Enemy";
        go.AddComponent<RoachDancer>().stageLeft = stageLeft;
    }

    Rigidbody2D body;
    SpriteRenderer sr;
    int hp = QuestState.MiniBossMaxHp;
    bool done;
    float stageLeft, stageWidth = 12f, moveTimer, faceCooldown;
    float beat = 0.55f; // nhịp disco 110 BPM xấp xỉ
    float beatTimer;
    int danceStep;
    float touchCooldown; // chống trúng nhiều lần khi player đứng chồng lên
    public int Hp => hp; // bot đọc để log tiến độ khiêu vũ

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.freezeRotation = true;
        body.gravityScale = 0f; // collider là trigger nên không có sàn đỡ — tự trôi xuống void!

        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = CuteArt.Roach(0);
        sr.sortingOrder = 12;
        sr.color = new Color(1f, 0.82f, 0.55f); // váy disco vàng-hồng nhẹ nhàng
        transform.localScale = Vector3.one * 2.1f;

        // đĩa mirror-ball nhỏ lơ lửng trên đầu — biểu tượng vũ công
        GameObject ball = new GameObject("MirrorBall");
        ball.transform.SetParent(transform, false);
        ball.transform.localPosition = new Vector3(0f, 1.15f, 0);
        ball.transform.localScale = new Vector3(0.4f, 0.4f, 1);
        SpriteRenderer ballSr = ball.AddComponent<SpriteRenderer>();
        ballSr.sprite = CuteArt.Sparkle;
        ballSr.sortingOrder = 13;
        ballSr.color = new Color(1f, 0.95f, 0.6f);

        BoxCollider2D solid = gameObject.AddComponent<BoxCollider2D>();
        // sprite cao ~2.4 unit (scale 2.1): collider thấp ở chân.
        // LÀ TRIGGER: Vũ Công thân thiện, player chạm/đi xuyên qua = một "bước nhảy"
        // (solid + solid sẽ KHÔNG bao giờ fire OnTriggerEnter2D — bài học AttackTrigger)
        solid.size = new Vector2(1.1f, 0.6f);
        solid.offset = new Vector2(0, -0.85f);
        solid.isTrigger = true;
    }

    void Start()
    {
        Fx.Popup(transform.position + Vector3.up * 2f, "Showtime!", new Color(1f, 0.7f, 0.9f));
    }

    void Update()
    {
        if (done) return;
        if (!GameSession.IsPlaying)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        // nhảy theo nhịp: lắc trái-phải qua lại trên sân khấu, nảy theo beat
        if (touchCooldown > 0f) touchCooldown -= Time.deltaTime;
        beatTimer += Time.deltaTime;
        if (beatTimer >= beat)
        {
            beatTimer = 0f;
            danceStep = 1 - danceStep;
            body.linearVelocity = new Vector2(0f, 2.6f); // nhún theo nhịp
            Fx.Burst(transform.position + Vector3.down * 0.8f, new Color(1f, 0.85f, 0.5f), 3, 1.2f, 0.18f);
        }
        float sway = Mathf.Sin(Time.time * 4.5f) * 1.4f;
        float targetX = Mathf.Clamp(stageLeft + stageWidth * 0.5f + sway, stageLeft + 1f, stageLeft + stageWidth - 1f);
        body.linearVelocity = new Vector2((targetX - transform.position.x) * 2.2f, body.linearVelocity.y);

        // quay mặt về phía player cho "tương tác"
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null && faceCooldown <= 0)
        {
            sr.flipX = p.transform.position.x > transform.position.x;
            faceCooldown = 0.3f;
        }
        if (faceCooldown > 0) faceCooldown -= Time.deltaTime;
    }

    // player chạm vào: thay vì gây damage cho player thì Vũ Công "trúng" 1 lần,
    // xoay tít 360° rồi cúi chào — ai chạm cũng được xem show miễn phí.
    // Dùng cả Stay: bot teleport đè sẵn lên trigger thì Enter không fire (bài học KeyPickup)
    void OnTriggerEnter2D(Collider2D collision) => TryTouch(collision);
    void OnTriggerStay2D(Collider2D collision) => TryTouch(collision);

    void TryTouch(Collider2D collision)
    {
        if (done || touchCooldown > 0f || !GameSession.IsPlaying) return;
        if (collision.isTrigger || !collision.CompareTag("Player")) return;
        Player player = collision.GetComponent<Player>();
        if (player == null) return;
        touchCooldown = 0.8f; // mỗi 0.8s tính một bước nhảy, không dồn hp trong lúc chồng lên nhau
        hp--;
        GameHUD.Toast(hp > 0 ? "The Roach Dancer spins! (" + hp + " left)" : "What a performance!");
        Fx.HitStop();
        Fx.Burst(transform.position + Vector3.up * 1f, new Color(1f, 0.8f, 0.4f), 10, 2.5f, 0.25f);
        if (hp <= 0) { StartCoroutine(BowOutCo()); return; }
        StartCoroutine(SpinCo());
    }

    IEnumerator SpinCo()
    {
        float t = 0;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            // xoay tít một vòng: vũ công chuyên nghiệp
            transform.rotation = Quaternion.Euler(0, 0, t * 720f);
            yield return null;
        }
        transform.rotation = Quaternion.identity;
    }

    IEnumerator BowOutCo()
    {
        done = true;
        QuestState.RunSquished++;
        Diary.Rescue("the Roach Dancer");
        body.linearVelocity = Vector2.zero;
        GameMaster gm = GameObject.FindGameObjectWithTag("gamemaster")?.GetComponent<GameMaster>();
        if (gm != null) gm.points += 40;
        Fx.WinJingle(false);
        Fx.Popup(transform.position + Vector3.up * 1.8f, "*takes a bow*", CuteTheme.Mint);

        // cúi chào thật sâu rồi rực rỡ hóa bướm
        float t = 0;
        while (t < 0.9f)
        {
            t += Time.deltaTime;
            transform.localScale = new Vector3(2.1f, 2.1f * Mathf.Lerp(1f, 0.45f, Mathf.PingPong(t * 2.2f, 1f)), 1);
            yield return null;
        }
        transform.rotation = Quaternion.identity;
        Fx.Burst(transform.position + Vector3.up * 0.5f, CuteTheme.Pink, 22, 3.5f, 0.35f);
        // đám bướm nhỏ bay quanh vũ công như châu báu
        for (int i = 0; i < 6; i++)
        {
            GameObject b = new GameObject("BowButterfly" + i);
            b.AddComponent<PartyButterfly>().Init(i);
            b.transform.position = transform.position + new Vector3(Mathf.Cos(i) * 1.2f, Mathf.Sin(i * 2f), 0);
        }
        QuestState.DefeatMiniBoss();
        yield return new WaitForSeconds(0.4f);
        Destroy(gameObject);
    }
}
