using UnityEngine;

// Chìa khóa vàng đung đưa, nhặt được thì đếm vào QuestState.
// Lơ lửng nhẹ nhàng để nhìn thấy từ xa - luôn là tin vui, không bao giờ hại người chơi.
public class KeyPickup : MonoBehaviour
{
    bool taken;

    public static void SpawnAt(float x)
    {
        if (!AdventureProps.TryGround(x, out float y)) return;
        GameObject go = new GameObject("GoldenKey");
        go.transform.position = new Vector3(x, y + 1.4f, 0);
        KeyPickup key = go.AddComponent<KeyPickup>();
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = KeySprite;
        sr.sortingOrder = 8;
        BoxCollider2D box = go.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2(1.2f, 1.2f);
        go.AddComponent<KeyBob>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryCollect(other);
    }

    // nếu player spawn/teleport đã nằm đè sẵn thì trigger enter không fire - stay sẽ gom
    void OnTriggerStay2D(Collider2D other)
    {
        TryCollect(other);
    }

    void TryCollect(Collider2D other)
    {
        if (taken || !other.CompareTag("Player")) return;
        Player player = other.GetComponent<Player>();
        if (player == null || !player.CanAct) return;
        taken = true;
        QuestState.CollectKey();
        Fx.Coin();
        Fx.Burst(transform.position, new Color(1f, 0.85f, 0.3f), 18, 3f, 0.2f);
        Fx.Popup(transform.position, "Key!", new Color(1f, 0.85f, 0.3f));
        GameHUD.Toast("Key " + QuestState.KeysCollected + "/" + QuestState.KeysRequired);
        Destroy(gameObject);
    }

    // vẽ chìa khóa pixel: vành tròn + đuôi răng
    static Sprite keySprite;
    public static Sprite KeySprite
    {
        get
        {
            if (keySprite != null) return keySprite;
            Texture2D t = new Texture2D(12, 12, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            for (int y = 0; y < 12; y++) for (int x = 0; x < 12; x++)
            {
                Color c = Color.clear;
                float ring = (new Vector2(x - 4, y - 8)).magnitude;
                if (ring < 3.4f && ring > 1.6f) c = new Color32(255, 205, 80, 255);   // vành tròn
                if (x >= 5 && x <= 10 && y >= 2 && y <= 4) c = new Color32(255, 205, 80, 255); // đuôi
                if ((x == 8 || x == 10) && y == 4) c = new Color32(255, 205, 80, 255);         // răng
                if (c.a > 0 && y >= 2 && y <= 4 && x >= 5 && x <= 10 && (x + y) % 3 == 0)
                    c = new Color32(255, 232, 150, 255); // lấp lánh
                t.SetPixel(x, y, c);
            }
            t.Apply();
            return keySprite = Sprite.Create(t, new Rect(0, 0, 12, 12), new Vector2(0.5f, 0.5f), 16);
        }
    }

    // lắc lư nhẹ
    class KeyBob : MonoBehaviour
    {
        Vector3 basePos;
        void Start() { basePos = transform.position; }
        void Update() { transform.position = basePos + new Vector3(0, Mathf.Sin(Time.time * 3f) * 0.15f, 0); }
    }
}
