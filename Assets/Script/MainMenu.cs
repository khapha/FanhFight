using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    GameObject modal;
    Button[] menuButtons;
    Button playButton, helpButton, returnFocus;
    TextMeshProUGUI musicText, sfxText;

    void Start()
    {
        // Keep the serialized menu component, but replace the old tiny scene UI.
        Canvas legacy = GetComponentInParent<Canvas>();
        foreach (Graphic graphic in legacy.GetComponentsInChildren<Graphic>(true)) graphic.enabled = false;
        foreach (Button button in legacy.GetComponentsInChildren<Button>(true)) button.interactable = false;

        Canvas canvas = MiniUI.MakeCanvas("CuteMainMenu");
        canvas.sortingOrder = 30;
        Transform safe = SafeArea.Create(canvas.transform);
        Transform card = CuteTheme.Card(safe, Vector2.one * 0.5f, Vector2.zero, new Vector2(480, 610)).transform;
        Label(card, "Fanh Fight", 62, new Vector2(0, 220));
        Label(card, "SMALL FOREST. BIG ADVENTURE.", 16, new Vector2(0, 166));
        Decoration(card, CuteArt.Flower, new Vector2(-190, 228), new Vector2(36, 42));
        Decoration(card, CuteArt.Flower, new Vector2(190, 228), new Vector2(36, 42));

        playButton = ActionButton(card, "Adventure", 85, PlayGame, true);
        Button smash = ActionButton(card, "Slipper Smash", 10, () => SceneManager.LoadScene("MiniSmash"));
        Button garden = ActionButton(card, "Flower Garden", -65, () => SceneManager.LoadScene("MiniSpray"));
        helpButton = ActionButton(card, "How to Play", -140, HowToPlay);
        Button quit = MiniUI.MakeButton(card, "Quit", Vector2.one * 0.5f, new Vector2(0, -225), new Vector2(150, 42), Quit);
        menuButtons = new[] { playButton, smash, garden, helpButton, quit };
        MiniUI.Select(playButton);
    }

    void Update()
    {
        if (modal != null && GameInput.Pause.WasPressedThisFrame()) CloseModal();
    }

    static TextMeshProUGUI Label(Transform parent, string value, int size, Vector2 position)
    {
        TextMeshProUGUI text = MiniUI.MakeText(parent, value, size, Vector2.one * 0.5f, position);
        CuteTheme.Text(text, size);
        text.rectTransform.sizeDelta = new Vector2(540, size * 1.8f);
        return text;
    }

    static void Decoration(Transform parent, Sprite sprite, Vector2 position, Vector2 size)
    {
        GameObject go = new GameObject("FlowerDetail");
        go.transform.SetParent(parent, false);
        Image image = go.AddComponent<Image>();
        image.sprite = sprite;
        image.raycastTarget = false;
        image.rectTransform.anchoredPosition = position;
        image.rectTransform.sizeDelta = size;
    }

    static Button ActionButton(Transform parent, string label, float y, System.Action action, bool primary = false)
    {
        Button b = MiniUI.MakeButton(parent, label, Vector2.one * 0.5f, new Vector2(0, y), new Vector2(340, 62), action);
        CuteTheme.Button(b, primary);
        return b;
    }

    Transform OpenModal(string name, Button origin)
    {
        foreach (Button b in menuButtons) b.interactable = false;
        returnFocus = origin;
        Canvas canvas = MiniUI.MakeCanvas(name);
        canvas.sortingOrder = 80;
        modal = canvas.gameObject;
        MiniUI.Panel(canvas.transform, new Color(0.12f, 0.28f, 0.23f, 0.38f));
        Transform safe = SafeArea.Create(canvas.transform);
        return CuteTheme.Card(safe, Vector2.one * 0.5f, Vector2.zero, new Vector2(600, 610)).transform;
    }

    public void PlayGame()
    {
        if (modal != null) return;
        Transform card = OpenModal("LevelSelect", playButton);
        Label(card, "Choose an adventure", 32, new Vector2(0, 240));
        Label(card, "Complete a level to unlock the next", 18, new Vector2(0, 193));
        Button selected = null;
        for (int i = 1; i <= AdventureProgress.Count; i++)
        {
            int level = i;
            bool unlocked = level <= AdventureProgress.Unlocked;
            string status = !unlocked ? "Locked" : AdventureProgress.IsCompleted(level)
                ? "Completed - Best: " + AdventureProgress.BestScore(level) : "Ready to explore";
            Button b = MiniUI.MakeButton(card, level + ". " + AdventureProgress.Titles[level - 1] + "\n" + status,
                Vector2.one * 0.5f, new Vector2(0, 105 - (i - 1) * 105), new Vector2(500, 86),
                () => AdventureProgress.StartLevel(level));
            b.GetComponentInChildren<TMP_Text>().fontSize = 25;
            b.interactable = unlocked;
            if (unlocked) selected = b;
        }
        ActionButton(card, "Back", -230, CloseModal);
        MiniUI.Select(selected);
    }

    public void HowToPlay()
    {
        if (modal != null) return;
        Transform card = OpenModal("HelpPanel", helpButton);
        Label(card, "Ready for adventure?", 36, new Vector2(0, 240));
        TextMeshProUGUI help = Label(card,
            "Move: A / D or Left / Right arrows\nDouble jump: W / Up (press twice)\n"
            + "Spray: Space / J    |    Enter door: E\nDrop down: S    |    Pause: Esc\n"
            + "Controller: A Jump, X Spray, Y Enter\nStart: Pause",
            20, new Vector2(0, 100));
        help.rectTransform.sizeDelta = new Vector2(540, 190);
        musicText = Label(card, "", 22, new Vector2(0, -65));
        sfxText = Label(card, "", 22, new Vector2(0, -130));
        VolumeButtons(card, -65, delta => { Fx.MusicVolume += delta; RefreshVolume(); });
        VolumeButtons(card, -130, delta => { Fx.SfxVolume += delta; Fx.Coin(); RefreshVolume(); });
        Button close = ActionButton(card, "Back", -230, CloseModal);
        RefreshVolume();
        MiniUI.Select(close);
    }

    static void VolumeButtons(Transform parent, float y, System.Action<float> change)
    {
        MiniUI.MakeButton(parent, "-", Vector2.one * 0.5f, new Vector2(-215, y), new Vector2(50, 44), () => change(-0.1f));
        MiniUI.MakeButton(parent, "+", Vector2.one * 0.5f, new Vector2(215, y), new Vector2(50, 44), () => change(0.1f));
    }

    void RefreshVolume()
    {
        musicText.text = "Music: " + Mathf.RoundToInt(Fx.MusicVolume * 100) + "%";
        sfxText.text = "Sound effects: " + Mathf.RoundToInt(Fx.SfxVolume * 100) + "%";
    }

    void CloseModal()
    {
        Destroy(modal);
        modal = null;
        foreach (Button b in menuButtons) b.interactable = true;
        MiniUI.Select(returnFocus);
    }

    public void Quit() { Application.Quit(); }
}
