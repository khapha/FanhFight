using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Replaces the scene's old four-button layout with complete mobile controls.
public class TouchButtons : MonoBehaviour
{
    public static bool DownHeld { get; private set; }
    Player player;
    GameObject controls;
    Button pauseButton;
    TMPro.TMP_Text pauseLabel;
    GameSession.Phase lastPhase = GameSession.Phase.Finished;
    bool left, right;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded -= Setup;
        SceneManager.sceneLoaded += Setup;
    }

    static void Setup(Scene scene, LoadSceneMode mode)
    {
        DownHeld = false;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.GetComponent<Canvas>() == null) continue;
            Transform buttons = root.transform.Find("Button");
            if (buttons != null && buttons.Find("Left") != null) buttons.gameObject.SetActive(false);
        }
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (!Application.isMobilePlatform || p == null) return;
        Canvas canvas = MiniUI.MakeCanvas("TouchControls");
        canvas.sortingOrder = 20;
        TouchButtons touch = canvas.gameObject.AddComponent<TouchButtons>();
        touch.Build(p.GetComponent<Player>());
    }

    void Build(Player target)
    {
        player = target;
        Transform safe = SafeArea.Create(transform);
        controls = new GameObject("Gameplay", typeof(RectTransform));
        RectTransform rt = (RectTransform)controls.transform;
        rt.SetParent(safe, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        Hold("<", new Vector2(0, 0), new Vector2(65, 72), held => { left = held; Move(); });
        Hold(">", new Vector2(0, 0), new Vector2(175, 72), held => { right = held; Move(); });
        Hold("Jump", new Vector2(1, 0), new Vector2(-65, 72), player.Jumping);
        PlayerAttack attack = player.GetComponent<PlayerAttack>();
        if (attack != null) Hold("Spray", new Vector2(1, 0), new Vector2(-175, 72), attack.PlayerActtack);
        Hold("Down", new Vector2(0, 0), new Vector2(65, 180), held => DownHeld = held);
        MiniUI.MakeButton(rt, "Enter", new Vector2(1, 0), new Vector2(-175, 180), new Vector2(140, 76), () =>
        {
            foreach (Door door in FindObjectsByType<Door>(FindObjectsSortMode.None)) door.TryEnter();
        });
        pauseButton = MiniUI.MakeButton(safe, "Pause", new Vector2(1, 1), new Vector2(-80, -90), new Vector2(120, 60), () =>
        {
            PausedMenu menu = FindAnyObjectByType<PausedMenu>();
            if (menu != null) menu.TogglePause();
        });
        pauseLabel = pauseButton.GetComponentInChildren<TMPro.TMP_Text>();
    }

    void Hold(string label, Vector2 anchor, Vector2 position, System.Action<bool> changed)
    {
        Button button = MiniUI.MakeButton(controls.transform, label, anchor, position, new Vector2(96, 88), () => { });
        Navigation nav = button.navigation;
        nav.mode = Navigation.Mode.None;
        button.navigation = nav;
        button.gameObject.AddComponent<TouchHoldButton>().HeldChanged = changed;
    }

    void Move() { if (player != null) player.Move((right ? 1 : 0) - (left ? 1 : 0)); }

    void Update()
    {
        if (controls == null) return;
        controls.SetActive(player != null && player.CanAct);
        pauseButton.interactable = !GameOverUI.IsShowing;
        if (lastPhase != GameSession.State)
        {
            lastPhase = GameSession.State;
            pauseLabel.text = lastPhase == GameSession.Phase.Paused ? "Resume" : "Pause";
        }
    }

    void OnDestroy() { DownHeld = false; }
}
