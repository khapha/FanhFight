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
        sr.enabled = true; // khối mẫu có thể đã bị LevelStyler ẩn
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

    // Dựng lại màn 1: bố cục cũ có nhiều cột đất cao ~10 đơn vị, nhảy không qua được
    public static void RebuildLevel1()
    {
        target = EditorSceneManager.OpenScene(S1);
        Scene s2 = EditorSceneManager.OpenScene(S2, OpenSceneMode.Additive);
        SceneManager.SetActiveScene(target);

        string[] keep = { "Main Camera", "Canvas", "EventSystem", "GameMaster", "Sounds", "Player" };
        List<GameObject> originals = target.GetRootGameObjects().Where(g => !keep.Contains(g.name)).ToList();
        GameObject R2(string n) => s2.GetRootGameObjects().First(g => g.name == n);
        GameObject R1(string n) => target.GetRootGameObjects().First(g => g.name == n);

        solidT = R2("Ground (3)");
        oneWayT = R2("Ground (10)");
        spikeT = R2("Spikes (1)");
        coinT = R2("Coin (12)");
        enemyT = s2.GetRootGameObjects().First(g => g.GetComponent<global::Enemy>() != null);
        turretT = R2("Turret");
        heartT = R2("Heart (1)");
        shoesT = R2("shoes");
        movingT = R2("MovingPlat (1)");
        killT = R2("Worl_end");
        fallT = R1("Fallingplat (1)");
        doorT = R1("door");

        // --- A. Khởi động: đất bằng, một cụm gai nhỏ để tập nhảy (0–26)
        Ground(-6, -4, 8, 14);                 // tường trái
        Ground(-4, 26, 0);
        Coins(4, 10, 1f, 4);
        SpikeRow(14, 15.2f, 0);
        Coins(17, 24, 1f, 4);

        // --- B. Con gián đầu tiên + bậc một chiều có xu (26–46)
        Ground(26, 46, 0);
        Enemy(38, 2.5f);
        Ground(30, 34, 1.2f, 0.4f, true);
        Coins(30.5f, 33.5f, 2.2f, 3);

        // --- C. Hố gai nhỏ, rồi cầu thang lên (46–72)
        SpikeRow(46, 49, -0.6f);
        Ground(46, 49, -0.6f, 5.4f);
        Ground(49, 60, 0);
        PlaceOn(Clone(heartT), 56, 0.3f);
        Ground(60, 63, 0.75f);
        Ground(63, 66, 1.5f);
        Ground(66, 84, 2.25f);
        Coins(61, 65, 2.5f, 3);
        Enemy(78, 4.5f);

        // --- D. Platform sụt qua hố (84–96)
        foreach (float x in new[] { 86.5f, 90f, 93.5f })
            Clone(fallT).transform.position = new Vector3(x, 2.05f, 0);
        SpikeRow(84, 96, -2f);
        Ground(84, 96, -2f, 4f);
        Coins(86.5f, 93.5f, 3.3f, 3);

        // --- E. Bãi gián + giày tăng tốc (96–120)
        Ground(96, 120, 2.25f);
        Enemy(104, 4.5f);
        Enemy(113, 5f);
        PlaceOn(Clone(shoesT), 108, 2.55f);
        Coins(98, 118, 3.3f, 6);

        // --- F. Xuống dốc, tháp pháo đầu tiên (120–150)
        Ground(120, 122, 1.5f);
        Ground(122, 124, 0.75f);
        Ground(124, 150, 0);
        PlaceOn(Clone(heartT), 128, 0.3f);
        Turret(142, 0);
        Coins(130, 138, 1f, 4);

        // --- G. Platform di động qua hố (150–166)
        GameObject mp = Clone(movingT);
        Bounds mb = mp.GetComponent<Renderer>().bounds;
        mp.transform.position = new Vector3(150.5f + mb.extents.x, 0 - mb.extents.y, 0);
        SpikeRow(150, 166, -3f);
        Ground(150, 166, -3f, 3f);
        Coins(153, 163, 1.5f, 4);

        // --- H. Về đích (166–196)
        Ground(166, 186, 0);
        Enemy(176, 2.5f);
        Coins(168, 184, 1f, 6);
        Ground(186, 189, 0.75f);
        Ground(189, 198, 1.5f);
        Ground(198, 200, 10, 14);              // tường phải
        GameObject door = Clone(doorT);
        door.name = "door";
        PlaceOn(door, 194, 1.5f);

        GameObject kill = Clone(killT);
        Bounds kb = kill.GetComponent<Renderer>().bounds;
        Vector3 ks = kill.transform.localScale;
        ks.x *= 220f / Mathf.Max(kb.size.x, 0.01f);
        kill.transform.localScale = ks;
        kill.transform.position = new Vector3(97, -5.5f, 0);

        foreach (GameObject g in originals) Object.DestroyImmediate(g);
        EditorSceneManager.CloseScene(s2, true);

        PlaceOn(Find("Player"), 1.5f, 0);
        SetBounds(6, -2, 190, 8);
        EditorSceneManager.SaveScene(target);
        Debug.Log("LevelBuilder: Scene 1 rebuilt");
    }

    static GameObject waterT;

    // hồ nước từ x0 tới x1, mặt nước ở top, đáy ở bottom
    static void Water(float x0, float x1, float top, float bottom)
    {
        // khối nước mẫu là object kiểu UI (RectTransform) nên không đặt vị trí được;
        // tạo object mới, chỉ chép sprite và thông số lực đẩy
        SpriteRenderer tsr = waterT.GetComponent<SpriteRenderer>();
        AreaEffector2D tae = waterT.GetComponent<AreaEffector2D>();
        GameObject w = new GameObject("Water");
        SceneManager.MoveGameObjectToScene(w, target);
        w.tag = "water";
        w.transform.position = new Vector3((x0 + x1) / 2f, (top + bottom) / 2f, 0);
        SpriteRenderer sr = w.AddComponent<SpriteRenderer>();
        sr.sprite = tsr.sprite;
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = new Vector2(x1 - x0, top - bottom);
        // nước trong suốt, vẽ trước nhân vật cho có cảm giác đang lặn
        Color c = tsr.color;
        c.a = 0.55f;
        sr.color = c;
        sr.sortingOrder = 20;
        BoxCollider2D bc = w.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.usedByEffector = true;
        bc.size = new Vector2(x1 - x0, top - bottom);
        AreaEffector2D ae = w.AddComponent<AreaEffector2D>();
        EditorUtility.CopySerialized(tae, ae);
    }

    // Dựng lại màn 2: mặt nước cũ thấp hơn các platform 7–11 đơn vị, không lên tới cửa được
    public static void RebuildLevel2()
    {
        target = EditorSceneManager.OpenScene(S2);
        Scene s1 = EditorSceneManager.OpenScene(S1, OpenSceneMode.Additive);
        SceneManager.SetActiveScene(target);

        string[] keep = { "Main Camera", "Canvas", "EventSystem", "GameMaster", "Sounds", "Player" };
        List<GameObject> originals = target.GetRootGameObjects().Where(g => !keep.Contains(g.name)).ToList();
        GameObject R2(string n) => target.GetRootGameObjects().First(g => g.name == n);
        GameObject R1(string n) => s1.GetRootGameObjects().First(g => g.name == n);

        solidT = R2("Ground (3)");
        oneWayT = R2("Ground (10)");
        spikeT = R2("Spikes (1)");
        coinT = R2("Coin (12)");
        enemyT = target.GetRootGameObjects().First(g => g.GetComponent<global::Enemy>() != null);
        turretT = R2("Turret");
        heartT = R2("Heart (1)");
        shoesT = R2("shoes");
        movingT = R2("MovingPlat (1)");
        killT = R2("Worl_end");
        waterT = R2("Water");
        fallT = R1("Fallingplat (1)");
        doorT = R1("door");

        // --- A. Khởi động (0–18)
        Ground(-6, -4, 8, 14);                 // tường trái
        Ground(-4, 18, 0);
        Coins(4, 15, 1f, 5);

        // --- B. Hồ nhỏ: lặn lấy xu, một con gián nước (18–34)
        Ground(18, 34, -5f, 3f);
        Water(18, 34, -0.3f, -5f);
        Coins(21, 31, -2.5f, 5);
        Enemy(27, -3.2f);

        // --- C. Bờ có tháp pháo (34–52)
        Ground(34, 52, 0);
        Turret(46, 0);
        Coins(36, 42, 1f, 3);

        // --- D. Hồ dài: bơi dưới nước hoặc nhảy qua các bậc đá (52–84)
        Ground(52, 84, -6f, 3f);
        Water(52, 84, -0.3f, -6f);
        Ground(55, 59, 1.2f, 0.4f, true);
        Ground(63, 67, 1.2f, 0.4f, true);
        Ground(71, 75, 1.2f, 0.4f, true);
        Ground(79, 82, 1.2f, 0.4f, true);
        PlaceOn(Clone(heartT), 65, 1.4f);
        Coins(57, 81, 2.2f, 4);
        Coins(58, 78, -3.5f, 5);
        Enemy(62, -3.5f);
        Enemy(76, -3.5f);

        // --- E. Bờ có gián bay + giày (84–104)
        Ground(84, 104, 0);
        Enemy(96, 2.5f);
        PlaceOn(Clone(shoesT), 100, 0.3f);
        Coins(86, 94, 1f, 4);

        // --- F. Platform di động qua hố gai (104–120)
        GameObject mp = Clone(movingT);
        Bounds mb = mp.GetComponent<Renderer>().bounds;
        mp.transform.position = new Vector3(104.5f + mb.extents.x, 0 - mb.extents.y, 0);
        SpikeRow(104, 120, -3f);
        Ground(104, 120, -3f, 3f);
        Coins(107, 117, 1.5f, 4);

        // --- G. Bờ có tháp pháo thứ hai (120–140)
        Ground(120, 140, 0);
        PlaceOn(Clone(heartT), 124, 0.3f);
        Turret(134, 0);
        Coins(126, 132, 1f, 3);

        // --- H. Hồ sâu cuối cùng (140–156)
        Ground(140, 156, -7f, 3f);
        Water(140, 156, -0.3f, -7f);
        Coins(143, 153, -4.5f, 4);
        Enemy(148, -4f);

        // --- I. Về đích (156–188)
        Ground(156, 170, 0);
        Coins(158, 168, 1f, 4);
        Ground(170, 173, 0.75f);
        Ground(173, 186, 1.5f);
        Ground(186, 188, 10, 14);              // tường phải
        GameObject door = Clone(doorT);
        door.name = "door";
        PlaceOn(door, 182, 1.5f);

        GameObject kill = Clone(killT);
        Bounds kb = kill.GetComponent<Renderer>().bounds;
        Vector3 ks = kill.transform.localScale;
        ks.x *= 210f / Mathf.Max(kb.size.x, 0.01f);
        kill.transform.localScale = ks;
        kill.transform.position = new Vector3(92, -10f, 0);

        foreach (GameObject g in originals) Object.DestroyImmediate(g);
        EditorSceneManager.CloseScene(s1, true);

        PlaceOn(Find("Player"), 1.5f, 0);
        SetBounds(6, -4, 178, 8);
        EditorSceneManager.SaveScene(target);
        Debug.Log("LevelBuilder: Scene 2 rebuilt");
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
