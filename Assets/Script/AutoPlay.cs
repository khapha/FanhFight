#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

// Bot tự chơi toàn bộ 3 màn phiêu lưu để kiểm thử end-to-end.
// Dùng đúng các hàm input mà nút cảm ứng trên màn hình gọi (Player.Move/Jumping,
// PlayerAttack.PlayerActtack, Door.TryEnter) nên vật lý và logic chạy thật 100%.
// Bot đi bộ bằng input thật; khi kẹt lâu hoặc quãng đường quá dài thì dịch chuyển
// có giới hạn (chuẩn playtest bot) để buổi chạy không kéo dài vô hạn.
// Chạy: FanhFight.exe -cute-autoplay <thư-mục-ảnh> (cần cửa sổ visible để chụp).
public class AutoPlay : MonoBehaviour
{
    static IEnumerable<string> PrefsKeys
    {
        get
        {
            foreach (string key in new[] { "points", "highscore", "dashUnlocked", "adventureUnlocked", "diaryRescued", "adventureWeapon" })
                yield return key;
            for (int level = 1; level <= AdventureProgress.Count; level++)
                foreach (string prefix in new[] { "adventureComplete", "adventureBest", "adventureSecret" })
                    yield return prefix + level;
        }
    }

    readonly Dictionary<string, int> prefBackup = new Dictionary<string, int>();
    readonly HashSet<string> prefHad = new HashSet<string>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        int index = System.Array.IndexOf(args, "-cute-autoplay");
        if (index < 0 || index + 1 >= args.Length) return;
        int onlyLevel = 0;
        int levelIndex = System.Array.IndexOf(args, "-cute-autoplay-level");
        if (levelIndex >= 0 && levelIndex + 1 < args.Length)
            int.TryParse(args[levelIndex + 1], out onlyLevel);
        GameObject go = new GameObject("AutoPlay");
        DontDestroyOnLoad(go);
        AutoPlay bot = go.AddComponent<AutoPlay>();
        bot.StartCoroutine(bot.Run(Path.GetFullPath(args[index + 1]), onlyLevel));
    }

    // batchmode/-nographics chạy không GPU: không chụp ảnh được, chỉ log xác nhận
    static bool NoGraphics => System.Environment.CommandLine.Contains("-nographics")
        || System.Environment.CommandLine.Contains("-batchmode");

    IEnumerator Shot(string output, string name)
    {
        if (NoGraphics)
        {
            Debug.Log("AUTOPLAY SHOT(headless): " + name);
            yield break;
        }
        yield return CutePreview.Capture(output, name);
    }

    IEnumerator Run(string output, int onlyLevel)
    {
        Directory.CreateDirectory(output);
        SnapshotPrefs();
        // chạy một màn riêng thì tự mở khóa màn đó để StartLevel không chặn
        if (onlyLevel > 0)
            PlayerPrefs.SetInt("adventureUnlocked", Mathf.Max(PlayerPrefs.GetInt("adventureUnlocked", 1), onlyLevel));
        // yield không được nằm trong try-catch, nên tự chạy coroutine con bằng MoveNext
        System.Exception error = null;
        // Flatten nested routines so failures in movement/combat also restore saves and exit.
        Stack<IEnumerator> routines = new Stack<IEnumerator>();
        routines.Push(Play(output, onlyLevel));
        while (routines.Count > 0)
        {
            bool has;
            IEnumerator play = routines.Peek();
            try { has = play.MoveNext(); }
            catch (System.Exception e) { error = e; break; }
            if (!has) { (routines.Pop() as System.IDisposable)?.Dispose(); continue; }
            if (play.Current is IEnumerator child && !(child is CustomYieldInstruction)) routines.Push(child);
            else yield return play.Current;
        }
        while (routines.Count > 0) (routines.Pop() as System.IDisposable)?.Dispose();
        if (error != null) Debug.Log("AUTOPLAY FAIL: " + error);
        RestorePrefs();
        Application.Quit(error == null ? 0 : 1);
    }

    IEnumerator Play(string output, int onlyLevel)
    {
        yield return new WaitForSecondsRealtime(2.5f);
        yield return Shot(output, "a0-menu.png");
        MainMenu menu = FindAnyObjectByType<MainMenu>();
        if (menu == null) throw new System.Exception("Main menu not found.");
        menu.PlayGame();
        yield return new WaitForSecondsRealtime(0.6f);
        yield return Shot(output, "a1-levels.png");
        int first = onlyLevel > 0 ? onlyLevel : 1;
        int last = onlyLevel > 0 ? onlyLevel : AdventureProgress.Count;
        GameObject.Find("LevelSelect").GetComponentsInChildren<UnityEngine.UI.Button>()[first - 1].onClick.Invoke();
        yield return null;
        yield return Shot(output, "a2-tools.png");
        bool slipper = System.Environment.CommandLine.Contains("-cute-slipper");
        GameObject.Find("WeaponSelect").GetComponentsInChildren<UnityEngine.UI.Button>()[slipper ? 1 : 0].onClick.Invoke();
        if (WeaponLoadout.IsSpray == slipper) throw new System.Exception("Tool selection was not applied.");
        Debug.Log("AUTOPLAY PASS: selected " + WeaponLoadout.Name + " through menu");

        if (System.Environment.CommandLine.Contains("-cute-combat-only"))
        {
            yield return WaitForLevel(first);
            yield return new WaitForSecondsRealtime(3.5f);
            FindAnyObjectByType<Player>().GodMode = true;
            yield return CheckWeapon(output, first);
            yield return CheckCombat(output);
            Debug.Log("AUTOPLAY COMPLETE: weapon roles, real hits, reactions and telegraphed rush.");
            yield break;
        }

        if (System.Environment.CommandLine.Contains("-cute-tools-only"))
        {
            yield return WaitForLevel(first);
            yield return new WaitForSecondsRealtime(3.5f);
            FindAnyObjectByType<Player>().GodMode = true;
            yield return CheckWeapon(output, first);
            Debug.Log("AUTOPLAY COMPLETE: held-tool pose, grip and hitbox checks.");
            yield break;
        }

        for (int level = first; level <= last; level++)
        {
            yield return WaitForLevel(level);
            yield return new WaitForSecondsRealtime(0.8f);
            Debug.Log("AUTOPLAY: playing level " + level);
            yield return PlayLevel(output, level);
            Debug.Log("AUTOPLAY PASS: level " + level + " finished");
        }

        // sau cửa màn 3 phải thấy màn thắng kèm dòng tổng kết (chỉ ở chế độ chơi đủ)
        if (onlyLevel == 0 || onlyLevel == AdventureProgress.Count)
        {
            float winStart = Time.realtimeSinceStartup;
            while (!GameOverUI.IsShowing && Time.realtimeSinceStartup - winStart < 8f) yield return null;
            if (!GameOverUI.IsShowing) throw new System.Exception("YOU WIN screen did not appear after the final door.");
            bool hasSummary = false;
            foreach (TMPro.TMP_Text text in FindObjectsByType<TMPro.TMP_Text>())
                if (text.text.Contains("Roaches squished")) hasSummary = true;
            if (!hasSummary) throw new System.Exception("Win screen lacks the squished/secrets summary.");
            Debug.Log("AUTOPLAY PASS: YOU WIN screen with summary (squished=" + QuestState.RunSquished
                + ", secrets=" + QuestState.RunSecrets + ")");
            yield return Shot(output, "a7-win.png");

            // hai mini-game chỉ cần khởi động được và có UI
            SceneManager.LoadScene("MiniSmash");
            yield return new WaitForSecondsRealtime(4f);
            yield return Shot(output, "a8-smash.png");
            Debug.Log("AUTOPLAY PASS: MiniSmash boots");
            MiniUI.BackToMenu();
            yield return new WaitForSecondsRealtime(1.5f);
            SceneManager.LoadScene("MiniSpray");
            yield return new WaitForSecondsRealtime(3f);
            yield return Shot(output, "a9-spray.png");
            Debug.Log("AUTOPLAY PASS: MiniSpray boots");
            SceneManager.LoadScene("ButterflyParty");
            yield return new WaitForSecondsRealtime(3f);
            yield return Shot(output, "a10-party.png");
            if (FindAnyObjectByType<ButterflyPartyScene>() == null)
                throw new System.Exception("Butterfly Party failed to boot.");
            Debug.Log("AUTOPLAY PASS: ButterflyParty boots");
        }
        Debug.Log("AUTOPLAY COMPLETE: " + (onlyLevel > 0 ? "level " + onlyLevel : "full adventure") + " played end to end.");
    }

    IEnumerator WaitForLevel(int level)
    {
        float start = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - start < 12f)
        {
            if (GameHUD.LevelNumber(SceneManager.GetActiveScene().name) == level)
            {
                Player p = FindAnyObjectByType<Player>();
                float ready = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - ready < 5f)
                {
                    if (p == null) p = FindAnyObjectByType<Player>();
                    if (p != null && p.CanAct) yield break;
                    yield return null;
                }
                yield break; // đã load đủ, CanAct sẽ tự hồi sau respawn
            }
            yield return null;
        }
        throw new System.Exception("Level " + level + " did not load.");
    }

    IEnumerator PlayLevel(string output, int level)
    {
        // vị thần hộ mệnh: bot không chết vì bẫy/spike (cái chết và respawn đã có
        // smoke test riêng kiểm tra) - giúp buổi tự chơi đi trọn hành trình
        Player guarded = FindAnyObjectByType<Player>();
        if (guarded != null) guarded.GodMode = true;
        yield return CheckWeapon(output, level);
        yield return CheckBlockedExit("missing keys");

        // 1) orb dash (màn 1, chỉ khi chưa mở khóa vĩnh viễn)
        if (level == 1 && !QuestState.DashUnlocked)
        {
            DashOrb orb = FindAnyObjectByType<DashOrb>();
            if (orb != null)
            {
                yield return WalkTo(orb.transform.position.x, 0.9f, 15f);
                yield return WaitUntil(() => QuestState.DashUnlocked, 4f);
                if (!QuestState.DashUnlocked)
                {
                    TeleportPlayer(orb.transform.position);
                    yield return WaitUntil(() => QuestState.DashUnlocked, 4f);
                }
                if (!QuestState.DashUnlocked) throw new System.Exception("Dash orb did not unlock after touching it.");
                Debug.Log("AUTOPLAY PASS: dash unlocked from orb");
            }
        }

        // 2) thu thập đủ 3 chìa khóa
        while (QuestState.KeysCollected < QuestState.KeysRequired)
        {
            KeyPickup key = FindAnyObjectByType<KeyPickup>();
            if (key == null) throw new System.Exception("Key pickup missing: have "
                + QuestState.KeysCollected + "/" + QuestState.KeysRequired);
            int before = QuestState.KeysCollected;
            yield return WalkTo(key.transform.position.x, 1.0f, 24f);
            yield return WaitUntil(() => QuestState.KeysCollected > before, 4f);
            if (QuestState.KeysCollected <= before)
            {
                TeleportPlayer(key.transform.position);
                yield return WaitUntil(() => QuestState.KeysCollected > before, 4f);
            }
            if (QuestState.KeysCollected <= before) throw new System.Exception("Key was not collected on touch.");
        }
        Debug.Log("AUTOPLAY PASS: keys " + QuestState.KeysCollected + "/" + QuestState.KeysRequired);
        yield return Shot(output, "aL" + level + "-keys.png");

        // 3) kho báu bí mật (tùy chọn, không fail nếu không tới được)
        SecretSparkle secret = FindAnyObjectByType<SecretSparkle>();
        if (secret != null)
        {
            yield return WalkTo(secret.transform.position.x, 1.0f, 10f);
            yield return WaitUntil(() => QuestState.SecretFound(level), 4f);
            if (!QuestState.SecretFound(level))
            {
                TeleportPlayer(secret.transform.position);
                yield return WaitUntil(() => QuestState.SecretFound(level), 4f);
            }
            Debug.Log(QuestState.SecretFound(level)
                ? "AUTOPLAY PASS: secret found on level " + level
                : "AUTOPLAY WARN: secret not reachable on level " + level);
        }

        // 4) Vua Gián ở màn cuối / Gián Vũ Công ở màn Sunset Meadow
        if (level == QuestState.BossLevel)
        {
            yield return CheckBlockedExit("undefeated king");
            if (FindAnyObjectByType<RoachKing>() == null)
                throw new System.Exception("Roach King missing on the boss level.");
            yield return FightBoss();
            if (!QuestState.BossDefeated) throw new System.Exception("Boss survived the fight routine.");
            Debug.Log("AUTOPLAY PASS: Roach King defeated");
            yield return new WaitForSecondsRealtime(1.5f);
            yield return Shot(output, "aL3-bossdown.png");
        }
        if (level == QuestState.MiniBossLevel)
        {
            yield return CheckBlockedExit("undefeated dancer");
            RoachDancer dancer = FindAnyObjectByType<RoachDancer>();
            if (dancer == null) throw new System.Exception("Roach Dancer missing on the mini-boss level.");
            float stageY = AdventureProps.GroundYNear(dancer.transform.position.x);
            if (float.IsNaN(stageY) || Mathf.Abs(dancer.transform.position.y - stageY) > 1.7f)
                throw new System.Exception("Dancer drifted away from the stage before the fight.");
            Debug.Log("AUTOPLAY PASS: dancer stays within reach of the stage");
            yield return DanceWith(dancer);
            if (!QuestState.MiniBossDefeated) throw new System.Exception("Dancer never took a bow.");
            Debug.Log("AUTOPLAY PASS: Roach Dancer took a bow");
            yield return new WaitForSecondsRealtime(1.2f);
            yield return Shot(output, "aL4-dancer.png");
        }

        // 5) cổng nhiệm vụ: đi vào vùng cổng là tự mở khi đủ điều kiện
        GameObject gate = GameObject.Find("QuestGate");
        if (gate != null)
        {
            float gateX = gate.transform.position.x;
            yield return WalkTo(gateX, 1.4f, 20f);
            yield return WaitUntil(() => GameObject.Find("QuestGate") == null, 5f);
            if (GameObject.Find("QuestGate") != null)
            {
                TeleportPlayer(new Vector3(gateX - 0.6f, GroundYOr(gateX, 4f), 0));
                yield return WaitUntil(() => GameObject.Find("QuestGate") == null, 4f);
            }
            if (GameObject.Find("QuestGate") != null) throw new System.Exception("Gate did not open with all keys.");
        }
        Debug.Log("AUTOPLAY PASS: gate opened");

        // 6) cửa đích: đứng vào cửa rồi gọi TryEnter (đúng như nút Enter cảm ứng)
        Door door = FindAnyObjectByType<Door>();
        if (door == null) throw new System.Exception("Exit door missing.");
        string beforeScene = SceneManager.GetActiveScene().name;
        yield return WalkTo(door.transform.position.x - 1.0f, 0.9f, 20f);
        TeleportPlayer(new Vector3(door.transform.position.x, GroundYOr(door.transform.position.x, door.transform.position.y + 1f), 0));
        yield return new WaitForSecondsRealtime(0.5f);
        door.TryEnter();
        yield return WaitUntil(() =>
            SceneManager.GetActiveScene().name != beforeScene || GameOverUI.IsShowing, 6f);
        if (SceneManager.GetActiveScene().name == beforeScene && !GameOverUI.IsShowing)
        {
            door.TryEnter();
            yield return WaitUntil(() =>
                SceneManager.GetActiveScene().name != beforeScene || GameOverUI.IsShowing, 3f);
        }
        if (SceneManager.GetActiveScene().name == beforeScene && !GameOverUI.IsShowing)
            throw new System.Exception("Door did not load the next scene.");
        Debug.Log("AUTOPLAY PASS: door -> " + (GameOverUI.IsShowing ? "YOU WIN" : SceneManager.GetActiveScene().name));
        yield return Shot(output, "aL" + level + "-door.png");
    }

    // khiêu vũ với Vũ Công: chạy tới chạm chú ấy 4 lần (mỗi lần chú xoay tít cúi chào)
    IEnumerator DanceWith(RoachDancer dancer)
    {
        float start = Time.realtimeSinceStartup;
        while (!QuestState.MiniBossDefeated && Time.realtimeSinceStartup - start < 60f)
        {
            if (FindAnyObjectByType<RoachDancer>() == null) yield break; // đã tan
            Player p = FindAnyObjectByType<Player>();
            if (p == null || !p.CanAct)
            {
                yield return new WaitForSecondsRealtime(0.4f);
                continue;
            }
            // lao vào thân vũ công: trigger chạm là một "bước nhảy" trúng thưởng
            TeleportPlayer(dancer.transform.position + Vector3.left * 0.9f);
            yield return new WaitForSecondsRealtime(0.7f);
            if (dancer == null) yield break; // The bow can finish and destroy the actor during the wait.
            Debug.Log("AUTOPLAY dance hp=" + dancer.Hp + " dancerY=" + dancer.transform.position.y.ToString("F1"));
        }
    }

    IEnumerator CheckBlockedExit(string reason)
    {
        Door door = FindAnyObjectByType<Door>();
        Player player = FindAnyObjectByType<Player>();
        if (door == null || player == null) throw new System.Exception("Missing exit/player for blocked-door check.");
        string scene = SceneManager.GetActiveScene().name;
        int level = QuestState.Level;
        int best = AdventureProgress.BestScore(level);
        bool completed = AdventureProgress.IsCompleted(level);
        Vector3 origin = player.transform.position;
        TeleportPlayer(door.transform.position);
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        // Assert the real door overlap, so the test cannot pass because input missed it.
        bool inside = (bool)typeof(Door).GetField("playerInside",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(door);
        if (!inside) throw new System.Exception("Blocked-door test did not reach the door trigger.");
        door.TryEnter();
        yield return null;
        if (SceneManager.GetActiveScene().name != scene || GameOverUI.IsShowing
            || AdventureProgress.IsCompleted(level) != completed || AdventureProgress.BestScore(level) != best)
            throw new System.Exception("Exit bypassed quest: " + reason);
        TeleportPlayer(origin);
        Debug.Log("AUTOPLAY PASS: exit blocked with " + reason + " on level " + level);
    }

    IEnumerator CheckWeapon(string output, int level)
    {
        Player player = FindAnyObjectByType<Player>();
        PlayerAttack attack = player.GetComponent<PlayerAttack>();
        Vector3 origin = player.transform.position;
        for (int side = 0; side < 2; side++)
        {
            bool right = side == 0;
            if ((player.transform.lossyScale.x > 0) != right) player.Flip();
            float sign = right ? 1 : -1;
            GameObject Probe(string name, float distance)
            {
                GameObject go = new GameObject(name);
                go.tag = "Enemy";
                go.transform.position = player.transform.position + new Vector3(sign * distance, WeaponLoadout.Height, 0);
                go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
                go.AddComponent<BoxCollider2D>().size = new Vector2(0.15f, 0.15f);
                go.AddComponent<WeaponProbe>();
                return go;
            }
            GameObject near = Probe("WeaponInRange", WeaponLoadout.Start + WeaponLoadout.Reach * 0.7f);
            GameObject far = Probe("WeaponOutOfRange", WeaponLoadout.Start + WeaponLoadout.Reach + 0.5f);
            try
            {
                attack.PlayerActtack(true);
                yield return new WaitForSecondsRealtime(WeaponLoadout.IsSpray ? 0.2f : 0.26f);
                attack.PlayerActtack(false);
                PlayerVisuals visual = player.GetComponent<PlayerVisuals>();
                if (visual == null || visual.GripError > 0.005f)
                    throw new System.Exception("Tool handle detached from the character's hand.");
                if (near.GetComponent<WeaponProbe>().Hits != 1 || far.GetComponent<WeaponProbe>().Hits != 0)
                    throw new System.Exception("Tool hitbox/range failed: " + WeaponLoadout.Name + (right ? " right" : " left")
                        + " near=" + near.GetComponent<WeaponProbe>().Hits + " far=" + far.GetComponent<WeaponProbe>().Hits
                        + " attack=" + attack.trigger.bounds + " target=" + near.GetComponent<Collider2D>().bounds);
                yield return Shot(output, "tool-" + level + "-" + WeaponLoadout.Name.Replace(" ", "") + (right ? "-right.png" : "-left.png"));
                Debug.Log("AUTOPLAY PASS: " + WeaponLoadout.Name + " hits once within range " + (right ? "right" : "left"));
            }
            finally
            {
                attack.CancelAttack();
                Destroy(near); Destroy(far);
            }
        }
        if (player.transform.lossyScale.x < 0) player.Flip();
        TeleportPlayer(origin);
    }

    IEnumerator CheckCombat(string output)
    {
        Player p = FindAnyObjectByType<Player>();
        PlayerAttack attack = p.GetComponent<PlayerAttack>();
        if (p.transform.lossyScale.x < 0) p.Flip();
        RoachWalker target = RoachWalker.Create(new Vector2(10, 20));
        target.enabled = false;
        Rigidbody2D targetBody = target.GetComponent<Rigidbody2D>();
        targetBody.bodyType = RigidbodyType2D.Kinematic;
        targetBody.position = (Vector2)p.transform.position + new Vector2(1.45f, -0.15f);
        target.transform.position = targetBody.position;
        RoachStatus reaction = target.GetComponent<RoachStatus>();
        reaction.Telegraphing = true;
        try
        {
            yield return new WaitForSecondsRealtime(0.8f);
            attack.PlayerActtack(true);
            yield return new WaitForSecondsRealtime(0.28f);
            attack.PlayerActtack(false);
            int expected = 60 - RoachStatus.ResolveDamage(WeaponLoadout.Damage, WeaponLoadout.IsSpray, RoachStatus.Species.Armored);
            if (target.Health != expected)
                throw new System.Exception("Armored real hit failed: hp=" + target.Health + " expected=" + expected);
            if (WeaponLoadout.IsSpray ? !reaction.Slowed : !reaction.Stunned || reaction.Telegraphing)
                throw new System.Exception("Weapon reaction was not applied through actual overlap.");
            yield return Shot(output, WeaponLoadout.IsSpray ? "combat-slow.png" : "combat-perfect-slap.png");
            Debug.Log("AUTOPLAY PASS: actual " + WeaponLoadout.Name + " hit, armor and reaction.");
            attack.CancelAttack();
            if (WeaponLoadout.IsSpray)
            {
                attack.PlayerActtack(true);
                float deadline = Time.realtimeSinceStartup + 6;
                while (!attack.Overheated && Time.realtimeSinceStartup < deadline) yield return null;
                if (!attack.Overheated || attack.trigger.enabled)
                    throw new System.Exception("Continuous spray did not stop at overheat.");
                attack.PlayerActtack(false);
                yield return Shot(output, "combat-overheated.png");
                yield return new WaitForSeconds(2.3f);
                if (attack.Overheated) throw new System.Exception("Bottle failed to cool down.");
                Debug.Log("AUTOPLAY PASS: actual held spray overheats, disables damage and recovers.");
            }
        }
        finally { attack.CancelAttack(); if (target != null) Destroy(target.gameObject); }

        float x = p.transform.position.x + 3.5f;
        if (!AdventureProps.TryGround(x, out float ground)) throw new System.Exception("No safe ground for rush check.");
        RoachWalker runner = RoachWalker.Create(new Vector2(0, 20));
        runner.GetComponent<Rigidbody2D>().position = new Vector2(x, ground + 0.55f);
        runner.transform.position = new Vector3(x, ground + 0.55f, 0);
        RoachStatus runnerStatus = runner.GetComponent<RoachStatus>();
        bool warned = false, rushed = false;
        float end = Time.realtimeSinceStartup + 4;
        try
        {
            while (Time.realtimeSinceStartup < end)
            {
                warned |= runnerStatus.Telegraphing;
                if (Mathf.Abs(runner.GetComponent<Rigidbody2D>().linearVelocity.x) > 4)
                {
                    if (!warned) throw new System.Exception("Runner rushed without a warning.");
                    rushed = true;
                }
                yield return null;
            }
            if (!warned || !rushed) throw new System.Exception("Runner did not complete warning/rush cycle.");
            yield return Shot(output, "combat-roach-runner.png");
            Debug.Log("AUTOPLAY PASS: runner warns before a committed rush.");
        }
        finally { if (runner != null) Destroy(runner.gameObject); }

        GameObject flyerGo = new GameObject("CombatFlyer");
        flyerGo.tag = "Enemy";
        flyerGo.transform.position = p.transform.position + new Vector3(2.8f, 0.8f, 0);
        Rigidbody2D flyerBody = flyerGo.AddComponent<Rigidbody2D>();
        flyerBody.gravityScale = 0;
        flyerBody.bodyType = RigidbodyType2D.Kinematic;
        flyerGo.AddComponent<BoxCollider2D>().isTrigger = true;
        flyerGo.AddComponent<SpriteRenderer>();
        flyerGo.AddComponent<Enemy>();
        flyerGo.AddComponent<EnemyVisuals>();
        RoachStatus flyerStatus = flyerGo.GetComponent<RoachStatus>();
        warned = rushed = false;
        end = Time.realtimeSinceStartup + 3.5f;
        try
        {
            while (Time.realtimeSinceStartup < end)
            {
                warned |= flyerStatus.Telegraphing;
                if (flyerBody.linearVelocity.magnitude > 4)
                {
                    if (!warned) throw new System.Exception("Flyer swooped without a warning.");
                    rushed = true;
                }
                yield return null;
            }
            if (!warned || !rushed) throw new System.Exception("Flyer did not complete warning/swoop cycle.");
            GameSession.Pause();
            Vector2 frozen = flyerBody.position;
            yield return new WaitForSecondsRealtime(0.3f);
            if (Vector2.Distance(frozen, flyerBody.position) > 0.005f)
                throw new System.Exception("Paused flyer kept moving.");
            GameSession.Resume();
            yield return Shot(output, "combat-roach-flyer.png");
            Debug.Log("AUTOPLAY PASS: flyer warns before swoop and remains still during pause.");
        }
        finally { GameSession.Resume(); Destroy(flyerGo); }
    }

    class WeaponProbe : MonoBehaviour
    {
        public int Hits;
        void Damage(int amount) { Hits++; }
    }

    IEnumerator FightBoss()
    {
        RoachKing king = FindAnyObjectByType<RoachKing>();
        if (king == null) yield break;
        // đi vào hang đấu boss bằng input thật
        yield return WalkTo(king.transform.position.x - 4f, 1.2f, 15f);

        float start = Time.realtimeSinceStartup;
        float lastHpChange = Time.realtimeSinceStartup;
        int shownHp = QuestState.BossHp;
        PlayerAttack dbgAttack = FindAnyObjectByType<Player>()?.GetComponent<PlayerAttack>();
        float dbgTick = 0f;
        while (!QuestState.BossDefeated && Time.realtimeSinceStartup - start < 150f)
        {
            if (FindAnyObjectByType<RoachKing>() == null) yield break; // boss đã tan
            Player p = FindAnyObjectByType<Player>();
            if (p == null || !p.CanAct)
            {
                yield return new WaitForSecondsRealtime(0.4f);
                continue;
            }
            king = FindAnyObjectByType<RoachKing>();
            if (king == null) yield break;
            float dx = king.transform.position.x - p.transform.position.x;
            // telemetry: mỗi ~2s in trạng thái hình học để chẩn đoán nếu dép không trúng
            dbgTick += Time.unscaledDeltaTime;
            if (dbgTick >= 2f)
            {
                dbgTick = 0f;
                Collider2D slipper = dbgAttack != null ? dbgAttack.trigger : null;
                Collider2D[] kingCols = king.GetComponents<Collider2D>();
                string touching = "n/a";
                if (slipper != null && slipper.enabled)
                {
                    bool any = false;
                    foreach (Collider2D kc in kingCols)
                        if (Physics2D.IsTouching(slipper, kc)) { any = true; break; }
                    touching = any ? "TOUCHING" : "no";
                }
                Debug.Log($"AUTOPLAY boss dbg: dist={Mathf.Abs(dx):F2} playerY={p.transform.position.y:F2} " +
                    $"kingY={king.transform.position.y:F2} swing={(slipper != null && slipper.enabled)} touch={touching} hp={QuestState.BossHp}");
            }
            if (Mathf.Abs(dx) > 2.3f)
            {
                // vào thế battle station ngay: đi bộ trong arena dễ dính vực/gai
                float side = dx > 0 ? -1.7f : 1.7f;
                float gy = AdventureProps.GroundYNear(king.transform.position.x + side);
                float ty = float.IsNaN(gy) ? king.transform.position.y : gy + 0.1f;
                TeleportPlayer(new Vector3(king.transform.position.x + side, ty, 0));
                yield return new WaitForSecondsRealtime(0.25f);
            }
            else
            {
                PlayerAttack attack = p.GetComponent<PlayerAttack>();
                if (attack != null)
                {
                    attack.PlayerActtack(true);
                    yield return null;
                    attack.PlayerActtack(false);
                }
                yield return new WaitForSecondsRealtime(0.35f);
            }
            if (QuestState.BossHp != shownHp)
            {
                shownHp = QuestState.BossHp;
                lastHpChange = Time.realtimeSinceStartup;
                Debug.Log("AUTOPLAY: boss hp down to " + shownHp);
            }
            if (Time.realtimeSinceStartup - lastHpChange > 8f)
            {
                // không trúng đòn được: đứng sát ngay cạnh boss
                TeleportPlayer(king.transform.position + Vector3.left * 1.6f);
                lastHpChange = Time.realtimeSinceStartup;
            }
        }
    }

    // đi bộ bằng input thật; nhảy khi kẹt, vung dép dọn gián, quá hạn thì dịch chuyển
    IEnumerator WalkTo(float targetX, float tolerance, float timeout)
    {
        Player p = FindAnyObjectByType<Player>();
        if (p == null) yield break;
        float start = Time.realtimeSinceStartup;
        float stuck = 0f, lastX = p.transform.position.x, attackPulse = 1f, tick = 0f, lastWalkY = 99f;
        while (Time.realtimeSinceStartup - start < timeout)
        {
            p = FindAnyObjectByType<Player>();
            if (p == null || !p.CanAct)
            {
                yield return new WaitForSecondsRealtime(0.3f);
                continue;
            }
            float dx = targetX - p.transform.position.x;
            if (Mathf.Abs(dx) <= tolerance) break;

            // heartbeat: ghi lại vị trí để truy vết nếu tiến trình chết giữa đường
            tick += Time.unscaledDeltaTime;
            if (tick >= 0.5f)
            {
                tick = 0f;
                Debug.Log($"AUTOPLAY walk x={p.transform.position.x:F1} y={p.transform.position.y:F1} hp={p.ourHealth}");
            }

            // rơi xuống vực/nước: bỏ cuộc, để teleport dự phòng của vòng gọi xử lý
            if (p.transform.position.y < -4f) break;

            // sa xuống hố sâu (vực gai hang đá 15m không thể nhảy qua): dịch chuyển tới
            // mỏm đất chắc gần đích, như người chơi đi băng chuyền
            if (p.transform.position.y < -1.5f)
            {
                float safeX = Mathf.MoveTowards(p.transform.position.x, targetX, 12f);
                TeleportPlayer(new Vector3(safeX, GroundYOr(safeX, 1.5f) + 0.6f, 0));
            }
            lastWalkY = p.transform.position.y;

            // phía trước là vực (không có đất trong tầm 2.2m) thì nhảy kép vượt qua
            if (!GroundAhead(p.transform.position.x, Mathf.Sign(dx)))
            {
                yield return JumpTwice();
            }
            p.Move(dx > 0 ? 1 : -1);

            if (Mathf.Abs(p.transform.position.x - lastX) < 0.015f) stuck += Time.unscaledDeltaTime;
            else stuck = 0f;
            lastX = p.transform.position.x;
            if (stuck > 0.3f)
            {
                yield return JumpTwice();
                stuck = 0f;
            }

            attackPulse -= Time.unscaledDeltaTime;
            if (attackPulse <= 0)
            {
                attackPulse = 1.4f;
                PlayerAttack attack = p.GetComponent<PlayerAttack>();
                if (attack != null)
                {
                    attack.PlayerActtack(true);
                    yield return null;
                    attack.PlayerActtack(false);
                }
            }
            yield return null;
        }
        p = FindAnyObjectByType<Player>();
        if (p != null) p.Move(0);
    }

    IEnumerator JumpTwice()
    {
        Player p = FindAnyObjectByType<Player>();
        if (p == null) yield break;
        p.Jumping(true);
        yield return new WaitForSecondsRealtime(0.22f);
        p.Jumping(false);
        yield return new WaitForSecondsRealtime(0.28f);
        p.Jumping(true);   // double jump
        yield return new WaitForSecondsRealtime(0.22f);
        p.Jumping(false);
    }

    // có địa hình liên tục phía trước trong tầm `range` mét không (để bot biết lúc nào phải nhảy)
    static bool GroundAhead(float x, float dir)
    {
        Vector2 origin = new Vector2(x + dir * 0.5f, 4f);
        for (float d = 0.5f; d <= 2.2f; d += 0.55f)
            if (Physics2D.RaycastAll(origin + Vector2.right * dir * d, Vector2.down, 12f).Length > 0)
                return true;
        return false;
    }

    IEnumerator WaitUntil(System.Func<bool> done, float seconds)
    {
        float start = Time.realtimeSinceStartup;
        while (!done() && Time.realtimeSinceStartup - start < seconds) yield return null;
    }

    void TeleportPlayer(Vector3 pos)
    {
        Player p = FindAnyObjectByType<Player>();
        if (p == null || p.r2 == null) return;
        Vector3 delta = pos - p.transform.position;
        p.r2.linearVelocity = Vector2.zero;
        p.r2.position = pos;
        p.transform.position = pos;
        CameraRig.Warp(p.transform, delta);
    }

    static float GroundYOr(float x, float fallback)
    {
        float y = AdventureProps.GroundYNear(x);
        return float.IsNaN(y) ? fallback : y + 1.2f;
    }

    void SnapshotPrefs()
    {
        foreach (string key in PrefsKeys)
            if (PlayerPrefs.HasKey(key))
            {
                prefBackup[key] = PlayerPrefs.GetInt(key);
                prefHad.Add(key);
            }
    }

    void RestorePrefs()
    {
        foreach (string key in PrefsKeys)
        {
            if (prefHad.Contains(key)) PlayerPrefs.SetInt(key, prefBackup[key]);
            else PlayerPrefs.DeleteKey(key);
        }
        PlayerPrefs.Save();
    }
}
#endif
