using UnityEngine;

// Âm thanh 8-bit tạo bằng code + hạt bụi/nổ, không cần file asset
public class Fx : MonoBehaviour
{
    static Fx instance;
    AudioSource sfx, music;
    AudioClip jumpClip, hurtClip, explodeClip, musicClip;
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
        music.volume = 0.25f;

        jumpClip = Tone("jump", 0.12f, 300f, 700f, 0f);
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

    public static void Jump() { Play(instance?.jumpClip, 0.5f); }
    public static void Hurt() { Play(instance?.hurtClip, 0.7f); }
    public static void Explode() { Play(instance?.explodeClip, 0.8f); }

    static void Play(AudioClip c, float vol)
    {
        if (instance != null && c != null) instance.sfx.PlayOneShot(c, vol);
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

    public static void Dust(Vector3 pos) { Burst(pos, new Color(0.8f, 0.75f, 0.6f), 6, 1.5f, 0.15f); }
    public static void Explosion(Vector3 pos) { Burst(pos, new Color(1f, 0.6f, 0.2f), 30, 5f, 0.3f); Explode(); }
}
