using UnityEngine;
using UnityEngine.SceneManagement;

// Cosmetic only: no colliders, input, progress or random gameplay state is changed.
public class SunnyForest : MonoBehaviour
{
    Camera cam;
    SpriteRenderer background;
    readonly Transform[] clouds = new Transform[4];
    readonly Transform[] butterflies = new Transform[9];
    float clock;
    bool cave;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Scene 3" && AdventureProgress.LevelNumber(scene.name) == 0) return;
        if (Camera.main == null) return;
        new GameObject("SunnyForest").AddComponent<SunnyForest>().Build(Camera.main, scene.name);
    }

    void Build(Camera camera, string sceneName)
    {
        cam = camera;
        cave = sceneName == "Scene 4";
        Texture2D texture = Resources.Load<Texture2D>(cave ? "CuteCave" : sceneName == "Scene 2" ? "CuteLake" : "CuteForest");
        if (texture == null) { enabled = false; return; }
        texture.filterMode = FilterMode.Point;
        ParallaxBackground old = cam.GetComponent<ParallaxBackground>();
        if (old != null) old.enabled = false;
        Transform previous = cam.transform.Find("Parallax");
        if (previous != null) previous.gameObject.SetActive(false);
        cam.backgroundColor = new Color32(171, 226, 222, 255);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f, 100);
        background = Make("ForestBackdrop", sprite, -200);
        // Camera-local transforms follow Cinemachine's final pose automatically,
        // regardless of which LateUpdate callback executes first.
        background.transform.SetParent(cam.transform, false);
        background.transform.localPosition = new Vector3(0, 0, 150);
        for (int i = 0; i < clouds.Length && !cave; i++)
        {
            SpriteRenderer sr = Make("DriftingCloud", CuteArt.Cloud, -180);
            sr.color = new Color(1, 1, 1, 0.7f);
            clouds[i] = sr.transform;
            clouds[i].SetParent(cam.transform, false);
        }
        for (int i = 0; i < butterflies.Length; i++)
        {
            SpriteRenderer sr = Make(cave ? "CrystalMote" : "Butterfly", cave ? CuteArt.Sparkle : CuteArt.Butterfly, -20);
            sr.color = i % 3 == 0 ? new Color(1f, 0.88f, 0.6f) : i % 3 == 1 ? new Color(1f, 0.7f, 0.83f) : Color.white;
            butterflies[i] = sr.transform;
            butterflies[i].SetParent(cam.transform, false);
        }
        if (AdventureProgress.LevelNumber(sceneName) > 0) DecorateGround();
        LateUpdate();
    }

    SpriteRenderer Make(string name, Sprite sprite, int order)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;
        return sr;
    }

    void LateUpdate()
    {
        if (background == null || cam == null) return;
        clock += Time.deltaTime;
        float h = cam.orthographicSize * 2, w = h * cam.aspect;
        float scale = Mathf.Max(w / background.sprite.bounds.size.x, h / background.sprite.bounds.size.y) * 1.08f;
        Vector3 parentScale = cam.transform.lossyScale;
        background.transform.localScale = new Vector3(scale / Mathf.Abs(parentScale.x), scale / Mathf.Abs(parentScale.y), 1);
        for (int i = 0; i < clouds.Length; i++)
        {
            if (clouds[i] == null) continue;
            float x = Mathf.Repeat(i * w / clouds.Length + clock * 0.12f, w + 6) - (w + 6) * 0.5f;
            clouds[i].localPosition = new Vector3(x, h * (0.25f + i % 2 * 0.12f), 100);
            clouds[i].localScale = Vector3.one * (0.7f + i * 0.12f);
        }
        for (int i = 0; i < butterflies.Length; i++)
        {
            float x = Mathf.Repeat(i * 3.71f + clock * 0.2f, w + 2) - (w + 2) * 0.5f;
            float y = Mathf.Sin(clock * 0.7f + i * 2.3f) * h * 0.13f - h * 0.1f;
            butterflies[i].localPosition = new Vector3(x, y, 30);
            butterflies[i].localScale = new Vector3(0.35f + Mathf.Abs(Mathf.Sin(clock * 6 + i)) * 0.4f, 0.6f, 1);
        }
    }

    void DecorateGround()
    {
        GameObject visuals = GameObject.Find("GroundVisuals");
        if (visuals == null) return;
        foreach (SpriteRenderer sr in visuals.GetComponentsInChildren<SpriteRenderer>())
        {
            Vector2 size = sr.size;
            sr.sprite = cave ? (sr.name == "Grass" ? CuteArt.CaveRim : CuteArt.Stone)
                : sr.name == "Grass" ? CuteArt.Grass : CuteArt.Dirt;
            sr.size = size;
            sr.color = Color.white;
            if (sr.name != "Grass" || sr.bounds.size.x < 5) continue;
            Bounds b = sr.bounds;
            int count = Mathf.Min(12, Mathf.FloorToInt(b.size.x / 3));
            for (int i = 0; i < count; i++)
            {
                float x = Mathf.Lerp(b.min.x + 1, b.max.x - 1, (i + 0.5f) / count);
                SpriteRenderer flower = Make(cave ? "SmallCrystal" : "Wildflower", cave ? CuteArt.Crystal : CuteArt.Flower, sr.sortingOrder + 1);
                flower.transform.position = new Vector3(x, b.max.y + 0.13f, 0);
                flower.transform.localScale = Vector3.one * 0.4f;
                if (i % 2 == 0) flower.color = new Color(1f, 0.94f, 0.72f);
            }
        }
    }
}
