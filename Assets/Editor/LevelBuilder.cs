using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Sắp xếp lại các màn chơi:
// - Màn 1: đưa người chơi về điểm xuất phát
// - Màn 2: thêm cửa ra ở cuối, mở rộng giới hạn camera
// - Màn 3 (Scene 4): dựng màn mới từ các khối có sẵn
public static class LevelBuilder
{
    const string S1 = "Assets/Scenes/Scene 1.unity";
    const string S2 = "Assets/Scenes/Scene 2.unity";
    const string S4 = "Assets/Scenes/Scene 4.unity";
    const string Menu = "Assets/Scenes/Scene 3.unity";

    public static void Run()
    {
        FixScene1();
        FixScene2();
        BuildScene4();
        EditorBuildSettings.scenes = new[] {
            new EditorBuildSettingsScene(Menu, true),
            new EditorBuildSettingsScene(S1, true),
            new EditorBuildSettingsScene(S2, true),
            new EditorBuildSettingsScene(S4, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("LevelBuilder: done");
    }

    static GameObject Find(string name)
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            if (root.name == name) return root;
        Debug.LogError("LevelBuilder: missing " + name);
        return null;
    }

    static void PlaceOn(GameObject go, float x, float groundTop)
    {
        go.transform.position = new Vector3(x, groundTop, go.transform.position.z);
        Renderer r = go.GetComponent<Renderer>();
        if (r != null)
            go.transform.position += new Vector3(0, groundTop - r.bounds.min.y + 0.02f, 0);
    }

    static void SetBounds(float minX, float minY, float maxX, float maxY)
    {
        CameraFollow cf = Object.FindAnyObjectByType<CameraFollow>();
        cf.minipos = new Vector2(minX, minY);
        cf.maxpos = new Vector2(maxX, maxY);
        cf.bound = true;
        EditorUtility.SetDirty(cf);
    }

    // ---------- Màn 1 ----------
    static void FixScene1()
    {
        EditorSceneManager.OpenScene(S1);
        GameObject player = Find("Player");
        // nền dài ở đầu màn
        GameObject ground = Find("Ground");
        Bounds gb = ground.GetComponent<Renderer>().bounds;
        PlaceOn(player, gb.min.x + 1.5f, gb.max.y);
        EditorUtility.SetDirty(player.transform);
        // nới giới hạn trái của camera để thấy được điểm xuất phát
        Camera cam = Camera.main;
        float halfW = cam.orthographicSize * cam.aspect;
        CameraFollow cf = Object.FindAnyObjectByType<CameraFollow>();
        SetBounds(Mathf.Min(cf.minipos.x, gb.min.x + halfW), cf.minipos.y, cf.maxpos.x, cf.maxpos.y);
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Debug.Log("LevelBuilder: Scene 1 player start " + player.transform.position);
    }

    // ---------- Màn 2 ----------
    static void FixScene2()
    {
        EditorSceneManager.OpenScene(S2);
        Scene s2 = SceneManager.GetActiveScene();
        // lấy cửa từ màn 1
        Scene s1 = EditorSceneManager.OpenScene(S1, OpenSceneMode.Additive);
        GameObject doorT = s1.GetRootGameObjects().First(g => g.name == "door");

        // platform cuối cùng trên đường đi (khối xa nhất bên phải là bức tường, không lên được)
        Renderer last = s2.GetRootGameObjects().First(g => g.name == "Ground (13)").GetComponent<Renderer>();
        SceneManager.SetActiveScene(s2);
        GameObject door = (GameObject)Object.Instantiate(doorT);
        door.name = "door";
        SceneManager.MoveGameObjectToScene(door, s2);
        PlaceOn(door, last.bounds.max.x - 1.5f, last.bounds.max.y);
        EditorSceneManager.CloseScene(s1, true);

        CameraFollow cf = Object.FindAnyObjectByType<CameraFollow>();
        SetBounds(cf.minipos.x, cf.minipos.y, door.transform.position.x, cf.maxpos.y);
        EditorSceneManager.SaveScene(s2);
        Debug.Log("LevelBuilder: Scene 2 door at " + door.transform.position);
    }

    // ---------- Màn 3 ----------
    static GameObject solidT, oneWayT, spikeT, coinT, enemyT, turretT, heartT, shoesT, movingT, killT, fallT, doorT;
    static Scene target;

    static GameObject Clone(GameObject t)
    {
        GameObject g = (GameObject)Object.Instantiate(t);
        g.name = t.name;
        SceneManager.MoveGameObjectToScene(g, target);
        g.SetActive(true);
        return g;
    }

    // khối nền đặc từ x0 tới x1, mặt trên ở top, dày depth
    static GameObject Ground(float x0, float x1, float top, float depth = 6f, bool oneWay = false)
    {
        GameObject g = Clone(oneWay ? oneWayT : solidT);
        g.transform.localScale = Vector3.one;
        g.transform.rotation = Quaternion.identity;
        SpriteRenderer sr = g.GetComponent<SpriteRenderer>();
        Vector2 size = new Vector2(x1 - x0, depth);
        if (sr.drawMode == SpriteDrawMode.Simple) sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = size;
        BoxCollider2D bc = g.GetComponent<BoxCollider2D>();
        bc.autoTiling = false;
        bc.size = size;
        bc.offset = Vector2.zero;
        if (!oneWay)
        {
            // khối đặc: chặn cả cạnh bên, để platform di động dội lại
            bc.usedByEffector = false;
            PlatformEffector2D pe = g.GetComponent<PlatformEffector2D>();
            if (pe != null) Object.DestroyImmediate(pe);
            g.tag = "Ground";
        }
        g.transform.position = new Vector3((x0 + x1) / 2f, top - depth / 2f, 0);
        return g;
    }

    static void SpikeRow(float x0, float x1, float top)
    {
        float w = spikeT.GetComponent<Renderer>().bounds.size.x;
        for (float x = x0 + w / 2; x <= x1 - w / 2 + 0.01f; x += w)
            PlaceOn(Clone(spikeT), x, top);
    }

    static void Coins(float x0, float x1, float y, int n)
    {
        for (int i = 0; i < n; i++)
        {
            GameObject c = Clone(coinT);
            float x = n == 1 ? x0 : Mathf.Lerp(x0, x1, (float)i / (n - 1));
            c.transform.position = new Vector3(x, y, 0);
        }
    }

    static void Enemy(float x, float y)
    {
        Clone(enemyT).transform.position = new Vector3(x, y, 0);
    }

    static void Turret(float x, float top)
    {
        GameObject t = Clone(turretT);
        PlaceOn(t, x, top);
    }

    static void BuildScene4()
    {
        AssetDatabase.DeleteAsset(S4);
        AssetDatabase.CopyAsset(S2, S4);
        target = EditorSceneManager.OpenScene(S4);
        Scene s1 = EditorSceneManager.OpenScene(S1, OpenSceneMode.Additive);
        SceneManager.SetActiveScene(target);

        string[] keep = { "Main Camera", "Canvas", "EventSystem", "GameMaster", "Sounds", "Player" };
        List<GameObject> originals = target.GetRootGameObjects().Where(g => !keep.Contains(g.name)).ToList();
        GameObject R(string n) => target.GetRootGameObjects().First(g => g.name == n);

        solidT = R("Ground (3)");
        oneWayT = R("Ground (10)");
        spikeT = R("Spikes (1)");
        coinT = R("Coin (12)");
        enemyT = target.GetRootGameObjects().First(g => g.GetComponent<global::Enemy>() != null);
        turretT = R("Turret");
        heartT = R("Heart (1)");
        shoesT = R("shoes");
        movingT = R("MovingPlat (1)");
        killT = R("Worl_end");
        fallT = s1.GetRootGameObjects().First(g => g.name == "Fallingplat (1)");
        doorT = s1.GetRootGameObjects().First(g => g.name == "door");

        // --- A. Khởi động (0–20)
        Ground(-4, 20, 0);
        Coins(6, 18, 1f, 5);
        Ground(-6, -4, 8, 14);                 // tường trái

        // --- B. Hố gai nhỏ (20–23), hố vừa (40–43)
        Ground(23, 40, 0);
        Enemy(34, 2.5f);
        Coins(26, 38, 1f, 4);
        Ground(43, 49, 0);

        // --- C. Cầu thang lên + tháp pháo (49–64)
        Ground(49, 52, 0.75f);
        Ground(52, 55, 1.5f);
        Ground(55, 58, 2.25f);
        Ground(58, 64, 3);
        Turret(61, 3);
        Coins(50.5f, 56.5f, 3.5f, 3);

        // --- D. Platform sụt qua hố gai (64–80)
        float[] fx = { 67, 71, 75 };
        foreach (float x in fx)
        {
            GameObject f = Clone(fallT);
            f.transform.position = new Vector3(x, 2.8f, 0);
        }
        Coins(67, 75, 4f, 3);

        // --- E. Bãi gián (80–100)
        Ground(80, 100, 3);
        Enemy(88, 5.5f);
        Enemy(95, 6f);
        PlaceOn(Clone(heartT), 98, 3.3f);
        Coins(82, 92, 4f, 4);

        // --- F. Platform di động qua hố lớn (100–124)
        GameObject mp = Clone(movingT);
        Bounds mb = mp.GetComponent<Renderer>().bounds;
        mp.transform.position = new Vector3(102.5f + mb.extents.x, 3 - mb.extents.y, 0);
        Ground(124, 132, 3);
        Coins(106, 120, 5f, 5);

        // --- G. Xuống dốc, giày tăng tốc (132–150)
        Ground(132, 134, 2.25f);
        Ground(134, 136, 1.5f);
        Ground(136, 138, 0.75f);
        Ground(138, 150, 0);
        PlaceOn(Clone(shoesT), 143, 0.3f);

        // --- H. Bậc nhảy một chiều (150–160): đi dưới hoặc nhảy lên lấy xu
        Ground(150, 172, 0);
        Ground(152, 156, 1.2f, 0.4f, true);
        Ground(157, 161, 2.4f, 0.4f, true);
        Ground(162, 166, 3.6f, 0.4f, true);
        Coins(163, 165, 4.6f, 3);

        // --- I. Hành lang tháp pháo (172–205)
        Ground(175, 205, 0);
        Turret(182, 0);
        Enemy(190, 2.5f);
        Turret(198, 0);
        PlaceOn(Clone(heartT), 186, 0.3f);
        Coins(176, 204, 1f, 8);

        // --- J. Cửa đích (205–220)
        Ground(205, 208, 0.75f);
        Ground(208, 220, 1.5f);
        Ground(220, 222, 10, 14);              // tường phải
        GameObject door = Clone(doorT);
        door.name = "door";
        PlaceOn(door, 216, 1.5f);

        // hố gai: sàn tử thần dọc cả màn
        GameObject kill = Clone(killT);
        Bounds kb = kill.GetComponent<Renderer>().bounds;
        Vector3 ks = kill.transform.localScale;
        ks.x *= 240f / Mathf.Max(kb.size.x, 0.01f);
        kill.transform.localScale = ks;
        kill.transform.position = new Vector3(108, -5.5f, 0);
        SpikeRow(20, 23, -0.6f);
        SpikeRow(40, 43, -0.6f);
        SpikeRow(64, 80, -2f);
        SpikeRow(100, 124, -2f);
        SpikeRow(172, 175, -0.6f);
        Ground(20, 23, -0.6f, 5.4f);
        Ground(40, 43, -0.6f, 5.4f);
        Ground(64, 80, -2f, 4f);
        Ground(100, 124, -2f, 4f);
        Ground(172, 175, -0.6f, 5.4f);

        // xóa khối cũ của màn 2
        foreach (GameObject g in originals) Object.DestroyImmediate(g);
        EditorSceneManager.CloseScene(s1, true);

        PlaceOn(Find("Player"), 1.5f, 0);
        SetBounds(6, -2, 214, 8);
        EditorSceneManager.SaveScene(target);
        LevelTools.Render("LevelDump/Scene 4.png");
        Debug.Log("LevelBuilder: Scene 4 built");
    }
}
