using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// VUA GIÁN - boss cuối Rocky Cave. To gấp 3 lần nhưng là "big scaredy-cat":
// đi lạch bạch đuổi player, thi thoảng lao tới (có báo trước bằng dấu chấm than),
// thi thoảng hét gọi đệ tử. Trúng 6 phát dép thì đầu hàng: nảy lên như bóng,
// biến thành bầy bướm, cổng đá tự mở. Hoàn toàn không đáng sợ, chỉ lố bịch.
[RequireComponent(typeof(Rigidbody2D))]
public class RoachKing : MonoBehaviour
{
    public float chaseSpeed = 1.5f;
    public float dashSpeed = 6f;

    Rigidbody2D body;
    SpriteRenderer sr;
    SpriteRenderer alert;
    Transform playerT;

    int hp = QuestState.BossMaxHp;
    bool defeated;
    bool dashing;
    float dashTimeLeft, dashDir = 1f;
    float actionTimer = 3.5f, callTimer = 7f, animTimer, faceCooldown;
    int frame;
    float arenaLeft, arenaRight;
    readonly List<RoachWalker> minions = new List<RoachWalker>();

    public static void SpawnAt(float x, float arenaLeft)
    {
        if (!AdventureProps.TryGround(x, out float y)) return;
        GameObject go = new GameObject("RoachKing");
        go.transform.position = new Vector2(x, y + 1.45f);
        go.tag = "Enemy";
        RoachKing king = go.AddComponent<RoachKing>();
        king.arenaLeft = arenaLeft;
        king.arenaRight = arenaLeft + 22f;
    }

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = CuteArt.KingRoach(0);
        sr.sortingOrder = 12;
        transform.localScale = Vector3.one * 2.6f;

        // vương miện vàng - biểu tượng "vua" hề
        GameObject crownGo = new GameObject("Crown");
        crownGo.transform.SetParent(transform, false);
        crownGo.transform.localPosition = new Vector3(-0.15f, 0.42f, 0);
        crownGo.transform.localScale = new Vector3(0.42f, 0.42f, 1);
        SpriteRenderer crownSr = crownGo.AddComponent<SpriteRenderer>();
        crownSr.sprite = CrownSprite;
        crownSr.sortingOrder = 13;

        // dấu chấm than báo động (ẩn, hiện khi sắp lao)
        GameObject alertGo = new GameObject("Alert");
        alertGo.transform.SetParent(transform, false);
        alertGo.transform.localPosition = new Vector3(0f, 0.95f, 0);
        alertGo.transform.localScale = new Vector3(0.35f, 0.35f, 1);
        alert = alertGo.AddComponent<SpriteRenderer>();
        alert.sprite = CuteArt.Alert;
        alert.sortingOrder = 14;
        alert.gameObject.SetActive(false);

        BoxCollider2D solid = gameObject.AddComponent<BoxCollider2D>();
        // sprite boss cao ~2.9 unit (scale 2.6), collider đặt dưới tâm để chân chạm đất
        solid.size = new Vector2(0.95f, 0.9f);
        solid.offset = new Vector2(0, -0.1f);
        BoxCollider2D sensor = gameObject.AddComponent<BoxCollider2D>();
        // sensor chồng-trúng: phủ từ chân lên tầm ngực/đầu (vùng dép vung ở y~0.7)
        // để nhát chém trúng boss; vẫn là trigger nên không đẩy player
        sensor.isTrigger = true;
        sensor.size = new Vector2(1.2f, 1.05f);
        sensor.offset = Vector2.zero;
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerT = p.transform;
    }

    void Update()
    {
        if (defeated || playerT == null) return;
        if (!GameSession.IsPlaying)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        float dt = Time.deltaTime;
        float dx = playerT.position.x - transform.position.x;

        // luôn quay mặt về phía player (coi như quyền uy của vua)
        if (Mathf.Abs(dx) > 0.3f && faceCooldown <= 0)
        {
            sr.flipX = dx > 0;
            faceCooldown = 0.25f;
        }
        if (faceCooldown > 0) faceCooldown -= dt;

        if (dashing)
        {
            dashTimeLeft -= dt;
            body.linearVelocity = new Vector2(dashDir * dashSpeed, 0f);
            if (dashTimeLeft <= 0)
            {
                dashing = false;
                body.linearVelocity = Vector2.zero;
            }
            return;
        }

        // đi lạch bạch đuổi theo
        body.linearVelocity = new Vector2(Mathf.Sign(dx) * chaseSpeed, body.linearVelocity.y);

        actionTimer -= dt;
        if (actionTimer <= 0)
        {
            StartCoroutine(TelegraphThenDash());
            actionTimer = Random.Range(3.5f, 5.5f);
        }

        callTimer -= dt;
        if (callTimer <= 0)
        {
            CallMinions();
            callTimer = Random.Range(7f, 10f);
        }

        animTimer += dt * 4f;
        if (animTimer >= 1f)
        {
            animTimer = 0;
            frame = 1 - frame;
            sr.sprite = CuteArt.KingRoach(frame);
        }

        // nấp trong sân khấu, không truy đuổi ra ngoài
        Vector2 pos = body.position;
        float clampedX = Mathf.Clamp(pos.x, arenaLeft, arenaRight);
        if (clampedX != pos.x) body.position = new Vector2(clampedX, pos.y);
    }

    // báo động 0.7 giây (dấu chấm than + rung nhẹ) rồi lao tới - luôn có thời gian né
    IEnumerator TelegraphThenDash()
    {
        alert.gameObject.SetActive(true);
        float t = 0.7f;
        Vector3 baseLocal = alert.transform.localPosition;
        while (t > 0)
        {
            t -= Time.deltaTime;
            alert.transform.localPosition = baseLocal + new Vector3(Random.Range(-0.04f, 0.04f), 0, 0);
            yield return null;
        }
        alert.transform.localPosition = baseLocal;
        alert.gameObject.SetActive(false);
        if (defeated || playerT == null || !GameSession.IsPlaying) yield break;
        dashDir = Mathf.Sign(playerT.position.x - transform.position.x);
        if (Mathf.Abs(playerT.position.x - transform.position.x) < 1f) dashDir = sr.flipX ? 1 : -1;
        dashing = true;
        dashTimeLeft = 0.85f;
        Fx.Hit();
    }

    void CallMinions()
    {
        minions.RemoveAll(m => m == null);
        if (minions.Count >= 3) return;
        for (int i = 0; i < 2 && minions.Count < 3; i++)
        {
            float x = transform.position.x + (i == 0 ? -3f : 3f);
            RoachWalker minion = RoachWalker.SpawnAt(x);
            if (minion != null) minions.Add(minion);
        }
        Fx.Popup(transform.position + Vector3.up * 1.6f, "Guys, help!", CuteTheme.Pink);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (defeated || !GameSession.IsPlaying) return;
        if (collision.isTrigger || !collision.CompareTag("Player")) return;
        Player player = collision.GetComponent<Player>();
        if (player != null && player.CanTakeDamage)
        {
            collision.SendMessageUpwards("Damage", 1);
            player.KnockBack(450f, transform.position);
        }
    }

    void Damage(int damage)
    {
        if (defeated || damage <= 0 || !GameSession.IsPlaying) return;
        hp--;
        QuestState.BossHp = Mathf.Max(hp, 0);
        Fx.HitStop();
        CameraRig.Shake(1.4f);
        Fx.Burst(transform.position + Vector3.up * 0.6f, new Color(1f, 0.8f, 0.4f), 12, 3f, 0.25f);
        if (hp <= 0) { StartCoroutine(SurrenderCo()); return; }
        // nhảy bật lên một cái như bị giật mình
        body.linearVelocity = new Vector2(0f, 5f);
        GameHUD.Toast("Roach King: " + hp + " left!");
    }

    // đầu hàng đầy sân khấu: nảy như bóng, buông vương miện, hóa bầy bướm
    IEnumerator SurrenderCo()
    {
        defeated = true;
        QuestState.RunSquished++;
        Diary.Rescue("the Roach King"); // cả nhà bướm đón vua về
        body.linearVelocity = Vector2.zero;
        GameMaster gm = GameObject.FindGameObjectWithTag("gamemaster")?.GetComponent<GameMaster>();
        if (gm != null) gm.points += 50;
        alert.gameObject.SetActive(false);
        Fx.Popup(transform.position + Vector3.up * 1.8f, "I give up!", CuteTheme.Mint);
        Fx.Explode();
        Fx.WinJingle(false); // kèn chiến thắng nho nhỏ khi vua đầu hàng

        Vector3 baseScale = transform.localScale;
        float t = 0;
        while (t < 1.1f)
        {
            t += Time.deltaTime;
            // nảy vài cái rồi lấm tấm thu nhỏ lại
            float bounce = Mathf.Abs(Mathf.Sin(t * 12f)) * Mathf.Max(0f, 1f - t * 0.9f);
            transform.localScale = baseScale * (1f + bounce * 0.25f - t * 0.3f);
            if (t > 0.35f && t - Time.deltaTime <= 0.35f)
            {
                // văng vương miện lên trời
                GameObject crown = transform.Find("Crown")?.gameObject;
                if (crown != null)
                {
                    crown.transform.SetParent(null);
                    Rigidbody2D rb = crown.AddComponent<Rigidbody2D>();
                    rb.linearVelocity = new Vector2(Random.Range(-2f, 2f), 8f);
                    rb.gravityScale = 3f;
                    crown.transform.localScale = Vector3.one * 0.4f;
                    Destroy(crown, 2f);
                }
            }
            yield return null;
        }

        // bầy bướm bay ra - kết có hoa có bướm, không gì u ám
        Fx.Burst(transform.position, new Color(1f, 0.75f, 0.85f), 30, 4f, 0.35f);
        for (int i = 0; i < 8; i++)
        {
            GameObject b = new GameObject("VictoryButterfly");
            b.transform.position = transform.position + new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(0f, 1.5f), 0);
            SpriteRenderer bsr = b.AddComponent<SpriteRenderer>();
            bsr.sprite = CuteArt.Butterfly;
            bsr.sortingOrder = 11;
            b.AddComponent<KingButterfly>();
        }
        QuestState.DefeatBoss();
        FindAnyObjectByType<Gate>()?.Open();
        Destroy(gameObject);
    }

    // bướm chiến thắng bay loạn xạ rồi tự biến mất
    class KingButterfly : MonoBehaviour
    {
        Vector2 vel;
        float life = 2.2f;
        void Start()
        {
            vel = new Vector2(Random.Range(-2f, 2f), Random.Range(1.5f, 3.5f));
        }
        void Update()
        {
            life -= Time.deltaTime;
            if (life <= 0) { Destroy(gameObject); return; }
            vel += new Vector2(Mathf.Sin(Time.time * 7f) * 0.1f, 0.5f * Time.deltaTime);
            transform.position += (Vector3)(vel * Time.deltaTime);
            transform.localScale = Vector3.one * Mathf.Clamp01(life / 1.2f);
        }
    }

    // vương miện pixel 5 đỉnh
    static Sprite crownSprite;
    public static Sprite CrownSprite
    {
        get
        {
            if (crownSprite != null) return crownSprite;
            Texture2D t = new Texture2D(14, 8, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            for (int y = 0; y < 8; y++) for (int x = 0; x < 14; x++)
            {
                Color c = Color.clear;
                bool band = y <= 2;
                bool spike = y > 2 && ((x >= 0 && x <= 2 && x >= y - 3 && x <= 2 - (y - 3))
                    || (x >= 5 && x <= 8 && x >= 4 + (y - 3) && x <= 9 - (y - 3))
                    || (x >= 11 && x <= 13 && x >= 11 + (y - 3)));
                if (band || spike) c = y == 0 ? new Color32(255, 232, 150, 255) : new Color32(255, 205, 80, 255);
                if ((x == 3 || x == 10) && y == 1) c = new Color32(255, 205, 80, 255);
                t.SetPixel(x, y, c);
            }
            t.Apply();
            return crownSprite = Sprite.Create(t, new Rect(0, 0, 14, 8), new Vector2(0.5f, 0.5f), 16);
        }
    }
}
