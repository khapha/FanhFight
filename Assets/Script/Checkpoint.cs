using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Cờ checkpoint: chạm vào thì lần chết sau hồi sinh ở đây.
// Cờ được dựng bằng code theo vị trí x của từng màn, không cần sửa scene.
public class Checkpoint : MonoBehaviour
{
    public const int MaxLives = 3;
    public static int Lives = MaxLives;
    public static Vector3 SpawnPoint;

    static readonly Dictionary<string, float[]> Positions = new Dictionary<string, float[]>
    {
        { "Scene 1", new[] { 72f, 128f } },
        { "Scene 2", new[] { 88f, 160f } },
        { "Scene 4", new[] { 90f, 150f } },
    };

    static Sprite poleSprite, flagSprite;
    SpriteRenderer flag;
    bool reached;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            // mỗi lần vào (hoặc chơi lại) một màn: đủ mạng, hồi sinh ở điểm xuất phát
            Lives = MaxLives;
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            SpawnPoint = player.transform.position;
            if (!Positions.TryGetValue(scene.name, out float[] xs)) return;
            foreach (float x in xs) Spawn(x);
        };
    }

    static void Spawn(float x)
    {
        // tìm mặt đất ở vị trí x bằng tia chiếu từ trên xuống
        RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(x, 30f), Vector2.down, 60f);
        float groundY = float.NaN;
        foreach (RaycastHit2D h in hits)
        {
            if (h.collider.isTrigger || h.collider.attachedRigidbody != null) continue;
            groundY = h.point.y;
            break;
        }
        if (float.IsNaN(groundY)) return;

        if (poleSprite == null) MakeSprites();
        GameObject go = new GameObject("Checkpoint");
        go.transform.position = new Vector3(x, groundY, 0);
        SpriteRenderer pole = go.AddComponent<SpriteRenderer>();
        pole.sprite = poleSprite;
        pole.sortingOrder = 5;
        GameObject f = new GameObject("Flag");
        f.transform.SetParent(go.transform, false);
        f.transform.localPosition = new Vector3(0.05f, 1.6f, 0);
        Checkpoint cp = go.AddComponent<Checkpoint>();
        cp.flag = f.AddComponent<SpriteRenderer>();
        cp.flag.sprite = flagSprite;
        cp.flag.color = new Color(0.6f, 0.6f, 0.6f);
        cp.flag.sortingOrder = 5;
        BoxCollider2D bc = go.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(1f, 2.5f);
        bc.offset = new Vector2(0, 1.25f);
    }

    // cột và lá cờ vẽ bằng pixel
    static void MakeSprites()
    {
        Texture2D p = new Texture2D(2, 32) { filterMode = FilterMode.Point };
        for (int y = 0; y < 32; y++) for (int i = 0; i < 2; i++) p.SetPixel(i, y, new Color(0.85f, 0.85f, 0.9f));
        p.Apply();
        poleSprite = Sprite.Create(p, new Rect(0, 0, 2, 32), new Vector2(0.5f, 0), 16);

        Texture2D t = new Texture2D(12, 8) { filterMode = FilterMode.Point };
        for (int x = 0; x < 12; x++)
            for (int y = 0; y < 8; y++)
            {
                // lá cờ tam giác
                bool on = Mathf.Abs(y - 3.5f) <= (12 - x) * 0.33f;
                t.SetPixel(x, y, on ? Color.white : Color.clear);
            }
        t.Apply();
        flagSprite = Sprite.Create(t, new Rect(0, 0, 12, 8), new Vector2(0, 0.5f), 16);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (reached || !other.CompareTag("Player")) return;
        reached = true;
        SpawnPoint = transform.position + new Vector3(0, 1f, 0);
        flag.color = new Color(0.3f, 0.9f, 0.4f);
        Fx.Burst(flag.transform.position, new Color(0.3f, 0.9f, 0.4f), 20, 3f, 0.15f);
        Fx.Coin();
        GameHUD.Toast("Checkpoint!");
    }
}
