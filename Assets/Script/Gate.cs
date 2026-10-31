using UnityEngine;

// Cổng đá chắn trước cửa đích: tự chìm xuống đất khi người chơi đủ điều kiện
// (đủ chìa khóa, màn boss phải hạ Vua Gián). Chạm vào cổng chưa mở thì hiện gợi ý.
public class Gate : MonoBehaviour
{
    bool open;
    bool lifting;
    int level;

    public static void SpawnAt(float x, int level)
    {
        if (!AdventureProps.TryGround(x, out float y)) return;
        GameObject go = new GameObject("QuestGate");
        go.transform.position = new Vector3(x, y, 0);
        Gate gate = go.AddComponent<Gate>();
        gate.level = level;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GateSprite;
        sr.sortingOrder = 6;
        BoxCollider2D solid = go.AddComponent<BoxCollider2D>();
        solid.size = new Vector2(1.4f, 4f);
        solid.offset = new Vector2(0, 2f);
        BoxCollider2D sensor = go.AddComponent<BoxCollider2D>();
        sensor.isTrigger = true;
        sensor.size = new Vector2(2.6f, 5f);
        sensor.offset = new Vector2(0, 2.5f);
    }

    void Start()
    {
        // vào lại màn đã xong nhiệm vụ (chơi lại) thì cổng biến mất luôn
        if (QuestState.ReadyToFinish(level)) Open();
    }

    public void Open()
    {
        if (lifting) return;
        lifting = true;
        StartCoroutine(RiseCo());
    }

    System.Collections.IEnumerator RiseCo()
    {
        open = true;
        Fx.Coin();
        CameraRig.Shake(0.6f);
        Fx.Burst(transform.position + Vector3.up * 2f, new Color(0.75f, 0.73f, 0.85f), 16, 2.5f, 0.25f);
        // cổng trượt xuống dưới đất rồi tự hủy
        for (float k = 0; k < 1f; k += Time.deltaTime * 1.1f)
        {
            transform.position += Vector3.down * Time.deltaTime * 3.2f;
            yield return null;
        }
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || open) return;
        Player player = other.GetComponent<Player>();
        if (player == null || !player.CanAct) return;
        if (QuestState.ReadyToFinish(level))
            Open();
        else
        {
            string hint = QuestState.BlockerHint();
            if (!string.IsNullOrEmpty(hint)) GameHUD.Toast(hint);
        }
    }

    // cổng đá xám có ổ khóa vàng ở giữa
    static Sprite gateSprite;
    public static Sprite GateSprite
    {
        get
        {
            if (gateSprite != null) return gateSprite;
            Texture2D t = new Texture2D(22, 64, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            for (int y = 0; y < 64; y++) for (int x = 0; x < 22; x++)
            {
                bool edge = x < 2 || x > 19 || y > 61;
                Color c = edge ? new Color32(70, 68, 96, 255)
                    : (x / 5 + y / 5) % 2 == 0 ? new Color32(150, 148, 178, 255) : new Color32(126, 124, 156, 255);
                if (y > 56) c = new Color32(96, 94, 124, 255); // đỉnh vòm
                if (x >= 9 && x <= 12 && y >= 30 && y <= 38)
                {
                    bool keyhole = (new Vector2(x - 10.5f, y - 35f)).magnitude < 2.2f
                        || (x >= 10 && x <= 11 && y >= 30 && y <= 33);
                    if (keyhole) c = new Color32(255, 205, 80, 255);
                }
                t.SetPixel(x, y, c);
            }
            t.Apply();
            return gateSprite = Sprite.Create(t, new Rect(0, 0, 22, 64), new Vector2(0.5f, 0f), 16);
        }
    }
}
