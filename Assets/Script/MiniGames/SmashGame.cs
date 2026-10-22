using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Mini-game "Đập dép": 60 giây, chạm vào gián để đập, đập liên tiếp được nhân combo
public class SmashGame : MonoBehaviour
{
    public float duration = 60f;

    Camera cam;
    Rect area;
    readonly List<Roach> roaches = new List<Roach>();
    TextMeshProUGUI scoreText, timeText, comboText, bigText;
    Canvas canvas;
    GameObject endPanel;
    int score, combo, smashed;
    float timeLeft, spawnTimer, comboTimer;
    bool playing;
    int displayedScore = -1, displayedSeconds = -1, displayedCombo = -1;

    void Start()
    {
        cam = Camera.main;
        area = MiniUI.WorldArea(cam, 0.8f);
        MiniUI.MakeFloor(cam);

        canvas = MiniUI.MakeCanvas("UI");
        Transform c = SafeArea.Create(canvas.transform);
        bool compact = Screen.width < Screen.height;
        float scoreY = compact ? -105 : -40;
        CuteTheme.Card(c, new Vector2(0.5f, 1), new Vector2(0, scoreY), new Vector2(250, 60));
        CuteTheme.Card(c, new Vector2(1, 1), new Vector2(-80, -40), new Vector2(140, 60));
        MiniUI.MakeButton(c, "< Menu", new Vector2(0, 1), new Vector2(90, -40), new Vector2(150, 50), MiniUI.BackToMenu);
        scoreText = MiniUI.MakeText(c, "", 32, new Vector2(0.5f, 1), new Vector2(0, scoreY));
        timeText = MiniUI.MakeText(c, "", 32, new Vector2(1, 1), new Vector2(-80, -40));
        CuteTheme.Text(scoreText, 32);
        CuteTheme.Text(timeText, 32);
        comboText = MiniUI.MakeText(c, "", 28, new Vector2(0.5f, 1), new Vector2(0, scoreY - 60));
        comboText.color = new Color(0.85f, 0.4f, 0.1f);
        bigText = MiniUI.MakeText(c, "", 90, new Vector2(0.5f, 0.5f), new Vector2(0, 40));
        StartCoroutine(Countdown());
    }

    IEnumerator Countdown()
    {
        score = 0; combo = 0; smashed = 0;
        timeLeft = duration;
        UpdateTexts();
        foreach (string s in new[] { "3", "2", "1", "Smash!" })
        {
            bigText.text = s;
            Fx.Coin();
            yield return new WaitForSeconds(0.7f);
        }
        bigText.text = "";
        playing = true;
    }

    void Update()
    {
        if (GameInput.Pause.WasPressedThisFrame()) MiniUI.BackToMenu();
        UpdateHover();
        if (!playing) return;

        timeLeft -= Time.deltaTime;
        float progress = 1f - timeLeft / duration;   // 0 → 1: càng về cuối càng nhanh, càng nhiều

        spawnTimer -= Time.deltaTime;
        roaches.RemoveAll(r => r == null);
        if (spawnTimer <= 0 && roaches.Count < 4 + (int)(progress * 8))
        {
            spawnTimer = Mathf.Lerp(0.9f, 0.3f, progress);
            bool golden = Random.value < 0.1f;
            float speed = Mathf.Lerp(1.5f, 4f, progress) * (golden ? 1.5f : 1f);
            roaches.Add(Roach.Spawn(area, speed, golden, Random.Range(4f, 7f)));
        }

        foreach (Vector2 p in MiniUI.Taps(cam)) Tap(p);

        comboTimer -= Time.deltaTime;
        if (comboTimer <= 0) combo = 0;
        UpdateTexts();
        if (timeLeft <= 0) End();
    }

    void Tap(Vector2 p)
    {
        StartCoroutine(SlipperHit(p));
        // con gián gần điểm chạm nhất trong bán kính đập
        Roach best = null;
        float bestDist = 0.9f;
        foreach (Roach r in roaches)
        {
            if (r == null || !r.Alive) continue;
            float d = Vector2.Distance(p, r.transform.position);
            if (d < bestDist) { bestDist = d; best = r; }
        }
        if (best == null)
        {
            Fx.Burst(p, new Color(0.7f, 0.65f, 0.55f), 5, 1.2f, 0.12f);
            return;
        }
        combo++;
        comboTimer = 1.2f;
        smashed++;
        int gain = (best.golden ? 5 : 1) * Mathf.Min(1 + combo / 5, 5);
        score += gain;
        best.Squash();
        Fx.HitStop(0.04f);
        CameraShake();
        Fx.Popup(best.transform.position, "+" + gain, best.golden ? new Color(1f, 0.85f, 0.2f) : Color.white);
    }

    // trên máy tính: chiếc dép hồng lơ lửng theo con trỏ chuột khi đang chơi
    GameObject hover;

    void UpdateHover()
    {
        var mouse = UnityEngine.InputSystem.Mouse.current;
        bool show = playing && mouse != null && UnityEngine.InputSystem.Touchscreen.current == null;
        if (show && hover == null)
        {
            hover = new GameObject("HoverSlipper");
            SpriteRenderer sr = hover.AddComponent<SpriteRenderer>();
            sr.sprite = MiniArt.Slipper;
            sr.color = new Color(1, 1, 1, 0.85f);
            sr.sortingOrder = 29;
        }
        if (hover != null)
        {
            hover.SetActive(show);
            if (show)
            {
                Vector3 p = cam.ScreenToWorldPoint(mouse.position.ReadValue());
                hover.transform.position = new Vector3(p.x, p.y, 0);
                hover.transform.rotation = Quaternion.Euler(0, 0, 15f);
                hover.transform.localScale = Vector3.one * 1.3f;
            }
        }
        // ẩn con trỏ hệ thống khi có dép thay thế, hiện lại ở bảng kết thúc/đếm ngược
        Cursor.visible = !show;
    }

    void OnDestroy()
    {
        Cursor.visible = true;
    }

    // chiếc dép đập xuống chỗ chạm
    IEnumerator SlipperHit(Vector2 p)
    {
        GameObject go = new GameObject("Slipper");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = MiniArt.Slipper;
        sr.sortingOrder = 30;
        go.transform.position = p;
        go.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-25f, 25f));
        for (float t = 0; t < 0.12f; t += Time.unscaledDeltaTime)
        {
            go.transform.localScale = Vector3.one * Mathf.Lerp(1.8f, 1f, t / 0.12f);
            yield return null;
        }
        go.transform.localScale = Vector3.one;
        yield return new WaitForSecondsRealtime(0.12f);
        for (float t = 0; t < 0.2f; t += Time.unscaledDeltaTime)
        {
            sr.color = new Color(1, 1, 1, 1 - t / 0.2f);
            yield return null;
        }
        Destroy(go);
    }

    void CameraShake()
    {
        StopCoroutine("Shake");
        StartCoroutine("Shake");
    }

    IEnumerator Shake()
    {
        Vector3 basePos = new Vector3(0, 0, cam.transform.position.z);
        for (float t = 0; t < 0.1f; t += Time.unscaledDeltaTime)
        {
            cam.transform.position = basePos + (Vector3)(Random.insideUnitCircle * 0.12f);
            yield return null;
        }
        cam.transform.position = basePos;
    }

    void UpdateTexts()
    {
        if (displayedScore != score) { displayedScore = score; scoreText.text = "Score: " + score; }
        int seconds = Mathf.CeilToInt(Mathf.Max(timeLeft, 0));
        if (displayedSeconds != seconds) { displayedSeconds = seconds; timeText.text = seconds + "s"; }
        if (displayedCombo != combo) { displayedCombo = combo; comboText.text = combo >= 3 ? "Combo x" + combo : ""; }
    }

    void End()
    {
        playing = false;
        foreach (Roach r in roaches) if (r != null) Destroy(r.gameObject);
        roaches.Clear();
        int best = Mathf.Max(PlayerPrefs.GetInt("smashBest", 0), score);
        PlayerPrefs.SetInt("smashBest", best);
        PlayerPrefs.Save();

        endPanel = MiniUI.Panel(canvas.transform, new Color(0, 0, 0, 0.6f));
        Transform p = endPanel.transform;
        foreach (TextMeshProUGUI t in new[] {
            MiniUI.MakeText(p, "Time's up!", 72, new Vector2(0.5f, 0.5f), new Vector2(0, 170)),
            MiniUI.MakeText(p, "Score: " + score + "     Smashes: " + smashed, 40, new Vector2(0.5f, 0.5f), new Vector2(0, 80)),
            MiniUI.MakeText(p, "Best: " + best, 34, new Vector2(0.5f, 0.5f), new Vector2(0, 25)) })
        {
            t.color = Color.white;
            t.outlineColor = Color.black;
        }
        Button again = MiniUI.MakeButton(p, "Play Again", new Vector2(0.5f, 0.5f), new Vector2(-120, -70), new Vector2(200, 60), Restart);
        MiniUI.MakeButton(p, "Menu", new Vector2(0.5f, 0.5f), new Vector2(120, -70), new Vector2(200, 60), MiniUI.BackToMenu);
        MiniUI.Select(again);
    }

    void Restart()
    {
        Destroy(endPanel);
        StartCoroutine(Countdown());
    }
}
