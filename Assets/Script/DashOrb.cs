using UnityEngine;
using UnityEngine.SceneManagement;

// Orb dash: chạm vào là mở khóa năng lực lướt nhanh vĩnh viễn (lưu PlayerPrefs).
// Sinh trước cổng màn 1; đã mở rồi thì không sinh lại.
public class DashOrb : MonoBehaviour
{
    public static void SpawnAt(float x)
    {
        if (!AdventureProps.TryGround(x, out float y)) return;
        GameObject go = new GameObject("DashOrb");
        go.transform.position = new Vector3(x, y + 1.2f, 0);
        go.AddComponent<DashOrb>();
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = OrbSprite;
        sr.sortingOrder = 8;
        BoxCollider2D box = go.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2(1.2f, 1.2f);
        go.AddComponent<OrbBob>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        Player player = other.GetComponent<Player>();
        if (player == null || !player.CanAct) return;
        QuestState.UnlockDash();
        Fx.Coin();
        Fx.Burst(transform.position, new Color(0.5f, 0.9f, 1f), 24, 3.5f, 0.25f);
        Fx.Popup(transform.position, "Dash!", new Color(0.5f, 0.9f, 1f));
        GameHUD.Toast("Dash unlocked! Press Shift / L2 to dash");
        Destroy(gameObject);
    }

    // quả cầu xanh có tia lấp lánh
    static Sprite orbSprite;
    public static Sprite OrbSprite
    {
        get
        {
            if (orbSprite != null) return orbSprite;
            Texture2D t = new Texture2D(14, 14, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            for (int y = 0; y < 14; y++) for (int x = 0; x < 14; x++)
            {
                float d = (new Vector2(x - 6.5f, y - 6.5f)).magnitude;
                Color c = Color.clear;
                if (d < 5.5f) c = new Color32(120, 200, 255, 255);
                if (d < 3.5f) c = new Color32(200, 240, 255, 255);
                if (d < 1.8f) c = Color.white;
                // 4 tia
                if ((x == 6 || x == 7 || y == 6 || y == 7) && d >= 5.5f && d < 6.8f)
                    c = new Color32(160, 220, 255, 255);
                t.SetPixel(x, y, c);
            }
            t.Apply();
            return orbSprite = Sprite.Create(t, new Rect(0, 0, 14, 14), new Vector2(0.5f, 0.5f), 16);
        }
    }

    class OrbBob : MonoBehaviour
    {
        Vector3 basePos;
        void Start() { basePos = transform.position; }
        void Update() { transform.position = basePos + new Vector3(0, Mathf.Sin(Time.time * 2.6f) * 0.2f, 0); }
    }
}
