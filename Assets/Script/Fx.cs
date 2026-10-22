using UnityEngine;

// Âm thanh 8-bit tạo bằng code + hạt bụi/nổ, không cần file asset
public class Fx : MonoBehaviour
{
    static Fx instance;
    AudioSource sfx, music;
    AudioClip jumpClip, hurtClip, explodeClip, coinClip, hitClip, sprayClip, musicClip;
    Material particleMat;
    const int Rate = 22050;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        if (instance != null) return;
        GameObject go = new GameObject("Fx");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<Fx>();
    }

    void Awake()
    {
        sfx = gameObject.AddComponent<AudioSource>();
        music = gameObject.AddComponent<AudioSource>();
        music.loop = true;
        ApplyVolume();

        jumpClip = Tone("jump", 0.12f, 300f, 700f, 0f);
        coinClip = Tone("coin", 0.1f, 900f, 1400f, 0f);
        hitClip = Tone("hit", 0.08f, 200f, 90f, 0.7f);
        sprayClip = Tone("spray", 0.3f, 3000f, 2500f, 0.97f);
        hurtClip = Tone("hurt", 0.25f, 400f, 120f, 0.5f);
        explodeClip = Tone("explode", 0.5f, 120f, 40f, 1f);
        musicClip = Music();
        music.clip = musicClip;
        music.Play();

        particleMat = new Material(Shader.Find("Sprites/Default"));
    }

    // sóng vuông trượt tần số, trộn thêm nhiễu
    static AudioClip Tone(string name, float len, float f0, float f1, float noise)
    {
        int n = (int)(Rate * len);
        float[] data = new float[n];
        float phase = 0;
        System.Random rnd = new System.Random(1);
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            phase += Mathf.Lerp(f0, f1, t) / Rate;
            float sq = Mathf.Repeat(phase, 1f) < 0.5f ? 1f : -1f;
            float nz = (float)rnd.NextDouble() * 2f - 1f;
            data[i] = Mathf.Lerp(sq, nz, noise) * (1f - t) * 0.4f;
        }
        AudioClip c = AudioClip.Create(name, n, 1, Rate, false);
        c.SetData(data, 0);
        return c;
    }

    // vòng nhạc nền ngắn: bass + giai điệu
    static AudioClip Music()
    {
        int[] melody = { 0, 4, 7, 4, 9, 7, 4, 2, 0, 4, 7, 12, 11, 7, 4, 2 };
        int[] bass = { 0, 0, -3, -3, -7, -7, -5, -5 };
        float step = 0.2f;
        int perStep = (int)(Rate * step);
        int n = perStep * melody.Length;
        float[] data = new float[n];
        float pm = 0, pb = 0;
        for (int i = 0; i < n; i++)
        {
            int s = i / perStep;
            float local = (float)(i % perStep) / perStep;
            float fm = 440f * Mathf.Pow(2f, melody[s] / 12f);
            float fb = 110f * Mathf.Pow(2f, bass[s / 2] / 12f);
            pm += fm / Rate;
            pb += fb / Rate;
            float m = (Mathf.Repeat(pm, 1f) < 0.25f ? 1f : -1f) * (1f - local) * 0.15f;
            float b = (Mathf.Repeat(pb, 1f) < 0.5f ? 1f : -1f) * 0.12f;
            data[i] = m + b;
        }
        AudioClip c = AudioClip.Create("music", n, 1, Rate, false);
        c.SetData(data, 0);
        return c;
    }

    // âm lượng (0..1), lưu trong PlayerPrefs, chỉnh ở menu chính
    public static float MusicVolume
    {
        get { return PlayerPrefs.GetFloat("musicVolume", 0.8f); }
        set { PlayerPrefs.SetFloat("musicVolume", Mathf.Clamp01(value)); if (instance != null) instance.ApplyVolume(); }
    }
    public static float SfxVolume
    {
        get { return PlayerPrefs.GetFloat("sfxVolume", 1f); }
        set { PlayerPrefs.SetFloat("sfxVolume", Mathf.Clamp01(value)); }
    }
    void ApplyVolume() { music.volume = 0.3f * MusicVolume; }

    public static void Coin() { Play(instance?.coinClip, 0.4f); }
    public static void Hit() { Play(instance?.hitClip, 0.8f); }
    public static void Jump() { Play(instance?.jumpClip, 0.5f); }
    public static void Hurt() { Play(instance?.hurtClip, 0.7f); }
    public static void Explode() { Play(instance?.explodeClip, 0.8f); }

    static void Play(AudioClip c, float vol)
    {
        if (instance != null && c != null) instance.sfx.PlayOneShot(c, vol * SfxVolume);
    }

    // bắn ra một chùm hạt rồi tự hủy
    public static void Burst(Vector3 pos, Color color, int count, float speed, float size)
    {
        if (instance == null) return;
        GameObject go = new GameObject("Burst");
        go.transform.position = pos;
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.5f, speed);
        main.startSize = new ParticleSystem.MinMaxCurve(size * 0.5f, size);
        main.startColor = color;
        main.gravityModifier = 0.5f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.Destroy;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.1f;
        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(color, 0), new GradientColorKey(color, 1) },
                  new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1) });
        col.color = g;

        ParticleSystemRenderer r = go.GetComponent<ParticleSystemRenderer>();
        r.material = instance.particleMat;
        r.sortingOrder = 50;

        ps.Emit(count);
        ps.Play();
    }

    // luồng khí xịt từ bình xịt phun ra phía trước
    public static void Spray(Vector3 pos, bool right)
    {
        if (instance == null) return;
        Play(instance.sprayClip, 0.5f);
        GameObject go = new GameObject("Spray");
        go.transform.position = pos + new Vector3(right ? 0.6f : -0.6f, 0.15f, 0);
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.loop = false;
        main.duration = 0.4f;
        main.gravityModifier = 0;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.Destroy;
        var emission = ps.emission;
        emission.rateOverTime = 0;
        var shape = ps.shape;
        shape.enabled = false;
        // hạt to dần và mờ dần như làn sương
        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 0.4f, 1, 1.6f));
        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                  new[] { new GradientAlphaKey(0.8f, 0), new GradientAlphaKey(0, 1) });
        col.color = g;
        ParticleSystemRenderer r = go.GetComponent<ParticleSystemRenderer>();
        r.material = instance.particleMat;
        r.sortingOrder = 40;
        float dir = right ? 1f : -1f;
        for (int i = 0; i < 28; i++)
        {
            ParticleSystem.EmitParams e = new ParticleSystem.EmitParams();
            e.velocity = new Vector3(dir * Random.Range(3f, 6f), Random.Range(-0.8f, 0.8f), 0);
            e.startLifetime = Random.Range(0.25f, 0.4f);
            e.startSize = Random.Range(0.15f, 0.3f);
            e.startColor = new Color(0.75f, 1f, 0.8f);
            ps.Emit(e, 1);
        }
        ps.Play();
    }

    // khựng hình rất ngắn khi đánh trúng cho có lực
    public static void HitStop(float seconds = 0.06f)
    {
        if (instance == null || Time.timeScale == 0) return;
        Hit();
        instance.StartCoroutine(instance.DoHitStop(seconds));
    }

    System.Collections.IEnumerator DoHitStop(float seconds)
    {
        Time.timeScale = 0.05f;
        yield return new WaitForSecondsRealtime(seconds);
        if (Time.timeScale > 0) Time.timeScale = 1;
    }

    // chữ bay lên rồi mờ dần (ví dụ "+1" khi nhặt xu)
    public static void Popup(Vector3 pos, string text, Color color)
    {
        if (instance == null) return;
        GameObject go = new GameObject("Popup");
        go.transform.position = pos + new Vector3(0, 0.6f, 0);
        TMPro.TextMeshPro t = go.AddComponent<TMPro.TextMeshPro>();
        t.text = text;
        t.fontSize = 4;
        t.alignment = TMPro.TextAlignmentOptions.Center;
        t.color = color;
        t.outlineWidth = 0.25f;
        t.outlineColor = Color.black;
        t.sortingOrder = 60;
        instance.StartCoroutine(instance.FloatUp(t));
    }

    System.Collections.IEnumerator FloatUp(TMPro.TextMeshPro t)
    {
        Color c = t.color;
        for (float a = 0; a < 0.7f; a += Time.deltaTime)
        {
            t.transform.position += new Vector3(0, Time.deltaTime * 1.5f, 0);
            c.a = 1f - a / 0.7f;
            t.color = c;
            yield return null;
        }
        Destroy(t.gameObject);
    }

    public static void Dust(Vector3 pos) { Burst(pos, new Color(0.8f, 0.75f, 0.6f), 6, 1.5f, 0.15f); }
    public static void Explosion(Vector3 pos) { Burst(pos, new Color(1f, 0.6f, 0.2f), 30, 5f, 0.3f); Explode(); }
}
