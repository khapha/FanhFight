using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Mini-game "Khu vườn hoa": thư giãn, không thua, không giờ.
// Cầm bình xịt theo ngón tay, sương phun ra từ vòi phía trên; gián dính đủ thuốc hoá thành hoa.
// Hoa được lưu lại, khu vườn lớn dần qua mỗi lần chơi.
public class SprayGame : MonoBehaviour
{
    public int maxRoaches = 5;
    public float sprayRange = 1.6f;      // tầm phun phía trước vòi
    public float sprayRadius = 1.0f;     // độ rộng làn sương
    public float dosePerSecond = 1.4f;
    public int maxFlowers = 80;

    const string SaveKey = "garden";

    Camera cam;
    Rect area;
    readonly List<Roach> roaches = new List<Roach>();
    readonly List<GameObject> flowers = new List<GameObject>();
    readonly StringBuilder save = new StringBuilder();
    TextMeshProUGUI countText;
    ParticleSystem mist;
    AudioSource hiss;
    Transform bottle;
    float spawnTimer;
    int total;

    static readonly Color[] FlowerColors = {
        new Color(1f, 0.6f, 0.75f), new Color(0.7f, 0.8f, 1f), new Color(1f, 0.95f, 0.6f),
        new Color(1f, 1f, 1f), new Color(0.95f, 0.75f, 1f), new Color(1f, 0.7f, 0.5f) };

    void Start()
    {
        cam = Camera.main;
        cam.backgroundColor = new Color(0.45f, 0.72f, 0.38f);
        area = MiniUI.WorldArea(cam, 0.8f);
        MiniUI.MakeFloor(cam, MiniArt.Grass);
        total = PlayerPrefs.GetInt("sprayTotal", 0);
        LoadGarden();

        Canvas canvas = MiniUI.MakeCanvas("UI");
        Transform c = canvas.transform;
        MiniUI.MakeButton(c, "< Menu", new Vector2(0, 1), new Vector2(90, -40), new Vector2(150, 50), MiniUI.BackToMenu);
        MiniUI.MakeButton(c, "Lam moi vuon", new Vector2(1, 1), new Vector2(-120, -40), new Vector2(210, 50), ClearGarden);
        countText = MiniUI.MakeText(c, "", 30, new Vector2(0.5f, 1), new Vector2(0, -40));
        countText.color = Color.white;
        countText.outlineColor = new Color(0.15f, 0.35f, 0.15f);
        TextMeshProUGUI hint = MiniUI.MakeText(c, "Giu va di chuyen binh xit, nham voi vao gian", 26, new Vector2(0.5f, 0), new Vector2(0, 40));
        hint.color = Color.white;
        hint.outlineColor = new Color(0.15f, 0.35f, 0.15f);
        StartCoroutine(FadeHint(hint));

        MakeBottle();
        MakeMist();
        hiss = gameObject.AddComponent<AudioSource>();
        hiss.clip = HissClip();
        hiss.loop = true;
    }

    IEnumerator FadeHint(TextMeshProUGUI t)
    {
        yield return new WaitForSeconds(5f);
        for (float a = 1; a > 0; a -= Time.deltaTime) { t.alpha = a; yield return null; }
    }

    void Update()
    {
        if (GameInput.Pause.WasPressedThisFrame()) MiniUI.BackToMenu();

        roaches.RemoveAll(r => r == null);
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0 && roaches.Count < maxRoaches)
        {
            spawnTimer = Random.Range(1.2f, 2.5f);
            Roach r = Roach.Spawn(area, Random.Range(0.6f, 1.1f), false, 999f);
            r.calm = true;
            roaches.Add(r);
        }

        // bình xịt đi theo chuột/ngón tay; chỉ phun khi đang giữ
        Vector2? held = MiniUI.Held(cam);
        Vector2 pointer = held ?? PointerWorld();
        bottle.position = new Vector3(pointer.x, pointer.y - 1.3f, 0);
        Vector2 nozzle = (Vector2)bottle.position + new Vector2(0.13f, 2.3f);
        Vector2 sprayCenter = nozzle + Vector2.up * sprayRange * 0.6f;
        mist.transform.position = nozzle;

        var emission = mist.emission;
        emission.enabled = held.HasValue;
        if (held.HasValue)
        {
            if (!hiss.isPlaying) hiss.Play();
            hiss.volume = 0.25f * Fx.SfxVolume;
            foreach (Roach r in roaches)
            {
                if (r == null || !r.Alive) continue;
                if (Vector2.Distance(sprayCenter, r.transform.position) > sprayRadius) continue;
                r.dose += dosePerSecond * Time.deltaTime;
                r.Flee(nozzle);
                r.Tint(Color.Lerp(Color.white, new Color(0.7f, 1f, 0.7f), r.dose));
                if (r.dose >= 1f) r.Faint(Bloom);
            }
        }
        else if (hiss.isPlaying) hiss.Stop();

        countText.text = "Vuon co " + flowers.Count + " bong hoa";
    }

    static Vector2 PointerWorld()
    {
        Vector2 screen = Pointer.current != null ? Pointer.current.position.ReadValue() : new Vector2(Screen.width / 2f, Screen.height / 3f);
        return Camera.main.ScreenToWorldPoint(screen);
    }

    // gián hoá thành hoa nở trên cỏ, lưu vào khu vườn
    void Bloom(Vector3 pos)
    {
        total++;
        PlayerPrefs.SetInt("sprayTotal", total);
        Fx.Coin();
        Fx.Burst(pos, Color.white, 14, 2.5f, 0.15f);
        int color = Random.Range(0, FlowerColors.Length);
        float size = Random.Range(1.4f, 2f);
        float rot = Random.Range(0f, 360f);
        GameObject f = PlantFlower(pos, color, size, rot);
        StartCoroutine(Grow(f.transform, size));
        if (flowers.Count > maxFlowers)
        {
            Destroy(flowers[0]);
            flowers.RemoveAt(0);
        }
        SaveGarden();
    }

    GameObject PlantFlower(Vector2 pos, int color, float size, float rot)
    {
        GameObject f = new GameObject("Flower");
        SpriteRenderer sr = f.AddComponent<SpriteRenderer>();
        sr.sprite = MiniArt.Flower;
        sr.color = FlowerColors[Mathf.Clamp(color, 0, FlowerColors.Length - 1)];
        sr.sortingOrder = 5;
        f.transform.position = pos;
        f.transform.rotation = Quaternion.Euler(0, 0, rot);
        f.transform.localScale = Vector3.one * size;
        flowers.Add(f);
        return f;
    }

    IEnumerator Grow(Transform t, float target)
    {
        for (float a = 0; a < 0.4f; a += Time.deltaTime)
        {
            if (t == null) yield break;
            float k = a / 0.4f;
            t.localScale = Vector3.one * target * (k + Mathf.Sin(k * Mathf.PI) * 0.3f);
            yield return null;
        }
        if (t != null) t.localScale = Vector3.one * target;
    }

    // ---- lưu khu vườn: "x,y,màu,cỡ,góc;..." ----
    void SaveGarden()
    {
        save.Clear();
        foreach (GameObject f in flowers)
        {
            SpriteRenderer sr = f.GetComponent<SpriteRenderer>();
            int color = System.Array.IndexOf(FlowerColors, sr.color);
            Vector3 p = f.transform.position;
            save.Append(string.Format(CultureInfo.InvariantCulture, "{0:F2},{1:F2},{2},{3:F2},{4:F0};",
                p.x, p.y, Mathf.Max(color, 0), f.transform.localScale.x, f.transform.eulerAngles.z));
        }
        PlayerPrefs.SetString(SaveKey, save.ToString());
        PlayerPrefs.Save();
    }

    void LoadGarden()
    {
        foreach (string item in PlayerPrefs.GetString(SaveKey, "").Split(';'))
        {
            string[] v = item.Split(',');
            if (v.Length != 5) continue;
            var ci = CultureInfo.InvariantCulture;
            PlantFlower(new Vector2(float.Parse(v[0], ci), float.Parse(v[1], ci)),
                        int.Parse(v[2], ci), float.Parse(v[3], ci), float.Parse(v[4], ci));
        }
    }

    void ClearGarden()
    {
        foreach (GameObject f in flowers) Destroy(f);
        flowers.Clear();
        PlayerPrefs.DeleteKey(SaveKey);
    }

    void MakeBottle()
    {
        GameObject go = new GameObject("Bottle");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = MiniArt.Bottle;
        sr.sortingOrder = 25;
        go.transform.localScale = Vector3.one * 1.3f;
        bottle = go.transform;
    }

    // làn sương thuốc phun lên từ vòi
    void MakeMist()
    {
        GameObject go = new GameObject("Mist");
        mist = go.AddComponent<ParticleSystem>();
        mist.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = mist.main;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.45f);
        main.startColor = new Color(0.85f, 1f, 0.9f, 0.6f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 400;
        var emission = mist.emission;
        emission.rateOverTime = 80;
        var shape = mist.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 20f;
        shape.radius = 0.05f;
        // hình nón mặc định hướng theo trục Z; xoay để phun thẳng lên
        shape.rotation = new Vector3(-90f, 0, 0);
        var size = mist.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 0.5f, 1, 2f));
        var col = mist.colorOverLifetime;
        col.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                  new[] { new GradientAlphaKey(0.7f, 0), new GradientAlphaKey(0, 1) });
        col.color = g;
        ParticleSystemRenderer r = go.GetComponent<ParticleSystemRenderer>();
        r.material = new Material(Shader.Find("Sprites/Default"));
        r.sortingOrder = 20;
        mist.Play();
    }

    // tiếng xì của bình xịt (nhiễu trắng nhẹ, lặp liên tục)
    static AudioClip HissClip()
    {
        int rate = 22050, n = rate / 2;
        float[] d = new float[n];
        System.Random rnd = new System.Random(3);
        float last = 0;
        for (int i = 0; i < n; i++)
        {
            float x = (float)rnd.NextDouble() * 2 - 1;
            last = last * 0.6f + x * 0.4f;   // lọc bớt tiếng rít
            d[i] = last * 0.5f;
        }
        AudioClip c = AudioClip.Create("hiss", n, 1, rate, false);
        c.SetData(d, 0);
        return c;
    }
}
