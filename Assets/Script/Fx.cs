using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

// Âm thanh 8-bit tạo bằng code + hạt bụi/nổ, không cần file asset
public class Fx : MonoBehaviour
{
    static Fx instance;
    AudioSource sfx, music;
    AudioClip jumpClip, hurtClip, explodeClip, coinClip, hitClip, sprayClip;
    AudioClip menuClip, woodlandClip, lakesideClip, caveClip, meadowClip;
    static AudioClip jingleClip, jingleClipS, jingleClipL;
    AudioClip currentTrack;
    Material particleMat;
    const int Rate = 22050;
    readonly Queue<FxParticle> availableParticles = new Queue<FxParticle>(24);
    readonly List<FxParticle> allParticles = new List<FxParticle>(24);

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
        menuClip = Theme(new[] { 0, 4, 7, 12, 7, 4, 0, 4, 5, 9, 12, 9, 5, 4, 2, 0 },
            new[] { 0, 0, -3, -3, -5, -5, -3, -3 }, 0.2f, 0.3f, 0.3f);
        woodlandClip = Theme(new[] { 0, 4, 7, 4, 9, 7, 4, 2, 0, 4, 7, 12, 11, 7, 4, 2 },
            new[] { 0, 0, -3, -3, -7, -7, -5, -5 }, 0.2f, 0.25f, 0.25f);
        lakesideClip = Theme(new[] { 4, 7, 9, 7, 4, 0, 2, 5, 9, 5, 2, -3, 4, 7, 12, 7 },
            new[] { 0, 0, -5, -5, -3, -3, -7, -7 }, 0.24f, 0.8f, 0.3f);
        caveClip = Theme(new[] { 0, 3, 7, 3, 8, 7, 3, 0, 0, 3, 8, 12, 11, 8, 3, 2 },
            new[] { -12, -12, -9, -9, -14, -14, -12, -12 }, 0.16f, 0.1f, 0.12f);
        meadowClip = Theme(new[] { 7, 12, 14, 12, 7, 5, 7, 9, 12, 9, 7, 5, 2, 5, 7, 12 },
            new[] { 2, 2, 5, 5, 7, 7, 5, 5 }, 0.18f, 0.5f, 0.35f);
        jingleClip = Theme(new[] { 0, 4, 7, 12, 16, 19, 24, 19, 16, 12, 7, 12 },
            new int[12], 0.11f, 0.95f, 0.9f);
        SceneManager.sceneLoaded += OnSceneLoaded;

        particleMat = new Material(Shader.Find("Sprites/Default"));
        for (int i = 0; i < 8; i++) availableParticles.Enqueue(CreateParticle());
        SceneManager.sceneLoaded += ClearParticles;
    }

    FxParticle CreateParticle()
    {
        GameObject go = new GameObject("PooledEffect");
        go.transform.SetParent(transform, false);
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.playOnAwake = false;
        main.loop = false;
        main.stopAction = ParticleSystemStopAction.Callback;
        FxParticle effect = go.AddComponent<FxParticle>();
        effect.Owner = this;
        effect.System = ps;
        go.GetComponent<ParticleSystemRenderer>().sharedMaterial = particleMat;
        go.SetActive(false);
        allParticles.Add(effect);
        return effect;
    }

    ParticleSystem TakeParticle(Vector3 position)
    {
        FxParticle effect = availableParticles.Count > 0 ? availableParticles.Dequeue()
            : allParticles.Count < 24 ? CreateParticle() : null;
        if (effect == null) return null;
        effect.InUse = true;
        effect.transform.position = position;
        effect.gameObject.SetActive(true);
        return effect.System;
    }

    public void ReturnParticle(FxParticle effect)
    {
        if (!effect.InUse) return;
        effect.InUse = false;
        effect.System.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        effect.gameObject.SetActive(false);
        availableParticles.Enqueue(effect);
    }

    void ClearParticles(Scene scene, LoadSceneMode mode)
    {
        foreach (FxParticle effect in allParticles)
            if (effect.InUse) ReturnParticle(effect);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= ClearParticles;
        if (particleMat != null) Destroy(particleMat);
        if (instance == this) instance = null;
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

    // vòng nhạc nền ngắn: bass + giai điệu (detune = quãng nhỏ cho màu sắc khác nhau)
    static AudioClip Theme(int[] melody, int[] bass, float step, float detune, float sustain)
    {
        int perStep = (int)(Rate * step);
        int n = perStep * melody.Length;
        float[] data = new float[n];
        float pm = 0, pb = 0;
        for (int i = 0; i < n; i++)
        {
            int s = i / perStep;
            float local = (float)(i % perStep) / perStep;
            float fm = 440f * Mathf.Pow(2f, melody[s] / 12f);
            float fb = 110f * Mathf.Pow(2f, (bass.Length > 0 ? bass[s / 2 % bass.Length] : melody[s] / 2) / 12f);
            pm += fm / Rate;
            pb += fb / Rate;
            float env = 1f - local;
            float m = (Mathf.Repeat(pm, 1f) < 0.25f ? 1f : -1f) * Mathf.Lerp(env, 1f, 1f - sustain) * 0.15f;
            float b = (Mathf.Repeat(pb, 1f) < 0.5f ? 1f : -1f) * 0.12f;
            data[i] = m + b;
        }
        AudioClip c = AudioClip.Create("theme", n, 1, Rate, false);
        c.SetData(data, 0);
        return c;
    }

    static AudioClip Music() => Theme(new[] { 0, 4, 7, 4, 9, 7, 4, 2, 0, 4, 7, 12, 11, 7, 4, 2 },
        new[] { 0, 0, -3, -3, -7, -7, -5, -5 }, 0.2f, 0.25f, 0.25f);

    // chọn nhạc theo scene: menu 1 bài, mỗi màn 1 màu âm riêng, mini-game dùng bài menu
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        int level = AdventureProgress.LevelNumber(scene.name);
        AudioClip want = scene.buildIndex == 0 || level <= 0 ? menuClip
            : level == 1 ? woodlandClip
            : level == 2 ? lakesideClip
            : level == 3 ? caveClip
            : level == 4 ? meadowClip : menuClip;
        PlayTrack(want);
    }

    static void PlayTrack(AudioClip clip)
    {
        if (instance == null || clip == null || instance.currentTrack == clip) return;
        instance.currentTrack = clip;
        instance.music.clip = clip;
        instance.music.Play();
    }

    // jingle chiến thắng ngắn (dùng khi boss đầu hàng / qua màn)
    public static void WinJingle(bool big)
    {
        if (instance == null) return;
        if (big && jingleClipL == null) jingleClipL = Theme(
            new[] { 0, 4, 7, 12, 16, 19, 24, 19, 16, 12, 19, 24 }, new int[12], 0.13f, 0.95f, 0.85f);
        Play(big ? jingleClipL : (jingleClipS ?? (jingleClipS = Theme(
            new[] { 0, 4, 7, 12, 16, 12, 7, 12 }, new int[12], 0.12f, 0.95f, 0.85f))), 0.9f);
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
        ParticleSystem ps = instance.TakeParticle(pos);
        if (ps == null) return;
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
        main.stopAction = ParticleSystemStopAction.Callback;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.1f;
        var sizeOverLife = ps.sizeOverLifetime;
        sizeOverLife.enabled = false;
        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(color, 0), new GradientColorKey(color, 1) },
                  new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1) });
        col.color = g;

        ParticleSystemRenderer r = ps.GetComponent<ParticleSystemRenderer>();
        r.sharedMaterial = instance.particleMat;
        r.sortingOrder = 50;

        ps.Play();
        ps.Emit(count);
    }

    // luồng khí xịt từ bình xịt phun ra phía trước
    public static void Spray(Vector3 pos, bool right)
    {
        if (instance == null) return;
        Play(instance.sprayClip, 0.5f);
        ParticleSystem ps = instance.TakeParticle(pos + new Vector3(right ? 0.6f : -0.6f, 0.15f, 0));
        if (ps == null) return;
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.loop = false;
        main.duration = 0.4f;
        main.gravityModifier = 0;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.Callback;
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
        ParticleSystemRenderer r = ps.GetComponent<ParticleSystemRenderer>();
        r.sharedMaterial = instance.particleMat;
        r.sortingOrder = 40;
        float dir = right ? 1f : -1f;
        ps.Play();
        for (int i = 0; i < 28; i++)
        {
            ParticleSystem.EmitParams e = new ParticleSystem.EmitParams();
            e.velocity = new Vector3(dir * Random.Range(3f, 6f), Random.Range(-0.8f, 0.8f), 0);
            e.startLifetime = Random.Range(0.25f, 0.4f);
            e.startSize = Random.Range(0.15f, 0.3f);
            e.startColor = new Color(0.75f, 1f, 0.8f);
            ps.Emit(e, 1);
        }
    }

    // khựng hình rất ngắn khi đánh trúng cho có lực
    public static void HitStop(float seconds = 0.035f)
    {
        if (instance == null || !GameSession.IsPlaying || Time.timeScale == 0) return;
        Hit();
        instance.StartCoroutine(instance.DoHitStop(seconds));
    }

    System.Collections.IEnumerator DoHitStop(float seconds)
    {
        int sceneVersion = GameSession.SceneVersion;
        Time.timeScale = 0.35f;
        yield return new WaitForSecondsRealtime(seconds);
        if (sceneVersion == GameSession.SceneVersion && GameSession.IsPlaying && Time.timeScale > 0)
            Time.timeScale = 1;
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
            if (t == null) yield break;
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
