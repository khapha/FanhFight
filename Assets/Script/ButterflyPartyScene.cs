using System.Collections;
using UnityEngine;

// TIỆC HÓA HOA — scene thưởng thức: cả màn trời hồng rực rỡ bầy bướm bay lượn,
// hoa nở khắp sân, banner cảm ơn — phần quà cho người đã cứu đủ 8 chú bướm.
public class ButterflyPartyScene : MonoBehaviour
{
    void Start()
    {
        // sàn cỏ hồng
        GameObject floor = new GameObject("Floor");
        SpriteRenderer fsr = floor.AddComponent<SpriteRenderer>();
        fsr.sprite = CuteArt.Grass;
        fsr.transform.position = new Vector3(0, -4.2f, 5);
        fsr.transform.localScale = new Vector3(40f, 3f, 1);
        fsr.sortingOrder = -5;

        // hàng hoa khắp sân khấu
        for (int i = 0; i < 16; i++)
        {
            GameObject flower = new GameObject("Flower" + i);
            SpriteRenderer sr = flower.AddComponent<SpriteRenderer>();
            sr.sprite = CuteArt.Flower;
            float x = -14f + i * 1.9f;
            sr.transform.position = new Vector3(x, -3.6f + Mathf.Sin(i * 1.7f) * 0.15f, 0);
            sr.sortingOrder = -3;
            sr.color = Color.Lerp(new Color(1f, 0.75f, 0.85f), new Color(0.65f, 0.85f, 1f), (i % 4) / 3f);
        }

        // bầy bướm bay lượn ngẫu nhiên (10 + số bướm đã cứu, tối đa 20)
        int count = 10 + Mathf.Clamp(Diary.Rescued, 0, 10);
        for (int i = 0; i < count; i++)
        {
            GameObject b = new GameObject("PartyButterfly" + i);
            b.AddComponent<PartyButterfly>().Init(i);
        }

        // banner cảm ơn (dùng MiniUI cho đồng bộ phong cách menu)
        Canvas ui = MiniUI.MakeCanvas("PartyCanvas");
        Transform safe = SafeArea.Create(ui.transform);
        TMPro.TextMeshProUGUI thanks = MiniUI.MakeText(safe,
            Diary.Rescued > 0 ? "Thank you for rescuing us!" : "The garden is waiting for you!",
            30, Vector2.one * 0.5f, new Vector2(0, 170));
        CuteTheme.Text(thanks, 30);
        thanks.color = new Color(0.55f, 0.25f, 0.4f);
        TMPro.TextMeshProUGUI hint = MiniUI.MakeText(safe,
            "Esc: back to menu   |   Butterflies rescued: " + Diary.Rescued,
            20, Vector2.one * 0.5f, new Vector2(0, -200));
        CuteTheme.Text(hint, 20);
        hint.color = new Color(0.45f, 0.3f, 0.45f);
    }

    void Update()
    {
        // Esc (hoặc nút Pause trên gamepad) quay về menu
        if (GameInput.Pause != null && GameInput.Pause.WasPressedThisFrame())
            GameSession.LoadScene(0);
    }
}

// một chú bướm tiệc: bay theo quỹ đạo Lissajous, quay mặt theo hướng bay
public class PartyButterfly : MonoBehaviour
{
    SpriteRenderer sr;
    Vector2 center;
    float ax, ay, fx, fy, phase, speed;
    float flipTimer;

    static readonly Color[] Tints = {
        new Color(1f, 0.8f, 0.9f), new Color(0.85f, 0.9f, 1f), new Color(1f, 0.95f, 0.75f),
        new Color(0.85f, 1f, 0.9f), new Color(0.95f, 0.85f, 1f), Color.white
    };

    public void Init(int index)
    {
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = CuteArt.Butterfly;
        sr.sortingOrder = 2;
        sr.color = Tints[index % Tints.Length];
        center = new Vector2(Random.Range(-13f, 13f), Random.Range(-1.5f, 4.5f));
        ax = Random.Range(1.5f, 4f);
        ay = Random.Range(0.8f, 2f);
        fx = Random.Range(0.4f, 0.9f);
        fy = Random.Range(0.5f, 1.1f);
        phase = Random.Range(0f, Mathf.PI * 2f);
        speed = Random.Range(0.7f, 1.3f);
        transform.localScale = Vector3.one * Random.Range(0.9f, 1.6f);
        transform.position = center;
    }

    void Update()
    {
        float t = Time.time * speed;
        float x = center.x + Mathf.Sin(t * fx + phase) * ax;
        float y = center.y + Mathf.Sin(t * fy + phase * 1.7f) * ay;
        transform.position = new Vector3(x, y, 0);

        // quay mặt theo chiều ngang của vận tốc ngang (đạo hàm quỹ đạo)
        float vx = Mathf.Cos(t * fx + phase);
        flipTimer += Time.deltaTime;
        if (flipTimer > 0.25f)
        {
            flipTimer = 0;
            sr.flipX = vx < 0;
        }
    }
}
