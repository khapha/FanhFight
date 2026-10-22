using System.Collections;
using UnityEngine;

// Con gián trong mini-game: bò lang thang trong khung màn hình
public class Roach : MonoBehaviour
{
    public float speed = 2f;
    public bool golden;
    public bool Alive { get; private set; } = true;
    public float dose;                 // lượng thuốc đã dính (game Bình xịt)
    public bool calm;                  // thong thả: thỉnh thoảng dừng lại (game Khu vườn)

    SpriteRenderer sr;
    Vector2 velocity;
    float turnTimer, animTimer, life;
    float pauseTimer;
    int frame;
    Rect area;
    bool leaving;

    public static Roach Spawn(Rect area, float speed, bool golden, float life)
    {
        GameObject go = new GameObject("Roach");
        Roach r = go.AddComponent<Roach>();
        r.sr = go.AddComponent<SpriteRenderer>();
        r.sr.sprite = MiniArt.Roach[0];
        r.sr.sortingOrder = 10;
        r.area = area;
        r.speed = speed;
        r.golden = golden;
        r.life = life;
        if (golden) r.sr.color = new Color(1f, 0.85f, 0.3f);
        go.transform.localScale = Vector3.one * 1.3f;

        // xuất hiện từ một cạnh màn hình, bò vào trong
        int edge = Random.Range(0, 4);
        Vector2 p = edge switch
        {
            0 => new Vector2(area.xMin - 0.8f, Random.Range(area.yMin, area.yMax)),
            1 => new Vector2(area.xMax + 0.8f, Random.Range(area.yMin, area.yMax)),
            2 => new Vector2(Random.Range(area.xMin, area.xMax), area.yMin - 0.8f),
            _ => new Vector2(Random.Range(area.xMin, area.xMax), area.yMax + 0.8f),
        };
        go.transform.position = p;
        r.velocity = (area.center + Random.insideUnitCircle * 2f - p).normalized * speed;
        return r;
    }

    void Update()
    {
        if (!Alive) return;
        life -= Time.deltaTime;
        if (life <= 0 && !leaving)
        {
            // hết giờ thì bò ra khỏi màn hình
            leaving = true;
            Vector2 p = transform.position;
            velocity = (p - area.center).normalized * speed * 1.3f;
        }

        if (pauseTimer > 0)
        {
            pauseTimer -= Time.deltaTime;
            return;
        }
        turnTimer -= Time.deltaTime;
        if (turnTimer <= 0 && !leaving)
        {
            turnTimer = Random.Range(0.5f, 1.5f);
            if (calm && Random.value < 0.3f) pauseTimer = Random.Range(0.8f, 2f);
            velocity = Quaternion.Euler(0, 0, Random.Range(-70f, 70f)) * velocity;
        }

        Vector2 pos = (Vector2)transform.position + velocity * Time.deltaTime;
        if (!leaving)
        {
            // dội lại ở mép màn hình (chỉ khi đã vào hẳn bên trong)
            if (area.Contains(transform.position))
            {
                if (pos.x < area.xMin || pos.x > area.xMax) velocity.x = -velocity.x;
                if (pos.y < area.yMin || pos.y > area.yMax) velocity.y = -velocity.y;
            }
        }
        else if (!area.Contains(pos) && (pos - area.center).magnitude > area.size.magnitude * 0.7f)
        {
            Destroy(gameObject);
            return;
        }
        transform.position = pos;

        // quay đầu theo hướng bò, đổi khung chân
        if (velocity.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg - 90f);
        animTimer += Time.deltaTime * speed * 4f;
        if (animTimer >= 1f)
        {
            animTimer = 0;
            frame = 1 - frame;
            sr.sprite = MiniArt.Roach[frame];
        }
    }

    // né nhẹ ra xa một điểm (khi bị xịt)
    public void Flee(Vector2 from)
    {
        if (!Alive) return;
        pauseTimer = 0;
        Vector2 away = ((Vector2)transform.position - from).normalized;
        velocity = Vector2.Lerp(velocity.normalized, away, 0.5f).normalized * speed;
    }

    // bị đập dép: bẹp xuống rồi biến mất thành làn khói
    public void Squash()
    {
        if (!Alive) return;
        Alive = false;
        StartCoroutine(SquashAnim());
    }

    IEnumerator SquashAnim()
    {
        Vector3 s = transform.localScale;
        transform.localScale = new Vector3(s.x * 1.4f, s.y * 0.35f, 1);
        sr.color = sr.color * 0.7f;
        yield return new WaitForSeconds(0.25f);
        Fx.Burst(transform.position, new Color(0.9f, 0.9f, 0.9f), 12, 2f, 0.25f);
        Destroy(gameObject);
    }

    // trúng đủ thuốc: lật ngửa, giãy rồi biến thành hoa
    public void Faint(System.Action<Vector3> onDone)
    {
        if (!Alive) return;
        Alive = false;
        StartCoroutine(FaintAnim(onDone));
    }

    IEnumerator FaintAnim(System.Action<Vector3> onDone)
    {
        sr.color = new Color(0.75f, 0.95f, 0.75f);
        for (float t = 0; t < 0.7f; t += Time.deltaTime)
        {
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 40f) * 25f);
            yield return null;
        }
        onDone?.Invoke(transform.position);
        Destroy(gameObject);
    }

    public void Tint(Color c) { if (Alive) sr.color = c; }
}
