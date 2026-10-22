using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Bounded scene decoration; transforms with colliders are never animated here.
public class ScenePolish : MonoBehaviour
{
    struct Pickup { public SpriteRenderer Body; public Transform Sparkle; public float Phase; }
    readonly List<Pickup> pickups = new List<Pickup>();
    readonly List<SpriteRenderer> water = new List<SpriteRenderer>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        QualitySettings.vSyncCount = Application.isMobilePlatform ? 0 : 1;
        Application.targetFrameRate = Application.isMobilePlatform ? 60 : -1;
        UnityEngine.Rendering.OnDemandRendering.renderFrameInterval = 1;
        SceneManager.sceneLoaded -= Setup;
        SceneManager.sceneLoaded += Setup;
    }

    static void Setup(Scene scene, LoadSceneMode mode)
    {
        if (AdventureProgress.LevelNumber(scene.name) == 0) return;
        new GameObject("ScenePolish").AddComponent<ScenePolish>().Build();
        Player player = FindAnyObjectByType<Player>();
        if (player != null) player.gameObject.AddComponent<PlayerVisuals>();
        foreach (Enemy enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None)) enemy.gameObject.AddComponent<EnemyVisuals>();
        foreach (TurretAI turret in FindObjectsByType<TurretAI>(FindObjectsSortMode.None)) turret.gameObject.AddComponent<EnemyVisuals>();
    }

    void Build()
    {
        foreach (SpriteRenderer sr in FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
        {
            if (sr.CompareTag("Coins") || sr.CompareTag("heart") || sr.CompareTag("shoes"))
            {
                GameObject go = new GameObject("PickupGlint");
                go.transform.SetParent(sr.transform, false);
                SpriteRenderer glint = go.AddComponent<SpriteRenderer>();
                glint.sprite = CuteArt.Sparkle;
                glint.sortingOrder = sr.sortingOrder + 1;
                pickups.Add(new Pickup { Body = sr, Sparkle = go.transform, Phase = sr.transform.position.x * 0.7f });
            }
            if (sr.CompareTag("water"))
            {
                Bounds b = sr.bounds;
                sr.color = new Color(0.3f, 0.8f, 0.88f, 0.4f);
                GameObject go = new GameObject("WaterSurface");
                go.transform.SetParent(transform, false);
                go.transform.position = new Vector3(b.center.x, b.max.y, 0);
                SpriteRenderer surface = go.AddComponent<SpriteRenderer>();
                surface.sprite = CuteArt.Wave;
                surface.drawMode = SpriteDrawMode.Tiled;
                surface.size = new Vector2(b.size.x, 0.15f);
                surface.sortingOrder = sr.sortingOrder + 1;
                water.Add(surface);
            }
        }
    }

    void Update()
    {
        float time = Time.time;
        foreach (Pickup pickup in pickups)
        {
            if (pickup.Body == null || pickup.Sparkle == null || !pickup.Body.isVisible) continue;
            float phase = time * 2 + pickup.Phase;
            pickup.Sparkle.localPosition = new Vector3(0.25f, 0.5f + Mathf.Sin(phase) * 0.07f, 0);
            pickup.Sparkle.localScale = Vector3.one * (0.2f + Mathf.Abs(Mathf.Sin(phase)) * 0.15f);
        }
        for (int i = 0; i < water.Count; i++)
            water[i].color = new Color(1, 1, 1, 0.65f + Mathf.Sin(time * 1.3f + i) * 0.12f);
    }
}
