using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Làm đẹp các màn: nền rừng parallax + thay gạch nền bằng cỏ/đất
public static class LevelStyler
{
    const string Grass = "Assets/Graphic/Tile_Grass.png";
    const string Dirt = "Assets/Graphic/Tile_Dirt.png";
    const string Forest = "Assets/Free Pixel Art Forest/PNG/Background layers";

    public static void Run()
    {
        Import(Grass, 16);
        Import(Dirt, 16);
        string[] layerPaths = AssetDatabase.FindAssets("t:Texture2D", new[] { Forest })
            .Select(AssetDatabase.GUIDToAssetPath).OrderByDescending(p => p).ToArray(); // _0011 (xa) -> _0000 (gần)
        foreach (string p in layerPaths) Import(p, 100);

        Sprite grass = AssetDatabase.LoadAssetAtPath<Sprite>(Grass);
        Sprite dirt = AssetDatabase.LoadAssetAtPath<Sprite>(Dirt);
        Sprite[] layers = layerPaths.Select(p => AssetDatabase.LoadAssetAtPath<Sprite>(p)).ToArray();

        Style("Assets/Scenes/Scene 1.unity", layers, Color.white, Color.white, grass, dirt);
        Style("Assets/Scenes/Scene 2.unity", layers, new Color(0.8f, 0.9f, 1f), Color.white, grass, dirt);
        Style("Assets/Scenes/Scene 4.unity", layers, new Color(0.55f, 0.5f, 0.65f), new Color(0.7f, 0.7f, 0.8f), grass, dirt);
        Debug.Log("LevelStyler: done");
    }

    // làm đẹp lại riêng màn 1 (sau khi dựng lại bố cục)
    public static void StyleLevel1()
    {
        Sprite grass = AssetDatabase.LoadAssetAtPath<Sprite>(Grass);
        Sprite dirt = AssetDatabase.LoadAssetAtPath<Sprite>(Dirt);
        Sprite[] layers = AssetDatabase.FindAssets("t:Texture2D", new[] { Forest })
            .Select(AssetDatabase.GUIDToAssetPath).OrderByDescending(p => p)
            .Select(p => AssetDatabase.LoadAssetAtPath<Sprite>(p)).ToArray();
        Style("Assets/Scenes/Scene 1.unity", layers, Color.white, Color.white, grass, dirt);
        FixBackgroundColor();
    }

    // làm đẹp lại riêng màn 2 (sau khi dựng lại bố cục)
    public static void StyleLevel2()
    {
        Sprite grass = AssetDatabase.LoadAssetAtPath<Sprite>(Grass);
        Sprite dirt = AssetDatabase.LoadAssetAtPath<Sprite>(Dirt);
        Sprite[] layers = AssetDatabase.FindAssets("t:Texture2D", new[] { Forest })
            .Select(AssetDatabase.GUIDToAssetPath).OrderByDescending(p => p)
            .Select(p => AssetDatabase.LoadAssetAtPath<Sprite>(p)).ToArray();
        Style("Assets/Scenes/Scene 2.unity", layers, new Color(0.8f, 0.9f, 1f), Color.white, grass, dirt);
        FixBackgroundColor();
    }

    // chỉ đặt lại màu nền camera (không dựng lại gạch)
    public static void FixBackgroundColor()
    {
        foreach (string s in new[] { "Assets/Scenes/Scene 1.unity", "Assets/Scenes/Scene 2.unity", "Assets/Scenes/Scene 4.unity" })
        {
            Scene scene = EditorSceneManager.OpenScene(s);
            Camera.main.backgroundColor = new Color(0.09f, 0.1f, 0.14f);
            EditorSceneManager.SaveScene(scene);
        }
    }

    static void Import(string path, int ppu)
    {
        TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(path);
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spritePixelsPerUnit = ppu;
        ti.filterMode = FilterMode.Point;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.wrapMode = TextureWrapMode.Repeat;
        TextureImporterSettings s = new TextureImporterSettings();
        ti.ReadTextureSettings(s);
        s.spriteMeshType = SpriteMeshType.FullRect;
        ti.SetTextureSettings(s);
        ti.SaveAndReimport();
    }

    static void Style(string scenePath, Sprite[] layers, Color bgTint, Color groundTint, Sprite grass, Sprite dirt)
    {
        Scene scene = EditorSceneManager.OpenScene(scenePath);
        Camera cam = Camera.main;

        // tắt nền cũ, gắn nền parallax
        ScrollBG old = cam.GetComponentInChildren<ScrollBG>(true);
        if (old != null) old.gameObject.SetActive(false);
        ParallaxBackground pb = cam.GetComponent<ParallaxBackground>();
        if (pb == null) pb = cam.gameObject.AddComponent<ParallaxBackground>();
        pb.layers = layers;
        pb.tint = bgTint;
        // màu nền trùng màu đất tối ở chân ảnh rừng, để mép dưới không lộ dải trời
        cam.backgroundColor = new Color(0.09f, 0.1f, 0.14f);

        // gạch nền: ẩn sprite cũ, vẽ đất + viền cỏ theo đúng khung collider
        GameObject visuals = scene.GetRootGameObjects().FirstOrDefault(g => g.name == "GroundVisuals");
        if (visuals != null) Object.DestroyImmediate(visuals);
        visuals = new GameObject("GroundVisuals");
        int count = 0;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (!root.name.StartsWith("Ground") || root == visuals) continue;
            SpriteRenderer sr = root.GetComponent<SpriteRenderer>();
            if (sr == null || root.GetComponent<BoxCollider2D>() == null) continue;
            Bounds b = sr.bounds;
            sr.enabled = false;

            GameObject v = new GameObject(root.name);
            v.transform.SetParent(visuals.transform, false);
            v.transform.position = new Vector3(b.center.x, b.center.y, 0);
            float grassH = Mathf.Min(1f, b.size.y);
            if (b.size.y > grassH + 0.01f)
            {
                SpriteRenderer d = v.AddComponent<SpriteRenderer>();
                d.sprite = dirt;
                d.drawMode = SpriteDrawMode.Tiled;
                d.size = new Vector2(b.size.x, b.size.y);
                d.color = groundTint;
                d.sortingOrder = sr.sortingOrder;
            }
            GameObject g = new GameObject("Grass");
            g.transform.SetParent(v.transform, false);
            g.transform.localPosition = new Vector3(0, b.extents.y - grassH / 2, 0);
            SpriteRenderer gs = g.AddComponent<SpriteRenderer>();
            gs.sprite = grass;
            gs.drawMode = SpriteDrawMode.Tiled;
            gs.size = new Vector2(b.size.x, grassH);
            gs.color = groundTint;
            gs.sortingOrder = sr.sortingOrder + 1;
            count++;
        }
        EditorSceneManager.SaveScene(scene);
        Debug.Log("LevelStyler: " + scenePath + " restyled " + count + " grounds");
    }
}
