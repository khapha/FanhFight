using UnityEngine;

// Kho báu bí mật: chùm tinh thể lấp lánh nửa ẩn trong nền, chạm vào được thưởng
// điểm + ghi nhận bí mật cho màn. Sinh ở vị trí khó tiếp cận, chỉ đếm 1 lần.
public class SecretSparkle : MonoBehaviour
{
    bool taken;

    public static void SpawnAt(float x, int level)
    {
        // thử chỗ chính, nếu là hố/nước thì dịch vài bước
        for (int i = 0; i < 5; i++)
        {
            float tryX = x + new[] { 0f, 6f, -6f, 12f, -12f }[i];
            if (AdventureProps.TryGround(tryX, out float y))
            {
                Create(tryX, y, level);
                return;
            }
        }
    }

    static void Create(float x, float groundY, int level)
    {
        GameObject go = new GameObject("SecretTreasure");
        go.transform.position = new Vector3(x, groundY + 1.0f, 0);
        SecretSparkle secret = go.AddComponent<SecretSparkle>();
        secret.level = level;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CuteArt.Crystal;
        sr.sortingOrder = 3;
        sr.color = new Color(1f, 0.95f, 0.6f, 0.85f); // ánh vàng nhẹ, khá ẩn
        BoxCollider2D box = go.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2(1.4f, 1.6f);
        go.AddComponent<Shimmer>();
    }

    int level;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (taken || !other.CompareTag("Player")) return;
        Player player = other.GetComponent<Player>();
        if (player == null || !player.CanAct) return;
        taken = true;
        QuestState.FindSecret();
        GameMaster gm = GameObject.FindGameObjectWithTag("gamemaster")?.GetComponent<GameMaster>();
        if (gm != null) gm.points += 10;
        Fx.Coin();
        Fx.Burst(transform.position, new Color(1f, 0.95f, 0.5f), 22, 3f, 0.22f);
        Fx.Popup(transform.position, "+10 Secret!", new Color(1f, 0.95f, 0.5f));
        GameHUD.Toast("Secret found!");
        Destroy(gameObject);
    }

    class Shimmer : MonoBehaviour
    {
        SpriteRenderer sr;
        void Start() { sr = GetComponent<SpriteRenderer>(); }
        void Update()
        {
            if (sr == null) return;
            float a = 0.55f + Mathf.Sin(Time.time * 2.2f) * 0.25f;
            sr.color = new Color(1f, 0.95f, 0.6f, a);
        }
    }
}
