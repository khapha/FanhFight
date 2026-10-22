using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

string root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));
int passed = 0;
void Check(string name, Action test)
{
    test();
    passed++;
    Console.WriteLine("PASS " + name);
}
void Assert(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}
void Invoke(Type type, string name, object instance = null)
{
    type.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)!.Invoke(instance, null);
}

Check("all project C# parses with Unity's C# 9 language level", () =>
{
    foreach (string path in Directory.EnumerateFiles(Path.Combine(root, "Assets"), "*.cs", SearchOption.AllDirectories))
    {
        var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(path), new CSharpParseOptions(LanguageVersion.CSharp9), path);
        var errors = tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert(errors.Length == 0, string.Join(Environment.NewLine, errors.Select(d => d.ToString())));
    }
});

Check("adventure scene mapping agrees with enabled build scenes", () =>
{
    string settings = File.ReadAllText(Path.Combine(root, "ProjectSettings/EditorBuildSettings.asset"));
    foreach (Match match in Regex.Matches(settings, @"enabled: 1\s+path: ([^\r\n]+)"))
        SceneUtility.Paths.Add(match.Groups[1].Value.Trim());
    Assert(SceneUtility.Paths.Count == 6, "Expected menu, three adventure levels and two mini-games");
    for (int i = 0; i < AdventureProgress.Count; i++)
    {
        string path = "Assets/Scenes/" + AdventureProgress.Scenes[i] + ".unity";
        Assert(File.Exists(Path.Combine(root, path)), "Missing " + path);
        Assert(SceneUtility.GetBuildIndexByScenePath(path) == i + 1, "Unexpected build order: " + path);
        Assert(AdventureProgress.LevelNumber(AdventureProgress.Scenes[i]) == i + 1, "Incorrect level number");
    }
    Assert(AdventureProgress.LevelNumber("Scene 3") == 0, "Menu must not be an adventure level");
    Assert(AdventureProgress.LevelNumber("MiniSmash") == 0, "Mini-game must not be an adventure level");
});

Check("UI literals use English across menus, levels and mini-games", () =>
{
    var untranslated = new Regex(@"(?i)(phieu luu|khu rung|khu vuon|huong dan|quay lai|choi lai|tiep tuc|het gio|diem:|ky luc:|mang:|nhac nen|bong hoa)");
    foreach (string path in Directory.EnumerateFiles(Path.Combine(root, "Assets/Script"), "*.cs", SearchOption.AllDirectories))
    {
        if (Path.GetFileName(path) == "CutePreview.cs") continue; // Contains the untranslated-label detector itself.
        var syntax = CSharpSyntaxTree.ParseText(File.ReadAllText(path));
        foreach (var token in syntax.GetRoot().DescendantTokens().Where(t => t.IsKind(SyntaxKind.StringLiteralToken)))
            Assert(!untranslated.IsMatch(token.ValueText), "Untranslated UI string in " + path + ": " + token.ValueText);
    }
    Assert(AdventureProgress.Titles.SequenceEqual(new[] { "Woodland", "Lakeside", "Rocky Cave" }), "Level titles must be English");
});

Check("scene loads reset finished and paused state", () =>
{
    Invoke(typeof(GameSession), "Init");
    GameSession.Finish();
    Assert(!GameSession.IsPlaying && Time.timeScale == 0, "Finish must block play");
    GameSession.Pause();
    GameSession.Resume();
    Assert(GameSession.State == GameSession.Phase.Finished, "Pause/resume must not dismiss game over");
    SceneManager.LoadScene(1);
    Assert(GameSession.IsPlaying && Time.timeScale == 1, "Direct editor scene load must reset state");
    GameSession.Pause();
    GameSession.LoadScene(1);
    Assert(GameSession.IsPlaying && Time.timeScale == 1, "Restart while paused must unfreeze");
});

Check("pause during respawn resumes respawn before gameplay", () =>
{
    Time.timeScale = 0.05f;
    GameSession.BeginRespawn();
    Assert(Time.timeScale == 1, "Hit stop must not leave respawn slowed");
    GameSession.Pause();
    GameSession.Pause();
    GameSession.EndRespawn();
    Assert(GameSession.State == GameSession.Phase.Paused, "EndRespawn cannot bypass pause");
    GameSession.Resume();
    Assert(GameSession.State == GameSession.Phase.Respawning && !GameSession.IsPlaying, "Must restore respawn state");
    GameSession.EndRespawn();
    Assert(GameSession.IsPlaying, "Respawn completion should allow gameplay");
});

Check("scene changes invalidate outstanding hit-stop work", () =>
{
    int before = GameSession.SceneVersion;
    GameSession.LoadScene(0);
    Assert(GameSession.SceneVersion != before, "Old scene token must become invalid");
});

Check("locked levels cannot load or erase run points", () =>
{
    PlayerPrefs.ClearTestData();
    PlayerPrefs.SetInt("points", 17);
    SceneManager.LoadScene(0);
    AdventureProgress.StartLevel(2);
    AdventureProgress.StartLevel(0);
    AdventureProgress.StartLevel(99);
    Assert(SceneManager.LoadedIndex == 0, "Locked or invalid level loaded");
    Assert(PlayerPrefs.GetInt("points") == 17, "Rejected selection erased points");
});

Check("completing levels unlocks the next and preserves personal best", () =>
{
    AdventureProgress.Complete(1, 25);
    AdventureProgress.Complete(1, 8);
    Assert(AdventureProgress.Unlocked == 2 && AdventureProgress.IsCompleted(1), "Next level not unlocked");
    Assert(AdventureProgress.BestScore(1) == 25, "Best score regressed");
    AdventureProgress.Complete(2, 30);
    AdventureProgress.Complete(3, 40);
    Assert(AdventureProgress.Unlocked == 3 && AdventureProgress.IsCompleted(3), "Final completion overflowed");
    AdventureProgress.Complete(-1, 100);
    AdventureProgress.Complete(4, 100);
    Assert(AdventureProgress.Unlocked == 3, "Invalid completion changed progress");
});

Check("selecting an unlocked level starts fresh without erasing progress", () =>
{
    AdventureProgress.StartLevel(2);
    Assert(SceneManager.LoadedIndex == 2 && PlayerPrefs.GetInt("points", -1) == -1, "Selection did not reset run points");
    Assert(AdventureProgress.IsCompleted(1) && AdventureProgress.BestScore(1) == 25, "Selection erased progress");
});

Check("flower data round trips independently of render animation and locale", () =>
{
    CultureInfo original = CultureInfo.CurrentCulture;
    try
    {
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("vi-VN");
        var flower = new GardenSave.Flower(new Vector2(1.25f, -2.5f), 3, 1.75f, 42);
        string saved = GardenSave.Write(new[] { flower });
        var loaded = GardenSave.Read(saved, 80, 6);
        Assert(loaded.Count == 1 && loaded[0].Size == 1.75f && loaded[0].Position.y == -2.5f, "Size or position lost");
        Assert(loaded[0].Color == 3 && loaded[0].Rotation == 42, "Appearance lost");
    }
    finally { CultureInfo.CurrentCulture = original; }
});

Check("legacy zero-size flowers recover and corrupt entries are skipped", () =>
{
    var flowers = GardenSave.Read("bad;NaN,1,0,1,2;1,Infinity,0,1,2;1,2,3,0,40;3,4,999,900,20;", 80, 6);
    Assert(flowers.Count == 2, "Malformed entries were not skipped");
    Assert(flowers[0].Size == 1.7f, "Invisible legacy flower did not recover");
    Assert(flowers[1].Size == 3 && flowers[1].Color == 5, "Out-of-range values were not clamped");
    Assert(GardenSave.Read(null, 80, 6).Count == 0, "Empty save failed");
    Assert(GardenSave.Read("1,2,0,1.5,0;3,4,0,1.5,0;", 1, 6).Count == 1, "Flower limit ignored");
});

Check("held touch buttons handle multiple fingers and release on disable", () =>
{
    GameSession.LoadScene(1);
    var button = new TouchHoldButton();
    var events = new List<bool>();
    button.HeldChanged = events.Add;
    var first = new PointerEventData { pointerId = 1 };
    var second = new PointerEventData { pointerId = 2 };
    button.OnPointerDown(first);
    button.OnPointerDown(first);
    button.OnPointerDown(second);
    button.OnPointerUp(first);
    Assert(events.SequenceEqual(new[] { true }), "First finger release canceled second finger");
    Invoke(typeof(TouchHoldButton), "OnDisable", button);
    button.OnPointerUp(second);
    Assert(events.SequenceEqual(new[] { true, false }), "Disable must release once");
    GameSession.Pause();
    button.OnPointerDown(first);
    Assert(events.Count == 2, "Paused button accepted gameplay input");
    GameSession.Resume();
    button.OnPointerDown(first);
    button.OnPointerExit(first);
    Assert(events.SequenceEqual(new[] { true, false, true, false }), "Dragging off button must release");
});

Check("attack colliders cancel on pause, respawn, death and disable", () =>
{
    GameSession.LoadScene(1);
    var attack = new PlayerAttack { trigger = new Collider2D() };
    var player = new Player();
    var animator = new Animator();
    attack.gameObject.Components[typeof(Player)] = player;
    attack.gameObject.Components[typeof(Animator)] = animator;
    Invoke(typeof(PlayerAttack), "Awake", attack);
    Assert(!attack.trigger.enabled, "Attack collider must start disabled");
    GameInput.Attack.Pressed = true;
    Invoke(typeof(PlayerAttack), "Update", attack);
    Assert(attack.attacking && attack.trigger.enabled && animator.Attacking && Fx.Sprays == 1, "Playing attack failed");
    GameSession.Pause();
    attack.battack = true;
    Invoke(typeof(PlayerAttack), "Update", attack);
    Assert(!attack.trigger.enabled && !attack.attacking && !attack.battack && !animator.Attacking, "Paused attack was not canceled");
    GameSession.Resume();
    GameSession.BeginRespawn();
    Invoke(typeof(PlayerAttack), "Update", attack);
    Assert(!attack.trigger.enabled && Fx.Sprays == 1, "Respawn accepted an attack");
    GameSession.EndRespawn();
    player.Alive = false;
    Invoke(typeof(PlayerAttack), "Update", attack);
    Assert(!attack.trigger.enabled && Fx.Sprays == 1, "Dead player attacked");
    player.Alive = true;
    Invoke(typeof(PlayerAttack), "Update", attack);
    Assert(attack.trigger.enabled && Fx.Sprays == 2, "Attack did not recover after respawn");
    Invoke(typeof(PlayerAttack), "OnDisable", attack);
    Assert(!attack.trigger.enabled && !attack.attacking, "Disable left attack collider active");
    GameSession.Finish();
    Invoke(typeof(PlayerAttack), "Update", attack);
    Assert(Fx.Sprays == 2, "Game over accepted attack input");
    GameInput.Attack.Pressed = false;
});

Check("every C# asset has a unique Unity metadata GUID", () =>
{
    var guids = new HashSet<string>();
    foreach (string path in Directory.EnumerateFiles(Path.Combine(root, "Assets"), "*.cs", SearchOption.AllDirectories))
    {
        Assert(File.Exists(path + ".meta"), "Missing metadata: " + path);
        string guid = Regex.Match(File.ReadAllText(path + ".meta"), @"guid: ([a-f0-9]{32})").Groups[1].Value;
        Assert(guid.Length == 32 && guids.Add(guid), "Missing or duplicate GUID: " + path);
    }
});

Console.WriteLine($"{passed} regression checks passed. Unity Editor playtesting is still required.");
