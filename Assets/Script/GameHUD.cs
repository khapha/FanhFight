using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// HUD gọn: tim, xu, mạng (trái) - tên màn (giữa) - điểm cao nhất (phải).
// Dựng bằng code và ẩn HUD cũ trong scene.
public class GameHUD : MonoBehaviour
{
    static GameHUD instance;

    Player player;
    GameMaster gm;
    Image[] hearts;
    TextMeshProUGUI coinText, livesText, bestText, toastText;
    float toastTimer;
    static Sprite heartSprite, circleSprite;
    Transform content;
    RectTransform titleCard;
    int lastHealth = -1, lastPoints = -1, lastLives = -1, lastBest = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p == null) return;
            new GameObject("GameHUD").AddComponent<GameHUD>().Build(p.GetComponent<Player>(), LevelNumber(scene.name));
        };
    }

    // số thứ tự màn theo tên scene (màn 3 nằm trong file Scene 4)
    public static int LevelNumber(string sceneName)
    {
        return AdventureProgress.LevelNumber(sceneName);
    }

    public static void Toast(string text)
    {
        if (instance == null) return;
        instance.toastText.text = text;
        instance.toastTimer = 1.5f;
    }

    void Build(Player p, int level)
    {
        instance = this;
        player = p;
        gm = GameObject.FindGameObjectWithTag("gamemaster").GetComponent<GameMaster>();
        HideOldHud();
        MakeSprites();

        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        content = SafeArea.Create(transform);
        CuteTheme.Card(content, new Vector2(0, 1), new Vector2(155, -79), new Vector2(286, 142));
        titleCard = CuteTheme.Card(content, new Vector2(0.5f, 1), new Vector2(0, -39), new Vector2(370, 58)).rectTransform;
        CuteTheme.Card(content, new Vector2(1, 1), new Vector2(-138, -39), new Vector2(252, 58));

        // trái trên: tim
        hearts = new Image[player.maxhealth];
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i] = MakeImage(heartSprite, new Vector2(0, 1), new Vector2(30 + i * 42, -30), new Vector2(36, 32));
        }
        // xu
        MakeImage(circleSprite, new Vector2(0, 1), new Vector2(30, -78), new Vector2(26, 26)).color = new Color(1f, 0.85f, 0.2f);
        coinText = MakeText("0", 30, new Vector2(0, 1), new Vector2(62, -78), TextAlignmentOptions.Left);
        // mạng
        livesText = MakeText("", 26, new Vector2(0, 1), new Vector2(30, -118), TextAlignmentOptions.Left);
        // giữa trên: tên màn
        string title = level >= 1 && level <= AdventureProgress.Count
            ? "Level " + level + "/" + AdventureProgress.Count + " - " + AdventureProgress.Titles[level - 1] : "";
        TextMeshProUGUI titleText = MakeText(title, 24, new Vector2(0.5f, 0.5f), Vector2.zero, TextAlignmentOptions.Center);
        titleText.transform.SetParent(titleCard, false);
        titleText.rectTransform.anchorMin = titleText.rectTransform.anchorMax = Vector2.one * 0.5f;
        titleText.rectTransform.pivot = Vector2.one * 0.5f;
        titleText.rectTransform.anchoredPosition = Vector2.zero;
        // phải trên: điểm cao nhất
        bestText = MakeText("", 26, new Vector2(1, 1), new Vector2(-30, -30), TextAlignmentOptions.Right);
        // giữa màn: thông báo ngắn (checkpoint...)
        toastText = MakeText("", 40, new Vector2(0.5f, 0.5f), new Vector2(0, 140), TextAlignmentOptions.Center);
    }

    // tắt chữ điểm/điểm cao và thanh máu cũ (script cũ vẫn chạy bình thường)
    void HideOldHud()
    {
        if (gm.pointText != null) gm.pointText.gameObject.SetActive(false);
        if (gm.Hightext != null) gm.Hightext.gameObject.SetActive(false);
        foreach (HeartUI h in FindObjectsByType<HeartUI>(FindObjectsSortMode.None))
        {
            // HeartUI cũ ép máu tối đa về 4 (cho khớp ảnh thanh máu cũ), tắt hẳn
            h.enabled = false;
            if (h.Heart != null) h.Heart.enabled = false;
        }
    }

    Image MakeImage(Sprite s, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject("Image");
        go.transform.SetParent(content, false);
        Image img = go.AddComponent<Image>();
        img.sprite = s;
        img.raycastTarget = false;
        RectTransform rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return img;
    }

    TextMeshProUGUI MakeText(string s, int size, Vector2 anchor, Vector2 pos, TextAlignmentOptions align)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(content, false);
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.text = s;
        t.fontSize = size;
        t.alignment = align;
        CuteTheme.Text(t, size);
        t.raycastTarget = false;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        RectTransform rt = t.rectTransform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(anchor.x, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(500, size * 1.4f);
        return t;
    }

    // tim và hình tròn vẽ bằng pixel
    static void MakeSprites()
    {
        if (heartSprite != null) return;
        string[] heart = {
            "..XX...XX..",
            ".XXXX.XXXX.",
            "XXXXXXXXXXX",
            "XXXXXXXXXXX",
            ".XXXXXXXXX.",
            "..XXXXXXX..",
            "...XXXXX...",
            "....XXX....",
            ".....X.....",
        };
        Texture2D t = new Texture2D(11, 9) { filterMode = FilterMode.Point };
        for (int y = 0; y < 9; y++)
            for (int x = 0; x < 11; x++)
                t.SetPixel(x, 8 - y, heart[y][x] == 'X' ? Color.white : Color.clear);
        t.Apply();
        heartSprite = Sprite.Create(t, new Rect(0, 0, 11, 9), new Vector2(0.5f, 0.5f), 16);

        Texture2D c = new Texture2D(16, 16) { filterMode = FilterMode.Point };
        for (int y = 0; y < 16; y++)
            for (int x = 0; x < 16; x++)
                c.SetPixel(x, y, (new Vector2(x - 7.5f, y - 7.5f)).magnitude <= 7.5f ? Color.white : Color.clear);
        c.Apply();
        circleSprite = Sprite.Create(c, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16);
    }

    void Update()
    {
        titleCard.anchoredPosition = new Vector2(0, ((RectTransform)content).rect.width < 1000 ? -185 : -39);
        if (lastHealth != player.ourHealth)
        {
            lastHealth = player.ourHealth;
            for (int i = 0; i < hearts.Length; i++)
                hearts[i].color = i < lastHealth ? CuteTheme.Pink : new Color(0.73f, 0.77f, 0.64f, 0.8f);
        }
        if (lastPoints != gm.points) { lastPoints = gm.points; coinText.text = lastPoints.ToString(); }
        if (lastLives != Checkpoint.Lives)
        {
            lastLives = Checkpoint.Lives;
            livesText.text = "Lives: " + Mathf.Max(lastLives, 0) + "/" + Checkpoint.MaxLives;
        }
        int best = Mathf.Max(gm.highscore, gm.points);
        if (lastBest != best) { lastBest = best; bestText.text = "Best: " + best; }
        if (toastTimer > 0)
        {
            toastTimer -= Time.unscaledDeltaTime;
            toastText.alpha = Mathf.Clamp01(toastTimer);
        }
    }
}
