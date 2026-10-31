using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// NPC bướm tiên động viên: đứng đầu màn, chạm vào thì hiện bong bóng hội thoại
// gõ chữ kiểu typewriter. Lời nói luôn khích lệ - game trị liệu nỗi sợ gián nên
// gián lúc nào cũng được miêu tả ngốc nghếch và vô hại.
public class ButterflyNPC : MonoBehaviour
{
    static readonly string[][] Lines =
    {
        new[] {
            "Hi! The roaches here are silly and harmless.",
            "Bop them with your slipper - they turn into flowers!",
            "Find 3 golden keys to open the gate. You got this!"
        },
        new[] {
            "Wow, you're getting so brave!",
            "These lake roaches just float around being silly.",
            "Grab the keys and meet me at the gate!"
        },
        new[] {
            "Last level - I believe in you!",
            "The Roach King is big, but he's a big scaredy-cat.",
            "Bop him 6 times and he'll give up for good!"
        },
    };

    Canvas canvas;
    RectTransform card;
    TMPro.TextMeshProUGUI bubble;
    int level;
    float cooldown;
    bool talking;
    int lineIndex, charIndex;
    float charTimer, lineHold;
    const float CharSpeed = 0.025f;

    public static void SpawnAt(float x, int level)
    {
        if (!AdventureProps.TryGround(x, out float y)) return;
        GameObject go = new GameObject("FairyButterfly");
        go.transform.position = new Vector3(x, y + 1.6f, 0);
        ButterflyNPC npc = go.AddComponent<ButterflyNPC>();
        npc.level = Mathf.Clamp(level, 1, Lines.Length) - 1;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CuteArt.Butterfly;
        sr.sortingOrder = 7;
        BoxCollider2D box = go.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2(1.6f, 1.8f);
        go.AddComponent<Flutter>();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        cooldown -= Time.deltaTime;
        if (cooldown > 0 || talking) return;
        Player p = other.GetComponent<Player>();
        if (p == null || !p.CanAct) return;
        StartTalk();
        cooldown = 8f;
    }

    void StartTalk()
    {
        talking = true;
        lineIndex = charIndex = 0;
        charTimer = lineHold = 0;
        EnsureBubble();
        card.gameObject.SetActive(true);
        bubble.text = "";
    }

    // bong bóng hội thoại: overlay canvas, đặt theo vị trí màn hình của con bướm
    void EnsureBubble()
    {
        if (canvas != null) return;
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 40;

        GameObject cardGo = new GameObject("Bubble");
        cardGo.transform.SetParent(canvas.transform, false);
        Image img = cardGo.AddComponent<Image>();
        img.sprite = CuteTheme.Frame;
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = 0.45f;
        img.raycastTarget = false;
        card = cardGo.GetComponent<RectTransform>();
        card.anchorMin = card.anchorMax = Vector2.one * 0.5f;
        card.sizeDelta = new Vector2(660, 110);

        GameObject textGo = new GameObject("Words");
        textGo.transform.SetParent(cardGo.transform, false);
        bubble = textGo.AddComponent<TMPro.TextMeshProUGUI>();
        bubble.text = "";
        CuteTheme.Text(bubble, 22);
        bubble.alignment = TMPro.TextAlignmentOptions.Center;
        bubble.textWrappingMode = TMPro.TextWrappingModes.Normal;
        bubble.raycastTarget = false;
        bubble.rectTransform.anchorMin = bubble.rectTransform.anchorMax = Vector2.one * 0.5f;
        bubble.rectTransform.offsetMin = new Vector2(14, 8);
        bubble.rectTransform.offsetMax = new Vector2(-14, -8);

        cardGo.SetActive(false);
    }

    void Update()
    {
        // đặt bong bóng trên đầu con bướm (đổi world -> screen -> toạ độ canvas)
        if (card != null && card.gameObject.activeSelf && Camera.main != null)
        {
            Vector3 screen = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1.3f);
            card.anchoredPosition = ((Vector2)screen - new Vector2(Screen.width, Screen.height) * 0.5f)
                / Mathf.Max(canvas.scaleFactor, 0.0001f);
        }

        if (!talking) return;
        string[] lines = Lines[level];
        if (charIndex < lines[lineIndex].Length)
        {
            charTimer -= Time.unscaledDeltaTime;
            while (charTimer <= 0 && charIndex < lines[lineIndex].Length)
            {
                charIndex++;
                charTimer += CharSpeed;
            }
            bubble.text = lines[lineIndex].Substring(0, charIndex);
            return;
        }
        // hết câu: giữ một lúc rồi sang câu hoặc kết thúc
        lineHold += Time.unscaledDeltaTime;
        float hold = lineIndex == lines.Length - 1 ? 1.6f : 2.2f;
        if (lineHold >= hold)
        {
            lineHold = 0;
            lineIndex++;
            charIndex = 0;
            if (lineIndex >= lines.Length) EndTalk();
        }
    }

    void EndTalk()
    {
        talking = false;
        if (card != null) card.gameObject.SetActive(false);
    }

    // vỗ cánh lắc lư
    class Flutter : MonoBehaviour
    {
        SpriteRenderer sr;
        Vector3 basePos;
        void Start()
        {
            sr = GetComponent<SpriteRenderer>();
            basePos = transform.position;
        }
        void Update()
        {
            transform.position = basePos + new Vector3(Mathf.Sin(Time.time * 1.7f) * 0.25f, Mathf.Sin(Time.time * 2.3f) * 0.35f, 0);
            if (sr == null) return;
            bool flap = Mathf.Sin(Time.time * 9f) > 0;
            float face = Mathf.Sin(Time.time * 0.7f) > 0 ? 1 : -1;
            transform.localScale = new Vector3(face * (flap ? 0.9f : 1f), flap ? 0.9f : 1f, 1);
        }
    }
}
